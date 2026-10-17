using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Exchanges.Craft;
using Stump.Server.WorldServer.Game.Exchanges.Craft.Runes;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    [Discriminator((int)SkillTemplateEnum.SHATTER_AN_ITEM_INTO_RUNES, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    [Discriminator("Decraft", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillDecraftItem : Skill
    {
        public SkillDecraftItem(int id, InteractiveSkillTemplate record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        public SkillDecraftItem(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record.Template ?? InteractiveManager.Instance.GetDefaultSkillTemplate(), interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            var trade = new RuneTrade(character);
            trade.Open();
            return 0;
        }
    }
}