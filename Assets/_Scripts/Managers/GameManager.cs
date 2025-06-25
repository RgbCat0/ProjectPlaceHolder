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
            Application.targetFrameRate = 144;
            // if (NetworkManager.IsHost)
            // {
            //     // NetworkObject.SpawnWithOwnership(0);
            // }


            _waveManager = GetComponent<WaveManager>();
        }

        public void StartGame() // NOTE: Only runs on host/server
        {
            Debug.Log("Is being called");
            _playerSpawnPoint = GameObject.Find("Playerspawn").transform;
            // spawns the players
            if (NetworkManager.IsHost)
                Debug.LogError(NetworkManager.ConnectedClients.Count + " clients are connected!??!?!?!?!?!?!");
            foreach (var clientId in NetworkManager.ConnectedClientsIds) // ain no way this fixed the ghost player issue
            {
                Debug.Log(clientId);
                NetworkObject newPlayer = NetworkManager.SpawnManager.InstantiateAndSpawn(
                    playerPrefab,
                    ownerClientId: clientId,
                    isPlayerObject: true,
                    position: _playerSpawnPoint.position + Random.Range(0f, 1f) * Vector3.right
                );
                players.Add(newPlayer);
                foreach (NetworkObject player in players)
                {
                    player.GetComponent<PlayerHealth>().onDeath += HandleDeathRpc;
                }
            }

            _waveManager.OnWaveCompleteEvent += RevivePlayersRpc;
            _waveManager.Init();
        }


        public void GameOverUI(GameObject gameOverObj)
        {
            Debug.Log("Being called");
            gameOverScreen = gameOverObj;

            Button restartButton = gameOverObj.transform.GetChild(0).GetComponent<Button>();
            Button quitButton = gameOverObj.transform.GetChild(1).GetComponent<Button>();
            quitButton.onClick.AddListener(QuitButton);
            restartButton.onClick.AddListener(RevivePlayersRpc);
            gameOverScreen.SetActive(false);
        }

        [Rpc(SendTo.Everyone)]
        public void HandleDeathRpc(ulong playerID)
        {
            if (deadPlayers.Count >= players.Count && IsServer)
            {
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
                    stats.currentMana.Value = stats.currentMaxMana;
                    player.GetComponent<PlayerHealth>().Health = player.GetComponent<PlayerHealth>().MaxHealth;
                }

                player.SetActive(true);
                if (_playerSpawnPoint == null)
                {
                    _playerSpawnPoint = GameObject.Find("Playerspawn").transform;
                    if (_playerSpawnPoint == null)
                        Debug.LogError("Player spawn point is null!");
                }

                player.transform.position = _playerSpawnPoint.position + Random.Range(0f, 1f) * Vector3.right;
            }

            if (IsServer)
            {
                if (gameOver) _waveManager.SendNextWaveEventRpc();
                deadPlayers.Clear();
                gameOver = false;
            }
        }

        public string GetDifficultyName() => GetComponent<DifficultyManager>().GetDifficultyName();

        private void QuitButton()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}