using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        public NetworkObject playerPrefab;
        public List<NetworkObject> players = new();
        public List<GameObject> deadPlayers = new();
        public static GameManager Instance { get; private set; }
        private Transform _playerSpawnPoint; // Set this to the desired spawn point in the scene
        private WaveManager _waveManager;
        
        bool gameOver = false;
        

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
            
            
            _waveManager = GetComponent<WaveManager>();
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
            foreach (NetworkObject player in players)
            {
                player.GetComponent<PlayerHealth>().onDeath += HandleDeathRpc;
            }

            _waveManager.OnWaveCompleteEvent += RevivePlayersRpc;
            // Button restartButton = GameObject.Find("RestartButton").GetComponent<Button>();
            // restartButton.onClick.AddListener(RevivePlayersRpc);
            // TODO set gameover screen to false
            _waveManager.Init();
        }

        [Rpc(SendTo.Server)]
        public void HandleDeathRpc(ulong playerID)
        {
            foreach (NetworkObject player in players)
            {
                if (player != null && !player.gameObject.activeInHierarchy)
                {
                    deadPlayers.Add(player.gameObject);
                }

                if (players.Count >= deadPlayers.Count)
                {
                    gameOver = true;
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void RevivePlayersRpc()
        {
            foreach (GameObject player in deadPlayers)
            {
                if (gameOver)
                {
                    gameOver = false;
                    _waveManager.ResetWaves();
                }
                player.SetActive(true);
                player.transform.position = _playerSpawnPoint.position + Random.Range(0f, 1f) * Vector3.right;
            }
        }
    }
}
