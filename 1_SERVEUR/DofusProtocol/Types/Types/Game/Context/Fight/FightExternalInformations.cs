namespace Stump.DofusProtocol.Types
{
    using System;
    using System.Linq;
    using System.Text;
    using Stump.DofusProtocol.Types;
    using System.Collections.Generic;
    using Stump.Core.IO;

    [Serializable]
    public class FightExternalInformations
    {
        public const short Id  = 117;
        public virtual short TypeId
        {
            get { return Id; }
        }
        public ushort FightId { get; set; }
        public sbyte FightType { get; set; }
        public int FightStart { get; set; }
        public bool FightSpectatorLocked { get; set; }
        // MYSTIC 20/10 : le client 2.51 lit aussi les 2 équipes et leurs options (sans elles, la liste des combats en cours
        // (l'œil spectateur) était illisible pour le client)
        public FightTeamLightInformations[] FightTeams { get; set; }
        public FightOptionsInformations[] FightTeamsOptions { get; set; }

        public FightExternalInformations(ushort fightId, sbyte fightType, int fightStart, bool fightSpectatorLocked)
        {
            this.FightId = fightId;
            this.FightType = fightType;
            this.FightStart = fightStart;
            this.FightSpectatorLocked = fightSpectatorLocked;
        }

        public FightExternalInformations(ushort fightId, sbyte fightType, int fightStart, bool fightSpectatorLocked, FightTeamLightInformations[] fightTeams, FightOptionsInformations[] fightTeamsOptions)
            : this(fightId, fightType, fightStart, fightSpectatorLocked)
        {
            this.FightTeams = fightTeams;
            this.FightTeamsOptions = fightTeamsOptions;
        }

        public FightExternalInformations() { }

        public virtual void Serialize(IDataWriter writer)
        {
            writer.WriteVarUShort(FightId);
            writer.WriteSByte(FightType);
            writer.WriteInt(FightStart);
            writer.WriteBoolean(FightSpectatorLocked);
            for (var i = 0; i < 2; i++)
                (FightTeams != null && FightTeams.Length > i ? FightTeams[i] : new FightTeamLightInformations()).Serialize(writer);
            for (var i = 0; i < 2; i++)
                (FightTeamsOptions != null && FightTeamsOptions.Length > i ? FightTeamsOptions[i] : new FightOptionsInformations()).Serialize(writer);
        }

        public virtual void Deserialize(IDataReader reader)
        {
            FightId = reader.ReadVarUShort();
            FightType = reader.ReadSByte();
            FightStart = reader.ReadInt();
            FightSpectatorLocked = reader.ReadBoolean();
            FightTeams = new FightTeamLightInformations[2];
            for (var i = 0; i < 2; i++) { FightTeams[i] = new FightTeamLightInformations(); FightTeams[i].Deserialize(reader); }
            FightTeamsOptions = new FightOptionsInformations[2];
            for (var i = 0; i < 2; i++) { FightTeamsOptions[i] = new FightOptionsInformations(); FightTeamsOptions[i].Deserialize(reader); }
        }

    }
}
