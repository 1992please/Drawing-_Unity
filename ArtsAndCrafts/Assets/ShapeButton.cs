using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ShapeButton : MonoBehaviour {
    public int ShapeIndex = 0;

    private void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => { OnPointerDown(); });
        trigger.triggers.Add(entry);
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { OnPointerUp(); });
        trigger.triggers.Add(entry);

    }

    private void OnEnable()
    {
        PlayChoosedAnimation(GlobalDraw.singleton.GetShapeIndex() == ShapeIndex);
    }

    public void OnPointerDown()
    {
        PlayChoosedAnimation(true);
    }

    public void OnPointerUp()
    {
        GlobalDraw.singleton.SetShapeIndex(ShapeIndex);
    }

    public void PlayChoosedAnimation(bool value)
    {
        GetComponent<RectTransform>().localScale = value ? new Vector3(1.1f, 1.1f) : new Vector3(1, 1);
        if (value)
        {
            ToolButton[] Tools = transform.parent.GetComponentsInChildren<ToolButton>();
            foreach (var Tool in Tools)
            {
                if (Tool != this)
                {
                    Tool.PlayChoosedAnimation(false);
                }
            }
        }
    }
}
