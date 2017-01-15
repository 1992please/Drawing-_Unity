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
    public InputField ThicknessInput;
    public Sprite TestImage;
    public RawImage UIImage;
    public event Action<Texture2D> OnClickSendImage;
    public static GlobalDraw singleton;

    private Texture2D OutTexture;
    private int Thickness;
    private Vector2 OldCoord;
    private Color DrawColor;
    private bool bDraw;
    private EDrawMode DrawMode;


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

        OutTexture = LoadImage(TestImage);
        UIImage.texture = OutTexture;
        SetThickness();
        DrawColor = Color.green;
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

    public void OnClickColor(string InColor)
    {
        switch (InColor)
        {
            case "Red":
                {
                    DrawColor = Color.red;
                }
                break;
            case "Green":
                {
                    DrawColor = Color.green;

                }
                break;
            case "Yellow":
                {
                    DrawColor = Color.yellow;

                }
                break;
            case "Blue":
                {
                    DrawColor = Color.blue;

                }
                break;
        }
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
                    CursorPositionRelative.x /= UIImage.rectTransform.rect.width;
                    CursorPositionRelative.y /= UIImage.rectTransform.rect.height;


                    FloodFillArea(CursorPositionRelative, DrawColor);
                    OutTexture.Apply();
                }
                break;
            case EDrawMode.Pin:
                {
                    Vector2 UIImagePos = UIImage.rectTransform.position;
                    Vector2 CursorPosition = Input.mousePosition;

                    Vector2 CursorPositionRelative = CursorPosition - (UIImagePos + UIImage.rectTransform.rect.min);
                    CursorPositionRelative.x /= UIImage.rectTransform.rect.width;
                    CursorPositionRelative.y /= UIImage.rectTransform.rect.height;
                    OldCoord = CursorPositionRelative;

                    if (bDraw)
                    {
                        DrawLine(OldCoord, CursorPositionRelative, DrawColor, Thickness);
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
                    CursorPositionRelative.x /= UIImage.rectTransform.rect.width;
                    CursorPositionRelative.y /= UIImage.rectTransform.rect.height;

                    if (bDraw)
                    {
                        DrawLine(OldCoord, CursorPositionRelative, DrawColor, Thickness);
                        OutTexture.Apply();
                    }
                    OldCoord = CursorPositionRelative;
                }
                break;
        }
    }

    public void OnDrawExit()
    {
        bDraw = false;
    }

    public void OnDrawMove()
    {
        print("Move");
    }

    public void SetThickness()
    {
        Thickness = int.Parse(ThicknessInput.text);
    }


    public static Texture2D LoadImage(Sprite InImage )
    {
        Texture2D OutImage = new Texture2D((int)InImage.rect.width, (int)InImage.rect.height);
        Color[] pixels = InImage.texture.GetPixels((int)InImage.textureRect.x,
                                                (int)InImage.textureRect.y,
                                                (int)InImage.textureRect.width,
                                                (int)InImage.textureRect.height);

        OutImage.SetPixels(pixels);
        OutImage.Apply();
        return OutImage;
    }

    void SetSquareDotAtRatio(Vector2 Coord, Color NewColor)
    {
        if (Coord.x >= 0 && Coord.y >= 0 && Coord.x <= 1 && Coord.y <= 1)
        {
            int X = (int)(Coord.x * OutTexture.width);
            int Y = (int)(Coord.y * OutTexture.height);

            for (int i = -5; i < 5; i++)
            {
                for (int j = -5; j < 5; j++)
                {
                    OutTexture.SetPixel((int)(X + j), (int)(Y + i), NewColor);
                }
            }
        }
    }

    public void SetCircleDotAtRatio(Vector2 Coord, int Rad, Color col)
    {
        int px, nx, py, ny, Dist;
        int X = (int)(Coord.x * OutTexture.width);
        int Y = (int)(Coord.y * OutTexture.height);

        for (int i = 0; i <= Rad; i++)
        {
            Dist = (int)Mathf.Ceil(Mathf.Sqrt(Rad * Rad - i * i));
            for (int j = 0; j <= Dist; j++)
            {
                px = X + i;
                nx = X - i;
                py = Y + j;
                ny = Y - j;

                OutTexture.SetPixel(px, py, col);
                OutTexture.SetPixel(nx, py, col);

                OutTexture.SetPixel(px, ny, col);
                OutTexture.SetPixel(nx, ny, col);

            }
        }
    }

    void DrawLine(Vector2 Point0, Vector2 Point1, Color col, int HalfThickness)
    {
        int x0 = (int)(Point0.x * OutTexture.width);
        int x1 = (int)(Point1.x * OutTexture.width);
        int y0 = (int)(Point0.y * OutTexture.height);
        int y1 = (int)(Point1.y * OutTexture.height);

        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        SetSquareDot(x0, y0, HalfThickness, HalfThickness, col);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                SetSquareDot(x0, y0, HalfThickness, HalfThickness, col);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                SetSquareDot(x0, y0, HalfThickness, HalfThickness, col);
            }
        }
    }

    void SetSquareDot(int x, int y, int xHalfThickness, int yHalfThickness, Color NewColor)
    {
        for (int i = 0; i < xHalfThickness; i++)
        {
            for (int j = 0; j < yHalfThickness; j++)
            {
                OutTexture.SetPixel((int)(x + i), (int)(y + j), NewColor);
                OutTexture.SetPixel((int)(x - i), (int)(y + j), NewColor);
                OutTexture.SetPixel((int)(x - i), (int)(y - j), NewColor);
                OutTexture.SetPixel((int)(x + i), (int)(y - j), NewColor);
            }
        }
    }

    public struct Point
    {
        public short x;
        public short y;
        public Point(short aX, short aY) { x = aX; y = aY; }
        public Point(int aX, int aY) : this((short)aX, (short)aY) { }
    }

    void FloodFillArea(Vector2 Coord, Color aFillColor)
    {
        int w = OutTexture.width;
        int h = OutTexture.height;

        int aX = (int)(Coord.x * w);
        int aY = (int)(Coord.y * h);

        //Color[] colors = OutTexture.GetPixels();
        byte[] Data = OutTexture.GetRawTextureData();
        Draw.FloodFillArea(Data, aX, aY, aFillColor, OutTexture.height , OutTexture.width);
        OutTexture.LoadRawTextureData(Data);
    }

    public static Texture2D GetWhiteTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] pixels = tex.GetPixels();
        for(int i = 0; i< pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}
