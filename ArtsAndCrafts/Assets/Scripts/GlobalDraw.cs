using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

enum EDrawMode
{
    Pin,
    Fill
}

public class GlobalDraw : MonoBehaviour
{
    public Texture2D BrushTexture;
    public InputField ThicknessInput;
    public Sprite TestImage;
    public RawImage UIImage;
    public event Action<Texture2D> OnClickSendImage;
    public static GlobalDraw singleton;


    private Brush CurrentBrush;
    private Texture2D OutTexture;
    private int Size;
    private Vector2 OldCoord;
    private Color DrawColor;
    private bool bDraw;
    private EDrawMode DrawMode;
    private Vector2 TransRatio;
    public void SetDrawColor(Color NewColor)
    {
        DrawColor = NewColor;
    }

    private void Awake()
    {
        if (!singleton)
            singleton = this;

        DrawMode = EDrawMode.Pin;
    }

    private void Start()
    {
        BrushTexture = LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, .1f);
        OutTexture = LoadImage(TestImage);
        UIImage.texture = OutTexture;
        DrawColor = Color.black;
        SetThickness();
        TransRatio.x = OutTexture.width / UIImage.rectTransform.rect.width;
        TransRatio.y = OutTexture.height / UIImage.rectTransform.rect.height;
    }

    public void OnClickSend()
    {
        if (OnClickSendImage != null)
            OnClickSendImage(OutTexture);
    }

    public void OnClickFill()
    {
        DrawMode = EDrawMode.Fill;
    }

    public void OnClickPin()
    {
        DrawMode = EDrawMode.Pin;
    }

    public void OnDrawUp()
    {
        bDraw = false;
    }

    public void OnDrawDown()
    {
        bDraw = true;

        switch (DrawMode)
        {
            case EDrawMode.Fill:
                {
                    Vector2 UIImagePos = UIImage.rectTransform.position;
                    Vector2 CursorPosition = Input.mousePosition;

                    Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + UIImage.rectTransform.rect.min);
                    CursorPositionRelative.x *= TransRatio.x;
                    CursorPositionRelative.y *= TransRatio.y;

                    Draw.FloodFillArea(OutTexture, (int)CursorPositionRelative.x, (int)CursorPositionRelative.y, DrawColor);

                    OutTexture.Apply();
                }
                break;
            case EDrawMode.Pin:
                {
                    Vector2 UIImagePos = UIImage.rectTransform.position;
                    Vector2 CursorPosition = Input.mousePosition;

                    Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + UIImage.rectTransform.rect.min);
                    CursorPositionRelative.x *= TransRatio.x;
                    CursorPositionRelative.y *= TransRatio.y;
                    OldCoord = CursorPositionRelative;

                    if (bDraw)
                    {
                        Draw.DrawBrushTip(OutTexture, CurrentBrush, DrawColor, (int)OldCoord.x, (int)OldCoord.y);

                        OutTexture.Apply();
                    }
                }
                break;
        }
    }

    public void OnDrawDrag()
    {
        switch (DrawMode)
        {
            case EDrawMode.Pin:
                {
                    Vector2 UIImagePos = UIImage.rectTransform.position;
                    Vector2 CursorPosition = Input.mousePosition;

                    Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + UIImage.rectTransform.rect.min);
                    CursorPositionRelative.x *= TransRatio.x;
                    CursorPositionRelative.y *= TransRatio.y;

                    Vector2 DPosition = (OldCoord - CursorPositionRelative);

                    if (bDraw && (CurrentBrush.Spacing < Mathf.Abs(DPosition.x) || CurrentBrush.Spacing < Mathf.Abs(DPosition.y)))
                    {
                        Draw.DrawLine(OutTexture, CurrentBrush, DrawColor, (int)OldCoord.x, (int)OldCoord.y, (int)CursorPositionRelative.x, (int)CursorPositionRelative.y);
                        OutTexture.Apply();
                        OldCoord = CursorPositionRelative;
                    }
                }
                break;
        }
    }

    public void OnDrawExit()
    {
        bDraw = false;
    }

    public void SetThickness()
    {
        Size = int.Parse(ThicknessInput.text);
        CurrentBrush.Resize(Size);
    }


    public static Texture2D LoadImage(Sprite InImage)
    {
        Texture2D OutImage = new Texture2D((int)InImage.rect.width, (int)InImage.rect.height);
        Color[] pixels = InImage.texture.GetPixels((int)InImage.textureRect.x,
                                                (int)InImage.textureRect.y,
                                                (int)InImage.textureRect.width,
                                                (int)InImage.textureRect.height);
        OutImage.wrapMode = TextureWrapMode.Clamp;
        OutImage.SetPixels(pixels);
        OutImage.Apply();
        return OutImage;
    }

    public static Texture2D LoadImage(Texture2D InImage)
    {
        Texture2D OutImage = new Texture2D((int)InImage.width, (int)InImage.height);
        OutImage.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = InImage.GetPixels();
        OutImage.SetPixels(pixels);
        OutImage.Apply();
        return OutImage;
    }

    public static Texture2D GetWhiteTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = tex.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
