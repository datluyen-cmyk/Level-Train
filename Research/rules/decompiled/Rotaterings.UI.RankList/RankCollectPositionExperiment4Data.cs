using UnityEngine;

namespace Rotaterings.UI.RankList;

public static class RankCollectPositionExperiment4Data
{
	public static RankCollectPairPosition ItemLeft { get; } = new RankCollectPairPosition(new Vector2(-80f, 3.5f), new Vector2(20f, 0f));

	public static RankCollectPairPosition ItemRight { get; } = new RankCollectPairPosition(new Vector2(80f, 3.5f), new Vector2(-20f, 0f));

	public static RankCollectPairPosition TopHeaderLeft { get; } = new RankCollectPairPosition(new Vector2(-80f, 4.5f), new Vector2(20f, 0f));

	public static RankCollectPairPosition TopHeaderRight { get; } = new RankCollectPairPosition(new Vector2(80f, 4.5f), new Vector2(-20f, 0f));

	public static RankCollectPairPosition Select(bool topHeader, int side)
	{
		if (!topHeader)
		{
			if (side != 1)
			{
				return ItemLeft;
			}
			return ItemRight;
		}
		if (side != 1)
		{
			return TopHeaderLeft;
		}
		return TopHeaderRight;
	}

	public static RankCollectPairPosition SelectHalloween(int side)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (side != 1)
		{
			return new RankCollectPairPosition(new Vector2(-85f, 0f), new Vector2(20f, 0f));
		}
		return new RankCollectPairPosition(new Vector2(85f, 0f), new Vector2(-20f, 0f));
	}
}
