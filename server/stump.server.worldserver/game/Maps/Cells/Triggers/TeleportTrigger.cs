using System;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Database;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Database.World.Triggers;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Maps.Cells.Triggers
{
    [Discriminator("Teleport", typeof(CellTrigger), typeof(CellTriggerRecord))]
    public class TeleportTrigger : CellTrigger
    {
        public TeleportTrigger(CellTriggerRecord record)
            : base(record)
        {
        }

        private short? m_destinationCellId;
        private int? m_destinationMapId;
        private ObjectPosition m_destinationPosition;
        private bool m_mustRefreshDestinationPosition;

        /// <summary>
        /// Parameter 0
        /// </summary>
        public short DestinationCellId
        {
            get
            {
                return m_destinationCellId ?? ( m_destinationCellId = Record.GetParameter<short>(0) ).Value;
            }
            set
            {
                Record.SetParameter(0, value);
                m_destinationCellId = value;
                m_mustRefreshDestinationPosition = true;
            }
        }

        /// <summary>
        /// Parameter 1
        /// </summary>
        public int DestinationMapId
        {
            get
            {
                return m_destinationMapId ?? ( m_destinationMapId = Record.GetParameter<int>(1) ).Value;
            }
            set
            {
                m_destinationMapId = value;
                m_mustRefreshDestinationPosition = true;
            }
        }

        private void RefreshPosition()
        {
            Map map = World.Instance.GetMap(DestinationMapId);

            if (map == null)
                throw new Exception(string.Format("Cannot load CellTeleport id={0}, DestinationMapId {1} isn't found", Record.Id, DestinationMapId));

            // NEKFEU : case -1 (centre) ou case occupee par une porte / un declencheur -> case libre la plus proche
            Cell cell = map.GetArrivalCell(DestinationCellId);

            m_destinationPosition = new ObjectPosition(map, cell, DirectionsEnum.DIRECTION_EAST);
        }

        public ObjectPosition GetDestinationPosition()
        {
            if (m_destinationPosition == null || m_mustRefreshDestinationPosition)
                RefreshPosition();

            m_mustRefreshDestinationPosition = false;

            return m_destinationPosition;
        }

        public override void Apply(Character character)
        {
            if (!Record.IsConditionFilled(character))
            {
                character.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_ERROR, 1); //Certaines conditions ne sont pas satisfaites
                return;
            }              

            var destination = GetDestinationPosition();
            character.Teleport(destination.Map, destination.Cell);
        }
    }
}