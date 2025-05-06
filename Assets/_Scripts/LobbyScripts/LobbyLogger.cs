using TMPro;
using UnityEngine;

namespace _Scripts.LobbyScripts
{
    public static class LobbyLogger
    {
        public static bool EnableLogging = true;
        public static bool EnableWarnings = true;
        public static bool EnableErrors = true;
        private static TextMeshProUGUI StatusText;

        public static void Initialize(TextMeshProUGUI statusText)
        {
            StatusText = statusText;
            StatusText.gameObject.SetActive(true);
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
            if (color == default)
                color = Color.white;
            StatusText.color = color;
            StatusText.text = message;
        }
    }
}
