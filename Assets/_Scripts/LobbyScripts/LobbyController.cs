using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

namespace _Scripts.LobbyScripts
{
    public class LobbyController : NetworkBehaviour
    {
        private LobbyUiManager _uiManager;
        private LobbyNetManager _lobbyNetManager;
        private LobbyServiceManager _serviceManager;
        private PlayerDataSync _playerDataSync;

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
                _uiManager.ShowMainMenu();
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
            LobbyLogger.StatusMessage("");
            CanStartGame(true);
        }

        private async void HandleJoinLobby(Lobby lobby)
        {
            LobbyLogger.StatusMessage("Joining Lobby...");
            await _serviceManager.JoinLobbyTask(lobby.Id);
            LobbyLogger.StatusMessage("Starting Networking...");
            await _lobbyNetManager.ClientNetworkTask(lobby.Data["RelayJoinCode"].Value);
            LobbyLogger.StatusMessage("");
        }

        public void CanStartGame(bool canStart)
        {
            var allReady = true;
            foreach (var playerData in _playerDataSync.SyncedPlayerList)
            {
                if (!playerData.IsReady)
                    allReady = false;
            }
            if (canStart && allReady)
            {
                _uiManager.EnableStartGameButton();
            }
            else
            {
                _uiManager.DisableStartGameButton();
            }
        }

        // needs to run local first for correct data.
        public void HandleNewPlayer(ulong clientId) =>
            HandleNewPlayerRpc(
                clientId,
                AuthenticationService.Instance.PlayerId,
                _lobbyNetManager.PlayerName,
                NetworkManager.IsHost
            );

        [Rpc(SendTo.Server)]
        private void HandleNewPlayerRpc(
            ulong clientId,
            string playerId,
            string playerName,
            bool isHost
        ) =>
            StartCoroutine(
                _playerDataSync.RegisterPlayerServer(playerName, playerId, clientId, isHost)
            );

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
    }
}
