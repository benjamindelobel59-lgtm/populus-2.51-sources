using Stump.DofusProtocol.Enums;
using System.Linq;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Interactives;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator((int)SkillTemplateEnum.USE_114, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    [Discriminator("ZaapTeleport", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillZaapTeleport : Skill
    {
        public SkillZaapTeleport(int id, InteractiveSkillTemplate record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        public SkillZaapTeleport(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record.Template, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            // PADGREF 23/10 : zaap de l'Arche de Vili = seulement les 4 salles des portails de l'Eliocalypse
            var dialog = InteractiveObject.Map.Id == ZaapDialog.ArcheDeVili
                ? new ZaapDialog(character, InteractiveObject, ZaapDialog.SallesPortails.Keys.Select(id => Game.World.Instance.GetMap(id)).Where(m => m != null))
                : new ZaapDialog(character, InteractiveObject, character.KnownZaaps);
            dialog.Open();

            return base.StartExecute(character);
        }
    }
}