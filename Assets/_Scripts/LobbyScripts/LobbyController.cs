using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.Managers;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;

namespace _Scripts.LobbyScripts
{
    public class LobbyController : NetworkBehaviour
    {
#if UNITY_EDITOR
        public bool quickTest;
#endif
        private LobbyNetManager _lobbyNetManager;
        private PlayerDataSync _playerDataSync;
        private LobbyServiceManager _serviceManager;
        private LobbyUiManager _uiManager;

        #region Init

        private async void Start()
        {
            try
            {
                _uiManager = GetComponent<LobbyUiManager>();
                _lobbyNetManager = GetComponent<LobbyNetManager>();
                _serviceManager = GetComponent<LobbyServiceManager>();
                _playerDataSync = GetComponent<PlayerDataSync>();
                await _lobbyNetManager.SignInTask();
                _uiManager.OnCreateLobby += HandleCreateLobby;
                _uiManager.OnMenuCreate += OnHostClicked;
                _uiManager.OnMenuJoin += OnClientClicked;
                _uiManager.OnJoinLobby += HandleJoinLobby;
                _uiManager.OnStartGame += StartGameRpc;
                _uiManager.ShowMainMenu();
                _playerDataSync.OnPlayerJoin += () => LobbyLogger.StatusMessage("");
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
            await _lobbyNetManager.UpdateName(obj);
            LobbyLogger.StatusMessage("");
        }

        private async void OnClientClicked(string obj)
        {
            LobbyLogger.StatusMessage("Updating name...");
            await _lobbyNetManager.UpdateName(obj);
            LobbyLogger.StatusMessage("");
        }

        private async void HandleCreateLobby(string obj)
        {
            LobbyLogger.StatusMessage("Starting Networking...");
            string relayJoinCode = await _lobbyNetManager.HostNetworkTask();
            LobbyLogger.StatusMessage("Creating Lobby...");
            await _serviceManager.HostLobbyTask(obj, relayJoinCode);
            NetworkManager.OnClientConnectedCallback += _ => _uiManager.ResetReadyStatusRpc();
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
            await _serviceManager.JoinLobbyTask(lobby.Id);
            LobbyLogger.StatusMessage("Starting Networking...");
            await _lobbyNetManager.ClientNetworkTask(lobby.Data["RelayJoinCode"].Value);
            LobbyLogger.StatusMessage("Hold on...");
            NetworkManager.OnClientConnectedCallback += _ => LobbyLogger.StatusMessage("New Player Joining...");
        }

        public void CanStartGame(bool canStart)
        {
            if (!NetworkManager.IsHost)
            {
                _uiManager.DisableStartGameButton();
                return;
            }

            bool allReady = _playerDataSync.syncedPlayerList.TrueForAll(p => p.IsReady);
            if (allReady && canStart)
                _uiManager.EnableStartGameButton();
            else
                _uiManager.DisableStartGameButton();
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
            StartCoroutine(_playerDataSync.RegisterPlayerServer(playerName, playerId, clientId, isHost));
        }

        [Rpc(SendTo.Server, RequireOwnership = true)]
        private void StartGameRpc()
        {
            if (NetworkManager.IsHost) StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            _serviceManager.StopHeartbeat();
            NetworkManager.SceneManager.LoadScene("Main", LoadSceneMode.Single);
            NetworkManager.SceneManager.OnLoadComplete += (id, _, _) =>
            {
                if (id == NetworkManager.LocalClientId) GameManager.Instance.StartGame();
            };
            yield return null;
        }

        public async Task<List<Lobby>> GetLobbies()
        {
            try
            {
                return await _serviceManager.GetLobbiesAsync();
            }
            catch (Exception e)
            {
                LobbyLogger.Exception(e);
                return null;
            }
        }

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