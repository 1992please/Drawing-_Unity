#include "Draw.h"
#include <algorithm>
#include <queue>


extern "C" {
	const static Color White = { 255, 255, 255 };
	const static Color Black = { 0, 0, 0 };

	void FillFloodRecursion(byte a[], int x, int y, Color ReplacementColor, int height, int width)
	{
		Color TargetColor = GetColor(a, x + y * width);
		if (TargetColor.CheckEqual(ReplacementColor))
			return;

		std::queue<Node> Q;
		Q.push(Node(x, y, width));


		while (!Q.empty())
		{
			Node n = Q.front();
			Q.pop();
			for (int i = n.x; i < width; i++)
			{
				Color c = GetColor(a, i + n.y * width);
				if (!c.CheckEqual(TargetColor) || c.CheckEqual(ReplacementColor))
					break;
				SetColor(a, i + n.y * width, ReplacementColor);
				if (n.y + 1 < height)
				{
					c = GetColor(a, i + n.y * width + width);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, n.y + 1, width));
				}
				if (n.y - 1 >= 0)
				{
					c = GetColor(a, i + n.y * width - width);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, n.y - 1, width));
				}
			}

			for (int i = n.x - 1; i >= 0; i--)
			{
				Color c = GetColor(a, i + n.y * width);
				if (!c.CheckEqual(TargetColor) || c.CheckEqual(ReplacementColor))
					break;
				SetColor(a, i + n.y * width, ReplacementColor);
				if (n.y + 1 < height)
				{
					c = GetColor(a, i + n.y * width + width);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, n.y + 1, width));
				}
				if (n.y - 1 >= 0)
				{
					c = GetColor(a, i + n.y * width - width);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, n.y - 1, width));
				}
			}
		}

	}

	void SetBrightTexture(byte a[], Color MainColor, int height, int width)
	{
		Color tempColor;
		float ratio;
		int HalfWidth = width / 2;
		for (int i = 0; i < HalfWidth; i++)
		{
			ratio = (float)i / (float)HalfWidth;
			tempColor = Color::Lerp(MainColor, White, ratio);
			for (int j = 0; j < height; j++)
			{
				SetColor(a, i + j * width + HalfWidth, tempColor);
			}
		}

		for (int i = 0; i < HalfWidth + 1; i++)
		{
			ratio = (float)i / (float)HalfWidth;
			tempColor = Color::Lerp(Black, MainColor, ratio);
			for (int j = 0; j < height; j++)
			{
				SetColor(a, i + j * width, tempColor);
			}
		}
	}

	void SetColor(byte a[], int Index, Color DesiredColor)
	{
		Index <<= 2;
		a[Index] = DesiredColor.R;
		a[Index + 1] = DesiredColor.G;
		a[Index + 2] = DesiredColor.B;
	}

	Color GetColor(byte a[], int Index)
	{
		Index <<= 2;
		Color OutColor;
		OutColor.R = a[Index];
		OutColor.G = a[Index + 1];
		OutColor.B = a[Index + 2];
		return OutColor;
	}
}