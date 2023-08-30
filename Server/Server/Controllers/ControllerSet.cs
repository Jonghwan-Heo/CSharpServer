using log4net;
using network;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class ControllerSet
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<Type, BaseController> controllers;
        private readonly Dictionary<Packet.MessageOneofCase, Func<Packet, Task>> responsors;
        private readonly WorldSession session;

        public ControllerSet(WorldSession session)
        {
            IDictionary<BaseController, Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>> entries = new Dictionary<BaseController, Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>>();
            
            // TODO : 기능별로 정리 대상
            entries.Add(ComposeController(session, new WorldLoginController(session)));
            entries.Add(ComposeController(session, new WorldPlayerController(session)));

            var controllers = new Dictionary<Type, BaseController>();
            var responsors = new Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>();

            foreach (var controller in entries.Keys)
            {
                controllers.Add(controller.GetType(), controller);
            }

            foreach (Dictionary<Packet.MessageOneofCase, Func<Packet, Task>> composedResponsors in entries.Values)
            {
                foreach ((Packet.MessageOneofCase type, Func<Packet, Task> action) in composedResponsors)
                {
                    responsors.Add(type, action);
                }
            }

            this.session = session;
            this.controllers = controllers;
            this.responsors = responsors;
        }

        public void SetPlayer(WorldPlayer player)
        {
            foreach ((_, var controller) in controllers)
            {
                controller.SetPlayer(player);
            }
        }

        public async Task Response(Packet packet)
        {
            var exist = responsors.TryGetValue(packet.MessageCase, out var action);
            if (exist == false)
            {
                logger.Error($"there is no handler - key({packet.MessageCase})");
                return;
            }

            await action.Invoke(packet);
        }

        private KeyValuePair<BaseController, Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>> ComposeController(WorldSession session, BaseController controller)
        {
            var composedResponsors = new Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>();

            foreach (var (type, action) in controller.GetHandlers())
            {
                var composedResponsor = MakerResponsor(session, action);
                var old = composedResponsors.TryAdd(type, composedResponsor);
                if (old == false)
                    logger.Error($"there is already a handler - key({type})");
            }

            return new KeyValuePair<BaseController, Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>>(controller, composedResponsors);
        }

        private Func<Packet, Task> MakerResponsor(WorldSession session, Func<Packet, Task> action)
        {
            return async packet =>
            {
#if DEBUG
                if (packet.MessageCase != Packet.MessageOneofCase.Ping
                && packet.MessageCase != Packet.MessageOneofCase.UnitMove)
                {
                    if (session.Player == null)
                    {
                        logger.Info($"World GotPacket type={packet.MessageCase} No Session Player");
                    }
                    else
                        logger.Info($"World GotPacket type={packet.MessageCase} playerID={session.Player.Id}");
                }
#endif
                await action.Invoke(packet);
            };
        }
    }
}