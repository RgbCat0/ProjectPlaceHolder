using System;
using System.Collections.Generic;
using System.Linq;
using ArtificeToolkit.Attributes;
using Enemies;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "WaveInfo", menuName = "ScriptableObjects/WaveInfo", order = 2)]
public class WaveInfo : ScriptableObject
{
    public List<EnemySpawnInfo> enemyTypesToSpawn;

    [Title("Test")]
    public int enemyCount;

    [Tooltip("Time between enemy spawns in seconds")]
    public float spawnInterval;

    [Tooltip("Delay before the first enemy spawns in seconds")]
    public float startDelay;

    public EnemyInfo GetRandomInfo()
    {
        float totalChance = enemyTypesToSpawn.Sum(e => e.spawnChance);
        float roll = Random.Range(0f, totalChance);
        var cumulative = 0f;
        foreach (EnemySpawnInfo enemy in enemyTypesToSpawn)
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

    [Tooltip("Higher value = higher chance to spawn")]
    public float spawnChance;
}