using log4net;
using network;
using Server.Units;

namespace Server.Rooms
{
    public class RoomPlayer
    {
        public readonly Room room;
        public readonly PlayerUnit Unit;
        public readonly WorldPlayer WorldPlayer;

        public bool MarkLeft;
        public bool Destroyed;
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public long Id => WorldPlayer.Id;

        public RoomPlayer(WorldPlayer player, Room room)
        {
            WorldPlayer = player;
            this.room = room;

            Unit = new PlayerUnit(this);

            Refresh();
        }

        public void Destroy()
        {
            if (Unit != null)
                Unit.Destroy();

            Destroyed = true;
        }

        public bool IsDisconnected()
        {
            return WorldPlayer.Session == null;
        }

        public void HandleSessionChanged()
        {
        }

        public void SendPacket(Packet p)
        {
            WorldPlayer.SendPacket(p);
        }

        public void Update(float dt)
        {
            if (Unit.Health.Dead == false)
                return;
        }

        public void Refresh()
        {
        }
    }
}