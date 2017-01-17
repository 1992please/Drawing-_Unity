using UnityEngine;
using System.Runtime.InteropServices;

public struct Brush
{
    public byte[] Data;
    public int Size;
    public int Spacing;
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
        Spacing = CalcSpacing();
    }

    public void Resize(int NewSize)
    {
        float ratio = (float)Size / NewSize;
        byte[] NewData = new byte[NewSize * NewSize];
        for (int y = 0; y < NewSize; y++)
        {
            int thisY = (int)(ratio * y) * Size;
            int yw = y * NewSize;
            for(int x = 0; x < NewSize; x++)
            {
                NewData[yw + x] = Data[(int)(thisY + ratio * x)];
            }
        }
        Data = NewData;
        Size = NewSize;
        Spacing = CalcSpacing();
    }

    int CalcSpacing()
    {
        int S = (int)(SpacingRatio * Size);
        return S > 0 ? S : 1;
    }

}

public class Draw
{
    // Use this for initialization

    private struct ByteColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
        public ByteColor(Color NewColor)
        {
            R = (byte)(NewColor.r * 255);
            G = (byte)(NewColor.g * 255);
            B = (byte)(NewColor.b * 255);
            A = (byte)(NewColor.a * 255);
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

    [DllImport("DrawDLL")]
    private static extern void FillFloodRecursion(MyTexture InTex, int x, int y, ByteColor ReplacementColor);
    [DllImport("DrawDLL")]
    private static extern void SetBrightTexture(MyTexture InTex, ByteColor MainColor);
    [DllImport("DrawDLL")]
    private static extern void DrawBrushTip(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x, int y);
    [DllImport("DrawDLL")]
    private static extern void DrawLine(MyTexture TexData, Brush BrushData, ByteColor DrawColor, int x0, int y0, int x1, int y1);

    public static void FloodFillArea(Texture2D DrawTex, int x, int y, Color aFillColor)
    {
        MyTexture OutTex = new MyTexture(DrawTex);
        FillFloodRecursion(OutTex, x, y, new ByteColor(aFillColor));
        DrawTex.LoadRawTextureData(OutTex.Data);
    }

    public static void SetBrightTex(Texture2D DrawTex, Color MainColor)
    {
        MyTexture OutTex = new MyTexture(DrawTex);
        SetBrightTexture(OutTex, new ByteColor(MainColor));
        DrawTex.LoadRawTextureData(OutTex.Data);
    }

    public static void DrawBrushTip(Texture2D DrawTex, Brush _Brush, Color DrawColor, int x, int y)
    {
        MyTexture OutTex = new MyTexture(DrawTex);
        DrawBrushTip(OutTex, _Brush, new ByteColor(DrawColor), x, y);
        DrawTex.LoadRawTextureData(OutTex.Data);
    }

    public static void DrawLine(Texture2D DrawTex, Brush _Brush, Color DrawColor, int x0, int y0, int x1, int y1)
    {
        MyTexture OutTex = new MyTexture(DrawTex);
        DrawLine(OutTex, _Brush, new ByteColor(DrawColor), x0, y0, x1, y1);
        DrawTex.LoadRawTextureData(OutTex.Data);
    }


}
