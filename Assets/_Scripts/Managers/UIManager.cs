using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Enemies;
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

        private void ShowUpgradeMenu() // called at end of wave
        {
            var currUpgrades = GetComponent<PlayerStats>().GetRandomUpgrades(3);
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

        private void UpgradeCall(Upgrade upgrade)
        {
            upgradeMenu.SetActive(false);
            foreach (Transform child in upgradeMenu.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var singleUpgrade in upgrade.upgrades)
            {
                GetComponent<PlayerStats>().ApplyUpgrade(singleUpgrade);
            }
        }
    }
}
