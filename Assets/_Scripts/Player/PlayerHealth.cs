using UnityEngine;
using _Scripts.Managers;
using System.Globalization;
using Unity.Netcode;

namespace _Scripts.Player
{
    public class PlayerHealth : NetworkBehaviour, IDamageable
    {
        private PlayerStats _playerStats;

        public float Health { get; private set; }
        public float MaxHealth { get; private set; }

        private float healthRegenTimer = 0f;

        private void Start()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
            _playerStats = GetComponent<PlayerStats>();
            MaxHealth = _playerStats.baseMaxHealth;
            Health = MaxHealth; // Set initial health
        }

        private void Update()
        {
                MaxHealth = _playerStats.currentMaxHealth;
                UIManager.Instance.UpdateHealthBar(Health, MaxHealth);

            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= 1f)
            {
                Health += _playerStats.currentHealthRegen;
                if (Health > MaxHealth)
                    Health = MaxHealth;
                healthRegenTimer = 0f;
            }
        }
        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }
            UIManager.Instance.UpdateHealthBar(Health, MaxHealth);
        }

        private void Die()
        {
            // Handle player death (e.g., play animation, destroy object, etc.)
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject); // Destroy the player object
        }
    }
}
