using System;
using RotateRings.Model;
using RotateRings.Tool;
using XingYun.Experiment;
using XingYun.Level;
using XingYun.Proxy;

namespace RotateRings.UI.Rating;

public class RatingProxy : BaseProxy
{
	private const string LogTag = "Rating";

	private readonly RatingData data;

	public RatingProxy()
		: base("RatingProxy")
	{
		data = GameApp.Save.GetOrCreate<RatingData>();
	}

	public RatingRuntimeSnapshot GetSnapshot()
	{
		return new RatingRuntimeSnapshot
		{
			ExperimentEnabled = IsExperimentEnabled(),
			CompletedLevel = GetCompletedLevel(),
			ValidDurationSeconds = GetValidLevelDurationSeconds(),
			ActiveDays = GetActiveDays(),
			NetworkAvailable = NetUtil.IsHaveNet,
			PopupCount = data.PopupCount,
			HasGivenHighStar = data.HasGivenHighStar,
			CloseCount = data.CloseCount,
			LowStarSubmitCount = data.SubmitLowStarCount,
			RequiredActiveDayInterval = GetRequiredActiveDayInterval(),
			LastActionActiveDays = data.LastActionActiveDays
		};
	}

	public bool ShouldShowRating()
	{
		return RatingEligibility.Evaluate(GetSnapshot()).IsEligible;
	}

	public bool IsNetworkAvailable()
	{
		return NetUtil.IsHaveNet;
	}

	public void RecordPopupShown()
	{
		data.PopupCount++;
		data.LastPopupPassTime = GetTotalValidDurationSeconds();
	}

	public void MarkHighStarGiven()
	{
		data.HasGivenHighStar = true;
	}

	public void RecordClose()
	{
		if (data.FirstPopupAction == 0)
		{
			data.FirstPopupAction = 1;
		}
		data.CloseCount++;
		data.LastActionType = 1;
		data.LastActionActiveDays = GetActiveDays();
	}

	public void RecordLowStarSubmit()
	{
		if (data.FirstPopupAction == 0)
		{
			data.FirstPopupAction = 2;
		}
		data.SubmitLowStarCount++;
		data.LastActionType = 2;
		data.LastActionActiveDays = GetActiveDays();
	}

	public void Reset()
	{
		data.OnCreateDefault();
	}

	public bool IsHighStar(int star)
	{
		return star >= RatingRuntimeRules.HighStarMin;
	}

	private static bool IsExperimentEnabled()
	{
		try
		{
			return FunctionDataCenter.Get<RatingFunction>()?.FunctionEnable ?? false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private int GetRequiredActiveDayInterval()
	{
		switch ((data.LastActionType != 0) ? data.LastActionType : data.FirstPopupAction)
		{
		case 0:
			return 0;
		case 1:
			return RatingRuntimeRules.GetCloseInterval(data.CloseCount - 1);
		case 2:
			if (data.SubmitLowStarCount == 1)
			{
				return RatingRuntimeRules.LowStarInterval;
			}
			goto default;
		default:
			return -1;
		}
	}

	private static int GetActiveDays()
	{
		try
		{
			PlayerProxy val = GameApp.Proxy.Get<PlayerProxy>();
			return Math.Max(1, (val == null) ? 1 : val.ActiveDays);
		}
		catch (Exception)
		{
			return 1;
		}
	}

	private static int GetCompletedLevel()
	{
		return Math.Max(0, (GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 1) - 1);
	}

	private float GetValidLevelDurationSeconds()
	{
		return GetTotalValidDurationSeconds() - data.LastPopupPassTime;
	}

	private static float GetTotalValidDurationSeconds()
	{
		GameTrackProxy obj = GameApp.Proxy.Get<GameTrackProxy>();
		if (obj == null)
		{
			return 0f;
		}
		return obj.GetPassTime("0");
	}
}
