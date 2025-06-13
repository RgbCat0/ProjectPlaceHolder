using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
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
        private GameObject smallUpgrades; // shows all the upgrades in the bottom right corner

        [SerializeField]
        private GameObject upgradePrefab;

        [SerializeField]
        private GameObject smallUpgradePrefab;

        [Header("Hurt flash")]
        [SerializeField]
        private RawImage hurtFlashImage; // to change opacity when hurt

        [SerializeField]
        private AnimationCurve hurtFlashCurve;

        [SerializeField]
        private float hurtFlashDuration = 0.5f; // duration of the flash effect

        private float _hurtFlashTimer = 0f;
        private bool _isHurtFlashing = false;

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
            WaveManager.Instance.StartNextWaveEvent += HideUpgradeMenu;
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
                    .GetComponent<TextMeshProUGUI>()
                    .text = upgrade.name;
                upgradeObject
                    .transform.GetChild(1)
                    .GetChild(0)
                    .GetComponent<TextMeshProUGUI>()
                    .text = upgrade.shortText; // icon replacement

                foreach (var singleUpgrade in upgrade.upgrades)
                {
                    if (!singleUpgrade.customDescription)
                    {
                        singleUpgrade.description = singleUpgrade.GenerateDescription();
                    }

                    upgradeObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text +=
                        singleUpgrade.description + "\n";
                }

                upgradeObject
                    .transform.GetChild(3)
                    .GetComponent<Button>()
                    .onClick.AddListener(() => UpgradeCall(upgrade));
            }
        }

        private void UpgradeCall(ScriptableUpgrades upgrade)
        {
            foreach (Transform child in upgradeMenu.transform)
                Destroy(child.gameObject);


            var localPlayerObjectId = PlayerDataSync.Instance.localPlayerObjectId;
            var playerStats = NetworkManager
                .Singleton.SpawnManager.SpawnedObjects[localPlayerObjectId]
                .GetComponent<PlayerStats>();
            foreach (var singleUpgrade in upgrade.upgrades)
            {
                playerStats.ApplyUpgrade(singleUpgrade);
            }

            if (PlayerPrefs.GetInt("ShowUpgradeStats") == 0)
            {
                var newSmall = Instantiate(smallUpgradePrefab, smallUpgrades.transform).GetComponent<HoverDesc>();
                string desc = string.Join("\n", upgrade.upgrades.ConvertAll(x => x.description));
                newSmall.SetText(desc);
                newSmall.GetComponent<HoverDesc>().SetIconReplaceText(upgrade.shortText);
            }

            WaveManager.Instance.ReportPlayerUpgradeDone();
        }

        private void HideUpgradeMenu()
        {
            // aka all players are done
            upgradeMenu.SetActive(false);
        }

        public IEnumerator HurtFlashCoroutine()
        {
            if (_isHurtFlashing)
            {
                // just reset the timer if already flashing and break this instance of the coroutine
                _hurtFlashTimer = 0f;
                yield break;
            }
            _isHurtFlashing = true;
            _hurtFlashTimer = 0f;
            hurtFlashImage.color = new Color(1, 1, 1, 0);
            
            while (_hurtFlashTimer < hurtFlashDuration)
            {
                _hurtFlashTimer += Time.deltaTime;
                float alpha = hurtFlashCurve.Evaluate(_hurtFlashTimer / hurtFlashDuration);
                hurtFlashImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }

            hurtFlashImage.color = new Color(1, 1, 1, 0);
            _isHurtFlashing = false;
        }
    }
}