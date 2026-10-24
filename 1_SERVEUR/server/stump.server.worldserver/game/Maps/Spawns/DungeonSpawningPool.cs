using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Monster = Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters.Monster;

namespace Stump.Server.WorldServer.Game.Maps.Spawns
{
    public class DungeonSpawningPool : SpawningPoolBase
    {
        [Variable(true)]
        public static int DungeonSpawnsInterval = 30;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly object m_locker = new object();
        private readonly List<MonsterDungeonSpawn> m_spawns = new List<MonsterDungeonSpawn>();
        private Queue<MonsterDungeonSpawn> m_spawnsQueue = new Queue<MonsterDungeonSpawn>();
        private readonly Dictionary<MonsterGroup, MonsterDungeonSpawn> m_groupsSpawn = new Dictionary<MonsterGroup, MonsterDungeonSpawn>();

        public DungeonSpawningPool(Map map)
            : this(map, DungeonSpawnsInterval)
        {
        }

        public DungeonSpawningPool(Map map, int interval)
            : base(map, interval)
        {
        }

        public ReadOnlyCollection<MonsterDungeonSpawn> Spawns
        {
            get { return m_spawns.AsReadOnly(); }
        }

        public void AddSpawn(MonsterDungeonSpawn spawn)
        {
            lock (m_locker)
            {
                m_spawns.Add(spawn);
                m_spawnsQueue.Enqueue(spawn);
            }
        }

        public void RemoveSpawn(MonsterDungeonSpawn spawn)
        {
            lock (m_locker)
            {
                m_spawns.Remove(spawn);

                var asList = m_spawnsQueue.ToList();
                if (asList.Remove(spawn))
                    m_spawnsQueue = new Queue<MonsterDungeonSpawn>(asList);
            }
        }

        protected override bool IsLimitReached()
        {
            return m_spawnsQueue.Count == 0;
        }

        // NEKFEU : groupes conserves d'un redemarrage a l'autre (MonsterGroupPersistence)
        public override string GetPersistenceKey(MonsterGroup group)
        {
            lock (m_locker)
                return m_groupsSpawn.TryGetValue(group, out var spawn) ? "D" + spawn.Id : null;
        }

        // PADGREF 24/10 : numéro du spawn d'un groupe posé par cette salle (placements fixes)
        public int? SpawnIdDe(MonsterGroup group)
        {
            lock (m_locker)
                return group != null && m_groupsSpawn.TryGetValue(group, out var spawn) ? spawn.Id : (int?)null;
        }

        protected override MonsterGroup DequeueNextGroupToSpawn()
        {
            if (!Map.CanSpawnMonsters())
            {
                StopAutoSpawn();
                return null;
            }

            lock (m_locker)
            {
                if (m_spawnsQueue.Count == 0)
                {
                    logger.Error("SpawningPool Map = {0} try to spawn a monser but m_groupsToSpawn is empty", Map.Id);
                    return null;
                }

                // NEKFEU : salle du boss qui sert aussi de salle de sortie (pas de teleportation apres le combat) :
                // le boss ne reapparait pas tant qu'un joueur est sur la map (sinon on le relancerait a l'infini sans cle) ;
                // le minuteur reessaie au delai suivant
                if (!m_spawnsQueue.Peek().TeleportEvent && Map.GetAllCharacters().Any())
                    return null;

                var spawn = m_spawnsQueue.Dequeue();

                // NEKFEU : un spawn sans monstre dans monsters_spawns_dungeons_groups (133 salles de la base d'origine) donnait,
                // via le LEFT JOIN du chargement, un groupe avec un monstre vide (Template null) -> deconnexion a l'entree de la map
                var monsters = spawn.GroupMonsters.Where(x => x.MonsterGrade != null && x.MonsterGrade.Template != null).ToList();
                if (monsters.Count == 0)
                {
                    logger.Warn("[Donjon] spawn {0} (map {1}) sans monstre : aucun groupe pose", spawn.Id, Map.Id);
                    return null;
                }

                var cell = spawn.CellId == null ? Map.GetRandomFreeCell() : Map.Cells[(int)spawn.CellId];

                var group = new MonsterGroupWithAlternatives(Map.GetNextContextualId(), new ObjectPosition(Map, cell, Map.GetRandomDirection()), this);
                foreach (var entity in monsters)
                {
                    if (entity.MinPartyMembers != null)
                        group.AddMonster(new Monster(entity.MonsterGrade, group), entity.MinPartyMembers.Value);
                    else
                        group.AddMonster(new Monster(entity.MonsterGrade, group));
                }

                m_groupsSpawn.Add(group, spawn);

                // NEKFEU : reprend la date d'apparition enregistree avant le redemarrage (bonus d'etoiles)
                MonsterGroupPersistence.Instance.ApplyFixed(group, Map, "D" + spawn.Id);

                return group;
            }
        }

        protected override int GetNextSpawnInterval()
        {
            return Interval * 1000;
        }

        protected override void OnGroupSpawned(MonsterGroup group)
        {
            group.EnterFight += OnGroupEnterFight;

            base.OnGroupSpawned(group);
        }

        private void OnGroupEnterFight(MonsterGroup group, Character character)
        {
            group.EnterFight -= OnGroupEnterFight;
            group.Fight.WinnersDetermined += OnWinnersDetermined;
        }

        private void OnWinnersDetermined(IFight fight, FightTeam winners, FightTeam losers, bool draw)
        {
            fight.WinnersDetermined -= OnWinnersDetermined;

            if (draw)
                return;

            // if players didn't win they don't get teleported
            if (!(winners is FightPlayerTeam) || !(losers is FightMonsterTeam))
                return;

            var group = ((MonsterFighter)losers.Leader).Monster.Group;

            if (!m_groupsSpawn.ContainsKey(@group))
            {
                logger.Error("Group {0} (Map {1}) has ended his fight but is not register in the pool", @group.Id, Map.Id);
                return;
            }

            var spawn = m_groupsSpawn[@group];

            GiveRoomReward(spawn, winners);

            if (!spawn.TeleportEvent)
                return;

            var nextMap = spawn.TeleportMap;
            if (nextMap == null)
            {
                logger.Error("[Donjon] spawn {0} (map {1}) : map de teleportation {2} introuvable", spawn.Id, Map.Id, spawn.TeleportMapId);
                return;
            }

            // NEKFEU : toujours arriver sur une case utilisable de la salle suivante (case de la base praticable et reliee au reste de la map)
            var cell = nextMap.GetUsableCell(spawn.TeleportCell);

            // MYSTIC 17/10 : la salle suivante n'est plus dans le donjon (boss battu) -> plus de reprise pour les gagnants
            var donjon = Donjons.RepriseDonjons.Instance.DonjonDe(Map.Id);
            if (donjon != null && Donjons.RepriseDonjons.Instance.DonjonDe(nextMap.Id) != donjon)
                foreach (var gagnant in winners.GetAllFighters<CharacterFighter>())
                    Donjons.RepriseDonjons.Instance.Effacer(gagnant.Character, donjon.Value);

            foreach (var fighter in winners.GetAllFighters<CharacterFighter>())
            {
                fighter.Character.NextMap = nextMap;
                fighter.Character.Cell = cell;
                fighter.Character.Direction = spawn.TeleportDirection;


                m_groupsSpawn.Remove(group);
            }
        }

        // NEKFEU : relique de salle (ex. Kolosso) donnee a 100 % a chaque personnage gagnant, liee a la salle et pas aux monstres
        private void GiveRoomReward(MonsterDungeonSpawn spawn, FightTeam winners)
        {
            if (spawn.RewardItemId == null || spawn.RewardItemId <= 0)
                return;

            var template = ItemManager.Instance.TryGetTemplate(spawn.RewardItemId.Value);
            if (template == null)
            {
                logger.Error("[Donjon] spawn {0} (map {1}) : objet de recompense {2} introuvable", spawn.Id, Map.Id, spawn.RewardItemId);
                return;
            }

            var max = spawn.RewardMax ?? 0;
            foreach (var fighter in winners.GetAllFighters<CharacterFighter>())
            {
                var character = fighter.Character;
                var owned = character.Inventory.Where(x => x.Template.Id == template.Id).Sum(x => x.Stack);
                if (max > 0 && owned >= max)
                    continue;

                character.Inventory.AddItem(template);
                character.SendServerMessage(string.Format("Vous obtenez : {0} ({1}{2}).", template.Name, owned + 1, max > 0 ? "/" + max : ""));
            }
        }

        protected override void OnGroupUnSpawned(MonsterGroup monster)
        {
            lock (m_locker)
            {
                if (!m_groupsSpawn.ContainsKey(monster))
                {
                    logger.Error("Group {0} (Map {1}) was not bind to a dungeon spawn", monster.Id, Map.Id);
                }
                else
                {
                    var spawn = m_groupsSpawn[monster];

                    if (m_spawns.Contains(spawn))
                        m_spawnsQueue.Enqueue(spawn);
                }
            }

            base.OnGroupUnSpawned(monster);
        }
    }
}