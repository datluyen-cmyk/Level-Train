using System;
using System.Collections.Generic;
using RotateRings.Prop;
using UnityEngine;
using XingYun.Utility;

namespace RotateRings.Services;

[CreateAssetMenu(fileName = "PropIconConfig", menuName = "Config/PropIconConfig")]
public class PropIconConfig : ScriptableObject
{
	public const int DefaultStyleId = 1;

	private static PropIconConfig _instance;

	[Header("道具图标配置")]
	public List<PropIconEntry> entries = new List<PropIconEntry>();

	public static PropIconConfig Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_instance))
			{
				try
				{
					_instance = GameApp.Res.LoadAsset<PropIconConfig>(Res.GetDefaultResPath("PropIconConfig"), (string)null);
				}
				catch (Exception)
				{
				}
			}
			return _instance;
		}
	}

	public PropIconEntry FindEntry(EPropType propType)
	{
		if (entries == null)
		{
			return null;
		}
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].propType == propType)
			{
				return entries[i];
			}
		}
		return null;
	}
}
