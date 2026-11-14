using System.Collections.Generic;

namespace LethalMenu.Language;

public class Language
{
	private Dictionary<string, string> _language;

	public int Rank { get; set; }

	public string Name { get; private set; }

	public string Translator { get; private set; }

	public Language(string name, string translator, Dictionary<string, string> language)
	{
		Name = name;
		Translator = translator;
		_language = language;
	}

	public string Localize(string key)
	{
		return _language.ContainsKey(key) ? _language[key] : key;
	}

	public bool Has(string key)
	{
		return _language.ContainsKey(key);
	}

	public int Count()
	{
		return _language.Count;
	}
}
