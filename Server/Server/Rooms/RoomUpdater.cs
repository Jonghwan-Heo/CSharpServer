using log4net;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.Rooms
{
    public class RoomUpdater
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private long _lastTime;
        private ConcurrentDictionary<Room, byte> _rooms;
        private Thread _thread;
        public int Count => _rooms.Count;

        public RoomUpdater()
        {
            _rooms = new ConcurrentDictionary<Room, byte>();
            _thread = new Thread(Start);
            _thread.Start();
        }

        public bool Add(Room room)
        {
            if (null == _thread)
                return false;

            if (null == room)
                return false;

            return _rooms.TryAdd(room, 1);
        }

        public void Destory()
        {
            _thread = null;
        }

        public bool Remove(Room room)
        {
            return _rooms.TryRemove(room, out byte removed);
        }

        private void Start()
        {
            _lastTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            while (_thread != null)
            {
                long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                float dt = (now - _lastTime) / 1000f;
                _lastTime = now;

                try
                {
                    foreach (var r in _rooms)
                    {
                        try
                        {
                            r.Key.Update(dt);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex);
                        }
                    }

                    long endNow = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

                    int latency = (int)(endNow - now);
                    int delay = Math.Max(0, 100 - latency);

                    Thread.Sleep(delay);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex);
                }
            }
        }
    }
}