using log4net;
using Server.Rooms;
using System;
using System.Collections.Concurrent;

namespace Server.Components
{
    public class RoomFieldComponent
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<long, Field> _fields;
        private Field _mainField;
        private Room _room;

        public Field MainField => _mainField;

        public RoomFieldComponent(Room room, RoomOptions roomOptions)
        {
            _room = room;
            _fields = new ConcurrentDictionary<long, Field>();

            _mainField = GetOrCreate(roomOptions.MapId);
        }

        public Field GetOrCreate(int dataID)
        {
            return _fields.GetOrAdd(dataID, dataId => new Field(_room, dataID));
        }

        public void DestroyAll()
        {
            foreach (var field in _fields.Values)
            {
                field.Destroy();
            }
            _fields.Clear();
        }

        public void Update(float dt)
        {
            foreach (var field in _fields.Values)
            {
                try
                {
                    field.Update(dt);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
        }
    }
}