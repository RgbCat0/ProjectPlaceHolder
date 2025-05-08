using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerData : MonoBehaviour
    {
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float manaMultiplier = 1f;
        public float cooldownMultiplier = 1f;

        public void ApplyMultiplierPercentage(
            float health = 0f,
            float damage = 0f,
            float speed = 0f,
            float mana = 0f,
            float cooldown = 0f
        )
        {
            healthMultiplier += health * 0.01f; // Convert percentage to multiplier
            damageMultiplier += damage * 0.01f;
            speedMultiplier += speed * 0.01f;
            manaMultiplier += mana * 0.01f;
            cooldownMultiplier += cooldown * 0.01f;
        }
    }
}
