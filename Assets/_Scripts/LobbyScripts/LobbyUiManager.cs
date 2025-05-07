using System;
using TMPro;
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
        private TextMeshProUGUI statusText;

        private void Start()
        {
            LobbyLogger.Initialize(statusText);
            hostMenuButton?.onClick.AddListener(HostMenuClicked);
            clientMenuButton?.onClick.AddListener(ClientMenuClicked);
            nameInputField?.onValueChanged.AddListener(NameEdit);
            createLobbyNameField?.onValueChanged.AddListener(LobbyNameEdit);

            createLobbyButton?.onClick.AddListener(CreateLobby);
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
    }
}
