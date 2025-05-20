using System;
using Unity.Collections;
using Unity.Netcode;

namespace _Scripts.LobbyScripts
{
    [Serializable]
    public struct PlayerData : INetworkSerializable
    {
        public FixedString64Bytes PlayerName;
        public FixedString64Bytes PlayerLobbyId;
        public ulong PlayerNetworkId;
        public bool IsHost;
        public bool IsReady;
        public ulong InGameObjectId;

        public PlayerData(
            FixedString64Bytes name,
            FixedString64Bytes lobbyId,
            ulong networkId,
            bool isHost,
            bool isReady,
            ulong inGameObjectId
        )
        {
            PlayerName = name;
            PlayerLobbyId = lobbyId;
            PlayerNetworkId = networkId;
            IsHost = isHost;
            IsReady = isReady;
            InGameObjectId = 0;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref PlayerLobbyId);
            serializer.SerializeValue(ref PlayerNetworkId);
            serializer.SerializeValue(ref IsHost);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref InGameObjectId);
        }
    }
}
