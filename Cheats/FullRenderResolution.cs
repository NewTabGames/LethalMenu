using UnityEngine;

namespace LethalMenu.Cheats;

/// <summary>
/// The game draws the 3D view into a small 860x520 texture and stretches it over the screen (the pixelated look).
/// While on, that texture is kept at the real screen size; turning it off puts the game's size back.
/// Applied every frame it differs, so it works when switched mid-round and after respawning or resizing the window.
/// </summary>
internal class FullRenderResolution : Cheat
{
	private const int GameWidth = 860;

	private const int GameHeight = 520;

	private static bool applied;

	public override void Update()
	{
		RenderTexture texture = PlayerTexture();
		if (texture == null)
		{
			return;
		}
		bool on = Hack.FullRenderResolution.IsEnabled();
		if (!on && !applied)
		{
			return; // never touched it: leave the game (and other mods) alone
		}
		int width = on ? Mathf.Max(Screen.width, 1) : GameWidth;
		int height = on ? Mathf.Max(Screen.height, 1) : GameHeight;
		if (texture.width == width && texture.height == height)
		{
			applied = on;
			return;
		}
		Resize(texture, width, height);
		applied = on;
	}

	/// <summary>Puts the game's low-res render target back (used when ejecting).</summary>
	public static void Restore()
	{
		if (!applied)
		{
			return;
		}
		applied = false;
		RenderTexture texture = PlayerTexture();
		if (texture != null && (texture.width != GameWidth || texture.height != GameHeight))
		{
			Resize(texture, GameWidth, GameHeight);
		}
	}

	private static RenderTexture PlayerTexture()
	{
		var player = GameNetworkManager.Instance != null ? GameNetworkManager.Instance.localPlayerController : null;
		if (player == null || player.gameplayCamera == null)
		{
			return null;
		}
		return player.gameplayCamera.targetTexture;
	}

	// A render texture that already exists cannot just be given a new size; it has to be released and made again.
	private static void Resize(RenderTexture texture, int width, int height)
	{
		texture.Release();
		texture.width = width;
		texture.height = height;
		texture.Create();
	}
}
