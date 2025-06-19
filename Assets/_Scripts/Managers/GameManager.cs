using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public GameObject gameOverScreen;
        public bool gameOver = false;
        

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
            {
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
                    foreach (NetworkObject player in players)
                    {
                        player.GetComponent<PlayerHealth>().onDeath += HandleDeathRpc;
                    }

                    _waveManager.OnWaveCompleteEvent += RevivePlayersRpc;
                    _waveManager.Init();
                }
            }
            niggerRpc();
            
        }
        [Rpc(SendTo.Everyone)]
        private void niggerRpc()
        {
            NetworkManager.SceneManager.OnLoadComplete += (id, _, _) => GameOverUIRpc();  
        }
        
        [Rpc(SendTo.Everyone)]
        public void GameOverUIRpc()
        {
            gameOverScreen = GameObject.Find("Gameover");
            Button restartButton = GameObject.Find("RestartButton").GetComponent<Button>();
            Button quitButton = GameObject.Find("Quit").GetComponent<Button>();
            quitButton.onClick.AddListener(QuitButton);
            restartButton.onClick.AddListener(RevivePlayersRpc);
            // gameOverScreen.SetActive(false);
        }

        [Rpc(SendTo.Everyone)]
        public void HandleDeathRpc(ulong playerID)
        {
            if (deadPlayers.Count >= players.Count && IsServer )
            {
                Debug.Log("all players are dead, game over22131313213");
                gameOver = true;
                gameOverScreen.SetActive(true);
                _waveManager.ResetWaves();
            }
        }

        [Rpc(SendTo.Everyone)]
        public void RevivePlayersRpc()
        {
            foreach (GameObject player in deadPlayers)
            {
                if (gameOver)
                {
                    gameOverScreen.SetActive(false);
                    PlayerStats stats = player.GetComponent<PlayerStats>();
                    stats.currentMana = stats.currentMaxMana;
                    player.GetComponent<PlayerHealth>().Health = player.GetComponent<PlayerHealth>().MaxHealth;
                }

                player.SetActive(true);
                player.transform.position = _playerSpawnPoint.position + Random.Range(0f, 1f) * Vector3.right;
            }

            if (IsServer)
            {
                if (gameOver) _waveManager.SendNextWaveEventRpc();
                deadPlayers.Clear();
                gameOver = false;
            }
        }
                 
        private void QuitButton()
        {
            SceneManager.LoadScene("MainMenu");
        } 
        
        
    }
}
