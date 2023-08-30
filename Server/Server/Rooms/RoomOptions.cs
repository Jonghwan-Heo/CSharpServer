using network;

namespace Server.Rooms
{
    public struct RoomOptions
    {
        private int _channelId;
        private int _mapId;
        private long _roomId;
        private RoomMode _roomMode;

        public int ChannelId => _channelId;

        public int MapId => _mapId;

        public long RoomId => _roomId;

        public RoomMode RoomMode => _roomMode;

        public RoomOptions(int channelId, long roomId, int mapId, RoomMode roomMode)
        {
            _channelId = channelId;
            _roomId = roomId;
            _mapId = mapId;
            _roomMode = roomMode;
        }

        public void SetChannelId(int value)
        {
            _channelId = value;
        }

        public void SetRoomId(long value)
        {
            _roomId = value;
        }
    }
}