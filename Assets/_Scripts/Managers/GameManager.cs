using System.Collections.Generic;
using Unity.Netcode;

namespace _Scripts.Managers
{
    public class GameManager : NetworkBehaviour
    {
        public NetworkObject playerPrefab;
        public List<NetworkObject> players = new();
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            // if (NetworkManager.IsHost)
            // {
            //     // NetworkObject.SpawnWithOwnership(0);
            // }
        }

        public void StartGame()
        {
            // spawns the players
            if (NetworkManager.IsHost)
                for (var i = 0; i < NetworkManager.Singleton.ConnectedClients.Count; i++)
                {
                    // spawns in the player (the lobby player is only for lobby purposes)
                    NetworkObject newPlayer = NetworkManager.SpawnManager.InstantiateAndSpawn(
                        playerPrefab,
                        (ulong)i,
                        isPlayerObject: true
                    );
                    players.Add(newPlayer);
                }

            GetComponent<WaveManager>().Init();
        }
    }
}
