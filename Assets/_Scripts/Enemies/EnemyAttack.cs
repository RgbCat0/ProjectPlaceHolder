using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField]
        private float damage;

        [SerializeField]
        private float attackCooldown = 1f; // ex: 1f

        private float _attackCooldownTimer;
        private AniManager _aniManager;
        
        private void Start()
        {
            _aniManager = transform.parent.GetComponent<AniManager>();
            if (_aniManager == null)
            {
                Debug.LogError("EnemyAnimator component not found on the GameObject.");
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _attackCooldownTimer = 0f;
            StartCoroutine(Attack(other));
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) 
                return;
            _attackCooldownTimer += Time.deltaTime;
            if(_attackCooldownTimer >= attackCooldown)
            {
                _attackCooldownTimer = 0f;
                StartCoroutine(Attack(other));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _attackCooldownTimer = 0f;
        }

        private IEnumerator Attack(Collider other)
        {
            _aniManager.ChangeAnimation("zombattack");
            yield return new WaitForSeconds(0.5f);
            other.GetComponent<IDamageable>()?.TakeDamageRpc(damage);
        }
    }
}