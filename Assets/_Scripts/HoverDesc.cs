using TMPro;
using UnityEngine;
using _Scripts.Player;

public class HoverDesc : MonoBehaviour // hover over upgrade to show description
{
    public string description; // description of the upgrade
    public bool isHovered = false;
    public GameObject descriptionPanel; // panel to show the description

    private void OnMouseEnter()
    {
        isHovered = true;
        descriptionPanel.SetActive(true);
    }

    private void OnMouseExit()
    {
        isHovered = false;
        descriptionPanel.SetActive(false);
    }

    public void SetText(string text)
    {
        description = text;
        descriptionPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(text);
    }
}
