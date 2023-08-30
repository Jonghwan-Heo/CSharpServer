using Core;
using log4net;
using network;
using Server.Rooms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Server.Components
{
    public class WorldRoomComponent
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConcurrentDictionary<long, Room> _roomMap;
        private readonly SynchronizedCollection<RoomUpdater> _roomUpdaters;
        private Thread _threadShutdown;

        public WorldRoomComponent()
        {
            _roomMap = new ConcurrentDictionary<long, Room>();
            _roomUpdaters = new SynchronizedCollection<RoomUpdater>();
        }

        public Room CreateRoom(RoomOptions roomOptions)
        {
            int channelId = 1;
            roomOptions.SetChannelId(channelId);

            if (roomOptions.RoomId <= 0)
                roomOptions.SetRoomId(SWorld.INSTANCE.RoomUid.Next());

            var r = new Room(roomOptions);

            _roomUpdaters.OrderBy(x => x.Count).First().Add(r);
            _roomMap.TryAdd(r.Id, r);

            return r;
        }

        public Room GetRoom(long id)
        {
            return _roomMap.TryGetValue(id, out var room) ? room : null;
        }

        public IEnumerable<Room> GetRooms()
        {
            return _roomMap.Values;
        }

        public StatusCode JoinQuicklyByMapID(ulong packetID, WorldPlayer player, int mapID, string spawner = "")
        {
            StatusCode status;
            var room = _roomMap.FirstOrDefault(i => i.Value.Field.MainField.MapDataId == mapID);
            status = room.Value.JoinPlayer(0, player, 0, spawner);
            return status;
        }

        public void Shutdown()
        {
            logger.Info("RoomManager shutdown!");

            // 프로세스 종료하기
            Environment.Exit(0);
        }

        public void Shutdown(int seconds)
        {
            logger.Info("RoomManager shutdown! seconds=" + seconds);

            _threadShutdown = null;

            if (seconds < 0)
                return;

            _threadShutdown = new Thread(delegate (object val)
            {
                for (int i = 0; i < seconds;)
                {
                    if (_threadShutdown != val)
                        return;

                    Thread.Sleep(5000);
                    i += 5;
                }

                Shutdown();
                Environment.Exit(0);
            });
            _threadShutdown.Start(_threadShutdown);
        }

        public void Start()
        {
            int threadCount = Config.Debug ? 1 : Math.Max(4, Environment.ProcessorCount / 2);

            for (int i = 0; i < threadCount; Interlocked.Increment(ref i))
                _roomUpdaters.Add(new RoomUpdater());

            logger.Info($"RoomManager is running... with room updates = {_roomUpdaters.Count}");

            CreateFirstRooms();
        }

        private void CreateFirstRooms()
        {
            var roomOptions = new RoomOptions(1, 1, 1, RoomMode.Squre);
            CreateRoom(roomOptions);
            roomOptions = new RoomOptions(1, 2, 2, RoomMode.OXQuiz);
            CreateRoom(roomOptions);
            roomOptions = new RoomOptions(1, 3, 3, RoomMode.Museum);
            CreateRoom(roomOptions);
        }
    }
}