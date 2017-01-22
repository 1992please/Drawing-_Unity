#pragma once
#include <string>


#define DRAW_API __declspec(dllexport)

typedef unsigned char byte;
typedef void(__stdcall * DebugCallback) (const char* str);

static void Print(std::string message);
static int Clip(int n, int lower, int upper);
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
		color.A = 1;
		return color;
	}

	std::string ToString()
	{
		return "R: " + std::to_string(R) + ", G: " + std::to_string(G) + ", B: " + std::to_string(B) + ", A: " + std::to_string(A);
	}
};

const static Color White = { 255, 255, 255, 1 };
const static Color Black = { 0, 0, 0 , 1 };

struct Vector
{
	float x;
	float y;

	Vector(float _x, float _y)
	{
		x = _x;
		y = _y;
	}

	Vector operator+ (const Vector& In)
	{
		return Vector(In.x + x, In.y + y);
	}

	Vector operator- (const Vector& In)
	{
		return Vector(In.x - x, In.y - y);
	}

	Vector operator* (const float In)
	{
		return Vector(x * In, y * In);
	}

	Vector operator-() {
		return Vector(-x, -y);
	}
};

static Vector operator* (float x, const Vector& InVec)
{
	return Vector(InVec.x * x, InVec.y * x);
}

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
		Data[Index + 3] = 255;
	}

	void SetColorA(int X, int Y, Color DesiredColor)
	{
		int Index = X + Y * Width;
		Index <<= 2;
		Data[Index] = (byte)(DesiredColor.R * DesiredColor.A + Data[Index] * (1 - DesiredColor.A));
		Data[Index + 1] = (byte)(DesiredColor.G * DesiredColor.A + Data[Index + 1] * (1 - DesiredColor.A));
		Data[Index + 2] = (byte)(DesiredColor.B * DesiredColor.A + Data[Index + 2] * (1 - DesiredColor.A));
		Data[Index + 3] = 255;
	}

	void SetColorYW(int X, int YW, Color DesiredColor)
	{
		int Index = X + YW;
		Index <<= 2;

		Data[Index] = DesiredColor.R;
		Data[Index + 1] = DesiredColor.G;
		Data[Index + 2] = DesiredColor.B;
		Data[Index + 3] = 255;
	}

	void SetColorAYW(int X, int YW, Color DesiredColor)
	{
		int Index = X + YW;
		Index <<= 2;

		Data[Index] = (byte)(DesiredColor.R * DesiredColor.A + Data[Index] * (1 - DesiredColor.A));
		Data[Index + 1] = (byte)(DesiredColor.G * DesiredColor.A + Data[Index + 1] * (1 - DesiredColor.A));
		Data[Index + 2] = (byte)(DesiredColor.B * DesiredColor.A + Data[Index + 2] * (1 - DesiredColor.A));
		Data[Index + 3] = 255;
	}

	Color GetColor(int X, int Y)
	{
		int Index = X + Y * Width;
		Index <<= 2;
		Color OutColor;
		OutColor.R = Data[Index];
		OutColor.G = Data[Index + 1];
		OutColor.B = Data[Index + 2];
		OutColor.A = Data[Index + 3];

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
		OutColor.A = Data[Index + 3];

		return OutColor;
	}

	void FillWithColor(Color NewColor)
	{
		const int length = Width * Height;
		for (int i = 0; i < length; i += 4)
		{
			Data[i] = NewColor.R;
			Data[i + 1] = NewColor.G;
			Data[i + 2] = NewColor.B;
			Data[i + 3] = 255;
		}
	}

	void DrawBrushTip(Brush BrushData, Color DrawColor, int x, int y)
	{
		const int HalfBrushSize = BrushData.Size >> 1;
		const int StartX = Clip(HalfBrushSize - x, 0, HalfBrushSize);
		const int StartY = Clip(HalfBrushSize - y, 0, HalfBrushSize);
		const int EndX = Clip(Width - x + HalfBrushSize, HalfBrushSize, BrushData.Size);
		const int EndY = Clip(Height - y + HalfBrushSize, HalfBrushSize, BrushData.Size);

		for (int j = StartY; j < EndY; j++)
		{
			int BrushYW = j * BrushData.Size;
			int ImYW = (j + y - HalfBrushSize) * Width;
			for (int i = StartX; i < EndX; i++)
			{
				if (BrushData.GetBinaryColorYW(i, BrushYW))
				{
					SetColorAYW(i + x - HalfBrushSize, ImYW, DrawColor);
				}
			}
		}
	}

	void DrawBrushTipWithTexture(Brush BrushData, Texture Tex, int x, int y)
	{
		const int HalfBrushSize = BrushData.Size >> 1;
		const int StartX = Clip(HalfBrushSize - x, 0, HalfBrushSize);
		const int StartY = Clip(HalfBrushSize - y, 0, HalfBrushSize);
		const int EndX = Clip(Width - x + HalfBrushSize, HalfBrushSize, BrushData.Size);
		const int EndY = Clip(Height - y + HalfBrushSize, HalfBrushSize, BrushData.Size);

		for (int j = StartY; j < EndY; j++)
		{
			int BrushYW = j * BrushData.Size;
			int ImYW = (j + y - HalfBrushSize) * Width;
			for (int i = StartX; i < EndX; i++)
			{
				if (BrushData.GetBinaryColorYW(i, BrushYW))
				{
					SetColorYW(i + x - HalfBrushSize, ImYW, Tex.GetColorYW(i + x - HalfBrushSize, ImYW));
				}
			}
		}
	}
};

extern "C"
{
	DRAW_API void FillFloodRecursion(Texture TexData, int x, int y, Color ReplacementColor);
	DRAW_API void GetBrightTexture(Texture TexData, Color MainColor);
	DRAW_API void DrawBrushTip(Texture TexData, Brush BrushData, Color DrawColor, int x, int y);
	DRAW_API void DrawBrushTipWithTex(Texture TexData, Brush BrushData, Texture DrawColor, int x, int y);
	DRAW_API void DrawLine(Texture TexData, Brush BrushData, Color DrawColor, int x0, int y0, int x1, int y1, Vector* FinalPos);
	DRAW_API void DrawLineWithTex(Texture TexData, Brush BrushData, Texture DrawColor, int x0, int y0, int x1, int y1, Vector* FinalPos);
	DRAW_API void RegisterDebugCallback(DebugCallback callback);
}