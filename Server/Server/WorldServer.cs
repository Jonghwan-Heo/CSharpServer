using Core;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using log4net;
using network;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Server
{
    public class WorldServer
    {
        private static readonly WorldServer _singleton = new WorldServer();
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static ConcurrentQueue<MemoryStream> _smallPools = new ConcurrentQueue<MemoryStream>();
        private IEventLoopGroup _bossGroup;
        private IChannel _channel;
        private IEventLoopGroup _workerGroup;

        private WorldServer()
        {
            RootDir = Directory.GetCurrentDirectory();
            Key = 0x98;
        }

        public static byte[] MakeXor(byte[] InputBuffer, int startIndex = 0, int count = -1)
        {
            if (count < 0)
                count = InputBuffer.Length;

            //
            for (int i = startIndex; i < startIndex + count; i++)
                InputBuffer[i] = (byte)(InputBuffer[i] ^ Key);
            return InputBuffer;
        }

        public void Start(int port, bool localServer)
        {
            if (_channel != null)
                return;

            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            LocalServer = localServer;

            SWorld.Initialize(1, 1);

            if (Config.IsLinux)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                _bossGroup = dispatcher;
                _workerGroup = new WorkerEventLoopGroup(dispatcher, 4);
            }
            else
            {
                _bossGroup = new MultithreadEventLoopGroup(1);
                _workerGroup = new MultithreadEventLoopGroup(4);
            }

            var bootstrap = new ServerBootstrap();
            bootstrap.Group(_bossGroup, _workerGroup);

            if (Config.IsLinux)
                bootstrap.Channel<TcpServerChannel>();
            else
                bootstrap.Channel<TcpServerSocketChannel>();

            bootstrap
               .Option(ChannelOption.SoBacklog, 1000)
               .Option(ChannelOption.TcpNodelay, true)
               .Option(ChannelOption.SoReuseaddr, true)
               .ChildOption(ChannelOption.SoReuseaddr, true)

               // 서버에서 수많은 UnitMove 패킷이 클라로 날라가는데 왠만하면 묶는게 좋다.
               .ChildOption(ChannelOption.TcpNodelay, false)
               .ChildOption(ChannelOption.SoKeepalive, true)
               //.Handler(new LoggingHandler("", LogLevel.INFO))
               .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
               {
                   IChannelPipeline p = channel.Pipeline;
                   p.AddLast(new IdleStateHandler(120, 0, 0));
                   p.AddLast("frameDecoder", new LengthFieldBasedFrameDecoder(204800, 0, 4, 0, 4));
                   p.AddLast("decoder", new MyProtobufDecoder(Key));

                   //
                   p.AddLast("frameEncoder", new LengthFieldPrepender(4));
                   p.AddLast("encoder", new MyProtobufEncoder(Key));

                   //
                   p.AddLast("flushHandler", new FlushConsolidationHandler());
                   p.AddLast("handler", new WorldServerHandler());
               }));

            bootstrap.BindAsync(port).ContinueWith(t =>
            {
                _channel = t.Result;
            }).Wait();

            logger.Info("Server is listening on " + port + "...");
        }

        public void Stop()
        {
            _channel?.CloseAsync().Wait();
            _channel = null;

            _bossGroup?.ShutdownGracefullyAsync();
            _bossGroup = null;
            _workerGroup?.ShutdownGracefullyAsync();
            _workerGroup = null;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }

        public static WorldServer INSTANCE => _singleton;
        public static byte Key { get; set; }
        public bool LocalServer { private set; get; }
        public string RootDir { get; set; }
    }
}