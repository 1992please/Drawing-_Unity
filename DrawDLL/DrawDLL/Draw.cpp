#include "Draw.h"
#include <algorithm>
#include <queue>


extern "C" {

	void FillFloodRecursion(Texture Image, int x, int y, Color ReplacementColor)
	{
		Color TargetColor = Image.GetColor(x, y);
		if (TargetColor.CheckEqual(ReplacementColor))
			return;

		std::queue<Node> Q;
		Q.push(Node(x, y, Image.Width));


		while (!Q.empty())
		{
			Node node = Q.front();
			Q.pop();
			for (int i = node.x; i < Image.Width; i++)
			{
				Color c = Image.GetColor(i, node.y);
				if (!c.CheckEqual(TargetColor) || c.CheckEqual(ReplacementColor))
					break;
				Image.SetColor(i, node.y, ReplacementColor);
				if (node.y + 1 < Image.Height)
				{
					c = Image.GetColor(i + Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, node.y + 1, Image.Width));
				}
				if (node.y - 1 >= 0)
				{
					c = Image.GetColor(i - Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, node.y - 1, Image.Width));
				}
			}

			for (int i = node.x - 1; i >= 0; i--)
			{
				Color c = Image.GetColor(i, node.y);
				if (!c.CheckEqual(TargetColor) || c.CheckEqual(ReplacementColor))
					break;
				Image.SetColor(i, node.y, ReplacementColor);
				if (node.y + 1 < Image.Height)
				{
					c = Image.GetColor(i + Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, node.y + 1, Image.Width));
				}
				if (node.y - 1 >= 0)
				{
					c = Image.GetColor(i - Image.Width, node.y);
					if (c.CheckEqual(TargetColor) && !c.CheckEqual(ReplacementColor))
						Q.push(Node(i, node.y - 1, Image.Width));
				}
			}
		}

	}

	void SetBrightTexture(Texture Image, Color MainColor)
	{
		Color tempColor;
		float ratio;
		int HalfWidth = Image.Width / 2;

		for (int i = 0; i < HalfWidth + 1; i++)
		{
			ratio = (float)i / (float)HalfWidth;
			tempColor = Color::Lerp(Black, MainColor, ratio);
			for (int j = 0; j < Image.Height; j++)
			{
				Image.SetColor(i, j, tempColor);
			}
		}

		for (int i = 0; i < HalfWidth; i++)
		{
			ratio = (float)i / (float)HalfWidth;
			tempColor = Color::Lerp(MainColor, White, ratio);
			for (int j = 0; j < Image.Height; j++)
			{
				Image.SetColor(i + HalfWidth, j, tempColor);
			}
		}
	}

	void DrawBrushTip(Texture Image, Brush BrushData, Color DrawColor, int x, int y)
	{
		const int HalfBrushSize = BrushData.Size >> 1;
		for (int j = 0; j < BrushData.Size; j++)
		{
			int BrushYW = j * BrushData.Size;
			int ImYW = (j + y - HalfBrushSize) * Image.Width;
			for (int i = 0; i < BrushData.Size; i++)
			{
				if (BrushData.GetBinaryColorYW(i, BrushYW))
				{
					Image.SetColorYW(i + x - HalfBrushSize, ImYW, DrawColor);
				}
			}
		}
	}

	void DrawLine(Texture Image, Brush BrushData, Color DrawColor, int x0, int y0, int x1, int y1)
	{
		// prepare our dy, dx, stepx, stepy
		int dy = y1 - y0;
		int dx = x1 - x0;
		int stepx, stepy;

		if (dy < 0)
		{
			dy = -dy;
			stepy = -BrushData.Spacing;
		}
		else
		{
			stepy = BrushData.Spacing;
		}

		if (dx < 0)
		{
			dx = -dx;
			stepx = -BrushData.Spacing;
		}
		else
		{
			stepx = BrushData.Spacing;
		}

		dy <<= 1;
		dx <<= 1;

		int fraction = 0;

		//DrawBrushTip(Image, BrushData, DrawColor, x0, y0);

		if (dx > dy)
		{
			fraction = dy - (dx >> 1);
			while (abs(x0 - x1) > BrushData.Spacing)
			{
				if (fraction >= 0)
				{
					y0 += stepy;
					fraction -= dx;
				}

				x0 += stepx;
				fraction += dy;
				DrawBrushTip(Image, BrushData, DrawColor, x0, y0);
			}
		}
		else
		{
			fraction = dx - (dy >> 1);
			while (abs(y0 - y1) > BrushData.Spacing)
			{
				if (fraction >= 0)
				{
					x0 += stepx;
					fraction -= dy;
				}
				y0 += stepy;
				fraction += dx;
				DrawBrushTip(Image, BrushData, DrawColor, x0, y0);
			}
		}

	}
}