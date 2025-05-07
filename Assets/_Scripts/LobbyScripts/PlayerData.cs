using System;
using Unity.Collections;
using Unity.Netcode;

namespace _Scripts.LobbyScripts
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public FixedString64Bytes PlayerName;
        public FixedString64Bytes PlayerLobbyId;
        public ulong PlayerNetworkId;
        public bool IsHost;
        public bool IsReady;

        public PlayerData(
            FixedString64Bytes name,
            FixedString64Bytes lobbyId,
            ulong networkId,
            bool isHost,
            bool isReady
        )
        {
            PlayerName = name;
            PlayerLobbyId = lobbyId;
            PlayerNetworkId = networkId;
            IsHost = isHost;
            IsReady = isReady;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref PlayerLobbyId);
            serializer.SerializeValue(ref PlayerNetworkId);
        }

        public bool Equals(PlayerData other)
        {
            return PlayerNetworkId.Equals(other.PlayerNetworkId); // Or use all fields if needed
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return PlayerNetworkId.GetHashCode();
        }
    }
}
