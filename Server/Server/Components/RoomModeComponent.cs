using log4net;
using network;
using Server.Rooms;
using System.Numerics;

namespace Server.Components
{
    public class RoomModeComponent
    {
        protected readonly Room _room;

        private readonly RoomMode _roomMode;

        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public virtual int MaxLevel => int.MaxValue;

        public virtual int MinLevel => 1;

        public RoomMode RoomMode => _roomMode;

        public RoomModeComponent(Room room, RoomOptions roomOptions)
        {
            _room = room;
            _roomMode = roomOptions.RoomMode;
        }

        public virtual void HandleFieldJoined(RoomPlayer rp)
        {
        }

        public virtual StatusCode IsJoinable(WorldPlayer player)
        {
            return StatusCode.Success;
        }

        public virtual bool IsLeader(RoomPlayer rp)
        {
            return false;
        }

        public virtual void Update(float dt)
        {
        }

        public virtual Vector3 GetSpawnPosition(RoomMode beforeRoomMode)
        {
            return new Vector3(0, 0, 0);
        }
    }
}
