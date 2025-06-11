using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.LobbyScripts
{
    public class LobbyUiManager : MonoBehaviour
    {
        [Header("Menu Parents")]
        [SerializeField]
        private GameObject mainParent;

        [SerializeField]
        private GameObject createParent;

        [SerializeField]
        private GameObject joinParent;

        [SerializeField]
        private GameObject lobbyParent;

        [Header("Main Menu")]
        [SerializeField]
        private Button hostMenuButton;

        [SerializeField]
        private Button clientMenuButton;

        [SerializeField]
        private TMP_InputField nameInputField;

        public event Action<string> OnMenuCreate;
        public event Action<string> OnMenuJoin;

        [Header("Create Lobby")]
        [SerializeField]
        private Button createLobbyButton;

        [SerializeField]
        private TMP_InputField createLobbyNameField;
        public event Action<string> OnCreateLobby;

        [SerializeField]
        private Button goBackButton;

        [Header("Join Lobby")]
        [SerializeField]
        private GameObject joinLobbyPanel;

        [SerializeField]
        private GameObject joinLobbyPrefab;
        private List<GameObject> _lobbies = new();

        [SerializeField]
        private Button refreshButton;
        public event Action<Lobby> OnJoinLobby;

        [SerializeField]
        private Button joinGoBackButton;

        [Header("Lobby")]
        [SerializeField]
        private Button startGameButton;

        [SerializeField]
        private Button readyButton;

        [SerializeField]
        private GameObject playerLobbyPanel;

        [SerializeField]
        private GameObject playerLobbyPrefab;
        private List<GameObject> _playerLobbyPanels = new();
        public event Action OnStartGame;

        [Header("Misc")]
        [SerializeField]
        private TextMeshProUGUI statusText;

        private void Start()
        {
            LobbyLogger.Initialize(statusText);
            hostMenuButton?.onClick.AddListener(HostMenuClicked);
            clientMenuButton?.onClick.AddListener(ClientMenuClicked);
            nameInputField?.onValueChanged.AddListener(NameEdit);
            createLobbyNameField?.onValueChanged.AddListener(LobbyNameEdit);
            createLobbyButton?.onClick.AddListener(CreateLobby);
            startGameButton?.onClick.AddListener(StartGame);
            refreshButton?.onClick.AddListener(PopulateLobbies);
            goBackButton?.onClick.AddListener(ShowMainMenu);
            joinGoBackButton?.onClick.AddListener(ShowMainMenu);

            // GetComponent<PlayerDataSync>().SyncedPlayerList.OnListChanged += _ =>
            //     UpdateLobbyPlayers();
        }

        private void LobbyNameEdit(string arg0)
        {
            if (string.IsNullOrEmpty(arg0))
            {
                createLobbyButton.interactable = false;
                return;
            }
            createLobbyButton.interactable = true;
            createLobbyNameField.text = arg0.Replace(" ", "");
            if (createLobbyNameField.text.Length > 20)
                createLobbyNameField.text = createLobbyNameField.text[..20];
        }

        private void NameEdit(string arg0)
        {
            if (string.IsNullOrEmpty(arg0))
            {
                hostMenuButton.interactable = false;
                clientMenuButton.interactable = false;
                return;
            }
            hostMenuButton.interactable = true;
            clientMenuButton.interactable = true;
            nameInputField.text = arg0.Replace(" ", "");
            if (nameInputField.text.Length > 20)
                nameInputField.text = nameInputField.text[..20];
        }

        private void HostMenuClicked()
        {
            OnMenuCreate?.Invoke(nameInputField.text);
            ChangeMenu(createParent);
        }

        private void ClientMenuClicked()
        {
            OnMenuJoin?.Invoke(nameInputField.text);
            ChangeMenu(joinParent);
            PopulateLobbies(); // TODO add refresh button (already implemented in UI)
        }

        private void CreateLobby()
        {
            string lobbyName = createLobbyNameField.text;
            if (string.IsNullOrEmpty(lobbyName))
            {
                Debug.LogError("Lobby name cannot be empty.");
                return;
            }
            createLobbyButton.interactable = false;
            OnCreateLobby?.Invoke(lobbyName);
            ChangeMenu(lobbyParent);
        }

        private async void PopulateLobbies()
        {
            try
            {
                refreshButton.interactable = false;
                foreach (var obj in _lobbies)
                {
                    Destroy(obj);
                }
                _lobbies.Clear();
                // get lobbies from lobbyService
                List<Lobby> lobbies = await LobbyController.Instance.GetLobbies();
                foreach (var lobby in lobbies)
                {
                    var newPanel = Instantiate(joinLobbyPrefab, joinLobbyPanel.transform);
                    newPanel
                        .transform.GetChild(0)
                        .GetChild(1)
                        .GetComponent<TextMeshProUGUI>()
                        .text = $"{lobby.Name}\n {lobby.Data["HostName"].Value}";
                    newPanel
                        .GetComponentInChildren<Button>()
                        .onClick.AddListener(() => JoinLobby(lobby));
                    _lobbies.Add(newPanel);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error populating lobbies: {e.Message}");
                LobbyLogger.StatusMessage("Error populating lobbies", Color.red);
            }
            finally
            {
                refreshButton.interactable = true;
            }
        }

        private void JoinLobby(Lobby lobby)
        {
            OnJoinLobby?.Invoke(lobby);
            ChangeMenu(lobbyParent);
            startGameButton.interactable = false;
        }



        public void EnableDisableStartGameButton(bool enable)
        {
            startGameButton.interactable = enable;
        }

        public void ShowMainMenu()
        {
            ChangeMenu(mainParent);
            hostMenuButton.interactable = false;
            clientMenuButton.interactable = false;
            nameInputField.text = string.Empty;
        }

        private void ChangeMenu(GameObject menu = null)
        {
            createParent.SetActive(false);
            joinParent.SetActive(false);
            mainParent.SetActive(false);
            lobbyParent.SetActive(false);
            menu?.SetActive(true);
        }

        public IEnumerator AddNewPlayer()
        {
            var playerDataSync = LobbyController.Instance.playerDataSync;
            while (!playerDataSync.listIsSynced.Value)
                yield return null;

            foreach (var obj in _playerLobbyPanels)
            {
                Destroy(obj);
            }
            _playerLobbyPanels.Clear();
            foreach (var playerData in playerDataSync.syncedPlayerList)
            {
                var newPanel = Instantiate(playerLobbyPrefab, playerLobbyPanel.transform);
                newPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    playerData.PlayerName.ToString();
                var toggle = newPanel.transform.GetChild(0).GetChild(1).GetComponent<Toggle>();
                if (playerData.PlayerNetworkId == NetworkManager.Singleton.LocalClientId)
                {
                    toggle.onValueChanged.AddListener(readyStatus =>
                        LobbyController.Instance.UpdateReadyButtonRpc(readyStatus, NetworkManager.Singleton.LocalClientId)
                    );
                }
                else
                {
                    toggle.interactable = false;
                }
                _playerLobbyPanels.Add(newPanel);
            }
        }



        public IEnumerator UpdateReadyStatus()
        {
            var playerDataSync = LobbyController.Instance.playerDataSync;
            while (!playerDataSync.listIsSynced.Value)
                yield return null;
            for (int i = 0; i < _playerLobbyPanels.Count; i++)
            {
                var playerNameText = _playerLobbyPanels[i]
                    .GetComponentInChildren<TextMeshProUGUI>();
                playerNameText.color = playerDataSync.syncedPlayerList[i].IsReady
                    ? Color.green
                    : Color.white;
                _playerLobbyPanels[i]
                    .transform.GetChild(0)
                    .GetChild(1)
                    .GetComponent<Toggle>()
                    .isOn = playerDataSync.syncedPlayerList[i].IsReady;
            }
            LobbyController.Instance.CanStartGame(true);
        }

        public void StartGame()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                OnStartGame?.Invoke();
            }
        }
    }
}
