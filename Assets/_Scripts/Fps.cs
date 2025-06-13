using TMPro;
using UnityEngine;

namespace _Scripts
{
    public class Fps : MonoBehaviour
    {
        
        public bool showFps = true;
        [SerializeField]
        private TextMeshProUGUI fpsText;
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
        void Update()
        {
            if (showFps)
            {
                fpsText.text = "FPS: " + Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            }   
            else 
            {
                fpsText.text = "";
            }
        }
    }
}
