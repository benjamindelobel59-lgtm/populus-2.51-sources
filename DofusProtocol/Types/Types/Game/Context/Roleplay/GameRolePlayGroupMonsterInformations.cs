namespace Stump.DofusProtocol.Types
{
    using System;
    using System.Linq;
    using System.Text;
    using Stump.DofusProtocol.Types;
    using Stump.Core.IO;

    [Serializable]
    public class GameRolePlayGroupMonsterInformations : GameRolePlayActorInformations
    {
        public new const short Id = 160;
        public override short TypeId
        {
            get { return Id; }
        }
        public bool KeyRingBonus { get; set; }
        public bool HasHardcoreDrop { get; set; }
        public bool HasAVARewardToken { get; set; }
        public GroupMonsterStaticInformations StaticInfos { get; set; }
        public sbyte LootShare { get; set; }
        public sbyte AlignmentSide { get; set; }
        // Ajout Nekfeu : bonus d'étoiles du groupe (0-200 %), envoyé à la fin du message (client modifié requis)
        public short AgeBonus { get; set; }

        public GameRolePlayGroupMonsterInformations(double contextualId, EntityLook look, EntityDispositionInformations disposition, bool keyRingBonus, bool hasHardcoreDrop, bool hasAVARewardToken, GroupMonsterStaticInformations staticInfos, sbyte lootShare, sbyte alignmentSide, short ageBonus)
            : this(contextualId, look, disposition, keyRingBonus, hasHardcoreDrop, hasAVARewardToken, staticInfos, lootShare, alignmentSide)
        {
            this.AgeBonus = ageBonus;
        }

        public GameRolePlayGroupMonsterInformations(double contextualId, EntityLook look, EntityDispositionInformations disposition, bool keyRingBonus, bool hasHardcoreDrop, bool hasAVARewardToken, GroupMonsterStaticInformations staticInfos, sbyte lootShare, sbyte alignmentSide)
        {
            this.ContextualId = contextualId;
            this.Look = look;
            this.Disposition = disposition;
            this.KeyRingBonus = keyRingBonus;
            this.HasHardcoreDrop = hasHardcoreDrop;
            this.HasAVARewardToken = hasAVARewardToken;
            this.StaticInfos = staticInfos;
            this.LootShare = lootShare;
            this.AlignmentSide = alignmentSide;
        }

        public GameRolePlayGroupMonsterInformations() { }

        public override void Serialize(IDataWriter writer)
        {
            base.Serialize(writer);
            var flag = new byte();
            flag = BooleanByteWrapper.SetFlag(flag, 0, KeyRingBonus);
            flag = BooleanByteWrapper.SetFlag(flag, 1, HasHardcoreDrop);
            flag = BooleanByteWrapper.SetFlag(flag, 2, HasAVARewardToken);
            writer.WriteByte(flag);
            writer.WriteShort(StaticInfos.TypeId);
            StaticInfos.Serialize(writer);
            writer.WriteSByte(LootShare);
            writer.WriteSByte(AlignmentSide);
            // AgeBonus : le client doit etre le DofusInvoker modifie qui le lit (sinon chargement bloque a 100 %).
            writer.WriteShort(AgeBonus);
        }

        public override void Deserialize(IDataReader reader)
        {
            base.Deserialize(reader);
            var flag = reader.ReadByte();
            KeyRingBonus = BooleanByteWrapper.GetFlag(flag, 0);
            HasHardcoreDrop = BooleanByteWrapper.GetFlag(flag, 1);
            HasAVARewardToken = BooleanByteWrapper.GetFlag(flag, 2);
            StaticInfos = ProtocolTypeManager.GetInstance<GroupMonsterStaticInformations>(reader.ReadShort());
            StaticInfos.Deserialize(reader);
            LootShare = reader.ReadSByte();
            AlignmentSide = reader.ReadSByte();
            AgeBonus = reader.ReadShort();
        }

    }
}
