using System.Globalization;
using LethalMenu.Language;
using Newtonsoft.Json;
using UnityEngine;

namespace LethalMenu.Util;

public class RGBAColor
{
	public float r;

	public float g;

	public float b;

	public float a;

	[JsonConstructor]
	public RGBAColor(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public RGBAColor(int r, int g, int b, float a)
	{
		this.r = (float)r / 255f;
		this.g = (float)g / 255f;
		this.b = (float)b / 255f;
		this.a = a;
	}

	public RGBAColor(string hexCode, float alpha = 1f)
	{
		try
		{
			if (hexCode.StartsWith("#"))
			{
				hexCode = hexCode.Substring(1);
			}
			int num = int.Parse(hexCode, NumberStyles.HexNumber);
			r = ((hexCode.Length == 8) ? ((float)((num >> 24) & 0xFF) / 255f) : ((float)((num >> 16) & 0xFF) / 255f));
			g = ((hexCode.Length == 8) ? ((float)((num >> 16) & 0xFF) / 255f) : ((float)((num >> 8) & 0xFF) / 255f));
			b = ((hexCode.Length == 8) ? ((float)((num >> 8) & 0xFF) / 255f) : ((float)(num & 0xFF) / 255f));
			a = ((hexCode.Length == 8) ? ((float)(num & 0xFF) / 255f) : alpha);
		}
		catch
		{
			r = 1f;
			g = 1f;
			b = 1f;
			a = 1f;
		}
	}

	public Color GetColor()
	{
		return new Color(r, g, b, a);
	}

	public string GetHexCode()
	{
		if (a < 1f)
		{
			return GetHexCodeAlpha();
		}
		int num = Mathf.Clamp((int)(r * 255f), 0, 255);
		int num2 = Mathf.Clamp((int)(g * 255f), 0, 255);
		int num3 = Mathf.Clamp((int)(b * 255f), 0, 255);
		return ((num << 16) | (num2 << 8) | num3).ToString("X6");
	}

	public string GetHexCodeAlpha()
	{
		int num = Mathf.Clamp((int)(r * 255f), 0, 255);
		int num2 = Mathf.Clamp((int)(g * 255f), 0, 255);
		int num3 = Mathf.Clamp((int)(b * 255f), 0, 255);
		int num4 = Mathf.Clamp((int)(a * 255f), 0, 255);
		return ((num << 24) | (num2 << 16) | (num3 << 8) | num4).ToString("X8");
	}

	public string AsString(string text)
	{
		return "<color=#" + GetHexCode() + ">" + Localization.Localize(text) + "</color>";
	}
}
