using System;
using UnityEngine;
using XingYun.Utility;
using YooAsset;

namespace Basis.Common.UI;

public class UIPrefabLoader : UIResourceLoaderBase
{
	private const string PrefabRoot = "Assets/02_Res/Art/UI/Prefab/";

	private const string PrefabExtension = ".prefab";

	protected override string LogTag => "UIPrefabLoader";

	protected override void OnInstanceReady(GameObject readyInstance)
	{
	}

	protected override void OnLoadFailed()
	{
	}

	protected override string ResolvePath(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		if (name.StartsWith("Assets/02_Res/Art/UI/Prefab/", StringComparison.Ordinal) && IsRuntimeLocationValid(name))
		{
			return name;
		}
		if (IsRuntimeLocationValid(name))
		{
			return name;
		}
		string text = BuildDirectAssetPath(name);
		if (IsRuntimeLocationValid(text))
		{
			return text;
		}
		string defaultResPath = Res.GetDefaultResPath(ExtractFileName(name));
		if (IsRuntimeLocationValid(defaultResPath))
		{
			return defaultResPath;
		}
		return null;
	}

	private static string BuildDirectAssetPath(string name)
	{
		string text = (name.EndsWith(".prefab", StringComparison.Ordinal) ? string.Empty : ".prefab");
		return "Assets/02_Res/Art/UI/Prefab/" + name + text;
	}

	private static string ExtractFileName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		string text = name;
		if (text.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
		{
			text = text.Substring(0, text.Length - ".prefab".Length);
		}
		int num = text.LastIndexOf('/');
		if (num >= 0)
		{
			text = text.Substring(num + 1);
		}
		return text;
	}

	private static bool IsRuntimeLocationValid(string location)
	{
		if (!string.IsNullOrEmpty(location))
		{
			return YooAssets.CheckLocationValid(location);
		}
		return false;
	}
}
