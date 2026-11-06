using System;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

[Serializable]
public class ScreenShakePresetEntry
{
	[Tooltip("触发类型（同一资产内不应重复）")]
	public ScreenShakeTriggerType TriggerType;

	[Tooltip("该触发对应的震屏参数")]
	public ScreenShakePreset Preset = new ScreenShakePreset();
}
