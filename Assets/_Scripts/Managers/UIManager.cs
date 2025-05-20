using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Enemies;
using _Scripts.LobbyScripts;
using _Scripts.Player;

namespace _Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField]
        private RectTransform healthBar;

        [SerializeField]
        private RectTransform manaBar;

        [SerializeField]
        private TextMeshProUGUI healthText;

        [SerializeField]
        private TextMeshProUGUI manaText;

        [Header("Spell Selection")]
        [SerializeField]
        private List<RawImage> spells;

        [SerializeField]
        private Texture deselectedSpellImage;

        [SerializeField]
        private Texture selectedSpellImage;

        private float _maxHealthManaBarWidth;

        [Header("Upgrade Menu")]
        [SerializeField]
        private GameObject upgradeMenu;

        [SerializeField]
        private GameObject upgradePrefab;

        [SerializeField]
        private GameObject smallUpgradePrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (healthBar == null)
            {
                Debug.LogError("Health bar is not assigned in the inspector.");
                return;
            }

            _maxHealthManaBarWidth = healthBar.sizeDelta.x;
            //GetComponent<WaveManager>().OnWaveCompleteEvent += ShowUpgradeMenu;
            WaveManager.Instance.OnWaveCompleteEvent += ShowUpgradeMenu;
        }

        public void UpdateHealthBar(float currHealth, float maxHealth)
        {
            if (healthBar == null)
            {
                Debug.LogError("Health bar is not assigned in the inspector.");
                return;
            }

            if (currHealth <= 0)
            {
                healthBar.sizeDelta = new Vector2(0, healthBar.sizeDelta.y);
            }
            else
            {
                float healthPercentage = currHealth / maxHealth;
                healthBar.sizeDelta = new Vector2(
                    _maxHealthManaBarWidth * healthPercentage,
                    healthBar.sizeDelta.y
                );
                healthText.text = $"{currHealth}/{maxHealth}";
            }
        }

        public void UpdateManaBar(float currMana, float maxMana)
        {
            if (manaBar == null)
            {
                Debug.LogError("Mana bar is not assigned in the inspector.");
                return;
            }

            if (currMana <= 0)
            {
                manaBar.sizeDelta = new Vector2(0, manaBar.sizeDelta.y);
            }
            else
            {
                float manaPercentage = currMana / maxMana;
                manaBar.sizeDelta = new Vector2(
                    _maxHealthManaBarWidth * manaPercentage,
                    manaBar.sizeDelta.y
                );
                manaText.text = $"{currMana}/{maxMana}";
            }
        }

        public void UpdateSelectedSpell(int spellIndex)
        {
            if (spellIndex < 0 || spellIndex >= 4)
            {
                Debug.LogError("Invalid spell index.");
                return;
            }

            foreach (var image in spells)
            {
                image.texture = deselectedSpellImage;
            }
            if (spellIndex == 0)
                return; // basic attack has no image
            spells[spellIndex - 1].texture = selectedSpellImage;
        }

        private void ShowUpgradeMenu() // called at end of wave
        {
            upgradeMenu.SetActive(true);
            var localPlayerObjectId = PlayerDataSync.Instance.localPlayerObjectId;
            var playerObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[
                localPlayerObjectId
            ];
            var currUpgrades = playerObject.GetComponent<PlayerStats>().GetRandomUpgrades(3);
            if (currUpgrades.Count == 0)
            {
                Debug.Log("No upgrades available.");
                return;
            }
            foreach (var upgrade in currUpgrades)
            {
                var upgradeObject = Instantiate(upgradePrefab, upgradeMenu.transform);
                upgradeObject
                    .transform.GetChild(0)
                    .GetChild(0)
                    .GetComponent<TextMeshProUGUI>()
                    .text = upgrade.shortText; // icon replacement
                foreach (var singleUpgrade in upgrade.upgrades)
                {
                    upgradeObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text +=
                        singleUpgrade.description + "\n";
                }
                upgradeObject
                    .transform.GetChild(2)
                    .GetComponent<Button>()
                    .onClick.AddListener(() => UpgradeCall(upgrade));
            }
        }

        private void UpgradeCall(ScriptableUpgrades upgrade)
        {
            upgradeMenu.SetActive(false);
            foreach (Transform child in upgradeMenu.transform)
            {
                Destroy(child.gameObject);
            }
            var localPlayerObjectId = PlayerDataSync.Instance.localPlayerObjectId;
            var playerStats = NetworkManager
                .Singleton.SpawnManager.SpawnedObjects[localPlayerObjectId]
                .GetComponent<PlayerStats>();
            foreach (var singleUpgrade in upgrade.upgrades)
            {
                playerStats.ApplyUpgrade(singleUpgrade);
            }
            WaveManager.Instance.ReportPlayerUpgradeDone();
        }
    }
}
