using Core;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using log4net;
using network;
using Server.Controllers;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace Server
{
    public class WorldSession
    {
        public readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ControllerSet _controllerSet;
        private readonly Ping _ping = new Ping();
        private IChannel _channel;
        private WorldPlayer _player;
        private readonly ConcurrentQueue<Packet> _gatheringPacket = new ConcurrentQueue<Packet>();

        public WorldSession(IChannel channel)
        {
            _channel = channel;

            _controllerSet = new ControllerSet(this);
        }

        public void Close()
        {
            _player = null;
            _channel?.CloseAsync().Wait();
            _channel = null;
        }

        public async Task HandlePacket(Packet p)
        {
#if DEBUG
            if (p.MessageCase != Packet.MessageOneofCase.Ping && p.MessageCase != Packet.MessageOneofCase.UnitMove)
                logger.Info("World GotPacket type=" + p.MessageCase);
#endif
            await _controllerSet.Response(p);
        }

        public void Mount(WorldPlayer player)
        {
            _player = player;
            _controllerSet.SetPlayer(player);
            logger.Info($"mounted player - playerId({player.Id})");
        }

        public void SendPacket(Packet p)
        {
            if(_channel != null)
                _channel.WriteAndFlushAsync(p);
        }

        public void SendPing()
        {
            _ping.Time = TimeUtils.GetUtcNowSenconds();
            SendPacket(new Packet { Ping = _ping });
        }

        public void Unmount()
        {
            var tempPlayer = _player;
            if (_player == null)
                return;

            _player = null;
            _controllerSet.SetPlayer(null);

            logger.Info($"unmounted player - playerId({tempPlayer.Id})");

            tempPlayer.Destroy();
        }

        public void Update()
        {
        }

        public WorldPlayer Player => _player;
    }
}