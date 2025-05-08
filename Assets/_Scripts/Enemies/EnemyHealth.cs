using UnityEngine;

namespace _Scripts.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public float Health { get; private set; } = 100f;

        public void Initialize(float health)
        {
            // can be used to set the initial health of the enemy
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0f)
                Die();
        }

        private void Die()
        {
            // Handle enemy death (e.g., play animation, destroy object, etc.)
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject); // Destroy the enemy object
        }
    }
}
