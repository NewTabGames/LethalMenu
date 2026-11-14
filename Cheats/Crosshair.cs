using LethalMenu.Util;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class Crosshair : Cheat
{
	public override void OnGui()
	{
		if (Hack.Crosshair.IsEnabled())
		{
			Vector2[] array = new Vector2[4]
			{
				new Vector2((float)Screen.width / 2f - Settings.f_crosshairScale, (float)Screen.height / 2f - Settings.f_crosshairScale),
				new Vector2((float)Screen.width / 2f + Settings.f_crosshairScale, (float)Screen.height / 2f + Settings.f_crosshairScale),
				new Vector2((float)Screen.width / 2f + Settings.f_crosshairScale, (float)Screen.height / 2f - Settings.f_crosshairScale),
				new Vector2((float)Screen.width / 2f - Settings.f_crosshairScale, (float)Screen.height / 2f + Settings.f_crosshairScale)
			};
			Vector2[] array2 = new Vector2[4]
			{
				new Vector2((float)Screen.width / 2f - Settings.f_crosshairScale, (float)Screen.height / 2f),
				new Vector2((float)Screen.width / 2f + Settings.f_crosshairScale, (float)Screen.height / 2f),
				new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f - Settings.f_crosshairScale),
				new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f + Settings.f_crosshairScale)
			};
			Vector2[] array3 = new Vector2[1]
			{
				new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f)
			};
			switch (Settings.ct_crosshairType)
			{
			case CrosshairType.X:
				VisualUtil.DrawLine(array[0], array[1], Settings.c_crosshair, Settings.f_crosshairThickness);
				VisualUtil.DrawLine(array[2], array[3], Settings.c_crosshair, Settings.f_crosshairThickness);
				break;
			case CrosshairType.Plus:
				VisualUtil.DrawLine(array2[0], array2[1], Settings.c_crosshair, Settings.f_crosshairThickness);
				VisualUtil.DrawLine(array2[2], array2[3], Settings.c_crosshair, Settings.f_crosshairThickness);
				break;
			case CrosshairType.Dot:
				VisualUtil.DrawDot(array3[0], Settings.c_crosshair, Settings.f_crosshairThickness);
				break;
			}
		}
	}
}
