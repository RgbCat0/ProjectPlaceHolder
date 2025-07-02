using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemies;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Managers
{
    public class WaveManager : NetworkBehaviour
    {
        // [SerializeField]
        // private List<WaveInfo> waves = new();

        [SerializeField]
        private List<Transform> enemies = new();

        [SerializeField]
        private int
            currentWaveIndex;

        [SerializeField]
        private NetworkObject enemyBasePrefab;

        [SerializeField]
        private List<Transform> spawnPoints = new();

        [Header("Spawn Settings")] // not using waveinfo anymore as the waves will be automatically generated
        [SerializeField]
        private float spawnInterval = 0.4f; // time between enemy spawns in seconds

        [SerializeField]
        private int baseEnemyCount = 30; // base number of enemies to spawn in a wave

        [SerializeField]
        private float startDelay = 2f; // delay before the first enemy spawns in seconds

        // private int _currentAmountToSpawn = 0;
        public NetworkVariable<bool> waitingForNextWave; // used to prevent mana and health regen during wave transition

        private DifficultyScaling _currentDifficultyScaling;
        public List<EnemySpawnInfo> enemyTypesToSpawn;

        private Transform _enemyParent;
        private int _playersDoneUpgrading;
        private bool _waitingForUpgrade;
        public static WaveManager Instance { get; private set; }
#if UNITY_EDITOR
        [Header("Debug")]
        public bool disableSpawning; // for testing without spawning enemies

        public bool disableMovement; // for testing spawns but not movement
#endif

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

        // events
        public event Action OnWaveCompleteEvent; // send from this script to notify other scripts
        public event Action StartNextWaveEvent; // used in another script to notify starting the next wave

        #region init

        /// <summary>
        /// Tells the WaveManager to initialize and start the first wave.
        /// </summary>
        public void Init()
        {
            waitingForNextWave.Initialize(this);
            _enemyParent = GameObject.Find("EnemyParent").transform;
            var spawnPointParent = GameObject.FindWithTag("EnemySpawnpoint");
            spawnPoints = spawnPointParent
                .transform.GetComponentsInChildren<Transform>()
                .Where(t => t != spawnPointParent.transform)
                .ToList();
            // if (waves.Count == 0)
            // {
            // Debug.LogError("No waves set up");
            // return;
            // }
            // load all wave info from resources folder
            // waves = Resources.LoadAll<WaveInfo>("Waves").ToList();
            _currentDifficultyScaling = GetComponent<DifficultyManager>().GetDifficultyScaling();

            currentWaveIndex--; // start at -1 to trigger the first wave
#if UNITY_EDITOR
            if (disableSpawning)
            {
                Debug.Log("Spawning disabled");
                return;
            }
#endif
            StartNextWaveEvent?.Invoke(); // only start the first wave
        }

        #endregion

        private void StartNextWave()
        {
            if (!IsHost)
                return;
            waitingForNextWave.Value = false;
            currentWaveIndex++;
            UIManager.Instance.UpdateWaveText(currentWaveIndex);
            StartCoroutine(StartWave());
        }

        // new method to incorporate the new wave system and difficulty scaling
        public IEnumerator StartWave()
        {
            yield return new WaitForSeconds(startDelay);
            int enemyCount = Mathf.RoundToInt((baseEnemyCount * _currentDifficultyScaling.SpawnMultiplier) *
                                              _currentDifficultyScaling.SpawnScaling * ((currentWaveIndex + 1) / 6));
            float healthScaling = _currentDifficultyScaling.HealthScaling * (currentWaveIndex + 1);
            float damageScaling = _currentDifficultyScaling.DamageScaling * (currentWaveIndex + 1);
            while (true)
            {
                if (enemies.Count >= enemyCount || GameManager.Instance.gameOver)
                {
                    yield break;
                }

                EnemyInfo enemyInfo = GetRandomInfo();
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                NetworkObject enemy = NetworkManager.SpawnManager.InstantiateAndSpawn(
                    enemyBasePrefab,
                    position: spawnPoint.position,
                    rotation: Quaternion.identity
                );
                enemy.transform.SetParent(_enemyParent);

                enemy.GetComponent<Enemy>().Initialize(enemyInfo, spawnPoint.position, healthScaling, damageScaling, disableMovement);

                enemies.Add(enemy.transform);
                UIManager.Instance.UpdateEnemiesRemainText(enemies.Count);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        public EnemyInfo GetRandomInfo()
        {
            List<(EnemySpawnInfo enemy, float chance)> eligibleEnemies = new();

            foreach (EnemySpawnInfo enemy in enemyTypesToSpawn)
            {
                var applicableSpawn = enemy.spawnChanceList
                    .Where(f => f.startWave <= currentWaveIndex)
                    .OrderByDescending(f => f.startWave)
                    .FirstOrDefault();

                if (applicableSpawn != null)
                {
                    eligibleEnemies.Add((enemy, applicableSpawn.spawnChance));
                }
            }
            
            float totalChance = eligibleEnemies.Sum(e => e.chance);
            
            float roll = Random.Range(0f, totalChance);
            float cumulative = 0f;

            foreach (var (enemy, chance) in eligibleEnemies)
            {
                cumulative += chance;
                if (roll <= cumulative)
                {
                    return enemy.info;
                }
            }
            
            return eligibleEnemies.Count > 0 ? eligibleEnemies[0].enemy.info : enemyTypesToSpawn[0].info;
        }

        public void EnemyDeath(NetworkObject enemy)
        {
            enemies.Remove(enemy.transform);
            UIManager.Instance.UpdateEnemiesRemainText(enemies.Count);
            if (enemies.Count == 0 && !_waitingForUpgrade)
            {
                _waitingForUpgrade = true;
                Debug.Log("Wave complete, showing upgrade menu");
                SendCompleteEventRpc();
            }
        }

        [Rpc(SendTo.Everyone)]
        private void SendCompleteEventRpc()
        {
            waitingForNextWave.Value = true;
            OnWaveCompleteEvent?.Invoke();
        }

        [Rpc(SendTo.Server)]
        public void ReportPlayerUpgradeDoneRpc()
        {
            _playersDoneUpgrading++;
            if (_playersDoneUpgrading == GameManager.Instance.players.Count)
            {
                _waitingForUpgrade = false;
                _playersDoneUpgrading = 0;
                SendNextWaveEventRpc();
            }
        }

        [Rpc(SendTo.Everyone)]
        public void SendNextWaveEventRpc()
        {
            Debug.Log("Sending next wave event");
            StartNextWaveEvent?.Invoke();
        }

        public void ResetWaves()
        {
            foreach (Transform enemy in enemies)
            {
                enemy.GetComponent<NetworkObject>().Despawn();
            }

            enemies.Clear();
            currentWaveIndex = 0;
        }
    }
}

