using Core;
using log4net;
using network;
using Server.Components;
using Server.Core;
using Server.Units;
using System.Linq;
using System.Numerics;

namespace Server.Rooms
{
    public class Field
    {
        public readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        public readonly long id;
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly FieldPlayersComponent _players;
        private readonly FieldUnitComponent _unit;
        private bool _destroyed;
        private Room _room;
        private int _mapDataId;

        public FieldPlayersComponent Players => _players;
        public Room Room => _room;
        public FieldUnitComponent Unit => _unit;
        public int MapDataId => _mapDataId;

        public Field(Room room, int dataId)
        {
            id = SWorld.INSTANCE.InstanceUid.Next();
            _mapDataId = dataId;

            _room = room;
            _players = new FieldPlayersComponent(this);
            _unit = new FieldUnitComponent(this);
        }

        public virtual void Destroy()
        {
            // 플레이어 유닛을 제외한 나머지 유닛의 Callback 호춣
            foreach (var u in Unit.Units)
            {
                if (null == u)
                    continue;

                LeaveUnit(u);
            }

            _destroyed = true;
        }

        public bool JoinUnit(MovableUnit unit, Vector3? position = null, string spawner = "")
        {
            var firstJoined = Unit.GetUnitByID(unit.Id) == null;
            if (firstJoined)
            {
                if (unit.Destroyed)
                    return false;

                // 이미 파괴된 필드
                if (_destroyed)
                    return false;

                // 기존 필드 나오기
                unit.LeaveField();
            }

            using (@lock.Enter())
            {
                // 이 필드에 처음 접속한 경우,
                if (firstJoined)
                {
                    if (unit is PlayerUnit playerUnit)
                    {
                        Players.AddUnit(playerUnit);
                    }

                    Unit.AddUnit(unit);
                }

                unit.SetPosition(position.Value);

                unit.HandleFieldJoined(this);

                if (unit is PlayerUnit pu)
                {
                    var l = new WelcomeField()
                    {
                        MyUnit = unit.GetNetworkUnit(),
                    };

                    foreach (var u in Unit.Units)
                    {
                        if (u != unit)
                            l.NetworkUnits.Add(u.GetNetworkUnit());
                    }

                    if (position != null)
                        l.MyUnit.Position = position != null ? position.Value.ToNetworkVector() : VectorUtils.Zero.ToNetworkVector();

                    HandleWelcomeField(l);

                    pu.SendPacket(new Packet { WelcomeField = l });
                }

                var networkUnit = unit.GetNetworkUnit();
                SendPacket(new Packet
                {
                    UnitJoined = new UnitJoined
                    {
                        NetworkUnit = networkUnit,
                    }
                }, unit);
            }

            return true;
        }

        public void LeaveUnit(MovableUnit unit)
        {
            using (@lock.Enter())
            {
                if (null == unit)
                    return;

                Unit.RemoveUnit(unit);

                if (unit is PlayerUnit)
                {
                    Players.RemoveUnit(unit as PlayerUnit);
                }

                unit.HandleFieldLeft(this);

                SendPacket(new Packet
                {
                    UnitLeft = new UnitLeft
                    {
                        UnitId = unit.Id,
                    }
                }, unit);
            }
        }

        public void SendPacket(Packet p, MovableUnit without = null)
        {
            if (Players.Count <= 0)
                return;

            foreach (var u in Players.PlayerUnits)
            {
                if (u != without)
                    u.SendPacket(p);
            }
        }

        public virtual void Update(float dt)
        {
            UpdateUnits(dt);
            Players.Update(dt);
        }

        protected virtual void HandleWelcomeField(WelcomeField l)
        {
        }

        private void UpdateUnits(float dt)
        {
            foreach (var u in Unit.Units)
                u.Update(dt);
        }
    }
}