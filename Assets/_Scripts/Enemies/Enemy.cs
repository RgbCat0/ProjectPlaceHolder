using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class Enemy : NetworkBehaviour, IDamageable
    {
        private EnemyAttack _attack;
        private EnemyMovement _movement;
        public float Health { get; private set; } = 100f;

        public void Initialize(EnemyInfo enemyInfo, Vector3 spawnPoint, bool debug = false)
        {
            _attack = GetComponent<EnemyAttack>();
            _movement = GetComponent<EnemyMovement>();
            Health = enemyInfo.health;
            _movement.SetSpeed(enemyInfo.speed);
            transform.position = spawnPoint;
            GameObject model = Instantiate(enemyInfo.modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
            if (debug)
                _movement.SetSpeed(0f); // UNITY_EDITOR debugging
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