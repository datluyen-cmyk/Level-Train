using System;
using System.Collections.Generic;
using UnityEngine;
using XingYun.Utility;

namespace RotateRings.GamePlay.Services;

[CreateAssetMenu(fileName = "ScreenShakeConfig", menuName = "Config/ScreenShakeConfig")]
public sealed class ScreenShakeConfigAsset : ScriptableObject
{
	private const string TAG = "[ScreenShakeConfig] ";

	private const string ResName = "ScreenShakeConfig";

	private static ScreenShakeConfigAsset instance;

	[Tooltip("全部触发类型与预设的映射条目；同一触发类型不应重复出现")]
	[SerializeField]
	private List<ScreenShakePresetEntry> entries = new List<ScreenShakePresetEntry>();

	public static ScreenShakeConfigAsset Instance
	{
		get
		{
			if ((Object)(object)instance != (Object)null)
			{
				return instance;
			}
			try
			{
				instance = GameApp.Res.LoadAsset<ScreenShakeConfigAsset>(Res.GetDefaultResPath("ScreenShakeConfig"), (string)null);
			}
			catch (Exception)
			{
				instance = CreateDefaultInstance();
			}
			return instance;
		}
	}

	public static void Reload()
	{
		instance = null;
	}

	public Dictionary<ScreenShakeTriggerType, ScreenShakePreset> BuildLookup()
	{
		Dictionary<ScreenShakeTriggerType, ScreenShakePreset> dictionary = new Dictionary<ScreenShakeTriggerType, ScreenShakePreset>();
		if (entries == null)
		{
			return dictionary;
		}
		for (int i = 0; i < entries.Count; i++)
		{
			ScreenShakePresetEntry screenShakePresetEntry = entries[i];
			if (screenShakePresetEntry != null && screenShakePresetEntry.Preset != null)
			{
				dictionary.ContainsKey(screenShakePresetEntry.TriggerType);
				dictionary[screenShakePresetEntry.TriggerType] = screenShakePresetEntry.Preset;
			}
		}
		return dictionary;
	}

	private static ScreenShakeConfigAsset CreateDefaultInstance()
	{
		ScreenShakeConfigAsset screenShakeConfigAsset = ScriptableObject.CreateInstance<ScreenShakeConfigAsset>();
		screenShakeConfigAsset.entries = new List<ScreenShakePresetEntry>
		{
			new ScreenShakePresetEntry
			{
				TriggerType = ScreenShakeTriggerType.BombExplosion,
				Preset = new ScreenShakePreset
				{
					Priority = 3,
					Delay = 0.55f,
					Duration = 0.35f,
					Amplitude = 0.15f,
					Vibrato = 10,
					FadeOut = true,
					PositionRandomness = 30f
				}
			},
			new ScreenShakePresetEntry
			{
				TriggerType = ScreenShakeTriggerType.HammerHit,
				Preset = new ScreenShakePreset
				{
					Priority = 2,
					Delay = 0.1f,
					Duration = 0.25f,
					Amplitude = 0.12f,
					Vibrato = 8,
					FadeOut = true,
					PositionRandomness = 20f
				}
			},
			new ScreenShakePresetEntry
			{
				TriggerType = ScreenShakeTriggerType.RocketHit,
				Preset = new ScreenShakePreset
				{
					Priority = 2,
					Delay = 0.1f,
					Duration = 0.25f,
					Amplitude = 0.12f,
					Vibrato = 8,
					FadeOut = true,
					PositionRandomness = 20f
				}
			}
		};
		return screenShakeConfigAsset;
	}
}
