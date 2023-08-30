using log4net;
using network;
using Server.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Components
{
    public class RoomPlayerComponent
    {
        private static readonly ILog logger = LogManager.GetLogger("", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly SynchronizedCollection<RoomPlayer> _players = new SynchronizedCollection<RoomPlayer>();
        private readonly Room _room;
        private RoomPlayer _masterPlayer;

        public RoomPlayer[] CopyPlayers => _players.ToArray();

        public int Count => _players.Count();

        public bool IsEmpty => _players.Any() == false;

        public RoomPlayer MasterPlayer => _masterPlayer;

        public long MasterPlayerId => _masterPlayer?.Id ?? -1L;

        public RoomPlayerComponent(Room room, RoomOptions roomOptions)
        {
            _room = room;
        }

        public void Add(RoomPlayer player)
        {
            _players.Add(player);
        }

        public RoomPlayer GetByUnitId(long unitId)
        {
            return _players.FirstOrDefault(i => i.Unit.Id == unitId);
        }

        public RoomPlayer GetById(long id)
        {
            return _players.FirstOrDefault(i => i.Id == id);
        }

        public void LeaveDisconnected()
        {
            foreach (RoomPlayer rp in _players)
            {
                // 끊긴 유저들을 내보낸다.
                if (rp.IsDisconnected())
                {
                    _room.LeavePlayer(rp.WorldPlayer, false);
                }
            }
        }

        public RoomPlayer RemoveById(long playerId)
        {
            RoomPlayer removed = null;
            foreach (RoomPlayer erp in _players)
            {
                if (erp.Id == playerId)
                {
                    removed = erp;
                    _players.Remove(erp);
                    break;
                }
            }

            return removed;
        }

        public void SendPacket(Packet p, RoomPlayer without = null)
        {
            if (_players.Count == 0)
                return;

            foreach (var e in _players)
            {
                if (e != without)
                    e.SendPacket(p);
            }
        }

        public void SetMaster(WorldPlayer player, RoomPlayer roomPlayer)
        {
            if (_masterPlayer == null)
                _masterPlayer = roomPlayer;
        }

        public void Update(float dt)
        {
            foreach (RoomPlayer rp in _players)
            {
                if (_room != rp.WorldPlayer.Room)
                {
                    continue;
                }

                try
                {
                    rp.Update(dt);
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
        }
    }
}