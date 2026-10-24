using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 17/10 : audit des donjons (AUDIT_MECANIQUES.md) — les passifs officiels de ces monstres existent dans le client 2.51
    // mais n'étaient lancés par rien (pas de cerveau). Chaque cerveau pose le passif au début du combat, ou à l'arrivée du monstre
    // s'il est invoqué / arrive en vague (SortPassif). Le reste du jeu est l'IA normale. Règle validée : on suit les données 2.51.

    /// <summary>MYSTIC 18/10 : relève la limite d'invocations d'un boss (le moteur la met à 1 pour tous les monstres).</summary>
    public static class LimiteInvocations
    {
        public static void Fixer(AIFighter fighter, int limite)
        {
            fighter.Stats[PlayerFields.SummonLimit].Base = limite;
            if (fighter.Fight.State != FightState.Fighting)
                fighter.Fight.FightStarted += Debut;

            void Debut(IFight fight)
            {
                fight.FightStarted -= Debut;
                fighter.Stats[PlayerFields.SummonLimit].Base = limite;
            }
        }
    }

    public abstract class BrainPassif : Brain
    {
        protected BrainPassif(AIFighter fighter, params int[] passifs)
            : base(fighter)
        {
            foreach (var passif in passifs)
                SortPassif.Poser(fighter, passif);
        }
    }

    /// <summary>Toxoliath : Toxmose (dégâts eau par PM sur coup reçu).</summary>
    [BrainIdentifier(3752)] public class ToxoliathBrain : BrainPassif { public ToxoliathBrain(AIFighter f) : base(f, 4796) { } }

    // Lucrane (vague du Toxoliath) : Carapace gardée codée dans ToxoliathMonstresBrains.cs (PADGREF 21/10)

    // Malléfisk : voir FabriqueMallefiskBrains.cs (MYSTIC 20/10)

    /// <summary>Kanigroula : Phéromones bestiales.</summary>
    [BrainIdentifier(3556)] public class KanigroulaBrain : BrainPassif { public KanigroulaBrain(AIFighter f) : base(f, 4093) { } }

    /// <summary>Missiz Frizz : Congère (invulnérabilité glaciale des monstres, pièges, Antigel quand on pousse).</summary>
    [BrainIdentifier(3391)]
    public class MissizFrizzBrain : BrainPassif
    {
        /// <summary>PADGREF 21/10 (retour utilisateur) : elle se soigne de 15 % de ses PV max (valeur d'Antigel 3520) à chaque dommage
        /// de poussée, sauf ceux du Piège glacial (3522, Vergladiateur posé par les joueurs en début de tour).</summary>
        public const int PiegeGlacial = 3522, SoinPourcent = 15;

        public MissizFrizzBrain(AIFighter f) : base(f, 3517)
        {
            f.DamageInflicted += (cible, d) =>
            {
                if (d.School != Game.Effects.EffectSchoolEnum.Pushback || d.Amount <= 0 || d.SortPoussee == PiegeGlacial || !cible.IsAlive())
                    return;
                var soin = cible.Heal((int)(cible.MaxLifePoints * SoinPourcent / 100d), cible, true);
                NLog.LogManager.GetCurrentClassLogger().Info("[Frizz] poussée (sort {0}) : -{1} PV, soin +{2}", d.SortPoussee, d.Amount, soin);
            };
        }
    }

    /// <summary>
    /// PADGREF 22/10 (retour utilisateur, Antre du Klime) : Harrogant : « Fierté » (3471 : sur lui, 1 tour : frappé → Insolence 3470 :
    /// soin de 101 à 120 à ses alliés à 1-5 cases et répartition des dommages subis) n'était jamais lancé par l'IA normale, donc
    /// Insolence ne soignait jamais. Lancé au début de chacun de ses tours.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.HARROGANT)]
    public class HarrogantBrain : Brain
    {
        public const int Fierte = 3471;

        public HarrogantBrain(AIFighter fighter) : base(fighter) { }

        public override void Play()
        {
            SortsForces.Lancer(Fighter, Fierte, Fighter.Cell);
            base.Play();
        }
    }

    /// <summary>
    /// PADGREF 21/10 (retour utilisateur) : Verglasseur : « Braconnage » (3511 : sur lui, +2 PM à ses alliés au contact, et 1 tour
    /// frappé à distance → Braquage 3512 : -75 % PV aux ennemis et -25 % à ses alliés à 1-5 cases) n'était pas lancé par l'IA
    /// normale, donc Braquage ne partait jamais. Lancé au début de chacun de ses tours.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.VERGLASSEUR)]
    public class VerglasseurBrain : Brain
    {
        public const int Braconnage = 3511;

        public VerglasseurBrain(AIFighter fighter) : base(fighter) { }

        public override void Play()
        {
            SortsForces.Lancer(Fighter, Braconnage, Fighter.Cell);
            base.Play();
        }
    }

    /// <summary>Daïgoro : Corps de Dolomite (états Pandawush qui débloquent ses sorts).</summary>
    [BrainIdentifier(3208)] public class DaigoroBrain : BrainPassif { public DaigoroBrain(AIFighter f) : base(f, 3037) { } }

    /// <summary>Capitaine Ekarlatte : Kutcha Libre.</summary>
    [BrainIdentifier(3753)] public class EkarlatteBrain : BrainPassif { public EkarlatteBrain(AIFighter f) : base(f, 4804) { } }

    /// <summary>Tromblion (vague d'Ekarlatte) : Isolement (pièges en fin de tour).</summary>
    [BrainIdentifier(3733)] public class TromblionBrain : BrainPassif { public TromblionBrain(AIFighter f) : base(f, 4730) { } }

    /// <summary>Haute Truche : Struchthio.</summary>
    [BrainIdentifier(3618)] public class HauteTrucheBrain : BrainPassif { public HauteTrucheBrain(AIFighter f) : base(f, 4367) { } }

    [BrainIdentifier(3619)] public class GrucheBrain : BrainPassif { public GrucheBrain(AIFighter f) : base(f, 4376) { } }
    [BrainIdentifier(3620)] public class TruchmucheBrain : BrainPassif { public TruchmucheBrain(AIFighter f) : base(f, 4388) { } }
    [BrainIdentifier(3621)] public class TruchideurBrain : BrainPassif { public TruchideurBrain(AIFighter f) : base(f, 4394) { } }
    [BrainIdentifier(3622)] public class TruchtineBrain : BrainPassif { public TruchtineBrain(AIFighter f) : base(f, 4399) { } }
    [BrainIdentifier(3623)] public class TruchonBrain : BrainPassif { public TruchonBrain(AIFighter f) : base(f, 4404) { } }

    /// <summary>Meulou : Fureur (version 2.51 : invulnérable au début du tour de chaque joueur jusqu'au 1er coup ; +30 puissance aux monstres frappés).</summary>
    // MYSTIC 18/10 : 2 invocations (décision utilisateur) : sinon le Mulou d'Étripage / de Mulanthropie est bloqué tant qu'un Milimeulou vit
    [BrainIdentifier(232)]
    public class MeulouBrain : BrainPassif
    {
        public MeulouBrain(AIFighter f) : base(f, 305)
        {
            LimiteInvocations.Fixer(f, 2);
        }
    }

    /// <summary>Phossile : Phorreur de Gloire.</summary>
    // MYSTIC 18/10 : le passif est posé avant le tour 1 alors que le délai (4) d'Abîme se décompte au début de chaque tour du
    // Phossile → Abîme partait aux tours 4, 8, 12 ; officiel : 5, 9, 13
    [BrainIdentifier(3651)]
    public class PhossileBrain : BrainPassif
    {
        public PhossileBrain(AIFighter f) : base(f, 4497)
        {
            if (f.Fight.State != FightState.Fighting)
                f.Fight.FightStarted += Decaler;
        }

        void Decaler(IFight fight)
        {
            fight.FightStarted -= Decaler;
            foreach (var buff in System.Linq.Enumerable.ToArray(Fighter.GetBuffs(x => x.Spell.Id == 4498 && x.Delay > 0)))
            {
                buff.Delay++;
                fight.UpdateBuff(buff, false);
            }
        }
    }

    /// <summary>XLII : Buff heure (invoque son Auroraire 3850).</summary>
    [BrainIdentifier(3849)] public class XliiBrain : BrainPassif { public XliiBrain(AIFighter f) : base(f, 5106) { } }

    /// <summary>Auroraire de XLII : Heure du temps 5104 (multiplicateur de dégâts selon l'heure) ; ne joue pas.</summary>
    [BrainIdentifier(3850)]
    public class AuroraireXliiBrain : BrainPassif
    {
        public AuroraireXliiBrain(AIFighter f) : base(f, 5104) { }

        public override void Play()
        {
        }
    }

    /// <summary>Sylargh : Chambranle (invulnérable, Nécromignon / Zombidule).</summary>
    /// MYSTIC 18/10 (décision utilisateur, comme le site) : tout Zombi qui meurt lance Zombidule, même s'il n'a pas encore joué (en 2.51
    /// strict il ne l'avait qu'à son 1er tour) ; journal [Sylargh] pour vérifier le déverrouillage (distance 5 à 10 PO).
    [BrainIdentifier(3409)]
    public class SylarghBrain : BrainPassif
    {
        const int Zombidule = 3608, EtatZombi = 74;

        public SylarghBrain(AIFighter f) : base(f, 3606)
        {
            f.Fight.FightStarted += Debut;
        }

        void Debut(IFight fight)
        {
            fight.FightStarted -= Debut;
            foreach (var m in Fighter.Team.Fighters)
                m.BeforeDead += AvantMort;
            Fighter.Team.FighterAdded += (t, a) => { a.BeforeDead -= AvantMort; a.BeforeDead += AvantMort; };
        }

        void AvantMort(FightActor mort, FightActor tueur)
        {
            if (!mort.HasState(EtatZombi) || !Fighter.IsAlive())
                return;
            var distance = mort.Position.Point.ManhattanDistanceTo(Fighter.Position.Point);
            var pose = System.Linq.Enumerable.Any(mort.GetBuffs(b => b is Game.Fights.Buffs.TriggerBuff t && t.Spell.Id == Zombidule));
            NLog.LogManager.GetCurrentClassLogger().Info("[Sylargh] mort du Zombi {0} (case {1}) : Sylargh case {2} à {3} PO, Zombidule posé={4}",
                mort, mort.Cell.Id, Fighter.Cell.Id, distance, pose);
            if (!pose)
                mort.CastAutoSpell(new Game.Spells.Spell(Zombidule, 1), mort.Cell);
        }
    }

    /// <summary>Reine des Voleurs : Péché Mignon (Bonbombe au début du tour de chaque joueur).</summary>
    [BrainIdentifier(3726)] public class ReineDesVoleursBrain : BrainPassif { public ReineDesVoleursBrain(AIFighter f) : base(f, 4693) { } }

    /// <summary>
    /// MYSTIC 17/10 : Bonbombe (invoquée par chaque joueur au début de son tour, Péché Mignon) : passif Nova (4690) jamais posé
    /// -> elle n'explosait jamais. Nova tue la bombe au bout de 1 tour (tours 1-3 du combat), 2 tours (4-7) ou 3 tours (8+) ; à sa
    /// mort elle explose (4689) : tue ses alliés (les joueurs) à 10 PO en ligne, fait exploser / désamorce les autres bombes.
    /// </summary>
    [BrainIdentifier(3725)]
    public class BonbombeBrain : Brain
    {
        static readonly NLog.Logger journal = NLog.LogManager.GetCurrentClassLogger();
        const int Nova = 4690;
        int m_tours;

        public BonbombeBrain(AIFighter fighter)
            : base(fighter)
        {
            var tour = fighter.Fight?.TimeLine?.RoundNumber ?? 1;
            // PADGREF 20/10 (journaux du test VPS) : les niveaux 2 et 3 du passif (posés à partir du tour 4) ne faisaient jamais
            // exploser la bombe à la fin de son 2e tour ; seul le niveau 1 marche, et ils ont les mêmes effets dans le client 2.51
            SortPassif.Poser(fighter, Nova, 1);
            // PADGREF 20/10 : journal de diagnostic des explosions (chaque tour de la bombe, sa mort, l'ordre de jeu)
            fighter.Fight.TurnStopped += FinDeTour;
            fighter.Dead += Mort;
            journal.Info("[Bonbombe] {0} apparait (tour de combat {1}, invocateur {2}, case {3}, position dans l'ordre de jeu {4})",
                fighter.Id, tour, (fighter as Stump.Server.WorldServer.Game.Actors.Fight.SummonedFighter)?.Summoner?.Id, fighter.Cell?.Id,
                fighter.Fight.TimeLine.Fighters.IndexOf(fighter));
        }

        bool PassifPose => Fighter.GetBuffs(b => b.Spell != null && b.Spell.Id == Nova).Any();

        void FinDeTour(Stump.Server.WorldServer.Game.Fights.IFight fight, Stump.Server.WorldServer.Game.Actors.Fight.FightActor acteur)
        {
            if (acteur != Fighter)
                return;
            m_tours++;
            if (Fighter.IsAlive())
                journal.Info("[Bonbombe] {0} fin de son tour n°{1} : toujours en vie (passif Nova : {2}, Meche courte : {3}, Desamorcee : {4})",
                    Fighter.Id, m_tours, PassifPose, Fighter.HasState(195), Fighter.HasState(198));
        }

        void Mort(Stump.Server.WorldServer.Game.Actors.Fight.FightActor bombe, Stump.Server.WorldServer.Game.Actors.Fight.FightActor tueur)
        {
            Fighter.Fight.TurnStopped -= FinDeTour;
            Fighter.Dead -= Mort;
            journal.Info("[Bonbombe] {0} meurt / explose apres {1} tour(s) (tuee par {2})", Fighter.Id, m_tours, tueur?.Id);
        }

        public override void Play()
        {
            if (!PassifPose)
                journal.Info("[Bonbombe] {0} : passif Nova ABSENT au debut de son tour", Fighter.Id);
        }
    }

    /// <summary>Reine Nyée : Appel de la Reine.</summary>
    [BrainIdentifier(3996)] public class ReineNyeeBrain : BrainPassif { public ReineNyeeBrain(AIFighter f) : base(f, 5520) { } }

    /// <summary>Oeuf de Trégénaire (invoqué par la Reine Nyée) : Incubation accélérée (éclosion).</summary>
    [BrainIdentifier(3997)] public class OeufTregenaireBrain : BrainPassif { public OeufTregenaireBrain(AIFighter f) : base(f, 5515) { } }

    /// <summary>
    /// MYSTIC 17/10 : Boostache inébranlable (demande utilisateur, écart avec les données 2.51) : ni poussé ni attiré (Inébranlable
    /// 157), ni porté (Lourd 63), ni échangé de place (CanSwitchPos = 0 en base, 35_boostache_inebranlable.sql). États permanents.
    /// </summary>
    [BrainIdentifier(2975)]
    public class BoostacheBrain : Brain
    {
        public BoostacheBrain(AIFighter fighter)
            : base(fighter)
        {
            if (fighter.Fight.State == FightState.Fighting)
                Poser();
            else
                fighter.Fight.FightStarted += OnFightStarted;
        }

        void OnFightStarted(IFight fight)
        {
            fight.FightStarted -= OnFightStarted;
            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                Poser();
        }

        void Poser()
        {
            EtatsBoss.Ajouter(Fighter, Fighter, (int)SpellStatesEnum.INEBRANLABLE_157, 2444);
            EtatsBoss.Ajouter(Fighter, Fighter, (int)SpellStatesEnum.LOURD_63, 2444);
        }
    }
}
