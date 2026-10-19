using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.Interactives;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Interactives;
using Stump.Server.WorldServer.Game.Interactives.Skills;
using Stump.Server.WorldServer.Game.Items.Player;

namespace Stump.Server.WorldServer.Game.Chasses
{
    /// <summary>MYSTIC 19/10 : coffre de chasse (de niveau ou légendaire) : des Roses des sables (ChasseManager.OuvrirCoffre).</summary>
    public class CoffreChasse : BasePlayerItem
    {
        public CoffreChasse(Character owner, PlayerItemRecord record) : base(owner, record) { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
            => ChasseManager.Instance.OuvrirCoffre(Owner, Template.Id);
    }

    /// <summary>MYSTIC 19/10 : Boîte de fragments : 0, 1 ou 2 fragments de carte au hasard (ChasseManager.OuvrirBoite).</summary>
    public class BoiteFragments : BasePlayerItem
    {
        public BoiteFragments(Character owner, PlayerItemRecord record) : base(owner, record) { }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
            => ChasseManager.Instance.OuvrirBoite(Owner);
    }

    /// <summary>
    /// MYSTIC 19/10 : « Chercher un trésor (niveau X) » au Distributeur de missions de la Malle aux Trésors
    /// (interactives_skills : Type « ChasseTresor », CustomTemplateId 248 à 257, Parameter0 = niveau).
    /// </summary>
    [Discriminator("ChasseTresor", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillChasseTresor : CustomSkill
    {
        public SkillChasseTresor(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject) { }

        public int Niveau => Record.GetParameter<int>(0);

        public override int StartExecute(Character character)
        {
            ChasseManager.Instance.DemarrerClassique(character, Niveau);
            return base.StartExecute(character);
        }
    }

    /// <summary>
    /// MYSTIC 19/10 : console d'une dimension dans la Salle de localisation des portails (avant : téléportation directe).
    /// Portail ouvert -> sa position ; sinon -> chasse aux portails. Type « ChassePortail », Parameter0 = AreaId de la dimension.
    /// </summary>
    [Discriminator("ChassePortail", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillChassePortail : CustomSkill
    {
        public SkillChassePortail(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject) { }

        public override int StartExecute(Character character)
        {
            ChasseManager.Instance.DemarrerPortail(character, Record.GetParameter<int>(0));
            return base.StartExecute(character);
        }
    }

    /// <summary>MYSTIC 19/10 : « Chercher un trésor légendaire » à la Table de planification (Type « ChasseLegendaire », CustomTemplateId 277).</summary>
    [Discriminator("ChasseLegendaire", typeof(Skill), typeof(int), typeof(InteractiveCustomSkillRecord), typeof(InteractiveObject))]
    public class SkillChasseLegendaire : CustomSkill
    {
        public SkillChasseLegendaire(int id, InteractiveCustomSkillRecord record, InteractiveObject interactiveObject)
            : base(id, record, interactiveObject) { }

        public override int StartExecute(Character character)
        {
            ChasseManager.Instance.OuvrirTableLegendaire(character);
            return base.StartExecute(character);
        }
    }
}
