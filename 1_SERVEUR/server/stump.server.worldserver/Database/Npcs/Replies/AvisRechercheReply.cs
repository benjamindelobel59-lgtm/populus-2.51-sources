using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Npcs;
using Stump.Server.WorldServer.Game.Dialogs.Npcs;
using Stump.Server.WorldServer.Game.Recherches;

namespace Stump.Server.WorldServer.Database.Npcs.Replies
{
    /// <summary>
    /// MYSTIC 19/10 : réponses du Chasseur de primes (PNJ 10062, zaap d'Astrub). Parameter0 = « libres » (recherchés présents dans
    /// leur zone), « vaincus » (déjà vaincus, heure de retour), « suivant » (page suivante) ou « retour » (résumé).
    /// Les textes sont calculés par RecherchesManager.ParametresDialogue (paramètre #1 des messages 60003 / 60004).
    /// </summary>
    [Discriminator("AvisRecherche", typeof(NpcReply), typeof(NpcReplyRecord))]
    public class AvisRechercheReply : NpcReply
    {
        public AvisRechercheReply(NpcReplyRecord record)
            : base(record)
        {
        }

        string Action => Record.GetParameter<string>(0, true) ?? "";

        public override bool CanShow(Npc npc, Character character) =>
            base.CanShow(npc, character) && RecherchesManager.Instance.ReponseVisible(character, Action);

        public override bool Execute(Npc npc, Character character)
        {
            if (!base.Execute(npc, character) || !(character.Dialog is NpcDialog dialog))
                return false;

            dialog.ChangeMessage(RecherchesManager.Instance.Consulter(character, Action, dialog.CurrentMessage?.Id ?? 0));
            return true;
        }
    }
}
