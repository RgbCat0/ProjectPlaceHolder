using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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
        private Spell _spell;
        private PlayerStats _playerStats;
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

        public void SetAttacker(Spell castedSpell, PlayerStats playerStats)
        {
            _spell = castedSpell;
            _playerStats = playerStats;
            ApplyElementEffectRpc();
        }

        [Rpc(SendTo.Server)]
        public void ApplyElementEffectRpc()
        {
            Debug.Log(currentEffect);
            switch (_spell.spellType)
            {
                case Spell.SpellType.Fire:
                    StartCoroutine(ApplyFire());
                    break;

                case Spell.SpellType.Lightning:
                    ApplyLightning();
                    break;

                case Spell.SpellType.Ice:
                    ApplyIce();
                    break;

                case Spell.SpellType.Water:
                    ApplyWater();
                    break;

                case Spell.SpellType.Earth:
                    break;

                case Spell.SpellType.None:
                    break;
            }
        }

        #region reactions
        private IEnumerator ApplyFire()
        {
            float duration = Time.time + _spell.effectDuration;
            if (currentEffect == Spell.SpellType.Water)
            {
                TakeDamage(_spell.damage * _playerStats.damageMultiplier * 1.5f);
                currentEffect = Spell.SpellType.None;
            }
            else
            {
                currentEffect = _spell.spellType;
                TakeDamage(_spell.damage * _playerStats.damageMultiplier);

                while (Time.time < duration)
                {
                    TakeDamage(_spell.effectDamage);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            yield return null;
        }

        private void ApplyWater()
        {
            currentEffect = Spell.SpellType.Water;
        }

        private void ApplyIce()
        {
            Debug.Log("ice");
            float duration = Time.time + _spell.effectDuration;
            TakeDamage(_spell.damage * _playerStats.damageMultiplier);
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
        }

        private void ApplyLightning()
        {
            Debug.Log("Lightning");
            if (currentEffect == Spell.SpellType.Water)
            {
                TakeDamage(_spell.damage * _playerStats.damageMultiplier * 1.5f);
                currentEffect = Spell.SpellType.None;
            }
            else
            {
                TakeDamage(_spell.damage * _playerStats.damageMultiplier);
            }
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
