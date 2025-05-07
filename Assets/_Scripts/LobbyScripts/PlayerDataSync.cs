using Unity.Netcode;

namespace _Scripts.LobbyScripts
{
    public class PlayerDataSync : NetworkBehaviour
    {
        public NetworkList<PlayerData> SyncedPlayerList;

        // public event Action<PlayerData>/

        private void Awake()
        {
            SyncedPlayerList = new NetworkList<PlayerData>();
        }

        // public override void OnNetworkSpawn()
        // {
        //     if (IsServer)
        //     {
        //
        //     }
        // }

        [Rpc(SendTo.Server)]
        public void RegisterPlayerServerRpc(
            string playerName,
            string lobbyId,
            ulong networkId,
            bool isHost
        )
        {
            var newData = new PlayerData(playerName, lobbyId, networkId, isHost, false);

            // Avoid duplicates (e.g., reconnects)
            foreach (var data in SyncedPlayerList)
            {
                if (data.PlayerNetworkId == networkId)
                    return;
            }

            SyncedPlayerList.Add(newData);
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
