using UnityEngine;

namespace _Scripts.Enemies
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField]
        private float damage;

        [SerializeField]
        private float attackCooldown = 1f; // ex: 1f

        private float _attackCooldownTimer;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _attackCooldownTimer = 0f;
            Attack(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player")) 
                return;
            _attackCooldownTimer += Time.deltaTime;
            if(_attackCooldownTimer >= attackCooldown)
            {
                _attackCooldownTimer = 0f;
                Attack(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;
            _attackCooldownTimer = 0f;
        }

        private void Attack(Collider other) => other.GetComponent<IDamageable>()?.TakeDamage(damage);
    }
}