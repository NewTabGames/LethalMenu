using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LethalMenu.Language;

public class Localization
{
	private static Dictionary<string, Language> _languages = new Dictionary<string, Language>();

	private static bool _initialized = false;

	public static Language Language { get; private set; }

	public static void Initialize()
	{
		if (!_initialized)
		{
			LoadLanguage();
			SetLanguage("English");
			_initialized = true;
		}
	}

	private static void LoadLanguage()
	{
		(from x in Assembly.GetExecutingAssembly().GetManifestResourceNames()
			where x.StartsWith("LethalMenu.Resources.Language.") && x.EndsWith(".json")
			select x).ToList().ForEach((string x) =>
		{
			string text = null;
			try
			{
				using (StreamReader streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(x)))
				{
					text = streamReader.ReadToEnd();
				}
				JObject jObject = JObject.Parse(text);
				Dictionary<string, string> localization = new Dictionary<string, string>();
				if (!jObject.ContainsKey("LANGUAGE") || !jObject.ContainsKey("TRANSLATOR"))
				{
					Debug.LogError("Failed to load localization file => " + x);
				}
				else
				{
					string text2 = jObject["LANGUAGE"].ToString();
					string text3 = jObject["TRANSLATOR"].ToString();
					jObject.Properties().ToList().ForEach((JProperty p) =>
					{
						string name = p.Name;
						string value = p.Value.ToString();
						localization.Add(name, value);
					});
					_languages.Add(text2, new Language(text2, text3, localization));
					Debug.Log("Loaded Language " + text2 + " by " + text3);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Json Error: Loading language file " + x + " because " + ex.Message);
				int num = 0;
				using (StringReader stringReader = new StringReader(text))
				{
					string text4;
					while ((text4 = stringReader.ReadLine()) != null)
					{
						num++;
						if (text4.Contains(ex.Message))
						{
							Debug.LogError($"Error occurred at line {num} in file {x}");
							break;
						}
					}
				}
				if (ex is JsonReaderException ex2)
				{
					Debug.LogError($"Json parsing error: Line {ex2.LineNumber}, Position {ex2.LinePosition} [{ex2.Path}]");
				}
			}
		});
		int englishCount = _languages["English"].Count();
		_languages.Values.ToList().ForEach((Language x) =>
		{
			if (x.Count() != englishCount)
			{
				double num = (double)x.Count() / (double)englishCount * 100.0;
				Debug.LogWarning($"Language {x.Name} is missing {englishCount - x.Count()} keys");
				if (num < 80.0)
				{
					_languages.Remove(x.Name);
					Debug.LogError(x.Name + " is too far behind. Unloading it.");
				}
			}
		});
	}

	public static string[] GetLanguages()
	{
		_languages.Values.ToList().ForEach((Language x) =>
		{
			x.Rank = ((Language == x) ? 1 : 999);
		});
		return (from x in _languages.Values
			orderby x.Rank, x.Name
			select x.Name).ToArray();
	}

	public static void SetLanguage(string name)
	{
		Language = (LanguageExists(name) ? _languages[name] : _languages["English"]);
	}

	public static bool LanguageExists(string name)
	{
		return _languages.ContainsKey(name);
	}

	public static string Localize(string key)
	{
		return Language.Has(key) ? Language.Localize(key) : LocalizeEnglish(key);
	}

	public static string[] LocalizeArray(string[] keys)
	{
		return keys.Select((string key) => Localize(key)).ToArray();
	}

	public static string Localize(string[] keys, bool newLine = false)
	{
		return keys.Aggregate("", (string current, string key) => current + (newLine ? "\n" : " ") + Localize(key)).Substring(1);
	}

	private static string LocalizeEnglish(string key)
	{
		return _languages["English"].Localize(key);
	}
}
