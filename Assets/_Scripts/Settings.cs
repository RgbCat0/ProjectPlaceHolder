using UnityEngine;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject keybindsPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject closePanel;
    public void SwitchTabs(GameObject newPanel)
    {
       newPanel.SetActive(true);
       if(newPanel == mainPanel) {keybindsPanel.SetActive(false); videoPanel.SetActive(false);}
       if(newPanel == keybindsPanel) {mainPanel.SetActive(false); videoPanel.SetActive(false);}
       if(newPanel == videoPanel) {mainPanel.SetActive(false); keybindsPanel.SetActive(false);}
       if(newPanel == closePanel) {mainPanel.SetActive(false); keybindsPanel.SetActive(false); videoPanel.SetActive(false);} 
    }
}
