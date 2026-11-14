using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LethalMenu.Cheats;
using LethalMenu.Language;
using LethalMenu.Menu.Core;
using LethalMenu.Themes;
using LethalMenu.Types;
using LethalMenu.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace LethalMenu;

internal class Settings
{
	internal class Changelog
	{
		public static List<string> changes;

		public static void ReadChanges()
		{
			changes = new List<string>();
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LethalMenu.Resources.Changelog.txt");
			using StreamReader streamReader = new StreamReader(stream);
			while (!streamReader.EndOfStream)
			{
				changes.Add(streamReader.ReadLine());
			}
		}
	}

	internal class Config
	{
		public static string config = "lethalmenu.config.json";

		public static string defaultConf = "lethalmenu.default.config.json";

		public static void CreateConfigIfNotExists()
		{
			if (!HasConfig())
			{
				SaveConfig();
			}
		}

		public static void SaveDefaultConfig()
		{
			SaveConfig(defaultConf);
		}

		public static bool HasConfig()
		{
			return config != null && File.Exists(config);
		}

		public static void SaveConfig()
		{
			SaveConfig(config);
		}

		public static void SaveConfig(string conf)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			foreach (KeyValuePair<Hack, ButtonControl> keyBind in HackExtensions.KeyBinds)
			{
				string key = keyBind.Key.ToString();
				string value = ((keyBind.Value.GetType() == typeof(KeyControl)) ? ((KeyControl)keyBind.Value).keyCode.ToString() : keyBind.Value.displayName);
				dictionary.Add(key, value);
			}
			foreach (KeyValuePair<Hack, bool> toggleFlag in HackExtensions.ToggleFlags)
			{
				if (!toggleFlag.Key.CanBeExecuted())
				{
					string key2 = toggleFlag.Key.ToString();
					string value2 = toggleFlag.Value.ToString();
					dictionary2.Add(key2, value2);
				}
			}
			foreach (KeyValuePair<EnemyAIType, bool> item in EnemyAITypeExtensions.EnemyFilter)
			{
				if (item.Key != EnemyAIType.Unknown)
				{
					string key3 = item.Key.ToString();
					string value3 = item.Value.ToString();
					dictionary3.Add(key3, value3);
				}
			}
			JObject jObject = new JObject();
			JObject jObject2 = new JObject();
			JObject jObject3 = new JObject();
			JObject jObject4 = new JObject();
			JObject jObject5 = new JObject();
			jObject4["NightVisionIntensity"] = f_nvIntensity.ToString();
			jObject4["NightVisionRange"] = f_nvRange.ToString();
			jObject4["ClimbSpeed"] = f_climbSpeed.ToString();
			jObject4["JumpForce"] = f_jumpForce.ToString();
			jObject4["GrabDistance"] = f_grabDistance.ToString();
			jObject4["MovementSpeed"] = f_movementSpeed.ToString();
			jObject4["NoClipSpeed"] = f_noclipSpeed.ToString();
			jObject4["BreadcrumbInterval"] = f_breadcrumbInterval.ToString();
			jObject4["CrosshairThickness"] = f_crosshairThickness.ToString();
			jObject4["CrosshairType"] = ct_crosshairType.ToString();
			jObject4["CrosshairScale"] = f_crosshairScale.ToString();
			jObject4["ESPDistance"] = f_espDistance.ToString();
			jObject4["DisableSpectatorModels"] = b_disableSpectatorModels.ToString();
			jObject4["VCDisplay"] = b_VCDisplay.ToString();
			jObject4["UseScrapTiers"] = b_useScrapTiers.ToString();
			jObject4["PlayerHPDisplay"] = b_PlayerHPDisplay.ToString();
			jObject4["ShowShipItems"] = b_ShowShipItems.ToString();
			jObject4["FPSCounter"] = b_FPSCounter.ToString();
			jObject4["StealthMode"] = b_StealthMode.ToString();
			jObject4["HackHighlight"] = b_HackHighlight.ToString();
			jObject4["FOV"] = f_fov.ToString();
			jObject4["EnemyFilter"] = JObject.FromObject(dictionary3);
			jObject4["PJSpamSpeed"] = f_pjSpamSpeed.ToString();
			jObject4["ESPThickness"] = f_ESPThickness.ToString();
			jObject4["ObjectQueueDelay"] = f_ObjectQueueDelay.ToString();
			jObject4["NoFog"] = b_NoFog.ToString();
			jObject5["Distance"] = f_chamDistance.ToString();
			jObject5["Object"] = b_chamsObject.ToString();
			jObject5["Enemy"] = b_chamsEnemy.ToString();
			jObject5["Player"] = b_chamsPlayer.ToString();
			jObject5["Landmine"] = b_chamsLandmine.ToString();
			jObject5["Turret"] = b_chamsTurret.ToString();
			jObject5["BigDoor"] = b_chamsBigDoor.ToString();
			jObject5["DoorLock"] = b_chamsDoorLock.ToString();
			jObject5["SteamHazard"] = b_chamsSteamHazard.ToString();
			jObject5["Breaker"] = b_chamsBreaker.ToString();
			jObject5["Ship"] = b_chamsShip.ToString();
			jObject5["SpikeRoofTrap"] = b_chamsSpikeRoofTrap.ToString();
			jObject5["UseSingleChamColor"] = b_UseSingleChamColor.ToString();
			jObject5["MineshaftElevator"] = b_chamsMineshaftElevator.ToString();
			jObject5["EnemyVent"] = b_chamsEnemyVent.ToString();
			jObject5["VainShroud"] = b_chamsVainShroud.ToString();
			jObject5["Cruiser"] = b_chamsCruiser.ToString();
			jObject5["ItemDropShip"] = b_chamsItemDropship.ToString();
			jObject4["Chams"] = jObject5;
			jObject3["Background"] = JsonConvert.SerializeObject(c_background);
			jObject3["Primary"] = JsonConvert.SerializeObject(c_primary);
			jObject3["MenuText"] = JsonConvert.SerializeObject(c_menuText);
			jObject3["Crosshair"] = JsonConvert.SerializeObject(c_crosshair);
			jObject3["HackHighlightColor"] = JsonConvert.SerializeObject(c_hackhighlight);
			jObject3["Chams"] = JsonConvert.SerializeObject(c_chams);
			jObject3["ObjectChams"] = JsonConvert.SerializeObject(c_objectChams);
			jObject3["PlayerChams"] = JsonConvert.SerializeObject(c_playerChams);
			jObject3["EnemyChams"] = JsonConvert.SerializeObject(c_enemyChams);
			jObject3["ShipChams"] = JsonConvert.SerializeObject(c_shipChams);
			jObject3["BreakerChams"] = JsonConvert.SerializeObject(c_breakerChams);
			jObject3["LandmineChams"] = JsonConvert.SerializeObject(c_landmineChams);
			jObject3["TurretChams"] = JsonConvert.SerializeObject(c_turretChams);
			jObject3["BigDoorChams"] = JsonConvert.SerializeObject(c_bigDoorChams);
			jObject3["DoorLockChams"] = JsonConvert.SerializeObject(c_doorLockChams);
			jObject3["SteamHazardChams"] = JsonConvert.SerializeObject(c_steamHazardChams);
			jObject3["SpikeRoofTrapChams"] = JsonConvert.SerializeObject(c_spikeRoofTrapChams);
			jObject3["MineshaftElevatorChams"] = JsonConvert.SerializeObject(c_mineshaftElevatorChams);
			jObject3["EnemyVentChams"] = JsonConvert.SerializeObject(c_enemyVentChams);
			jObject3["VainShroudChams"] = JsonConvert.SerializeObject(c_vainShroudChams);
			jObject3["CruiserChams"] = JsonConvert.SerializeObject(c_CruiserChams);
			jObject3["ItemDropShipChams"] = JsonConvert.SerializeObject(c_itemDropShipChams);
			jObject3["ObjectESP"] = JsonConvert.SerializeObject(c_objectESP);
			jObject3["PlayerESP"] = JsonConvert.SerializeObject(c_playerESP);
			jObject3["EnemyESP"] = JsonConvert.SerializeObject(c_enemyESP);
			jObject3["ShipESP"] = JsonConvert.SerializeObject(c_shipESP);
			jObject3["BreakerESP"] = JsonConvert.SerializeObject(c_breakerESP);
			jObject3["LandmineESP"] = JsonConvert.SerializeObject(c_landmineESP);
			jObject3["TurretESP"] = JsonConvert.SerializeObject(c_turretESP);
			jObject3["BigDoorESP"] = JsonConvert.SerializeObject(c_bigDoorESP);
			jObject3["DoorLockESP"] = JsonConvert.SerializeObject(c_doorLockESP);
			jObject3["EntranceExitESP"] = JsonConvert.SerializeObject(c_entranceExitESP);
			jObject3["SteamHazardESP"] = JsonConvert.SerializeObject(c_steamHazardESP);
			jObject3["SpikeRoofTrapESP"] = JsonConvert.SerializeObject(c_spikeRoofTrapESP);
			jObject3["MineshaftElevatorESP"] = JsonConvert.SerializeObject(c_mineshaftElevatorESP);
			jObject3["EnemyVentESP"] = JsonConvert.SerializeObject(c_enemyVentESP);
			jObject3["VainShroudESP"] = JsonConvert.SerializeObject(c_vainShroudESP);
			jObject3["CruiserESP"] = JsonConvert.SerializeObject(c_CruiserESP);
			jObject3["ItemDropShipESP"] = JsonConvert.SerializeObject(c_itemDropShipESP);
			jObject3["CauseOfDeath"] = JsonConvert.SerializeObject(c_causeOfDeath);
			jObject2["FirstLaunch"] = isFirstLaunch.ToString();
			jObject2["MenuFontSize"] = i_menuFontSize.ToString();
			jObject2["ScreenFontSize"] = i_screenFontSize.ToString();
			jObject2["MenuWidth"] = i_menuWidth.ToString();
			jObject2["MenuHeight"] = i_menuHeight.ToString();
			jObject2["SliderWidth"] = i_sliderWidth.ToString();
			jObject2["TextboxWidth"] = i_textboxWidth.ToString();
			jObject2["MenuAlpha"] = f_menuAlpha.ToString();
			jObject2["DebugMode"] = DebugMode.ToString();
			jObject["Theme"] = Theme.name;
			jObject["Language"] = Localization.Language.Name;
			jObject["Colors"] = jObject3;
			jObject["HackSettings"] = jObject4;
			jObject["MenuSettings"] = jObject2;
			jObject["KeyBinds"] = JObject.FromObject(dictionary);
			jObject["Toggles"] = JObject.FromObject(dictionary2);
			File.WriteAllText(conf, jObject.ToString());
		}

		public static void LoadConfig()
		{
			CreateConfigIfNotExists();
			JObject jObject = JObject.Parse(File.ReadAllText(config));
			if (jObject.TryGetValue("Language", out JToken value))
			{
				Localization.SetLanguage(value.ToString());
			}
			if (jObject.TryGetValue("Theme", out JToken value2))
			{
				Theme.SetTheme(value2.ToString());
			}
			if (jObject.TryGetValue("MenuSettings", out JToken value3))
			{
				JObject jObject2 = value3.ToObject<JObject>();
				if (jObject2.TryGetValue("FirstLaunch", out JToken value4))
				{
					isFirstLaunch = bool.Parse(value4.ToString());
				}
				if (jObject2.TryGetValue("MenuFontSize", out JToken value5))
				{
					i_menuFontSize = int.Parse(value5.ToString());
				}
				if (jObject2.TryGetValue("MenuWidth", out JToken value6))
				{
					i_menuWidth = int.Parse(value6.ToString());
				}
				if (jObject2.TryGetValue("MenuHeight", out JToken value7))
				{
					i_menuHeight = int.Parse(value7.ToString());
				}
				if (jObject2.TryGetValue("SliderWidth", out JToken value8))
				{
					i_sliderWidth = int.Parse(value8.ToString());
				}
				if (jObject2.TryGetValue("TextboxWidth", out JToken value9))
				{
					i_textboxWidth = int.Parse(value9.ToString());
				}
				if (jObject2.TryGetValue("MenuAlpha", out JToken value10))
				{
					f_menuAlpha = float.Parse(value10.ToString());
				}
				if (jObject2.TryGetValue("ScreenFontSize", out JToken value11))
				{
					i_screenFontSize = int.Parse(value11.ToString());
				}
				if (jObject2.TryGetValue("DebugMode", out JToken value12))
				{
					DebugMode = bool.Parse(value12.ToString());
					HackMenu.Instance.ToggleDebugTab(DebugMode);
				}
			}
			if (jObject.TryGetValue("HackSettings", out JToken value13))
			{
				JObject jObject3 = value13.ToObject<JObject>();
				if (jObject3.TryGetValue("NightVisionIntensity", out JToken value14))
				{
					f_nvIntensity = float.Parse(value14.ToString());
				}
				if (jObject3.TryGetValue("NightVisionRange", out JToken value15))
				{
					f_nvRange = float.Parse(value15.ToString());
				}
				if (jObject3.TryGetValue("ClimbSpeed", out JToken value16))
				{
					f_climbSpeed = float.Parse(value16.ToString());
				}
				if (jObject3.TryGetValue("JumpForce", out JToken value17))
				{
					f_jumpForce = float.Parse(value17.ToString());
				}
				if (jObject3.TryGetValue("GrabDistance", out JToken value18))
				{
					f_grabDistance = float.Parse(value18.ToString());
				}
				if (jObject3.TryGetValue("MovementSpeed", out JToken value19))
				{
					f_movementSpeed = float.Parse(value19.ToString());
				}
				if (jObject3.TryGetValue("NoClipSpeed", out JToken value20))
				{
					f_noclipSpeed = float.Parse(value20.ToString());
				}
				if (jObject3.TryGetValue("BreadcrumbInterval", out JToken value21))
				{
					f_breadcrumbInterval = int.Parse(value21.ToString());
				}
				if (jObject3.TryGetValue("CrosshairThickness", out JToken value22))
				{
					f_crosshairThickness = float.Parse(value22.ToString());
				}
				if (jObject3.TryGetValue("CrosshairType", out JToken value23))
				{
					ct_crosshairType = (CrosshairType)Enum.Parse(typeof(CrosshairType), value23.ToString());
				}
				if (jObject3.TryGetValue("CrosshairScale", out JToken value24))
				{
					f_crosshairScale = float.Parse(value24.ToString());
				}
				if (jObject3.TryGetValue("ESPDistance", out JToken value25))
				{
					f_espDistance = float.Parse(value25.ToString());
				}
				if (jObject3.TryGetValue("DisableSpectatorModels", out JToken value26))
				{
					b_disableSpectatorModels = bool.Parse(value26.ToString());
				}
				if (jObject3.TryGetValue("VCDisplay", out JToken value27))
				{
					b_VCDisplay = bool.Parse(value27.ToString());
				}
				if (jObject3.TryGetValue("UseScrapTiers", out JToken value28))
				{
					b_useScrapTiers = bool.Parse(value28.ToString());
				}
				if (jObject3.TryGetValue("PlayerHPDisplay", out JToken value29))
				{
					b_PlayerHPDisplay = bool.Parse(value29.ToString());
				}
				if (jObject3.TryGetValue("ShowShipItems", out JToken value30))
				{
					b_ShowShipItems = bool.Parse(value30.ToString());
				}
				if (jObject3.TryGetValue("FPSCounter", out JToken value31))
				{
					b_FPSCounter = bool.Parse(value31.ToString());
				}
				if (jObject3.TryGetValue("HackHighlight", out JToken value32))
				{
					b_HackHighlight = bool.Parse(value32.ToString());
				}
				if (jObject3.TryGetValue("StealthMode", out JToken stealthValue))
				{
					b_StealthMode = bool.Parse(stealthValue.ToString());
				}
				if (jObject3.TryGetValue("UseSingleChamColor", out JToken value34))
				{
					b_UseSingleChamColor = bool.Parse(value34.ToString());
				}
				if (jObject3.TryGetValue("FOV", out JToken value35))
				{
					f_fov = float.Parse(value35.ToString());
				}
				if (jObject3.TryGetValue("PJSpamSpeed", out JToken value36))
				{
					f_pjSpamSpeed = float.Parse(value36.ToString());
				}
				if (jObject3.TryGetValue("ESPThickness", out JToken value37))
				{
					f_ESPThickness = float.Parse(value37.ToString());
				}
				if (jObject3.TryGetValue("ObjectQueueDelay", out JToken value38))
				{
					f_ObjectQueueDelay = float.Parse(value38.ToString());
				}
				if (jObject3.TryGetValue("NoFog", out JToken value39))
				{
					b_NoFog = bool.Parse(value39.ToString());
				}
				if (jObject3.TryGetValue("EnemyFilter", out JToken value41))
				{
					foreach (KeyValuePair<string, string> item in value41.ToObject<Dictionary<string, string>>())
					{
						string key = item.Key;
						string value42 = item.Value;
						if (Enum.TryParse<EnemyAIType>(key, out var result) && result != EnemyAIType.Unknown)
						{
							bool value43 = bool.TryParse(value42, out var result2) && result2;
							EnemyAITypeExtensions.EnemyFilter[result] = value43;
						}
					}
				}
				if (jObject3.TryGetValue("Chams", out JToken value44))
				{
					JObject jObject4 = value44.ToObject<JObject>();
					if (jObject4.TryGetValue("Distance", out JToken value45))
					{
						f_chamDistance = float.Parse(value45.ToString());
					}
					if (jObject4.TryGetValue("Object", out JToken value46))
					{
						b_chamsObject = bool.Parse(value46.ToString());
					}
					if (jObject4.TryGetValue("Enemy", out JToken value47))
					{
						b_chamsEnemy = bool.Parse(value47.ToString());
					}
					if (jObject4.TryGetValue("Player", out JToken value48))
					{
						b_chamsPlayer = bool.Parse(value48.ToString());
					}
					if (jObject4.TryGetValue("Landmine", out JToken value49))
					{
						b_chamsLandmine = bool.Parse(value49.ToString());
					}
					if (jObject4.TryGetValue("Turret", out JToken value50))
					{
						b_chamsTurret = bool.Parse(value50.ToString());
					}
					if (jObject4.TryGetValue("BigDoor", out JToken value51))
					{
						b_chamsBigDoor = bool.Parse(value51.ToString());
					}
					if (jObject4.TryGetValue("DoorLock", out JToken value52))
					{
						b_chamsDoorLock = bool.Parse(value52.ToString());
					}
					if (jObject4.TryGetValue("SteamHazard", out JToken value53))
					{
						b_chamsSteamHazard = bool.Parse(value53.ToString());
					}
					if (jObject4.TryGetValue("Breaker", out JToken value54))
					{
						b_chamsBreaker = bool.Parse(value54.ToString());
					}
					if (jObject4.TryGetValue("Ship", out JToken value55))
					{
						b_chamsShip = bool.Parse(value55.ToString());
					}
					if (jObject4.TryGetValue("SpikeRoofTrap", out JToken value56))
					{
						b_chamsSpikeRoofTrap = bool.Parse(value56.ToString());
					}
					if (jObject4.TryGetValue("MineshaftElevator", out JToken value57))
					{
						b_chamsMineshaftElevator = bool.Parse(value57.ToString());
					}
					if (jObject4.TryGetValue("EnemyVent", out JToken value58))
					{
						b_chamsEnemyVent = bool.Parse(value58.ToString());
					}
					if (jObject4.TryGetValue("VainShroud", out JToken value59))
					{
						b_chamsVainShroud = bool.Parse(value59.ToString());
					}
					if (jObject4.TryGetValue("Cruiser", out JToken value60))
					{
						b_chamsCruiser = bool.Parse(value60.ToString());
					}
					if (jObject4.TryGetValue("ItemDropShip", out JToken value61))
					{
						b_chamsItemDropship = bool.Parse(value61.ToString());
					}
				}
			}
			if (jObject.TryGetValue("Colors", out JToken value62))
			{
				JObject jObject5 = value62.ToObject<JObject>();
				if (jObject5.TryGetValue("Background", out JToken value63))
				{
					c_background = JsonConvert.DeserializeObject<RGBAColor>(value63.ToString());
				}
				if (jObject5.TryGetValue("Primary", out JToken value64))
				{
					c_primary = JsonConvert.DeserializeObject<RGBAColor>(value64.ToString());
				}
				if (jObject5.TryGetValue("MenuText", out JToken value65))
				{
					c_menuText = JsonConvert.DeserializeObject<RGBAColor>(value65.ToString());
				}
				if (jObject5.TryGetValue("Crosshair", out JToken value66))
				{
					c_crosshair = JsonConvert.DeserializeObject<RGBAColor>(value66.ToString());
				}
				if (jObject5.TryGetValue("HackHighlightColor", out JToken value67))
				{
					c_hackhighlight = JsonConvert.DeserializeObject<RGBAColor>(value67.ToString());
				}
				if (jObject5.TryGetValue("Chams", out JToken value68))
				{
					c_chams = JsonConvert.DeserializeObject<RGBAColor>(value68.ToString());
				}
				if (jObject5.TryGetValue("ObjectESP", out JToken value69))
				{
					c_objectESP = JsonConvert.DeserializeObject<RGBAColor>(value69.ToString());
				}
				if (jObject5.TryGetValue("PlayerESP", out JToken value70))
				{
					c_playerESP = JsonConvert.DeserializeObject<RGBAColor>(value70.ToString());
				}
				if (jObject5.TryGetValue("EnemyESP", out JToken value71))
				{
					c_enemyESP = JsonConvert.DeserializeObject<RGBAColor>(value71.ToString());
				}
				if (jObject5.TryGetValue("ShipESP", out JToken value72))
				{
					c_shipESP = JsonConvert.DeserializeObject<RGBAColor>(value72.ToString());
				}
				if (jObject5.TryGetValue("BreakerESP", out JToken value73))
				{
					c_breakerESP = JsonConvert.DeserializeObject<RGBAColor>(value73.ToString());
				}
				if (jObject5.TryGetValue("LandmineESP", out JToken value74))
				{
					c_landmineESP = JsonConvert.DeserializeObject<RGBAColor>(value74.ToString());
				}
				if (jObject5.TryGetValue("TurretESP", out JToken value75))
				{
					c_turretESP = JsonConvert.DeserializeObject<RGBAColor>(value75.ToString());
				}
				if (jObject5.TryGetValue("BigDoorESP", out JToken value76))
				{
					c_bigDoorESP = JsonConvert.DeserializeObject<RGBAColor>(value76.ToString());
				}
				if (jObject5.TryGetValue("DoorLockESP", out JToken value77))
				{
					c_doorLockESP = JsonConvert.DeserializeObject<RGBAColor>(value77.ToString());
				}
				if (jObject5.TryGetValue("EntranceExitESP", out JToken value78))
				{
					c_entranceExitESP = JsonConvert.DeserializeObject<RGBAColor>(value78.ToString());
				}
				if (jObject5.TryGetValue("SteamHazardESP", out JToken value79))
				{
					c_steamHazardESP = JsonConvert.DeserializeObject<RGBAColor>(value79.ToString());
				}
				if (jObject5.TryGetValue("SpikeRoofTrapESP", out JToken value80))
				{
					c_spikeRoofTrapESP = JsonConvert.DeserializeObject<RGBAColor>(value80.ToString());
				}
				if (jObject5.TryGetValue("EnemyVentESP", out JToken value81))
				{
					c_enemyVentESP = JsonConvert.DeserializeObject<RGBAColor>(value81.ToString());
				}
				if (jObject5.TryGetValue("VainShroudESP", out JToken value82))
				{
					c_vainShroudESP = JsonConvert.DeserializeObject<RGBAColor>(value82.ToString());
				}
				if (jObject5.TryGetValue("CruiserESP", out JToken value83))
				{
					c_CruiserESP = JsonConvert.DeserializeObject<RGBAColor>(value83.ToString());
				}
				if (jObject5.TryGetValue("ItemDropShipESP", out JToken value84))
				{
					c_itemDropShipESP = JsonConvert.DeserializeObject<RGBAColor>(value84.ToString());
				}
				if (jObject5.TryGetValue("CauseOfDeath", out JToken value85))
				{
					c_causeOfDeath = JsonConvert.DeserializeObject<RGBAColor>(value85.ToString());
				}
				if (jObject5.TryGetValue("MineshaftElevatorESP", out JToken value86))
				{
					c_mineshaftElevatorESP = JsonConvert.DeserializeObject<RGBAColor>(value86.ToString());
				}
				if (jObject5.TryGetValue("MineshaftElevatorChams", out JToken value87))
				{
					c_mineshaftElevatorChams = JsonConvert.DeserializeObject<RGBAColor>(value87.ToString());
				}
				if (jObject5.TryGetValue("VainShroudChams", out JToken value88))
				{
					c_vainShroudChams = JsonConvert.DeserializeObject<RGBAColor>(value88.ToString());
				}
				if (jObject5.TryGetValue("CruiserChams", out JToken value89))
				{
					c_CruiserChams = JsonConvert.DeserializeObject<RGBAColor>(value89.ToString());
				}
				if (jObject5.TryGetValue("ItemDropShipChams", out JToken value90))
				{
					c_itemDropShipChams = JsonConvert.DeserializeObject<RGBAColor>(value90.ToString());
				}
				if (jObject5.TryGetValue("EnemyVentChams", out JToken value91))
				{
					c_enemyVentChams = JsonConvert.DeserializeObject<RGBAColor>(value91.ToString());
				}
			}
			if (jObject.TryGetValue("KeyBinds", out JToken value92))
			{
				HackExtensions.KeyBinds.Clear();
				ButtonControl[] source = new ButtonControl[5]
				{
					Mouse.current.leftButton,
					Mouse.current.rightButton,
					Mouse.current.middleButton,
					Mouse.current.forwardButton,
					Mouse.current.backButton
				};
				foreach (KeyValuePair<string, string> item2 in value92.ToObject<Dictionary<string, string>>())
				{
					string key2 = item2.Key;
					string s_key = item2.Value;
					ButtonControl buttonControl = source.FirstOrDefault((ButtonControl k) => k.displayName == s_key);
					ButtonControl buttonControl2 = Keyboard.current.allKeys.FirstOrDefault((KeyControl k) => k.keyCode.ToString() == s_key);
					if (Enum.TryParse<Hack>(key2, out var result3) && (buttonControl != null || buttonControl2 != null))
					{
						result3.SetKeyBind(buttonControl ?? buttonControl2);
					}
				}
			}
			if (!jObject.TryGetValue("Toggles", out JToken value93))
			{
				return;
			}
			foreach (KeyValuePair<string, string> item3 in value93.ToObject<Dictionary<string, string>>())
			{
				string key3 = item3.Key;
				string value94 = item3.Value;
				if (Enum.TryParse<Hack>(key3, out var result4) && !result4.CanBeExecuted())
				{
					bool b = bool.TryParse(value94, out var result5) && result5;
					result4.SetToggle(b);
				}
			}
		}

		public static void RegenerateConfig()
		{
			if (HasConfig())
			{
				File.Delete(config);
			}
			File.Copy(defaultConf, config);
			HackExtensions.KeyBinds.Clear();
			LoadConfig();
		}

		public static void OpenConfig()
		{
			Process.Start("explorer.exe", config);
		}
	}

	public static string version = "v1.0.0";

	public static bool DebugMode = false;

	public static bool isFirstLaunch = true;

	public static int i_menuFontSize = 14;

	public static int i_menuWidth = 810;

	public static int i_menuHeight = 410;

	public static int i_sliderWidth = 100;

	public static int i_textboxWidth = 85;

	public static float f_menuAlpha = 1f;

	public static int i_screenFontSize = 14;

	public static RGBAColor c_background = new RGBAColor(51, 51, 51, 1f);

	public static RGBAColor c_primary = new RGBAColor(165, 55, 253, 1f);

	public static RGBAColor c_menuText = new RGBAColor(255, 255, 255, 1f);

	public static RGBAColor c_crosshair = new RGBAColor(255, 43, 43, 1f);

	public static RGBAColor c_hackhighlight = new RGBAColor(165, 55, 253, 1f);

	public static RGBAColor c_objectESP = new RGBAColor(255, 255, 255, 1f);

	public static RGBAColor c_playerESP = new RGBAColor(0, 255, 0, 1f);

	public static RGBAColor c_bodyESP = new RGBAColor(255, 165, 0, 1f);

	public static RGBAColor c_enemyESP = new RGBAColor(255, 0, 0, 1f);

	public static RGBAColor c_shipESP = new RGBAColor(0, 0, 255, 1f);

	public static RGBAColor c_landmineESP = new RGBAColor(255, 0, 0, 1f);

	public static RGBAColor c_turretESP = new RGBAColor(255, 0, 0, 1f);

	public static RGBAColor c_bigDoorESP = new RGBAColor(0, 255, 255, 1f);

	public static RGBAColor c_doorLockESP = new RGBAColor(128, 128, 128, 255f);

	public static RGBAColor c_entranceExitESP = new RGBAColor(0, 0, 255, 1f);

	public static RGBAColor c_steamHazardESP = new RGBAColor(255, 0, 255, 1f);

	public static RGBAColor c_breakerESP = new RGBAColor(255, 0, 116, 1f);

	public static RGBAColor c_spikeRoofTrapESP = new RGBAColor(139, 69, 19, 1f);

	public static RGBAColor c_vainShroudESP = new RGBAColor(102, 51, 153, 0.1f);

	public static RGBAColor c_enemyVentESP = new RGBAColor(211, 211, 211, 0.1f);

	public static RGBAColor c_itemDropShipESP = new RGBAColor(199, 21, 133, 0.1f);

	public static RGBAColor c_CruiserESP = new RGBAColor(255, 215, 0, 0.1f);

	public static RGBAColor c_mineshaftElevatorESP = new RGBAColor(0, 0, 255, 1f);

	public static RGBAColor c_objectChams = new RGBAColor(255, 255, 255, 0.1f);

	public static RGBAColor c_playerChams = new RGBAColor(0, 255, 0, 0.1f);

	public static RGBAColor c_enemyChams = new RGBAColor(255, 0, 0, 0.1f);

	public static RGBAColor c_shipChams = new RGBAColor(0, 0, 255, 0.1f);

	public static RGBAColor c_landmineChams = new RGBAColor(255, 0, 0, 0.1f);

	public static RGBAColor c_turretChams = new RGBAColor(255, 0, 0, 0.1f);

	public static RGBAColor c_bigDoorChams = new RGBAColor(0, 255, 255, 0.1f);

	public static RGBAColor c_doorLockChams = new RGBAColor(0.5f, 0.5f, 0.5f, 0.1f);

	public static RGBAColor c_steamHazardChams = new RGBAColor(255, 0, 255, 0.1f);

	public static RGBAColor c_breakerChams = new RGBAColor(255, 0, 116, 0.1f);

	public static RGBAColor c_enemyVentChams = new RGBAColor(211, 211, 211, 0.1f);

	public static RGBAColor c_vainShroudChams = new RGBAColor(102, 51, 153, 0.1f);

	public static RGBAColor c_spikeRoofTrapChams = new RGBAColor(139, 69, 19, 0.1f);

	public static RGBAColor c_itemDropShipChams = new RGBAColor(199, 21, 133, 0.1f);

	public static RGBAColor c_CruiserChams = new RGBAColor(255, 215, 0, 0.1f);

	public static RGBAColor c_mineshaftElevatorChams = new RGBAColor(0, 0, 255, 0.1f);

	public static RGBAColor c_chams = new RGBAColor(238, 111, 255, 0.1f);

	public static RGBAColor c_error = new RGBAColor(221, 11, 11, 1f);

	public static RGBAColor c_deadPlayer = new RGBAColor(255, 0, 0, 1f);

	public static RGBAColor c_causeOfDeath = new RGBAColor(255, 235, 4, 255f);

	public static Vector3 v_savedLocation = Vector3.zero;

	public static float f_nvIntensity = 3000f;

	public static float f_nvRange = 10000f;

	public static float f_climbSpeed = 8f;

	public static float f_jumpForce = 26f;

	public static float f_grabDistance = 10000f;

	public static float f_movementSpeed = 9.2f;

	public static float f_noclipSpeed = 10f;

	public static float f_crosshairScale = 14f;

	public static float f_crosshairThickness = 1.75f;

	public static float f_breadcrumbInterval = 1f;

	public static float f_espDistance = 5000f;

	public static float f_chamDistance = 15f;

	public static float f_enemyKillDistance = 15f;
	public static float f_fov = 66f;

	public static float f_mouseSensitivity = 0.15f;

	public static float f_inputMovementSpeed = 15f;

	public static float f_carAcceleration = 5f;

	public static float f_carMaxSpeed = 7f;

	public static float f_brakeSpeed = 7f;

	public static float f_pjSpamSpeed = 0.5f;

	public static float f_ESPThickness = 1f;

	public static float f_ObjectQueueDelay = 0.1f;

	public static bool b_disableSpectatorModels = true;

	public static bool b_useScrapTiers = false;

	public static bool b_VCDisplay = false;

	public static bool b_PlayerHPDisplay = false;

	public static bool b_ShowShipItems = false;

	public static bool b_DropItems = false;

	public static bool b_HackHighlight = false;

	public static bool b_FPSCounter = false;

	/// <summary>Stealth Mode: no menu, hotkeys or notifications on this PC; the website is the only control.</summary>
	public static bool b_StealthMode = false;

	public static bool b_NoFog = false;

	public static CrosshairType ct_crosshairType = CrosshairType.Plus;

	public static bool b_chamsObject = false;

	public static bool b_chamsEnemy = false;

	public static bool b_chamsPlayer = false;

	public static bool b_chamsLandmine = false;

	public static bool b_chamsTurret = false;

	public static bool b_chamsBigDoor = false;

	public static bool b_chamsDoorLock = false;

	public static bool b_chamsSteamHazard = false;

	public static bool b_chamsBreaker = false;

	public static bool b_chamsShip = false;

	public static bool b_chamsSpikeRoofTrap = false;

	public static bool b_UseSingleChamColor = false;

	public static bool b_chamsMineshaftElevator = false;

	public static bool b_chamsEnemyVent = false;

	public static bool b_chamsItemDropship = false;

	public static bool b_chamsCruiser = false;

	public static bool b_chamsVainShroud = false;

	public static float f_defaultGrabDistance = -1f;

	public static float f_defaultClimbSpeed = 3f;

	public static float f_defaultJumpForce = 13f;

	public static float f_defaultMovementSpeed = 4.6f;

	public static float f_defaultNightVisionIntensity = 360f;

	public static float f_defaultNightVisionRange = 12f;

	public static float f_defaultFOV = 66f;

	public static float f_slots = 4f;

	public static int[] i_scrapValueThresholds = new int[4] { 30, 50, 75, 100 };

	public static SteamId s_lobbyid;

	public static RGBAColor[] c_scrapValueColors = new RGBAColor[4]
	{
		new RGBAColor(0.5f, 0.5f, 0.5f, 1f),
		new RGBAColor(10, 187, 10, 1f),
		new RGBAColor(255, 0, 255, 1f),
		new RGBAColor(255, 165, 0, 1f)
	};

	public static string debugMessage = "";

	/// <summary>
	/// Super Speed / Super Jump / Fast Climb must never be weaker than the game's normal values
	/// (old configs saved 0.5 speed and 5 jump, which made you slower than normal). Bumps any such value
	/// to a 2x boost. Called after loading the config.
	/// </summary>
	public static void EnsureBoosts()
	{
		if (f_movementSpeed <= f_defaultMovementSpeed)
		{
			f_movementSpeed = f_defaultMovementSpeed * 2f;
		}
		if (f_jumpForce <= f_defaultJumpForce)
		{
			f_jumpForce = f_defaultJumpForce * 2f;
		}
		if (f_climbSpeed <= f_defaultClimbSpeed)
		{
			f_climbSpeed = f_defaultClimbSpeed * 2f;
		}
	}

	public static bool isMenuOpen
	{
		get
		{
			return Hack.OpenMenu.IsEnabled();
		}
		set
		{
			Hack.OpenMenu.SetToggle(value);
		}
	}
}
