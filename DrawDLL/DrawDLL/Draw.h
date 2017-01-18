#pragma once
#include <string>


#define DRAW_API __declspec(dllexport)

typedef unsigned char byte;
typedef void(__stdcall * DebugCallback) (const char * str);

void Print(std::string message);

DebugCallback gDebugCallback;
struct Color
{
	byte R;
	byte G;
	byte B;
	float A;

	Color()
	{
		R = 0;
		G = 0;
		B = 0;
		A = 0;
	}

	Color(byte _R, byte _G, byte _B, float _A)
	{
		R = _R;
		G = _G;
		B = _B;
		A = _A;
	}

	bool CheckEqual(Color Color1)
	{
		return (Color1.R == R && Color1.G == G && Color1.B == B);
	}

	static Color Lerp(Color from, Color to, float value)
	{
		Color color;
		color.R = (byte)(from.R + (to.R - from.R) * value);
		color.G = (byte)(from.G + (to.G - from.G) * value);
		color.B = (byte)(from.B + (to.B - from.B) * value);
		return color;
	}
};

const static Color White = { 255, 255, 255, 255 };
const static Color Black = { 0, 0, 0 , 255 };

struct Vector
{
	float x;
	float y;

	Vector(float _x, float _y)
	{
		x = _x;
		y = _y;
	}
};

struct Node
{
	short x;
	short y;
	int Index;
	Node()
	{
		x = 0;
		y = 0;
	}

	Node(int _x, int _y, int width)
	{
		x = _x;
		y = _y;
		Index = (_x + _y * width) << 2;
	}

	void SetColor(byte a[], Color DesiredColor)
	{
		a[Index] = DesiredColor.R;
		a[Index + 1] = DesiredColor.G;
		a[Index + 2] = DesiredColor.B;
	}

	Color GetColor(byte a[])
	{
		Color OutColor;
		OutColor.R = a[Index];
		OutColor.G = a[Index + 1];
		OutColor.B = a[Index + 2];
		return OutColor;
	}
};

struct Texture
{
	byte* Data;
	int Width;
	int Height;

	void SetColor(int X, int Y, Color DesiredColor)
	{
		int Index = X + Y * Width;
		Index <<= 2;
		Data[Index] = DesiredColor.R;
		Data[Index + 1] = DesiredColor.G;
		Data[Index + 2] = DesiredColor.B;
	}

	void SetColorYW(int X, int YW, Color DesiredColor)
	{
		int Index = X + YW;
		Index <<= 2;

		Data[Index] = (byte)(DesiredColor.R * DesiredColor.A + Data[Index] * (1 - DesiredColor.A));
		Data[Index + 1] = (byte)(DesiredColor.G * DesiredColor.A + Data[Index + 1] * (1 - DesiredColor.A));
		Data[Index + 2] = (byte)(DesiredColor.B * DesiredColor.A + Data[Index + 2] * (1 - DesiredColor.A));

	}

	Color GetColor(int X, int Y)
	{
		int Index = X + Y * Width;
		Index <<= 2;
		Color OutColor;
		OutColor.R = Data[Index];
		OutColor.G = Data[Index + 1];
		OutColor.B = Data[Index + 2];
		return OutColor;
	}

	Color GetColorYW(int X, int YW)
	{
		int Index = X + YW;
		Index <<= 2;
		Color OutColor;
		OutColor.R = Data[Index];
		OutColor.G = Data[Index + 1];
		OutColor.B = Data[Index + 2];
		return OutColor;
	}

	void FillWithColor(Color NewColor)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				SetColor(i, j, Black);
			}
		}
	}
};

struct Brush
{
	byte* Data;
	int Size;
	int Spacing;
	// the direction of bresh normalized
	Vector Direction;
	float SpacingRatio;
	byte GetBinaryColor(int X, int Y)
	{
		return Data[X + Y * Size];
	}

	byte GetBinaryColorYW(int X, int YW)
	{
		return Data[X + YW];
	}
};

extern "C"
{
	DRAW_API void FillFloodRecursion(Texture TexData, int x, int y, Color ReplacementColor);
	DRAW_API void SetBrightTexture(Texture TexData, Color MainColor);
	DRAW_API void DrawBrushTip(Texture TexData, Brush BrushData, Color DrawColor, int x, int y);
	DRAW_API void DrawLine(Texture TexData, Brush BrushData, Color DrawColor, int x0, int y0, int x1, int y1 , Vector* FinalPos);
	DRAW_API void RegisterDebugCallback(DebugCallback callback);
}