using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSounds : MonoBehaviour
{
    private void Start()
    {
        if (GetComponent<EventTrigger>() == null)
        {
            gameObject.AddComponent<EventTrigger>();
        }
        
        AddPointerClickTrigger();
        AddPointerEnterTrigger();
    }

    public void AddPointerClickTrigger()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener(data => { OnPointerClickAction(); });
        trigger.triggers.Add(click);
    }
    
    void OnPointerClickAction()
    {
        SoundManager.Instance.PlaySound2D("ClickPress");
    }
    
    public void AddPointerEnterTrigger()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(data => { OnPointerEnterAction(); });
        trigger.triggers.Add(enter);
    }

    void OnPointerEnterAction()
    {
        SoundManager.Instance.PlaySound2D("ClickHover");
    }


}
