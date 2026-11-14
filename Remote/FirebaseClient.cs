using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LethalMenu.Remote;

internal class FirebaseException : Exception
{
	public HttpStatusCode Status { get; }

	public FirebaseException(HttpStatusCode status, string message)
		: base("HTTP " + (int)status + ": " + message)
	{
		Status = status;
	}
}

/// <summary>
/// Minimal Firebase client over plain HTTPS (no Firebase SDK in the game):
/// anonymous Auth via the Identity Toolkit REST API, and Realtime Database REST + server-sent-event streaming.
/// All methods block, so call them from a worker thread only.
/// </summary>
internal class FirebaseClient
{
	private static readonly HttpClient Http = new HttpClient
	{
		Timeout = TimeSpan.FromSeconds(20)
	};

	private static readonly HttpClient StreamHttp = new HttpClient
	{
		Timeout = Timeout.InfiniteTimeSpan
	};

	private readonly RemoteConfig config;

	private readonly object authLock = new object();

	private string idToken;

	private DateTime tokenExpiresUtc = DateTime.MinValue;

	public FirebaseClient(RemoteConfig config)
	{
		this.config = config;
	}

	public string Uid => config.Uid;

	/// <summary>Quitting: aborts every request still open (above all the endless command stream).</summary>
	public static void CloseConnections()
	{
		try
		{
			StreamHttp.CancelPendingRequests();
			Http.CancelPendingRequests();
		}
		catch
		{
		}
	}

	/// <summary>Makes sure there is a valid ID token, refreshing or signing up anonymously as needed.</summary>
	public string Token
	{
		get
		{
			lock (authLock)
			{
				if (idToken != null && DateTime.UtcNow < tokenExpiresUtc.AddMinutes(-5.0))
				{
					return idToken;
				}
				if (!string.IsNullOrEmpty(config.RefreshToken))
				{
					try
					{
						Refresh();
						return idToken;
					}
					catch (FirebaseException ex) when (ex.Status == HttpStatusCode.BadRequest)
					{
						// Refresh token revoked or the user was deleted: start over with a new identity.
					}
				}
				SignUpAnonymously();
				return idToken;
			}
		}
	}

	/// <summary>Forgets the current identity; the next request signs up as a brand-new device.</summary>
	public void ResetIdentity()
	{
		lock (authLock)
		{
			idToken = null;
			tokenExpiresUtc = DateTime.MinValue;
			config.Uid = "";
			config.RefreshToken = "";
			config.Save();
		}
	}

	public void InvalidateToken()
	{
		lock (authLock)
		{
			idToken = null;
		}
	}

	private void SignUpAnonymously()
	{
		JObject response = PostJson("https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=" + Uri.EscapeDataString(config.ApiKey), new StringContent("{\"returnSecureToken\":true}", Encoding.UTF8, "application/json"));
		idToken = (string)response["idToken"];
		config.RefreshToken = (string)response["refreshToken"];
		config.Uid = (string)response["localId"];
		tokenExpiresUtc = DateTime.UtcNow.AddSeconds(ParseSeconds(response["expiresIn"]));
		config.Save();
	}

	private void Refresh()
	{
		FormUrlEncodedContent body = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			{ "grant_type", "refresh_token" },
			{ "refresh_token", config.RefreshToken }
		});
		JObject response = PostJson("https://securetoken.googleapis.com/v1/token?key=" + Uri.EscapeDataString(config.ApiKey), body);
		idToken = (string)response["id_token"];
		config.RefreshToken = (string)response["refresh_token"];
		string uid = (string)response["user_id"];
		if (!string.IsNullOrEmpty(uid) && uid != config.Uid)
		{
			config.Uid = uid;
		}
		tokenExpiresUtc = DateTime.UtcNow.AddSeconds(ParseSeconds(response["expires_in"]));
		config.Save();
	}

	private static double ParseSeconds(JToken token)
	{
		return (token != null && double.TryParse(token.ToString(), out var result)) ? result : 3600.0;
	}

	private static JObject PostJson(string url, HttpContent content)
	{
		using HttpResponseMessage response = Http.PostAsync(url, content).GetAwaiter().GetResult();
		string text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		if (!response.IsSuccessStatusCode)
		{
			throw new FirebaseException(response.StatusCode, ExtractError(text));
		}
		return JObject.Parse(text);
	}

	private static string ExtractError(string body)
	{
		try
		{
			JObject json = JObject.Parse(body);
			return (string)(json["error"]?["message"] ?? json["error"]) ?? body;
		}
		catch
		{
			return body;
		}
	}

	private string Url(string path)
	{
		return config.BaseUrl + "/" + path.Trim('/') + ".json?auth=" + Uri.EscapeDataString(Token);
	}

	public JToken Get(string path)
	{
		string text = Send(HttpMethod.Get, path, null);
		return string.IsNullOrEmpty(text) ? null : JToken.Parse(text);
	}

	public void Put(string path, object value)
	{
		Send(HttpMethod.Put, path, value);
	}

	public void PutRaw(string path, string json)
	{
		SendRaw(HttpMethod.Put, path, json);
	}

	public void Patch(string path, object value)
	{
		Send(new HttpMethod("PATCH"), path, value);
	}

	public void Delete(string path)
	{
		Send(HttpMethod.Delete, path, null);
	}

	private string Send(HttpMethod method, string path, object value)
	{
		return SendRaw(method, path, (value == null) ? null : JsonConvert.SerializeObject(value));
	}

	private string SendRaw(HttpMethod method, string path, string json)
	{
		using HttpRequestMessage request = new HttpRequestMessage(method, Url(path));
		if (json != null)
		{
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");
		}
		using HttpResponseMessage response = Http.SendAsync(request).GetAwaiter().GetResult();
		string text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			// Either the token expired early or the rules said no: let the caller decide, but drop the token.
			InvalidateToken();
		}
		if (!response.IsSuccessStatusCode)
		{
			throw new FirebaseException(response.StatusCode, ExtractError(text));
		}
		return text;
	}

	/// <summary>
	/// Streams changes under <paramref name="path"/> (Firebase REST streaming / server-sent events) until the
	/// server closes the stream, the token is revoked, or <paramref name="cancel"/> fires.
	/// </summary>
	public void Stream(string path, Action<string, JToken> onEvent, CancellationToken cancel)
	{
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Url(path));
		request.Headers.Accept.ParseAdd("text/event-stream");
		using HttpResponseMessage response = StreamHttp.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel).GetAwaiter().GetResult();
		if (!response.IsSuccessStatusCode)
		{
			string text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				InvalidateToken();
			}
			throw new FirebaseException(response.StatusCode, ExtractError(text));
		}
		using Stream stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
		using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
		using (cancel.Register(() => reader.Dispose()))
		{
			string eventName = null;
			StringBuilder data = new StringBuilder();
			while (!cancel.IsCancellationRequested)
			{
				string line;
				try
				{
					line = reader.ReadLine();
				}
				catch (ObjectDisposedException)
				{
					return;
				}
				if (line == null)
				{
					return;
				}
				if (line.Length == 0)
				{
					if (eventName != null)
					{
						string payload = data.ToString();
						if (eventName == "auth_revoked" || eventName == "cancel")
						{
							InvalidateToken();
							return;
						}
						if (eventName == "put" || eventName == "patch")
						{
							onEvent(eventName, string.IsNullOrEmpty(payload) || payload == "null" ? null : JToken.Parse(payload));
						}
					}
					eventName = null;
					data.Clear();
				}
				else if (line.StartsWith("event:"))
				{
					eventName = line.Substring(6).Trim();
				}
				else if (line.StartsWith("data:"))
				{
					data.Append(line.Substring(5).Trim());
				}
			}
		}
	}
}
