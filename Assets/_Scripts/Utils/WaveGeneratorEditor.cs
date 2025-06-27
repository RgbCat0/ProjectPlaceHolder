#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Enemies; // your EnemyInfo namespace

public class WaveEditorWindow : EditorWindow
{
    private List<EnemyInfo> allEnemies;
    private List<EnemySelection> enemySelections = new();

    private int waveCount = 10;
    private int startEnemyCount = 5;
    private int enemyCountIncrement = 2;
    private float spawnInterval = 1f;
    private float startDelay = 2f;

    [MenuItem("Tools/Wave Generator")]
    private static void ShowWindow()
    {
        GetWindow<WaveEditorWindow>("Wave Generator");
    }

    private void OnEnable()
    {
        LoadEnemyInfos();
    }

    private void LoadEnemyInfos()
    {
        string[] guids = AssetDatabase.FindAssets("t:EnemyInfo");
        allEnemies = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyInfo>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(e => e != null)
            .ToList();

        enemySelections = allEnemies.Select(e => new EnemySelection { enemyInfo = e, spawnChance = 1f }).ToList();
    }

    private Vector2 scroll;

    private void OnGUI()
    {
        GUILayout.Label("Wave Settings", EditorStyles.boldLabel);
        waveCount = EditorGUILayout.IntField("Wave Count", waveCount);
        startEnemyCount = EditorGUILayout.IntField("Start Enemy Count", startEnemyCount);
        enemyCountIncrement = EditorGUILayout.IntField("Enemy Count Increment", enemyCountIncrement);
        spawnInterval = EditorGUILayout.FloatField("Spawn Interval", spawnInterval);
        startDelay = EditorGUILayout.FloatField("Start Delay", startDelay);

        EditorGUILayout.Space();
        GUILayout.Label("Enemies", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var enemy in enemySelections)
        {
            EditorGUILayout.BeginVertical("box");
            enemy.enabled = EditorGUILayout.Toggle(enemy.enemyInfo.identifier, enemy.enabled);
            enemy.spawnChance = EditorGUILayout.FloatField("Spawn Chance", enemy.spawnChance);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Generate Waves"))
        {
            GenerateWaves();
        }
    }

    private void GenerateWaves()
    {
        string folderPath = "Assets/Resources/Waves";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        for (int i = 0; i < waveCount; i++)
        {
            var wave = ScriptableObject.CreateInstance<WaveInfo>();
            wave.enemyCount = startEnemyCount + i * enemyCountIncrement;
            wave.spawnInterval = spawnInterval;
            wave.startDelay = startDelay;

            wave.enemyTypesToSpawn = enemySelections
                .Where(e => e.enabled)
                .Select(e => new EnemySpawnInfo
                {
                    info = e.enemyInfo,
                    spawnChance = e.spawnChance
                })
                .ToList();

            string assetName = $"Wave_{i + 1}.asset";
            AssetDatabase.CreateAsset(wave, Path.Combine(folderPath, assetName));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"{waveCount} waves generated.");
    }

    [System.Serializable]
    private class EnemySelection
    {
        public EnemyInfo enemyInfo;
        public float spawnChance;
        public bool enabled = true;
    }
}
#endif
