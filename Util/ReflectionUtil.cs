using System;
using System.Reflection;

namespace LethalMenu.Util;

public class ReflectionUtil<R>
{
	private const BindingFlags privateInst = BindingFlags.Instance | BindingFlags.NonPublic;

	private const BindingFlags privateStatic = BindingFlags.Static | BindingFlags.NonPublic;

	private const BindingFlags privateField = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField;

	private const BindingFlags privateProp = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty;

	private const BindingFlags privateMethod = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

	private const BindingFlags staticField = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField;

	private const BindingFlags staticProp = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty;

	private const BindingFlags staticMethod = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;

	private R @object { get; }

	private Type type { get; }

	internal ReflectionUtil(R obj)
	{
		@object = obj;
		type = typeof(R);
	}

	private const BindingFlags anyMember = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

	private static readonly System.Collections.Generic.HashSet<string> warned = new System.Collections.Generic.HashSet<string>();

	// Game updates rename things and switch them between private and public. Look on the class and its parents,
	// public or not, and if the name is gone say so once in the log instead of crashing the feature.
	private static void Missing(string kind, string name)
	{
		if (warned.Add(typeof(R).Name + "." + name))
		{
			UnityEngine.Debug.LogWarning("[LethalMenu] " + kind + " '" + name + "' not found on " + typeof(R).Name + " (game update?)");
		}
	}

	private FieldInfo FindField(string name)
	{
		for (Type t = type; t != null; t = t.BaseType)
		{
			FieldInfo field = t.GetField(name, anyMember);
			if (field != null)
			{
				return field;
			}
		}
		Missing("Field", name);
		return null;
	}

	private PropertyInfo FindProperty(string name)
	{
		for (Type t = type; t != null; t = t.BaseType)
		{
			PropertyInfo property = t.GetProperty(name, anyMember);
			if (property != null)
			{
				return property;
			}
		}
		Missing("Property", name);
		return null;
	}

	private MethodInfo FindMethod(string name, object[] args)
	{
		for (Type t = type; t != null; t = t.BaseType)
		{
			foreach (MethodInfo method in t.GetMethods(anyMember))
			{
				if (method.Name == name && method.GetParameters().Length == (args?.Length ?? 0))
				{
					return method;
				}
			}
		}
		Missing("Method", name);
		return null;
	}

	private T? GetValue<T>(string variableName, BindingFlags flags)
	{
		try
		{
			FieldInfo field = FindField(variableName);
			return field == null ? default : (T)field.GetValue(field.IsStatic ? null : (object)@object);
		}
		catch (Exception)
		{
			return default;
		}
	}

	private T? GetProperty<T>(string propertyName, BindingFlags flags)
	{
		try
		{
			PropertyInfo property = FindProperty(propertyName);
			return property == null ? default : (T)property.GetValue(@object);
		}
		catch (Exception)
		{
			return default;
		}
	}

	private ReflectionUtil<R>? SetValue(string variableName, object value, BindingFlags flags)
	{
		try
		{
			FieldInfo field = FindField(variableName);
			if (field == null)
			{
				return null;
			}
			field.SetValue(field.IsStatic ? null : (object)@object, value);
			return this;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private ReflectionUtil<R>? SetProperty(string propertyName, object value, BindingFlags flags)
	{
		try
		{
			PropertyInfo property = FindProperty(propertyName);
			if (property == null)
			{
				return null;
			}
			property.SetValue(@object, value);
			return this;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private T? Invoke<T>(string methodName, BindingFlags flags, params object[] args)
	{
		MethodInfo method = FindMethod(methodName, args);
		if (method == null)
		{
			return default;
		}
		try
		{
			return (T)method.Invoke(method.IsStatic ? null : (object)@object, args);
		}
		catch (InvalidCastException)
		{
			return default;
		}
	}

	public T? GetValue<T>(string fieldName, bool isStatic = false, bool isProperty = false)
	{
		BindingFlags flags;
		if (isProperty)
		{
			flags = (isStatic ? (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty) : (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty));
		}
		else
		{
			flags = (isStatic ? (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField) : (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField));
		}
		return isProperty ? GetProperty<T>(fieldName, flags) : GetValue<T>(fieldName, flags);
	}

	public ReflectionUtil<R>? SetValue(string fieldName, object value, bool isStatic = false, bool isProperty = false)
	{
		BindingFlags flags;
		if (isProperty)
		{
			flags = (isStatic ? (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetProperty) : (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty));
		}
		else
		{
			flags = (isStatic ? (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField) : (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField));
		}
		return isProperty ? SetProperty(fieldName, value, flags) : SetValue(fieldName, value, flags);
	}

	private T? Invoke<T>(string methodName, bool isStatic = false, params object[] args)
	{
		return Invoke<T>(methodName, isStatic ? (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod) : (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod), args);
	}

	public object? GetValue(string fieldName, bool isStatic = false, bool isProperty = false)
	{
		// Was GetValue<R>: that tried to turn the value into the owner's own type, so it always came back empty.
		return GetValue<object>(fieldName, isStatic, isProperty);
	}

	public ReflectionUtil<R>? Invoke(string methodName, bool isStatic = false, params object[] args)
	{
		R val = Invoke<R>(methodName, isStatic, args);
		return (val != null) ? val.Reflect() : null;
	}

	public ReflectionUtil<R>? Invoke(string methodName, params object[] args)
	{
		R val = Invoke<R>(methodName, isStatic: false, args);
		return (val != null) ? val.Reflect() : null;
	}
}
