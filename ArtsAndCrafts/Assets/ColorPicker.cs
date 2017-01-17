using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ColorPicker : MonoBehaviour
{
    [SerializeField]
    private Sprite PickColorSprite;
    [SerializeField]
    private RawImage PickColorImage;
    [SerializeField]
    private RawImage ColorCursor;
    [SerializeField]
    private RawImage PickIntensityImage;
    [SerializeField]
    private RawImage PreviewImage;
    [SerializeField]
    private Slider OpacitySlider;
    // for 0 t0 1
    private float ColorValue = 0;
    private Color BaseColor;
    private Color FinalColor;
    private float IntensityValue = 0;


    private void Awake()
    {
        PickColorImage.texture = GlobalDraw.LoadImage(PickColorSprite);
        PickIntensityImage.texture = GlobalDraw.GetWhiteTexture(256, 32);
    }

    private void Start()
    {
        SetColorValue(ColorValue);
        SetIntensityValue(IntensityValue);
    }

    public void OnColorPick()
    {
        Vector2 UIImagePos = PickColorImage.rectTransform.position;
        Vector2 CursorPosition = Input.mousePosition;
        Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + PickColorImage.rectTransform.rect.min);

        SetColorValue(CursorPositionRelative.x / PickColorImage.rectTransform.rect.width);
    }

    public void OnIntensityPick()
    {
        Vector2 UIImagePos = PickIntensityImage.rectTransform.position;
        Vector2 CursorPosition = Input.mousePosition;
        Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + PickColorImage.rectTransform.rect.min);

        SetIntensityValue(CursorPositionRelative.x / PickColorImage.rectTransform.rect.width);
    }

    public void OnOpacityChanged()
    {
        OnChangeColor();
    } 

    void SetColorValue(float NewColorValue)
    {
        ColorValue = Mathf.Clamp01(NewColorValue);
        Texture2D tex = (Texture2D)PickColorImage.texture;
        BaseColor = tex.GetPixel((int)(ColorValue * tex.width), (int)(.5f * tex.height));
        // Set Color Cursor Position
        RectTransform rectTransform = PickColorImage.rectTransform;
        ColorCursor.rectTransform.position = new Vector2(ColorValue * rectTransform.rect.width, .5f * rectTransform.rect.height) + (Vector2)rectTransform.position + rectTransform.rect.min;

        OnChangeColor();
        UpdateIntensityTexture();
    }

    void SetIntensityValue(float NewIntensityValue)
    {
        IntensityValue = Mathf.Clamp01(NewIntensityValue);

        // set Image preview position
        Texture2D tex = (Texture2D)PickIntensityImage.texture;
        RectTransform rectTransform = PickIntensityImage.rectTransform;
        PreviewImage.rectTransform.position = new Vector2(IntensityValue * rectTransform.rect.width, .5f * rectTransform.rect.height) + (Vector2)rectTransform.position + rectTransform.rect.min;

        OnChangeColor();
    }

    void OnChangeColor()
    {
        Color PickedColor = GetFinalColor();
        GlobalDraw.singleton.SetDrawColor(PickedColor);
        PreviewImage.color = PickedColor; 
    }

    void UpdateIntensityTexture()
    {
        Texture2D IntensityTex = (Texture2D)PickIntensityImage.texture;
        Draw.SetBrightTex(IntensityTex, BaseColor);
        IntensityTex.Apply();
    }

    Color GetFinalColor()
    {
        Color OutputColor;
        if(IntensityValue > .5f)
        {
            OutputColor =  Color.Lerp(BaseColor, Color.white, (IntensityValue - .5f) / .5f);
        }
        else
        {
            OutputColor = Color.Lerp(BaseColor, Color.black, 1 - (IntensityValue / .5f));
        }
        OutputColor.a = OpacitySlider.value;
        return OutputColor;
    }

}
