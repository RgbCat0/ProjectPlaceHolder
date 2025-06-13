using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField]
    private GameObject mainPanel;

    [SerializeField]
    private GameObject keybindsPanel;

    [SerializeField]
    private GameObject videoPanel;
    
    [SerializeField]
    private TMP_Dropdown fullScreenDropdown;
    [SerializeField]
    private Toggle damageNumbersToggle;
    [SerializeField]
    private Toggle showFPSCounterToggle;
    [SerializeField]
    private Toggle showUpgradeStatsToggle;

    private void Start()
    {
        fullScreenDropdown.onValueChanged.AddListener(FullScreenDropdown);
        damageNumbersToggle.onValueChanged.AddListener(DamageNumbersToggle);
        showFPSCounterToggle.onValueChanged.AddListener(ShowFPSCounterToggle);
        showUpgradeStatsToggle.onValueChanged.AddListener(ShowUpgradeStatsToggle);
    }

    public void SwitchTabs(GameObject newPanel)
    {
        newPanel.SetActive(true);
        if (newPanel == mainPanel)
        {
            keybindsPanel.SetActive(false);
            videoPanel.SetActive(false);
        }

        if (newPanel == keybindsPanel)
        {
            mainPanel.SetActive(false);
            videoPanel.SetActive(false);
        }

        if (newPanel == videoPanel)
        {
            mainPanel.SetActive(false);
            keybindsPanel.SetActive(false);
        }
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    // public void TogglePostProcessing(bool isOn)
    // {
    //     var postProcessLayer = Camera.main.GetComponent<PostProcessLayer>();
    //     if (postProcessLayer != null)
    //     {
    //         postProcessLayer.enabled = isOn;
    //     }
    // }
    public void FullScreenDropdown(int value)
    {
        if (value == 0) // Fullscreen
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if (value == 1) // Borderless
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else if (value == 2) // Windowed
        {
            Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
        }
    }

    public void DamageNumbersToggle(bool isOn)
        => PlayerPrefs.SetInt("DamageNumbersEnabled", isOn ? 1 : 0);

    public void ShowFPSCounterToggle(bool isOn)
        => GameObject.Find("FpsCanvas")?.SetActive(isOn);

    public void ShowUpgradeStatsToggle(bool isOn)
        => PlayerPrefs.SetInt("ShowUpgradeStats", isOn ? 1 : 0);
}