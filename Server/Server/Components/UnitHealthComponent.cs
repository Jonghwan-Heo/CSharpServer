using Server.Units;

namespace Server.Components
{
    public class UnitHealthComponent
    {
        private MovableUnit _unit;
        private long _barrier;
        private long _hp;
        private bool _isDead;
        private long _maxBarrier;
        private long _maxHp;
        public long Barrier => _barrier;
        public bool Dead => _isDead;
        public long Hp => _hp;
        public long MaxBarrier => _maxBarrier;
        public long MaxHp => _maxHp;

        public UnitHealthComponent(MovableUnit unit)
        {
            _unit = unit;
        }

        public void Init(MovableUnit unit)
        {
            _unit = unit;
        }

        public void Clear()
        {
            _unit = null;
            _barrier = 0;
            _hp = 0;
            _isDead = false;
            _maxBarrier = 0;
            _maxHp = 0;
        }

        public void ResetHpZero()
        {
            _hp = 0;
        }

        public void SetDead(bool dead)
        {
            _isDead = dead;
        }
    }
}