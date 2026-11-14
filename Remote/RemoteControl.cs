using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LethalMenu.Remote;

internal enum RemoteStatus
{
	Disabled,
	NotConfigured,
	Connecting,
	WaitingForApproval,
	Connected,
	Error
}

/// <summary>
/// Connects the menu to the Firebase Realtime Database so it can be driven from the website.
///
/// Database layout (see RemotePanel/database.rules.json):
///   pairCodes/{CODE}              {device, name, at}  shown in the menu; typed into the website's "Add PC"
///   devices/{deviceUid}/owner     {uid, username, code}  the website account this PC belongs to
///   devices/{deviceUid}/info      {name, version, online, lastSeen}
///   devices/{deviceUid}/catalog   every hack/setting the website can show
///   devices/{deviceUid}/state     live toggles, settings, players, enemies, game info
///   devices/{deviceUid}/commands/{id}  queued by the website, executed + deleted here
///   devices/{deviceUid}/lastResult     outcome of the last command
///   devices/{deviceUid}/map       floor outline of the current moon, inside and outside (see RemoteMap)
///
/// Network work happens on background threads; commands run on the Unity main thread via <see cref="Tick"/>.
/// </summary>
internal static class RemoteControl
{
	private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

	private static readonly object ServerTimestamp = new Dictionary<string, string> { { ".sv", "timestamp" } };

	private static readonly ConcurrentQueue<KeyValuePair<string, JObject>> Inbox = new ConcurrentQueue<KeyValuePair<string, JObject>>();

	private static readonly ConcurrentQueue<JObject> Results = new ConcurrentQueue<JObject>();

	private static readonly ConcurrentQueue<string> StaleCommands = new ConcurrentQueue<string>();

	private static readonly HashSet<string> SeenCommands = new HashSet<string>();

	private static RemoteConfig config;

	private static FirebaseClient client;

	private static CancellationTokenSource cancel;

	private static volatile string latestState;

	private static float nextStateAt;

	public static RemoteStatus Status { get; private set; } = RemoteStatus.Disabled;

	public static string Message { get; private set; } = "";

	public static string PairCode { get; private set; }

	/// <summary>Username of the website account this PC is paired to (once connected).</summary>
	public static string OwnerName { get; private set; }

	public static RemoteConfig Config => config ??= RemoteConfig.Load();

	public static string StatusText => Status switch
	{
		RemoteStatus.Disabled => "Off",
		RemoteStatus.NotConfigured => "Not set up: fill apiKey and databaseUrl in " + RemoteConfig.FileName,
		RemoteStatus.Connecting => "Connecting...",
		RemoteStatus.WaitingForApproval => "Not paired yet: on the website press Add PC and enter code " + PairCode,
		RemoteStatus.Connected => "Connected" + (string.IsNullOrEmpty(OwnerName) ? "" : " to " + OwnerName),
		RemoteStatus.Error => "Error: " + Message + " (retrying)",
		_ => Status.ToString()
	};

	public static void Start()
	{
		Stop();
		config = RemoteConfig.Load();
		if (!config.Enabled)
		{
			Status = RemoteStatus.Disabled;
			return;
		}
		if (!config.IsConfigured)
		{
			Status = RemoteStatus.NotConfigured;
			return;
		}
		client = new FirebaseClient(config);
		cancel = new CancellationTokenSource();
		Status = RemoteStatus.Connecting;
		CancellationToken token = cancel.Token;
		new Thread(() => WorkerLoop(token))
		{
			IsBackground = true,
			Name = "LethalMenu.Remote"
		}.Start();
	}

	public static void Stop()
	{
		if (cancel != null)
		{
			cancel.Cancel();
			cancel = null;
			FirebaseClient last = client;
			if (last != null && Status == RemoteStatus.Connected)
			{
				// Best effort, off the main thread: tell the website we are gone.
				new Thread(() =>
				{
					try
					{
						last.Patch("devices/" + last.Uid + "/info", new Dictionary<string, object>
						{
							{ "online", false },
							{ "lastSeen", ServerTimestamp }
						});
					}
					catch
					{
					}
				})
				{
					IsBackground = true
				}.Start();
			}
		}
		client = null;
		PairCode = null;
		Status = RemoteStatus.Disabled;
		while (Inbox.TryDequeue(out _))
		{
		}
		while (Results.TryDequeue(out _))
		{
		}
		while (StaleCommands.TryDequeue(out _))
		{
		}
		lock (SeenCommands)
		{
			SeenCommands.Clear();
		}
		latestState = null;
	}

	public static void SetEnabled(bool enabled)
	{
		Config.Enabled = enabled;
		Config.Save();
		if (enabled)
		{
			Start();
		}
		else
		{
			Stop();
		}
	}

	/// <summary>Drops this PC's Firebase identity and pairs again from scratch.</summary>
	public static void ResetPairing()
	{
		Stop();
		RemoteConfig c = RemoteConfig.Load();
		c.Uid = "";
		c.RefreshToken = "";
		c.Save();
		Start();
	}

	/// <summary>Main thread, every frame: runs queued commands and snapshots the state for upload.</summary>
	private static float nextCatalogAt;

	private static volatile string latestCatalog;

	/// <summary>The database refuses result messages over 2000 characters.</summary>
	private static string Clip(string message)
	{
		message = message ?? "";
		return message.Length <= 1000 ? message : message.Substring(0, 1000) + "...";
	}

	public static void Tick()
	{
		while (Inbox.TryDequeue(out var item))
		{
			JObject result = new JObject
			{
				["id"] = item.Key,
				["at"] = JObject.FromObject(ServerTimestamp)
			};
			try
			{
				result["ok"] = true;
				result["message"] = Clip(RemoteCommands.Execute(item.Value));
			}
			catch (Exception ex)
			{
				Exception inner = (ex is TargetInvocationException && ex.InnerException != null) ? ex.InnerException : ex;
				result["ok"] = false;
				result["message"] = Clip(inner.Message);
				Debug.LogWarning("[LethalMenu] Remote command failed: " + inner);
			}
			Results.Enqueue(result);
			nextStateAt = 0f; // push the new state right away
		}
		// The feature list includes choices that only exist once a lobby is loaded (enemy types, items, moons),
		// so it is rebuilt here on the main thread now and then; the network thread sends it only if it changed.
		if (Status == RemoteStatus.Connected && Time.unscaledTime >= nextCatalogAt)
		{
			nextCatalogAt = Time.unscaledTime + 5f;
			try
			{
				latestCatalog = RemoteCatalog.BuildCatalog(live: true).ToString(Formatting.None);
			}
			catch (Exception ex)
			{
				Settings.debugMessage = "Remote catalog: " + ex.Message;
			}
		}
		if (Status == RemoteStatus.Connected)
		{
			RemoteMap.Tick();
		}
		if (Status == RemoteStatus.Connected && Time.unscaledTime >= nextStateAt)
		{
			nextStateAt = Time.unscaledTime + 0.5f;
			try
			{
				latestState = RemoteCatalog.BuildState().ToString(Formatting.None);
			}
			catch (Exception ex)
			{
				Settings.debugMessage = "Remote state: " + ex.Message;
			}
		}
	}

	private static void WorkerLoop(CancellationToken token)
	{
		FirebaseClient firebase = client;
		while (!token.IsCancellationRequested)
		{
			CancellationTokenSource session = CancellationTokenSource.CreateLinkedTokenSource(token);
			try
			{
				Status = RemoteStatus.Connecting;
				_ = firebase.Token;
				DeleteLeftoverCode(firebase, token);
				string device = "devices/" + firebase.Uid;
				WaitForApproval(firebase, token);
				if (token.IsCancellationRequested)
				{
					return;
				}
				firebase.Put(device + "/info", new Dictionary<string, object>
				{
					{ "name", config.DeviceName },
					{ "version", Settings.version },
					{ "online", true },
					{ "lastSeen", ServerTimestamp }
				});
				firebase.PutRaw(device + "/catalog", RemoteCatalog.BuildCatalog().ToString(Formatting.None));
				Status = RemoteStatus.Connected;
				Message = "";
				Debug.Log("[LethalMenu] Remote panel connected as " + firebase.Uid);
				StartCommandStream(firebase, device, session.Token);
				RunConnected(firebase, device, session.Token);
			}
			catch (Exception ex) when (!token.IsCancellationRequested)
			{
				Status = RemoteStatus.Error;
				Message = (ex is FirebaseException fe && fe.Status == System.Net.HttpStatusCode.Unauthorized) ? "Permission denied: check the database rules are deployed" : ex.Message;
				Debug.LogWarning("[LethalMenu] Remote panel: " + ex.Message);
				Sleep(5000, token);
			}
			catch
			{
				return;
			}
			finally
			{
				session.Cancel();
				session.Dispose();
			}
		}
	}

	/// <summary>Codes expire on the website side after 10 minutes; make a fresh one a bit before that.</summary>
	private const double CodeLifetimeSeconds = 540.0;

	/// <summary>
	/// A code left in the database by a session that crashed or was closed while showing it. Codes stop working
	/// after 10 minutes anyway; this just removes the leftover. Only this PC's own codes can be deleted.
	/// </summary>
	private static void DeleteLeftoverCode(FirebaseClient firebase, CancellationToken token)
	{
		RemoteConfig cfg = config;
		if (cfg == null || string.IsNullOrEmpty(cfg.PendingCode))
		{
			return;
		}
		TryDelete(firebase, "pairCodes/" + cfg.PendingCode);
		RememberCode(cfg, "", token);
	}

	/// <summary>Saves which code is in the database. Skipped once stopping: the settings file may already
	/// belong to a new pairing (Reset Pairing), and an old copy must not be written over it.</summary>
	private static void RememberCode(RemoteConfig cfg, string code, CancellationToken token)
	{
		if (cfg == null || token.IsCancellationRequested || cfg.PendingCode == code)
		{
			return;
		}
		cfg.PendingCode = code;
		cfg.Save();
	}

	private static void WaitForApproval(FirebaseClient firebase, CancellationToken token)
	{
		if (IsApproved(firebase))
		{
			return;
		}
		Status = RemoteStatus.WaitingForApproval;
		RemoteConfig cfg = config;
		string code = null;
		DateTime codeMadeUtc = DateTime.MinValue;
		try
		{
			while (!token.IsCancellationRequested)
			{
				if (code == null || (DateTime.UtcNow - codeMadeUtc).TotalSeconds > CodeLifetimeSeconds)
				{
					if (code != null)
					{
						TryDelete(firebase, "pairCodes/" + code);
					}
					code = PublishNewCode(firebase);
					RememberCode(cfg, code, token);
					codeMadeUtc = DateTime.UtcNow;
					PairCode = code;
					Debug.Log("[LethalMenu] Remote panel: on the website press Add PC and enter code " + code);
				}
				Sleep(3000, token);
				if (IsApproved(firebase))
				{
					break;
				}
			}
		}
		finally
		{
			if (code != null)
			{
				TryDelete(firebase, "pairCodes/" + code);
				RememberCode(cfg, "", token);
			}
			PairCode = null;
		}
	}

	/// <summary>Writes a new random code; retries if that code happens to be taken.</summary>
	private static string PublishNewCode(FirebaseClient firebase)
	{
		for (int attempt = 0; attempt < 5; attempt++)
		{
			string code = NewCode();
			try
			{
				firebase.Put("pairCodes/" + code, new Dictionary<string, object>
				{
					{ "device", firebase.Uid },
					{ "name", config.DeviceName.Length > 64 ? config.DeviceName.Substring(0, 64) : config.DeviceName },
					{ "at", ServerTimestamp }
				});
				return code;
			}
			catch (FirebaseException ex) when (ex.Status == System.Net.HttpStatusCode.Unauthorized && attempt < 4)
			{
				// Someone else's code; pick another.
			}
		}
		throw new InvalidOperationException("Could not create a pairing code");
	}

	private static void TryDelete(FirebaseClient firebase, string path)
	{
		try
		{
			firebase.Delete(path);
		}
		catch
		{
		}
	}

	/// <summary>True once a website account has claimed this PC; remembers the account's username.</summary>
	private static bool IsApproved(FirebaseClient firebase)
	{
		JToken owner = firebase.Get("devices/" + firebase.Uid + "/owner");
		if (owner is JObject o && o["uid"] != null && o["uid"].Type == JTokenType.String)
		{
			OwnerName = (string)o["username"];
			return true;
		}
		OwnerName = null;
		return false;
	}

	private static void RunConnected(FirebaseClient firebase, string device, CancellationToken token)
	{
		string sentState = null;
		string sentCatalog = null;
		string sentMap = null;
		DateTime nextHeartbeat = DateTime.UtcNow.AddSeconds(10.0);
		DateTime nextApprovalCheck = DateTime.UtcNow.AddSeconds(30.0);
		while (!token.IsCancellationRequested)
		{
			while (StaleCommands.TryDequeue(out var stale))
			{
				firebase.Delete(device + "/commands/" + stale);
			}
			while (Results.TryDequeue(out var result))
			{
				firebase.Put(device + "/lastResult", result);
				firebase.Delete(device + "/commands/" + (string)result["id"]);
			}
			string catalog = latestCatalog;
			if (catalog != null && catalog != sentCatalog)
			{
				firebase.PutRaw(device + "/catalog", catalog);
				sentCatalog = catalog;
			}
			string state = latestState;
			if (state != null && state != sentState)
			{
				firebase.PutRaw(device + "/state", state);
				sentState = state;
			}
			string map = RemoteMap.Latest;
			if (map != null && map != sentMap)
			{
				sentMap = map;
				try
				{
					firebase.PutRaw(device + "/map", map);
				}
				catch (FirebaseException ex)
				{
					// Not fatal (e.g. database rules without the map yet): everything else keeps working.
					Debug.LogWarning("[LethalMenu] Remote panel: couldn't send the map: " + ex.Message);
				}
			}
			if (DateTime.UtcNow >= nextHeartbeat)
			{
				nextHeartbeat = DateTime.UtcNow.AddSeconds(10.0);
				firebase.Patch(device + "/info", new Dictionary<string, object>
				{
					{ "online", true },
					{ "lastSeen", ServerTimestamp }
				});
			}
			if (DateTime.UtcNow >= nextApprovalCheck)
			{
				nextApprovalCheck = DateTime.UtcNow.AddSeconds(30.0);
				if (!IsApproved(firebase))
				{
					throw new InvalidOperationException("This PC was removed from its account on the website; showing a new pairing code");
				}
			}
			Sleep(100, token);
		}
	}

	private static void StartCommandStream(FirebaseClient firebase, string device, CancellationToken token)
	{
		bool firstSnapshot = true;
		new Thread(() =>
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					firebase.Stream(device + "/commands", (string eventName, JToken payload) =>
					{
						if (payload is JObject evt)
						{
							string path = (string)evt["path"] ?? "/";
							JToken data = evt["data"];
							if (path == "/")
							{
								if (data is JObject all)
								{
									foreach (JProperty property in all.Properties())
									{
										// Commands already waiting when the game connects were sent while it was offline: drop them.
										Accept(property.Name, property.Value as JObject, firstSnapshot && eventName == "put");
									}
								}
								if (eventName == "put")
								{
									firstSnapshot = false;
								}
							}
							else
							{
								Accept(path.Trim('/').Split('/')[0], data as JObject, stale: false);
							}
						}
					}, token);
				}
				catch (Exception ex) when (!token.IsCancellationRequested)
				{
					Debug.LogWarning("[LethalMenu] Remote command stream: " + ex.Message);
				}
				Sleep(2000, token);
			}
		})
		{
			IsBackground = true,
			Name = "LethalMenu.RemoteStream"
		}.Start();
	}

	private static void Accept(string id, JObject command, bool stale)
	{
		if (string.IsNullOrEmpty(id) || command == null)
		{
			return;
		}
		lock (SeenCommands)
		{
			if (!SeenCommands.Add(id))
			{
				return;
			}
		}
		if (stale)
		{
			StaleCommands.Enqueue(id);
		}
		else
		{
			Inbox.Enqueue(new KeyValuePair<string, JObject>(id, command));
		}
	}

	/// <summary>
	/// 8 characters from 32 easy-to-read ones (no 0/O, 1/I): about a trillion possible codes, so guessing a
	/// live one in its 9 minutes is not realistic. Each code is deleted as soon as the PC is paired.
	/// </summary>
	private const int CodeLength = 8;

	private static string NewCode()
	{
		byte[] bytes = new byte[CodeLength];
		using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
		{
			rng.GetBytes(bytes);
		}
		char[] code = new char[CodeLength];
		for (int i = 0; i < code.Length; i++)
		{
			code[i] = CodeAlphabet[bytes[i] % CodeAlphabet.Length];
		}
		return new string(code);
	}

	private static void Sleep(int milliseconds, CancellationToken token)
	{
		token.WaitHandle.WaitOne(milliseconds);
	}
}
