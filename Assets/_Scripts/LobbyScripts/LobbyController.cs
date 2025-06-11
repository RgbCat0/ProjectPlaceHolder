using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.Managers;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.LobbyScripts
{
    public class LobbyController : NetworkBehaviour
    {
#if UNITY_EDITOR
        public bool quickTest;
#endif
        [HideInInspector]
        public LobbyNetManager lobbyNetManager;
        [HideInInspector]
        public PlayerDataSync playerDataSync;
        [HideInInspector]
        public LobbyServiceManager serviceManager;
        [HideInInspector]
        public LobbyUiManager uiManager;

        [SerializeField]
        private GameObject
            lobbyUIPrefab; // Spawns in the UI in the lobby scene as this is the only thing that is not carried over from the main scene

        private GameObject _lobbyUI;

        #region Init

        private async void Start()
        {
            try
            {
                if (lobbyUIPrefab != null)
                {
                    _lobbyUI = Instantiate(lobbyUIPrefab);
                    _lobbyUI.name = "LobbyUI";
                    uiManager = _lobbyUI.GetComponent<LobbyUiManager>();
                }
                else
                {
                    LobbyLogger.Error("Lobby UI prefab is not assigned!");
                }

                // _uiManager = GetComponent<LobbyUiManager>();
                lobbyNetManager = GetComponent<LobbyNetManager>();
                serviceManager = GetComponent<LobbyServiceManager>();
                playerDataSync = GetComponent<PlayerDataSync>();
                await lobbyNetManager.SignInTask();
                uiManager.OnCreateLobby += HandleCreateLobby;
                uiManager.OnMenuCreate += OnHostClicked;
                uiManager.OnMenuJoin += OnClientClicked;
                uiManager.OnJoinLobby += HandleJoinLobby;
                uiManager.OnStartGame += StartGameRpc;
                uiManager.ShowMainMenu();
                playerDataSync.OnPlayerJoin += () => LobbyLogger.StatusMessage("");
#if UNITY_EDITOR
                if (quickTest) HandleCreateLobby("TestLobby");
#endif
            }
            catch (Exception e)
            {
                LobbyLogger.Exception(e);
            }
        }

        #endregion

        private async void OnHostClicked(string obj)
        {
            LobbyLogger.StatusMessage("Updating name...");
            await lobbyNetManager.UpdateName(obj);
            LobbyLogger.StatusMessage("");
        }

        private async void OnClientClicked(string obj)
        {
            LobbyLogger.StatusMessage("Updating name...");
            await lobbyNetManager.UpdateName(obj);
            LobbyLogger.StatusMessage("");
        }

        private async void HandleCreateLobby(string obj)
        {
            LobbyLogger.StatusMessage("Starting Networking...");
            string relayJoinCode = await lobbyNetManager.HostNetworkTask();
            LobbyLogger.StatusMessage("Creating Lobby...");
            await serviceManager.HostLobbyTask(obj, relayJoinCode);
            NetworkManager.OnClientConnectedCallback += _ => ResetReadyStatusRpc();
            NetworkManager.OnClientConnectedCallback += NetworkManagerOnOnClientConnectedCallback;
            CanStartGame(true);
#if UNITY_EDITOR
            if (quickTest) StartGameRpc();
#endif
        }

        private void NetworkManagerOnOnClientConnectedCallback(ulong obj)
        {
            LobbyLogger.StatusMessage("New Player Joining...");
        }

        private async void HandleJoinLobby(Lobby lobby)
        {
            LobbyLogger.StatusMessage("Joining Lobby...");
            await serviceManager.JoinLobbyTask(lobby.Id);
            LobbyLogger.StatusMessage("Starting Networking...");
            await lobbyNetManager.ClientNetworkTask(lobby.Data["RelayJoinCode"].Value);
            LobbyLogger.StatusMessage("Hold on...");
            NetworkManager.OnClientConnectedCallback += _ => LobbyLogger.StatusMessage("New Player Joining...");
        }

        public void CanStartGame(bool canStart)
        {
            if (!NetworkManager.IsHost)
            {
                uiManager.EnableDisableStartGameButton(false);
                return;
            }

            bool allReady = playerDataSync.syncedPlayerList.TrueForAll(p => p.IsReady);
            if (allReady && canStart)
                uiManager.EnableDisableStartGameButton(true);
            else
                uiManager.EnableDisableStartGameButton(false);
        }

        // needs to run local first for correct data.
        public void HandleNewPlayer(ulong clientId)
        {
            HandleNewPlayerRpc(NetworkManager.LocalClientId, AuthenticationService.Instance.PlayerId,
                AuthenticationService.Instance.PlayerName, NetworkManager.IsHost);
        }

        [Rpc(SendTo.Server)]
        private void HandleNewPlayerRpc(ulong clientId, string playerId, string playerName, bool isHost)
        {
            StartCoroutine(playerDataSync.RegisterPlayerServer(playerName, playerId, clientId, isHost));
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        private void StartGameRpc()
        {
            if (!NetworkManager.IsHost)
                return;
            serviceManager.StopHeartbeat();
            NetworkManager.SceneManager.LoadScene("Main", LoadSceneMode.Single);
            NetworkManager.SceneManager.OnLoadComplete += (id, _, _) =>
            {
                if (id == NetworkManager.LocalClientId) GameManager.Instance.StartGame();
            };
            Destroy(_lobbyUI);
        }

        public async Task<List<Lobby>> GetLobbies()
        {
            try
            {
                return await serviceManager.GetLobbiesAsync();
            }
            catch (Exception e)
            {
                LobbyLogger.Exception(e);
                return null;
            }
        }
        #region readyStatus 
        [Rpc(SendTo.Server)]
        public void UpdateReadyButtonRpc(bool readyStatus, ulong clientId)
        {
            try
            {
                var playerDataSync = PlayerDataSync.Instance;
                var playerList = playerDataSync.syncedPlayerList;
                for (int i = 0; i < playerList.Count; i++)
                {
                    if (playerList[i].PlayerNetworkId == clientId)
                    {
                        var data = playerList[i];
                        data.IsReady = readyStatus;
                        playerList[i] = data;
                        // break;
                    }
                }
                playerDataSync.SendFullListRpc();
                UpdateReadyStatusRpc();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in UpdateReadyButtonRpc: {e.Message}");
            }
        }

        [Rpc(SendTo.Server)]
        public void ResetReadyStatusRpc()
        {
            var playerList = playerDataSync.syncedPlayerList;
            for (int i = 0; i < playerList.Count; i++)
            {
                var data = playerList[i];
                data.IsReady = false;
                playerList[i] = data;
            }
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateReadyStatusRpc()
        {
            StartCoroutine(uiManager.UpdateReadyStatus());
        }
        #endregion

        #region Singleton

        public static LobbyController Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                return;
            }

            Destroy(gameObject);
        }

        #endregion
    }
}