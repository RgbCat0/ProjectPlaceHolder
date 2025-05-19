using Unity.Netcode;
using UnityEngine;
using _Scripts.Managers;

namespace _Scripts.Enemies
{
    public class Enemy : NetworkBehaviour, IDamageable
    {
        private EnemyAttack _attack;
        private EnemyMovement _movement;
        public float Health { get; private set; } = 100f;

        #region init



        public void Initialize(EnemyInfo enemyInfo, Vector3 spawnPoint, bool debug = false)
        {
            _attack = GetComponent<EnemyAttack>();
            _movement = GetComponent<EnemyMovement>();
            Health = enemyInfo.health;
            _movement.SetSpeed(enemyInfo.speed);
            // Debug.Log(spawnPoint);
            // transform.position = spawnPoint;
            GameObject model = Instantiate(enemyInfo.modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
            if (debug)
                _movement.SetSpeed(0f); // UNITY_EDITOR debugging
        }
        #endregion

        #region Health

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0f)
                DieRpc();
            Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {Health}");
        }

        //ensure running on server
        [Rpc(SendTo.Server)]
        private void DieRpc()
        {
            // Handle enemy death (e.g., play animation, destroy object, etc.)
            WaveManager.Instance.EnemyDeath(NetworkObject);
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject); // Destroy the enemy object
        }

        #endregion
    }
}
