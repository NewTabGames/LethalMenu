using UnityEngine;

namespace LethalMenu.Handler;

public static class ChamHandlerExtensions
{
	public static ChamHandler GetChamHandler(this Object obj)
	{
		return ChamHandler.GetHandler(obj);
	}
}
