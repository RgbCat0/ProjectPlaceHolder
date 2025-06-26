using TMPro;
using UnityEngine;

namespace LobbyScripts
{
    public static class LobbyLogger // never even used this except for the status text lol
    {
        public static bool EnableLogging = true;
        public static bool EnableWarnings = true;
        public static bool EnableErrors = true;
        private static TextMeshProUGUI _statusText;

        public static void Initialize(TextMeshProUGUI statusText)
        {
            _statusText = statusText;
            StatusMessage("");
        }

        public static void Log(object message)
        {
            if (!EnableLogging)
                return;
            Debug.Log($"[Lobby] {message}");
        }

        public static void Warn(object message)
        {
            if (!EnableLogging)
                return;
            Debug.LogWarning($"[Lobby] {message}");
        }

        public static void Error(object message)
        {
            if (!EnableLogging)
                return;
            Debug.LogError($"[Lobby] {message}");
        }

        public static void Exception(object message)
        {
            if (!EnableLogging)
                return;
            Debug.LogException(message as System.Exception);
        }

        public static void StatusMessage(string message, Color color = default)
        {
            if (!PlayerPrefs.HasKey("Status"))
                PlayerPrefs.SetInt("Status", 0); // Default to disabled
            if (PlayerPrefs.GetInt("Status") == 0)
            {
                _statusText.transform.parent.gameObject.SetActive(false);
            }
            else if(color != default || PlayerPrefs.GetInt("Status") == 1) // always show if color is set (means error or warning)
            {
                _statusText.transform.parent.gameObject.SetActive(true);
                if (color == default)
                    color = Color.white;
                _statusText.color = color;
                _statusText.text = message;
            }
        }
    }
}