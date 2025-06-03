using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using _Scripts.Managers;
using _Scripts.Player;
using static Spell;

namespace _Scripts.Enemies
{
    public class Enemy : NetworkBehaviour, IDamageable
    {
        private SpellType _currentEffect = SpellType.None;
        // private EnemyAttack _attack;
        private EnemyMovement _movement;
        private NavMeshAgent _navMeshAgent;
        private Spell _spell;
        private PlayerStats _playerStats;
        public float Health { get; private set; } = 100f;

#if UNITY_EDITOR
        [SerializeField]
        private bool debug; // for logging purposes, can be set in the inspector
#endif


        #region init

        public void Initialize(EnemyInfo enemyInfo, Vector3 spawnPoint, bool debug1 = false)
        {
            // _attack = GetComponent<EnemyAttack>(); unused kept for future reference
            _movement = GetComponent<EnemyMovement>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            Health = enemyInfo.health;
            _movement.SetSpeed(enemyInfo.speed);
            // Debug.Log(spawnPoint);
            // transform.position = spawnPoint;
            GameObject model = Instantiate(enemyInfo.modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
            if (debug1)
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
            Debug.Log(_currentEffect);
            switch (_spell.spellType)
            {
                case SpellType.Fire:
                    StartCoroutine(ApplyFire());
                    break;

                case SpellType.Lightning:
                    ApplyLightning();
                    break;

                case SpellType.Ice:
                    StartCoroutine(ApplyIce());
                    break;

                case SpellType.Water:
                    ApplyWater();
                    break;

                case SpellType.Earth:
                    break;

                case SpellType.None:
                    TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value);
                    break;
            }
        }

        #region reactions

        private IEnumerator ApplyFire()
        {
            float duration = Time.time + _spell.effectDuration;
            if (_currentEffect == SpellType.Water)
            {
                TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value * 1.5f);
                _currentEffect = SpellType.None;
            }
            else
            {
                _currentEffect = _spell.spellType;
                TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value);

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
            _currentEffect = SpellType.Water;
            TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value);
        }

        private IEnumerator ApplyIce()
        {
            Debug.Log("ice");
            TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value);
            float speed = _navMeshAgent.speed;
            if (_currentEffect == SpellType.Water)
            {
                _movement.SetSpeed(0f);
                yield return new WaitForSeconds(_spell.effectDuration);
                _navMeshAgent.speed = speed;
            }
            else
            {
                _movement.SetSpeed(_navMeshAgent.speed / 2);
                yield return new WaitForSeconds(_spell.effectDuration);
                _movement.SetSpeed(speed);
            }
        }

        private void ApplyLightning()
        {
            Debug.Log("Lightning");
            if (_currentEffect == SpellType.Water)
            {
                TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value * 1.5f);
                _currentEffect = SpellType.None;
            }
            else
            {
                TakeDamage(_spell.damage * _playerStats.damageMultiplier.Value);
            }
        }

        #endregion


        #region Health

        // ReSharper disable Unity.PerformanceAnalysis
        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0f)
                DieRpc();
#if UNITY_EDITOR
            if (debug)
                Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {Health}");
#endif
        }

        //ensure running on server
        [Rpc(SendTo.Server)]
        private void DieRpc()
        {
            // Handle enemy death (e.g., play animation, destroy object, etc.)
            WaveManager.Instance.EnemyDeath(NetworkObject);
#if UNITY_EDITOR
            if(debug)
                Debug.Log($"{gameObject.name} has died.");
#endif
            NetworkObject.Despawn();
        }

        #endregion
    }
}