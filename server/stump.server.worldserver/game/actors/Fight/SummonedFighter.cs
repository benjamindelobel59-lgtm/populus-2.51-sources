using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Handlers.Context;
using System.Collections.Generic;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public abstract class SummonedFighter : AIFighter
    {
        protected SummonedFighter(int id, FightTeam team, IEnumerable<Spell> spells, FightActor summoner, Cell cell)
            : base(team, spells)
        {
            Id = id;
            Position = summoner.Position.Clone();
            Cell = cell;
            Summoner = summoner;

            FightStartPosition = Position.Clone();
            MovementHistory.RegisterEntry(FightStartPosition.Cell);
        }

        protected SummonedFighter(int id, FightTeam team, IEnumerable<Spell> spells, FightActor summoner, Cell cell, int identifier, MonsterGrade template)
            : base(team, spells, identifier, template)
        {
            Id = id;
            Position = summoner.Position.Clone();
            Cell = cell;
            Summoner = summoner;

            FightStartPosition = Position.Clone();
            MovementHistory.RegisterEntry(FightStartPosition.Cell);
        }

        public override sealed int Id
        {
            get;
            protected set;
        }

        private CharacterFighter m_controller;
        public CharacterFighter Controller
        {
            get { return m_controller; }
            protected set
            {
                m_controller = value;

                if (value == null)
                {
                    Fight.TurnStopped -= OnTurnStopped;
                }
                else
                {
                    Fight.TurnStopped += OnTurnStopped;
                }
            }
        }

        public void SetController(CharacterFighter controller)
        {
            Controller = controller;
        }

        public bool IsControlled() => Controller != null;

        public override void OnTurnStarted(IFight fight, FightActor currentfighter)
        {
            if (IsControlled())
            {
                return;
            }

            base.OnTurnStarted(fight, currentfighter);
        }

        public override bool HasResult => false;

        public override int GetTackledAP(int mp, Cell cell) => 0;

        public override int GetTackledMP(int mp, Cell cell) => 0;

        public CharacterCharacteristicsInformations GetSlaveCharacteristicsInformations()
        {
            var characterFighter = Summoner as CharacterFighter;
            if (characterFighter == null)
            {
                return new CharacterCharacteristicsInformations();
            }

            var slaveStats = characterFighter.Character.GetCharacterCharacteristicsInformations();

            slaveStats.ActionPoints = Stats.AP;
            slaveStats.ActionPointsCurrent = (short)Stats.AP.Total;
            slaveStats.MovementPoints = Stats.MP;
            slaveStats.MovementPointsCurrent = (short)Stats.MP.Total;
            slaveStats.LifePoints = (uint)Stats.Health.Total;
            slaveStats.MaxLifePoints = (uint)Stats.Health.TotalMax;

            // MYSTIC : toutes les caracteristiques viennent de l'invocation (avant : seulement PA/PM/PV et les 5 carac,
            // le reste — portee, dommages, resistances, critiques... — etait celui de l'invocateur → PO fausse pour une invocation controlee)
            foreach (var field in typeof(CharacterCharacteristicsInformations).GetProperties())
            {
                if (field.PropertyType != typeof(CharacterBaseCharacteristic) || !field.CanWrite || field.Name == "ActionPoints" || field.Name == "MovementPoints")
                    continue;

                string statName;
                if (!m_slaveFieldNames.TryGetValue(field.Name, out statName))
                    statName = field.Name.Replace("ElementResistPercent", "ResistPercent");

                PlayerFields stat;
                var data = System.Enum.TryParse(statName, out stat) ? Stats[stat] : null;
                field.SetValue(slaveStats, data != null ? (CharacterBaseCharacteristic)data : new CharacterBaseCharacteristic(0, 0, 0, 0, 0));
            }

            return slaveStats;
        }

        // MYSTIC : nom du champ de la fiche client -> nom de la caracteristique du serveur (quand ils different)
        private static readonly System.Collections.Generic.Dictionary<string, string> m_slaveFieldNames = new System.Collections.Generic.Dictionary<string, string>
        {
            {"SummonableCreaturesBoost", "SummonLimit"},
            {"Reflect", "DamageReflection"},
            {"AllDamagesBonus", "DamageBonus"},
            {"WeaponDamagesBonusPercent", "WeaponDamageBonus"},
            {"DamagesBonusPercent", "DamageBonusPercent"},
            {"PAAttack", "APAttack"},
            {"PMAttack", "MPAttack"},
            {"DodgePALostProbability", "DodgeAPProbability"},
            {"DodgePMLostProbability", "DodgeMPProbability"},
        };


        protected override void OnDead(FightActor killedBy, bool passTurn = true, bool isKillEffect = false)
        {
            Controller = null;
            base.OnDead(killedBy, passTurn);
            Summoner.RemoveSummon(this);
        }

        void OnTurnStopped(IFight fight, FightActor player)
        {
            if (fight.TimeLine.Next != this)
            {
                return;
            }

            var characterFighter = Summoner as CharacterFighter;
            if (characterFighter == null)
            {
                return;
            }

            // MYSTIC : One Window -> l'invocation d'un esclave se joue depuis la fenetre du maitre
            var master = IsControlled() ? characterFighter.GetOneWindowMaster() : null;
            if (master != null)
            {
                ContextHandler.SendSlaveSwitchContextMessage(master.Character.Client, master, this);
                return;
            }

            ContextHandler.SendSlaveSwitchContextMessage(characterFighter.Character.Client, this);
        }
    }
}