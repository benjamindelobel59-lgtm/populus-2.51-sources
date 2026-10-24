using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    /// <summary>
    /// PADGREF 24/10 : termine une quête en cours (Parameter0 = numéro de la quête) : tous les objectifs restants de son étape sont validés.
    /// Sert à la quête d'accès à Frigost (649) : on la termine en reparlant au Capitaine Ardier à l'arrivée sur l'île.
    /// </summary>
    [Discriminator("QuestFinish", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class FinishQuestReply : NpcReply
    {
        public FinishQuestReply(NpcReplyRecord record)
            : base(record)
        {
        }

        public int QuestId => Record.GetParameter<int>(0);

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character))
                return false;

            var quete = character.Quests.FirstOrDefault(x => x.Id == QuestId && !x.Finished);
            if (quete?.CurrentStep == null)
                return true;

            foreach (var objectif in quete.CurrentStep.Objectives.Where(x => !x.Finished).ToArray())
                objectif.CompleteObjective();
            return true;
        }
    }
}
