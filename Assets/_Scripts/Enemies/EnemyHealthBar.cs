using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Enemies
{
    public class EnemyHealthBar : NetworkBehaviour
    {
        private Enemy _enemy;

        [SerializeField]
        private RectTransform healthBar;

        [SerializeField]
        private GameObject canvas;

        private float _maxHealthBarWidth;
        private float _startHealth;
        private float _hideTime = 5f;
        private float _hideTimer;

        private void Start()
        {
            _enemy = GetComponent<Enemy>();
            if (_enemy == null)
            {
                Debug.LogError("Enemy component not found on the GameObject.");
                return;
            }

            _startHealth = _enemy.Health;
            _maxHealthBarWidth = healthBar.sizeDelta.x;
        }
        [Rpc(SendTo.Everyone)]
        public void UpdateHealthBarRpc(float health)
        {
            _hideTimer += _hideTime;
            if (_enemy == null)
            {
                Debug.LogError("Enemy component is not assigned.");
                return;
            }

            if (healthBar == null)
            {
                Debug.LogError("Health bar RectTransform is not assigned.");
                return;
            }

            float currentHealth = health;
            if (currentHealth <= 0)
            {
                healthBar.sizeDelta = new Vector2(0, healthBar.sizeDelta.y);
            }
            else
            {
                float healthPercentage = currentHealth / _startHealth;
                healthBar.sizeDelta = new Vector2(
                    _maxHealthBarWidth * healthPercentage,
                    healthBar.sizeDelta.y
                );
            }
        }

        private void FixedUpdate()
        {
            canvas.SetActive(_hideTimer > 0);
            if (_hideTimer > 0)
            {
                _hideTimer -= Time.fixedDeltaTime;
                if (_hideTimer <= 0)
                {
                    canvas.SetActive(false);
                }
            }
        }
    }
}
