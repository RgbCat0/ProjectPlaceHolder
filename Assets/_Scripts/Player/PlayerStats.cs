using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
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
        public float currentLuck;

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
            upgrades = Resources.LoadAll<ScriptableUpgrades>("Upgrades").ToList();
            Debug.Log(JsonConvert.SerializeObject(upgrades));
            CalculateUpgradeChance();
        }

        private void Update()
        {
            if (!IsOwner)
                return;
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
                case UpgradeTypes.Luck:

                    currentLuck += upgrade.value;
                    CalculateUpgradeChanceWithLuck(currentLuck);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<ScriptableUpgrades> GetRandomUpgrades(int count)
        {
            List<ScriptableUpgrades> selectedUpgrades = new();
            CalculateUpgradeChanceWithLuck(currentLuck);

            // Adjust curve: lower curve = more likely to get rare upgrades
            float curve = 1f / (1f + Mathf.Max(currentLuck, -0.99f)); // Prevent divide by 0 or negative root

            for (int i = 0; i < count; i++)
            {
                var weights = upgrades.Select(u => 1f / Mathf.Pow(u.rarity, curve)).ToList();
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

            return selectedUpgrades;
        }

        // misc
        private void CalculateUpgradeChance()
        {
            float totalInverseRarity = upgrades.Sum(u => 1f / u.rarity);

            foreach (var upgrade in upgrades)
            {
                upgrade.percentageChance = (1f / upgrade.rarity) / totalInverseRarity * 100f;
            }
        }

        private void CalculateUpgradeChanceWithLuck(float luck)
        {
            float curve = 1f / (1f + Mathf.Max(luck, -0.99f)); // Prevent invalid math

            var weights = upgrades.Select(u => 1f / Mathf.Pow(u.rarity, curve)).ToList();
            float totalWeight = weights.Sum();

            for (int i = 0; i < upgrades.Count; i++)
            {
                upgrades[i].percentageWithLuck = weights[i] / totalWeight * 100f;
            }
        }
    }
}
