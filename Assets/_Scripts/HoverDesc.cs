using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverDesc : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler // hover over upgrade to show description
{
    public string description; // description of the upgrade
    public GameObject descriptionPanel; // panel to show the description
    public TextMeshProUGUI iconReplaceText; // text to replace the icon with

    public void OnPointerEnter(PointerEventData eventData)
    => descriptionPanel.SetActive(true);
    

    public void OnPointerExit(PointerEventData eventData)
    => descriptionPanel.SetActive(false);
    

    // private void Update()
    // {
    //     if (EventSystem.current.currentSelectedGameObject == gameObject)
    //     {
    //         if(!descriptionPanel.activeSelf)
    //             descriptionPanel.SetActive(true);
    //     }
    //     else
    //     {
    //         if(descriptionPanel.activeSelf)
    //             descriptionPanel.SetActive(false);
    //     }
    //     
    // }

    public void SetText(string text)
    {
        description = text;
        descriptionPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(text);
    }

    public void SetIcon(Sprite icon)
    {
        GetComponent<Image>().sprite = icon;
    }
    public void SetIconReplaceText(string text)
    {
        iconReplaceText.text = text;
    }


}
