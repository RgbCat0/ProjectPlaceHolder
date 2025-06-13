using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Managers
{
    public class GameManager : NetworkBehaviour
    {
        public NetworkObject playerPrefab;
        public List<NetworkObject> players = new();
        public static GameManager Instance { get; private set; }
        private Transform _playerSpawnPoint; // Set this to the desired spawn point in the scene

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
            _playerSpawnPoint = GameObject.Find("Playerspawn").transform;
            // spawns the players
            if (NetworkManager.IsHost)
                for (var client = 0; client < NetworkManager.Singleton.ConnectedClients.Count; client++)
                {
                    // spawns in the player (the lobby player is only for lobby purposes)
                    NetworkObject newPlayer = NetworkManager.SpawnManager.InstantiateAndSpawn(
                        playerPrefab,
                        (ulong)client,
                        isPlayerObject: true,
                        position: _playerSpawnPoint.position + Random.Range(0f, 1f) * Vector3.right
                    );
                    players.Add(newPlayer);
                }

            GetComponent<WaveManager>().Init();
        }
    }
}
