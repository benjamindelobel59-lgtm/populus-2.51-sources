using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Alliances;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    /// <summary>MYSTIC 17/10 : livre du Temple des alliances → fenêtre de création d'alliance.</summary>
    [Discriminator("AllianceCreation", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillAllianceCreation : CustomSkill
    {
        public SkillAllianceCreation(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            new AllianceCreationPanel(character).Open();
            return base.StartExecute(character);
        }
    }
}
