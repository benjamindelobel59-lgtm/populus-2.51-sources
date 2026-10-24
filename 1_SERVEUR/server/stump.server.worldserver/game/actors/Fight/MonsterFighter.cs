using System.Collections.Generic;
using System.Linq;
using Stump.Core.Extensions;
using Stump.Core.Threading;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Formulas;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Monster = Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters.Monster;
using Stump.Server.WorldServer.Game.Actors.Interfaces;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public sealed class MonsterFighter : AIFighter, ICreature
    {
        readonly Dictionary<DroppableItem, int> m_dropsCount = new Dictionary<DroppableItem, int>();
        readonly StatsFields m_stats;

        public MonsterFighter(FightTeam team, Monster monster, int waveNumber = 0, int minPlayerLevel = 0)
            : base(team, monster.Grade.Spells.ToArray(), monster.Grade.MonsterId)
        {
            Id = Fight.GetNextContextualId();
            Monster = monster;
            Look = monster.Look.Clone();

            m_stats = new StatsFields(this);
            m_stats.Initialize(Monster.Grade);
            
            Fight.FindRandomFreeCell(this, out var cell, false);
            Position = new ObjectPosition(monster.Group.Map, cell, monster.Group.Direction);
            WaveNumber = waveNumber;
            MinPlayerLevel = minPlayerLevel;
            CreerCerveau(monster.Grade.MonsterId);
        }

        public int WaveNumber
        {
            get;
            set;
        }

        public int MinPlayerLevel
        {
            get;
            set;
        }

        public Monster Monster
        {
            get;
        }

        public MonsterGrade MonsterGrade
        {
            get { return Monster.Grade; }
        }

        public override string Name
        {
            get { return Monster.Template.Name; }
        }

        public override ObjectPosition MapPosition
        {
            get { return Monster.Group.Position; }
        }

        public override ushort Level
        {
            get
            {
                return (ushort) Monster.Grade.Level;
            }
        }

        public byte HiddenLevel
        {
            get
            {
                return (byte)Monster.Grade.HiddenLevel;
            }
        }

        public override StatsFields Stats
        {
            get { return m_stats; }
        }


        // monster ignore tackles ...
        public override int GetTackledAP(int mp, Cell cell)
        {
            return 0;
        }

        public override int GetTackledMP(int mp, Cell cell)
        {
            return 0;
        }

        public override bool CanTackle(FightActor fighter) => base.CanTackle(fighter) && Monster.Template.CanTackle;

        public override bool CanBePushed() => base.CanBePushed() && Monster.Template.CanBePushed;

        public override bool CanSwitchPos() => base.CanSwitchPos() && Monster.Template.CanSwitchPos;

        public override uint GetDroppedKamas()
        {
            var random = new AsyncRandom();

            return (uint) random.Next(Monster.Template.MinDroppedKamas, Monster.Template.MaxDroppedKamas + 1);
        }

        public override int GetGivenExperience() => Monster.Grade.GradeXp;

        public override bool CanDrop() => true;

        public override bool CanPlay() => base.CanPlay() && Monster.Template.CanPlay;

        // PADGREF 23/10 (retour utilisateur, Pyramide Maudite) : un monstre sans PM de base ne bouge pas, sauf le Chakanoubis sous Pouvoir
        // antique (état 492, +10 PM quand il est frappé) : il doit aller au contact pour Pyrâmide (avant : jamais lancée)
        public override bool CanMove() => base.CanMove() && (MonsterGrade.MovementPoints > 0 || HasState(492));

        public override IEnumerable<DroppedItem> RollLoot(IFightResult looter)
        {
            // have to be dead before
            if (!IsDead())
                return new DroppedItem[0];

            var random = new AsyncRandom();
            var items = new List<DroppedItem>();

            var prospectingSum = OpposedTeam.GetAllFighters<CharacterFighter>().Sum(entry => entry.Stats[PlayerFields.Prospecting].Total);
            var droppedGroups = new List<int>();
            var enDonjon = Fight.Map != null && Fight.Map.IsDungeon();

            foreach (var droppableItem in Monster.Template.DroppableItems.Where(droppableItem => prospectingSum >= droppableItem.ProspectingLock).Shuffle())
            {
                if (droppedGroups.Contains(droppableItem.DropGroup))
                    continue;

                if (looter is TaxCollectorProspectingResult && droppableItem.TaxCollectorCannotLoot)
                    continue;

                // PADGREF 21/10 : condition du drop (ex. ressource de quête : « Qa=385&PO<2269,10 » = quête en cours et moins de 10
                // exemplaires) évaluée pour le personnage qui loote ; avant, la colonne Condition n'était jamais lue
                if (!ConditionDeDropRemplie(droppableItem, looter))
                    continue;

                // MYSTIC : un seul tirage par objet ; le taux de drop multiplie deja la chance (FightFormulas.AdjustDropChance).
                // Avant : en plus, Rates.DropsRate tirages → x4 sur les quantites aussi.
                for (var i = 0; i < 1; i++)
                {
                    if (droppableItem.DropLimit > 0 && m_dropsCount.ContainsKey(droppableItem) && m_dropsCount[droppableItem] >= droppableItem.DropLimit)
                        break;

                    var chance = ( random.Next(0, 100) + random.NextDouble() );
                    var dropRate = FightFormulas.AdjustDropChance(looter, droppableItem, Monster, Fight.AgeBonus);
                    var ressource = EstRessourceHeroique(droppableItem);
                    if (ressource && enDonjon)
                        dropRate /= 2;

                    if (!(dropRate >= chance))
                        continue;

                    if (droppableItem.DropGroup != 0)
                        droppedGroups.Add(droppableItem.DropGroup);

                    // PADGREF 22/10 : hors donjon, une ressource tombée a ChanceRessourceDouble % de chances de tomber en 2 exemplaires
                    var quantite = ressource && !enDonjon && random.Next(0, 100) < ChanceRessourceDouble ? 2u : 1u;
                    items.Add(new DroppedItem(droppableItem.ItemId, quantite));
                    // PADGREF 23/10 (demande utilisateur) : message au joueur quand sa ressource tombe en double
                    if (quantite == 2)
                        World.Instance.GetCharacter(looter.Id)?.SendServerMessage(
                            $"Butin doublé ! {ItemManager.Instance.TryGetTemplate(droppableItem.ItemId)?.Name ?? "Ressource"} x2", System.Drawing.Color.Gold);

                    if (!m_dropsCount.ContainsKey(droppableItem))
                        m_dropsCount.Add(droppableItem, 1);
                    else
                        m_dropsCount[droppableItem]++;
                }
            }


            return items;
        }

        /// <summary>
        /// PADGREF 22/10 (demande utilisateur, serveur héroïque : tout le monde farme les donjons, plus personne en zone) : les
        /// RESSOURCES (super-type 9) tombent 2 fois moins en donjon ; hors donjon, taux normal mais chaque ressource tombée a
        /// <see cref="ChanceRessourceDouble"/> % de chances de tomber en double. Ressources de quête (drop avec condition) non touchées.
        /// Seulement quand le mode héroïque est activé.
        /// </summary>
        public const int ChanceRessourceDouble = 10;

        static bool EstRessourceHeroique(DroppableItem item)
        {
            if (!Heroique.HeroicManager.Enabled || !string.IsNullOrWhiteSpace(item.Condition))
                return false;
            return ItemManager.Instance.TryGetTemplate(item.ItemId)?.Type?.SuperTypeId == 9;
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Conditions.ConditionExpression> s_conditionsDrops =
            new System.Collections.Concurrent.ConcurrentDictionary<string, Conditions.ConditionExpression>();

        /// <summary>PADGREF 21/10 : vrai si le drop n'a pas de condition, ou si le personnage qui loote la remplit (percepteur : non).</summary>
        static bool ConditionDeDropRemplie(Database.Monsters.DroppableItem item, IFightResult looter)
        {
            if (string.IsNullOrWhiteSpace(item.Condition))
                return true;
            var perso = World.Instance.GetCharacter(looter.Id);
            if (perso == null)
                return false;
            Conditions.ConditionExpression expr;
            try
            {
                expr = s_conditionsDrops.GetOrAdd(item.Condition, c => Conditions.ConditionExpression.Parse(c));
            }
            catch (System.Exception ex)
            {
                s_conditionsDrops.TryAdd(item.Condition, null);
                logger.Warn("[Drops] condition illisible « {0} » (objet {1}) : {2}", item.Condition, item.ItemId, ex.Message);
                return false;
            }
            if (expr == null)
                return false;
            try
            {
                return expr.Eval(perso);
            }
            catch
            {
                return false;
            }
        }

        public override int CalculateDamageResistance(int damage, EffectSchoolEnum type, bool critical, bool withArmor, bool poison, bool isRanged)
        {
            var percentResistance = CalculateTotalResistances(type, true, poison);
            var fixResistance = CalculateTotalResistances(type, false, poison);
            var armorResistance = withArmor && !poison ? CalculateArmorReduction(type) : 0;

            var result = (int)((1 - percentResistance / 100d) * (damage - armorResistance - fixResistance)) -
                         (critical ? Stats[PlayerFields.CriticalDamageReduction].Total : 0);

            return result;
        }

        public override GameContextActorInformations GetGameContextActorInformations(Character character)
        {
            return GetGameFightFighterInformations();
        }

        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            return new GameFightMonsterInformations(
                Id,
                Look.GetEntityLook(),
                GetEntityDispositionInformations(client),
                (sbyte)Team.Id,
                0,
                IsAlive(),
                GetGameFightMinimalStats(client),
                new ushort[0],
                (ushort)Monster.Template.Id,
                (sbyte)Monster.Grade.GradeId,
                (short)Monster.Grade.Level);
        }

        public override GameFightFighterLightInformations GetGameFightFighterLightInformations(WorldClient client = null)
        {
            return new GameFightFighterMonsterLightInformations(
                true,
                IsAlive(),
                Id,
                0,
                Level,
                (sbyte)BreedEnum.MONSTER,
                (ushort)Monster.Template.Id);
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations()
        {
            return new FightTeamMemberMonsterInformations(Id, Monster.Template.Id, (sbyte) Monster.Grade.GradeId);
        }

        public override string GetMapRunningFighterName()
        {
            return Monster.Template.Id.ToString();
        }

        public override string ToString()
        {
            return Monster.ToString();
        }

        protected override void OnDisposed()
        {
            base.OnDisposed();

            if (!Monster.Group.IsDisposed)
                Monster.Group.Delete();
        }
    }
}