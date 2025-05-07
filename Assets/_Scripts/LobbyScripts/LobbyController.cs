using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

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

            _playerDataSync.RegisterPlayerServerRpc(
                _lobbyNetManager.PlayerName,
                AuthenticationService.Instance.PlayerId,
                NetworkManager.LocalClientId,
                NetworkManager.IsHost
            );
        }
    }
}
