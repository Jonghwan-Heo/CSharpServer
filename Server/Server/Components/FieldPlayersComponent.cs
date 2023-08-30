using log4net;
using Server.Rooms;
using Server.Units;
using System.Collections.Generic;
using System.Linq;

namespace Server.Components
{
    public class FieldPlayersComponent
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Field _field;
        private List<PlayerUnit> _playerUnits = new List<PlayerUnit>();

        public int Count => _playerUnits.Count();
        public IEnumerable<PlayerUnit> PlayerUnits => _playerUnits;

        public FieldPlayersComponent(Field field)
        {
            _field = field;
        }

        public void AddUnit(PlayerUnit unit)
        {
            _playerUnits.Add(unit);
        }

        public PlayerUnit GetByUnitId(long unitId)
        {
            return _playerUnits.FirstOrDefault(i => i.Id == unitId);
        }

        public void RemoveUnit(PlayerUnit unit)
        {
            _playerUnits.Remove(unit);
        }

        public void Update(float dt)
        {
        }
    }
}