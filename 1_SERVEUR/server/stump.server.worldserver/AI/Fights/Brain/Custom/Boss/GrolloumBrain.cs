using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 17/10 : Grolloum (Mine abandonnée Sakaï), d'après tofus.fr et les choix de l'utilisateur. Avant : aucune mécanique.
    ///  - invulnérable dès le début du combat ;
    ///  - tous les 2 tours, il ressuscite (25 % PV, état Zombi) le dernier VRAI monstre tué, sinon la dernière invocation ressuscitée
    ///    (son sort officiel « Cycle » utilise un effet 1034 que le serveur ne gère pas : il est retiré de l'IA) ;
    ///  - « Gelée blanche » sur un zombi, puis tuer ce zombi à son contact → vulnérable jusqu'au tour suivant du tueur
    ///    (sort officiel « Instant », suivi par VulnerableJusquAuTourDuDeclencheur) ;
    ///  - lui retirer des PM pendant qu'il est vulnérable le rend invulnérable pour le reste du tour en cours.
    ///  (pas de soin de 1500 PV : choix utilisateur)
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.GROLLOUM)]
    public class GrolloumBrain : Brain
    {
        public const int CycleSpellId = 2403;
        public const int EtatZombi = 74;
        public const int PourcentResurrection = 25;

        int m_tours;
        readonly List<FightActor> m_vraisMorts = new List<FightActor>();
        readonly List<FightActor> m_ressuscites = new List<FightActor>();
        readonly List<Buff> m_desactivationsSuspendues = new List<Buff>();

        public GrolloumBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += OnFightStarted;
        }

        void OnFightStarted(IFight fight)
        {
            fight.FightStarted -= OnFightStarted;
            Fighter.Spells.Remove(CycleSpellId);

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                EtatsBoss.Ajouter(Fighter, Fighter, EtatsBoss.Invulnerable, CycleSpellId);
            new VulnerableJusquAuTourDuDeclencheur(Fighter);

            foreach (var monstre in Fighter.Team.Fighters.Where(x => x != Fighter))
                monstre.Dead += OnAllieMort;
            Fighter.Team.FighterAdded += OnAllieAjoute;
            Fighter.FightPointsVariation += OnPointsVariation;
            fight.TurnStopped += OnTurnStopped;
            fight.FightEnded += OnFightEnded;
        }

        public override void Play()
        {
            if (++m_tours % 2 == 0)
                using (Fighter.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                    Ressusciter();

            base.Play();
        }

        #region Résurrections

        void OnAllieAjoute(FightTeam team, FightActor allie)
        {
            if (allie != Fighter)
            {
                allie.Dead -= OnAllieMort;
                allie.Dead += OnAllieMort;
            }
        }

        void OnAllieMort(FightActor allie, FightActor tueur)
        {
            // une invocation ressuscitée qui meurt n'est pas un « vrai » monstre tué
            if (allie is MonsterFighter && !m_ressuscites.Contains(allie))
            {
                m_vraisMorts.Remove(allie);
                m_vraisMorts.Add(allie);
            }
        }

        void Ressusciter()
        {
            var cible = m_vraisMorts.LastOrDefault(x => x.IsDead()) ?? m_ressuscites.LastOrDefault(x => x.IsDead());
            if (cible == null)
                return;

            var cell = CaseLibreAutour(Fighter.Cell);
            if (cell == null)
                return;

            m_vraisMorts.Remove(cible);
            m_ressuscites.Remove(cible);
            m_ressuscites.Add(cible);

            cible.Revive(PourcentResurrection, Fighter);
            cible.Position.Cell = cell;
            ActionsHandler.SendGameActionFightReviveMessage(Fighter.Fight.Clients, Fighter, cible);
            ContextHandler.SendGameFightTurnListMessage(Fighter.Fight.Clients, Fighter.Fight);
            EtatsBoss.Ajouter(cible, Fighter, EtatZombi, CycleSpellId, FightDispellableEnum.DISPELLABLE_BY_DEATH);
        }

        Cell CaseLibreAutour(Cell centre)
        {
            var fight = Fighter.Fight;
            var point = new MapPoint(centre);
            return point.GetAdjacentCells(x => true)
                .Concat(point.GetAdjacentCells(x => true).SelectMany(x => x.GetAdjacentCells(y => true)))
                .Select(x => fight.Map.Cells[x.CellId])
                .FirstOrDefault(x => x.Walkable && !x.NonWalkableDuringFight && fight.IsCellFree(x));
        }

        #endregion

        #region Retrait de PM pendant la vulnérabilité

        void OnPointsVariation(FightActor actor, ActionsEnum action, FightActor source, FightActor target, short delta)
        {
            if (action != ActionsEnum.ACTION_CHARACTER_MOVEMENT_POINTS_LOST || source == null || source.Team == Fighter.Team)
                return;

            var desactivations = Fighter.GetBuffs(x => x is DisableStateBuff d && d.StateBuff?.State?.Id == EtatsBoss.Invulnerable).ToArray();
            if (desactivations.Length == 0)
                return;

            // invulnérable pour le reste du tour : on suspend la désactivation, remise à la fin du tour en cours
            foreach (var buff in desactivations)
            {
                Fighter.RemoveBuff(buff);
                m_desactivationsSuspendues.Add(buff);
            }
        }

        void OnTurnStopped(IFight fight, FightActor joueur)
        {
            if (m_desactivationsSuspendues.Count == 0 || !Fighter.IsAlive())
                return;

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                foreach (var buff in m_desactivationsSuspendues)
                    Fighter.AddBuff(buff, true);
            m_desactivationsSuspendues.Clear();
        }

        #endregion

        void OnFightEnded(IFight fight)
        {
            fight.FightEnded -= OnFightEnded;
            fight.TurnStopped -= OnTurnStopped;
            Fighter.Team.FighterAdded -= OnAllieAjoute;
            Fighter.FightPointsVariation -= OnPointsVariation;
            foreach (var allie in Fighter.Team.Fighters)
                allie.Dead -= OnAllieMort;
        }
    }
}
