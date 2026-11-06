using System;
using XingYun.UI.Core;

namespace RotateRings.UI.Rating;

public static class RatingPopupService
{
	private static Action postSettlementCompleted;

	private static bool hasPreparedSettlement;

	private static bool shouldShowPreparedSettlement;

	public static bool PrepareForSettlement()
	{
		RatingProxy ratingProxy = GameApp.Proxy.Get<RatingProxy>();
		hasPreparedSettlement = true;
		shouldShowPreparedSettlement = ratingProxy?.ShouldShowRating() ?? false;
		return shouldShowPreparedSettlement;
	}

	public static bool ShouldShowPostSettlement()
	{
		if (hasPreparedSettlement)
		{
			hasPreparedSettlement = false;
			return shouldShowPreparedSettlement;
		}
		return GameApp.Proxy.Get<RatingProxy>()?.ShouldShowRating() ?? false;
	}

	public static void ForceShow()
	{
		ResetPostSettlementFlow();
		if (CanOpenRatingPage())
		{
			GameApp.UI.Open("UIRating", (UILayer)4, (object)null, (Action)null);
		}
	}

	public static void ShowPostSettlement(Action onCompleted)
	{
		if (!CanOpenRatingPage())
		{
			onCompleted?.Invoke();
			return;
		}
		postSettlementCompleted = onCompleted;
		GameApp.UI.Open("UIRating", (UILayer)4, (object)null, (Action)null);
	}

	private static bool CanOpenRatingPage()
	{
		return GameApp.Proxy.Get<RatingProxy>()?.IsNetworkAvailable() ?? false;
	}

	public static void CompletePostSettlementFlow()
	{
		Action action = postSettlementCompleted;
		postSettlementCompleted = null;
		action?.Invoke();
	}

	public static void ResetPostSettlementFlow()
	{
		postSettlementCompleted = null;
		hasPreparedSettlement = false;
		shouldShowPreparedSettlement = false;
	}
}
