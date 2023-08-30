using Core;
using log4net;
using network;
using Server.Components;
using Server.Core;
using Server.Rooms;
using Shared.Enums;
using System;
using System.Numerics;

namespace Server.Units
{
    public class MovableUnit
    {
        public readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        private long _id;
        protected static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private UnitHealthComponent _health;
        private Vector3 _direction;
        private Field _field;
        private bool _isDestroyed;
        private Vector3 _position;
        private float _velocityXZ;
        private float _velocityY;
        protected string _name;

        public long Id => _id;
        public bool Destroyed => _isDestroyed;
        public Vector3 Direction => _direction;
        public Field Field => _field;
        public bool HasForceTargetPos => false;
        public UnitHealthComponent Health => _health;
        public Vector3 Position => _position;
        public virtual UnitType UnitType => UnitType.Player;
        public float VelocityXZ => _velocityXZ;
        public float VelocityY => _velocityY;
        public string Name => _name;

        public MovableUnit()
        {
        }

        public MovableUnit(Room room)
        {
            InitMovableUnit(room);
        }

        // 오브젝트풀 생성 초기화
        public void InitMovableUnit(Room room)
        {
            _id = SWorld.INSTANCE.InstanceUid.Next();

            _position = new Vector3(0, 0, 0);
            _direction = new Vector3(0, 0, 0);

            if (_health == null)
                _health = new UnitHealthComponent(this);
        }

        public virtual void Clear()
        {
            _health.Clear();

            _direction = new Vector3(0, 0, 0);
            _field = null;
            _isDestroyed = false;
            _position = new Vector3(0, 0, 0);
        }

        public static bool operator !=(MovableUnit lhs, MovableUnit rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return false;
                }

                // Only the left side is null.
                return true;
            }
            // Equals handles case of null on right side.
            return !lhs.Equals(rhs);
        }

        public static bool operator ==(MovableUnit lhs, MovableUnit rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public void AddPosition(Vector3 moveVector)
        {
            _position += moveVector;
        }

        public void AddPosition(float posX, float posZ)
        {
            _position.X += posX;
            _position.Y += posZ;
        }

        public virtual void ClearEntered()
        {
        }

        public virtual void Destroy()
        {
            LeaveField();
            _isDestroyed = true;
        }

        public override bool Equals(object obj)
        {
            if (obj is MovableUnit movableUnit)
            {
                if (this.UnitType == movableUnit.UnitType && this._id == movableUnit._id)
                    return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public virtual void HandleDead(MovableUnit attacker)
        {
            _health.SetDead(true);
            SendUnitUpdated();
        }

        public virtual void HandleFieldJoined(Field field)
        {
            _field = field;
        }

        public virtual void HandleFieldLeft(Field field)
        {
            if (_field == field)
                _field = null;
        }

        public virtual bool HandleTouched(MovableUnit sender, bool click)
        {
            return false;
        }

        public virtual void LeaveField()
        {
            if (Field == null)
                return;

            Field.LeaveUnit(this);
        }

        public virtual void LookAt(Vector3 v)
        {
            var prevDirection = Direction;
            SetDirection(v - _position);
            if (
                Math.Abs(Direction.X - prevDirection.X) > float.Epsilon
                || Math.Abs(Direction.Y - prevDirection.Y) > float.Epsilon
                || Math.Abs(Direction.Z - prevDirection.Z) > float.Epsilon
                )
                SendUnitMove(false);
        }

        public virtual void SendFieldPacket(Packet packet, MovableUnit without)
        {
            if (_field == null)
                return;

            _field.SendPacket(packet, without);
        }

        public void SendUnitMove(bool fixPosition, MovableUnit without = null)
        {
            if (Field == null)
                return;

            SendFieldPacket(new Packet
            {
                UnitMove = new UnitMove
                {
                    UnitId = _id,
                    Time = TimeUtils.GetUtcNowSenconds(),
                    Position = Position.ToNetworkVector(),
                    Direction = Position.ToNetworkVector(),
                    VelocityXZ = _velocityXZ,
                    VelocityY = _velocityY,
                    FixedPosition = fixPosition,
                }
            }, without);
        }

        public void SendUnitUpdated(uint flags = 0, bool fixPosition = false)
        {
            if (Field != null)
            {
                SendFieldPacket(new Packet
                {
                    UnitUpdated = new UnitUpdated
                    {
                        NetworkUnit = GetNetworkUnit(flags),
                        FixedPosition = fixPosition,
                    }
                }, null);
            }
        }

        public virtual NetworkUnit GetNetworkUnit(uint flags = 0)
        {
            var tunit = new NetworkUnit()
            {
                Id = _id,
                Name = _name ?? string.Empty,
                CharacterInformationJson = string.Empty,
                Position = _position.ToNetworkVector(),
                Direction = _direction.ToNetworkVector(),
            };

            return tunit;
        }

        public void SetDead()
        {
            _health.ResetHpZero();
            HandleDead(null);
        }

        public void SetDirection(Vector3 dir)
        {
            SetDirection(dir.X, dir.Z);
        }

        public void SetDirection(float dirX, float dirZ)
        {
            if (Math.Abs(dirX) > Math.Abs(dirZ))
            {
                if (0f < dirX)
                {
                    _direction = VectorUtils.Right;
                }
                else
                {
                    _direction = VectorUtils.Left;
                }
            }
            else
            {
                if (0f < dirZ)
                {
                    _direction = VectorUtils.Forward;
                }
                else
                {
                    _direction = VectorUtils.Back;
                }
            }
        }

        public virtual void SetPosition(Vector3 pos)
        {
            _position = pos;
        }

        public void SetVelocity(float velocity)
        {
            _velocityXZ = velocity;
        }

        public virtual void Update(float dt)
        {
        }
    }
}
