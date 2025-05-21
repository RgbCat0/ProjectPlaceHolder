using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Managers;
using Random = UnityEngine.Random;

namespace _Scripts.Player
{
    public class PlayerStats : NetworkBehaviour
    {
        // Base stats
        [Header("----------Base Stats----------")]
        public float baseMana;
        public float baseMaxMana = 100f;
        public float baseManaRegen = 1f;
        public float baseMaxHealth = 100f;
        public float baseHealthRegen = 1f;

        // Current active stats modified by upgrades
        [Header("----------Current Stats----------")]
        public float currentMana;
        public float currentMaxMana;
        public float currentManaRegen;
        public float currentMaxHealth;
        public float currentHealthRegen;

        private float manaRegenTimer;
        public List<ScriptableUpgrades> upgrades = new();

        [Header("----------Multipliers----------")]
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float manaMultiplier = 1f;
        public float manaRegenMultiplier = 1f;
        public float cooldownMultiplier = 1f;

        public event Action<float> OnHealthChanged;
        public event Action<float> OnDamageChanged;
        public event Action<float> OnSpeedChanged;
        public event Action<float> OnManaChanged;
        public event Action<float> OnManaRegenChanged;
        public event Action<float> OnCooldownChanged;

        private void Start()
        {
            currentMana = baseMaxMana;
            Debug.Log(JsonConvert.SerializeObject(upgrades));
            upgrades = Resources.LoadAll<ScriptableUpgrades>("Upgrades").ToList();
        }

        private void Update()
        {
            currentMaxHealth = baseMaxHealth * healthMultiplier;
            currentMaxMana = baseMaxMana * manaMultiplier;

            if (currentMana < currentMaxMana)
            {
                manaRegenTimer += Time.deltaTime;
                if (manaRegenTimer >= 1f)
                {
                    currentMana += currentManaRegen;
                    if (currentMana > currentMaxMana)
                        currentMana = currentMaxMana;
                    manaRegenTimer = 0f;
                }
            }
            if (currentMana > currentMaxMana)
                currentMana = currentMaxMana;
            UIManager.Instance.UpdateManaBar(currentMana, currentMaxMana);
        }

        public void ApplyUpgrade(SingleUpgrade upgrade)
        {
            switch (upgrade.type)
            {
                case UpgradeTypes.Health:
                    healthMultiplier += upgrade.value / 100f;
                    OnHealthChanged?.Invoke(healthMultiplier);
                    break;
                case UpgradeTypes.Damage:
                    damageMultiplier += upgrade.value / 100f;
                    OnDamageChanged?.Invoke(damageMultiplier);
                    break;
                case UpgradeTypes.Speed:
                    speedMultiplier += upgrade.value / 100f;
                    OnSpeedChanged?.Invoke(speedMultiplier);
                    break;
                case UpgradeTypes.Mana:
                    manaMultiplier += upgrade.value / 100f;
                    OnManaChanged?.Invoke(manaMultiplier);
                    break;
                case UpgradeTypes.ManaRegen:
                    manaRegenMultiplier += upgrade.value / 100f;
                    OnManaRegenChanged?.Invoke(manaRegenMultiplier);
                    break;
                case UpgradeTypes.Cooldown:
                    cooldownMultiplier -= upgrade.value / 100f;
                    OnCooldownChanged?.Invoke(cooldownMultiplier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<ScriptableUpgrades> GetRandomUpgrades(int count) // dupes allowed
        {
            List<ScriptableUpgrades> selectedUpgrades = new();

            for (int i = 0; i < count; i++)
            {
                // Calculate weights (inverted rarity: lower rarity = higher chance)
                var weights = upgrades.Select(u => 1f / u.rarity).ToList();
                float totalWeight = weights.Sum();
                float roll = Random.Range(0f, totalWeight);
                float cumulative = 0f;

                for (int j = 0; j < upgrades.Count; j++)
                {
                    cumulative += weights[j];
                    if (roll <= cumulative)
                    {
                        selectedUpgrades.Add(upgrades[j]);
                        break;
                    }
                }
            }

            if (selectedUpgrades.Count == 0)
            {
                Debug.LogWarning("Err no upgrades found");
                return null;
            }

            return selectedUpgrades;
        }
    }
}
