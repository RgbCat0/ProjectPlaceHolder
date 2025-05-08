using System;
using System.Collections;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.LobbyScripts
{
    public class PlayerDataSync : NetworkBehaviour
    {
        public NetworkList<PlayerData> SyncedPlayerList;
        public event Action OnPlayerJoin;
        private bool _isActuallySpawned;

        private void Awake()
        {
            SyncedPlayerList = new NetworkList<PlayerData>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _isActuallySpawned = true;
            if (IsServer) { }
        }

        public IEnumerator RegisterPlayerServer(
            string playerName,
            string lobbyId,
            ulong networkId,
            bool isHost
        )
        {
            while (true)
            {
                if (!_isActuallySpawned)
                {
                    yield return null;
                }
                else
                {
                    Debug.Log(networkId);
                    RegisterPlayerServerRpc(playerName, lobbyId, networkId, isHost);
                    yield break;
                }
            }
        }

        [Rpc(SendTo.Server)]
        private void RegisterPlayerServerRpc(
            string playerName,
            string lobbyId,
            ulong networkId,
            bool isHost
        )
        {
            var newData = new PlayerData(
                new FixedString64Bytes(playerName),
                new FixedString64Bytes(lobbyId),
                networkId,
                isHost,
                false
            );

            // Avoid duplicates (e.g., reconnects)
            foreach (var data in SyncedPlayerList)
            {
                if (data.PlayerNetworkId == networkId)
                {
                    return;
                }
            }

            SyncedPlayerList.Add(newData);
            NewPlayerJoinRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void NewPlayerJoinRpc() // mainly used to notify of a new player that updates the UI
        {
            OnPlayerJoin?.Invoke();
        }

        // Example helper
        public string GetPlayerNameByNetworkId(string networkId)
        {
            foreach (var data in SyncedPlayerList)
            {
                if (data.PlayerNetworkId.ToString() == networkId)
                    return data.PlayerName.ToString();
            }

            return "Unknown";
        }
    }
}
