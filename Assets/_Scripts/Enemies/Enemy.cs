using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using _Scripts.Managers;
using _Scripts.Player;

namespace _Scripts.Enemies
{
    public class Enemy : NetworkBehaviour, IDamageable
    {
        private Spell.SpellType currentEffect = Spell.SpellType.None;
        private EnemyAttack _attack;
        private EnemyMovement _movement;
        private NavMeshAgent _navMeshAgent;
        public float Health { get; private set; } = 100f;

        #region init
        public void Initialize(EnemyInfo enemyInfo, Vector3 spawnPoint, bool debug = false)
        {
            _attack = GetComponent<EnemyAttack>();
            _movement = GetComponent<EnemyMovement>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
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

        public void ApplyElementEffect(Spell spell, PlayerStats stats)
        {
            float duration = Time.time + spell.effectDuration;
            Debug.Log(currentEffect);
            switch (spell.spellType)
            {
                case Spell.SpellType.Fire:
                    Debug.Log("Fire");
                    if (currentEffect == Spell.SpellType.Water)
                    {
                        Debug.unityLogger.Log("Water", "Fire");
                        TakeDamage(spell.damage * stats.damageMultiplier * 1.5f);
                        currentEffect = Spell.SpellType.None;
                    }
                    else
                    {
                        Debug.unityLogger.Log("Fire", "Fire");
                        currentEffect = spell.spellType;
                        while (Time.time < duration)
                        {
                            TakeDamage(spell.damage * stats.damageMultiplier);
                            TakeDamage(spell.effectDamage);
                        }
                    }

                    break;

                case Spell.SpellType.Lightning:
                    Debug.Log("Lightning");
                    if (currentEffect == Spell.SpellType.Water)
                    {
                        TakeDamage(spell.damage * stats.damageMultiplier * 1.5f);
                        currentEffect = Spell.SpellType.None;
                    }
                    else
                    {
                        TakeDamage(spell.damage * stats.damageMultiplier);
                    }
                    break;

                case Spell.SpellType.Ice:
                    Debug.Log("ice");
                    TakeDamage(spell.damage * stats.damageMultiplier);
                    float speed = _navMeshAgent.speed;
                    if (currentEffect == Spell.SpellType.Water)
                    {
                        while (Time.time < duration)
                        {
                            _movement.SetSpeed(0f);
                        }
                        _navMeshAgent.speed = speed;
                    }
                    else
                    {
                        while (Time.time < duration)
                        {
                            _movement.SetSpeed(_navMeshAgent.speed / 2);
                        }

                        _movement.SetSpeed(speed);
                    }

                    break;

                case Spell.SpellType.Water:
                    Debug.Log("Water");
                    currentEffect = Spell.SpellType.Water;
                    break;

                case Spell.SpellType.Earth:
                    break;

                case Spell.SpellType.None:
                    break;
            }
        }

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
