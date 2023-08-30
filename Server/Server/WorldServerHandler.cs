using Amib.Threading;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using log4net;
using network;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace Server
{
    public class WorldServerHandler : ChannelHandlerAdapter
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static SmartThreadPool _threadPoolHandler;
        private static SmartThreadPool _threadPoolLoginHandler;

        private WorldSession _session;

        static WorldServerHandler()
        {
            _threadPoolHandler = new SmartThreadPool();
            _threadPoolLoginHandler = new SmartThreadPool();

            _threadPoolHandler.MinThreads = 4;
            _threadPoolHandler.MaxThreads = 4;
            _threadPoolLoginHandler.MinThreads = 2;
            _threadPoolLoginHandler.MaxThreads = 16;

            //_threadPoolLoginHandler.MinThreads = 1;
            //_threadPoolLoginHandler.MaxThreads = 1;
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            base.ChannelActive(ctx);
            _session = new WorldSession(ctx.Channel);
            SWorld.INSTANCE.Session.Add(_session);
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            base.ChannelInactive(ctx);
            SWorld.INSTANCE.Session.Remove(_session);

            _session?.Close();
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            if (_session == null)
                return;

            var packet = message as Packet;
            //ProcessHandlePacket(_session, packet);

            if (packet.MessageCase == Packet.MessageOneofCase.UnitMove)
                ProcessHandlePacket(_session, packet).Wait();
            else if (packet.MessageCase == Packet.MessageOneofCase.Login)
                _threadPoolLoginHandler.QueueWorkItem(ProcessHandlePacket, _session, packet);
            else
                _threadPoolHandler.QueueWorkItem(ProcessHandlePacket, _session, packet);
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception exception)
        {
            if (exception is SocketException
                || exception is ChannelException)
                return;

            logger.Warn("WorldServerHandler.ExceptionCaught", exception);
        }

        public override void UserEventTriggered(IChannelHandlerContext ctx, object evt)
        {
            if (evt is IdleStateEvent e)
            {
                if (e.State == IdleState.ReaderIdle)
                    ctx.CloseAsync();
            }
        }

        private static async Task ProcessHandlePacket(WorldSession s, Packet p)
        {
            try
            {
                await s.HandlePacket(p);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to handle packet.", e);
            }
        }
    }
}