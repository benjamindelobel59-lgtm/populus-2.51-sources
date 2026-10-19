using System;
using System.Linq;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Quests.Objectives
{
    public class DefeatMonstersObjective : QuestObjective
    {
        private int m_completion;

        public DefeatMonstersObjective(Character character, QuestObjectiveTemplate template, bool finished, int monsterId, int amount, int mapId = 0)
            : base(character, template, finished)
        {
            Monster = MonsterManager.Instance.GetTemplate(monsterId);
            Amount = amount;
            MapId = mapId;
        }

        public DefeatMonstersObjective(Character character, QuestObjectiveTemplate template, QuestObjectiveStatus finished, int monsterId, int amount, int mapId = 0)
          : base(character, template, finished)
        {
            Monster = MonsterManager.Instance.GetTemplate(monsterId);
            Amount = amount;
            MapId = mapId;
        }

        /// <summary>
        /// MYSTIC 19/10 : donjons à plusieurs boss dont un seul est combattu (Clos des Blops, Gelaxième Dimension) : n'importe quel
        /// boss de la famille valide l'objectif (« Le tour du monde »).
        /// </summary>
        static readonly int[][] Familles = { new[] { 1184, 1185, 1186, 1187 }, new[] { 58, 85, 86, 430 } };

        bool Compte(MonsterFighter x)
        {
            if (Monster == null)
                return false;
            if (x.Monster.Template == Monster)
                return true;
            var famille = Familles.FirstOrDefault(f => f.Contains(Monster.Id));
            return famille != null && famille.Contains(x.Monster.Template.Id);
        }

        /// <summary>MYSTIC 19/10 : objectif « vaincre x sur la carte #3 en un seul combat » (type 16) : 0 = n'importe quelle carte.</summary>
        public int MapId
        {
            get;
        }

        public MonsterTemplate Monster
        {
            get;
        }

        public int Amount
        {
            get;
        }

        public int Completions
        {
            get { return ObjectiveRecord.Completion; }
            private set
            {
                m_completion = value;
                ObjectiveRecord.Completion = value;
            }
        }

        public override void EnableObjective()
        {
            Character.FightEnded += OnFightEnded;
        }

        private void OnFightEnded(Character character, CharacterFighter fighter)
        {
            if (!(fighter.Fight is FightPvM))
                return;

            if ((fighter.Fight as FightPvM).IsPvMArenaFight)
                return;

            // MYSTIC 19/10 : victoire testée sur le combattant du combat qui se termine (Character.Fighter peut déjà être vide)
            if (fighter.Fight.Winners != fighter.Team)
                return;

            if (MapId != 0 && fighter.Fight.Map?.Id != MapId)
                return;

            // MYSTIC 19/10 : « en donjon seulement » : le combat doit avoir lieu sur une map de donjon (pas de capture / pierre d'âme ailleurs)
            if (Template.DungeonOnly && (fighter.Fight.Map == null || !fighter.Fight.Map.IsDungeon()))
                return;

            var count = fighter.OpposedTeam.Fighters.OfType<MonsterFighter>().Count(x => x.IsDead() && Compte(x));
            if (count == 0)
                return;

            if (Completions + count >= Amount)
            {
                Completions += count;
                CompleteObjective();
            }
            else
                Completions += count;
        }

        public override void DisableObjective()
        {
            Character.FightEnded -= OnFightEnded;
        }

        public override QuestObjectiveInformations GetQuestObjectiveInformations()
        {
            return new QuestObjectiveInformationsWithCompletion((ushort)Template.Id, ObjectiveRecord.Status ? false : true, new string[0], (ushort)Completions, (ushort)Amount);
        }

        public override bool CanSee()
        {
            return true;
        }

        public override int Completion()
        {
            return Completions;
        }
    }
}