using System;
using System.Collections.Generic;
using System.Linq;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Maps.Pathfinding;
using Stump.Server.WorldServer.Game.Maps.Spawns;
using Stump.Server.WorldServer.Handlers.Context;
using Stump.Core.Extensions;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters
{
    public class MonsterGroup : RolePlayActor, IContextDependant, IAutoMovedEntity
    {
        [Variable(true)]
        public static int StarsBonusRate = 500;

        [Variable(true)]
        public static short StarsBonusLimit = 300;

        public const short ClientStarsBonusLimit = 300;

        public event Action<MonsterGroup, Character> EnterFight;
        public event Action<MonsterGroup, IFight> ExitFight;

        readonly List<Monster> m_monsters = new List<Monster>();

        public MonsterGroup(int id, ObjectPosition position, SpawningPoolBase spawningPool = null)
        {
            ContextualId = id;
            Position = position;
            CreationDate = DateTime.Now;
            SpawningPool = spawningPool;
        }

        public sealed override ObjectPosition Position
        {
            get { return base.Position; }
            protected set { base.Position = value; }
        }

        public IFight Fight
        {
            get;
            private set;
        }

        public override int Id
        {
            get { return ContextualId; }
            protected set { ContextualId = value; }
        }

        public int ContextualId
        {
            get;
            private set;
        }

        public SpawningPoolBase SpawningPool
        {
            get;
            set;
        }

        public GroupSize GroupSize
        {
            get;
            set;
        }

        public Monster Leader
        {
            get;
            private set;
        }

        public short AgeBonus
        {
            get
            {
                //Note: To find rate choose how many bonus per hour and then divise 60(1 hour) by bonus and multiply result by 60(seconds).
                //Note: Exemple for 20 bonus per hour. 60/20 = 3 * 60 = 180.
                var bonus = ( DateTime.Now - CreationDate ).TotalSeconds / (StarsBonusRate);

                if (bonus > StarsBonusLimit)
                    bonus = StarsBonusLimit;

                return (short) bonus;
            }
            set { CreationDate = DateTime.Now - TimeSpan.FromSeconds(value*StarsBonusRate); }
        }

        #region NEKFEU : etoiles masquees pendant les 30 premieres minutes du groupe

        // Le bonus (XP / drop) compte des l'apparition du groupe comme avant ; seul l'AFFICHAGE des etoiles (meme vides)
        // est masque avant ce delai : un groupe sans etoiles = combattu (recree) il y a moins de 30 min.
        [Variable(true)]
        public static int StarsDisplayDelay = 1800; // secondes

        private Stump.Core.Timers.TimedTimerEntry m_starsDisplayTimer;

        public bool StarsVisible => (DateTime.Now - CreationDate).TotalSeconds >= StarsDisplayDelay;

        /// <summary>Bonus envoye au client : -1 (le client masque tout l'affichage des etoiles) avant le delai.</summary>
        public short DisplayedAgeBonus => StarsVisible ? AgeBonus : (short)-1;

        public override void OnEnterMap(Stump.Server.WorldServer.Game.Maps.Map map)
        {
            base.OnEnterMap(map);
            ScheduleStarsDisplay(map);
        }

        public override void OnLeaveMap(Stump.Server.WorldServer.Game.Maps.Map map)
        {
            StopStarsDisplayTimer();
            base.OnLeaveMap(map);
        }

        /// <summary>A la fin du delai, renvoie le groupe aux joueurs de la map pour faire apparaitre les etoiles.</summary>
        void ScheduleStarsDisplay(Stump.Server.WorldServer.Game.Maps.Map map)
        {
            StopStarsDisplayTimer();
            if (StarsVisible || map?.Area == null)
                return;

            var remaining = (int)Math.Max(1000, (StarsDisplayDelay - (DateTime.Now - CreationDate).TotalSeconds) * 1000 + 500);
            m_starsDisplayTimer = map.Area.CallDelayed(remaining, () =>
            {
                m_starsDisplayTimer = null;
                if (Map == map && map.IsActor(this))
                    map.Refresh(this);
            });
        }

        void StopStarsDisplayTimer()
        {
            if (m_starsDisplayTimer != null)
            {
                m_starsDisplayTimer.Stop();
                m_starsDisplayTimer = null;
            }
        }

        #endregion
        
        public DateTime NextMoveDate
        {
            get;
            set;
        }

        public DateTime LastMoveDate
        {
            get;
            private set;
        }

        public DateTime CreationDate
        {
            get;
            set;
        }

        public Character AuthorizedAgressor
        {
            get;
            set;
        }

        public override bool CanMove()
        {
            return true;
        }

        public override bool IsMoving()
        {
            return false;
        }

        public override bool StartMove(Path movementPath)
        {
            if (!CanMove() || movementPath.IsEmpty())
                return false;

            Position = movementPath.EndPathPosition;
            var keys = movementPath.GetServerPathKeys();

            Map.ForEach(entry => ContextHandler.SendGameMapMovementMessage(entry.Client, keys, this));

            // monsters movements are instants
            StopMove();
            LastMoveDate = DateTime.Now;

            return true;
        }

        public override bool StopMove() => false;

        public override bool StopMove(ObjectPosition currentObjectPosition) => false;

        public override bool MoveInstant(ObjectPosition destination) => false;

        public override bool Teleport(ObjectPosition destination, bool performCheck = true) => false;

        public void CheckAgression(Character character)
        {
            
            
                bool isPossible = false;
            

            foreach (var test in Position.Map.SubArea.GetMonstersAgression())
            {
                foreach (var tes in GetMonsters())
                {
                  
                    if (test.MonsterId == tes.Template.Id && character.IsOppositeAlignement(test.AlignmentId))
                        isPossible = true;
                }
            }
            if (Fight != null || !Position.Map.SubArea.IsAgressibleMonsters || !isPossible)
                return;

            var allCharacterMap = Map.GetAllCharacters().ToList();
            for (int i = 0; i < allCharacterMap.Count(); i++)
            {
                var distance = Position.Point.DistanceTo(allCharacterMap[i].Position.Point);
                if (distance <= 6)
                {
                    allCharacterMap[i].StopMove();
                    FightWith(allCharacterMap[i]);
                    return;
                }
            }
        }


        public void FightWith(Character character)
        {
            if (character.Map != Map)
                return;

            // only this character and his group can join the fight
            if (AuthorizedAgressor != null && AuthorizedAgressor != character && AuthorizedAgressor.Client.Connected)
            {
                ContextHandler.SendChallengeFightJoinRefusedMessage(character.Client, character, FighterRefusedReasonEnum.TEAM_LIMITED_BY_MAINCHARACTER);
                return;
            }

            var reason = character.CanAttack(this);

            if (reason != FighterRefusedReasonEnum.FIGHTER_ACCEPTED)
            {
                ContextHandler.SendChallengeFightJoinRefusedMessage(character.Client, character, reason);
                return;
            }

            Map.Leave(this);
            
            if (Map.GetBlueFightPlacement().Length < CountInitialFighters())
            {
                //character.SendServerMessage("Cannot start fight : Not enough fight placements");
                return;
            }

            // Nekfeu : chronométrage du lancement de combat (diagnostic latence)
            var chrono = System.Diagnostics.Stopwatch.StartNew();

            var fight = FightManager.Instance.CreatePvMFight(Map);

            var monsterFighters = CreateFighters(fight.DefendersTeam).ToArray();
            var tMonstres = chrono.ElapsedMilliseconds;

            // Correctif Nekfeu : les monstres sont ajoutés AVANT le personnage. Avec le join auto (.ow / master),
            // les suiveurs rejoignent pendant l'ajout du personnage ; les groupes de donjon à alternatives
            // (MonsterGroupWithAlternatives) remplacent alors les monstres selon le nombre de joueurs.
            // Avant, ce remplacement avait lieu avant l'ajout des monstres d'origine → monstres en double (jusqu'à 12).
            foreach (var monster in monsterFighters)
                fight.DefendersTeam.AddFighter(monster);
            fight.ChallengersTeam.AddFighter(character.CreateFighter(fight.ChallengersTeam));
            var tAjout = chrono.ElapsedMilliseconds;

            Fight = fight;

            fight.StartPlacement();
            var tPlacement = chrono.ElapsedMilliseconds;

            OnEnterFight(character);
            Fight.FightEnded += OnFightEnded;

            NLog.LogManager.GetCurrentClassLogger().Info("[Lancement combat] {0} ({7} persos) vs {1} monstres : total {2} ms (creation+monstres {3} ms, ajout combattants {4} ms, placement {5} ms, suiveurs {6} ms)",
                character.Name, fight.DefendersTeam.Fighters.Count, chrono.ElapsedMilliseconds, tMonstres, tAjout - tMonstres, tPlacement - tAjout, chrono.ElapsedMilliseconds - tPlacement, fight.ChallengersTeam.Fighters.Count);
        }

        void OnFightEnded(IFight fight)
        {
            OnExitFight(fight);
        }

        protected virtual void OnEnterFight(Character character)
        {
            var handler = EnterFight;
            if (handler != null)
                handler(this, character);
        }

        protected virtual void OnExitFight(IFight fight)
        {
            Fight = null;

            var handler = ExitFight;
            if (handler != null) handler(this, fight);
        }

        public virtual IEnumerable<MonsterFighter> CreateFighters(FightMonsterTeam team) => m_monsters.Select(monster => monster.CreateFighter(team));

        public virtual void AddMonster(Monster monster)
        {
            monster.SetMonsterGroup(this);
            m_monsters.Add(monster);

            if (m_monsters.Count == 1)
                Leader = monster;

            Map.Refresh(this);
        }

        public virtual void RemoveMonster(Monster monster)
        {
            m_monsters.Remove(monster);

            if (m_monsters.Count == 0)
                Leader = null;

            Map.Refresh(this);
        }

        public virtual IEnumerable<Monster> GetMonsters()
        {
            return m_monsters;
        }

        public IEnumerable<Monster> GetMonstersWithoutLeader()
        {
            return GetMonsters().Where(entry => entry != Leader);
        }
       
        public virtual int Count() => m_monsters.Count;

        protected virtual int CountInitialFighters() => Count();

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
                                                        => new GameRolePlayGroupMonsterInformations(Id,
                                                            Leader.Look.GetEntityLook(),
                                                            GetEntityDispositionInformations(),
                                                            false,
                                                            false,
                                                            false,
                                                            GetGroupMonsterStaticInformations(character),
                                                            0,
                                                            0,
                                                            DisplayedAgeBonus); // étoiles affichées par le client modifié (-1 = affichage masqué)

        public virtual GroupMonsterStaticInformations GetGroupMonsterStaticInformations(Character character)
            => new GroupMonsterStaticInformations(Leader.GetMonsterInGroupLightInformations(),
                GetMonstersWithoutLeader().Select(entry => entry.GetMonsterInGroupInformations()).ToArray());

        public override string ToString() => string.Format("{0} monsters ({1})", m_monsters.Count, Id);
    }
}