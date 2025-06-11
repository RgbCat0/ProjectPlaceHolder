using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace _Scripts.LobbyScripts
{
    public class PlayerDataSync : NetworkBehaviour
    {
        public List<PlayerData> syncedPlayerList = new();
        public NetworkVariable<bool> listIsSynced;
        public event Action OnPlayerJoin;
        private bool _isActuallySpawned;
        public bool gameStarted; // disables all Lobby related functions
        public ulong localPlayerObjectId;
        public static PlayerDataSync Instance { get; private set; }

        private void Awake()
        {
            listIsSynced = new NetworkVariable<bool>();
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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
            LobbyController.Instance.CanStartGame(false);
            while (true)
            {
                if (!_isActuallySpawned)
                {
                    yield return null;
                }
                else
                {
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
                false,
                0 // set once the game starts
            );

            // Avoid duplicates (e.g., reconnects)
            foreach (var data in syncedPlayerList)
            {
                if (data.PlayerNetworkId == networkId)
                {
                    return;
                }
            }

            syncedPlayerList.Add(newData);
            if (NetworkManager.ConnectedClientsList.Count == 1) // dont need it for the host
                listIsSynced.Value = true;
            else
                SendFullListRpc();
            NewPlayerJoinRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void NewPlayerJoinRpc() // mainly used to notify of a new player that updates the UI
        {
            OnPlayerJoin?.Invoke();

            StartCoroutine(LobbyController.Instance.uiManager.AddNewPlayer());
        }

        /// <summary>
        /// This function should be called from the server itself. Any new data from clients have to be sent via RPC.
        /// </summary>
        [Rpc(SendTo.Server)]
        public void SendFullListRpc()
        {
            if (NetworkManager.ConnectedClientsList.Count == 1)
                return; // No need to sync if only one player is connected
            listIsSynced.Value = false; // when set to false indicate that the list is not synced yet

            UpdateListRpc(syncedPlayerList.ToArray());
        }

        [Rpc(SendTo.NotServer)]
        private void UpdateListRpc(PlayerData[] playerList)
        {
            syncedPlayerList.Clear();
            foreach (var data in playerList)
            {
                syncedPlayerList.Add(data);
            }
            SyncIsTrueRpc();
        }

        [Rpc(SendTo.Server)]
        private void SyncIsTrueRpc()
        {
            listIsSynced.Value = true;
        }

        // Example helper
        public string GetPlayerNameByNetworkId(string networkId)
        {
            foreach (var data in syncedPlayerList)
            {
                if (data.PlayerNetworkId.ToString() == networkId)
                    return data.PlayerName.ToString();
            }

            return "Unknown";
        }

        public PlayerData? GetPlayerDataByNetworkId(ulong networkId)
        {
            foreach (var data in syncedPlayerList)
            {
                if (data.PlayerNetworkId == networkId)
                    return data;
            }

            return null;
        }
    }
}
