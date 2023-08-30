using network;
using Server.Rooms;
using System.Numerics;

namespace Server.Components
{
    public class RoomMuseumModeComponent : RoomModeComponent
    {
        public RoomMuseumModeComponent(Room room, RoomOptions roomOptions) : base(room, roomOptions)
        {
        }

        public override Vector3 GetSpawnPosition(RoomMode beforeRoomMode)
        {
            return new Vector3(0, 0.51f, -3.7f);
        }
    }
}
