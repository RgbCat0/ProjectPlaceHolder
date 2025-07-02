using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Enemies
{
    public class EnemyAttack : NetworkBehaviour
    {
        public float damage;

        [SerializeField]
        private float attackCooldown = 1f; // ex: 1f

        private float _attackCooldownTimer;
        private AniManager _aniManager;
        public bool isAttacking;
        private bool _playerInsideTrigger;

        private void Start()
        {
            _aniManager = transform.parent.GetComponent<AniManager>();
            if (_aniManager == null)
            {
                Debug.LogError("EnemyAnimator component not found on the GameObject.");
            }

            _aniManager.ChangeFloat("AttackSpeed", 2f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _attackCooldownTimer = 0f;
            _playerInsideTrigger = true;
            StartCoroutine(Attack(other));
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _attackCooldownTimer += Time.deltaTime;
            if (_attackCooldownTimer >= attackCooldown)
            {
                _attackCooldownTimer = 0f;
                StartCoroutine(Attack(other));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _playerInsideTrigger = false;
            _attackCooldownTimer = 0f;
        }

        private IEnumerator Attack(Collider other)
        {
            isAttacking = true;
            _aniManager.ChangeAnimation("Attack", 0.2f, 1);
            SoundManager.Instance.PlaySound3D("EnemyAttack", transform.position);
            yield return new WaitForSeconds(0.5f);
            if (_playerInsideTrigger)
                other.GetComponent<IDamageable>()?.TakeDamageRpc(damage);
            // no need to change animation again as it will be changed by the movement script
            _aniManager.ChangeAnimation("Idle", 0.5f, 1);
            isAttacking = false;
        }
    }
}