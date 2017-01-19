using UnityEngine;
using System.Runtime.InteropServices;
public struct MyVector
{
    public float x;
    public float y;

    MyVector(Vector2 InVec)
    {
        x = InVec.x;
        y = InVec.y;
    }

    public static implicit operator MyVector(Vector2 value)
    {
        return new MyVector(value);
    }

    public static implicit operator Vector2(MyVector value)
    {
        return new Vector2(value.x, value.y);
    }
};

public struct Brush
{
    public byte[] Data;
    public int Size;
    public int Spacing;
    //Direction Normalized
    public MyVector Direction;
    public float SpacingRatio;
    public Brush(Texture2D InputTex, float _SpacingRatio)
    {
        int DataLength = InputTex.width * InputTex.height;
        Data = new byte[DataLength];
        byte[] InputData = InputTex.GetRawTextureData();
        for (int i = 0; i < DataLength; i++)
        {
            int index = i << 2;
            Data[i] = ((InputData[index] + InputData[index + 1] + InputData[index + 2]) > 150) ? (byte)0 : (byte)1;
        }
        Size = InputTex.width;
        SpacingRatio = _SpacingRatio;
        Spacing = 1;
        Direction = new MyVector();
        CalcSpacing();
    }

    public void Resize(int NewSize)
    {
        float ratio = (float)Size / NewSize;
        byte[] NewData = new byte[NewSize * NewSize];
        for (int y = 0; y < NewSize; y++)
        {
            int thisY = (int)(ratio * y) * Size;
            int yw = y * NewSize;
            for (int x = 0; x < NewSize; x++)
            {
                NewData[yw + x] = Data[(int)(thisY + ratio * x)];
            }
        }
        Data = NewData;
        Size = NewSize;
        CalcSpacing();
    }

    void CalcSpacing()
    {
        int S = (int)(SpacingRatio * Size);
        Spacing = S > 0 ? S : 1;
    }
}

public class Draw
{
    // Use this for initialization
    private static MyTexture CurrentPatternTex;
    private static MyTexture PaintTex;
    private static MyTexture BrightTex;

    private struct ByteColor
    {
        public byte R;
        public byte G;
        public byte B;
        public float A;
        public ByteColor(Color NewColor)
        {
            R = (byte)(NewColor.r * 255);
            G = (byte)(NewColor.g * 255);
            B = (byte)(NewColor.b * 255);
            A = NewColor.a;
        }
    };

    private struct MyTexture
    {
        public byte[] Data;
        public int Width;
        public int Height;
        public MyTexture(byte[] _Data, int _Width, int _Height)
        {
            Data = _Data;
            Width = _Width;
            Height = _Height;
        }
        public MyTexture(Texture2D NewTex)
        {
            Data = NewTex.GetRawTextureData();
            Width = NewTex.width;
            Height = NewTex.height;
        }
    };

    private delegate void DebugCallback(string message);

    [DllImport("DrawDLL")]
    private static extern void FillFloodRecursion(MyTexture InTex, int x, int y, ByteColor ReplacementColor);
    [DllImport("DrawDLL")]
    private static extern void GetBrightTexture(MyTexture InTex, ByteColor MainColor);
    [DllImport("DrawDLL")]
    private static extern void DrawBrushTip(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x, int y);
    [DllImport("DrawDLL")]
    private static extern void DrawBrushTipWithTex(MyTexture TexData, Brush BrushData, MyTexture DrawColor, int x, int y);
    [DllImport("DrawDLL")]
    private static extern void DrawLine(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x0, int y0, int x1, int y1, ref MyVector FinalPos);
    [DllImport("DrawDLL")]
    private static extern void DrawLineWithTex(MyTexture TexData, Brush BrushData, MyTexture DrawColor, int x0, int y0, int x1, int y1, ref MyVector FinalPos);
    [DllImport("DrawDLL")]
    private static extern void RegisterDebugCallback(DebugCallback callback);

    public static void SetPaintTexture(Texture2D InTex)
    {
        PaintTex = new MyTexture(InTex);
    }

    public static void SetBrightTexture(Texture2D InTex)
    {
        BrightTex = new MyTexture(InTex);
    }

    public static void SetCurrentPatternTexture(Texture2D InTex)
    {
        CurrentPatternTex = new MyTexture(InTex);
    }

    public static void FloodFillArea(Texture2D OutTex, Vector2 Pos, Color aFillColor)
    {
        FillFloodRecursion(PaintTex, (int)Pos.x, (int)Pos.y, new ByteColor(aFillColor));
        OutTex.LoadRawTextureData(PaintTex.Data);
    }

    public static void GetBrightTex(Texture2D OutTex, Color MainColor)
    {
        GetBrightTexture(BrightTex, new ByteColor(MainColor));
        OutTex.LoadRawTextureData(BrightTex.Data);
    }

    public static void DrawBrushTip(Texture2D OutTex, Brush _Brush, Color DrawColor, Vector2 Pos)
    {
        DrawBrushTip(PaintTex, _Brush, new ByteColor(DrawColor), (int)Pos.x, (int)Pos.y);
        OutTex.LoadRawTextureData(PaintTex.Data);
    }

    public static void DrawBrushTipWithTex(Texture2D OutTex, Brush _Brush, Vector2 Pos)
    {
        DrawBrushTipWithTex(PaintTex, _Brush, CurrentPatternTex, (int)Pos.x, (int)Pos.y);
        OutTex.LoadRawTextureData(PaintTex.Data);
    }

    public static Vector2 DrawLine(Texture2D OutTex, Brush _Brush, Color DrawColor, Vector2 Pos0, Vector2 Pos1)
    {
        MyVector Vect = new MyVector();
        DrawLine(PaintTex, _Brush, new ByteColor(DrawColor), (int)Pos0.x, (int)Pos0.y, (int)Pos1.x, (int)Pos1.y, ref Vect);
        OutTex.LoadRawTextureData(PaintTex.Data);
        return Vect;
    }

    public static Vector2 DrawLineWithTex(Texture2D OutTex, Brush _Brush, Vector2 Pos0, Vector2 Pos1)
    {
        MyVector Vect = new MyVector();
        DrawLineWithTex(PaintTex, _Brush, CurrentPatternTex, (int)Pos0.x, (int)Pos0.y, (int)Pos1.x, (int)Pos1.y, ref Vect);
        OutTex.LoadRawTextureData(PaintTex.Data);
        return Vect;
    }

    public static void RegisterDrawCallbackDebugMessage()
    {
        RegisterDebugCallback(new DebugCallback(DebugMethod));
    }

    private static void DebugMethod(string message)
    {
        Debug.Log("Draw Plugin: " + message);
    }

    // Global static functions for usage
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
