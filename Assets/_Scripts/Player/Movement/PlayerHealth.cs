using UnityEngine;
using _Scripts.Managers;

namespace _Scripts.Player.Movement
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }

        private void Start()
        {
            Health = 100f; // Set initial health
            MaxHealth = Health;
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
