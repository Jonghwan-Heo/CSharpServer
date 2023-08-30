using System;
using System.Linq;
using System.Threading.Tasks;
using Core;
using log4net;
using network;
using Server.Data;
using Server.Rooms;

namespace Server
{
    interface IWorldPlayer
    {
        void Initialize(long id, string name, string hashId, string jwtToken, string avatar);
        void Save();
        void Update(float dt);
        void Destroy();
        void JoinRoom(Room room, RoomPlayer roomPlayer);

        void HandlePing(Ping packet);
        void HandleLeftRoom(Room room, RoomPlayer roomPlayer);
        void HandleSessionChanged(WorldSession newSession);

        void SendClose();
        void SendPacket(Packet p);
        void SendRandomEmotion();
    }

    public class WorldPlayer : IWorldPlayer
    {
        public readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        public readonly DisposableTimeoutLock @lockJoinRoom = new DisposableTimeoutLock();
        public int CurIndex = 0;
        public bool HasTicket = true;
        public bool? IsJoinMatching = null;
        public bool MatchingOrPlaying;
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _connectingTime;
        private bool _destroyed;
        private bool _initialize;
        private double _lastPingAt;
        private uint _latency;
        private float _pingTimer;
        private Room _room;
        private RoomPlayer _roomPlayer;
        private float _secondTimer;
        private WorldSession _session;
        private long _id;
        private string _hashId;
        private string _name;
        private string _jwtToken;
        private string _avatar;
        private bool _isGM;
        private long _pixelAmount;

        public string Avatar => _avatar;

        public long Id => _id;

        public string HashId => _hashId;

        public string Name => _name;

        public string JwtToken => _jwtToken;

        public bool Destroyed => _destroyed;

        public Room Room => _room;

        public RoomPlayer RoomPlayer => _roomPlayer;

        public WorldSession Session => _session;

        public bool IsGM => _isGM;

        public long PixelAmount => _pixelAmount;

        public WorldPlayer(WorldSession session)
        {
            _hashId = string.Empty;
            _session = session;
            _destroyed = false;
            _lastPingAt = TimeUtils.GetUtcNowSenconds();
        }

        public void Destroy()
        {
            if (_destroyed == false)
            {
                _destroyed = true;
                Save();
            }

            if (Room != null)
            {
                Room.LeavePlayer(this);
                _room = null;
            }

            SWorld.INSTANCE.Player.DestroyPlayer(this);
        }

        #region Base 
        public void Initialize(long id, string name, string hashId, string jwtToken, string avatar)
        {
            _id = id;
            _name = name;
            _hashId = hashId;
            _jwtToken = jwtToken;
            _avatar = avatar;

            if (DataConst.GmHashId.Contains(hashId))
                _isGM = true;

            _initialize = true;
        }

        public void Save()
        {
            // DB 직접 연결이 생길시 갑자기 연결 끊긴 유저의 정보를 저장하기 위함
        }

        public void Update(float dt)
        {
            if (_initialize == false)
                return;

            // 테스트를 위함, 원래는 60~120초로 조정
            if (TimeUtils.GetUtcNowSenconds() - _lastPingAt >= 360d)
                Destroy();

            _secondTimer += dt;
            if (_secondTimer >= 1f)
            {
                _connectingTime++;
                _secondTimer -= 1f;
            }

            // 10초마다 핑 보내기
            _pingTimer += dt;
            if (_pingTimer >= 10f)
            {
                _pingTimer = 0f;
                Session?.SendPing();
            }
        }
        #endregion

        #region Handle Function
        public virtual void HandleLeftRoom(Room room, RoomPlayer roomPlayer)
        {
            if (Room == room)
            {
                _room = null;
                _roomPlayer = null;
            }
        }

        public void HandlePing(Ping packet)
        {
            _lastPingAt = TimeUtils.GetUtcNowSenconds();
            _latency = (_latency + (uint)Math.Max(0, (_lastPingAt - packet.Time) * 1000)) / 2;
        }

        public void HandleSessionChanged(WorldSession newSession)
        {
            if (_session == newSession)
                return;

            var oldSession = _session;
            _session = newSession;

            // 기존 세션 닫기
            if (oldSession != null)
                oldSession.Close();

            // 핑 타임 업데이트
            if (newSession != null)
                _lastPingAt = TimeUtils.GetUtcNowSenconds();

            if (RoomPlayer != null)
                RoomPlayer.HandleSessionChanged();
        }

        #endregion

        #region Send Function
        public void SendPacket(Packet p)
        {
            _session?.SendPacket(p);
        }

        public void SendClose()
        {
            SendPacket(new Packet { Disconnected = new Disconnected() });
            Task.Delay(3000).ContinueWith(delegate
            {
                Session?.Close();
            });
        }

        public void SendRandomEmotion()
        {
            var rand = new Random();
            var actionIdx = rand.Next(1, 6);
            int emoticonIdx;
            if (actionIdx == 1)
            {
                emoticonIdx = 2;
            }
            else
            {
                emoticonIdx = 5;
            }
            Room.Field.MainField.SendPacket(new Packet()
            {
                //EmotionPlay = new EmotionPlay()
                //{
                //    UnitId = RoomPlayer.Unit.Id,
                //    SayEmotion = $"emoticon_{emoticonIdx:d3}",
                //    Emotion = $"etc_{actionIdx}",
                //}
            }); ;
        }

        //public void SendHttpRequestErrorPacket(ulong packetId, HttpRequestErrorCode errorCode)
        //{
        //    SendPacket(new Packet() { Id = packetId, HttpRequestError = new HttpRequestError() { HttpRequestErrorCode = errorCode } });
        //}
        #endregion

        public void JoinRoom(Room room, RoomPlayer roomPlayer)
        {
            _room = room;
            _roomPlayer = roomPlayer;
        }
    }
}