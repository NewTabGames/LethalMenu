using System;
using LethalMenu.Language;
using UnityEngine;

namespace LethalMenu.Util;

public class UIButton
{
	public string label;

	public Action action;

	private GUIStyle style = null;

	public UIButton(string label, Action action, GUIStyle style = null)
	{
		this.label = Localization.Localize(label);
		this.action = action;
		this.style = style;
	}

	public void Draw()
	{
		if ((style != null) ? GUILayout.Button(label, style) : GUILayout.Button(label))
		{
			action();
		}
	}
}
