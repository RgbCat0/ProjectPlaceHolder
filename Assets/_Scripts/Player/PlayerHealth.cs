using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using _Scripts.LobbyScripts;
using _Scripts.Managers;

namespace _Scripts.Player
{
    public class PlayerHealth : NetworkBehaviour, IDamageable
    {
        private PlayerStats _playerStats;

        public float Health { get; private set; }
        public float MaxHealth { get; private set; }

        private float healthRegenTimer = 0f;

        private void Start()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
            _playerStats = GetComponent<PlayerStats>();
            MaxHealth = _playerStats.baseMaxHealth;
            Health = MaxHealth; // Set initial health
            SendData();
        }

        private void SendData() =>
            SetPlayerDataSyncRpc(NetworkManager.LocalClientId, NetworkObject.NetworkObjectId);

        [Rpc(SendTo.Server)]
        private void SetPlayerDataSyncRpc(ulong networkManagerId, ulong playerObjectId)
        {
            var playerDataSync = PlayerDataSync.Instance;
            var playerList = playerDataSync.syncedPlayerList;

            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].PlayerNetworkId == networkManagerId)
                {
                    playerDataSync.localPlayerObjectId = playerObjectId;
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
            UIManager.Instance.UpdateHealthBar(Health, MaxHealth); // ui manager is not a network object

            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= 1f)
            {
                Health += _playerStats.currentHealthRegen;
                if (Health > MaxHealth)
                    Health = MaxHealth;
                healthRegenTimer = 0f;
            }
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                DieRpc();
            }
            UIManager.Instance.UpdateHealthBar(Health, MaxHealth);
        }

        [Rpc(SendTo.Server)]
        private void DieRpc()
        {
            // Handle player death (e.g., play animation, destroy object, etc.)
            Debug.Log($"{gameObject.name} has died.");
            // NetworkObject.Despawn();
        }
    }
}
