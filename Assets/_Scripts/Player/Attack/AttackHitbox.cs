using System;
using Enemies;
using Unity.Netcode;
using UnityEngine;

namespace Player.Attack
{
    public class AttackHitbox : MonoBehaviour
    {
        private PlayerStats _playerStats;
        private AttackManager _attackManager;
        private Collider _collider;

        [SerializeField]
        private ParticleSystem particle;

        private void Awake()
        {
            particle = GetComponent<ParticleSystem>();
        }

        public void SetCaster(GameObject player)
        {
            _playerStats = player.GetComponent<PlayerStats>();
            _attackManager = player.GetComponent<AttackManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_playerStats == null || _attackManager == null)
            {
                return;
            }

            if (other.CompareTag("Enemy"))
            {
                _collider = other;
                SendDamageRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void SendDamageRpc()
        {
            Enemy enemy = _collider.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetAttacker(_attackManager.GetCastedSpell(), _playerStats);
            }
            else
            {
                Debug.LogWarning("Enemy script not found.");
            }
        }

    }
}
