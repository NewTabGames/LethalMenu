using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LethalMenu.Language;
using UnityEngine;

namespace LethalMenu.Util;

public class UI
{
	public static void Header(string header, bool space = false)
	{
		if (space)
		{
			GUILayout.Space(20f);
		}
		GUILayout.Label(Localization.Localize(header), new GUIStyle(GUI.skin.label)
		{
			alignment = TextAnchor.MiddleCenter,
			fontStyle = FontStyle.Bold
		});
	}

	public static void SubHeader(string header, bool space = false)
	{
		if (space)
		{
			GUILayout.Space(20f);
		}
		GUILayout.Label(Localization.Localize(header), new GUIStyle(GUI.skin.label)
		{
			fontStyle = FontStyle.Bold
		});
	}

	public static void Label(string header, string label, RGBAColor color = null)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		GUILayout.Label((color == null) ? Localization.Localize(label) : color.AsString(label));
		GUILayout.EndHorizontal();
	}

	public static void Label(string label, RGBAColor color = null)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label((color == null) ? Localization.Localize(label) : color.AsString(label));
		GUILayout.EndHorizontal();
	}

	public static void Hack(Hack hack, string header, params object[] param)
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (Settings.b_HackHighlight)
		{
			GUIStyleState normal = gUIStyle.normal;
			Color textColor;
			if (!Settings.b_HackHighlight || !HackExtensions.ToggleFlags.TryGetValue(hack, out var value))
			{
				textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				textColor = (value ? Settings.c_hackhighlight.GetColor() : GUI.skin.label.normal.textColor);
			}
			normal.textColor = textColor;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header), gUIStyle);
		GUILayout.FlexibleSpace();
		if (hack.Button())
		{
			hack.Execute(param);
		}
		GUILayout.EndHorizontal();
	}

	public static void Hack(Hack hack, string[] header, params object[] param)
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (Settings.b_HackHighlight)
		{
			GUIStyleState normal = gUIStyle.normal;
			Color textColor;
			if (!Settings.b_HackHighlight || !HackExtensions.ToggleFlags.TryGetValue(hack, out var value))
			{
				textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				textColor = (value ? Settings.c_hackhighlight.GetColor() : GUI.skin.label.normal.textColor);
			}
			normal.textColor = textColor;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header), gUIStyle);
		GUILayout.FlexibleSpace();
		if (hack.Button())
		{
			hack.Execute(param);
		}
		GUILayout.EndHorizontal();
	}

	public static void Hack(Hack hack, string header, string buttonText, params object[] param)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(Localization.Localize(buttonText)))
		{
			hack.Execute(param);
		}
		GUILayout.EndHorizontal();
	}

	public static void Button(string header, Action action, string btnText = "General.Execute")
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(Localization.Localize(btnText)))
		{
			action();
		}
		GUILayout.EndHorizontal();
	}

	public static void Button(string[] header, Action action, string btnText = "General.Execute")
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(Localization.Localize(btnText)))
		{
			action();
		}
		GUILayout.EndHorizontal();
	}

	public static void Toggle(string header, ref bool value, string enabled = "General.Enable", string disabled = "General.Disable", params Action<bool>[] action)
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (Settings.b_HackHighlight)
		{
			gUIStyle.normal.textColor = (value ? Settings.c_hackhighlight.GetColor() : GUI.skin.button.normal.textColor);
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header), gUIStyle);
		GUILayout.FlexibleSpace();
		bool newValue = !value;
		if (GUILayout.Button(Localization.Localize(value ? disabled : enabled)))
		{
			value = newValue;
			action.ToList().ForEach((Action<bool> a) =>
			{
				a(newValue);
			});
		}
		GUILayout.EndHorizontal();
	}

	public static void ToggleAction(string header, ref bool value, string enabled = "General.Enable", string disabled = "General.Disable", params Action[] action)
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (Settings.b_HackHighlight)
		{
			gUIStyle.normal.textColor = (value ? Settings.c_hackhighlight.GetColor() : GUI.skin.button.normal.textColor);
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header), gUIStyle);
		GUILayout.FlexibleSpace();
		bool flag = !value;
		if (GUILayout.Button(Localization.Localize(value ? disabled : enabled)))
		{
			value = flag;
			action.ToList().ForEach((Action a) =>
			{
				a();
			});
		}
		GUILayout.EndHorizontal();
	}

	public static void HackSlider(Hack hack, string header, string displayValue, ref float value, float min, float max, params object[] param)
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (Settings.b_HackHighlight)
		{
			GUIStyleState normal = gUIStyle.normal;
			Color textColor;
			if (!Settings.b_HackHighlight || !HackExtensions.ToggleFlags.TryGetValue(hack, out var value2))
			{
				textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				textColor = (value2 ? Settings.c_hackhighlight.GetColor() : GUI.skin.label.normal.textColor);
			}
			normal.textColor = textColor;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header) + " (" + displayValue + ")", gUIStyle);
		GUILayout.FlexibleSpace();
		GUIStyle slider = new GUIStyle(GUI.skin.horizontalSlider)
		{
			alignment = TextAnchor.MiddleCenter,
			fixedWidth = Settings.i_sliderWidth
		};
		value = CenteredSlider(value, min, max, slider, GUI.skin.horizontalSliderThumb);
		if (hack.Button())
		{
			hack.Execute(param);
		}
		GUILayout.EndHorizontal();
	}

	public static void HackSlider(Hack hack, string[] header, string displayValue, ref float value, float min, float max, params object[] param)
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		if (Settings.b_HackHighlight)
		{
			GUIStyleState normal = gUIStyle.normal;
			Color textColor;
			if (!Settings.b_HackHighlight || !HackExtensions.ToggleFlags.TryGetValue(hack, out var value2))
			{
				textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				textColor = (value2 ? Settings.c_hackhighlight.GetColor() : GUI.skin.label.normal.textColor);
			}
			normal.textColor = textColor;
		}
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header) + " (" + displayValue + ")", gUIStyle);
		GUILayout.FlexibleSpace();
		GUIStyle slider = new GUIStyle(GUI.skin.horizontalSlider)
		{
			alignment = TextAnchor.MiddleCenter,
			fixedWidth = Settings.i_sliderWidth
		};
		value = CenteredSlider(value, min, max, slider, GUI.skin.horizontalSliderThumb);
		if (hack.Button())
		{
			hack.Execute(param);
		}
		GUILayout.EndHorizontal();
	}

	public static void Textbox(string label, ref string value, string regex = "", int size = 3, bool big = true)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(label));
		GUILayout.FlexibleSpace();
		value = GUILayout.TextField(value, GUILayout.Width(big ? (Settings.i_textboxWidth * size) : Settings.i_textboxWidth));
		value = Regex.Replace(value, regex, "");
		GUILayout.EndHorizontal();
	}

	public static void TextboxAction(string[] label, ref string value, string regex, int length, params UIButton[] buttons)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(label));
		GUILayout.FlexibleSpace();
		value = GUILayout.TextField(value, length, GUILayout.Width(Settings.i_textboxWidth));
		value = Regex.Replace(value, regex, "");
		buttons.ToList().ForEach((UIButton btn) =>
		{
			btn.Draw();
		});
		GUILayout.EndHorizontal();
	}

	public static void TextboxAction(string label, ref string value, string regex, int length, params UIButton[] buttons)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(label));
		GUILayout.FlexibleSpace();
		value = GUILayout.TextField(value, length, GUILayout.Width(Settings.i_textboxWidth));
		value = Regex.Replace(value, regex, "");
		buttons.ToList().ForEach((UIButton btn) =>
		{
			btn.Draw();
		});
		GUILayout.EndHorizontal();
	}

	public static void Divider(int height = 1)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Box("", GUILayout.ExpandWidth(expand: true), GUILayout.Height(height));
		GUILayout.EndHorizontal();
	}

	public static void Actions(params UIButton[] buttons)
	{
		GUILayout.BeginHorizontal();
		buttons.ToList().ForEach((UIButton btn) =>
		{
			btn.Draw();
		});
		GUILayout.EndHorizontal();
	}

	/// <summary>
	/// Horizontal slider centered vertically in its row. A plain slider sits at the top of a row whose height
	/// comes from the buttons/labels next to it, so its thumb poked into the row above.
	/// </summary>
	private static float CenteredSlider(float value, float min, float max, GUIStyle slider, GUIStyle thumb, params GUILayoutOption[] options)
	{
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		value = GUILayout.HorizontalSlider(value, min, max, slider, thumb, options);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		return value;
	}

	public static void Slider(string header, string displayValue, ref float value, float min, float max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header) + " ( " + displayValue + " )");
		GUILayout.FlexibleSpace();
		value = CenteredSlider(value, min, max, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, GUILayout.Width(Settings.i_sliderWidth));
		GUILayout.EndHorizontal();
	}

	public static void Slider(string header, string displayValue, ref float value, float min, float max, float step = 1f)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header) + " ( " + displayValue + " )");
		GUILayout.FlexibleSpace();
		value = (float)Math.Round(CenteredSlider(value, min, max, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, GUILayout.Width(Settings.i_sliderWidth)) / step) * step;
		GUILayout.EndHorizontal();
	}

	public static void NumSelect(string header, ref int value, int min, int max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		GUILayout.Label(value.ToString());
		if (GUILayout.Button("-"))
		{
			value = Mathf.Clamp(value - 1, min, max);
		}
		if (GUILayout.Button("+"))
		{
			value = Mathf.Clamp(value + 1, min, max);
		}
		GUILayout.EndHorizontal();
	}

	public static void PercentSelect(string header, ref float value, float min = 0.1f)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		GUILayout.Label(value.ToString());
		if (GUILayout.Button("-"))
		{
			value = Mathf.Clamp(value - 0.01f, min, 1f);
		}
		if (GUILayout.Button("+"))
		{
			value = Mathf.Clamp(value + 0.01f, min, 1f);
		}
		GUILayout.EndHorizontal();
	}

	public static void Checkbox(string header, ref bool value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		value = GUILayout.Toggle(value, "");
		GUILayout.EndHorizontal();
	}

	public static void Checkbox(string header, bool isChecked, Action onChanged)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		bool flag = GUILayout.Toggle(isChecked, "");
		if (flag != isChecked)
		{
			onChanged();
		}
		GUILayout.EndHorizontal();
	}

	public static void Select(string header, ref int index, params UIOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		options[index].Draw();
		if (GUILayout.Button("-"))
		{
			index = ((index == 0) ? (options.Length - 1) : Mathf.Clamp(index - 1, 0, options.Length - 1));
		}
		if (GUILayout.Button("+"))
		{
			index = ((index != options.Length - 1) ? Mathf.Clamp(index + 1, 0, options.Length - 1) : 0);
		}
		GUILayout.EndHorizontal();
	}

	public static void Select(string header, ref int index, ref string value, string regex, int length, params UIOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		value = GUILayout.TextField(value, length, GUILayout.Width(Settings.i_textboxWidth));
		value = Regex.Replace(value, regex, "");
		options[index].Draw();
		if (GUILayout.Button("-"))
		{
			index = ((index == 0) ? (options.Length - 1) : Mathf.Clamp(index - 1, 0, options.Length - 1));
		}
		if (GUILayout.Button("+"))
		{
			index = ((index != options.Length - 1) ? Mathf.Clamp(index + 1, 0, options.Length - 1) : 0);
		}
		GUILayout.EndHorizontal();
	}

	public static void Select(string header, ref int index, ref string value, string regex, int length, int size = 0, params UIOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(Localization.Localize(header));
		GUILayout.FlexibleSpace();
		value = GUILayout.TextField(value, length, GUILayout.Width(Settings.i_textboxWidth * size));
		value = Regex.Replace(value, regex, "");
		options[index].Draw();
		if (GUILayout.Button("-"))
		{
			index = ((index == 0) ? (options.Length - 1) : Mathf.Clamp(index - 1, 0, options.Length - 1));
		}
		if (GUILayout.Button("+"))
		{
			index = ((index != options.Length - 1) ? Mathf.Clamp(index + 1, 0, options.Length - 1) : 0);
		}
		GUILayout.EndHorizontal();
	}

	public static void ButtonGrid<T>(List<T> objects, Func<T, string> textSelector, string search, Action<T> action, int numPerRow, int btnWidth = 175)
	{
		List<T> list = objects.FindAll((T x) => textSelector(x).ToLower().Contains(search.ToLower()));
		int num = Mathf.CeilToInt((float)list.Count / (float)numPerRow);
		for (int num2 = 0; num2 < num; num2++)
		{
			GUILayout.BeginHorizontal();
			for (int num3 = 0; num3 < numPerRow; num3++)
			{
				int num4 = num2 * numPerRow + num3;
				if (num4 >= list.Count)
				{
					break;
				}
				T val = list[num4];
				if (GUILayout.Button(textSelector(val), GUILayout.Width(btnWidth)))
				{
					action(val);
				}
			}
			GUILayout.EndHorizontal();
		}
	}
}
