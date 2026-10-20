using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 17/10 : Oeil de Vortex (Dimension Xelorium), mécanique 2.51 codée ici (les passifs officiels du client reposent sur
    /// trop d'effets et de ciblages mal gérés par le serveur). Choix validés par l'utilisateur :
    ///  - l'Auroraire avance d'une heure au début du tour de chaque personnage (et quand un personnage marche sur la glyphe d'un monstre) ;
    ///  - un monstre tué est ressuscité au tour du Vortex (50 % PV, état Zombi), sans bonus ;
    ///  - tué une 2e fois à une même heure : « corrompu », le Vortex prend le bonus de cette heure (cumulable) et le monstre
    ///    revient à côté du Vortex, invulnérable, inébranlable, sans jouer (ni attaque ni déplacement) ;
    ///  - un message dans le chat (et l'étoile « Même heure ») quand un monstre peut être éliminé ;
    ///  - glyphe posée par chaque monstre au début de son tour : y marcher = échange de place, l'heure avance, bonus du monstre ;
    ///  - vagues : tous les 5 tours (vague 2 au tour 7) ou dès que tout est corrompu ;
    ///  - fin : un tour « à vide » (Vortex invulnérable, joueurs remis à leur case de départ), puis il devient vulnérable,
    ///    retrouve ses PM et se téléporte à l'Auroraire (Heurage) tous les 3 tours.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.VORTEX)]
    public class VortexBrain : Brain
    {
        public VortexBrain(AIFighter fighter)
            : base(fighter)
        {
            Controleur = new VortexControleur(fighter);
        }

        public VortexControleur Controleur { get; }

        public override void Play()
        {
            if (!Controleur.TourDuVortex())
                return;

            base.Play();
        }
    }

    /// <summary>Auroraire du Vortex : ne joue jamais, c'est l'horloge (déplacée par le contrôleur).</summary>
    [BrainIdentifier((int)MonsterIdEnum.AURORAIRE_3833)]
    public class AuroraireVortexBrain : Brain
    {
        public AuroraireVortexBrain(AIFighter fighter)
            : base(fighter)
        {
        }

        public override void Play()
        {
        }
    }

    public class VortexControleur
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const int AuroraireMonsterId = 3833;
        public const int EnTempsEtEnHeureSpellId = 5061;   // frappe dans les lignes de l'Auroraire
        public const int EnTempsEtEnHeureLanceurId = 5062;
        public const int HeurageSpellId = 5066;
        public const int VortexiphanSpellId = 5003;        // sort « porteur » des buffs d'heure (icône)
        public const int GlypheSpellId = 5012;
        public const int GlypheEffetSpellId = 5011;

        public const int EtatInvulnerable = 56, EtatIndeplacable = 97, EtatMarginal = 236, EtatZombi = 74, EtatMemeHeure = 234,
            EtatInebranlable = 157, EtatIntaclable = 96, EtatPremiereHeure = 221;

        // case de l'Auroraire pour chaque heure (données du client, sort « Décalage horaire » 4996)
        static readonly Dictionary<int, short> CasesHeures = new Dictionary<int, short>
        {
            { 1, 215 }, { 2, 218 }, { 3, 263 }, { 4, 334 }, { 5, 418 }, { 6, 487 },
            { 7, 526 }, { 8, 523 }, { 9, 479 }, { 10, 407 }, { 11, 323 }, { 12, 255 }
        };

        static readonly string[] NomsHeures = { "", "1re", "2e", "3e", "4e", "5e", "6e", "7e", "8e", "9e", "10e", "11e", "12e" };

        enum Phase { Vagues, FinaleAVide, Vulnerable }

        readonly AIFighter m_vortex;
        IFight m_fight;
        AIFighter m_auroraire;
        int m_heure = 12;
        Phase m_phase = Phase.Vagues;
        int m_toursDepuisHeurage = 3;
        StatBuff m_moinsCentPM;
        readonly Dictionary<FightActor, HashSet<int>> m_heuresDeMort = new Dictionary<FightActor, HashSet<int>>();
        readonly HashSet<FightActor> m_corrompus = new HashSet<FightActor>();
        readonly List<FightActor> m_aRessusciter = new List<FightActor>();
        readonly Dictionary<FightActor, GlypheVortex> m_glyphes = new Dictionary<FightActor, GlypheVortex>();
        readonly Dictionary<CharacterFighter, Cell> m_casesDepart = new Dictionary<CharacterFighter, Cell>();
        // MYSTIC 20/10 : case de départ de chaque monstre (début du combat, ou arrivée en vague) : il y ressuscite
        readonly Dictionary<FightActor, Cell> m_casesBase = new Dictionary<FightActor, Cell>();

        public VortexControleur(AIFighter vortex)
        {
            m_vortex = vortex;
            m_fight = vortex.Fight;
            m_fight.FightStarted += OnFightStarted;
            m_fight.PlacementRandomise += OnPlacementRandomise;
        }

        /// <summary>Le Vortex commence toujours sur la même case (la plus à gauche de la rangée des monstres, comme l'officiel).</summary>
        public const short CaseDepartVortex = 268;

        void OnPlacementRandomise(IFight fight)
        {
            fight.PlacementRandomise -= OnPlacementRandomise;
            var cell = fight.Map.Cells[CaseDepartVortex];
            if (m_vortex.Cell == cell)
                return;

            var occupant = fight.GetOneFighter(cell);
            if (occupant != null && occupant != m_vortex)
                occupant.Position.Cell = m_vortex.Cell;
            m_vortex.Position.Cell = cell;
        }

        #region Début du combat

        void OnFightStarted(IFight fight)
        {
            fight.FightStarted -= OnFightStarted;
            m_fight = fight;
            fight.DefinirToursDeVagues(new[] { 7, 12, 17, 22, 27, 32, 37, 42 });

            // ses sorts automatiques sont lancés par le contrôleur, pas par l'IA
            m_vortex.Spells.Remove(EnTempsEtEnHeureLanceurId);
            m_vortex.Spells.Remove(HeurageSpellId);

            foreach (var joueur in Joueurs())
                m_casesDepart[joueur] = joueur.Cell;

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                AjouterEtat(m_vortex, EtatInvulnerable);
                AjouterEtat(m_vortex, EtatIndeplacable);
                AjouterEtat(m_vortex, EtatMarginal);
                m_moinsCentPM = AjouterStat(m_vortex, m_vortex, -100, PlayerFields.MP, -1);
                InvoquerAuroraire();
            }

            foreach (var monstre in Monstres())
                Surveiller(monstre);

            m_vortex.Team.FighterAdded += OnFighterAdded;
            m_vortex.Dead += OnVortexDead;
            fight.TurnStarted += OnTurnStarted;
            fight.FightEnded += OnFightEnded;

            Annoncer("Œil de Vortex : chaque monstre tué revient à la vie. Tuez-le une 2e fois à la même heure de l'Auroraire pour l'éliminer définitivement.");
        }

        void InvoquerAuroraire()
        {
            var grade = MonsterManager.Instance.GetMonsterGrade(AuroraireMonsterId, 1);
            if (grade == null)
                return;

            var cell = m_fight.Map.Cells[CasesHeures[12]];
            if (!m_fight.IsCellFree(cell))
                cell = CaseLibreAutour(cell) ?? cell;

            var auroraire = new SummonedMonster(m_fight.GetNextContextualId(), m_vortex.Team, m_vortex, grade, cell);
            ActionsHandler.SendGameActionFightSummonMessage(m_fight.Clients, auroraire);
            m_vortex.AddSummon(auroraire);
            m_vortex.Team.AddFighter(auroraire);
            m_auroraire = auroraire;

            AjouterEtat(auroraire, EtatInvulnerable);
            AjouterEtat(auroraire, EtatIndeplacable);
            AjouterEtat(auroraire, EtatPremiereHeure + 11);
        }

        #endregion

        #region Tours

        void OnTurnStarted(IFight fight, FightActor joueur)
        {
            if (fight.State == FightState.Ended || joueur == null || !joueur.IsAlive())
                return;

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                if (joueur is CharacterFighter && joueur.Team != m_vortex.Team)
                    AvancerHeure();
                else if (EstMonstreActif(joueur))
                    PoserGlyphe(joueur);

                VerifierVaguesEtFin();
            }
        }

        /// <summary>Tour du Vortex ; false = il ne joue pas l'IA ce tour-ci.</summary>
        public bool TourDuVortex()
        {
            if (m_fight == null || m_fight.State != FightState.Fighting)
                return true;

            using (m_fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
            {
                Ressusciter();
                VerifierVaguesEtFin();

                switch (m_phase)
                {
                    case Phase.Vagues:
                        EnTempsEtEnHeure();
                        return true;

                    case Phase.FinaleAVide:
                        // tour « à vide » : il reste invulnérable, ne joue pas, les personnages reviennent à leur case de départ
                        RemettreJoueursAuDepart();
                        Annoncer("Le Vortex est seul : il deviendra vulnérable à son prochain tour !");
                        m_phase = Phase.Vulnerable;
                        m_toursDepuisHeurage = 3;
                        return false;

                    default:
                        if (m_vortex.HasState(EtatInvulnerable))
                            Reveiller();

                        if (++m_toursDepuisHeurage >= 3)
                        {
                            Heurage();
                            m_toursDepuisHeurage = 0;
                        }

                        EnTempsEtEnHeure();
                        return true;
                }
            }
        }

        void EnTempsEtEnHeure()
        {
            if (m_auroraire != null && m_auroraire.IsAlive())
                m_vortex.CastAutoSpell(new Spell(EnTempsEtEnHeureSpellId, 1), m_auroraire.Cell);
        }

        void Reveiller()
        {
            // les monstres éliminés définitivement (immobiles, inébranlables) meurent quand le Vortex devient vulnérable
            foreach (var corrompu in m_corrompus.Where(x => x.IsAlive()).ToArray())
                corrompu.Die(m_vortex);
            m_aRessusciter.Clear();

            RetirerEtat(m_vortex, EtatInvulnerable);
            RetirerEtat(m_vortex, EtatIndeplacable);
            RetirerEtat(m_vortex, EtatMarginal);
            if (m_moinsCentPM != null && m_vortex.GetBuffs().Contains(m_moinsCentPM))
                m_vortex.RemoveBuff(m_moinsCentPM);
            m_moinsCentPM = null;
            Annoncer("Le Vortex est vulnérable !");
        }

        void Heurage()
        {
            if (m_auroraire == null)
                return;

            var cell = CaseLibreAutour(m_auroraire.Cell);
            if (cell == null)
                return;

            m_vortex.Position.Cell = cell;
            ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(m_fight.Clients, m_vortex, m_vortex, cell);
        }

        void RemettreJoueursAuDepart()
        {
            foreach (var paire in m_casesDepart.Where(x => x.Key.IsAlive()))
            {
                var joueur = paire.Key;
                var cell = paire.Value;
                if (joueur.Cell == cell)
                    continue;
                if (!m_fight.IsCellFree(cell))
                    cell = CaseLibreAutour(cell);
                if (cell == null)
                    continue;

                joueur.Position.Cell = cell;
                ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(m_fight.Clients, m_vortex, joueur, cell);
            }
        }

        #endregion

        #region Horloge

        void AvancerHeure()
        {
            var ancienne = m_heure;
            m_heure = m_heure % 12 + 1;

            if (m_auroraire != null && m_auroraire.IsAlive())
            {
                RetirerEtat(m_auroraire, EtatPremiereHeure + ancienne - 1);
                AjouterEtat(m_auroraire, EtatPremiereHeure + m_heure - 1);

                var cible = m_fight.Map.Cells[CasesHeures[m_heure]];
                var occupant = m_fight.GetOneFighter(cible);
                if (occupant == null)
                {
                    m_auroraire.Position.Cell = cible;
                    ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(m_fight.Clients, m_auroraire, m_auroraire, cible);
                }
                else if (occupant != m_auroraire)
                    m_auroraire.ExchangePositions(occupant);
            }

            // monstres à éliminer à cette heure : étoile + message
            foreach (var monstre in Monstres().Where(x => x.IsAlive() && !m_corrompus.Contains(x)))
            {
                var aSonHeure = m_heuresDeMort.TryGetValue(monstre, out var heures) && heures.Contains(m_heure);
                if (aSonHeure)
                {
                    if (!monstre.HasState(EtatMemeHeure))
                        AjouterEtat(monstre, EtatMemeHeure);
                    Annoncer(string.Format("{0} peut être éliminé(e) définitivement ({1} heure).", Nom(monstre), NomsHeures[m_heure]));
                }
                else
                    RetirerEtat(monstre, EtatMemeHeure);
            }
        }

        #endregion

        #region Morts et résurrections

        void OnFighterAdded(FightTeam team, FightActor fighter)
        {
            if (fighter is MonsterFighter && fighter != m_vortex)
                Surveiller(fighter);
        }

        void Surveiller(FightActor monstre)
        {
            if (!m_casesBase.ContainsKey(monstre) && monstre.Cell != null)
                m_casesBase[monstre] = monstre.Cell;
            monstre.Dead -= OnMonstreDead;
            monstre.Dead += OnMonstreDead;
        }

        void OnMonstreDead(FightActor monstre, FightActor tueur)
        {
            if (m_fight.State == FightState.Ended)
                return;

            if (m_glyphes.TryGetValue(monstre, out var glyphe))
            {
                m_glyphes.Remove(monstre);
                if (m_fight.GetTriggers().Contains(glyphe))
                    m_fight.RemoveTrigger(glyphe);
            }

            if (m_corrompus.Contains(monstre))
                return;

            if (!m_heuresDeMort.TryGetValue(monstre, out var heures))
                m_heuresDeMort[monstre] = heures = new HashSet<int>();

            if (heures.Contains(m_heure))
            {
                m_corrompus.Add(monstre);
                Annoncer(string.Format("{0} est éliminé(e) définitivement.", Nom(monstre)));

                // le Vortex prend le bonus de l'heure à chaque élimination définitive (cumulable, même heure comprise)
                using (m_fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                    BonusHeure(m_vortex, m_heure);
            }
            else
                heures.Add(m_heure);

            if (!m_aRessusciter.Contains(monstre))
                m_aRessusciter.Add(monstre);
        }

        void Ressusciter()
        {
            foreach (var monstre in m_aRessusciter.ToArray())
            {
                if (monstre.IsAlive())
                {
                    m_aRessusciter.Remove(monstre);
                    continue;
                }

                // corrompu : ramené à côté du Vortex ; sinon (MYSTIC 20/10) à sa case de départ, ou juste à côté si elle est prise
                // (avant : case de placement au hasard)
                var cell = m_corrompus.Contains(monstre) ? CaseLibreAutour(m_vortex.Cell) : CaseDeBase(monstre);
                if (cell == null)
                    continue; // aucune case libre : il reviendra au prochain tour du Vortex (avant : perdu)

                m_aRessusciter.Remove(monstre);

                monstre.Revive(50, m_vortex);
                monstre.Position.Cell = cell;
                ActionsHandler.SendGameActionFightReviveMessage(m_fight.Clients, m_vortex, monstre);
                ContextHandler.SendGameFightTurnListMessage(m_fight.Clients, m_fight);

                if (m_corrompus.Contains(monstre))
                {
                    // il revient à côté du Vortex, invulnérable, inébranlable, et ne joue plus (ni attaque ni déplacement)
                    AjouterEtat(monstre, EtatInvulnerable);
                    AjouterEtat(monstre, EtatInebranlable);
                    AjouterSkipTurn(monstre);
                    continue;
                }

                // 1re mort : aucun bonus pour le monstre (demande utilisateur) ; seul le Vortex prend le bonus, à l'élimination définitive
                AjouterEtat(monstre, EtatZombi);
                // MYSTIC 20/10 (décision utilisateur) : ressuscité en Zombi, il n'est PAS invulnérable (seules les vagues le sont)
            }
        }

        void OnVortexDead(FightActor vortex, FightActor tueur)
        {
            // le combat finit avec lui : monstres corrompus et Auroraire tombent aussi
            foreach (var reste in m_vortex.Team.Fighters.Where(x => x != m_vortex && x.IsAlive()).ToArray())
                reste.Die(tueur);
        }

        void OnFightEnded(IFight fight)
        {
            fight.FightEnded -= OnFightEnded;
            fight.TurnStarted -= OnTurnStarted;
            m_vortex.Team.FighterAdded -= OnFighterAdded;
            m_vortex.Dead -= OnVortexDead;
            foreach (var monstre in m_heuresDeMort.Keys.Concat(Monstres()).Distinct())
                monstre.Dead -= OnMonstreDead;
        }

        #endregion

        #region Vagues et fin

        void VerifierVaguesEtFin()
        {
            if (m_phase != Phase.Vagues)
                return;

            var monstres = Monstres().ToList();
            if (monstres.Count == 0 || !monstres.All(x => m_corrompus.Contains(x)))
                return;

            // tout ce qui est présent est corrompu : vague suivante tout de suite, ou fin des vagues
            if (m_fight.ResteDesVagues())
            {
                m_fight.LancerVagueSuivante();
                Annoncer("Tous les monstres sont éliminés : la vague suivante arrive !");
            }
            else
            {
                m_phase = Phase.FinaleAVide;
                Annoncer("Toutes les vagues sont éliminées. Le Vortex reste invulnérable encore un tour...");
            }
        }

        #endregion

        #region Glyphes des monstres

        void PoserGlyphe(FightActor monstre)
        {
            if (m_glyphes.TryGetValue(monstre, out var ancienne) && m_fight.GetTriggers().Contains(ancienne))
                m_fight.RemoveTrigger(ancienne);

            var dice = new EffectDice(EffectsEnum.Effect_Glyph, 0, GlypheEffetSpellId, 1) { Duration = -1 };
            var glyphe = new GlypheVortex((short)m_fight.PopNextTriggerId(), monstre, new Spell(GlypheSpellId, 1), dice, monstre.Cell, OnGlypheDeclenchee);
            m_glyphes[monstre] = glyphe;
            m_fight.AddTriger(glyphe);
        }

        void OnGlypheDeclenchee(GlypheVortex glyphe, FightActor joueur)
        {
            var monstre = glyphe.Caster;
            m_glyphes.Remove(monstre);
            if (m_fight.GetTriggers().Contains(glyphe))
                m_fight.RemoveTrigger(glyphe);

            if (!monstre.IsAlive() || !joueur.IsAlive())
                return;

            monstre.ExchangePositions(joueur);
            BonusGlyphe(monstre, monstre, joueur);
            BonusGlyphe(monstre, joueur, joueur);
            AvancerHeure();
        }

        void BonusGlyphe(FightActor monstre, FightActor cible, FightActor joueur)
        {
            switch ((monstre as MonsterFighter)?.Monster.Template.Id)
            {
                case 3834: Appliquer(monstre, cible, (EffectsEnum)128, 1, 0, 1); break;            // Ikargn : +1 PM
                case 3836: Appliquer(monstre, cible, (EffectsEnum)1109, 10, 0, 0); break;          // Méjaire : soin 10 %
                case 3837: Appliquer(monstre, cible, (EffectsEnum)138, 200, 0, 1); break;          // Harpille : +200 puissance
                case 3838: Appliquer(monstre, cible, (EffectsEnum)1163, 50, 0, 1, "D"); break;     // Buboxor : dégâts subis x50 %
                case 3839: Appliquer(monstre, cible, (EffectsEnum)414, 200, 0, 1); break;          // Brabuzar : +200 dommages de poussée
            }
        }

        #endregion

        #region Bonus des heures

        void BonusHeure(FightActor cible, int heure)
        {
            switch (heure)
            {
                case 1:
                    Appliquer(m_vortex, cible, (EffectsEnum)115, 10, 0, -1);
                    AjouterStat(cible, m_vortex, 200, PlayerFields.CriticalDamageBonus, -1);
                    break;
                case 2: AjouterEtat(cible, EtatIntaclable); break;
                case 3: Appliquer(m_vortex, cible, (EffectsEnum)126, 400, 0, -1); break;
                case 4: Appliquer(m_vortex, cible, (EffectsEnum)128, 2, 0, -1); break;
                case 5: Appliquer(m_vortex, cible, (EffectsEnum)1163, 70, 0, -1, "D"); break;
                case 6: Appliquer(m_vortex, cible, (EffectsEnum)123, 400, 0, -1); break;
                case 7: AjouterStat(cible, m_vortex, 150, PlayerFields.CriticalDamageReduction, -1); break;
                case 8: Appliquer(m_vortex, cible, (EffectsEnum)111, 4, 0, -1); break;
                case 9: Appliquer(m_vortex, cible, (EffectsEnum)118, 400, 0, -1); break;
                case 10: AjouterEtat(cible, EtatInebranlable); break;
                case 11: Appliquer(m_vortex, cible, (EffectsEnum)1078, 30, 0, -1); break;
                case 12: Appliquer(m_vortex, cible, (EffectsEnum)119, 400, 0, -1); break;
            }
        }

        #endregion

        #region Outils

        static string Nom(FightActor monstre)
        {
            switch ((monstre as MonsterFighter)?.Monster.Template.Id)
            {
                case 3834: return "Ikargn";
                case 3836: return "Méjaire";
                case 3837: return "Harpille";
                case 3838: return "Buboxor";
                case 3839: return "Brabuzar";
                default: return "Le monstre";
            }
        }

        IEnumerable<CharacterFighter> Joueurs() => m_vortex.OpposedTeam.Fighters.OfType<CharacterFighter>();

        /// <summary>Monstres des vagues (sans le Vortex ni l'Auroraire), vivants ou en attente de résurrection.</summary>
        IEnumerable<FightActor> Monstres() =>
            m_vortex.Team.Fighters.Where(x => x is MonsterFighter && x != m_vortex && x != m_auroraire && (x.IsAlive() || m_aRessusciter.Contains(x)));

        bool EstMonstreActif(FightActor fighter) =>
            fighter is MonsterFighter && fighter.Team == m_vortex.Team && fighter != m_vortex && fighter != m_auroraire && !m_corrompus.Contains(fighter);

        /// <summary>Comme l'apparition des vagues : case de placement libre au hasard, sinon case libre la plus proche d'une case de placement.</summary>
        Cell CaseDeVague()
        {
            var bleues = m_vortex.Team.PlacementCells ?? new Cell[0];
            var libres = bleues.Where(x => m_fight.IsCellFree(x)).ToList();
            if (libres.Count > 0)
                return libres[new Random().Next(libres.Count)];

            return bleues.Length == 0 ? CaseLibreAutour(m_vortex.Cell) : m_fight.Map.Cells
                .Where(x => x.Walkable && !x.NonWalkableDuringFight && m_fight.IsCellFree(x))
                .OrderBy(x => bleues.Min(b => new MapPoint(b).ManhattanDistanceTo(new MapPoint(x))))
                .FirstOrDefault();
        }

        Cell CaseDeBase(FightActor monstre)
        {
            if (!m_casesBase.TryGetValue(monstre, out var depart))
                return CaseDeVague();
            if (depart.Walkable && !depart.NonWalkableDuringFight && m_fight.IsCellFree(depart))
                return depart;
            var point = new MapPoint(depart);
            return m_fight.Map.Cells.Where(x => x.Walkable && !x.NonWalkableDuringFight && m_fight.IsCellFree(x))
                .OrderBy(x => point.ManhattanDistanceTo(new MapPoint(x))).FirstOrDefault() ?? CaseDeVague();
        }

        Cell CaseLibreAutour(Cell centre)
        {
            var point = new MapPoint(centre);
            return point.GetAdjacentCells(x => true)
                .Concat(new MapPoint(centre).GetAdjacentCells(x => true).SelectMany(x => x.GetAdjacentCells(y => true)))
                .Select(x => m_fight.Map.Cells[x.CellId])
                .FirstOrDefault(x => x.Walkable && !x.NonWalkableDuringFight && m_fight.IsCellFree(x));
        }

        void Annoncer(string message)
        {
            foreach (var joueur in Joueurs())
                joueur.Character.SendServerMessage(message, Color.FromArgb(255, 210, 120));
        }

        SpellEffectHandler Handler(FightActor lanceur, FightActor cible, EffectsEnum effet, int diceNum, int valeur, int duree, string declencheurs = "I")
        {
            var dice = new EffectDice(effet, valeur, diceNum, 0) { Duration = duree, Triggers = declencheurs };
            var cast = new DefaultSpellCastHandler(new SpellCastInformations(lanceur, new Spell(VortexiphanSpellId, 1), cible.Cell));
            var handler = EffectManager.Instance.GetSpellEffectHandler(dice, lanceur, cast, cible.Cell, false);
            handler.SetAffectedActors(new[] { cible });
            return handler;
        }

        void Appliquer(FightActor lanceur, FightActor cible, EffectsEnum effet, int diceNum, int valeur, int duree, string declencheurs = "I")
        {
            try
            {
                Handler(lanceur, cible, effet, diceNum, valeur, duree, declencheurs).Apply();
            }
            catch (Exception ex)
            {
                Logger.Error("[Vortex] effet {0} sur {1} impossible : {2}", effet, cible, ex.Message);
            }
        }

        StatBuff AjouterStat(FightActor cible, FightActor lanceur, int valeur, PlayerFields champ, int duree)
        {
            try
            {
                var handler = Handler(lanceur, cible, EffectsEnum.Effect_AddCriticalHit, 0, 0, duree);
                return handler.AddStatBuff(cible, (short)valeur, champ) as StatBuff;
            }
            catch (Exception ex)
            {
                Logger.Error("[Vortex] stat {0} sur {1} impossible : {2}", champ, cible, ex.Message);
                return null;
            }
        }

        void AjouterEtat(FightActor cible, int etat)
        {
            var state = SpellManager.Instance.GetSpellState((uint)etat);
            if (state == null)
                return;

            var handler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, etat, 0, 0) { Duration = -1 }, cible, null, cible.Cell, false);
            var buff = new StateBuff(cible.PopNextBuffId(), cible, m_vortex, handler, new Spell(VortexiphanSpellId, 1),
                FightDispellableEnum.DISPELLABLE_BY_DEATH, state) { Duration = -1000 };
            cible.AddBuff(buff, true);
        }

        void AjouterSkipTurn(FightActor cible)
        {
            var handler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, EtatInebranlable, 0, 0) { Duration = -1 }, cible, null, cible.Cell, false);
            var buff = new SkipTurnBuff(cible.PopNextBuffId(), cible, m_vortex, handler, new Spell(VortexiphanSpellId, 1), false,
                FightDispellableEnum.REALLY_NOT_DISPELLABLE) { Duration = -1000 };
            cible.AddBuff(buff, true);
        }

        static void RetirerEtat(FightActor cible, int etat)
        {
            foreach (var buff in cible.GetBuffs(x => x is StateBuff s && s.State.Id == etat).ToArray())
                cible.RemoveBuff(buff);
        }

        #endregion
    }

    /// <summary>Glyphe d'un monstre de l'Oeil de Vortex : un personnage qui y entre déclenche l'échange (géré par le contrôleur).</summary>
    public class GlypheVortex : Glyph
    {
        readonly Action<GlypheVortex, FightActor> m_declenchee;

        public GlypheVortex(short id, FightActor caster, Spell spell, EffectDice dice, Cell cell, Action<GlypheVortex, FightActor> declenchee)
            : base(id, caster, spell, dice, new Spell(VortexControleur.GlypheEffetSpellId, 1), cell, SpellShapeEnum.P, 0, 0,
                   Color.FromArgb(64, 64, 100), false, TriggerType.MOVE)
        {
            m_declenchee = declenchee;
        }

        public override bool CanTrigger(FightActor actor) => actor is CharacterFighter && actor.Team != Caster.Team;

        public override void Trigger(FightActor trigger, Cell triggerCell) => m_declenchee(this, trigger);
    }
}
