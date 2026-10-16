using System.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Dialogs.Interactives;
using Stump.Server.WorldServer.Game.Dialogs.Jobs;
using Stump.Server.WorldServer.Game.Jobs;

namespace Stump.Server.WorldServer.Game.Interactives.Skills
{
    /// <summary>
    /// "Se faire transporter" : opens the zaapi dialog of the interactive.
    /// </summary>
    [Discriminator(157, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    public class SkillZaapiTemplate : Skill
    {
        public SkillZaapiTemplate(int id, InteractiveSkillTemplate skillTemplate, InteractiveObject interactiveObject)
            : base(id, skillTemplate, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            var dialog = new ZaapiDialog(character, InteractiveObject);
            dialog.Open();

            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// "Ressusciter" (obsidian) : brings a ghost back to life.
    /// </summary>
    [Discriminator(211, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    public class SkillReviveTemplate : Skill
    {
        public SkillReviveTemplate(int id, InteractiveSkillTemplate skillTemplate, InteractiveObject interactiveObject)
            : base(id, skillTemplate, interactiveObject)
        {
        }

        public override bool IsEnabled(Character character) => character.PlayerLifeStatus == PlayerLifeStatusEnum.STATUS_PHANTOM;

        public override int StartExecute(Character character)
        {
            character.Energy = 1000;
            character.RefreshStats();
            character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 33);

            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// "Consulter" (list of craftsmen) : opens the craftsmen directory for every job.
    /// </summary>
    [Discriminator(170, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    [Discriminator(293, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    public class SkillCraftsmenDirectory : Skill
    {
        public SkillCraftsmenDirectory(int id, InteractiveSkillTemplate skillTemplate, InteractiveObject interactiveObject)
            : base(id, skillTemplate, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            var jobs = JobManager.Instance.GetJobTemplates().Where(x => x.Id != 1).ToArray();
            var dialog = new JobIndexDialog(character, jobs);
            dialog.Open();

            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// "Actionner" (switch / lever). Puzzle levers need a dungeon specific script,
    /// until one is written the lever only tells the player it does nothing.
    /// </summary>
    [Discriminator(179, typeof(Skill), typeof(int), typeof(InteractiveSkillTemplate), typeof(InteractiveObject))]
    public class SkillSwitch : Skill
    {
        public SkillSwitch(int id, InteractiveSkillTemplate skillTemplate, InteractiveObject interactiveObject)
            : base(id, skillTemplate, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            character.SendServerMessage("Ce mécanisme ne semble rien déclencher pour le moment.");
            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// Used for any skill whose behaviour is not implemented yet, instead of dropping it with an error.
    /// </summary>
    public class SkillUnavailable : Skill
    {
        public SkillUnavailable(int id, InteractiveSkillTemplate skillTemplate, InteractiveObject interactiveObject)
            : base(id, skillTemplate, interactiveObject)
        {
        }

        public override int StartExecute(Character character)
        {
            character.SendServerMessage("Cette action n'est pas encore disponible sur ce serveur.");
            return base.StartExecute(character);
        }
    }
}
