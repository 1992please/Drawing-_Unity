using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ToolButton : MonoBehaviour
{
    public EDrawingTool ToolType = EDrawingTool.Brush;

    private RawImage PreviewImage;
    private float Size = .5f;
    private float BaseXMousePos;
    private DrawingTool DrawTool;

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
        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entry.callback.AddListener((data) => { OnDrag(); });
        trigger.triggers.Add(entry);

        DrawTool = GlobalDraw.singleton.GetDrawingTool(ToolType);
        PreviewImage = transform.GetChild(0).GetComponent<RawImage>();

    }

    private void OnEnable()
    {
        PlayChoosedAnimation(GlobalDraw.singleton.GetCurrentDrawToolType() == ToolType);
    }

    public void OnPointerDown()
    {
        BaseXMousePos = Input.mousePosition.x;
        Size = .5f;
        PlayChoosedAnimation(true);
        if (ToolType != EDrawingTool.Fill)
        {
            PreviewImage.gameObject.SetActive(true);
            UpdatePreviewTexture();
        }
    }

    public void OnPointerUp()
    {
        GlobalDraw.singleton.SetDrawTool(ToolType, Size);
        if (ToolType != EDrawingTool.Fill)
        {
            PreviewImage.gameObject.SetActive(false);
        }
    }

    public void OnDrag()
    {
        // Do Some
        if (ToolType != EDrawingTool.Fill)
        {
            Size = GetSize(Input.mousePosition.x - BaseXMousePos);
            UpdatePreviewTexture();
        }
    }

    float GetSize(float XMovement)
    {
        return (Mathf.Clamp(XMovement / 150, -1, 1) + 1) / 2;
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

    void UpdatePreviewTexture()
    {
        Texture2D previewTex = Draw.GetWhiteTexture((int)PreviewImage.rectTransform.rect.width, (int)PreviewImage.rectTransform.rect.height);
        PreviewImage.texture = previewTex;
        MyTexture paintTex = new MyTexture((Texture2D)PreviewImage.texture);
        Draw.DrawBrushTip(paintTex, DrawTool.GetBrush(Size), Color.black, new Vector2(PreviewImage.texture.width / 2, PreviewImage.texture.height / 2));
        previewTex.LoadRawTextureData(paintTex.Data);
        previewTex.Apply();
    }

}
