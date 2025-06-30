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
        [SerializeField]
        private float[] healthScalingMultipliers = {0.2f, 0.4f, 0.6f, 0.8f}; // Easy/Med/Hard/Insane
        [SerializeField]
        private float[] damageScalingMultipliers = {0.2f, 0.4f, 0.6f, 0.8f}; // Easy/Med/Hard/Insane

        private DifficultyScaling _currentDifficultySettings;
        
        public void SetDifficulty(Difficulty difficulty)
        =>
            currentDifficulty = difficulty;
        
        public DifficultyScaling GetDifficultyScaling()
        {
            float spawnMultiplier = playerCountMultipliers[GameManager.Instance.players.Count - 1]; // Adjust based on number of players
            float scaling = difficultyScalingMultipliers[(int)currentDifficulty]; // Adjust based on current difficulty
            float healthScaling = healthScalingMultipliers[(int)currentDifficulty];
            float damageScaling = damageScalingMultipliers[(int)currentDifficulty];

            return new DifficultyScaling
            {
                SpawnMultiplier = spawnMultiplier,
                SpawnScaling = scaling,
                HealthScaling = healthScaling,
                DamageScaling = damageScaling
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

        public float HealthScaling;
        public float DamageScaling;
    }
}