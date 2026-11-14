using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace LethalMenu.Remote;

/// <summary>
/// lethalmenu.remote.json next to the game exe (same folder as lethalmenu.config.json).
/// apiKey / databaseUrl come from your Firebase project's web app config.
/// refreshToken / uid are written by the mod: they are this PC's identity in Firebase.
/// </summary>
internal class RemoteConfig
{
	public const string FileName = "lethalmenu.remote.json";

	/// <summary>Built-in Firebase project (LM-Remote). Public web-app values; access is controlled by the database rules.</summary>
	public const string DefaultApiKey = "AIzaSyDy9IGnzFb1ef9OToBBsHbKulDPmwsolf8";

	public const string DefaultDatabaseUrl = "https://lm-remote-default-rtdb.firebaseio.com";

	[JsonProperty("enabled")]
	public bool Enabled = false;

	[JsonProperty("apiKey")]
	public string ApiKey = DefaultApiKey;

	[JsonProperty("databaseUrl")]
	public string DatabaseUrl = DefaultDatabaseUrl;

	[JsonProperty("deviceName")]
	public string DeviceName = Environment.MachineName;

	[JsonProperty("uid")]
	public string Uid = "";

	[JsonProperty("refreshToken")]
	public string RefreshToken = "";

	[JsonIgnore]
	public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(DatabaseUrl);

	[JsonIgnore]
	public string BaseUrl => DatabaseUrl.Trim().TrimEnd('/');

	public static string FullPath => Path.GetFullPath(FileName);

	public static RemoteConfig Load()
	{
		try
		{
			if (File.Exists(FileName))
			{
				RemoteConfig config = JsonConvert.DeserializeObject<RemoteConfig>(File.ReadAllText(FileName));
				if (config != null)
				{
					// Empty fields fall back to the built-in project, so a fresh install only needs "Enable Remote".
					if (string.IsNullOrWhiteSpace(config.ApiKey))
					{
						config.ApiKey = DefaultApiKey;
					}
					if (string.IsNullOrWhiteSpace(config.DatabaseUrl))
					{
						config.DatabaseUrl = DefaultDatabaseUrl;
					}
					return config;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Could not read " + FileName + ": " + ex.Message);
		}
		RemoteConfig created = new RemoteConfig();
		created.Save();
		return created;
	}

	public void Save()
	{
		try
		{
			File.WriteAllText(FileName, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[LethalMenu] Could not write " + FileName + ": " + ex.Message);
		}
	}
}
