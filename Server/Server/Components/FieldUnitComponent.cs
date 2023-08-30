using Server.Rooms;
using Server.Units;
using System.Collections.Generic;
using System.Linq;

namespace Server.Components
{
    public class FieldUnitComponent
    {
        private readonly Field _field;

        private readonly SynchronizedCollection<MovableUnit> _units = new SynchronizedCollection<MovableUnit>();

        public IEnumerable<MovableUnit> Units => _units.ToArray();

        public FieldUnitComponent(Field field)
        {
            _field = field;
        }

        public void AddUnit(MovableUnit unit)
        {
            _units.Add(unit);
        }

        public MovableUnit GetUnitByID(long id)
        {
            return _units.FirstOrDefault(x => x.Id == id);
        }

        public void RemoveUnit(MovableUnit unit)
        {
            _units.Remove(unit);
        }
    }
}