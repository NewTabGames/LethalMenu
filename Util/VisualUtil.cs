using System;
using LethalMenu.Handler;
using UnityEngine;

namespace LethalMenu.Util;

public class VisualUtil : Cheat
{
	public static Texture2D lineTex;

	public static GUIStyle StringStyle { get; set; } = new GUIStyle(GUI.skin.label);

	public static Color Color
	{
		get
		{
			return GUI.color;
		}
		set
		{
			GUI.color = value;
		}
	}

	public static void DrawString(Vector2 position, string label, Color color, bool centered = true, bool alignMiddle = false, bool bold = false, bool forceOnScreen = false, int fontSize = -1)
	{
		Color color2 = GUI.color;
		Color = color;
		DrawString(position, label, centered, alignMiddle, bold, forceOnScreen, fontSize);
		GUI.color = color2;
	}

	public static void DrawDistanceString(Vector2 position, string label, RGBAColor color, float distance, bool showDistance = true)
	{
		if (showDistance)
		{
			label = label + "\n" + distance + "m";
		}
		DrawString(position, label, color.GetColor(), centered: true, alignMiddle: true);
	}

	public static void DrawString(Vector2 position, string label, RGBAColor color, bool centered = true, bool alignMiddle = false, bool bold = false, bool forceOnScreen = false, int fontSize = -1)
	{
		DrawString(position, label, color.GetColor(), centered, alignMiddle, bold, forceOnScreen, fontSize);
	}

	public static void DrawString(Vector2 position, string label, bool centered = true, bool alignMiddle = false, bool bold = false, bool forceOnScreen = false, int fontSize = -1)
	{
		GUIContent content = new GUIContent(label);
		StringStyle.fontSize = ((fontSize < 0) ? Settings.i_menuFontSize : fontSize);
		StringStyle.fontStyle = (bold ? FontStyle.Bold : FontStyle.Normal);
		Vector2 vector = StringStyle.CalcSize(content);
		Vector2 position2 = (centered ? (position - vector / 2f) : position);
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (alignMiddle)
		{
			gUIStyle.alignment = TextAnchor.MiddleCenter;
		}
		else
		{
			gUIStyle.alignment = TextAnchor.MiddleLeft;
		}
		if (bold)
		{
			gUIStyle.fontStyle = FontStyle.Bold;
		}
		if (fontSize > 0)
		{
			gUIStyle.fontSize = fontSize;
		}
		Rect position3 = new Rect(position2, vector);
		if (forceOnScreen)
		{
			if (position3.x < 0f)
			{
				position3.x = 10f;
			}
			if (position3.y < 0f)
			{
				position3.y = 10f;
			}
			if (position3.x + position3.width > (float)Screen.width)
			{
				position3.x = (float)Screen.width - position3.width - 10f;
			}
			if (position3.y + position3.height > (float)Screen.height)
			{
				position3.y = (float)Screen.height - position3.height - 10f;
			}
		}
		GUI.Label(position3, content, gUIStyle);
	}

	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
	{
		Matrix4x4 matrix = GUI.matrix;
		if (!lineTex)
		{
			lineTex = new Texture2D(1, 1);
		}
		Color color2 = GUI.color;
		GUI.color = color;
		float num = Vector3.Angle(pointB - pointA, Vector2.right);
		if (pointA.y > pointB.y)
		{
			num = 0f - num;
		}
		Vector2 scale = new Vector2((pointB - pointA).magnitude, width);
		if (scale.x != 0f && scale.y != 0f)
		{
			GUIUtility.ScaleAroundPivot(scale, new Vector2(pointA.x, pointA.y + 0.5f));
			GUIUtility.RotateAroundPivot(num, pointA);
			GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1f, 1f), lineTex);
			GUI.matrix = matrix;
			GUI.color = color2;
		}
	}

	public static void DrawLine(Vector2 pointA, Vector2 pointB, RGBAColor color, float width)
	{
		DrawLine(pointA, pointB, color.GetColor(), width);
	}

	public static void DrawBoxOutline(GameObject GameObject, Color color, float thickness)
	{
		DrawBoxOutline(GameObject.GetBounds(), color, thickness);
	}

	public static void DrawBoxOutline(Bounds bounds, Color color, float thickness)
	{
		Vector3[] array = new Vector3[8]
		{
			new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
			new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
			new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
			new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
			new Vector3(bounds.min.x, bounds.min.y, bounds.max.z)
		};
		if (Cheat.WorldToScreen(array[0], out var screen) && Cheat.WorldToScreen(array[1], out var screen2) && Cheat.WorldToScreen(array[2], out var screen3) && Cheat.WorldToScreen(array[3], out var screen4) && Cheat.WorldToScreen(array[4], out var screen5) && Cheat.WorldToScreen(array[5], out var screen6) && Cheat.WorldToScreen(array[6], out var screen7) && Cheat.WorldToScreen(array[7], out var screen8))
		{
			DrawLine(screen, screen2, color, thickness);
			DrawLine(screen2, screen3, color, thickness);
			DrawLine(screen3, screen4, color, thickness);
			DrawLine(screen4, screen, color, thickness);
			DrawLine(screen5, screen6, color, thickness);
			DrawLine(screen6, screen7, color, thickness);
			DrawLine(screen7, screen8, color, thickness);
			DrawLine(screen8, screen5, color, thickness);
			DrawLine(screen, screen5, color, thickness);
			DrawLine(screen2, screen6, color, thickness);
			DrawLine(screen3, screen7, color, thickness);
			DrawLine(screen4, screen8, color, thickness);
		}
	}

	public static void DrawBoxOutline(GameObject GameObject, RGBAColor color, float thickness)
	{
		DrawBoxOutline(GameObject, color.GetColor(), thickness);
	}

	public static void DrawDot(Vector2 position, Color color, float radius)
	{
		Color color2 = GUI.color;
		Vector2 vector = position;
		for (float num = 0f; num < 360f; num++)
		{
			float x = vector.x + Mathf.Cos(MathF.PI / 180f * num) * radius;
			float y = vector.y + Mathf.Sin(MathF.PI / 180f * num) * radius;
			Vector2 vector2 = new Vector2(x, y);
			GUI.DrawTexture(new Rect(vector2.x, vector2.y, 1f, 1f), Texture2D.whiteTexture);
		}
		GUI.color = color2;
	}

	public static void DrawDot(Vector2 position, RGBAColor color, float radius)
	{
		DrawDot(position, color.GetColor(), radius);
	}
}
