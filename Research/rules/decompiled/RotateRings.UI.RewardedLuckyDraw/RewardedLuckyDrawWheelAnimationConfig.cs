using System;
using UnityEngine;

namespace RotateRings.UI.RewardedLuckyDraw;

[CreateAssetMenu(fileName = "RewardedLuckyDrawWheelAnimationConfig", menuName = "Config/RewardedLuckyDrawWheelAnimation")]
public sealed class RewardedLuckyDrawWheelAnimationConfig : ScriptableObject
{
	public const string ResourceName = "RewardedLuckyDrawWheelAnimationConfig";

	[SerializeField]
	[InspectorName("转盘动画参数")]
	private RewardedLuckyDrawWheelAnimationSettings settings = RewardedLuckyDrawWheelAnimationSettings.Default;

	public RewardedLuckyDrawWheelAnimationSettings Settings
	{
		get
		{
			if (!settings.IsValid)
			{
				throw new InvalidOperationException("转盘SO参数非法，请检查各阶段时长、圈数和动画曲线");
			}
			return settings;
		}
	}
}
