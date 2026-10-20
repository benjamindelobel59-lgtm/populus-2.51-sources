using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Handlers.Actions;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 20/10 : Palais du Roi Nidas (dofusdb donjon 76, dofuspourlesnoobs). Boubourse :
    /// - elle commence au contact du Roi Nidas, joue en dernier dans la liste des tours (ce n'est pas une invocation), et lance Cadeau Empoisonné (4481) à chacun de ses tours : côté monstres, +200 puissance
    ///   et +30 % critique aux monstres (+300 / +50 % au Nidas) ; côté joueurs, 500 de bouclier aux joueurs alliés, tue les invocations
    ///   au contact de la cible, retire 2 tours d'effets sur la bourse ;
    /// - tuée dans le camp des monstres (par qui que ce soit, mort du Nidas comprise), elle passe dans le camp des joueurs (Boubourse
    ///   volée 3649, invoquée par son tueur s'il est joueur, sinon par un joueur vivant) ; après 2 de ses tours elle revient au contact du
    ///   Nidas s'il vit encore ; Nidas mort, elle change de camp tous les 2 tours (côté monstres : au contact d'un monstre vivant) ;
    /// - décision utilisateur 20/10 : elle ne disparaît jamais : tuée dans le camp des joueurs, le Nidas la récupère s'il vit, sinon
    ///   elle réapparaît chez les joueurs.
    /// Les réapparitions sont faites juste après la mort (file de la zone), jamais pendant le traitement en cours.
    /// Avant : la chaîne 2.51 (Changement de camp lancé par la Boubourse déjà morte) ne faisait rien, la Boubourse restait morte.
    /// </summary>
    public static class BoubourseNidas
    {
        public const int RoiNidas = 3648, CoteNidas = 3650, CoteJoueurs = 3649, CadeauEmpoisonne = 4481, ToursChezJoueurs = 2, CaseDepart = 316;

        static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static FightActor TrouverNidas(IFight fight) =>
            fight.GetAllFighters(x => x.IsAlive() && x is MonsterFighter m && m.Monster.Template.Id == RoiNidas).FirstOrDefault();

        public static SummonedMonster Invoquer(FightActor invocateur, int monstreId, Cell prefere)
        {
            var fight = invocateur.Fight;
            if (fight.State == FightState.Ended || !invocateur.IsAlive())
                return null;
            var grade = MonsterManager.Instance.GetMonsterGrade(monstreId, 1);
            if (grade == null)
                return null;

            bool Libre(Cell c) => c != null && c.Walkable && !c.NonWalkableDuringFight && fight.IsCellFree(c);
            var cell = Libre(prefere) ? prefere : null;
            if (cell == null)
                cell = invocateur.Position.Point.GetAdjacentCells(c => Libre(fight.Map.Cells[c]))
                    .Select(p => fight.Map.Cells[p.CellId]).FirstOrDefault();
            if (cell == null)
                cell = fight.Map.Cells.Where(c => c.Walkable && !c.NonWalkableDuringFight && fight.IsCellFree(c))
                    .OrderBy(c => new Game.Maps.Cells.MapPoint(c).ManhattanDistanceTo(invocateur.Position.Point)).FirstOrDefault();
            if (cell == null)
                return null;

            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL))
            {
                var bourse = new SummonedMonster(fight.GetNextContextualId(), invocateur.Team, invocateur, grade, cell) { JoueEnDernier = true };
                ActionsHandler.SendGameActionFightSummonMessage(fight.Clients, bourse);
                invocateur.AddSummon(bourse);
                invocateur.Team.AddFighter(bourse);
                logger.Info("[Nidas] Boubourse {0} invoquée pour {1} case {2}", monstreId, invocateur, cell.Id);
                return bourse;
            }
        }

        /// <summary>Exécute l'action juste après le traitement en cours (jamais pendant une mort ou un décompte de tour).</summary>
        static void Plus(IFight fight, System.Action action) =>
            fight.Map.Area.AddMessage(() =>
            {
                if (fight.State == FightState.Fighting)
                    action();
            });

        /// <summary>Côté monstres : le Roi Nidas s'il vit, sinon un monstre vivant (pas une invocation) ; null s'il n'en reste aucun.</summary>
        static FightActor InvocateurMonstre(IFight fight) =>
            TrouverNidas(fight) ?? fight.GetAllFighters(x => x.IsAlive() && x is MonsterFighter && !x.IsSummoned()).FirstOrDefault();

        /// <summary>Retour dans le camp des monstres : au contact du Nidas s'il vit, sinon d'un monstre vivant ; s'il n'y a plus de
        /// monstre, elle reste chez les joueurs.</summary>
        public static void RevenirChezMonstres(IFight fight, Cell caseActuelle)
        {
            Plus(fight, () =>
            {
                var invocateur = InvocateurMonstre(fight);
                if (invocateur != null)
                    Invoquer(invocateur, CoteNidas, null);
                else
                    InvoquerChezJoueurs(fight, null, caseActuelle);
            });
        }

        /// <summary>Boubourse volée chez les joueurs : invoquée par <paramref name="prefere"/> (joueur ou invocation d'un joueur) s'il
        /// vit, sinon par un joueur vivant du camp opposé au Nidas.</summary>
        public static void ReapparaitreChezJoueurs(IFight fight, FightActor prefere, Cell caseMort) =>
            Plus(fight, () => InvoquerChezJoueurs(fight, prefere, caseMort));

        static void InvoquerChezJoueurs(IFight fight, FightActor prefere, Cell caseMort)
        {
            var nidas = TrouverNidas(fight);
            bool Valide(FightActor x) => x is CharacterFighter && x.IsAlive() && (nidas == null || x.Team != nidas.Team);
            var joueur = prefere != null && prefere.IsSummoned() ? prefere.Summoner : prefere;
            if (joueur == null || !Valide(joueur))
                joueur = fight.GetAllFighters(Valide).FirstOrDefault();
            if (joueur != null)
                Invoquer(joueur, CoteJoueurs, caseMort);
        }

        public static void LancerCadeau(FightActor bourse)
        {
            if (bourse.IsAlive() && bourse.Fight.State == FightState.Fighting)
                bourse.CastAutoSpell(new Spell(CadeauEmpoisonne, 1), bourse.Cell);
        }
    }

    /// <summary>Roi Nidas : Liquidation, Confusion, Attrape-Mutin par l'IA normale ; Mon précieux (échange avec sa Boubourse) dès que
    /// possible ; passifs Boursicotage 4920 (50 % de ses dommages sur la Boubourse) et Changement de camp 4485 (sa mort tue ses alliés).
    /// Toucher de Nidas 4486 et Décoffrage 4487 retirés (absents de ses sorts sur dofusdb ; la Boubourse est posée au début).</summary>
    [BrainIdentifier(3648)]
    public class RoiNidasBrain : BrainPhorreur
    {
        const int ToucherDeNidas = 4486, Decoffrage = 4487, MonPrecieux = 4494;

        public RoiNidasBrain(AIFighter fighter) : base(fighter)
        {
            SortPassif.Poser(fighter, 4920);
            SortPassif.Poser(fighter, 4485);
            fighter.Spells.Remove(ToucherDeNidas);
            fighter.Spells.Remove(Decoffrage);
            if (fighter.Fight.State == FightState.Fighting)
                return;
            fighter.Fight.FightStarted += PoserBoubourse;
        }

        void PoserBoubourse(IFight fight)
        {
            fight.FightStarted -= PoserBoubourse;
            if (!Fighter.IsAlive())
                return;
            // au contact du Nidas : case fixe 316 des données seulement si elle le touche, sinon une case libre collée à lui
            var depart = fight.Map.Cells[BoubourseNidas.CaseDepart];
            var collee = new Game.Maps.Cells.MapPoint(depart).ManhattanDistanceTo(Fighter.Position.Point) == 1;
            BoubourseNidas.Invoquer(Fighter, BoubourseNidas.CoteNidas, collee ? depart : null);
        }

        public override void Play()
        {
            LancerMonPrecieux();
            base.Play();
        }

        void LancerMonPrecieux()
        {
            if (!Actif() || !Fighter.Spells.TryGetValue(MonPrecieux, out var sort))
                return;
            var bourse = Fighter.Team.Fighters.FirstOrDefault(x => x.IsAlive() && x is MonsterFighter m && m.Monster.Template.Id == BoubourseNidas.CoteNidas);
            if (bourse != null && Fighter.CanCastSpell(sort, bourse.Cell) == SpellCastResult.OK)
                Fighter.CastSpell(sort, bourse.Cell);
        }
    }

    /// <summary>Changement de camp périodique : après 2 de ses tours, au début du tour suivant d'un autre combattant, si
    /// <see cref="DoitChanger"/> ; elle est retirée puis réapparaît dans l'autre camp (juste après, file de la zone).</summary>
    public abstract class BrainBoubourse : BrainPassif
    {
        int m_tours;

        protected BrainBoubourse(AIFighter f, int passif) : base(f, passif)
        {
            f.Dead += SurMort;
            f.Fight.TurnStarted += DebutTour;
        }

        protected abstract bool DoitChanger(IFight fight);

        protected abstract void Changer(IFight fight, Cell cellule);

        protected abstract void Tuee(FightActor tueur, Cell cellule);

        public override void Play()
        {
            BoubourseNidas.LancerCadeau(Fighter);
            m_tours++;
        }

        void DebutTour(IFight fight, FightActor joueur)
        {
            if (m_tours < BoubourseNidas.ToursChezJoueurs || joueur == Fighter || !Fighter.IsAlive() || !DoitChanger(fight))
                return;
            fight.TurnStarted -= DebutTour;
            var cellule = Fighter.Cell;
            fight.Map.Area.AddMessage(() =>
            {
                if (fight.State != FightState.Fighting || !Fighter.IsAlive())
                    return;
                Fighter.Dead -= SurMort;
                using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_SPELL))
                    Fighter.Die();
                Changer(fight, cellule);
            });
        }

        void SurMort(FightActor bourse, FightActor tueur)
        {
            bourse.Dead -= SurMort;
            bourse.Fight.TurnStarted -= DebutTour;
            Tuee(tueur, bourse.Cell);
        }
    }

    /// <summary>Boubourse dans le camp des monstres : tuée, elle passe chez les joueurs ; quand le Nidas est mort, elle change aussi
    /// de camp tous les 2 tours (décision utilisateur 20/10).</summary>
    [BrainIdentifier(BoubourseNidas.CoteNidas)]
    public class BoubourseNidasBrain : BrainBoubourse
    {
        public BoubourseNidasBrain(AIFighter f) : base(f, 4484) { }

        protected override bool DoitChanger(IFight fight) => BoubourseNidas.TrouverNidas(fight) == null;

        protected override void Changer(IFight fight, Cell cellule) => BoubourseNidas.ReapparaitreChezJoueurs(fight, null, cellule);

        protected override void Tuee(FightActor tueur, Cell cellule) => BoubourseNidas.ReapparaitreChezJoueurs(Fighter.Fight, tueur, cellule);
    }

    /// <summary>Boubourse volée (camp des joueurs) : après 2 tours, retour chez les monstres (au contact du Nidas s'il vit, sinon d'un
    /// monstre vivant) ; tuée, le Nidas la récupère s'il vit, sinon elle réapparaît chez les joueurs.</summary>
    [BrainIdentifier(BoubourseNidas.CoteJoueurs)]
    public class BoubourseJoueursBrain : BrainBoubourse
    {
        public BoubourseJoueursBrain(AIFighter f) : base(f, 4482) { }

        protected override bool DoitChanger(IFight fight) => true;

        protected override void Changer(IFight fight, Cell cellule) => BoubourseNidas.RevenirChezMonstres(fight, cellule);

        protected override void Tuee(FightActor tueur, Cell cellule)
        {
            if (BoubourseNidas.TrouverNidas(Fighter.Fight) != null)
                BoubourseNidas.RevenirChezMonstres(Fighter.Fight, cellule);
            else
                BoubourseNidas.ReapparaitreChezJoueurs(Fighter.Fight, Fighter.Summoner, cellule);
        }
    }

    /// <summary>Kamastérisk : Partoutatice 4514 (passif) ; Clignotement 4515 (téléportation à 3 cases + dégâts autour de l'arrivée) vers
    /// les ennemis ; Assomnolence 4516 par l'IA normale.</summary>
    [BrainIdentifier(3653)]
    public class KamasteriskBrain : BrainPhorreur
    {
        public KamasteriskBrain(AIFighter f) : base(f)
        {
            SortPassif.Poser(f, 4514);
        }

        protected override int[] SortsSaut => new[] { 4515 };
    }

    /// <summary>Barbétoal : Barbe à trucs 4517 par l'IA normale ; Super saillant 4518 sur lui (+2 PM, +300 puissance au tour suivant).</summary>
    [BrainIdentifier(3654)]
    public class BarbetoalBrain : BrainPhorreur
    {
        public BarbetoalBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4518 };
    }

    /// <summary>Lévitrof : Reflets placides 4521 sur un joueur (vert ; il meurt au tour suivant du Lévitrof s'il ne l'a pas frappé) ;
    /// Kama Haméa 4522 par l'IA normale ; fuyard.</summary>
    [BrainIdentifier(3655)]
    public class LevitrofBrain : BrainPhorreur
    {
        const int RefletsPlacides = 4521;

        public LevitrofBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurEnnemi => new[] { RefletsPlacides };
        protected override bool SurEnnemiSeulementSiLoin => false;
        protected override bool CibleEnnemiValide(FightActor ennemi) => ennemi is CharacterFighter;

        /// <summary>MYSTIC 20/10 (décision utilisateur) : Reflets placides en priorité absolue dès qu'il est disponible : depuis sa case,
        /// sinon il se déplace (avec ses PM) sur la case la plus proche d'où il peut viser un joueur, puis le lance.</summary>
        public override void Play()
        {
            TenterReflets();
            base.Play();
        }

        void TenterReflets()
        {
            if (!Actif() || !Fighter.Spells.TryGetValue(RefletsPlacides, out var sort))
                return;
            var niveau = sort.CurrentSpellLevel;
            if (Fighter.AP < niveau.ApCost || !Fighter.SpellHistory.CanCastSpell(niveau))
                return;

            // joueurs pas encore verts d'abord
            var cibles = Fighter.Fight.GetAllFighters(e => e.IsAlive() && e is CharacterFighter && e.IsEnnemyWith(Fighter) && e.IsVisibleFor(Fighter))
                .OrderBy(e => e.GetBuffs(b => b.Spell.Id == RefletsPlacides).Any())
                .ThenBy(e => e.Position.Point.ManhattanDistanceTo(Fighter.Position.Point)).ToArray();
            if (cibles.Length == 0)
                return;

            foreach (var cible in cibles)
                if (Fighter.CanCastSpell(sort, cible.Cell) == SpellCastResult.OK)
                {
                    Fighter.CastSpell(sort, cible.Cell);
                    return;
                }

            if (!Fighter.CanMove())
                return;
            var ici = Fighter.Position.Point;
            foreach (var cellule in Environment.GetMovementCells()
                         .Where(c => c.Id != Fighter.Cell.Id && Environment.CellInformationProvider.IsCellWalkable(c.Id) && Fighter.Fight.IsCellFree(c))
                         .OrderBy(c => new Game.Maps.Cells.MapPoint(c).ManhattanDistanceTo(ici)))
            {
                var cible = cibles.FirstOrDefault(c => Fighter.CanCastSpell(new SpellCastInformations(Fighter, sort, c.Cell) { CastCell = cellule }) == SpellCastResult.OK);
                if (cible == null)
                    continue;
                foreach (var r in new Actions.MoveAction(Fighter, cellule).Execute(this))
                    if (r == TreeSharp.RunStatus.Failure)
                        break;
                if (Actif() && Fighter.CanCastSpell(sort, cible.Cell) == SpellCastResult.OK)
                    Fighter.CastSpell(sort, cible.Cell);
                return;
            }
        }
    }

    /// <summary>Paspartou : Clémence 4524 et La clef du succès 4527 par l'IA normale ; Guidéal 4525 sur lui (+400 puissance autour).</summary>
    [BrainIdentifier(3656)]
    public class PaspartouBrain : BrainPhorreur
    {
        public PaspartouBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4525 };
    }

    /// <summary>Piloztère : Austérité 4529 (passif : 1000 de bouclier autour de lui quand il est frappé) ; Claque, Sourire Ravageur.</summary>
    [BrainIdentifier(3657)]
    public class PilozteresBrain : BrainPhorreur
    {
        public PilozteresBrain(AIFighter f) : base(f)
        {
            SortPassif.Poser(f, 4529);
        }
    }
}
