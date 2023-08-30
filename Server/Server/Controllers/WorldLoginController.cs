using Core;
using log4net;
using network;
using Newtonsoft.Json.Linq;
using Server.Core;
using Server.Data.Enum;
using Server.Scraps;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public class WorldLoginController : BaseController
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WorldLoginController(WorldSession session) : base(session)
        {
        }

        public override Dictionary<Packet.MessageOneofCase, Func<Packet, Task>> GetHandlers()
        {
            return new Dictionary<Packet.MessageOneofCase, Func<Packet, Task>>
            {
                { Packet.MessageOneofCase.Login, HandleLogin },
                { Packet.MessageOneofCase.Disconnected, HandleDisconnected },
            };
        }

        public async Task HandleDisconnected(Packet p)
        {
            await Task.CompletedTask;

            if (_player == null)
                return;

            _player.Destroy();
            _session.Unmount();
            _session.Close();
        }

        public async Task HandleLogin(Packet p)
        {
            await Task.CompletedTask;

            var login = p.Login;

            if (string.IsNullOrEmpty(login.JwtToken))
            {
                _session.Close();
                return;
            }

            _session.Mount(SWorld.INSTANCE.Player.Login(_session, login.JwtToken, login.JwtToken, "", ""));
            _session.SendPacket(new Packet
            {
                Id = p.Id,
                LoginResult = new LoginResult
                {
                    HashId = _player.HashId,
                    //NetworkUnit = _player
                    Status = (int)StatusCode.Success,
                }
            });

            _session.SendPing();

            if (Config.Debug)
                logger.Info($"Player #{_player.Id} logined. (JwtToken = {login.JwtToken})");

            SWorld.INSTANCE.Room.JoinQuicklyByMapID(0, _player, 1);
        }
    }
}