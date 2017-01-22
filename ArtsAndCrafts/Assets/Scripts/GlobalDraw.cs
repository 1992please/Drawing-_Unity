using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
enum EDrawMode
{
    Pin,
    Brush,
    Pattern,
    Shape,
    Fill
}

public class GlobalDraw : MonoBehaviour
{
    [SerializeField]
    private Texture2D BrushTexture;
    [SerializeField]
    private Texture2D ShapeTexture;
    [SerializeField]
    private Texture2D PatternTexture;
    [Range(.01f, 2)]
    [SerializeField]
    private float BrushSpacing = .1f;
    [SerializeField]
    private InputField ThicknessInput;
    [SerializeField]
    private Texture2D BaseDrawTexture;
    [SerializeField]
    private RawImage UIImage;
    public event Action<Texture2D> OnClickSendImage;
    public static GlobalDraw singleton;
    private Brush CurrentBrush;
    private Texture2D OutTexture;
    private int Size;
    private Vector2 OldCoord;
    private Color DrawColor;
    private bool bDrawDown;
    private EDrawMode DrawMode;
    private Vector2 TransRatio;

    [Range(0, 50)]
    private int HistorySize;
    private List<byte[]> History = new List<byte[]>();
    private int HistoryCurrentIndex;
    private Vector2 OldMousePosition;

    public void SetDrawColor(Color NewColor)
    {
        DrawColor = NewColor;
    }

    private void Awake()
    {
        if (!singleton)
            singleton = this;

        OutTexture = Draw.LoadImage(BaseDrawTexture);
        BrushTexture = Draw.LoadImage(BrushTexture);
        ShapeTexture = Draw.LoadImage(ShapeTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);

        UIImage.texture = OutTexture;
        Draw.SetPaintTexture(OutTexture);
        Draw.SetCurrentPatternTexture(Draw.LoadImage(PatternTexture));

        DrawMode = EDrawMode.Pin;
        HistoryCurrentIndex = -1;
    }

    private void Start()
    {
        Draw.RegisterDrawCallbackDebugMessage();

        DrawColor = Color.black;
        SetThickness();
        TransRatio.x = OutTexture.width / UIImage.rectTransform.rect.width;
        TransRatio.y = OutTexture.height / UIImage.rectTransform.rect.height;
        SaveToHistory();
    }

    private void Update()
    {
        if (bDrawDown)
        {
            switch (DrawMode)
            {
                case EDrawMode.Pin:
                    {
                        Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                        Vector2 DPosition = (CursorPosition - OldCoord);
                        float DPositionMang = DPosition.magnitude;
                        CurrentBrush.Direction = DPosition / DPositionMang;
                        if (bDrawDown && CurrentBrush.Spacing < DPositionMang)
                        {
                            OldCoord = Draw.DrawLine(OutTexture, CurrentBrush, DrawColor, OldCoord, CursorPosition);
                            //OldCoord = CursorPositionRelative;
                            OutTexture.Apply();
                        }
                    }
                    break;
                case EDrawMode.Pattern:
                    {
                        Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                        Vector2 DPosition = (CursorPosition - OldCoord);
                        float DPositionMang = DPosition.magnitude;
                        CurrentBrush.Direction = DPosition / DPositionMang;
                        if (bDrawDown && CurrentBrush.Spacing < DPositionMang)
                        {
                            OldCoord = Draw.DrawLineWithTex(OutTexture, CurrentBrush, OldCoord, CursorPosition);
                            OutTexture.Apply();
                        }
                    }
                    break;
                case EDrawMode.Shape:
                    {
                        Vector2 MousePosition = Input.mousePosition;
                        Vector2 Direction = MousePosition - OldMousePosition;
                        if (Mathf.Abs(Direction.x) > 10 || Mathf.Abs(Direction.y) > 10)
                        {
                            Direction /= 300;
                            Direction.x = Mathf.Sign(Direction.x) > 0 ? Mathf.Clamp01(Mathf.Abs(Direction.x)) : 1 - Mathf.Clamp01(Mathf.Abs(Direction.x));
                            Direction.y = Mathf.Sign(Direction.y) * Mathf.Clamp01(Mathf.Abs(Direction.y));

                            CurrentBrush = new Brush(ShapeTexture, BrushSpacing);
                            CurrentBrush.Rotate(Direction.x * 360);
                            CurrentBrush.Resize(CurrentBrush.Size + (int)(Direction.y * 1000));
                            Draw.SetPaintTexture(History[HistoryCurrentIndex].ToArray());
                            Draw.DrawBrushTip(OutTexture, CurrentBrush, DrawColor, OldCoord);
                            // Reset the PaintTexture

                            OutTexture.Apply();
                            
                        }
                    }
                    break;
            }
        }
    }
    public void SetThickness()
    {
        Size = int.Parse(ThicknessInput.text);

        //BrushTexture = Draw.LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);
        CurrentBrush.Resize(Size);

    }

    public void OnDrawDown()
    {
        bDrawDown = true;

        switch (DrawMode)
        {
            case EDrawMode.Fill:
                {
                    Draw.FloodFillArea(OutTexture, GetMousePositionOnDrawTexture(), DrawColor);
                    OutTexture.Apply();
                }
                break;
            case EDrawMode.Pin:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    if (bDrawDown)
                    {
                        Draw.DrawBrushTip(OutTexture, CurrentBrush, DrawColor, OldCoord);

                        OutTexture.Apply();
                    }
                }
                break;
            case EDrawMode.Pattern:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    if (bDrawDown)
                    {
                        Draw.DrawBrushTipWithTex(OutTexture, CurrentBrush, OldCoord);

                        OutTexture.Apply();
                    }
                }
                break;
            case EDrawMode.Shape:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    Draw.DrawBrushTip(OutTexture, CurrentBrush, DrawColor, OldCoord);
                    OutTexture.Apply();
                    OldMousePosition = Input.mousePosition;
                }
                break;
        }
    }

    public void OnDrawUp()
    {
        bDrawDown = false;
        switch (DrawMode)
        {
            case EDrawMode.Fill:
                {
                }
                break;
            case EDrawMode.Pin:
                {
                }
                break;
            case EDrawMode.Pattern:
                {
                }
                break;
            case EDrawMode.Shape:
                {
                }
                break;
        }


        SaveToHistory();
    }

    public void OnDrawExit()
    {
        bDrawDown = false;
    }

    Vector2 GetMousePositionOnDrawTexture()
    {
        Vector2 UIImagePos = UIImage.rectTransform.position;
        Vector2 CursorPosition = Input.mousePosition;

        Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + UIImage.rectTransform.rect.min);
        CursorPositionRelative.x *= TransRatio.x;
        CursorPositionRelative.y *= TransRatio.y;
        return CursorPositionRelative;
    }

    // History Managing
    public void OnHistoryForward()
    {
        if (HistoryCurrentIndex < History.Count - 1)
        {
            HistoryCurrentIndex++;
            SetOutTexture(History[HistoryCurrentIndex]);

        }
    }

    public void OnHistoryBackward()
    {
        if (HistoryCurrentIndex > 0)
        {
            HistoryCurrentIndex--;
            SetOutTexture(History[HistoryCurrentIndex]);
        }
    }

    void SaveToHistory()
    {
        if (HistoryCurrentIndex < History.Count - 1)
        {
            History.RemoveRange(HistoryCurrentIndex + 1, History.Count - 1 - HistoryCurrentIndex);
        }

        History.Add(OutTexture.GetRawTextureData());
        HistoryCurrentIndex++;
    }

    void SetOutTexture(byte[] TexData)
    {
        OutTexture.LoadRawTextureData(TexData);
        Draw.SetPaintTexture(OutTexture);
        OutTexture.Apply();
    }
    // DrawModes

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

        BrushTexture = Draw.LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);
        CurrentBrush.Resize(Size);
    }

    public void OnClickBrush()
    {
        DrawMode = EDrawMode.Brush;

        BrushTexture = Draw.LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);
        CurrentBrush.Resize(Size);
    }

    public void OnClickPattern()
    {
        DrawMode = EDrawMode.Pattern;

        BrushTexture = Draw.LoadImage(BrushTexture);
        CurrentBrush = new Brush(BrushTexture, BrushSpacing);
        CurrentBrush.Resize(Size);
    }

    public void OnClickShape()
    {
        DrawMode = EDrawMode.Shape;
        CurrentBrush = new Brush(ShapeTexture, 1);
    }

}
