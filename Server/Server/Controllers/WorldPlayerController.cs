using Core;
using log4net;
using network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class WorldPlayerController : BaseController
    {
        private readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WorldPlayerController(WorldSession session) : base(session)
        {
        }

        public override Dictionary<Packet.MessageOneofCase, Func<Packet, Task>> GetHandlers()
        {
            return new Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>
            {
                { Packet.MessageOneofCase.Ping, HandlePing },
                { Packet.MessageOneofCase.UnitMove, HandleUnitMove },
                { Packet.MessageOneofCase.ChatMessage, HandleChatMessage },
                { Packet.MessageOneofCase.RoomJoin, HandleRoomJoin },
            };
        }

        public async Task HandleUnitMove(Packet p)
        {
            await Task.CompletedTask;

            if (_player == null)
                return;

            var unitMove = p.UnitMove;
            _player.RoomPlayer.Unit.HandleUnitMove(unitMove);
        }

        public async Task HandlePing(Packet p)
        {
            await Task.CompletedTask;

            if (_player == null)
                return;

            var ping = p.Ping;

            _player.HandlePing(ping);
        }

        public async Task HandleChatMessage(Packet p)
        {
            await Task.CompletedTask;

            if (_player == null)
                return;

            var chatMessage = p.ChatMessage;
            _player.RoomPlayer.Unit.HandleChatMessage(chatMessage);
        }

        public async Task HandleRoomJoin(Packet p)
        {
            await Task.CompletedTask;

            if (_player == null)
                return;

            var packet = p.RoomJoin;
            SWorld.INSTANCE.Room.JoinQuicklyByMapID(p.Id, _player, packet.MapId);
        }
    }
}