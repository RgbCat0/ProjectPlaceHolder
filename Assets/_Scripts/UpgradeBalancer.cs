#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using _Scripts.Player;
using System.IO;
using System.Linq;

public class UpgradeAutoBalancer : EditorWindow
{
    private enum SortType
    {
        Name,
        Rarity,
        Chance,
        ChanceWithLuck
    }

    private SortType currentSort = SortType.Name;
    private List<ScriptableUpgrades> upgrades = new();
    private Vector2 scrollPos;
    private float luck = 0f;

    [MenuItem("Tools/Upgrade Viewer")]
    public static void ShowWindow()
    {
        GetWindow<UpgradeAutoBalancer>("Upgrade Balancer");
    }

    private void OnEnable()
    {
        LoadUpgrades();
        CalculateUpgradeChance();
        CalculateUpgradeChanceWithLuck(luck);
    }

    private void LoadUpgrades()
    {
        upgrades.Clear();
        string[] guids = AssetDatabase.FindAssets("t:ScriptableUpgrades");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableUpgrades upgrade = AssetDatabase.LoadAssetAtPath<ScriptableUpgrades>(path);
            if (upgrade != null)
            {
                upgrades.Add(upgrade);
            }
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Reload Upgrades"))
        {
            LoadUpgrades();
            CalculateUpgradeChance();
            CalculateUpgradeChanceWithLuck(luck);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Balance All Upgrades"))
        {
            BalanceUpgrades();
            CalculateUpgradeChance();
            CalculateUpgradeChanceWithLuck(luck);
        }

        EditorGUILayout.Space();
        luck = EditorGUILayout.Slider("Luck", luck, -0.99f, 5f);

        if (GUILayout.Button("Recalculate Chances"))
        {
            CalculateUpgradeChance();
            CalculateUpgradeChanceWithLuck(luck);
        }
        EditorGUILayout.Space();
        currentSort = (SortType)EditorGUILayout.EnumPopup("Sort By", currentSort);
        ApplySorting();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (var upgrade in upgrades)
        {
            float buffSum = 0f;
            float debuffSum = 0f;

            foreach (var effect in upgrade.upgrades)
            {
                if (effect.value > 0)
                    buffSum += effect.value;
                else
                    debuffSum += Mathf.Abs(effect.value);
            }



            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(upgrade.name, EditorStyles.boldLabel);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = upgrade;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Current Rarity: {upgrade.rarity:F2}");
            EditorGUILayout.LabelField($"Buffs: {buffSum:F2}, Debuffs: {debuffSum:F2}");
            EditorGUILayout.LabelField($"Chance: {upgrade.percentageChance:F2}%");
            EditorGUILayout.LabelField($"With Luck ({luck:F2}): {upgrade.percentageWithLuck:F2}%");
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void BalanceUpgrades()
    {
        int changedCount = 0;

        foreach (var upgrade in upgrades)
        {
            float buffSum = 0f;
            float debuffSum = 0f;

            foreach (var effect in upgrade.upgrades)
            {
                if (effect.value > 0)
                    buffSum += effect.value;
                else
                    debuffSum += Mathf.Abs(effect.value);
            }

            float synergyFactor = Mathf.Max(0.5f, 1f - (debuffSum / (buffSum + debuffSum + 0.01f)));
            float totalValue = buffSum + debuffSum;
            float newRarity = Mathf.Max(1f, (totalValue / 10f) * synergyFactor);

            if (Mathf.Abs(upgrade.rarity - newRarity) > 0.01f)
            {
                upgrade.rarity = newRarity;
                EditorUtility.SetDirty(upgrade);
                changedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Balanced {changedCount} upgrades.");
    }

    private void CalculateUpgradeChance()
    {
        float totalInverseRarity = upgrades.Sum(u => 1f / u.rarity);

        foreach (var upgrade in upgrades)
        {
            upgrade.percentageChance = (1f / upgrade.rarity) / totalInverseRarity * 100f;
        }
    }

    private void CalculateUpgradeChanceWithLuck(float luck)
    {
        float curve = 1f / (1f + Mathf.Max(luck, -0.99f)); // Prevent division by zero or negative powers

        var weights = upgrades.Select(u => 1f / Mathf.Pow(u.rarity, curve)).ToList();
        float totalWeight = weights.Sum();

        for (int i = 0; i < upgrades.Count; i++)
        {
            upgrades[i].percentageWithLuck = weights[i] / totalWeight * 100f;
        }
    }
    private void ApplySorting()
    {
        switch (currentSort)
        {
            case SortType.Name:
                upgrades = upgrades.OrderBy(u => u.name).ToList();
                break;
            case SortType.Rarity:
                upgrades = upgrades.OrderBy(u => u.rarity).Reverse().ToList();
                break;
            case SortType.Chance:
                upgrades = upgrades.OrderBy(u => u.percentageChance).ToList();
                break;
            case SortType.ChanceWithLuck:
                upgrades = upgrades.OrderBy(u => u.percentageWithLuck).ToList();
                break;
        }
    }
}
#endif
