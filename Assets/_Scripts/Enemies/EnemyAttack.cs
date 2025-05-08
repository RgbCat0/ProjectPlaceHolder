using System;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField]
        private float damage;

        [SerializeField]
        private float attackCooldown; // ex: 1f

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    Attack(other);
                    Debug.Log($"{other.name} took {damage} damage from {gameObject.name}.");
                }
                else
                {
                    Debug.LogWarning($"{other.name} does not implement IDamageable.");
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Check if the attack is on cooldown
                    if (Time.time >= attackCooldown)
                    {
                        Attack(other);
                        Debug.Log($"{other.name} took {damage} damage from {gameObject.name}.");
                        attackCooldown = Time.time + attackCooldown; // Reset cooldown
                    }
                }
                else
                {
                    Debug.LogWarning($"{other.name} does not implement IDamageable.");
                }
            }
        }

        private void Attack(Collider other) => other.GetComponent<IDamageable>().TakeDamage(damage);
    }
}
