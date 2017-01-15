using UnityEngine;
using System.Runtime.InteropServices;
public class Draw
{
    // Use this for initialization

    private struct ByteColor
    {
        public byte R;
        public byte G;
        public byte B;
    };

    [DllImport("DrawDLL")]
    private static extern void FillFloodRecursion(byte[] a, int x, int y, ByteColor ReplacementColor, int height, int width);
    [DllImport("DrawDLL")]
    private static extern void SetBrightTexture(byte[] a, ByteColor MainColor, int height, int width);


    public static void FloodFillArea(byte[] bytes, int x, int y, Color aFillColor, int height, int width)
    {
        FillFloodRecursion(bytes, x, y, GetByteColor(aFillColor), height, width);
    }

    public static void SetBrightTex(byte[] a, Color MainColor, int height, int width)
    {
        SetBrightTexture(a, GetByteColor(MainColor), height, width);
    }


    static ByteColor GetByteColor(Color color)
    {
        ByteColor NewColor;
        NewColor.R = (byte)(color.r * 255);
        NewColor.G = (byte)(color.g * 255);
        NewColor.B = (byte)(color.b * 255);
        return NewColor;
    }
}
