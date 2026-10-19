using System;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 18/10 : audit des donjons d'après dofuspourlesnoobs (_OUTILS/serveur/donjons/dpln/lot1..8.md, DECISIONS.md).
    // Passifs officiels du client 2.51 attribués à aucun monstre, donc jamais lancés. Chaque cerveau les pose au début du combat
    // ou à l'arrivée du monstre (invocation, vague) via SortPassif. Le reste du jeu est l'IA normale.

    /// <summary>Passif dont le niveau = grade du monstre (plafonné).</summary>
    public abstract class BrainPassifGrade : Brain
    {
        protected BrainPassifGrade(AIFighter fighter, int spellId, int niveauMax)
            : base(fighter)
        {
            var grade = (fighter as MonsterFighter)?.MonsterGrade?.GradeId ?? (fighter as SummonedMonster)?.MonsterGrade?.GradeId ?? 1;
            SortPassif.Poser(fighter, spellId, Math.Max(1, Math.Min(niveauMax, (int)grade)));
        }
    }

    /// <summary>Monstre statique : pose son passif et ne joue pas.</summary>
    public abstract class BrainPassifStatique : BrainPassif
    {
        protected BrainPassifStatique(AIFighter fighter, params int[] passifs)
            : base(fighter, passifs)
        {
        }

        public override void Play()
        {
        }
    }

    /// <summary>MYSTIC 18/10 (décision utilisateur) : IA fuyarde, comme le Fongeur : sorts puis éloignement.</summary>
    public abstract class BrainFuyard : Brain
    {
        protected BrainFuyard(AIFighter fighter)
            : base(fighter)
        {
        }

        public override bool Fuyard => true;
    }

    [BrainIdentifier(147)] public class BouftouRoyalBrain : BrainFuyard { public BouftouRoyalBrain(AIFighter f) : base(f) { } }
    [BrainIdentifier(800)] public class BatofuBrain : BrainFuyard { public BatofuBrain(AIFighter f) : base(f) { } }
    [BrainIdentifier(4294)] public class GeriatiqueBrain : BrainFuyard { public GeriatiqueBrain(AIFighter f) : base(f) { } }

    /// <summary>Chalœil : La Routourne (6301) ; chaque niveau = la variante d'un monstre du Grand Jeu (cycle de 3 bonus).</summary>
    public abstract class RoutourneBrain : Brain
    {
        protected RoutourneBrain(AIFighter fighter, int niveau)
            : base(fighter)
        {
            SortPassif.Poser(fighter, 6301, niveau);
        }
    }

    [BrainIdentifier(4283)] public class SoldatDeFortuneBrain : RoutourneBrain { public SoldatDeFortuneBrain(AIFighter f) : base(f, 1) { } }
    [BrainIdentifier(4285)] public class ValetVeinardBrain : RoutourneBrain { public ValetVeinardBrain(AIFighter f) : base(f, 2) { } }
    [BrainIdentifier(4284)] public class CavalierChanceuxBrain : RoutourneBrain { public CavalierChanceuxBrain(AIFighter f) : base(f, 3) { } }
    [BrainIdentifier(4286)] public class DameDuHasardBrain : RoutourneBrain { public DameDuHasardBrain(AIFighter f) : base(f, 4) { } }
    [BrainIdentifier(4287)] public class RoiJoueurBrain : RoutourneBrain { public RoiJoueurBrain(AIFighter f) : base(f, 5) { } }

    /// <summary>Gloutovore (Arbre de Moon) : Corps Gluant (décision utilisateur).</summary>
    [BrainIdentifier(216)] public class GloutovoreBrain : BrainPassif { public GloutovoreBrain(AIFighter f) : base(f, 6120) { } }

    /// <summary>Gargrouille (Maison Fantôme) : Rancune Totale (décision utilisateur : passif de la Gargrouille ; à sa mort les
    /// Gargrouilles passent Rancune +2 PM et débloquent Couteau).</summary>
    [BrainIdentifier(498)] public class GargrouilleBrain : BrainPassif { public GargrouilleBrain(AIFighter f) : base(f, 7075) { } }

    /// <summary>MYSTIC 18/10 : bonus de caractéristique posé par un cerveau (affiché comme un buff du sort donné).</summary>
    public static class BonusTemporaire
    {
        public static void Stat(FightActor cible, int spellId, EffectsEnum effet, PlayerFields champ, short valeur, int duree)
        {
            var handler = new Game.Effects.Handlers.Spells.Buffs.StatsBuff(
                new Game.Effects.Instances.EffectDice(effet, valeur, 0, 0) { Duration = (short)duree }, cible, null, cible.Cell, false);
            var buff = new StatBuff(cible.PopNextBuffId(), cible, cible, handler, new Game.Spells.Spell(spellId, 1), valeur, champ, false,
                FightDispellableEnum.DISPELLABLE) { Duration = (short)duree };
            cible.AddBuff(buff, true);
        }
    }

    /// <summary>MYSTIC 18/10 : états posés au début du combat ou à l'arrivée du monstre (décision utilisateur, comme le site).</summary>
    public abstract class BrainEtats : Brain
    {
        readonly int[] m_etats;
        readonly int m_sort;

        protected BrainEtats(AIFighter fighter, int sort, params int[] etats)
            : base(fighter)
        {
            m_etats = etats;
            m_sort = sort;
            if (fighter.Fight.State == FightState.Fighting)
                Poser();
            else
                fighter.Fight.FightStarted += Debut;
        }

        void Debut(IFight fight)
        {
            fight.FightStarted -= Debut;
            Poser();
        }

        void Poser()
        {
            foreach (var etat in m_etats)
                EtatsBoss.Ajouter(Fighter, Fighter, etat, m_sort);
        }
    }

    /// <summary>Masticroc et Trémorse (Père Ver) : Inébranlable + Lourd tout le combat.</summary>
    [BrainIdentifier(4729)] public class MasticrocBrain : BrainEtats { public MasticrocBrain(AIFighter f) : base(f, 7367, 157, 63) { } }
    [BrainIdentifier(4728)] public class TremorseBrain : BrainEtats { public TremorseBrain(AIFighter f) : base(f, 7367, 157, 63) { } }

    /// <summary>
    /// MYSTIC 18/10 (décision utilisateur, comme le site) : résistances tirées au hasard à chaque combat.
    /// Guerrier Pandikaze : 80 % dans 3 éléments, -80 % dans les 2 autres ; Pandikwakaze : 200 % dans un élément au hasard.
    /// </summary>
    public abstract class BrainResistancesAleatoires : Brain
    {
        protected static readonly PlayerFields[] Elements =
        {
            PlayerFields.NeutralResistPercent, PlayerFields.EarthResistPercent, PlayerFields.FireResistPercent,
            PlayerFields.WaterResistPercent, PlayerFields.AirResistPercent
        };

        static readonly Random m_hasard = new Random();

        protected BrainResistancesAleatoires(AIFighter fighter)
            : base(fighter)
        {
            PlayerFields[] ordre;
            lock (m_hasard)
                ordre = Elements.OrderBy(x => m_hasard.Next()).ToArray();
            Tirer(ordre);
        }

        protected abstract void Tirer(PlayerFields[] ordre);
    }

    [BrainIdentifier(607)]
    public class GuerrierPandikazeBrain : BrainResistancesAleatoires
    {
        public GuerrierPandikazeBrain(AIFighter f) : base(f) { }

        protected override void Tirer(PlayerFields[] ordre)
        {
            for (var i = 0; i < ordre.Length; i++)
                Fighter.Stats[ordre[i]].Base = i < 3 ? 80 : -80;
        }
    }

    [BrainIdentifier(580)]
    public class PandikwakazeBrain : BrainResistancesAleatoires
    {
        public PandikwakazeBrain(AIFighter f) : base(f) { }

        protected override void Tirer(PlayerFields[] ordre)
        {
            // l'élément à 200 % change de place au hasard, les autres résistances du grade sont gardées
            var forte = Elements.OrderByDescending(x => Fighter.Stats[x].Base).First();
            var cible = ordre[0];
            var valeurForte = Fighter.Stats[forte].Base;
            Fighter.Stats[forte].Base = Fighter.Stats[cible].Base;
            Fighter.Stats[cible].Base = Math.Max(valeurForte, 200);
        }
    }

    /// <summary>
    /// MYSTIC 18/10 : Solar et Bethel : quand tous les monstres encore en vie sont pétrifiés par Nécronyx (état 571, invulnérables),
    /// ils meurent pour terminer le combat (sinon des statues invulnérables bloquaient la victoire).
    /// </summary>
    public static class FinPetrification
    {
        public const int EtatNecronyx = 571;

        public const int SortNecronyx = 9348;
        static readonly int[] EtatsStatue = { EtatsBoss.Invulnerable, 97 /* indéplaçable */, 63 /* lourd */ };

        public static void Surveiller(AIFighter boss)
        {
            boss.Fight.TurnStopped += (fight, joueur) => Verifier(boss);

            // MYSTIC 19/10 : un monstre tué sous Nécronyx revient en statue : invulnérable, indéplaçable et il ne joue plus du tout
            // (comme les monstres corrompus du Vortex). Avant, la mort effaçait l'état 571 avant la résurrection du Pyrozomb :
            // le monstre revenait normal et rejouait.
            var suivis = new System.Collections.Generic.HashSet<FightActor>();
            var necronyxALaMort = new System.Collections.Generic.HashSet<FightActor>();
            void Suivre(FightActor f)
            {
                if (f == null || !suivis.Add(f))
                    return;
                f.BeforeDead += (mort, tueur) => { if (mort.HasState(EtatNecronyx)) necronyxALaMort.Add(mort); };
                f.GetAlive += vivant =>
                {
                    if (!necronyxALaMort.Remove(vivant))
                        return;
                    Petrifier(vivant, boss);
                };
            }

            foreach (var f in boss.Team.Fighters)
                Suivre(f);
            Suivre(boss);
            boss.Team.FighterAdded += (team, f) => Suivre(f);
        }

        static void Petrifier(FightActor statue, AIFighter boss)
        {
            if (!statue.HasState(EtatNecronyx))
                EtatsBoss.Ajouter(statue, boss, EtatNecronyx, SortNecronyx);
            foreach (var etat in EtatsStatue)
                if (!statue.HasState(etat))
                    EtatsBoss.Ajouter(statue, boss, etat, SortNecronyx);
            EtatsBoss.AjouterPasseTour(statue, boss, SortNecronyx);
        }

        static void Verifier(AIFighter boss)
        {
            var fight = boss.Fight;
            if (fight.State != FightState.Fighting)
                return;

            var vivants = boss.Team.Fighters.Where(x => x.IsAlive()).ToArray();
            if (vivants.Length == 0 || !vivants.All(x => x.HasState(EtatNecronyx)))
                return;

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                foreach (var statue in vivants)
                    statue.Die();
        }
    }

    /// <summary>Solar : Cycle Solaire (mort de l'équipe s'il meurt sans Nécronyx). Prérequis : Parchemin Nécronyx (Marchand de Doplons).</summary>
    [BrainIdentifier(5100)]
    public class SolarBrain : BrainPassif
    {
        public SolarBrain(AIFighter f) : base(f, 9415) { FinPetrification.Surveiller(f); }
    }

    [BrainIdentifier(5101)] public class Pyrozomb5101Brain : BrainPassif { public Pyrozomb5101Brain(AIFighter f) : base(f, 9354) { } }
    [BrainIdentifier(5102)] public class Pyrozomb5102Brain : BrainPassif { public Pyrozomb5102Brain(AIFighter f) : base(f, 9354) { } }
    [BrainIdentifier(5103)] public class Pyrozomb5103Brain : BrainPassif { public Pyrozomb5103Brain(AIFighter f) : base(f, 9354) { } }
    [BrainIdentifier(5104)] public class Pyrozomb5104Brain : BrainPassif { public Pyrozomb5104Brain(AIFighter f) : base(f, 9354) { } }
    [BrainIdentifier(5105)] public class Pyrozomb5105Brain : BrainPassif { public Pyrozomb5105Brain(AIFighter f) : base(f, 9354) { } }

    /// <summary>Bethel Akarna : Contrat Indicible (niveau = grade). Prérequis : Parchemin Nécronyx (Marchand de Doplons).</summary>
    [BrainIdentifier(5110)]
    public class BethelAkarnaBrain : BrainPassifGrade
    {
        public BethelAkarnaBrain(AIFighter f) : base(f, 9542, 5) { FinPetrification.Surveiller(f); }
    }

    [BrainIdentifier(5121)] public class MonolitheBrain : BrainPassif { public MonolitheBrain(AIFighter f) : base(f, 9559) { } }
    [BrainIdentifier(5122)] public class DagonDesProfondeursBrain : BrainPassif { public DagonDesProfondeursBrain(AIFighter f) : base(f, 9566) { } }
    [BrainIdentifier(5111)] public class ZombruthBrain : BrainPassif { public ZombruthBrain(AIFighter f) : base(f, 9450) { } }
    [BrainIdentifier(5112)] public class TournoyeBrain : BrainPassif { public TournoyeBrain(AIFighter f) : base(f, 9450) { } }
    [BrainIdentifier(5113)] public class FunespadonBrain : BrainPassif { public FunespadonBrain(AIFighter f) : base(f, 9450) { } }
    [BrainIdentifier(5114)] public class CranonierBrain : BrainPassif { public CranonierBrain(AIFighter f) : base(f, 9450) { } }
    [BrainIdentifier(5115)] public class MacrabBrain : BrainPassif { public MacrabBrain(AIFighter f) : base(f, 9450) { } }

    /// <summary>Miasme Polarisateur (Koutoulou, invoqué par le N'yalg) : Exhalaison Infectieuse (aura -25 % de dommages finaux, 2.51).</summary>
    [BrainIdentifier(4454)] public class MiasmePolarisateurBrain : BrainPassif { public MiasmePolarisateurBrain(AIFighter f) : base(f, 6731) { } }

    /// <summary>Diamantine (Skeunk) : Lourdeur Diamantine 13100 (nouveau sort, comme le site) : Pesanteur sur les joueurs dès le début du combat.</summary>
    [BrainIdentifier(681)] public class DiamantineBrain : BrainPassif { public DiamantineBrain(AIFighter f) : base(f, 13100) { } }

    /// <summary>Pépite du Croqueleur : niveau du passif 12297 = grade d'invocation.</summary>
    [BrainIdentifier(5665)] public class PepiteCroqueleurBrain : BrainPassifGrade { public PepiteCroqueleurBrain(AIFighter f) : base(f, 12297, 4) { } }

    /// <summary>Toile (Reine Nyée) : la toile est statique.</summary>
    [BrainIdentifier(3999)] public class ToileNefileuseBrain : BrainPassifStatique { public ToileNefileuseBrain(AIFighter f) : base(f, 5507) { } }

    /// <summary>Dardalaine : Kissifrotsipik.</summary>
    [BrainIdentifier(3992)] public class DardalaineBrain : BrainPassif { public DardalaineBrain(AIFighter f) : base(f, 5523) { } }

    /// <summary>Bworker : Poursuite.</summary>
    [BrainIdentifier(478)] public class BworkerBrain : BrainPassif { public BworkerBrain(AIFighter f) : base(f, 8675) { } }

    /// <summary>Le Chouque : Malédiction Pirate (début de tour : joueur non Maudit le plus proche + Maudits).</summary>
    /// <summary>
    /// Le Chouque : passif 6103. MYSTIC 18/10 (retour de test) : l'IA ne lançait jamais Téléportation Spectrale (6119 -> 1160 -> 6144 :
    /// échange avec l'allié le plus proche de l'ennemi visé) : le cerveau la lance si elle rapproche le Chouque d'un ennemi.
    /// </summary>
    [BrainIdentifier(230)]
    public class ChouqueBrain : BrainPassif
    {
        const int TeleportationSpectrale = 6119;

        public ChouqueBrain(AIFighter f) : base(f, 6103) { }

        public override void Play()
        {
            Teleporter();
            base.Play();
        }

        void Teleporter()
        {
            const int pesanteur = (int)SpellStatesEnum.PESANTEUR_7;
            if (!Fighter.Spells.TryGetValue(TeleportationSpectrale, out var sort) || Fighter.HasState(pesanteur))
                return;
            var ennemis = Fighter.OpposedTeam.GetAllFighters(x => x.IsAlive() && x.IsVisibleFor(Fighter)).ToArray();
            if (ennemis.Length == 0)
                return;
            var actuelle = ennemis.Min(e => e.Position.Point.ManhattanDistanceTo(Fighter.Position.Point));
            if (actuelle <= 1)
                return;
            var choix = ennemis.Select(e => new
                {
                    E = e,
                    A = Fighter.Team.GetAllFighters(a => a != Fighter && a.IsAlive() && !a.IsCarried() && !a.HasState(pesanteur) && a.CanSwitchPos())
                        .OrderBy(a => a.Position.Point.ManhattanDistanceTo(e.Position.Point)).FirstOrDefault()
                })
                .Where(x => x.A != null && x.A.Position.Point.ManhattanDistanceTo(x.E.Position.Point) < actuelle)
                .OrderBy(x => x.A.Position.Point.ManhattanDistanceTo(x.E.Position.Point))
                .FirstOrDefault(x => Fighter.CanCastSpell(sort, x.E.Cell) == Game.Spells.SpellCastResult.OK);
            if (choix != null)
                Fighter.CastSpell(sort, choix.E.Cell);
        }
    }

    /// <summary>Nakunbra : À l'abordage ! (+1 PM 1 tour par dégât reçu).</summary>
    [BrainIdentifier(229)] public class NakunbraBrain : BrainPassif { public NakunbraBrain(AIFighter f) : base(f, 6115) { } }

    /// <summary>Zilla Firefoux : Mot Deste (élément faible).</summary>
    [BrainIdentifier(599)] public class ZillaFirefouxBrain : BrainPassif { public ZillaFirefouxBrain(AIFighter f) : base(f, 4574) { } }

    /// <summary>Maho Firefoux : Culture (débloque ses Révolutions).</summary>
    [BrainIdentifier(529)] public class MahoFirefouxBrain : BrainPassif { public MahoFirefouxBrain(AIFighter f) : base(f, 4585) { } }

    /// <summary>Soryo Firefoux : Fuyumi.</summary>
    [BrainIdentifier(528)] public class SoryoFirefouxBrain : BrainPassif { public SoryoFirefouxBrain(AIFighter f) : base(f, 4577) { } }

    /// <summary>Grozilla : Lien Fusionnel.</summary>
    [BrainIdentifier(3142)] public class GrozillaBrain : BrainPassif { public GrozillaBrain(AIFighter f) : base(f, 2982) { } }

    /// <summary>Grasmera : Lien Fusionnel.</summary>
    [BrainIdentifier(3143)] public class GrasmeraBrain : BrainPassif { public GrasmeraBrain(AIFighter f) : base(f, 2982) { } }

    /// <summary>Trezz (XLII) : Rage Mahal.</summary>
    [BrainIdentifier(3840)] public class TrezzBrain : BrainPassif { public TrezzBrain(AIFighter f) : base(f, 5050) { } }

    /// <summary>Vindeux (XLII) : Rage Mahal.</summary>
    [BrainIdentifier(3841)] public class VindeuxBrain : BrainPassif { public VindeuxBrain(AIFighter f) : base(f, 5050) { } }

    /// <summary>Trantroa (XLII) : Rage Mahal.</summary>
    [BrainIdentifier(3842)] public class TrantroaBrain : BrainPassif { public TrantroaBrain(AIFighter f) : base(f, 5050) { } }

    /// <summary>Seith (XLII) : Rage Mahal.</summary>
    [BrainIdentifier(3843)] public class SeithBrain : BrainPassif { public SeithBrain(AIFighter f) : base(f, 5050) { } }

    /// <summary>Soissanth (XLII) : Rage Mahal.</summary>
    [BrainIdentifier(3844)] public class SoissanthBrain : BrainPassif { public SoissanthBrain(AIFighter f) : base(f, 5050) { } }

    /// <summary>Gromorso (Fraktale) : Fragmentation.</summary>
    [BrainIdentifier(3854)] public class GromorsoBrain : BrainPassif { public GromorsoBrain(AIFighter f) : base(f, 5052) { } }

    /// <summary>Farfacette (Fraktale) : Fragmentation.</summary>
    [BrainIdentifier(3855)] public class FarfacetteBrain : BrainPassif { public FarfacetteBrain(AIFighter f) : base(f, 5052) { } }

    /// <summary>Brikablak (Fraktale) : Fragmentation.</summary>
    [BrainIdentifier(3856)] public class BrikablakBrain : BrainPassif { public BrikablakBrain(AIFighter f) : base(f, 5052) { } }

    /// <summary>Eklatleth (Fraktale) : Fragmentation.</summary>
    [BrainIdentifier(3857)] public class EklatlethBrain : BrainPassif { public EklatlethBrain(AIFighter f) : base(f, 5052) { } }

    /// <summary>Segmantid (Fraktale) : Fragmentation.</summary>
    [BrainIdentifier(3858)] public class SegmantidBrain : BrainPassif { public SegmantidBrain(AIFighter f) : base(f, 5052) { } }

    /// <summary>Poupée Aycetroy (Kanniboul) : Frénésie Moonesque (grossit, se blesse, réinvocable à sa mort).</summary>
    [BrainIdentifier(2961)] public class PoupeeAycetroyBrain : BrainPassif { public PoupeeAycetroyBrain(AIFighter f) : base(f, 2411) { } }

    /// <summary>Père Fwetar : Vilain Garnement (+1 PM à chaque tentative de retrait de PM).</summary>
    [BrainIdentifier(1194)] public class PereFwetarBrain : BrainPassif { public PereFwetarBrain(AIFighter f) : base(f, 2557) { } }

    /// <summary>Darkli Moon : Puissance Obscure (totems, glyphes).</summary>
    [BrainIdentifier(225)]
    public class DarkliMoonBrain : BrainPassif
    {
        const int Moon = 226;
        const int PuissanceObscure = 302;

        static readonly int[] Totems = { 298, 299, 300, 301 };
        FightActor m_enAttente;

        public DarkliMoonBrain(AIFighter f)
            : base(f, PuissanceObscure)
        {
            // MYSTIC 18/10 : BeforeDamageInflicted part aussi quand le coup est annulé (DamageInflicted ne partait pas toujours)
            f.BeforeDamageInflicted += OnBeforeDamageInflicted;

            // MYSTIC 18/10 (décision utilisateur) : le Darkli est invulnérable tout le combat ; le frapper fait changer de place avec lui
            System.Action<Game.Fights.Teams.FightTeam, FightActor> arrivee = null;
            arrivee = (team, actor) =>
            {
                if (actor != f)
                    return;
                team.FighterAdded -= arrivee;
                EtatsBoss.Ajouter(f, f, EtatsBoss.Invulnerable, PuissanceObscure);
            };
            if (f.Fight.State == FightState.Fighting)
                f.Team.FighterAdded += arrivee;
            else
            {
                System.Action<IFight> debut = null;
                debut = fight =>
                {
                    fight.FightStarted -= debut;
                    EtatsBoss.Ajouter(f, f, EtatsBoss.Invulnerable, PuissanceObscure);
                };
                f.Fight.FightStarted += debut;
            }
        }

        /// <summary>
        /// MYSTIC 18/10 (retour de test) : le Darkli n'invoquait jamais ses totems (une invocation ne lance pas de sort d'invocation via
        /// l'IA) : il invoque d'abord le totem que Puissance Obscure a débloqué, puis joue normalement.
        /// </summary>
        public override void Play()
        {
            foreach (var id in Totems)
            {
                if (!Fighter.Spells.TryGetValue(id, out var totem))
                    continue;
                var niveau = totem.CurrentSpellLevel;
                if (Fighter.AP < niveau.ApCost || !Fighter.SpellHistory.CanCastSpell(niveau))
                    continue;
                var cible = Fighter.GetCastZoneSet(niveau, Fighter.Position.Point).EnumerateValidPoints()
                    .Select(p => Fighter.Fight.Map.Cells[p.CellId])
                    .Where(c => Fighter.Fight.IsCellFree(c))
                    .OrderBy(c => Fighter.Position.Point.ManhattanDistanceTo(new Game.Maps.Cells.MapPoint(c)))
                    .FirstOrDefault(c => Fighter.CanCastSpell(totem, c) == Game.Spells.SpellCastResult.OK);
                if (cible != null)
                    Fighter.CastSpell(totem, cible);
            }
            base.Play();
        }

        /// <summary>MYSTIC 18/10 (site) : frapper le Darkli donne +100 puissance au Moon (non cumulable) et l'attaquant échange sa
        /// place avec lui (arme : tout de suite ; sort : à la fin du sort).</summary>
        void OnBeforeDamageInflicted(FightActor darkli, Game.Fights.Damage damage)
        {
            var attaquant = damage?.Source;
            if (attaquant == null || attaquant.Team == Fighter.Team || damage.ReflectedDamages || damage.MarkTrigger != null
                || damage.School == Game.Effects.EffectSchoolEnum.Pushback || attaquant != Fighter.Fight.FighterPlaying)
                return;

            var moon = Fighter.Fight.GetAllFighters<MonsterFighter>(x => x.IsAlive() && x.Monster.Template.Id == Moon).FirstOrDefault();
            if (moon != null && !moon.GetBuffs(x => x is StatBuff s && s.Spell.Id == PuissanceObscure && s.Caracteristic == PlayerFields.DamageBonusPercent).Any())
                BonusTemporaire.Stat(moon, PuissanceObscure, EffectsEnum.Effect_IncreaseDamage_138, PlayerFields.DamageBonusPercent, 100, 2);

            if (damage.IsWeaponAttack)
            {
                Echanger(attaquant);
                return;
            }
            if (m_enAttente != null)
                return;
            m_enAttente = attaquant;
            attaquant.SpellCasted += ApresSort;
        }

        void ApresSort(FightActor attaquant, Game.Spells.Casts.SpellCastHandler handler)
        {
            attaquant.SpellCasted -= ApresSort;
            m_enAttente = null;
            Echanger(attaquant);
        }

        void Echanger(FightActor attaquant)
        {
            if (!Fighter.IsAlive() || !attaquant.IsAlive() || !Fighter.CanSwitchPos() || !attaquant.CanSwitchPos()
                || Fighter.IsCarrying() || attaquant.IsCarrying() || attaquant.IsCarried())
                return;
            using (Fighter.Fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                Fighter.ExchangePositions(attaquant);
                attaquant.TriggerBuffs(Fighter, BuffTriggerType.OnMoved);
                Fighter.TriggerBuffs(attaquant, BuffTriggerType.OnMoved);
            }
        }
    }

    /// <summary>Wobot Tamponneur : Bumper (rebonds, mécanique du Wa Wobot).</summary>
    /// <summary>
    /// MYSTIC 18/10 : Wa Wobot : Mékattwaction (attire et pose un Wobot Tamponneur là où était la cible) part EN PREMIER, puis
    /// Wouste frappe. Avant : Wouste (repousse 3) partait d'abord et Mékattwaction ramenait ensuite la cible (demande utilisateur).
    /// </summary>
    [BrainIdentifier(3460)]
    public class WaWobotBrain : Brain
    {
        const int Mekattwaction = 3789;

        public WaWobotBrain(AIFighter fighter)
            : base(fighter)
        {
            // MYSTIC 19/10 : après les chaînes de Bump (poussées / téléportations imbriquées), le client affichait parfois un combattant
            // à la mauvaise case : au début de chaque tour, on renvoie la vraie position de chaque combattant visible, et on journalise
            // deux combattants vivants sur la même case (vrai problème serveur).
            fighter.Fight.TurnStarted += (fight, joueur) =>
            {
                try
                {
                    var vivants = fight.GetAllFighters(x => x.IsAlive() && x.Cell != null).ToArray();
                    foreach (var g in vivants.GroupBy(x => x.Cell.Id).Where(g => g.Count() > 1))
                        NLog.LogManager.GetCurrentClassLogger().Warn($"[Wa Wobot] case {g.Key} occupée par : {string.Join(", ", g.Select(x => x.ToString()))}");
                    foreach (var f in vivants.Where(x => x.VisibleState == GameActionFightInvisibilityStateEnum.VISIBLE))
                        Stump.Server.WorldServer.Handlers.Actions.ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(fight.Clients, f, f, f.Cell);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error($"[Wa Wobot] resynchronisation : {ex.Message}");
                }
            };
        }

        public override void Play()
        {
            if (Fighter.Spells.TryGetValue(Mekattwaction, out var sort))
            {
                var cible = Fighter.OpposedTeam.GetAllFighters(x => x.IsAlive() && x.IsVisibleFor(Fighter))
                    .OrderBy(x => x.Position.Point.ManhattanDistanceTo(Fighter.Position.Point))
                    .FirstOrDefault(x => Fighter.CanCastSpell(sort, x.Cell) == Game.Spells.SpellCastResult.OK);
                if (cible != null)
                    Fighter.CastSpell(sort, cible.Cell);
            }

            base.Play();
        }
    }

    /// <summary>MYSTIC 18/10 : Wobot Tamponneur indéplaçable (état 97 : ni poussé, ni attiré, ni échangé, sauf Substitution du Wa Wobot).</summary>
    [BrainIdentifier(3459)]
    public class WobotTamponneurBrain : BrainPassif
    {
        public WobotTamponneurBrain(AIFighter f)
            : base(f, 3788)
        {
            // MYSTIC 19/10 : états posés au nom de Louwdeuw (3798) et non du Bumper (3788) : le Bump retire les effets de 3788
            // (effet 406) et les Tamponneurs perdaient Indéplaçable / Lourd, puis étaient déplacés.
            PoserEtats(f, 3798, 97, 63);
        }

        /// <summary>MYSTIC 19/10 : état Lourd (63, ne peut pas être porté) pour les Wobots (Louwdeuw).</summary>
        public static void PoserLourd(AIFighter f) => PoserEtats(f, 3798, 63);

        /// <summary>Pose des états permanents au début du combat, ou dès l'arrivée si le monstre est invoqué en cours de combat.</summary>
        static void PoserEtats(AIFighter f, int sort, params int[] etats)
        {
            if (f.Fight.State == FightState.Fighting)
            {
                Action<Game.Fights.Teams.FightTeam, FightActor> arrivee = null;
                arrivee = (team, actor) =>
                {
                    if (actor != f)
                        return;
                    team.FighterAdded -= arrivee;
                    foreach (var e in etats) EtatsBoss.Ajouter(f, f, e, sort);
                };
                f.Team.FighterAdded += arrivee;
            }
            else
            {
                Action<IFight> debut = null;
                debut = fight =>
                {
                    fight.FightStarted -= debut;
                    foreach (var e in etats) EtatsBoss.Ajouter(f, f, e, sort);
                };
                f.Fight.FightStarted += debut;
            }
        }
    }

    /// <summary>Black Wo Wabbit : Ventwe Webondi.</summary>
    [BrainIdentifier(3461)] public class BlackWoWabbitBrain : BrainPassif { public BlackWoWabbitBrain(AIFighter f) : base(f, 3794) { } }

    /// <summary>Wobot : Louwdeuw.</summary>
    [BrainIdentifier(181)] public class WobotBrain : BrainPassif { public WobotBrain(AIFighter f) : base(f, 3798) { WobotTamponneurBrain.PoserLourd(f); } }

    /// <summary>Wobot Kiafin : Louwdeuw.</summary>
    [BrainIdentifier(3462)] public class WobotKiafinBrain : BrainPassif { public WobotKiafinBrain(AIFighter f) : base(f, 3798) { WobotTamponneurBrain.PoserLourd(f); } }

    /// <summary>Tofu Fugace : Fugacité.</summary>
    [BrainIdentifier(4876)] public class TofuFugaceBrain : BrainPassif { public TofuFugaceBrain(AIFighter f) : base(f, 8712) { } }

    /// <summary>Bombe Illicale : statique.</summary>
    [BrainIdentifier(3745)] public class BombeIllicaleBrain : BrainPassifStatique { public BombeIllicaleBrain(AIFighter f) : base(f, 4773) { } }

    /// <summary>Malamibe (Baleine) : Décellularisation.</summary>
    [BrainIdentifier(3822)] public class MalamibeBrain : BrainPassifStatique { public MalamibeBrain(AIFighter f) : base(f, 4933) { } }

    /// <summary>Pupuce (Pounicheur) : Pupulsion.</summary>
    [BrainIdentifier(4291)] public class PupuceBrain : BrainPassif { public PupuceBrain(AIFighter f) : base(f, 6336) { } }

    /// <summary>MolUsk (Meno) : Mutation Instable.</summary>
    [BrainIdentifier(4455)] public class MolUskBrain : BrainPassif { public MolUskBrain(AIFighter f) : base(f, 6758) { } }

    /// <summary>CrabeYoloniste (Meno) : Mutation Instable.</summary>
    [BrainIdentifier(4456)] public class CrabeYolonisteBrain : BrainPassif { public CrabeYolonisteBrain(AIFighter f) : base(f, 6758) { } }

    /// <summary>Gambaf (Meno) : Mutation Instable.</summary>
    [BrainIdentifier(4457)] public class GambafBrain : BrainPassif { public GambafBrain(AIFighter f) : base(f, 6758) { } }

    /// <summary>Mantaze (Meno) : Mutation Instable.</summary>
    [BrainIdentifier(4458)] public class MantazeBrain : BrainPassif { public MantazeBrain(AIFighter f) : base(f, 6758) { } }

    /// <summary>Tilamproie (Meno) : Mutation Instable.</summary>
    [BrainIdentifier(4459)] public class TilamproieBrain : BrainPassif { public TilamproieBrain(AIFighter f) : base(f, 6758) { } }

    /// <summary>Dazak : Nimpatience (sans lui, résistances jamais baissées).</summary>
    [BrainIdentifier(5319)] public class DazakBrain : BrainPassif { public DazakBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>Tanklume (Dazak) : Nimpatience.</summary>
    [BrainIdentifier(5314)] public class TanklumeBrain : BrainPassif { public TanklumeBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>Boufbos (Dazak) : Nimpatience.</summary>
    [BrainIdentifier(5315)] public class BoufbosBrain : BrainPassif { public BoufbosBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>Barbelier (Dazak) : Nimpatience.</summary>
    [BrainIdentifier(5316)] public class BarbelierBrain : BrainPassif { public BarbelierBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>Kasrok (Dazak) : Nimpatience.</summary>
    [BrainIdentifier(5317)] public class KasrokBrain : BrainPassif { public KasrokBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>Vatenbiere (Dazak) : Nimpatience.</summary>
    [BrainIdentifier(5318)] public class VatenbiereBrain : BrainPassif { public VatenbiereBrain(AIFighter f) : base(f, 10346) { } }

    /// <summary>Tal Kasha.</summary>
    /// <summary>
    /// MYSTIC 18/10 (audit, site) : Tal Kasha. La chaîne 2.51 (glyphes, réanimation, retrait de PM) était cassée dans le moteur : chaque monstre
    /// tué laisse un glyphe marron et revient dessus au prochain tour de Tal Kasha (passe son 1er tour, -1 PM) ; si Tal Kasha commence son tour
    /// sur les glyphes de 3 monstres différents (Sceaux I, II), elle perd son invulnérabilité ; un retrait de PM sur elle l'échange de place avec
    /// le joueur (1 fois par tour, pas si elle est portée).
    /// </summary>
    [BrainIdentifier(4744)]
    public class TalKashaBrain : Brain
    {
        public TalKashaBrain(AIFighter f) : base(f)
        {
            SortPassif.Poser(f, 7868);
            Controleur = new TalKashaControleur(f);
        }

        public override void Play()
        {
            Controleur.DebutDeTour();
            base.Play();
        }

        public TalKashaControleur Controleur { get; }
    }

    public class TalKashaControleur
    {
        const int SortMalediction = 7868, SortRune = 8082, EtatInvulnerable = 56, SceauI = 493, SceauII = 494;
        // passifs des monstres (perdus à la mort, reposés à la résurrection)
        static readonly System.Collections.Generic.Dictionary<int, int> Passifs = new System.Collections.Generic.Dictionary<int, int>
            { { 4739, 7826 }, { 4740, 7829 }, { 4741, 7833 }, { 4742, 7840 }, { 4743, 8049 } };

        readonly AIFighter m_tk;
        IFight m_fight;
        readonly System.Collections.Generic.Dictionary<FightActor, GlypheTalKasha> m_glyphes = new System.Collections.Generic.Dictionary<FightActor, GlypheTalKasha>();
        readonly System.Collections.Generic.HashSet<FightActor> m_sceaux = new System.Collections.Generic.HashSet<FightActor>();
        bool m_vulnerable;
        FightActor m_echangeDuTourDe;

        public TalKashaControleur(AIFighter tk)
        {
            m_tk = tk;
            m_fight = tk.Fight;
            m_fight.FightStarted += Debut;
        }

        bool EstMonstre(FightActor f) => f is MonsterFighter && f != m_tk && f.Team == m_tk.Team;

        void Debut(IFight fight)
        {
            fight.FightStarted -= Debut;
            m_fight = fight;
            foreach (var m in m_tk.Team.Fighters.Where(EstMonstre).ToArray())
                Surveiller(m);
            m_tk.Team.FighterAdded += (t, f) => { if (EstMonstre(f)) Surveiller(f); };
            m_tk.PMTentative += OnTentativePM;
            fight.TurnStarted += OnTurnStarted;
        }

        void Surveiller(FightActor m)
        {
            m.Dead -= OnMort;
            m.Dead += OnMort;
        }

        void OnMort(FightActor monstre, FightActor tueur)
        {
            if (m_fight.State == FightState.Ended || !m_tk.IsAlive() || monstre.Cell == null)
                return;
            if (m_glyphes.TryGetValue(monstre, out var ancien) && m_fight.GetTriggers().Contains(ancien))
                m_fight.RemoveTrigger(ancien);
            var g = new GlypheTalKasha((short)m_fight.PopNextTriggerId(), m_tk, monstre.Cell);
            m_glyphes[monstre] = g;
            m_fight.AddTriger(g);
        }

        void OnTurnStarted(IFight fight, FightActor joueur)
        {
            m_echangeDuTourDe = null;
        }

        /// <summary>Appelé par le cerveau au début du tour de Tal Kasha, AVANT qu'elle joue (avant : après la fin de son tour).</summary>
        public void DebutDeTour()
        {
            var fight = m_fight;
            if (fight == null || !m_tk.IsAlive())
                return;
            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                // 1) sceau : Tal Kasha commence son tour sur le glyphe d'un monstre pas encore compté
                var sous = m_glyphes.FirstOrDefault(p => p.Value.CenterCell == m_tk.Cell).Key;
                if (!m_vulnerable && sous != null && m_sceaux.Add(sous))
                    Sceaux();

                // 2) les morts reviennent sur leur glyphe (ou à côté si la case est prise)
                foreach (var p in m_glyphes.ToArray().Where(p => p.Key.IsDead()))
                {
                    var cell = fight.IsCellFree(p.Value.CenterCell) ? p.Value.CenterCell
                        : fight.Map.GetRandomAdjacentFreeCell(new Game.Maps.Cells.MapPoint(p.Value.CenterCell), true);
                    if (cell == null)
                        continue;
                    var m = p.Key;
                    m.Revive(50, m_tk);
                    m.Position.Cell = cell;
                    Handlers.Actions.ActionsHandler.SendGameActionFightReviveMessage(fight.Clients, m_tk, m);
                    if (m is MonsterFighter mf && Passifs.TryGetValue(mf.Monster.Template.Id, out var passif))
                        m.CastAutoSpell(new Game.Spells.Spell(passif, 1), m.Cell);
                    BonusTemporaire.Stat(m, SortMalediction, EffectsEnum.Effect_SubMP, PlayerFields.MP, -1, -1000); // -1 PM (perdu à la mort, jamais cumulé)
                    Appliquer(m, EffectsEnum.Effect_SkipTurn, 0, 1); // passe son 1er tour
                }
                Handlers.Context.ContextHandler.SendGameFightTurnListMessage(fight.Clients, fight);
            }
        }

        void Sceaux()
        {
            Retirer(SceauI);
            Retirer(SceauII);
            switch (m_sceaux.Count)
            {
                case 1:
                    EtatsBoss.Ajouter(m_tk, m_tk, SceauI, SortMalediction);
                    break;
                case 2:
                    EtatsBoss.Ajouter(m_tk, m_tk, SceauII, SortMalediction);
                    break;
                default:
                    Retirer(EtatInvulnerable);
                    m_vulnerable = true;
                    NLog.LogManager.GetCurrentClassLogger().Info("[Tal Kasha] 3 sceaux : elle perd son invulnérabilité");
                    break;
            }
        }

        void Retirer(int etat)
        {
            foreach (var b in m_tk.GetBuffs(x => x is StateBuff s && s.State.Id == etat).ToArray())
                m_tk.RemoveBuff(b);
        }

        void OnTentativePM(FightActor cible, FightActor source)
        {
            if (source == null || !source.IsAlive() || source.Team == m_tk.Team || !m_tk.IsAlive() || m_tk.IsCarried())
                return;
            if (m_echangeDuTourDe == m_fight.FighterPlaying || !source.CanSwitchPos() || source.IsCarrying())
                return;
            m_echangeDuTourDe = m_fight.FighterPlaying;
            using (m_fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                m_tk.ExchangePositions(source);
        }

        void Appliquer(FightActor cible, EffectsEnum effet, int valeur, int duree)
        {
            try
            {
                var dice = new Game.Effects.Instances.EffectDice(effet, 0, valeur, 0) { Duration = duree };
                var cast = new Game.Spells.Casts.DefaultSpellCastHandler(new Game.Fights.SpellCastInformations(m_tk, new Game.Spells.Spell(SortRune, 1), cible.Cell));
                var h = Game.Effects.EffectManager.Instance.GetSpellEffectHandler(dice, m_tk, cast, cible.Cell, false);
                h.SetAffectedActors(new[] { cible });
                h.Apply();
            }
            catch (System.Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error("[Tal Kasha] effet {0} sur {1} : {2}", effet, cible, ex.Message);
            }
        }
    }

    /// <summary>Glyphe marron d'un monstre mort de la Chambre de Tal Kasha (visuel ; le contrôleur gère sceaux et résurrection).</summary>
    public class GlypheTalKasha : Game.Fights.Triggers.Glyph
    {
        public GlypheTalKasha(short id, FightActor tk, Database.World.Cell cell)
            : base(id, tk, new Game.Spells.Spell(7868, 1), new Game.Effects.Instances.EffectDice(EffectsEnum.Effect_Glyph, 0, 7868, 4) { Duration = -1 },
                   new Game.Spells.Spell(7868, 1), cell, SpellShapeEnum.P, 0, 0, System.Drawing.Color.FromArgb(120, 72, 24), false,
                   Game.Fights.Triggers.TriggerType.NEVER)
        {
        }

        public override void Trigger(FightActor trigger, Database.World.Cell triggerCell)
        {
        }
    }

    /// <summary>Chakanoubis : Pouvoir antique.</summary>
    [BrainIdentifier(4739)] public class ChakanoubisBrain : BrainPassif { public ChakanoubisBrain(AIFighter f) : base(f, 7826) { } }

    /// <summary>Bandleth : Morve érosive.</summary>
    [BrainIdentifier(4740)] public class BandlethBrain : BrainPassif { public BandlethBrain(AIFighter f) : base(f, 7829) { } }

    /// <summary>Momistik : Héritage Huppermage.</summary>
    [BrainIdentifier(4741)] public class MomistikBrain : BrainPassif { public MomistikBrain(AIFighter f) : base(f, 7833) { } }

    /// <summary>Rykaon : Rythme maudit.</summary>
    [BrainIdentifier(4742)] public class RykaonBrain : BrainPassif { public RykaonBrain(AIFighter f) : base(f, 7840) { } }

    /// <summary>Griffotep : Maudification.</summary>
    [BrainIdentifier(4743)] public class GriffotepBrain : BrainPassif { public GriffotepBrain(AIFighter f) : base(f, 8049) { } }

    /// <summary>Mawabouaino (Waddicts).</summary>
    [BrainIdentifier(5653)] public class MawabouainoBrain : BrainPassif { public MawabouainoBrain(AIFighter f) : base(f, 11944) { } }

    /// <summary>Croqueleur (Croquanterie).</summary>
    [BrainIdentifier(5664)] public class CroqueleurBrain : BrainPassif { public CroqueleurBrain(AIFighter f) : base(f, 12290) { } }

    /// <summary>Pépite croquante : Croquez-moi.</summary>
    [BrainIdentifier(5676)] public class PepiteCroquanteBrain : BrainPassif { public PepiteCroquanteBrain(AIFighter f) : base(f, 12024) { } }

    /// <summary>Kakoalak (Croquanterie).</summary>
    [BrainIdentifier(5677)] public class KakoalakBrain : BrainPassif { public KakoalakBrain(AIFighter f) : base(f, 12023) { } }

    /// <summary>Mansocolat (Croquanterie).</summary>
    [BrainIdentifier(5678)] public class MansocolatBrain : BrainPassif { public MansocolatBrain(AIFighter f) : base(f, 12023) { } }

    /// <summary>GloursonGuimauve (Croquanterie).</summary>
    [BrainIdentifier(5679)] public class GloursonGuimauveBrain : BrainPassif { public GloursonGuimauveBrain(AIFighter f) : base(f, 12023) { } }

    /// <summary>Chocoskargo (Croquanterie).</summary>
    [BrainIdentifier(5680)] public class ChocoskargoBrain : BrainPassif { public ChocoskargoBrain(AIFighter f) : base(f, 12023) { } }

    /// <summary>Kwakao (Croquanterie).</summary>
    [BrainIdentifier(5681)] public class KwakaoBrain : BrainPassif { public KwakaoBrain(AIFighter f) : base(f, 12023) { } }

    /// <summary>Prêtresse du Kao : Artisanat du Kao.</summary>
    [BrainIdentifier(5666)] public class PretresseKaoBrain : BrainPassif { public PretresseKaoBrain(AIFighter f) : base(f, 12311) { } }

    /// <summary>Éclat Kao.</summary>
    [BrainIdentifier(5667)] public class EclatKaoBrain : BrainPassif { public EclatKaoBrain(AIFighter f) : base(f, 11993) { } }

    /// <summary>Chocoligarque (Kao) : Chocolatomanie.</summary>
    [BrainIdentifier(5668)] public class ChocoligarqueBrain : BrainPassif { public ChocoligarqueBrain(AIFighter f) : base(f, 11994) { } }

    /// <summary>Torrefactueur (Kao) : Chocolatomanie.</summary>
    [BrainIdentifier(5669)] public class TorrefactueurBrain : BrainPassif { public TorrefactueurBrain(AIFighter f) : base(f, 11994) { } }

    /// <summary>Pralicienne (Kao) : Chocolatomanie.</summary>
    [BrainIdentifier(5670)] public class PralicienneBrain : BrainPassif { public PralicienneBrain(AIFighter f) : base(f, 11994) { } }

    /// <summary>Temperaturge (Kao) : Chocolatomanie.</summary>
    [BrainIdentifier(5671)] public class TemperaturgeBrain : BrainPassif { public TemperaturgeBrain(AIFighter f) : base(f, 11994) { } }

    /// <summary>Cabosseur (Kao) : Chocolatomanie.</summary>
    [BrainIdentifier(5672)] public class CabosseurBrain : BrainPassif { public CabosseurBrain(AIFighter f) : base(f, 11994) { } }

    /// <summary>Chause (Katrepat).</summary>
    [BrainIdentifier(4968)] public class ChauseBrain : BrainPassif { public ChauseBrain(AIFighter f) : base(f, 9078) { } }

    /// <summary>Ectorche (Katrepat).</summary>
    [BrainIdentifier(4969)] public class EctorcheBrain : BrainPassif { public EctorcheBrain(AIFighter f) : base(f, 9078) { } }

    /// <summary>Esprigne (Katrepat).</summary>
    [BrainIdentifier(4970)] public class EsprigneBrain : BrainPassif { public EsprigneBrain(AIFighter f) : base(f, 9078) { } }

    /// <summary>Feutome (Katrepat).</summary>
    [BrainIdentifier(4971)] public class FeutomeBrain : BrainPassif { public FeutomeBrain(AIFighter f) : base(f, 9078) { } }

    /// <summary>Crâme (Katrepat).</summary>
    [BrainIdentifier(4972)] public class CrameBrain : BrainPassif { public CrameBrain(AIFighter f) : base(f, 9075) { } }

    /// <summary>Père Ver.</summary>
    [BrainIdentifier(4726)] public class PereVerBrain : BrainPassif { public PereVerBrain(AIFighter f) : base(f, 7367) { } }

    /// <summary>Cycloporth (Père Ver).</summary>
    [BrainIdentifier(4731)] public class CycloporthBrain : BrainPassif { public CycloporthBrain(AIFighter f) : base(f, 7851) { } }

    /// <summary>Morsquale (Père Ver).</summary>
    [BrainIdentifier(4730)] public class MorsqualeBrain : BrainPassif { public MorsqualeBrain(AIFighter f) : base(f, 7845) { } }

    /// <summary>Comte Razof (sans lui, résistances jamais baissées).</summary>
    [BrainIdentifier(4803)] public class ComteRazofBrain : BrainPassif { public ComteRazofBrain(AIFighter f) : base(f, 8512) { } }

    /// <summary>Minikrone (Razof).</summary>
    /// <summary>Minikrone (Comte Razof) : Monokeros ; MYSTIC 18/10 (retour de test) : elle va au contact du Razof.</summary>
    [BrainIdentifier(4812)]
    public class MinikroneBrain : BrainPassif
    {
        public MinikroneBrain(AIFighter f) : base(f, 8530) { }

        public override void Play()
        {
            var razof = Fighter.Fight.GetAllFighters<MonsterFighter>(x => x.IsAlive() && x.Monster.Template.Id == (int)MonsterIdEnum.COMTE_RAZOF).FirstOrDefault();
            if (razof == null)
            {
                base.Play();
                return;
            }
            if (!Fighter.Position.Point.IsAdjacentTo(razof.Position.Point))
                foreach (var r in new Stump.Server.WorldServer.AI.Fights.Actions.MoveNearTo(Fighter, razof).Execute(this))
                    if (r == TreeSharp.RunStatus.Failure)
                        break;
        }
    }

    /// <summary>El Piko.</summary>
    [BrainIdentifier(4609)] public class ElPikoBrain : BrainPassif { public ElPikoBrain(AIFighter f) : base(f, 7199) { } }

    /// <summary>Choudini.</summary>
    [BrainIdentifier(4860)] public class ChoudiniBrain : BrainPassif { public ChoudiniBrain(AIFighter f) : base(f, 8630) { } }

    /// <summary>
    /// MYSTIC 19/10 (audit Laboratoire de Brumen Tinctorias) : Nelween lance Exhalation Toxique (2529 : -30 % résistances aux joueurs
    /// autour, puis Toxine surprise 2531 = le joueur intercepte les dommages des monstres proches de lui) EN PREMIER dès qu'un joueur
    /// est à son contact. Avant, l'IA ne choisissait que ses sorts de dégâts : la mécanique officielle n'apparaissait jamais.
    /// </summary>
    [BrainIdentifier(3100)]
    public class NelweenBrain : Brain
    {
        const int ExhalationToxique = 2529;

        public NelweenBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override void Play()
        {
            if (Fighter.Spells.TryGetValue(ExhalationToxique, out var sort))
            {
                var cible = Fighter.OpposedTeam.GetAllFighters(x => x.IsAlive() && x is CharacterFighter && x.IsVisibleFor(Fighter) &&
                        x.Position.Point.IsAdjacentTo(Fighter.Position.Point))
                    .FirstOrDefault(x => Fighter.CanCastSpell(sort, x.Cell) == Game.Spells.SpellCastResult.OK);
                if (cible != null)
                {
                    Fighter.CastSpell(sort, cible.Cell);
                    NLog.LogManager.GetCurrentClassLogger().Info($"[Nelween] Exhalation Toxique sur {cible}");
                }
            }

            base.Play();
        }
    }

    /// <summary>
    /// MYSTIC 19/10 (demande utilisateur, passif absent de la 2.51) : Kolérat — frappé au CONTACT par un ennemi, il renvoie 20 % des
    /// dommages reçus à l'attaquant (pas sur la poussée ni sur un renvoi).
    /// </summary>
    [BrainIdentifier(300)] [BrainIdentifier(3097)] [BrainIdentifier(4875)]
    public class KoleratBrain : Brain
    {
        const double PartRenvoyee = 0.20;

        public KoleratBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.DamageInflicted += (cible, degats) =>
            {
                var attaquant = degats?.Source;
                if (attaquant == null || attaquant == cible || !attaquant.IsAlive() || !cible.IsEnnemyWith(attaquant) || degats.Generated ||
                    degats.School == Stump.Server.WorldServer.Game.Effects.EffectSchoolEnum.Pushback || degats.Amount <= 0 ||
                    !attaquant.Position.Point.IsAdjacentTo(cible.Position.Point))
                    return;
                var renvoi = (int)System.Math.Ceiling(degats.Amount * PartRenvoyee);
                attaquant.InflictDamage(new Game.Fights.Damage(renvoi)
                {
                    Source = cible,
                    School = degats.School,
                    IgnoreDamageBoost = true,
                    IgnoreDamageReduction = true,
                    Generated = true
                });
            };
        }
    }

    /// <summary>Mantiscore : Garde-bouclier.</summary>
    [BrainIdentifier(4621)] public class MantiscoreBrain : BrainPassif { public MantiscoreBrain(AIFighter f) : base(f, 7223) { } }

    /// <summary>Anerice : Résilience vampyrique.</summary>
    [BrainIdentifier(4882)] public class AnericeBrain : BrainPassif { public AnericeBrain(AIFighter f) : base(f, 8727) { } }

    /// <summary>Goulafre : Goulala.</summary>
    [BrainIdentifier(4877)] public class GoulafreBrain : BrainPassif { public GoulafreBrain(AIFighter f) : base(f, 8716) { } }

    /// <summary>Kerigoule (Anerice) : Marche des goules.</summary>
    [BrainIdentifier(4878)] public class KerigouleBrain : BrainPassif { public KerigouleBrain(AIFighter f) : base(f, 8726) { } }

    /// <summary>Gouligane (Anerice) : Marche des goules.</summary>
    [BrainIdentifier(4879)] public class GouliganeBrain : BrainPassif { public GouliganeBrain(AIFighter f) : base(f, 8726) { } }

    /// <summary>Goultime (Anerice) : Marche des goules.</summary>
    [BrainIdentifier(4880)] public class GoultimeBrain : BrainPassif { public GoultimeBrain(AIFighter f) : base(f, 8726) { } }

    /// <summary>Pipisteuse (Anerice) : Marche des goules.</summary>
    [BrainIdentifier(4881)] public class PipisteuseBrain : BrainPassif { public PipisteuseBrain(AIFighter f) : base(f, 8726) { } }

}
