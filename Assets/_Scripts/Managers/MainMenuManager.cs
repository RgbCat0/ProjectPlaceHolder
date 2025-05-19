using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Scripts.Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private Button playButton;

        private void Start()
        {
            playButton.onClick.AddListener(GoToGame);
        }

        private void GoToGame() => SceneManager.LoadScene("Lobby");
    }
}
