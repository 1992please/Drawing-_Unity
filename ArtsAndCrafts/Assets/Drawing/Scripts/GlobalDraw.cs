using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
public enum EDrawMode
{
    Tool,
    Patern,
    Shape
}

public enum EDrawingTool
{
    Brush,
    Pencil,
    ColorPen,
    Crayon,
    Eraser,
    Spray,
    Fill
}

[Serializable]
public class DrawingTool
{
    public Texture2D ToolTexture;
    [Range(.1f, 2)]
    public float BrushSpacing = .1f;
    public int MinSize = 10;
    public int MaxSize = 100;

    public void ReloadTexture()
    {
        ToolTexture = Draw.LoadImage(ToolTexture);
    }

    public Brush GetBrush(float Size)
    {
        Brush OutBrush = new Brush(ToolTexture, BrushSpacing);
        OutBrush.Resize((int)Mathf.Lerp(MinSize, MaxSize, Size));
        return OutBrush;
    }
}

// Some Usage Notes:
// Pattern Textures must be the same width/height as the Draw Sheet


public class GlobalDraw : MonoBehaviour
{
    [Header("Draw Sheet")]
    [SerializeField]
    private int DrawSheetWidth = 2160;
    [SerializeField]
    private int DrawSheetHeight = 1440;
    [SerializeField]
    private RawImage DrawSheetImage;


    [Space(2)]
    [Header("Tools")]
    [SerializeField]
    private DrawingTool BrushTool;
    [SerializeField]
    private DrawingTool PencilTool;
    [SerializeField]
    private DrawingTool ColorPenTool;
    [SerializeField]
    private DrawingTool CrayonTool;
    [SerializeField]
    private DrawingTool EraserTool;
    [SerializeField]
    private DrawingTool SprayTool;

    [Space(2)]
    [Header("Pattern")]
    [SerializeField]
    private DrawingTool PatternDrawTool;
    [SerializeField]
    private Texture2D[] PatternTextures;

    [Space(2)]
    [Header("Shapes")]
    [SerializeField]
    private Texture2D[] ShapeTextures;
    [SerializeField]
    private float ShapeResizeMaxRate;

    public static GlobalDraw singleton;
    private Brush CurrentBrush;
    private Texture2D OutTexture;
    private Vector2 OldCoord;
    private Color DrawColor;
    private bool bDrawDown;
    private EDrawMode DrawMode;
    private EDrawingTool DrawTool;
    private int ShapeIndex;
    private int PatternIndex;

    private MyTexture CurrentPatternTex;
    private MyTexture PaintTex;


    private Vector2 TransRatio;
    private int HistorySize;
    private List<byte[]> History = new List<byte[]>();
    private int HistoryCurrentIndex;
    private Vector2 OldMousePosition;


    private void Awake()
    {
        if (!singleton)
            singleton = this;

        // Reload All Textures 
        BrushTool.ReloadTexture();
        PencilTool.ReloadTexture();
        EraserTool.ReloadTexture();
        SprayTool.ReloadTexture();
        CrayonTool.ReloadTexture();
        ColorPenTool.ReloadTexture();
        PatternDrawTool.ReloadTexture();

        OutTexture = Draw.GetWhiteTexture(DrawSheetWidth, DrawSheetHeight);
        DrawSheetImage.texture = OutTexture;
        PaintTex = new MyTexture(OutTexture);


        for (int i = 0; i < ShapeTextures.Length; i++)
        {
            ShapeTextures[i] = Draw.LoadImage(ShapeTextures[i]);
        }

        for (int i = 0; i < PatternTextures.Length; i++)
        {
            PatternTextures[i] = Draw.LoadImage(PatternTextures[i]);
        }

        HistoryCurrentIndex = -1;
    }

    private void Start()
    {
        //Uncomment the register if you need to debug the DLL
        //Draw.RegisterDrawCallbackDebugMessage();
        Draw.SeedRandomization();

        UpdateMouseTransferRatio();

        SetDrawMode(EDrawMode.Tool);
        SaveToHistory();
    }

    public void OnDrawDown()
    {
        bDrawDown = true;

        switch (DrawMode)
        {
            case EDrawMode.Tool:
                {
                    switch (DrawTool)
                    {
                        case EDrawingTool.Brush:
                        case EDrawingTool.ColorPen:
                        case EDrawingTool.Pencil:
                        case EDrawingTool.Crayon:
                            {
                                OldCoord = GetMousePositionOnDrawTexture();
                                if (bDrawDown)
                                {
                                    Draw.DrawBrushTip(PaintTex, CurrentBrush, DrawColor, OldCoord);
                                    OutTexture.LoadRawTextureData(PaintTex.Data);
                                    OutTexture.Apply();
                                }
                            }
                            break;
                        case EDrawingTool.Fill:
                            {
                                OldCoord = GetMousePositionOnDrawTexture();
                                Draw.FloodFillArea(PaintTex, OldCoord, DrawColor);
                                OutTexture.LoadRawTextureData(PaintTex.Data);
                                OutTexture.Apply();
                            }
                            break;
                        case EDrawingTool.Eraser:
                            {
                                OldCoord = GetMousePositionOnDrawTexture();

                                Draw.DrawBrushTip(PaintTex, CurrentBrush, Color.white, OldCoord);
                                OutTexture.LoadRawTextureData(PaintTex.Data);
                                OutTexture.Apply();
                            }
                            break;
                        case EDrawingTool.Spray:
                            {
                                OldCoord = GetMousePositionOnDrawTexture();

                                Draw.DrawSprayWithBrush(PaintTex, CurrentBrush, DrawColor, (int)OldCoord.x, (int)OldCoord.y);
                                OutTexture.LoadRawTextureData(PaintTex.Data);
                                OutTexture.Apply();

                            }
                            break;
                    }
                }
                break;
            case EDrawMode.Patern:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    if (bDrawDown)
                    {
                        Draw.DrawBrushTipWithTex(PaintTex, CurrentPatternTex, CurrentBrush, OldCoord);
                        OutTexture.LoadRawTextureData(PaintTex.Data);
                        OutTexture.Apply();
                    }
                }
                break;
            case EDrawMode.Shape:
                {
                    OldCoord = GetMousePositionOnDrawTexture();
                    Draw.DrawBrushTip(PaintTex, CurrentBrush, DrawColor, OldCoord);
                    OutTexture.LoadRawTextureData(PaintTex.Data);
                    OutTexture.Apply();
                    OldMousePosition = Input.mousePosition;
                }
                break;
        }
    }

    private void Update()
    {
        if (bDrawDown)
        {
            switch (DrawMode)
            {
                case EDrawMode.Tool:
                    {
                        switch (DrawTool)
                        {
                            case EDrawingTool.Brush:
                            case EDrawingTool.ColorPen:
                            case EDrawingTool.Pencil:
                            case EDrawingTool.Crayon:
                                {
                                    Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                                    Vector2 DPosition = (CursorPosition - OldCoord);
                                    float DPositionMang = DPosition.magnitude;
                                    CurrentBrush.Direction = DPosition / DPositionMang;
                                    if (bDrawDown && CurrentBrush.Spacing < DPositionMang)
                                    {
                                        OldCoord = Draw.DrawLine(PaintTex, CurrentBrush, DrawColor, OldCoord, CursorPosition);
                                        OutTexture.LoadRawTextureData(PaintTex.Data);
                                        OutTexture.Apply();
                                    }
                                }
                                break;
                            case EDrawingTool.Fill:
                                {
                                    Draw.FloodFillArea(PaintTex, GetMousePositionOnDrawTexture(), DrawColor);
                                    OutTexture.LoadRawTextureData(PaintTex.Data);
                                    OutTexture.Apply();
                                }
                                break;
                            case EDrawingTool.Eraser:
                                {
                                    Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                                    Vector2 DPosition = (CursorPosition - OldCoord);
                                    float DPositionMang = DPosition.magnitude;
                                    CurrentBrush.Direction = DPosition / DPositionMang;
                                    if (bDrawDown && CurrentBrush.Spacing < DPositionMang)
                                    {
                                        OldCoord = Draw.DrawLine(PaintTex, CurrentBrush, Color.white, OldCoord, CursorPosition);
                                        OutTexture.LoadRawTextureData(PaintTex.Data);
                                        OutTexture.Apply();
                                    }
                                }
                                break;
                            case EDrawingTool.Spray:
                                {
                                    OldCoord = GetMousePositionOnDrawTexture();

                                    Draw.DrawSprayWithBrush(PaintTex, CurrentBrush, DrawColor, (int)OldCoord.x, (int)OldCoord.y);
                                    OutTexture.LoadRawTextureData(PaintTex.Data);
                                    OutTexture.Apply();
                                }
                                break;
                        }
                    }
                    break;
                case EDrawMode.Patern:
                    {
                        Vector2 CursorPosition = GetMousePositionOnDrawTexture();

                        Vector2 DPosition = (CursorPosition - OldCoord);
                        float DPositionMang = DPosition.magnitude;
                        CurrentBrush.Direction = DPosition / DPositionMang;
                        if (bDrawDown && CurrentBrush.Spacing < DPositionMang)
                        {
                            OldCoord = Draw.DrawLineWithTex(PaintTex, CurrentPatternTex, CurrentBrush, OldCoord, CursorPosition);
                            OutTexture.LoadRawTextureData(PaintTex.Data);
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
                            Direction.x = Direction.x > 0 ? Mathf.Clamp01(Mathf.Abs(Direction.x)) : 1 - Mathf.Clamp01(Mathf.Abs(Direction.x));
                            Direction.y = Mathf.Sign(Direction.y) * Mathf.Clamp01(Mathf.Abs(Direction.y));

                            CurrentBrush = new Brush(ShapeTextures[ShapeIndex], 1);
                            CurrentBrush.Rotate(Direction.x * 360);
                            CurrentBrush.Resize(CurrentBrush.Size + (int)(Direction.y * 1000));
                            PaintTex.Data = History[HistoryCurrentIndex].ToArray();
                            Draw.DrawBrushTip(PaintTex, CurrentBrush, DrawColor, OldCoord);
                            OutTexture.LoadRawTextureData(PaintTex.Data);
                            OutTexture.Apply();
                        }
                    }
                    break;
            }
        }
    }

    public void OnDrawUp()
    {
        bDrawDown = false;
        SaveToHistory();
    }

    public void OnDrawExit()
    {
        bDrawDown = false;
    }

    public void OnClickSend()
    {
        string PlayerID = AttractionAttractionDeviceManager.AttractionDeviceManager.instance.GetCurrentPlayerIdOnTerminal();

        TcpDataTransfer.singlton.ServerIP = AttractionAttractionDeviceManager.AttractionDeviceManager.instance.SubServerIP;
        TcpDataTransfer.singlton.SendData(new ObjectToTransfer(PlayerID, 3, OutTexture));
        AttractionAttractionDeviceManager.AttractionDeviceManager.instance.FinishObjective(10);
        Juniverse.Model.RewardsData reward = AttractionAttractionDeviceManager.AttractionDeviceManager.instance.GetCurrentObjectiveData().Rewards[0];
        print("You Finished Your Objective and Currency: " + reward.Currency + ", Xp: " + reward.XP);
        //AttractionAttractionDeviceManager.AttractionDeviceManager.instance.FinishObjective(20);
    }

    //For UI Indexing :/
    public void SetDrawMode(int Index)
    {
        switch (Index)
        {
            case 0:
                {
                    SetDrawMode(EDrawMode.Tool);
                }
                break;
            case 1:
                {
                    SetDrawMode(EDrawMode.Patern);
                }
                break;
            case 2:
                {
                    SetDrawMode(EDrawMode.Shape);
                }
                break;
        }
    }

    public void SetDrawMode(EDrawMode _DrawingMode)
    {
        DrawMode = _DrawingMode;
        switch (DrawMode)
        {
            case EDrawMode.Patern:
                {
                    SetPatternIndex(0);
                }
                break;
            case EDrawMode.Shape:
                {
                    SetShapeIndex(0);
                }
                break;
            case EDrawMode.Tool:
                {
                    SetDrawTool(EDrawingTool.Brush, .5f);
                }
                break;
        }

    }

    // Size from 0 to 1
    public void SetDrawTool(EDrawingTool _DrawingTool, float Size)
    {
        DrawTool = _DrawingTool;
        switch (DrawTool)
        {
            case EDrawingTool.Brush:
                {
                    CurrentBrush = BrushTool.GetBrush(Size);
                }
                break;
            case EDrawingTool.ColorPen:
                {
                    CurrentBrush = ColorPenTool.GetBrush(Size);
                }
                break;
            case EDrawingTool.Crayon:
                {
                    CurrentBrush = CrayonTool.GetBrush(Size);
                }
                break;
            case EDrawingTool.Eraser:
                {
                    CurrentBrush = EraserTool.GetBrush(Size);
                }
                break;
            case EDrawingTool.Fill:
                {
                    // reset to default brush
                    CurrentBrush = BrushTool.GetBrush(Size);
                }
                break;
            case EDrawingTool.Pencil:
                {
                    CurrentBrush = PencilTool.GetBrush(Size);
                }
                break;
            case EDrawingTool.Spray:
                {
                    CurrentBrush = SprayTool.GetBrush(Size);
                }
                break;
        }
    }

    public void SetPatternIndex(int Index)
    {
        PatternIndex = Index;
        CurrentPatternTex = new MyTexture(PatternTextures[Index]);
    }

    public void SetShapeIndex(int Index)
    {
        ShapeIndex = Index;
        CurrentBrush = new Brush(ShapeTextures[Index], 1);
    }

    public void SetDrawColor(Color NewColor)
    {
        DrawColor = NewColor;
    }

    public int GetShapeIndex()
    {
        return ShapeIndex;
    }

    public int GetPatternIndex()
    {
        return PatternIndex;
    }

    public DrawingTool GetDrawingTool(EDrawingTool _DrawingTool)
    {
        switch (_DrawingTool)
        {
            case EDrawingTool.Brush:
                {
                    return BrushTool;
                }
            case EDrawingTool.ColorPen:
                {
                    return ColorPenTool;
                }
            case EDrawingTool.Crayon:
                {
                    return CrayonTool;
                }
            case EDrawingTool.Eraser:
                {
                    return EraserTool;
                }
            case EDrawingTool.Pencil:
                {
                    return PencilTool;
                }
            case EDrawingTool.Spray:
                {
                    return SprayTool;
                }
        }
        return null;
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
        PaintTex = new MyTexture(OutTexture);
        OutTexture.Apply();
    }

    public void ResetDrawSheet()
    {
        SetOutTexture(History[0]);
        SaveToHistory();
    }

    Vector2 GetMousePositionOnDrawTexture()
    {
        Vector2 UIImagePos = DrawSheetImage.rectTransform.position;
        Vector2 CursorPosition = Input.mousePosition;
        Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + DrawSheetImage.rectTransform.rect.min);
        CursorPositionRelative.x *= TransRatio.x;
        CursorPositionRelative.y *= TransRatio.y;
        return CursorPositionRelative;
    }

    void UpdateMouseTransferRatio()
    {
        // Updating the Transfer Ratio for the screen space to texture pixel space
        TransRatio.x = OutTexture.width / DrawSheetImage.rectTransform.rect.width;
        TransRatio.y = OutTexture.height / DrawSheetImage.rectTransform.rect.height;
    }

    public EDrawingTool GetCurrentDrawToolType()
    {
        return DrawTool;
    }
}
