using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class AlwaysShowClock : Cheat
{
	public override void Update()
	{
		if (Hack.AlwaysShowClock.IsEnabled())
		{
			HUDManager.Instance.SetClockVisible(visible: true);
		}
	}

	public override void OnGui()
	{
		if (Hack.SimpleClock.IsEnabled())
		{
			VisualUtil.DrawString(new Vector2(Screen.width / 2, 25f), GetTime(), Color.white, centered: true, alignMiddle: true, bold: true, forceOnScreen: true, 40);
			VisualUtil.DrawString(new Vector2(Screen.width / 2, 55f), TimeOfDay.Instance.dayMode.ToString(), Color.white, centered: true, alignMiddle: true, bold: true, forceOnScreen: true, 16);
		}
	}

	private string GetTime()
	{
		int num = (int)((double)TimeOfDay.Instance.normalizedTimeOfDay * (60.0 * (double)TimeOfDay.Instance.numberOfHours)) + 360;
		int num2 = (int)Mathf.Floor(num / 60);
		if (num2 >= 24)
		{
			return "12:00 AM";
		}
		string text = ((num2 >= 12) ? "PM" : "AM");
		if (num2 > 12)
		{
			num2 %= 12;
		}
		int num3 = num % 60;
		return $"{num2:00}:{num3:00} ".TrimStart('0') + text;
	}
}
