#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using _Scripts.Player;
using System.IO;

public class UpgradeAutoBalancer : MonoBehaviour
{
    [MenuItem("Tools/Balance All Upgrades")]
    public static void BalanceUpgrades()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScriptableUpgrades");
        int changedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableUpgrades upgrade = AssetDatabase.LoadAssetAtPath<ScriptableUpgrades>(path);
            if (upgrade == null) continue;

            float buffSum = 0f;
            float debuffSum = 0f;

            foreach (var effect in upgrade.upgrades)
            {
                if (effect.value > 0)
                {
                    buffSum += effect.value;
                }
                else
                {
                    debuffSum += Mathf.Abs(effect.value);
                }
            }

            float synergyFactor = Mathf.Max(0.5f, 1f - (debuffSum / (buffSum + debuffSum + 0.01f))); // Encourages synergy (buff + debuff)
            float totalValue = buffSum + debuffSum;
            float newRarity = Mathf.Clamp((totalValue / 10f) * synergyFactor, 1f, 10f);

            if (Mathf.Abs(upgrade.rarity - newRarity) > 0.01f)
            {
                upgrade.rarity = newRarity;
                EditorUtility.SetDirty(upgrade);
                changedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Balanced {changedCount} upgrades based on buff/debuff synergy.");
    }
}
#endif