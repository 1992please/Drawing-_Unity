#define DRAW_API __declspec(dllexport)
typedef unsigned char byte;

struct Color
{
	byte R;
	byte G;
	byte B;

	Color()
	{
		R = 0;
		G = 0;
		B = 0;
	}

	Color(byte _R, byte _G, byte _B)
	{
		R = _R;
		G = _G;
		B = _B;
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

extern "C"
{
	DRAW_API void FillFloodRecursion(byte a[], int x, int y, Color ReplacementColor, int height, int width);
	DRAW_API void SetBrightTexture(byte a[], Color MainColor, int height, int width);
	void SetColor(byte a[], int Index, Color DesiredColor);
	Color GetColor(byte a[], int Index);
}