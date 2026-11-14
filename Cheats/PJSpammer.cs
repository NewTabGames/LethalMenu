using UnityEngine;

namespace LethalMenu.Cheats;

internal class PJSpammer : Cheat
{
	private float lastTime = 0f;

	public override void Update()
	{
		if (!Hack.PJSpammer.IsEnabled() || Time.time - lastTime < Settings.f_pjSpamSpeed)
		{
			return;
		}
		LethalMenu.animatedTriggers.FindAll((AnimatedObjectTrigger c) => c != null && c.transform.parent != null && c.transform.parent.gameObject.name.StartsWith("PlushiePJManContainer")).ForEach((AnimatedObjectTrigger c) =>
		{
			if (c != null)
			{
				c.TriggerAnimation(LethalMenu.localPlayer);
			}
		});
		lastTime = Time.time;
	}
}
