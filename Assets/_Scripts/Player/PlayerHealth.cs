using LobbyScripts;
using Managers;
using Unity.Netcode;
using UnityEngine;
using System;

namespace Player
{
    public class PlayerHealth : NetworkBehaviour, IDamageable
    {
        private PlayerStats _playerStats;

        public float Health { get; set; }
        public float MaxHealth { get; private set; }

        public event Action<ulong> onDeath;

        private float healthRegenTimer = 0f;
        private Rigidbody _rb;

        private void Start()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _playerStats = GetComponent<PlayerStats>();
            _rb = GetComponent<Rigidbody>();
            MaxHealth = _playerStats.baseMaxHealth;
            Health = MaxHealth; // Set initial health
            SendData();
        }

        private void SendData()
        {
            PlayerDataSync.Instance.localPlayerObjectId = NetworkObject.NetworkObjectId;
            SetPlayerDataSyncRpc(NetworkManager.LocalClientId, NetworkObject.NetworkObjectId);
        }

        [Rpc(SendTo.Server)]
        private void SetPlayerDataSyncRpc(ulong networkManagerId, ulong playerObjectId)
        {
            var playerDataSync = PlayerDataSync.Instance;
            var playerList = playerDataSync.syncedPlayerList;

            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].PlayerNetworkId == networkManagerId)
                {
                    var data = playerList[i];
                    data.InGameObjectId = playerObjectId;
                    playerList[i] = data;
                    break;
                }
            }

            playerDataSync.SendFullListRpc();
        }

        private void Update()
        {
            if (!IsOwner)
                return;
            MaxHealth = _playerStats.currentMaxHealth;
            UIManager.Instance.UpdateHealthBar(Health, MaxHealth);

            healthRegenTimer += Time.deltaTime;
            if ((healthRegenTimer >= 1f &&
                 _rb.linearVelocity.magnitude < 0.1f) &&
                !WaveManager.Instance.waitingForNextWave.Value) // Only regenerate health if the player is not moving and not waiting for the next wave
            {
                Health += _playerStats.currentHealthRegen;
                if (Health > MaxHealth)
                    Health = MaxHealth;
                healthRegenTimer = 0f;
            }
        }

        [Rpc(SendTo.Owner)]
        public void TakeDamageRpc(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                UIManager.Instance.UpdateHealthBar(0, MaxHealth);
                UIManager.Instance.StopHurtFlash();
                Health = MaxHealth; // Prevents die Rpc from being called multiple times
                DieRpc();
                return;
            }
            ScreenShake.Instance.Shake(0.4f, 1f, 1f);
            UIManager.Instance.UpdateHealthBar(Health, MaxHealth);
            UIManager.Instance.HurtFlash();
        }

        [Rpc(SendTo.Everyone)]
        private void DieRpc()
        {
            // Handle player death (e.g., play animation, destroy object, etc.)
            gameObject.SetActive(false);
            GameManager.Instance.deadPlayers.Add(gameObject);
            onDeath?.Invoke(NetworkObject.NetworkObjectId);
        }
    }
}