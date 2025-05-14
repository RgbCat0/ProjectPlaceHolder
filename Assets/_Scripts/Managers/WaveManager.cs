using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using _Scripts.Enemies;

namespace _Scripts.Managers
{
    public class WaveManager : NetworkBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [SerializeField]
        private List<WaveInfo> waves = new();

        [SerializeField]
        private List<Transform> enemies = new();

        [SerializeField]
        private int currentWaveIndex;

        private Transform _enemyParent;

        [SerializeField]
        private NetworkObject enemyBasePrefab;

        [SerializeField]
        private List<Transform> spawnPoints = new();
        private bool _waitingForUpgrade = false;
        private int _playersDoneUpgrading;

        // events
        public event Action OnWaveCompleteEvent; // send from this script to notify other scripts
        public event Action StartNextWaveEvent; // used in another script to notify starting the next wave

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            StartNextWaveEvent += StartNextWave;
        }

        public void Init()
        {
            _enemyParent = GameObject.Find("EnemyParent").transform;
            spawnPoints = GameObject
                .Find("EnemySpawnPoints")
                .transform.GetComponentsInChildren<Transform>()
                .ToList();
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
                NetworkObject enemy = NetworkManager.SpawnManager.InstantiateAndSpawn(
                    enemyBasePrefab,
                    position: spawnPoint.position,
                    rotation: Quaternion.identity
                );
                enemy.transform.SetParent(_enemyParent);
                enemy.GetComponent<Enemy>().Initialize(enemyInfo, spawnPoint.position);
                enemies.Add(enemy.transform);
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        private void FixedUpdate()
        {
            if (enemies.Count == 0 && !_waitingForUpgrade)
            {
                _waitingForUpgrade = true;
                OnWaveCompleteEvent?.Invoke();
            }
        }

        public void ReportPlayerUpgradeDone()
        {
            _playersDoneUpgrading++;
            if (_playersDoneUpgrading == NetworkManager.Singleton.ConnectedClients.Count)
            {
                _waitingForUpgrade = false;
                _playersDoneUpgrading = 0;
                StartNextWaveEvent?.Invoke();
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
