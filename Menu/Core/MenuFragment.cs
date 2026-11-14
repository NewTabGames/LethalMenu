using System;
using UnityEngine;
using UnityEngine.Networking;

namespace LethalMenu.Menu.Core;

internal class MenuFragment
{
	public void GetImage(string url, Action<Texture2D> Action)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		AsyncOperation asyncOperation = www.SendWebRequest();
		asyncOperation.completed += (AsyncOperation op) =>
		{
			if (www.result == UnityWebRequest.Result.Success)
			{
				Texture2D content = DownloadHandlerTexture.GetContent(www);
				Action?.Invoke(content);
			}
		};
	}
}
