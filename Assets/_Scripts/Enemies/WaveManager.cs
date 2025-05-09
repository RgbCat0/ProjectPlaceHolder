using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField]
        private List<WaveInfo> waves = new();

        [SerializeField]
        private List<Transform> enemies = new();

        [SerializeField]
        private int currentWaveIndex = 0;

        [SerializeField]
        private Transform enemyParent;

        [SerializeField]
        private GameObject enemyBasePrefab;

        [SerializeField]
        private List<Transform> spawnPoints = new();
        private bool _waitingForUpgrade = false;

        // events
        public event Action OnWaveCompleteEvent; // send from this script to notify other scripts
        public event Action StartNextWaveEvent; // used in another script to notify starting the next wave

        private void Awake()
        {
            StartNextWaveEvent += StartNextWave;
        }

        private void Start()
        {
            if (waves.Count == 0)
            {
                Debug.LogError("No waves set up");
                return;
            }

            currentWaveIndex--; // start at -1 to trigger the first wave
            StartNextWaveEvent?.Invoke(); // only start the first wave
        }

        private void StartNextWave()
        {
            currentWaveIndex++;
            if (currentWaveIndex >= waves.Count)
            {
                Debug.Log("All waves completed");
                return;
            }

            StartCoroutine(StartWave());
        }

        private IEnumerator StartWave()
        {
            WaveInfo currentWave = waves[currentWaveIndex];
            yield return new WaitForSeconds(currentWave.startDelay);
            while (true)
            {
                if (enemies.Count >= currentWave.enemyCount)
                    yield break;
                EnemyInfo enemyInfo = currentWave.GetRandomInfo();
                Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
                GameObject enemy = Instantiate(
                    enemyBasePrefab,
                    spawnPoint.position,
                    Quaternion.identity
                );
                enemy.transform.SetParent(enemyParent);
                enemy.GetComponent<Enemy>().Initialize(enemyInfo, spawnPoint.position);
                enemies.Add(enemy.transform);
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        private void FixedUpdate()
        {
            if (enemies.Count == 0)
            {
                OnWaveCompleteEvent?.Invoke();
            }
        }
    }

    [Serializable]
    public class WaveInfo
    {
        public List<EnemySpawnInfo> enemyTypesToSpawn;
        public int enemyCount;

        [Tooltip("Time between enemy spawns in seconds")]
        public float spawnInterval;

        [Tooltip("Delay before the first enemy spawns in seconds")]
        public float startDelay;

        public EnemyInfo GetRandomInfo()
        {
            float totalChance = enemyTypesToSpawn.Sum(e => e.spawnChance);
            float roll = UnityEngine.Random.Range(0f, totalChance);
            float cumulative = 0f;

            foreach (var enemy in enemyTypesToSpawn)
            {
                cumulative += enemy.spawnChance;
                if (roll <= cumulative)
                    return enemy.info;
            }

            return enemyTypesToSpawn[0].info; // fallback
        }
    }

    [Serializable]
    public class EnemySpawnInfo
    {
        public EnemyInfo info;

        public float spawnChance;
    }
}
