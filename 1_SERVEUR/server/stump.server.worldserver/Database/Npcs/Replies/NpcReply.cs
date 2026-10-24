using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Conditions;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    public abstract class NpcReply
    {
        protected NpcReply()
        {
            Record = new NpcReplyRecord();
        }

        protected NpcReply(NpcReplyRecord record)
        {
            Record = record;
        }

        public int Id
        {
            get { return Record.Id; }
            set { Record.Id = value; }
        }


        public int ReplyId
        {
            get { return Record.ReplyId; }
            set { Record.ReplyId = value; }
        }

        public int MessageId
        {
            get { return Record.MessageId; }
            set { Record.MessageId = value; }
        }

        public ConditionExpression CriteriaExpression
        {
            get { return Record.CriteriaExpression; }
            set { Record.CriteriaExpression = value; }
        }

        public NpcMessage Message
        {
            get { return Record.Message; }
            set { Record.Message = value; }
        }

        public NpcReplyRecord Record
        {
            get;
            private set;
        }

        /// <summary>PADGREF 24/10 : condition d'AFFICHAGE facultative, écrite dans AdditionalParameters sous la forme « afficher:<critères> »
        /// (ex. « afficher:Qf!649 »). Les critères (colonne Criteria) ne jouent qu'au clic : une réponse qu'on ne doit pas voir restait
        /// affichée puis refusait. Seules les réponses qui ont « afficher: » sont concernées.</summary>
        public virtual bool CanShow(Npc npc, Character character)
        {
            var extra = Record.AdditionalParameters;
            if (string.IsNullOrEmpty(extra) || !extra.StartsWith("afficher:"))
                return true;
            if (m_afficher == null)
                m_afficher = Game.Conditions.ConditionExpression.Parse(extra.Substring("afficher:".Length));
            return m_afficher == null || m_afficher.Eval(character);
        }

        Game.Conditions.ConditionExpression m_afficher;

        public virtual bool CanExecute(Npc npc, Character character) => Record.CriteriaExpression == null || Record.CriteriaExpression.Eval(character);

        public virtual bool Execute(Npc npc, Character character)
        {
            if (CanExecute(npc, character))
                return true;

            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 34);
            return false;
        }
    }
}