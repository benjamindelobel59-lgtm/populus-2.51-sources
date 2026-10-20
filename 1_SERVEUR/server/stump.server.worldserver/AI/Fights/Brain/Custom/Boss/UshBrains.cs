using System.Linq;
using System.Runtime.CompilerServices;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 20/10 : Plateau de Ush (dofuspourlesnoobs), mécanique codée ici (la chaîne de sorts 2.51 « Rouge et Noir » 6264 et
    /// « Matoumorphose » 6604 ne fonctionnait pas : pas d'alternance distance / contact, Matoumorphose sur un seul Ush).
    ///  - Ush rouge (4264) : état Rouge + Intaclable ; Ush noir (4265) : état Noir + Indéplaçable ;
    ///  - Rouge : ne prend des dégâts qu'au corps à corps (attaquant sur une case adjacente) ; Noir : qu'à distance ;
    ///    un coup du mauvais type ne fait aucun dégât ;
    ///  - chaque coup qui blesse fait passer l'Ush touché de Rouge à Noir (ou l'inverse) ;
    ///  - dégâts partagés : un coup est divisé par deux, une moitié sur chaque Ush ;
    ///  - la mort de l'un tue l'autre ;
    ///  - Matoumorphose (tous les monstres du combat) : au début de son tour, chaque monstre la pose sur le monstre le plus proche
    ///    (sur lui-même s'il est seul) ; un monstre en Matoumorphose qui meurt la transmet au monstre le plus proche.
    /// </summary>
    public class UshControleur
    {
        public const int UshRouge = 4264, UshNoir = 4265;
        const int EtatRouge = 324, EtatNoir = 325, Intaclable = 96, Indeplacable = 97, EtatMatoumorphose = 377;
        const int SortRougeEtNoir = 6238, SortMatoumorphose = 6606;

        static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static readonly ConditionalWeakTable<IFight, UshControleur> s_controleurs = new ConditionalWeakTable<IFight, UshControleur>();

        public static void Inscrire(AIFighter ush) => s_controleurs.GetValue(ush.Fight, f => new UshControleur(f));

        readonly IFight m_fight;
        FightActor m_rouge, m_noir;

        UshControleur(IFight fight)
        {
            m_fight = fight;
            if (fight.State == FightState.Fighting)
                Demarrer(fight);
            else
                fight.FightStarted += Demarrer;
        }

        void Demarrer(IFight fight)
        {
            fight.FightStarted -= Demarrer;
            m_rouge = Ush(UshRouge);
            m_noir = Ush(UshNoir);

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                foreach (var ush in new[] { m_rouge, m_noir }.Where(x => x != null))
                {
                    ush.InvulnerabiliteSpeciale = (degats, contact) => Bloque(ush, degats, contact);
                    ush.AvantPerteVie += Partager;
                    ush.DamageInflicted += Basculer;
                    ush.Dead += MortUsh;
                }
                if (m_rouge != null)
                {
                    EtatsBoss.Ajouter(m_rouge, m_rouge, EtatRouge, SortRougeEtNoir);
                    EtatsBoss.Ajouter(m_rouge, m_rouge, Intaclable, SortRougeEtNoir);
                }
                if (m_noir != null)
                {
                    EtatsBoss.Ajouter(m_noir, m_noir, EtatNoir, SortRougeEtNoir);
                    EtatsBoss.Ajouter(m_noir, m_noir, Indeplacable, SortRougeEtNoir);
                }
            }

            foreach (var monstre in fight.GetAllFighters(x => x is MonsterFighter))
                monstre.BeforeDead += TransmettreMatoumorphose;
            var equipe = (m_rouge ?? m_noir)?.Team;
            if (equipe != null)
                equipe.FighterAdded += (t, f) => { if (f is MonsterFighter) { f.BeforeDead -= TransmettreMatoumorphose; f.BeforeDead += TransmettreMatoumorphose; } };

            fight.TurnStarted += TourMatoumorphose;
            fight.FightEnded += Fin;
            logger.Info("[Ush] combat {0} : mécanique active (rouge : {1}, noir : {2})", fight.Id, m_rouge, m_noir);
        }

        FightActor Ush(int id) =>
            m_fight.GetAllFighters(x => x is MonsterFighter m && m.Monster.Template.Id == id).FirstOrDefault();

        FightActor Partenaire(FightActor ush) => ush == m_rouge ? m_noir : m_rouge;

        static bool Ennemi(FightActor ush, Damage degats) => degats?.Source != null && degats.Source.Team != ush.Team;

        // ── Rouge : contact seulement ; Noir : distance seulement
        bool Bloque(FightActor ush, Damage degats, bool contact)
        {
            if (degats is DommagePartageUsh || !Ennemi(ush, degats))
                return false;
            var bloque = ush.HasState(EtatRouge) ? !contact : ush.HasState(EtatNoir) && contact;
            if (bloque)
                logger.Info("[Ush] coup {0} de {1} bloqué sur {2} (état {3})", contact ? "au contact" : "à distance", degats.Source, ush,
                    ush.HasState(EtatRouge) ? "Rouge" : "Noir");
            return bloque;
        }

        // ── dégâts partagés : la moitié passe sur l'autre Ush
        void Partager(FightActor ush, Damage degats)
        {
            if (degats is DommagePartageUsh || degats.Amount < 2 || !Ennemi(ush, degats))
                return;
            var autre = Partenaire(ush);
            if (autre == null || !autre.IsAlive())
                return;
            var moitie = degats.Amount / 2;
            degats.Amount -= moitie;
            autre.InflictDamage(new DommagePartageUsh(moitie)
            {
                Source = degats.Source,
                School = degats.School,
                IgnoreDamageBoost = true,
                IgnoreDamageReduction = true,
            });
        }

        // ── chaque coup qui blesse inverse l'état de l'Ush touché
        void Basculer(FightActor ush, Damage degats)
        {
            if (degats is DommagePartageUsh || degats.Amount <= 0 || !Ennemi(ush, degats) || !ush.IsAlive())
                return;
            var devientNoir = ush.HasState(EtatRouge);
            using (m_fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                Retirer(ush, devientNoir ? EtatRouge : EtatNoir);
                EtatsBoss.Ajouter(ush, ush, devientNoir ? EtatNoir : EtatRouge, SortRougeEtNoir);
            }
            logger.Info("[Ush] {0} touché ({1} PV) : passe en {2}", ush, degats.Amount, devientNoir ? "Noir (à distance)" : "Rouge (au contact)");
        }

        static void Retirer(FightActor cible, int etat)
        {
            foreach (var buff in cible.GetBuffs(x => x is StateBuff s && s.State.Id == etat).ToArray())
                cible.RemoveBuff(buff);
        }

        // ── la mort de l'un tue l'autre
        void MortUsh(FightActor ush, FightActor tueur)
        {
            ush.Dead -= MortUsh;
            var autre = Partenaire(ush);
            if (autre == null)
                return;
            autre.Dead -= MortUsh;
            m_fight.Map.Area.AddMessage(() =>
            {
                if (m_fight.State == FightState.Fighting && autre.IsAlive())
                    using (m_fight.StartSequence(SequenceTypeEnum.SEQUENCE_CHARACTER_DEATH))
                        autre.Die(tueur);
            });
        }

        // ── Matoumorphose
        FightActor MonstreLePlusProche(FightActor de, FightActor sauf) =>
            m_fight.GetAllFighters(x => x != sauf && x.IsAlive() && x is MonsterFighter && x.Team == de.Team)
                .OrderBy(x => x.Position.Point.ManhattanDistanceTo(de.Position.Point)).FirstOrDefault();

        void TourMatoumorphose(IFight fight, FightActor joueur)
        {
            if (fight.State != FightState.Fighting || !(joueur is MonsterFighter) || !joueur.IsAlive())
                return;
            var cible = MonstreLePlusProche(joueur, joueur) ?? joueur;
            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                joueur.CastAutoSpell(new Spell(SortMatoumorphose, 1), cible.Cell);
        }

        void TransmettreMatoumorphose(FightActor mort, FightActor tueur)
        {
            if (!mort.HasState(EtatMatoumorphose))
                return;
            var cible = MonstreLePlusProche(mort, mort);
            if (cible == null)
                return;
            m_fight.Map.Area.AddMessage(() =>
            {
                if (m_fight.State == FightState.Fighting && cible.IsAlive())
                    using (m_fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                        cible.CastAutoSpell(new Spell(SortMatoumorphose, 1), cible.Cell);
            });
        }

        void Fin(IFight fight)
        {
            fight.FightEnded -= Fin;
            fight.TurnStarted -= TourMatoumorphose;
        }
    }

    /// <summary>Part des dégâts d'un Ush reportée sur l'autre (ni bloquée, ni repartagée, ne fait pas basculer l'état).</summary>
    public class DommagePartageUsh : Damage
    {
        public DommagePartageUsh(int amount) : base(amount)
        {
        }
    }

    /// <summary>Ush Galesh rouge : sorts par l'IA normale (sans « Rouge et Noir », géré par le contrôleur).</summary>
    [BrainIdentifier(UshControleur.UshRouge)]
    public class UshRougeBrain : BrainPhorreur
    {
        public UshRougeBrain(AIFighter f) : base(f)
        {
            f.Spells.Remove(6238);
            UshControleur.Inscrire(f);
        }
    }

    /// <summary>MYSTIC 20/10 : Chasquatch : Ailurophilie 6390 (soin autour de lui quand il est frappé) lancé sur lui ; le reste par l'IA normale.</summary>
    [BrainIdentifier(4296)]
    public class ChasquatchBrain : BrainPhorreur
    {
        public ChasquatchBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 6390 };
    }

    /// <summary>MYSTIC 20/10 : Chacrebleu : Chamboulement 6395 (en ligne à 3 PO : attire la cible de 3 cases et la frappe) lancé sur
    /// l'ennemi le plus proche quand il n'est pas déjà au contact (avant : jamais lancé par l'IA normale) ; le reste par l'IA normale.</summary>
    [BrainIdentifier(4297)]
    public class ChacrebleuBrain : BrainPhorreur
    {
        public ChacrebleuBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurEnnemi => new[] { 6395 };
    }

    /// <summary>MYSTIC 20/10 : Chakichan : Chakcélération 6401 (+1 PM par coup reçu) lancé sur lui ; le reste par l'IA normale.</summary>
    [BrainIdentifier(4298)]
    public class ChakichanBrain : BrainPhorreur
    {
        public ChakichanBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 6401 };
    }

    /// <summary>MYSTIC 20/10 : Chargus : IA commune (Duplichation par l'IA normale).</summary>
    [BrainIdentifier(4299)]
    public class ChargusBrain : BrainPhorreur
    {
        public ChargusBrain(AIFighter f) : base(f) { }
    }

    /// <summary>MYSTIC 20/10 : Chakaroze : IA commune.</summary>
    [BrainIdentifier(4300)]
    public class ChakarozeBrain : BrainPhorreur
    {
        public ChakarozeBrain(AIFighter f) : base(f) { }
    }

    /// <summary>Ush Galesh noir.</summary>
    [BrainIdentifier(UshControleur.UshNoir)]
    public class UshNoirBrain : BrainPhorreur
    {
        public UshNoirBrain(AIFighter f) : base(f)
        {
            UshControleur.Inscrire(f);
        }
    }
}
