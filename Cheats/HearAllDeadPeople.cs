using System;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace LethalMenu.Cheats;

internal class HearAllDeadPeople : Cheat
{
	[HarmonyPatch(typeof(StartOfRound), "UpdatePlayerVoiceEffects")]
	public static class StartOfRoundUpdatePlayerVoiceEffectsPatch
	{
		public static void Postfix(StartOfRound __instance)
		{
			if (!Hack.HearAllDeadPeople.IsEnabled() || StartOfRound.Instance.shipIsLeaving)
			{
				return;
			}
			for (int i = 0; i < __instance.allPlayerScripts.Length; i++)
			{
				PlayerControllerB playerControllerB = __instance.allPlayerScripts[i];
				if (playerControllerB != null && playerControllerB.currentVoiceChatAudioSource != null && playerControllerB.isPlayerDead)
				{
					AudioSource currentVoiceChatAudioSource = playerControllerB.currentVoiceChatAudioSource;
					AudioLowPassFilter component = currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>();
					AudioHighPassFilter component2 = currentVoiceChatAudioSource.GetComponent<AudioHighPassFilter>();
					if (component != null)
					{
						component.enabled = false;
					}
					if (component2 != null)
					{
						component2.enabled = false;
					}
					currentVoiceChatAudioSource.panStereo = 0f;
					SoundManager.Instance.playerVoicePitchTargets[(int)(IntPtr)(long)playerControllerB.playerClientId] = 1f;
					SoundManager.Instance.SetPlayerPitch(1f, (int)playerControllerB.playerClientId);
					currentVoiceChatAudioSource.spatialBlend = 0f;
					playerControllerB.currentVoiceChatIngameSettings.set2D = true;
					playerControllerB.voicePlayerState.Volume = 1f;
				}
			}
		}
	}
}
