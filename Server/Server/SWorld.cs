using Core;
using log4net;
using Server.Components;
using Server.Core;
using Server.Data;
using Server.Data.Enum;
using Server.Scraps;

namespace Server
{
    public class SWorld
    {
        public readonly ServerUIDTransient InstanceUid;
        public readonly ServerUIDTransient RoomUid;

        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static SWorld _instance;
        private readonly int _channelId;
        private readonly int _serverId;
        private WorldPlayerComponent _player;
        private WorldRoomComponent _room;
        private WorldSessionComponent _session;
        public static SWorld INSTANCE => _instance;
        public int ChannelId => _channelId;
        public WorldPlayerComponent Player => _player;
        public WorldRoomComponent Room => _room;
        public WorldSessionComponent Session => _session;
        public int ServerId => _serverId;

        private SWorld(int serverId, int channelId)
        {
            _serverId = serverId;
            _channelId = channelId;

            _player = new WorldPlayerComponent();
            _room = new WorldRoomComponent();
            _session = new WorldSessionComponent();

            InstanceUid = ServerUIDTransient.CachedOfDomain(1, ServerUIDTransientType.Instance);
            RoomUid = ServerUIDTransient.CachedOfDomain(1, ServerUIDTransientType.Room);
        }

        public static void Initialize(int serverId, int channelId)
        {
            _instance = new SWorld(serverId, channelId);

            INSTANCE.Player.Start();
            INSTANCE.Room.Start();
            INSTANCE.Session.Start();
        }

        public void Shutdown(int seconds)
        {
            _player.Shutdown(seconds);
            _room.Shutdown(seconds);
        }
    }
}