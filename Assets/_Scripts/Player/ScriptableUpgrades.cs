using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    }

    [Serializable]
    public class SingleUpgrade
    {
        public UpgradeTypes type;

        [Tooltip("Percentage to Apply")]
        public float value;

        [Tooltip("Short description of the upgrade\n(ex: +10% Health)")]
        public string description;
        // [Tooltip("Displays on top of the ")]
        // public Image icon; // not used in game jam will be used for full game
    }

    public enum UpgradeTypes
    {
        Health,
        Damage,
        Speed,
        Mana,
        ManaRegen,
        Cooldown,
    }
}
