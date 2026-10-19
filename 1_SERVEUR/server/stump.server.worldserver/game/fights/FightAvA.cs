using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Alliances;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Fights
{
    // MYSTIC 17/10 : combats AvA (alliances phase 3, _OUTILS/serveur/alliances/PLAN_ALLIANCES.md).

    /// <summary>Défenseurs d'un prisme : le prisme (monstre officiel 3451) + les membres de l'alliance inscrits.</summary>
    public class FightPrismDefendersTeam : FightTeam
    {
        public FightPrismDefendersTeam(TeamEnum id, Cell[] placementCells) : base(id, placementCells)
        {
        }

        public override TeamTypeEnum TeamType => TeamTypeEnum.TEAM_TYPE_PRISM;

        // on rejoint la défense depuis l'interface d'alliance (PrismFightJoinLeaveRequest), pas par les épées
        public override FighterRefusedReasonEnum CanJoin(Character character) => FighterRefusedReasonEnum.TEAM_LIMITED_BY_MAINCHARACTER;

        public override FightOutcomeEnum GetOutcome() =>
            (Fight as FightPvPr)?.Prism?.IsDead() == true ? FightOutcomeEnum.RESULT_LOST : FightOutcomeEnum.RESULT_VICTORY;
    }

    /// <summary>Attaquants d'un prisme : 5 au plus, membres d'une autre alliance.</summary>
    public class FightPrismAttackersTeam : FightTeam
    {
        public FightPrismAttackersTeam(TeamEnum id, Cell[] placementCells) : base(id, placementCells)
        {
        }

        public override TeamTypeEnum TeamType => TeamTypeEnum.TEAM_TYPE_PLAYER;

        public override FighterRefusedReasonEnum CanJoin(Character character)
        {
            var fight = Fight as FightPvPr;
            if (fight == null)
                return FighterRefusedReasonEnum.TOO_LATE;

            if (Fighters.Count >= FightPvPr.MaxFighters)
                return FighterRefusedReasonEnum.TEAM_FULL;

            var alliance = character.Guild?.Alliance;
            if (alliance == null || alliance.Id == fight.Prisme.AllianceId)
                return FighterRefusedReasonEnum.WRONG_GUILD;

            if (Fighters.OfType<CharacterFighter>().Any(x => x.Character.Client.IP == character.Client.IP))
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            return base.CanJoin(character);
        }
    }

    /// <summary>
    /// Attaque d'un prisme (type officiel PvPr) : 5 attaquants contre le prisme + 5 défenseurs (le prisme ne prend pas de place).
    /// 30 s de placement pour les attaquants, puis les défenseurs inscrits sont téléportés et ont 10 s. Prisme inébranlable, lourd et
    /// invulnérable 2 tours. Prisme mort = prisme affaibli (vulnérable au prochain créneau, AvAManager).
    /// </summary>
    public class FightPvPr : Fight<FightPrismDefendersTeam, FightPrismAttackersTeam>
    {
        public const int MaxFighters = 5;
        // MYSTIC 17/10 (demande utilisateur) : +25 s pour les attaquants et +25 s pour les défenseurs
        public const int AttackersPlacementTime = 55000;
        public const int DefendersPlacementTime = 35000;

        readonly List<Character> m_defendersQueue = new List<Character>();
        readonly Dictionary<FightActor, Map> m_defendersMaps = new Dictionary<FightActor, Map>();
        bool m_attackersPhase;

        public FightPvPr(int id, Map fightMap, PrismeAlliance prisme, FightPrismDefendersTeam defendersTeam, FightPrismAttackersTeam challengersTeam)
            : base(id, fightMap, defendersTeam, challengersTeam)
        {
            Prisme = prisme;
        }

        public PrismeAlliance Prisme { get; }

        public MonsterFighter Prism { get; private set; }

        public IReadOnlyList<Character> DefendersQueue => m_defendersQueue.AsReadOnly();

        public IEnumerable<Character> Attackers => ChallengersTeam.Fighters.OfType<CharacterFighter>().Select(x => x.Character);

        public override FightTypeEnum FightType => FightTypeEnum.FIGHT_TYPE_PvPr;

        public override bool IsPvP => true;

        public override bool IsMultiAccountRestricted => true;

        public bool IsAttackersPlacementPhase => m_attackersPhase && (State == FightState.Placement || State == FightState.NotStarted);

        public override void StartPlacement()
        {
            base.StartPlacement();
            m_attackersPhase = true;
            m_placementTimer = Map.Area.CallDelayed(AttackersPlacementTime, StartDefendersPlacement);
            AvAManager.Instance.OnPrismAttacked(this);
        }

        void StartDefendersPlacement()
        {
            if (State != FightState.Placement)
                return;

            m_placementTimer?.Dispose();
            m_placementTimer = null;
            m_attackersPhase = false;

            // temps de défense toujours attendu en entier, même sans défenseur
            if (m_defendersQueue.Count == 0)
            {
                m_placementTimer = Map.Area.CallDelayed(DefendersPlacementTime, StartFighting);
                return;
            }

            foreach (var defender in m_defendersQueue.ToArray())
            {
                var d = defender;
                d.Area.ExecuteInContext(() =>
                {
                    var lastMap = d.Map;
                    d.Teleport(Map, Map.Cells[d.Cell.Id]);
                    Map.Area.ExecuteInContext(() =>
                    {
                        var fighter = d.CreateFighter(DefendersTeam);
                        m_defendersMaps[fighter] = lastMap;
                        DefendersTeam.AddFighter(fighter);

                        if (m_placementTimer == null &&
                            m_defendersQueue.All(x => DefendersTeam.Fighters.OfType<CharacterFighter>().Any(y => y.Character == x)))
                            m_placementTimer = Map.Area.CallDelayed(DefendersPlacementTime, StartFighting);
                    });
                });
            }
        }

        public override void StartFighting()
        {
            m_placementTimer?.Dispose();
            m_placementTimer = null;
            base.StartFighting();

            // MYSTIC 17/10 (demande utilisateur) : le prisme est inébranlable (157), lourd (63, ni porté ni échangé) et invulnérable
            // (56) pendant ses 2 premiers tours
            if (Prism != null)
            {
                using (StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                {
                    AI.Fights.Brain.Custom.Boss.EtatsBoss.Ajouter(Prism, Prism, (int)SpellStatesEnum.INEBRANLABLE_157, 3738);
                    AI.Fights.Brain.Custom.Boss.EtatsBoss.Ajouter(Prism, Prism, (int)SpellStatesEnum.LOURD_63, 3738);
                    AI.Fights.Brain.Custom.Boss.EtatsBoss.AjouterPourTours(Prism, Prism, AI.Fights.Brain.Custom.Boss.EtatsBoss.Invulnerable, 3738, 2);
                }
            }
        }

        public FighterRefusedReasonEnum AddDefender(Character character)
        {
            if (character.IsBusy() || character.IsInFight() || character.IsInJail())
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (!IsAttackersPlacementPhase)
                return FighterRefusedReasonEnum.TOO_LATE;

            if (character.Guild?.Alliance == null || character.Guild.Alliance.Id != Prisme.AllianceId)
                return FighterRefusedReasonEnum.WRONG_GUILD;

            if (m_defendersQueue.Count >= MaxFighters)
                return FighterRefusedReasonEnum.TEAM_FULL;

            if (m_defendersQueue.Contains(character) || m_defendersQueue.Any(x => x.Client.IP == character.Client.IP))
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            m_defendersQueue.Add(character);
            AvAManager.Instance.OnDefendersChanged(this, character, true);
            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public bool RemoveDefender(Character character)
        {
            if (!m_defendersQueue.Remove(character))
                return false;

            AvAManager.Instance.OnDefendersChanged(this, character, false);
            return true;
        }

        public int TimeLeftBeforeFight =>
            IsAttackersPlacementPhase && m_placementTimer != null ? (int)(m_placementTimer.NextTick - System.DateTime.Now).TotalMilliseconds / 100 : 0;

        protected override bool CanKickFighter(FightActor kicker, FightActor kicked) => false;

        /// <summary>« Prêt » ne lance pas le combat : on attend la fin complète du temps d'attaque puis de défense.</summary>
        protected override void OnSetReady(FightActor fighter, bool isReady)
        {
            if (State != FightState.Placement)
                return;

            ContextHandler.SendGameFightHumanReadyStateMessage(Clients, fighter);
        }

        public override bool CanChangePosition(FightActor fighter, Cell cell) => base.CanChangePosition(fighter, cell) &&
            ((IsAttackersPlacementPhase && fighter.Team == ChallengersTeam) || (!IsAttackersPlacementPhase && fighter.Team == DefendersTeam));

        protected override void OnFighterAdded(FightTeam team, FightActor actor)
        {
            if (actor is MonsterFighter monster && team == DefendersTeam && Prism == null)
            {
                Prism = monster;
                Prism.Dead += (f, killer) => EndFight();
            }

            base.OnFighterAdded(team, actor);
        }

        protected override void DeterminsWinners()
        {
            var prismMort = Prism == null || Prism.IsDead();
            Winners = prismMort ? (FightTeam)ChallengersTeam : DefendersTeam;
            Losers = prismMort ? (FightTeam)DefendersTeam : ChallengersTeam;
            Draw = false;
            OnWinnersDetermined(Winners, Losers, Draw);
        }

        protected override void OnWinnersDetermined(FightTeam winners, FightTeam losers, bool draw)
        {
            foreach (var defender in DefendersTeam.Fighters.OfType<CharacterFighter>().Where(x => m_defendersMaps.ContainsKey(x)))
                defender.Character.NextMap = m_defendersMaps[defender];

            AvAManager.Instance.OnPrismFightEnded(this, winners == ChallengersTeam && !draw);
            base.OnWinnersDetermined(winners, losers, draw);
        }

        protected override List<IFightResult> GetResults() =>
            GetFightersAndLeavers().Where(x => x.HasResult).Select(x => x.GetFightResult()).ToList();

        protected override void SendGameFightJoinMessage(CharacterFighter fighter)
        {
            ContextHandler.SendGameFightJoinMessage(fighter.Character.Client, CanCancelFight(),
                (fighter.Team == ChallengersTeam && IsAttackersPlacementPhase) || (fighter.Team == DefendersTeam && !IsAttackersPlacementPhase),
                IsStarted, IsStarted ? 0 : TempsRestant(fighter), FightType);
        }

        /// <summary>
        /// MYSTIC 17/10 : compteur affiché au client (dixièmes de seconde). L'attaquant rejoint avant le lancement du minuteur : il recevait 0
        /// (pas de compteur) ; comme pour le percepteur : 55 s pour les attaquants, 35 s pour les défenseurs, rien pour les attaquants
        /// pendant la phase des défenseurs.
        /// </summary>
        int TempsRestant(CharacterFighter fighter)
        {
            int Reste(int defaut) => m_placementTimer == null ? defaut / 100 : Math.Max(0, (int)(m_placementTimer.NextTick - System.DateTime.Now).TotalMilliseconds / 100);

            if (fighter.Team == ChallengersTeam)
                return State == FightState.NotStarted || m_attackersPhase ? Reste(AttackersPlacementTime) : 0;

            return m_attackersPhase && State != FightState.NotStarted ? 0 : Reste(DefendersPlacementTime);
        }

        protected override bool CanCancelFight() => false;
    }

    /// <summary>Agression AvA pendant un KOH (type officiel Koh) : sans alignement ni honneur ; le perdant est éliminé du KOH.</summary>
    public class FightKoh : Fight<FightPlayerTeam, FightPlayerTeam>
    {
        public FightKoh(int id, Map fightMap, FightPlayerTeam defendersTeam, FightPlayerTeam challengersTeam)
            : base(id, fightMap, defendersTeam, challengersTeam)
        {
        }

        public override FightTypeEnum FightType => FightTypeEnum.FIGHT_TYPE_Koh;

        public override bool IsPvP => true;

        public override bool IsMultiAccountRestricted => true;

        protected override void OnWinnersDetermined(FightTeam winners, FightTeam losers, bool draw)
        {
            if (!draw && losers != null)
                foreach (var perdant in losers.GetAllFightersWithLeavers().OfType<CharacterFighter>())
                    AvAManager.Instance.Eliminer(perdant.Character, Map);

            base.OnWinnersDetermined(winners, losers, draw);
        }

        protected override List<IFightResult> GetResults() =>
            GetFightersAndLeavers().Where(x => x.HasResult).Select(x => x.GetFightResult()).ToList();

        protected override bool CanCancelFight() => false;
    }
}
