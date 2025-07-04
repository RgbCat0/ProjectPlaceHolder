using LobbyScripts;
using Managers;
using Unity.Netcode;
using UnityEngine;
using System;
using TMPro;

namespace Player
{
    public class PlayerHealth : NetworkBehaviour
    {
        private PlayerStats _playerStats;

        public NetworkVariable<float> Health = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
        public float MaxHealth { get; private set; }

        private Rigidbody _rb;

        public event Action<ulong> onDeath;

        private float regenDelayTimer = 0f; // Time since last hit
        private float regenAcceleration = 1f; // Regen multiplier, grows over time

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

            if (IsServer)
                Health.Value = MaxHealth;

            Health.OnValueChanged += OnHealthChanged;

            SendData();
        }
        [Rpc(SendTo.Everyone)]
        public void SetNameRpc(ulong clientId)
        {
            var name1 = PlayerDataSync.Instance.syncedPlayerList.Find(x => x.PlayerNetworkId == clientId).PlayerName
                .ToString();
            name = name1;
            GetComponentInChildren<TextMeshProUGUI>().text = name1;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (IsOwner)
                Health.OnValueChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(float oldValue, float newValue)
        {
            UIManager.Instance.UpdateHealthBar(newValue, MaxHealth);

            if (newValue <= 0)
            {
                UIManager.Instance.StopHurtFlash();
            }
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

            // Only regen if player is standing still and not in between waves
            if (!WaveManager.Instance.waitingForNextWave.Value)
            {
                regenDelayTimer += Time.deltaTime;

                if (regenDelayTimer >= 1f && Health.Value < MaxHealth)
                {
                    // Heal faster the longer you stay unharmed
                    regenAcceleration += Time.deltaTime; // Gradually increase regen rate
                    float regenAmount = _playerStats.currentHealthRegen * regenAcceleration * Time.deltaTime;

                    RequestHealthRegenServerRpc(regenAmount);
                }
            }
            else
            {
                // Reset acceleration if moving
                regenAcceleration = 1f;
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestHealthRegenServerRpc(float regenAmount)
        {
            if (Health.Value <= 0 || WaveManager.Instance.waitingForNextWave.Value)
                return;

            Health.Value += regenAmount;
            if (Health.Value > MaxHealth)
                Health.Value = MaxHealth;
        }

        [Rpc(SendTo.Server)]
        public void TakeDamageServerRpc(float damage)
        {
            if (Health.Value <= 0)
                return;

            Health.Value -= damage;

            // Reset regen delay and speed on hit
            regenDelayTimer = 0f;
            regenAcceleration = 1f;

            ShowDamageClientRpc();

            if (Health.Value <= 0)
            {
                Die();
            }
        }

        [Rpc(SendTo.Owner)]
        private void ShowDamageClientRpc()
        {
            SoundManager.Instance.PlaySound3D("PlayerHit", transform.position);
            ScreenShake.Instance.Shake(0.4f, 2f, 1.3f);
            UIManager.Instance.HurtFlash();
        }

        private void Die()
        {
            DieRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void DieRpc()
        {
            gameObject.SetActive(false);
            GameManager.Instance.deadPlayers.Add(gameObject);
            onDeath?.Invoke(NetworkObject.NetworkObjectId);
        }
    }
}
