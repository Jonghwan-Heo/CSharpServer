using network;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers
{
    public abstract class BaseController
    {
        protected readonly WorldSession _session;
        protected WorldPlayer _player;

        public BaseController(WorldSession session)
        {
            this._session = session;
        }

        public abstract Dictionary<Packet.MessageOneofCase, Func<Packet, Task>> GetHandlers();

        public void SetPlayer(WorldPlayer player)
        {
            this._player = player;
        }
    }
}