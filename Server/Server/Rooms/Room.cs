using Core;
using log4net;
using network;
using Server.Components;

namespace Server.Rooms
{
    public class Room
    {
        public readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool _destroyed;
        private long _id;
        private RoomModeComponent _mode;
        private RoomFieldComponent _field;
        private RoomPlayerComponent _player;
        public long Id => _id;
        public RoomFieldComponent Field => _field;
        public RoomPlayerComponent Player => _player;
        public RoomModeComponent Mode => _mode;

        public Room(RoomOptions roomOptions)
        {
            _id = roomOptions.RoomId;
            _mode = roomOptions.RoomMode switch
            {
                RoomMode.Squre => new RoomSqureModeComponent(this, roomOptions),
                RoomMode.OXQuiz => new RoomOxQuizModeComponent(this, roomOptions),
                RoomMode.Museum => new RoomMuseumModeComponent(this, roomOptions),
                _ => new RoomSqureModeComponent(this, roomOptions),
            };
            _field = new RoomFieldComponent(this, roomOptions);
            _player = new RoomPlayerComponent(this, roomOptions);
        }

        public void Destroy()
        {
            _destroyed = true;

            Field.DestroyAll();
        }

        public StatusCode JoinPlayer(ulong packetID, WorldPlayer player, uint deckID = 0, string spawner = "")
        {
            bool firstJoined;
            RoomPlayer rp;
            var roomMode = RoomMode.None;

            using (player.@lockJoinRoom.Enter())
            using (@lock.Enter())
            {
                // 이미 파괴된 방
                if (_destroyed)
                    return StatusCode.Failed;

                rp = Player.GetById(player.Id);
                firstJoined = rp == null;

                if (firstJoined)
                {
                    if (player.Room != null && player.Room._destroyed == false)
                    {
                        roomMode = player.Room.Mode.RoomMode;
                        player.Room.LeavePlayer(player);
                    }

                    rp = new RoomPlayer(player, this);
                    Player.Add(rp);
                }
                else
                {
                }

                Player.SetMaster(player, rp);
                player.JoinRoom(this, rp);
                player.SendPacket(new Packet
                {
                    Id = packetID,
                    RoomJoinResult = new RoomJoinResult()
                    {
                        Room = GetNetworkRoom(),
                        StatusCode = StatusCode.Success
                    }
                });
            }

            if (firstJoined || rp.Unit.Field == null)
            {
                Field.MainField.JoinUnit(rp.Unit, Mode.GetSpawnPosition(roomMode), spawner);
            }
            else if (rp.Unit.Field != null)
            {
                rp.Unit.Field.JoinUnit(rp.Unit, rp.Unit.Position);
            }

            return 0;
        }

        public bool LeavePlayer(WorldPlayer player, bool send = true)
        {
            RoomPlayer rp;

            using (@lock.Enter())
            {
                rp = Player.GetById(player.Id);
                if (rp == null)
                    return false;

                player.HandleLeftRoom(this, rp);

                rp.Destroy();
                Player.RemoveById(player.Id);
                player.SendPacket(new Packet() { RoomLeave = new RoomLeave() });
            }

            return true;
        }

        public void Update(float dt)
        {
            Field.Update(dt);

            if (Player.IsEmpty == false)
                Player.Update(dt);
        }

        public NetworkRoom GetNetworkRoom()
        {
            return new NetworkRoom()
            {
                Id = _id,
                RoomMode = Mode.RoomMode,
            };
        }
    }
}