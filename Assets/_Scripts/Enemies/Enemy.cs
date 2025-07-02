using System.Collections;
using System.Globalization;
using Managers;
using Player;
using Player.Attack;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static Player.Attack.Spell;

namespace Enemies
{
    public class Enemy : NetworkBehaviour, IDamageable
    {
        private SpellType _currentEffect = SpellType.None;

        // private EnemyAttack _attack;
        private EnemyMovement _movement;
        private NavMeshAgent _navMeshAgent;
        private Spell _spell;
        private PlayerStats _playerStats;
        private EnemyHealthBar _healthBar;
        private EnemyAttack _enemyAttack;
        public float Health { get; private set; } = 100f;

        private bool isDead;
        [SerializeField]
        private GameObject damageNumberPrefab; // prefab for damage numbers

#if UNITY_EDITOR
        [SerializeField]
        private bool debug; // for logging purposes, can be set in the inspector
#endif
        private void Awake()
        {
            _movement = GetComponent<EnemyMovement>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _healthBar = GetComponent<EnemyHealthBar>();
            _enemyAttack = GetComponentInChildren<EnemyAttack>();
            _navMeshAgent.enabled = false; // causes weird spawning issues if enabled immediately
        }


        #region init

        public void Initialize(EnemyInfo enemyInfo, Vector3 spawnPoint, float healthMulti, float damageMulti,
            bool debug1 = false)
        {
            Health = enemyInfo.health * ( 1 + healthMulti);
            _movement.SetSpeed(enemyInfo.speed);
            _enemyAttack.damage = enemyInfo.damage * (1 + damageMulti);
            transform.position = spawnPoint;
            ClientInitRpc(enemyInfo.identifier);
            StartCoroutine(SpawnAnimation());

            if (debug1)
                _movement.SetSpeed(0f); // UNITY_EDITOR debugging
        }

        private IEnumerator SpawnAnimation() // moves the player 2f underground and lerps up
        {
            Vector3 startPosition = transform.position;
            Vector3 downUnder = startPosition + Vector3.down * 2.2f;
            float elapsedTime = 0f;
            float duration = 1f; // duration of the spawn animation
            transform.position = downUnder;

            // Move back to original position
            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(downUnder, startPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _navMeshAgent.enabled = true; // enable NavMeshAgent after setting position and speed
        }

        [Rpc(SendTo.Everyone)]
        private void ClientInitRpc(string enemyInfoIdentifier)
        {
            int index = WaveManager.Instance.enemyTypesToSpawn.FindIndex(e => e.info.name == enemyInfoIdentifier);
            EnemyInfo enemyInfo = WaveManager.Instance.enemyTypesToSpawn[index].info;
            GameObject model = Instantiate(enemyInfo.modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
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
                    TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier);
                    break;
            }
        }

        #region reactions

        private IEnumerator ApplyFire()
        {
            float duration = Time.time + _spell.effectDuration;
            if (_currentEffect == SpellType.Water)
            {
                TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier * 1.5f);
                _currentEffect = SpellType.None;
            }
            else
            {
                _currentEffect = _spell.spellType;
                TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier);

                while (Time.time < duration)
                {
                    TakeDamageRpc(_spell.effectDamage);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            yield return null;
        }

        private void ApplyWater()
        {
            _currentEffect = SpellType.Water;
            TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier);
        }

        private IEnumerator ApplyIce()
        {
            Debug.Log("ice");
            TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier);
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
                TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier * 1.5f);
                _currentEffect = SpellType.None;
            }
            else
            {
                TakeDamageRpc(_spell.damage * _playerStats.damageMultiplier);
            }
        }

        #endregion


        #region Health

        // ReSharper disable Unity.PerformanceAnalysis
        public void TakeDamageRpc(float damage)
        {
            if (!isDead)
            {
                Health -= damage;
                SoundManager.Instance.PlaySound3D("EnemyHit", transform.position);
                DamageNumbersRpc(damage);
                _healthBar.UpdateHealthBarRpc(Health);
                if (Health <= 0f)
                    DieRpc();
            }
#if UNITY_EDITOR
            if (debug)
                Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {Health}");
#endif
        }

        //ensure running on server
        [Rpc(SendTo.Server)]
        private void DieRpc()
        {
            isDead = true;
            WaveManager.Instance.EnemyDeath(NetworkObject);
            NetworkObject.GetComponent<EnemyMovement>().enabled = false;
            NetworkObject.GetComponent<EnemyMovement>().StopTarget();
            _navMeshAgent.enabled = false;
             Destroy(_enemyAttack);
             _healthBar.enabled = false;
            gameObject.GetComponentInChildren<Animator>().enabled = false;  
            gameObject.GetComponentInChildren<CapsuleCollider>().enabled = false;
            
            StartCoroutine(FallAndDespawn());
        }

        private IEnumerator FallAndDespawn()
        {
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.Euler(90f, transform.eulerAngles.y, transform.eulerAngles.z);
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * 0.5f;

            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.rotation = endRot;
            transform.position = endPos;

            yield return new WaitForSeconds(2.5f);
            
            Vector3 sinkStart = transform.position;
            Vector3 sinkEnd = sinkStart + Vector3.down * 2f;
            float sinkDuration = 1f;
            elapsed = 0f;
            while (elapsed < sinkDuration)
            {
                transform.position = Vector3.Lerp(sinkStart, sinkEnd, elapsed / sinkDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = sinkEnd;

            NetworkObject.Despawn();
        }
        

        [Rpc(SendTo.Everyone)]
        private void DamageNumbersRpc(float damage)
        {
            if (PlayerPrefs.GetInt("DamageNumbersEnabled") == 0)
                return;
            GameObject damageNumber = Instantiate(damageNumberPrefab, transform.position + Vector3.up * 2f,
                Quaternion.identity);
            damageNumber.GetComponent<HitAnimation>().ShowHitText(damage.ToString(CultureInfo.CurrentCulture));
            damageNumber.transform.parent = transform;
        }

        #endregion
    }
}