namespace LethalMenu.Util;

public static class ReflectorExtensions
{
	public static ReflectionUtil<R> Reflect<R>(this R obj)
	{
		return new ReflectionUtil<R>(obj);
	}
}
