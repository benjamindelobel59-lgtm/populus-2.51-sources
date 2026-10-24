using Stump.Core.Extensions;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Linq;
using static Stump.Server.WorldServer.Game.Actors.Fight.FightActor;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// PADGREF 22/10 (audit Belvédère d'Ilyzaelle, données 2.51 + guide DPLN). Le passif « Possession » (9052) déroule tout le combat :
    /// paliers 75 % / 50 % (9049 → « Défense des Rois » 9048 : case 437, état 555 invulnérable, glyphe-aura 9046 +400 % aux joueurs,
    /// 15 000 aux monstres qui y entrent, +2 PM aux monstres), retour (9047) puis « Éradication » (9050) au 2e palier, vagues
    /// « Invasion » (9085 / 9084 / 9083), Brûle-âme (9079). Corrections :
    /// - niveau de Possession selon le nombre de joueurs (avant : toujours 1 = vagues « 4 joueurs ou moins ») ;
    /// - palier atteint dès le coup qui fait passer sous 75 % / 50 % (le déclencheur « D » part AVANT la perte de PV) ;
    /// - dans le camp des joueurs (état 555) elle ne joue pas ; le glyphe (durée infinie dans les données) est retiré à la fin de la phase ;
    /// - « Vers les ténèbres » (9054, sur elle) lancé en début de tour quand il est disponible (tours 3, 6, 9…) ;
    /// - Éradication : tue les monstres PUIS elle (voir Eradiquer).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.ILYZAELLE)]
    public class IlyzaelleBrain : Brain
    {
        public const int Possession = 9052, Palier = 9049, VersLesTenebres = 9054, Eradication = 9050, GlypheDefense = 9046;
        public const int EtatDefenseDesRois = 555;

        static readonly NLog.Logger journal = NLog.LogManager.GetCurrentClassLogger();
        static readonly System.Random hasard = new System.Random();

        public IlyzaelleBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            fighter.DamageInflicted += OnDamageInflicted;
            fighter.BuffRemoved += OnBuffRemoved;
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            // groupe remplacé selon le nombre de joueurs (MonsterGroupWithAlternatives) : l'ancienne Ilyzaelle n'est plus dans le combat
            if (!Fighter.Team.Fighters.Contains(Fighter) || !Fighter.IsAlive())
                return;

            Fighter.Stats[PlayerFields.SummonLimit].Additional = 9999;
            Fighter.Stats[PlayerFields.CriticalDamageReduction].Additional = 250;

            var joueurs = Fighter.OpposedTeam.Fighters.OfType<CharacterFighter>().Count();
            var niveau = NiveauPossession(joueurs);
            journal.Info("[Ilyzaelle] {0} joueur(s) : Possession niveau {1}", joueurs, niveau);
            Fighter.CastAutoSpell(new Spell(Possession, (short)niveau), Fighter.Cell);
        }

        /// <summary>Niveaux 2.51 de Possession : 1 = 4 joueurs ou moins (2 monstres dès le tour 4), 2 = 5 joueurs (2 monstres dès le
        /// tour 3), 3 à 5 = 6, 7, 8 joueurs (3 monstres dès le tour 3). Même découpage que les grades / PV du boss.</summary>
        public static int NiveauPossession(int joueurs) => joueurs <= 4 ? 1 : joueurs == 5 ? 2 : System.Math.Min(5, joueurs - 3);

        private void OnDamageInflicted(FightActor fighter, Damage damage)
        {
            if (!Fighter.IsAlive() || Fighter.Fight.State != FightState.Fighting)
                return;

            // palier en attente : déclencheurs de 9049 niv. 2 (75 %) ou niv. 3 (50 %) encore posés sur elle
            var niveau = Fighter.GetBuffs(x => x is TriggerBuff && x.Spell.Id == Palier).Select(x => (int)x.Spell.CurrentLevel).DefaultIfEmpty(0).Max();
            var seuil = niveau == 2 ? 75 : niveau == 3 ? 50 : 0;
            if (seuil == 0 || Fighter.LifePoints * 100d / System.Math.Max(1, Fighter.MaxLifePoints) > seuil)
                return;

            journal.Info("[Ilyzaelle] palier {0} % atteint ({1}/{2} PV) : Défense des Rois", seuil, Fighter.LifePoints, Fighter.MaxLifePoints);
            Fighter.CastAutoSpell(new Spell(Palier, (short)niveau), Fighter.Cell);
        }

        private void OnBuffRemoved(FightActor fighter, Buff buff)
        {
            if (buff is StateBuff etat && etat.State.Id == EtatDefenseDesRois && !Fighter.HasState(EtatDefenseDesRois))
                RetirerGlyphe();
        }

        void RetirerGlyphe()
        {
            foreach (var glyphe in Fighter.Fight.GetTriggers().OfType<GlyphAura>().Where(x => x.Caster == Fighter && x.GlyphSpell.Id == GlypheDefense).ToArray())
                glyphe.Remove();
        }

        public override void Play()
        {
            // dans le camp des joueurs : elle ne frappe plus et ne bouge pas (guide)
            if (Fighter.HasState(EtatDefenseDesRois))
                return;

            SortsForces.Lancer(Fighter, VersLesTenebres, Fighter.Cell);
            base.Play();
        }

        public const int DefenseDesRois = 9048, CaseDefense = 437;

        /// <summary>
        /// PADGREF 22/10 (demande utilisateur) : Défense des Rois (9048) envoie toujours Ilyzaelle sur la case 437. Si quelqu'un l'occupe,
        /// il est poussé sur une case collée à 437, à gauche ou à droite à l'écran (avant : échange de place 2.51, la personne partait
        /// à l'ancienne place d'Ilyzaelle). Appelé juste avant le lancer de 9048 (CastSpell.cs).
        /// </summary>
        public static void LibererCase(FightActor ilyzaelle)
        {
            var fight = ilyzaelle.Fight;
            if (fight?.Map == null || CaseDefense >= fight.Map.Cells.Length)
                return;

            var cible = fight.Map.Cells[CaseDefense];
            var occupant = fight.GetOneFighter(cible);
            if (occupant == null || occupant == ilyzaelle)
                return;

            bool Libre(Database.World.Cell c) => c != null && c.Walkable && !c.NonWalkableDuringFight && fight.IsCellFree(c);

            var centre = new Game.Maps.Cells.MapPoint(cible);
            // cases collées à 437 (ses 4 voisines sont toutes à sa gauche ou à sa droite à l'écran), au hasard parmi les libres ;
            // à défaut, la case libre la plus proche
            var arrivee = centre.GetAdjacentCells(id => Libre(fight.Map.Cells[id]))
                .Select(p => fight.Map.Cells[p.CellId])
                .OrderBy(c => hasard.Next())
                .FirstOrDefault()
                ?? fight.Map.Cells.Where(Libre).OrderBy(c => new Game.Maps.Cells.MapPoint(c).ManhattanDistanceTo(centre)).FirstOrDefault();
            if (arrivee == null)
                return;

            journal.Info("[Ilyzaelle] case {0} occupée par {1} : poussé case {2}", CaseDefense, occupant, arrivee.Id);
            occupant.Position.Cell = arrivee;
            fight.ForEach(entry => Handlers.Actions.ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(entry.Client, ilyzaelle, occupant, arrivee), true);
            fight.TriggerMarks(arrivee, occupant, TriggerType.MOVE);
        }

        /// <summary>
        /// « Éradication » (9050 → 9051 sur chaque allié : tue l'allié et le lanceur). Avant : Ilyzaelle mourait dès le 1er monstre et
        /// lançait la suite morte. Maintenant : les monstres d'abord, puis elle (fin du combat). Sans monstre, rien (comme le guide :
        /// le combat continue jusqu'à ce qu'on la tue).
        /// </summary>
        public static void Eradiquer(MonsterFighter ilyzaelle)
        {
            if (!ilyzaelle.IsAlive() || ilyzaelle.Fight.State != FightState.Fighting)
                return;

            var monstres = ilyzaelle.Team.Fighters.Where(x => x != ilyzaelle && x.IsAlive()).ToArray();
            journal.Info("[Ilyzaelle] Éradication : {0} monstre(s)", monstres.Length);
            if (monstres.Length == 0)
                return;

            foreach (var monstre in monstres)
                if (monstre.IsAlive())
                    monstre.Die(ilyzaelle);

            if (ilyzaelle.IsAlive() && ilyzaelle.Fight.State == FightState.Fighting)
                ilyzaelle.Die(ilyzaelle);
        }
    }

    /// <summary>
    /// PADGREF 22/10 (audit Belvédère d'Ilyzaelle) : Chause, Ectorche, Esprigné, Feutôme, Crâme. Un joueur qui déplace un monstre ou
    /// tente de lui retirer des PM prend l'état Brûlure (556, Brûle-âme 9079) ; le premier monstre qui joue ensuite lance FORCÉMENT
    /// son sort « spécial » sur un joueur en Brûlure (au hasard s'il y en a plusieurs), ce qui consomme l'état. L'IA normale ne
    /// choisissait pas ces sorts (ou les lançait sans cible valable) : ils sont lancés ici et cachés à l'IA pour le reste du tour.
    /// </summary>
    public abstract class BrulureBrain : BrainPassif
    {
        public const int Brulure = 556;

        static readonly System.Random hasard = new System.Random();

        readonly int m_special;

        protected BrulureBrain(AIFighter fighter, int passif, int special)
            : base(fighter, passif)
        {
            m_special = special;
        }

        public override void Play()
        {
            ConsommerBrulure();

            Fighter.Spells.TryGetValue(m_special, out var special);
            Fighter.Spells.Remove(m_special);
            try
            {
                base.Play();
            }
            finally
            {
                if (special != null && !Fighter.Spells.ContainsKey(m_special))
                    Fighter.Spells.Add(m_special, special);
            }
        }

        void ConsommerBrulure()
        {
            var cibles = Fighter.OpposedTeam.Fighters.Where(x => x.IsAlive() && x.HasState(Brulure)).OrderBy(x => hasard.Next()).ToArray();
            foreach (var cible in cibles)
            {
                if (SortsForces.Lancer(Fighter, m_special, cible.Cell))
                    return;
            }
        }
    }
}
