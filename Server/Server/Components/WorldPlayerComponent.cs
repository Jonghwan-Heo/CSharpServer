using Core;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Server.Components
{
    public class WorldPlayerComponent
    {
        public readonly DisposableTimeoutLock @lock = new DisposableTimeoutLock();
        public readonly IDictionary<long, WorldPlayer> playerMap = new ConcurrentDictionary<long, WorldPlayer>();
        public readonly IDictionary<string, WorldPlayer> playerHashIdMap = new ConcurrentDictionary<string, WorldPlayer>();
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Thread _threadShutdown;

        private Thread threadUpdates;

        private ServerUIDTransient _serverUIDTransient;

        public WorldPlayerComponent()
        {
            _serverUIDTransient = new ServerUIDTransient(0, ServerUIDTransientType.None);
        }

        public void DestroyPlayer(WorldPlayer player)
        {
            if (player == null)
                return;

            playerMap.Remove(player.Id);
            playerHashIdMap.Remove(player.HashId);
        }

        public WorldPlayer GetPlayerById(long id)
        {
            playerMap.TryGetValue(id, out var p);
            return p;
        }

        public WorldPlayer GetPlayerBySNSID(string snsID)
        {
            playerHashIdMap.TryGetValue(snsID, out var p);
            return p;
        }

        public WorldPlayer Login(WorldSession session, string jwtToken, string hashId, string name, string avatar)
        {
            using (@lock.Enter())
            {
                if (playerHashIdMap.TryGetValue(hashId, out WorldPlayer p))
                {
                    p.HandleSessionChanged(session);
                    return p;
                }

                var id = SWorld.INSTANCE.InstanceUid.Next();

                if (string.IsNullOrEmpty(name))
                    name = $"익명의사용자{id}";

                playerMap[id] = p = new WorldPlayer(session);
                p.Initialize(id, name, hashId, jwtToken, avatar);
                playerHashIdMap[hashId] = p;

                return p;
            }
        }

        public void SaveAll()
        {
            foreach (var p in playerMap)
            {
                try
                {
                    p.Value.Save();
                }
                catch (Exception ex)
                {
                    logger.Warn("", ex);
                }
            }
        }

        public void Shutdown()
        {
            // 프로세스 종료하기
            Environment.Exit(0);
        }

        public void Shutdown(int seconds)
        {
            logger.Info("WorldManager shutdown! seconds=" + seconds);

            _threadShutdown = null;

            //
            if (seconds < 0)
                return;

            _threadShutdown = new Thread(delegate (object val)
            {
                for (int i = 0; i < seconds;)
                {
                    if (_threadShutdown != val)
                        return;

                    //foreach (var e in playerMap)
                    //    e.Value.SendCenterLabel("Center_ServerShutdown", seconds - i);

                    Thread.Sleep(5000);
                    i += 5;
                }

                SaveAll();

                // 게임서버에서 내보내기
                foreach (var e in playerMap)
                    e.Value.SendClose();

                // 재시작 요청
                Shutdown();
            });
            _threadShutdown.Start(_threadShutdown);
        }

        public void Start()
        {
            if (threadUpdates != null)
                return;

            threadUpdates = new Thread(delegate (object val)
            {
                var lastTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                while (threadUpdates != null)
                {
                    var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    var dt = (now - lastTime) / 1000f;
                    lastTime = now;

                    try
                    {
                        Update(dt);

                        int latency = (int)(dt * 1000f);
                        int delay = Math.Max(1000 - latency, 0);
                        Thread.Sleep(delay);
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e);
                    }
                }
            });
            threadUpdates.Start();
        }

        public void Stop()
        {
            threadUpdates = null;
        }

        private void Update(float dt)
        {
            foreach (var p in playerMap)
                p.Value.Update(dt);
        }
    }
}