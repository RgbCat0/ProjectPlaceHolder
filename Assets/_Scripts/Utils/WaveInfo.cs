using System;
using System.Collections.Generic;
using System.Linq;
using ArtificeToolkit.Attributes;
using Enemies;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "WaveInfo", menuName = "Scriptable Objects/WaveInfo", order = 2)]
public class WaveInfo : ScriptableObject
{
    public List<EnemySpawnInfo> enemyTypesToSpawn;

    [Title("Test")]
    public int enemyCount;

    [Tooltip("Time between enemy spawns in seconds")]
    public float spawnInterval;

    [Tooltip("Delay before the first enemy spawns in seconds")]
    public float startDelay;
}

[Serializable]
public class EnemySpawnInfo
{
    public EnemyInfo info;
    
    public List<EnemySpawnChances> spawnChanceList;
    
}

[System.Serializable]
public class EnemySpawnChances
{
    public float spawnChance; // chance to spawn this enemy type
    public int startWave; // wave at which this enemy type starts spawning
    
    
    
}