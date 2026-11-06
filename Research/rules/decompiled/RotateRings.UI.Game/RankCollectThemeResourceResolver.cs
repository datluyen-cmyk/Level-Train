using Rotaterings.Rank;

namespace RotateRings.UI.Game;

public static class RankCollectThemeResourceResolver
{
	private const string DefaultAcornItemLocation = "AcornItem";

	private const string HalloweenAcornItemLocation = "AcornItemHalloween";

	private const string DefaultCollectionLocation = "RankStarCollection";

	private const string HalloweenCollectionLocation = "RankStarCollectionHalloween";

	private const string DefaultLeftFlyEffectLocation = "CollectFlyEffectLeft";

	private const string HalloweenLeftFlyEffectLocation = "CollectFlyEffectLeftHalloween";

	private const string DefaultRightFlyEffectLocation = "CollectFlyEffectRight";

	private const string HalloweenRightFlyEffectLocation = "CollectFlyEffectRightHalloween";

	public static string GetAcornItemLocation()
	{
		if (!IsHalloweenRankTheme())
		{
			return "AcornItem";
		}
		return "AcornItemHalloween";
	}

	public static string GetCollectionLocation()
	{
		if (!IsHalloweenRankTheme())
		{
			return "RankStarCollection";
		}
		return "RankStarCollectionHalloween";
	}

	public static string GetFlyEffectLocation(bool isRightSide)
	{
		if (IsHalloweenRankTheme())
		{
			if (!isRightSide)
			{
				return "CollectFlyEffectLeftHalloween";
			}
			return "CollectFlyEffectRightHalloween";
		}
		if (!isRightSide)
		{
			return "CollectFlyEffectLeft";
		}
		return "CollectFlyEffectRight";
	}

	private static bool IsHalloweenRankTheme()
	{
		RankModelProxy rankModelProxy = GameApp.Proxy.Get<RankModelProxy>();
		if (rankModelProxy == null)
		{
			return false;
		}
		return rankModelProxy.Data?.IsHalloweenTheme == true;
	}
}
