using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 17/10 : gros chantiers des dimensions divines (AUDIT_MECANIQUES.md). Passifs officiels 2.51 jamais lancés ; la
    // mécanique est portée par leurs sous-sorts (déclencheurs, états, zones « case fixe » désormais lues).

    /// <summary>
    /// Chalœil : Jeu du Chalœil (invulnérable 20 tours, lancer de dé : les cases du dé tombé deviennent dangereuses, Dés invoqués,
    /// glyphes) + La Routourne (cycle de bonus tous les 3 tours ; une des 5 variantes au hasard).
    /// </summary>
    [BrainIdentifier(4263)]
    public class ChaloeilBrain : Brain
    {
        public ChaloeilBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, 6231, 1);
            fighter.Fight.TurnStarted += VerifierInvulnerabilite;
            fighter.Fight.FightEnded += Fin;
            // MYSTIC 18/10 : La Routourne (6301) n'est plus posée ici : ses 5 niveaux sont les variantes des 5 monstres du Grand Jeu
            // (voir RoutourneBrain dans PassifsAuditDonjonsBrains.cs) ; avant, le Chalœil (déjà invulnérable) en recevait une au hasard
        }

        /// <summary>MYSTIC 20/10 (décision utilisateur) : le Chalœil ne devient vulnérable qu'au tour 20 : jusque-là, s'il a perdu son
        /// invulnérabilité (désenvoûtement, désactivation…), elle est remise (non désenvoûtable) ; au tour 20 elle est retirée.</summary>
        const int TourDeblocage = 20, JeuDuChaloeil = 6231;

        void VerifierInvulnerabilite(Game.Fights.IFight fight, FightActor joueur)
        {
            if (!Fighter.IsAlive())
                return;
            if (fight.TimeLine.RoundNumber < TourDeblocage)
            {
                EtatsBoss.RetirerDesactivations(Fighter, EtatsBoss.Invulnerable);
                if (!Fighter.HasState(EtatsBoss.Invulnerable))
                    using (fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_TRIGGERED))
                        EtatsBoss.Ajouter(Fighter, Fighter, EtatsBoss.Invulnerable, JeuDuChaloeil);
                return;
            }

            fight.TurnStarted -= VerifierInvulnerabilite;
            using (fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_TRIGGERED))
                foreach (var buff in System.Linq.Enumerable.ToArray(Fighter.GetBuffs(x => x is Game.Fights.Buffs.StateBuff e && e.State.Id == EtatsBoss.Invulnerable)))
                    Fighter.RemoveBuff(buff);
        }

        void Fin(Game.Fights.IFight fight)
        {
            fight.FightEnded -= Fin;
            fight.TurnStarted -= VerifierInvulnerabilite;
        }
    }

    /// <summary>
    /// Ush Galesh rouge : Rouge et Noir (dommages partagés avec l'Ush noir, l'un réduit les dommages à distance, l'autre au corps à
    /// corps, les états s'inversent ; la mort de l'un tue l'autre ; Négativisme) + Matoumorphose (Chachas).
    /// </summary>
    // MYSTIC 20/10 : Ush rouge / noir : voir UshBrains.cs (mécanique codée)

    /// <summary>Ombre : Ex Nihilo.</summary>
    [BrainIdentifier(3564)] public class OmbreBrain : BrainPassif { public OmbreBrain(AIFighter f) : base(f, 4104) { } }

    /// <summary>Silhouette : Détachement.</summary>
    [BrainIdentifier(3565)] public class SilhouetteBrain : BrainPassif { public SilhouetteBrain(AIFighter f) : base(f, 4117) { } }

    // Roi Nidas, Boubourses, Kamastérisk : voir NidasBrains.cs (MYSTIC 20/10)
    [BrainIdentifier(3719)] public class KamasteriskObeseBrain : BrainPassif { public KamasteriskObeseBrain(AIFighter f) : base(f, 4514) { } }

    // Piloztère : voir NidasBrains.cs

    // Ventre de la Baleine : Tabacille, Verminocule, Bacterrib, Virustine, Protozorreur : voir KrobeBrain plus bas
    /// <summary>
    /// MYSTIC 20/10 (Ventre de la Baleine, décision utilisateur) : Restauration élémentaire codée. Le Pataugerme est invulnérable ;
    /// il faut le frapper dans les 4 éléments (Terre, Feu, Eau, Air) entre deux de ses tours, 3 tours de suite : il meurt alors.
    /// Un tour raté remet le compteur à zéro. Le passif 2.51 (4956 → 4959 : Dislocation, Malamibe, états Feu / Eau / Air / Terre
    /// affichés) est gardé ; il ne comptait jamais les 3 tours, le compteur et la mort sont codés ici.
    /// </summary>
    [BrainIdentifier(3827)]
    public class PataugermeBrain : Brain
    {
        const int Restauration = 4956, ToursRequis = 3;
        static readonly System.Collections.Generic.Dictionary<Game.Effects.EffectSchoolEnum, int> Etats =
            new System.Collections.Generic.Dictionary<Game.Effects.EffectSchoolEnum, int>
            {
                { Game.Effects.EffectSchoolEnum.Fire, 479 }, { Game.Effects.EffectSchoolEnum.Water, 480 },
                { Game.Effects.EffectSchoolEnum.Air, 481 }, { Game.Effects.EffectSchoolEnum.Earth, 482 },
            };

        readonly System.Collections.Generic.HashSet<Game.Effects.EffectSchoolEnum> m_touches = new System.Collections.Generic.HashSet<Game.Effects.EffectSchoolEnum>();
        int m_reussis;
        bool m_pose, m_regleDite;

        public PataugermeBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, Restauration, 1);
            fighter.BeforeDamageInflicted += Frappe;
            fighter.Fight.TurnStarted += Tour;
            fighter.Fight.FightEnded += Fin;
            if (fighter.Fight.State == Game.Fights.FightState.Fighting && fighter.Team.Fighters.Contains(fighter))
                Poser();
            else if (fighter.Fight.State == Game.Fights.FightState.Fighting)
                fighter.Team.FighterAdded += Arrivee;
            else
                fighter.Fight.FightStarted += Debut;
        }

        void Debut(Game.Fights.IFight fight)
        {
            fight.FightStarted -= Debut;
            if (Fighter.Team.Fighters.Contains(Fighter))
                Poser();
            else
                Fighter.Team.FighterAdded += Arrivee;
        }

        void Arrivee(Game.Fights.Teams.FightTeam team, FightActor actor)
        {
            if (actor != Fighter)
                return;
            team.FighterAdded -= Arrivee;
            Poser();
        }

        void Poser()
        {
            m_pose = true;
            if (!Fighter.HasState(EtatsBoss.Invulnerable))
                using (Fighter.Fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_TRIGGERED))
                    EtatsBoss.Ajouter(Fighter, Fighter, EtatsBoss.Invulnerable, Restauration);
        }

        void Frappe(FightActor cible, Game.Fights.Damage damage)
        {
            if (!m_pose || !Fighter.IsAlive() || damage.Source == null || damage.Source.Team == Fighter.Team || !Etats.ContainsKey(damage.School))
                return;
            if (!m_touches.Add(damage.School))
                return;
            if (m_touches.Count < Etats.Count)
                return;
            if (m_reussis + 1 >= ToursRequis)
            {
                Annoncer("Pataugerme : 4 éléments " + ToursRequis + " tours de suite, il succombe !");
                Fighter.Die(damage.Source);
                return;
            }
            Annoncer("Pataugerme : 4 éléments touchés ce tour (" + (m_reussis + 1) + "/" + ToursRequis + ")");
        }

        void Tour(Game.Fights.IFight fight, FightActor joueur)
        {
            if (joueur != Fighter || !m_pose || !Fighter.IsAlive())
                return;
            if (!m_regleDite)
            {
                m_regleDite = true;
                Annoncer("Pataugerme : frappez-le dans les 4 éléments (Terre, Feu, Eau, Air) entre deux de ses tours, 3 tours de suite");
            }
            if (m_touches.Count >= Etats.Count)
                m_reussis++;
            else
            {
                if (m_reussis > 0)
                    Annoncer("Pataugerme : tour raté, le compteur repart à 0/" + ToursRequis);
                m_reussis = 0;
            }
            m_touches.Clear();
            using (fight.StartSequence(Stump.DofusProtocol.Enums.SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                // invulnérabilité remise si elle a été retirée ou désactivée (désenvoûtement…)
                EtatsBoss.RetirerDesactivations(Fighter, EtatsBoss.Invulnerable);
                if (!Fighter.HasState(EtatsBoss.Invulnerable))
                    EtatsBoss.Ajouter(Fighter, Fighter, EtatsBoss.Invulnerable, Restauration);
            }
        }

        void Annoncer(string texte)
        {
            foreach (var joueur in Fighter.Fight.GetAllFighters<CharacterFighter>())
                joueur.Character.SendServerMessage(texte, System.Drawing.Color.FromArgb(120, 200, 255));
        }

        void Fin(Game.Fights.IFight fight)
        {
            fight.FightEnded -= Fin;
            fight.TurnStarted -= Tour;
            Fighter.BeforeDamageInflicted -= Frappe;
        }
    }

    /// <summary>
    /// MYSTIC 20/10 (Ventre de la Baleine, décision utilisateur, règles de dofuspourlesnoobs) : les Krobes sont invulnérables ; chacun
    /// meurt quand une condition est remplie plusieurs tours de suite (évaluée au début de son tour, un tour raté remet le compteur à 0).
    /// Il succombe à la fin du tour où le compteur est atteint. La règle et l'avancement sont écrits dans le chat du combat.
    /// Le passif 2.51 (Dislocation, Malamibe, invulnérabilité) est gardé ; il ne comptait jamais les tours.
    /// </summary>
    public abstract class KrobeBrain : Brain
    {
        int m_reussis;
        bool m_regleDite, m_mourir;

        protected KrobeBrain(AIFighter fighter, int passif)
            : base(fighter)
        {
            SortPassif.Poser(fighter, passif, 1);
            fighter.Fight.TurnStarted += Tour;
            fighter.Fight.TurnStopped += FinTour;
            fighter.Fight.FightEnded += Fin;
        }

        protected abstract string Nom { get; }
        protected abstract string Regle { get; }
        protected abstract string Echec { get; }
        protected virtual int ToursRequis => 3;

        /// <summary>Appelé au début du tour du Krobe : la condition du tour écoulé est-elle remplie ?</summary>
        protected abstract bool ConditionRemplie();

        /// <summary>Remise à zéro de ce qui est suivi pendant un tour (après l'évaluation).</summary>
        protected virtual void NouveauTour()
        {
        }

        protected virtual void Terminer()
        {
        }

        void Tour(Game.Fights.IFight fight, FightActor joueur)
        {
            if (joueur != Fighter || !Fighter.IsAlive())
                return;
            if (!m_regleDite)
            {
                m_regleDite = true;
                Annoncer(Nom + " : " + Regle);
            }
            var ok = ConditionRemplie();
            NouveauTour();
            if (ok)
            {
                m_reussis++;
                if (m_reussis >= ToursRequis)
                {
                    m_mourir = true;
                    Annoncer(Nom + " : condition remplie " + ToursRequis + " tours de suite, il succombera à la fin de son tour !");
                }
                else
                    Annoncer(Nom + " : condition remplie (" + m_reussis + "/" + ToursRequis + ")");
            }
            else
            {
                if (m_reussis > 0)
                    Annoncer(Nom + " : " + Echec + ", le compteur repart à 0/" + ToursRequis);
                m_reussis = 0;
            }
        }

        void FinTour(Game.Fights.IFight fight, FightActor joueur)
        {
            if (joueur != Fighter || !m_mourir || !Fighter.IsAlive())
                return;
            m_mourir = false;
            Fighter.Die();
        }

        protected void Annoncer(string texte)
        {
            foreach (var joueur in Fighter.Fight.GetAllFighters<CharacterFighter>())
                joueur.Character.SendServerMessage(texte, System.Drawing.Color.FromArgb(120, 200, 255));
        }

        protected System.Collections.Generic.IEnumerable<FightActor> Ennemis =>
            Fighter.OpposedTeam.Fighters.Where(x => x.IsAlive() && x.Cell != null);

        void Fin(Game.Fights.IFight fight)
        {
            fight.FightEnded -= Fin;
            fight.TurnStarted -= Tour;
            fight.TurnStopped -= FinTour;
            Terminer();
        }
    }

    /// <summary>Tabacille : Restauration curative : ne soigner personne pendant 3 tours (le vol de vie est autorisé).</summary>
    [BrainIdentifier(3823)]
    public class TabacilleBrain : KrobeBrain
    {
        bool m_soin;

        public TabacilleBrain(AIFighter f) : base(f, 4936) { FightActor.SoinApplique += Soin; }

        protected override string Nom => "Tabacille";
        protected override string Regle => "ne soignez personne pendant 3 tours de suite (le vol de vie est autorisé)";
        protected override string Echec => "un soin a été lancé";
        protected override bool ConditionRemplie() => !m_soin;
        protected override void NouveauTour() => m_soin = false;
        protected override void Terminer() => FightActor.SoinApplique -= Soin;

        void Soin(FightActor cible, Game.Fights.Damage soin)
        {
            // vol de vie : la source du soin est la victime (camp du Tabacille) : non compté
            if (cible.Fight == Fighter.Fight && soin.Source != null && soin.Source.Team != Fighter.Team)
                m_soin = true;
        }
    }

    /// <summary>Verminocule : Restauration de proximité : un personnage à son contact au début de son tour, 3 tours (pas une invocation).</summary>
    [BrainIdentifier(3824)]
    public class VerminoculeBrain : KrobeBrain
    {
        public VerminoculeBrain(AIFighter f) : base(f, 4941) { }

        protected override string Nom => "Verminocule";
        protected override string Regle => "ayez un personnage à son contact au début de son tour, 3 tours de suite (les invocations ne comptent pas)";
        protected override string Echec => "personne à son contact";
        protected override bool ConditionRemplie() =>
            Ennemis.OfType<CharacterFighter>().Any(x => x.Position.Point.ManhattanDistanceTo(Fighter.Position.Point) == 1);
    }

    /// <summary>Bacterrib : Restauration éloignée : aucun adversaire à 5 cases ou moins au début de son tour, 3 tours.</summary>
    [BrainIdentifier(3825)]
    public class BacterribBrain : KrobeBrain
    {
        public BacterribBrain(AIFighter f) : base(f, 4946) { }

        protected override string Nom => "Bacterrib";
        protected override string Regle => "aucun adversaire à 5 cases ou moins de lui au début de son tour, 3 tours de suite";
        protected override string Echec => "un adversaire était à 5 cases ou moins";
        protected override bool ConditionRemplie() =>
            !Ennemis.Any(x => x.Position.Point.ManhattanDistanceTo(Fighter.Position.Point) <= 5);
    }

    /// <summary>Virustine : Restauration mobile : ni déplacée ni PM retirés pendant 3 tours (déplacements par ses alliés compris).</summary>
    [BrainIdentifier(3826)]
    public class VirustineBrain : KrobeBrain
    {
        bool m_touchee;

        public VirustineBrain(AIFighter f)
            : base(f, 4951)
        {
            f.PositionChanged += Deplacee;
            f.FightPointsVariation += Points;
        }

        protected override string Nom => "Virustine";
        protected override string Regle => "ne la déplacez pas et ne lui retirez pas de PM pendant 3 tours de suite (les déplacements par ses alliés comptent aussi)";
        protected override string Echec => "elle a été déplacée ou a perdu des PM";
        protected override bool ConditionRemplie() => !m_touchee;
        protected override void NouveauTour() => m_touchee = false;

        protected override void Terminer()
        {
            Fighter.PositionChanged -= Deplacee;
            Fighter.FightPointsVariation -= Points;
        }

        void Deplacee(Game.Actors.ContextActor acteur, Game.Maps.Cells.ObjectPosition position)
        {
            // sa propre marche pendant son tour ne compte pas
            if (Fighter.Fight.State == Game.Fights.FightState.Fighting && Fighter.Fight.FighterPlaying != Fighter)
                m_touchee = true;
        }

        void Points(FightActor acteur, Stump.DofusProtocol.Enums.ActionsEnum action, FightActor source, FightActor cible, short delta)
        {
            if (action == Stump.DofusProtocol.Enums.ActionsEnum.ACTION_CHARACTER_MOVEMENT_POINTS_LOST ||
                action == Stump.DofusProtocol.Enums.ActionsEnum.ACTION_CHARACTER_MOVEMENT_POINTS_STEAL)
                m_touchee = true;
        }
    }

    /// <summary>
    /// Protozorreur : Restauration infectieuse : en état Infection (glyphe violet) au début de 6 de ses tours de suite, il succombe.
    /// Attraction cellulaire et Protoglyphes : voir Protozorreur dans CastSpell.cs ; jamais porté (FightActor.CarryActor).
    /// </summary>
    [BrainIdentifier(3828)]
    public class ProtozorreurBrain : KrobeBrain
    {
        public ProtozorreurBrain(AIFighter f) : base(f, 4972) { }

        protected override string Nom => "Protozorreur";
        protected override string Regle => "frappez-le en étant aligné pour l'attirer (première frappe du tour) dans un glyphe violet : il passe en Infection. Infecté au début de 6 de ses tours de suite, il succombe";
        protected override string Echec => "il n'était plus en Infection";
        protected override int ToursRequis => 6;
        protected override bool ConditionRemplie() => Fighter.HasState(215);
    }
}
