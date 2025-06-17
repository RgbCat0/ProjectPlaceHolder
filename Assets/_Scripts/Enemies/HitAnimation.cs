using TMPro;
using UnityEngine;

namespace Enemies
{
    public class HitAnimation : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI hitText;

        [SerializeField]
        private float destroyTime = 1f;

        public void ShowHitText(string text)
        {
            Destroy(gameObject, destroyTime);
            if (hitText != null)
            {
                hitText.text = text;
                hitText.gameObject.SetActive(true);
            }
        }
    }
}
