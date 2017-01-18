#include "Draw.h"
#include <algorithm>
#include <queue>
#include <string>

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

	void DrawLine(Texture Image, Brush BrushData, Color DrawColor, int x0, int y0, int x1, int y1, Vector* FinalPos)
	{
		const int SpacingY = (int)(BrushData.Spacing * BrushData.Direction.y);
		const int SpacingX = (int)(BrushData.Spacing * BrushData.Direction.x);

		int InsanityCheck = 0;
		bool bBreak = true;


		if (abs(y1 - y0) < abs(x1 - x0))
		{
			while (abs(SpacingX) < abs(x1 - x0))
			{
				x0 += SpacingX;
				y0 += SpacingY;

				DrawBrushTip(Image, BrushData, DrawColor, x0, y0);
				InsanityCheck++;
				if (InsanityCheck > 500)
				{
					Print("Insanity Gone, SpacingY: ");
					break;
				}
			}
		}
		else
		{
			while (abs(SpacingY) < abs(y1 - y0))
			{
				x0 += SpacingX;
				y0 += SpacingY;

				DrawBrushTip(Image, BrushData, DrawColor, x0, y0);
				InsanityCheck++;
				if (InsanityCheck > 500)
				{
					Print("Insanity Gone, SpacingY: " );
					break;
				}
			}
		}


		FinalPos->x = (float)x0;
		FinalPos->y = (float)y0;
		Print(std::to_string(InsanityCheck));
	}

	void RegisterDebugCallback(DebugCallback callback)
	{
		if (callback)
		{
			gDebugCallback = callback;
		}
	}
}

void Print(std::string message)
{
	gDebugCallback(message.c_str());
}