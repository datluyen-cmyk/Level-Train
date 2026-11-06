using System;
using UnityEngine;

namespace RotateRings.UI.LevelEnd;

[Serializable]
public struct LevelWinAnimationSettings
{
	internal static readonly LevelWinAnimationStage[] Stages = (LevelWinAnimationStage[])Enum.GetValues(typeof(LevelWinAnimationStage));

	[InspectorName("整体入场")]
	public LevelWinAnimationTiming RootAppear;

	[InspectorName("松鼠庆祝")]
	public LevelWinAnimationTiming SquirrelOpen;

	[InspectorName("顶部栏出现")]
	public LevelWinAnimationTiming TopBar;

	[InspectorName("分数显示/滚动")]
	[Tooltip("底板只显示最终分并触发每日最佳分；实验流程才播放滚分。")]
	public LevelWinAnimationTiming ScoreRoll;

	[InspectorName("分数强调")]
	public LevelWinAnimationTiming ScoreEmphasis;

	[InspectorName("鼓励词显示")]
	public LevelWinAnimationTiming Encouragement;

	[InspectorName("IQ面板弹出")]
	public LevelWinAnimationTiming IqAppear;

	[InspectorName("宝箱区域弹出")]
	public LevelWinAnimationTiming ChestAppear;

	[InspectorName("IQ数值增长")]
	public LevelWinAnimationTiming IqGrowth;

	[InspectorName("宝箱进度增长")]
	public LevelWinAnimationTiming ChestGrowth;

	[InspectorName("下一关按钮出现")]
	public LevelWinAnimationTiming NextButton;

	[InspectorName("分数滚动时长（秒）")]
	[Min(0f)]
	public float ScoreRollDuration;

	[InspectorName("IQ数值增长时长（秒）")]
	[Min(0f)]
	public float IqGrowthDuration;

	[InspectorName("宝箱进度增长时长（秒）")]
	[Min(0f)]
	public float ChestGrowthDuration;

	[InspectorName("分数强调动画")]
	[Tooltip("可留空；未配置或无法播放时跳过动效，在该阶段的配置触发时点立即完成，后续参照与延迟仍生效。")]
	public AnimationClip ScoreEmphasisClip;

	[InspectorName("鼓励词入场动画")]
	[Tooltip("仅实验流程使用；配置后播放该片段并按实际时长通知结束，留空或无法播放时直接显示鼓励词。")]
	public AnimationClip EncouragementClip;

	[InspectorName("IQ面板弹出动画（预留）")]
	[Tooltip("允许暂时留空；留空时仅显示起始值，增长仍由IQ数值增长阶段触发。")]
	public AnimationClip IqAppearClip;

	internal static int StageCapacity
	{
		get
		{
			int num = 0;
			LevelWinAnimationStage[] stages = Stages;
			foreach (LevelWinAnimationStage val in stages)
			{
				num = Math.Max(num, (int)val);
			}
			return num + 1;
		}
	}

	public static LevelWinAnimationSettings Default => new LevelWinAnimationSettings
	{
		RootAppear = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready),
		SquirrelOpen = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready),
		TopBar = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready, LevelWinAnimationAnchor.Start, 0.05f),
		ScoreRoll = new LevelWinAnimationTiming(LevelWinAnimationStage.EntranceComplete),
		ScoreEmphasis = new LevelWinAnimationTiming(LevelWinAnimationStage.ScoreRoll),
		Encouragement = new LevelWinAnimationTiming(LevelWinAnimationStage.ScoreEmphasis),
		IqAppear = new LevelWinAnimationTiming(LevelWinAnimationStage.Encouragement),
		ChestAppear = new LevelWinAnimationTiming(LevelWinAnimationStage.IqAppear),
		IqGrowth = new LevelWinAnimationTiming(LevelWinAnimationStage.PanelsComplete),
		ChestGrowth = new LevelWinAnimationTiming(LevelWinAnimationStage.IqGrowth),
		NextButton = new LevelWinAnimationTiming(LevelWinAnimationStage.ChestGrowth, LevelWinAnimationAnchor.Start),
		ScoreRollDuration = 1f,
		IqGrowthDuration = 1f,
		ChestGrowthDuration = 0.4f
	};

	public static LevelWinAnimationSettings OriginalDefault => new LevelWinAnimationSettings
	{
		RootAppear = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready),
		SquirrelOpen = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready),
		TopBar = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready, LevelWinAnimationAnchor.Start, 0.05f),
		Encouragement = new LevelWinAnimationTiming(LevelWinAnimationStage.Ready, LevelWinAnimationAnchor.Start, 0.85f),
		ScoreRoll = new LevelWinAnimationTiming(LevelWinAnimationStage.Encouragement, LevelWinAnimationAnchor.Start),
		ScoreEmphasis = new LevelWinAnimationTiming(LevelWinAnimationStage.ScoreRoll),
		IqAppear = new LevelWinAnimationTiming(LevelWinAnimationStage.ScoreRoll),
		IqGrowth = new LevelWinAnimationTiming(LevelWinAnimationStage.IqAppear, LevelWinAnimationAnchor.Start),
		NextButton = new LevelWinAnimationTiming(LevelWinAnimationStage.Encouragement, LevelWinAnimationAnchor.Start, 0.25f),
		ChestAppear = new LevelWinAnimationTiming(LevelWinAnimationStage.NextButton, LevelWinAnimationAnchor.Start, 0.25f),
		ChestGrowth = new LevelWinAnimationTiming(LevelWinAnimationStage.ChestAppear, LevelWinAnimationAnchor.Start, 0.4f),
		ScoreRollDuration = 0f,
		IqGrowthDuration = 1f,
		ChestGrowthDuration = 0.4f
	};

	internal static bool IsJoin(LevelWinAnimationStage stage)
	{
		if (stage != LevelWinAnimationStage.EntranceComplete)
		{
			return stage == LevelWinAnimationStage.PanelsComplete;
		}
		return true;
	}

	internal LevelWinAnimationTiming GetTiming(LevelWinAnimationStage stage)
	{
		return stage switch
		{
			LevelWinAnimationStage.RootAppear => RootAppear, 
			LevelWinAnimationStage.SquirrelOpen => SquirrelOpen, 
			LevelWinAnimationStage.TopBar => TopBar, 
			LevelWinAnimationStage.ScoreRoll => ScoreRoll, 
			LevelWinAnimationStage.ScoreEmphasis => ScoreEmphasis, 
			LevelWinAnimationStage.Encouragement => Encouragement, 
			LevelWinAnimationStage.IqAppear => IqAppear, 
			LevelWinAnimationStage.ChestAppear => ChestAppear, 
			LevelWinAnimationStage.IqGrowth => IqGrowth, 
			LevelWinAnimationStage.ChestGrowth => ChestGrowth, 
			LevelWinAnimationStage.NextButton => NextButton, 
			_ => throw new ArgumentOutOfRangeException("stage"), 
		};
	}

	internal void Validate()
	{
		LevelWinAnimationStage[] stages = Stages;
		foreach (LevelWinAnimationStage levelWinAnimationStage in stages)
		{
			if (levelWinAnimationStage != LevelWinAnimationStage.Ready && !IsJoin(levelWinAnimationStage))
			{
				GetTiming(levelWinAnimationStage).Validate(levelWinAnimationStage);
			}
		}
		if (!ValidDuration(ScoreRollDuration) || !ValidDuration(IqGrowthDuration) || !ValidDuration(ChestGrowthDuration))
		{
			throw new InvalidOperationException("结算动画时长必须为0以上的有限秒数");
		}
		byte[] marks = new byte[StageCapacity];
		stages = Stages;
		foreach (LevelWinAnimationStage stage in stages)
		{
			Visit(stage, marks);
		}
	}

	private void Visit(LevelWinAnimationStage stage, byte[] marks)
	{
		if (stage != LevelWinAnimationStage.Ready && marks[(int)stage] != 2)
		{
			if (marks[(int)stage] == 1)
			{
				throw new InvalidOperationException($"结算动画参照形成循环：{stage}");
			}
			marks[(int)stage] = 1;
			switch (stage)
			{
			case LevelWinAnimationStage.EntranceComplete:
				Visit(LevelWinAnimationStage.RootAppear, marks);
				Visit(LevelWinAnimationStage.SquirrelOpen, marks);
				break;
			case LevelWinAnimationStage.PanelsComplete:
				Visit(LevelWinAnimationStage.IqAppear, marks);
				Visit(LevelWinAnimationStage.ChestAppear, marks);
				break;
			default:
				Visit(GetTiming(stage).Reference, marks);
				break;
			}
			marks[(int)stage] = 2;
		}
	}

	private static bool ValidDuration(float value)
	{
		if (LevelWinAnimationTiming.IsFinite(value))
		{
			return value >= 0f;
		}
		return false;
	}
}
