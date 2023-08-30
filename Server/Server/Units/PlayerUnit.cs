using Core;
using network;
using Server.Core;
using Server.Data.Enum;
using Server.Rooms;
using Server.Scraps;
using Shared.Enums;
using System;

namespace Server.Units
{
    public class PlayerUnit : MovableUnit
    {
        public readonly RoomPlayer RoomPlayer;
        public int FreeRevival;
        public double LastSpawnAt;
        public int PayRevival;
        private long _fieldJoinedAt;
        private CharacterInformation _avatar;

        public long FieldJoinedAt => _fieldJoinedAt;
        public override UnitType UnitType => UnitType.Player;
        public CharacterInformation Avatar => _avatar;

        public PlayerUnit(RoomPlayer player) : base(player.room)
        {
            _name = player.WorldPlayer.Name;
            _avatar = JsonHelper.FromJson<CharacterInformation>(player.WorldPlayer.Avatar);
            if (_avatar == null || (_avatar.unitDataSoId == 0 && string.IsNullOrEmpty(_avatar.nftCharTokenId)))
                _avatar = new CharacterInformation() { unitDataSoId = 0 };
            RoomPlayer = player;
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void HandleDead(MovableUnit attacker)
        {
            base.HandleDead(attacker);
        }

        public override void HandleFieldJoined(Field field)
        {
            base.HandleFieldJoined(field);
            LastSpawnAt = TimeUtils.GetUtcNowSenconds();
            _fieldJoinedAt = (int)TimeUtils.GetTimeMilliSecond();
        }

        public void HandleUnitMove(UnitMove packet)
        {
            using (@lock.Enter())
            {
                SetPosition(packet.Position.ToVector3());
                SetDirection(packet.Direction.ToVector3());
            }

            SendFieldPacket(new Packet { UnitMove = packet }, this);
        }

        public void HandleChatMessage(ChatMessage packet)
        {
            try
            {
                if (RoomPlayer.WorldPlayer.IsGM && packet.Content.StartsWith('/'))
                {
                    var args = packet.Content.Split(" ");
                    if (args[0] == "/a")
                    {
                        packet.Content = packet.Content.Split("/a ")[1];
                        packet.Announce = true;
                        SendFieldPacket(new Packet { ChatMessage = packet }, null);
                    }
                }
                else
                {
                    SendFieldPacket(new Packet { ChatMessage = packet }, null);
                }
            }
            catch (Exception e)
            {
                logger.Info(e);
            }
        }

        protected virtual void UpdateMove(float dt)
        {
        }

        public override void Update(float dt)
        {
            UpdateMove(dt);
            base.Update(dt);
        }

        public override void SendFieldPacket(Packet packet, MovableUnit without)
        {
            Field.SendPacket(packet, without);
        }

        public void SendPacket(Packet p)
        {
            RoomPlayer?.SendPacket(p);
        }

        public override NetworkUnit GetNetworkUnit(uint flags = 0)
        {
            var tunit = new NetworkUnit()
            {
                Id = Id,
                Name = _name ?? string.Empty,
                CharacterInformationJson = JsonHelper.ToJson(_avatar),
                Position = Position.ToNetworkVector(),
                Direction = Direction.ToNetworkVector(),
                HashId = RoomPlayer.WorldPlayer.HashId,
                IsGm = RoomPlayer.WorldPlayer.IsGM,
            };

            return tunit;
        }
    }
}