namespace RotateRings.UI.Rating;

public readonly struct RatingEligibility
{
	public bool ExperimentEnabled { get; init; }

	public bool LevelQualified { get; init; }

	public bool DurationQualified { get; init; }

	public bool NetworkAvailable { get; init; }

	public bool HighStarAvailable { get; init; }

	public bool PopupLimitQualified { get; init; }

	public bool CloseLimitQualified { get; init; }

	public bool LowStarSubmitLimitQualified { get; init; }

	public bool ActiveDayQualified { get; init; }

	public bool IsEligible
	{
		get
		{
			if (ExperimentEnabled && LevelQualified && DurationQualified && NetworkAvailable && HighStarAvailable && PopupLimitQualified && CloseLimitQualified && LowStarSubmitLimitQualified)
			{
				return ActiveDayQualified;
			}
			return false;
		}
	}

	public static RatingEligibility Evaluate(RatingRuntimeSnapshot snapshot)
	{
		bool activeDayQualified = snapshot.RequiredActiveDayInterval == 0 || snapshot.ActiveDays - snapshot.LastActionActiveDays > snapshot.RequiredActiveDayInterval;
		return new RatingEligibility
		{
			ExperimentEnabled = snapshot.ExperimentEnabled,
			LevelQualified = (snapshot.CompletedLevel >= RatingRuntimeRules.RequiredCompletedLevel),
			DurationQualified = (snapshot.ValidDurationSeconds > RatingRuntimeRules.RequiredValidDurationSeconds),
			NetworkAvailable = snapshot.NetworkAvailable,
			HighStarAvailable = !snapshot.HasGivenHighStar,
			PopupLimitQualified = (snapshot.PopupCount < RatingRuntimeRules.MaxPopupCount),
			CloseLimitQualified = (snapshot.CloseCount < RatingRuntimeRules.MaxCloseCount),
			LowStarSubmitLimitQualified = (snapshot.LowStarSubmitCount < RatingRuntimeRules.MaxLowStarSubmitCount),
			ActiveDayQualified = activeDayQualified
		};
	}

	public string GetFirstBlockReason()
	{
		if (!ExperimentEnabled)
		{
			return "实验开关关闭";
		}
		if (!LevelQualified)
		{
			return "完成关卡不足";
		}
		if (!DurationQualified)
		{
			return "有效时长不足";
		}
		if (!NetworkAvailable)
		{
			return "网络不可用";
		}
		if (!HighStarAvailable)
		{
			return "已提交高星评分";
		}
		if (!PopupLimitQualified)
		{
			return "已达到总弹窗上限";
		}
		if (!CloseLimitQualified)
		{
			return "已达到关闭上限";
		}
		if (!LowStarSubmitLimitQualified)
		{
			return "已达到低星提交上限";
		}
		if (!ActiveDayQualified)
		{
			return "活跃日间隔不足";
		}
		return "无";
	}
}
