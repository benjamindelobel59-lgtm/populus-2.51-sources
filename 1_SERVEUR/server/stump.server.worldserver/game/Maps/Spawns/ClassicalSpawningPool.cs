using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;

namespace Stump.Server.WorldServer.Game.Maps.Spawns
{
    public class ClassicalSpawningPool : SpawningPoolBase
    {
        public const int NumberOfGroupSizes = 3;

        /// <summary>
        /// PADGREF 23/10 : un groupe qui part en combat est remplacé au bout de ce délai (secondes) au lieu d'attendre le minuteur
        /// normal de la zone (60 à 220 s). 0 = comportement d'origine.
        /// </summary>
        [Variable(true)]
        public static int DelaiRespawnApresCombat = 15;

        protected Dictionary<GroupSize, Tuple<int, int>> GroupSizes = new Dictionary<GroupSize, Tuple<int, int>>()
        {
            {GroupSize.Small, Tuple.Create(1, 2)},
            {GroupSize.Medium, Tuple.Create(3, 5)},
            {GroupSize.Big, Tuple.Create(6, 8)},
        };

        readonly object m_locker = new object();
        readonly MonsterGroup[] m_groupsBySize = new MonsterGroup[NumberOfGroupSizes];
        readonly Queue<GroupSize> m_groupsToSpawn = new Queue<GroupSize>();

        public readonly Queue<MonsterGroup>[] m_groupsToRespawn = {
            new Queue<MonsterGroup>(),
            new Queue<MonsterGroup>(),
            new Queue<MonsterGroup>()
        };

        public ClassicalSpawningPool(Map map)
            : base(map)
        {
            RandomQueue();
        }

        public ClassicalSpawningPool(Map map, int interval)
            : base(map, interval)
        {
            RandomQueue();
        }

        void RandomQueue()
        {
            var array = Enum.GetValues(typeof (GroupSize));

            foreach (var size in array.Cast<GroupSize>().Shuffle())
            {
                if (size != GroupSize.None)
                    m_groupsToSpawn.Enqueue(size);
            }
        }

        /// <summary>
        /// 1-2 Group
        /// </summary>
        public MonsterGroup SmallGroup => m_groupsBySize[(int)GroupSize.Small];

        /// <summary>
        /// 3 - 5 group
        /// </summary>
        public MonsterGroup MediumGroup => m_groupsBySize[(int)GroupSize.Medium];

        /// <summary>
        /// 6 - 8 group
        /// </summary>
        public MonsterGroup BigGroup => m_groupsBySize[(int)GroupSize.Big];

        protected override bool IsLimitReached() => m_groupsToSpawn.Count == 4;

        // NEKFEU : groupes conserves d'un redemarrage a l'autre (MonsterGroupPersistence)
        public override string GetPersistenceKey(MonsterGroup group) =>
            group.GroupSize != GroupSize.None ? "C" + (int)group.GroupSize : null;

        protected override void OnAutoSpawnEnabled()
        {
            try
            {
                foreach (var entry in MonsterGroupPersistence.Instance.TakeClassical(Map.Id))
                {
                    if (!int.TryParse(entry.Key.Substring(1), out var sizeValue) || !Enum.IsDefined(typeof(GroupSize), sizeValue))
                        continue;

                    var size = (GroupSize)sizeValue;
                    MonsterGroup group;

                    lock (m_locker)
                    {
                        if (size == GroupSize.None || m_groupsBySize[(int)size] != null || !m_groupsToSpawn.Contains(size))
                            continue;

                        group = MonsterGroupPersistence.Instance.BuildGroup(Map, entry);
                        if (group == null)
                            continue;

                        // cette taille n'est plus a tirer : on la retire de la file
                        var remaining = m_groupsToSpawn.Where(x => x != size).ToList();
                        var removed = m_groupsToSpawn.Count - remaining.Count;
                        m_groupsToSpawn.Clear();
                        // si la taille apparaissait plusieurs fois, on n'en retire qu'une
                        for (var i = 1; i < removed; i++)
                            remaining.Add(size);
                        foreach (var s in remaining)
                            m_groupsToSpawn.Enqueue(s);

                        group.SpawningPool = this;
                        group.GroupSize = size;
                        m_groupsBySize[(int)size] = group;
                    }

                    SpawnGroup(group);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error("[Groupes] restauration impossible sur la map {0} : {1}", Map.Id, ex);
            }

            base.OnAutoSpawnEnabled();
        }

        protected override MonsterGroup DequeueNextGroupToSpawn()
        {
            lock (m_locker)
            {
                if (m_groupsToSpawn.Count == 0)
                {
                    return null;
                }

                var size = m_groupsToSpawn.Dequeue();

                MonsterGroup group;
                if (m_groupsToRespawn[(int) size].Count > 0)
                {
                    group = m_groupsToRespawn[(int)size].Dequeue();
                    group = Map.GenerateRandomMonsterGroup(group);
                }
                else
                {
                    group = Map.GenerateRandomMonsterGroup(GroupSizes[size].Item1, GroupSizes[size].Item2);
                }

                if (group == null)
                    return null;

                group.SpawningPool = this;
                group.GroupSize = size;

                return m_groupsBySize[(int) size] = group;
            }
        }

        protected override int GetNextSpawnInterval()
        {
            var rand = new AsyncRandom();
            if (rand.Next(0, 1) == 0)
            {
                return (int) ((Interval - (rand.NextDouble()*Interval/4))*1000);
            }

            return (int) ((Interval + (rand.NextDouble()*Interval/4))*1000);
        }

        protected override void OnGroupSpawned(MonsterGroup group)
        {
            group.EnterFight -= OnEnterFightRespawn;
            group.EnterFight += OnEnterFightRespawn;

            base.OnGroupSpawned(group);
        }

        // PADGREF 23/10 : combat lancé sur un groupe -> un nouveau groupe apparaît vite (DelaiRespawnApresCombat)
        void OnEnterFightRespawn(MonsterGroup group, Actors.RolePlay.Characters.Character character)
        {
            group.EnterFight -= OnEnterFightRespawn;

            if (DelaiRespawnApresCombat <= 0)
                return;

            Map.Area.CallDelayed(DelaiRespawnApresCombat * 1000, () =>
            {
                if (State != SpawningPoolState.Running || !Map.CanSpawnMonsters())
                    return;

                lock (this)
                {
                    SpawnNextGroup();
                }
            });
        }

        protected override void OnGroupUnSpawned(MonsterGroup monster)
        {
            monster.ExitFight += OnExitFight;

            lock (m_locker)
            {
                if (monster.GroupSize != GroupSize.None)
                {
                    m_groupsBySize[(int) monster.GroupSize] = null;
                    m_groupsToSpawn.Enqueue(monster.GroupSize);
                }
            }

            base.OnGroupUnSpawned(monster);
        }

        void RespawnGroup(MonsterGroup group)
        {
            m_groupsToRespawn[(int) group.GroupSize].Enqueue(group);
        }

        void OnExitFight(MonsterGroup monster, IFight fight)
        {
            FightMonsterTeam team;
            if (fight.DefendersTeam is FightMonsterTeam)
                team = (FightMonsterTeam) fight.DefendersTeam;
            else if (fight.ChallengersTeam is FightMonsterTeam)
                team = (FightMonsterTeam) fight.ChallengersTeam;
            else
                return;

            if (fight.Winners == team) // respawn the group
            {
                RespawnGroup(monster);
            }
        }
    }
}