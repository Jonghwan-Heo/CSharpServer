using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Server.Components
{
    public class WorldSessionComponent
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _packetGatheringPerTick = 100;

        private bool _isRun = false;
        private Thread _thread;
        private SynchronizedCollection<WorldSession> _worldSessions = new SynchronizedCollection<WorldSession>();

        public WorldSessionComponent()
        {
        }

        public void Add(WorldSession session)
        {
            _worldSessions.Add(session);
        }

        public void Remove(WorldSession session)
        {
            _worldSessions.Remove(session);
            session.Unmount();
            session.Close();
        }

        public void Start()
        {
            if (_thread != null)
                return;

            _isRun = true;

            _thread = new Thread(delegate (object val)
            {
                while (_isRun)
                {
                    var startTick = Environment.TickCount;

                    Update();

                    // 게더링 처리 시간 제외한 sleep
                    var deltaTick = Environment.TickCount - startTick;
                    var sleepTick = _packetGatheringPerTick - deltaTick;
                    if (sleepTick < 0)
                    {
                        sleepTick = 0;
                    }
                    Thread.Sleep(sleepTick);
                }
            });
            logger.Info("WorldSessionComponent 1 thread start.");
            _thread.Start();
        }

        public void Stop()
        {
            _isRun = false;
        }

        public void Update()
        {
            foreach (var session in _worldSessions)
            {
                session.Update();
            }
        }
    }
}