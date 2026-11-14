using System;
using LethalMenu.Language;
using UnityEngine;

namespace LethalMenu.Util;

public class UIOption
{
	public string label;

	public object value;

	public Action action;

	public UIOption(string label, object value)
	{
		this.label = label;
		this.value = value;
	}

	public UIOption(string label, Action action)
	{
		this.label = Localization.Localize(label);
		this.action = action;
	}

	public void Draw(ref object refValue)
	{
		if (GUILayout.Button(label))
		{
			refValue = value;
		}
	}

	public void Draw()
	{
		if (GUILayout.Button(label))
		{
			action?.Invoke();
		}
	}
}
