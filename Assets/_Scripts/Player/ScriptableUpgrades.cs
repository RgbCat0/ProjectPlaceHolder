using System;
using System.Collections.Generic;
using ArtificeToolkit.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Player
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade", order = 1)]
    public class ScriptableUpgrades : ScriptableObject
    {
        public string upgradeName;

        [Tooltip("Used in place of icon in game jam\n(ex: Di == Damage Increase)")]
        public string shortText; // used in place of icon in game jam (ex: Di == Damage Increase)

        public List<SingleUpgrade> upgrades; // made for multiple upgrades so it can buff 1 and debuff 1 for example

        [Tooltip("higher numbers are more rare\ncannot go below 1"), Min(1)]
        public float rarity = 1; // same system as the wave system

        [Tooltip("Calculated each time the game is started. Only used for displaying.")]
        [ReadOnly, ForceArtifice]
        public float percentageChance; // Only used for displaying.

        [Tooltip("Calculated each time the upgrade UI is called. Only used for displaying.")]
        [ReadOnly, ForceArtifice]
        public float percentageWithLuck; // Only used for displaying.
    }

    [Serializable]
    public class SingleUpgrade
    {
        [EnumToggle, HideLabel]
        public UpgradeTypes type;

        [Tooltip("Percentage to Apply"), 
         ValidateInput(nameof(ValidateValue), "Value cannot be 0.")]
        public float value;
        [Tooltip("Auto-generated description based on type and value")]
        public bool customDescription = true; // if true, description will be generated automatically based on type and value

        [EnableIf(nameof(customDescription), true), Tooltip("Short description of the upgrade\n(ex: +10% Health)")]
        public string description;
        // [Tooltip("Displays on top of the ")]
        // public Image icon; // not used in game jam will be used for full game

        private bool ValidateValue()
        {
            if (value == 0)
            {
                return false;
            }
            return true;
        }

        private bool ValidateValue2()
        {
            if (value < -100 || value > 100)
            {
                return false;
            }
            return true;
        }
        
        public string GenerateDescription()
        {
            if (value < 0)
            {
                return $"{type} -{Mathf.Abs(value)}%";
            }
            return $"{type} +{value}%";
        }
        
    }

    public enum UpgradeTypes
    {
        Health,
        Damage,
        Speed,
        Mana,
        ManaRegen,
        Cooldown,
        Luck
    }
}
