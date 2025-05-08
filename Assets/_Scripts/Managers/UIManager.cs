using UnityEngine;

namespace _Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField]
        private RectTransform healthBar;
        private float _maxHealthBarWidth;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (healthBar == null)
            {
                Debug.LogError("Health bar is not assigned in the inspector.");
                return;
            }
            _maxHealthBarWidth = healthBar.sizeDelta.x;
        }

        public void UpdateHealthBar(float currHealth, float maxHealth)
        {
            if (healthBar == null)
            {
                Debug.LogError("Health bar is not assigned in the inspector.");
                return;
            }

            if (currHealth <= 0)
            {
                healthBar.sizeDelta = new Vector2(0, healthBar.sizeDelta.y);
            }
            else
            {
                float healthPercentage = currHealth / maxHealth;
                healthBar.sizeDelta = new Vector2(
                    _maxHealthBarWidth * healthPercentage,
                    healthBar.sizeDelta.y
                );
            }
        }
    }
}
