using UnityEngine;

namespace _Scripts.Enemies
{
    public class Enemy : MonoBehaviour, IDamageable
    {
        private EnemyAttack _attack;
        private EnemyMovement _movement;
        public float Health { get; private set; } = 100f;

        public void Initialize(EnemyInfo enemyInfo, Vector3 spawnPoint)
        {
            _attack = GetComponent<EnemyAttack>();
            _movement = GetComponent<EnemyMovement>();
            Health = enemyInfo.health;
            _movement.SetSpeed(enemyInfo.speed);
            transform.position = spawnPoint;
            var model = Instantiate(enemyInfo.modelPrefab, transform);
            model.transform.localPosition = enemyInfo.modelPrefab.transform.position;
        }

        #region Health

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0f)
                Die();
            Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {Health}");
        }

        private void Die()
        {
            // Handle enemy death (e.g., play animation, destroy object, etc.)
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject); // Destroy the enemy object
        }

        #endregion
    }
}
