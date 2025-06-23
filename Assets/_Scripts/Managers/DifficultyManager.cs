using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class DifficultyManager : MonoBehaviour
    {
        public Difficulty currentDifficulty = Difficulty.Medium;

        [SerializeField]
        private float[] playerCountMultipliers = { 1.0f, 1.25f, 1.5f, 2.0f }; // 1-4 players
        [SerializeField]
        private float[] difficultyScalingMultipliers = {0.5f, 1f, 1.5f, 2f}; // Easy/Med/Hard/Insane

        private DifficultyScaling _currentDifficultySettings;
        
        public void SetDifficulty(Difficulty difficulty)
        =>
            currentDifficulty = difficulty;
        
        public DifficultyScaling GetDifficultyScaling()
        {
            float spawnMultiplier = playerCountMultipliers[NetworkManager.Singleton.ConnectedClients.Count - 1]; // Adjust based on number of players
            float scaling = difficultyScalingMultipliers[(int)currentDifficulty]; // Adjust based on current difficulty

            return new DifficultyScaling
            {
                SpawnMultiplier = spawnMultiplier,
                SpawnScaling = scaling
            };
        }
        public string GetDifficultyName()
        {
            return currentDifficulty.ToString();
        }
        
    }
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
        Insane
    }
    public class DifficultyScaling
    {
        [Tooltip("base multiplier for the number of enemies spawned")]
        public float SpawnMultiplier;
        
        [Tooltip("How much the amount of enemies increases each round")]
        public float SpawnScaling;
    }
}