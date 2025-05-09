using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.Player
{
    public class PlayerUpgrade : MonoBehaviour
    {
        public List<Upgrade> upgrades = new();
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float manaMultiplier = 1f;
        public float cooldownMultiplier = 1f;

        public event Action<float> OnHealthChanged;
        public event Action<float> OnDamageChanged;
        public event Action<float> OnSpeedChanged;
        public event Action<float> OnManaChanged;
        public event Action<float> OnCooldownChanged;

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
                case UpgradeTypes.Cooldown:
                    cooldownMultiplier += upgrade.value / 100f;
                    OnCooldownChanged?.Invoke(cooldownMultiplier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public List<Upgrade> GetRandomUpgrades(int count) // dupes allowed
        {
            List<Upgrade> selectedUpgrades = new();

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

    [Serializable]
    public class Upgrade
    {
        public string name;

        [Tooltip("Used in place of icon in game jam (ex: Di == Damage Increase)")]
        public string shortText; // used in place of icon in game jam (ex: Di == Damage Increase)
        public List<SingleUpgrade> upgrades; // made for multiple upgrades so it can buff 1 and debuff 1 for example

        [Tooltip("higher numbers are more rare")]
        public float rarity; // same system as the wave system
    }

    [Serializable]
    public class SingleUpgrade
    {
        public UpgradeTypes type;

        [Tooltip("Percentage to Apply")]
        public float value;
        public string description;
        public Image icon; // not used in game jam will be used for full game
    }

    public enum UpgradeTypes
    {
        Health,
        Damage,
        Speed,
        Mana,
        Cooldown
    }
}
