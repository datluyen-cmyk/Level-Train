using Rotaterings.SO;
using Rotaterings.UI.RankList;
using UnityEngine;

namespace Rotaterings.Rank;

public static class RankConst
{
	public const int RankCooldownSeconds = 0;

	public const string DefaultPlayerName = "You";

	public static readonly Color BotNameColor = Color32.op_Implicit(new Color32((byte)80, (byte)80, (byte)80, byte.MaxValue));

	public static int CurrentRewardConfig
	{
		get
		{
			RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
			if (rankModelData == null || !rankModelData.HasActivityRecord)
			{
				return RankFunctionResolver.GetRankRewardConfig();
			}
			return rankModelData.RankRewardConfig;
		}
	}

	public static int CurrentRewardRankLimit => RankFunctionResolver.GetRewardRankLimitByConfig(CurrentRewardConfig);

	public static int MaxRankBot
	{
		get
		{
			RankRobotScoreConfig instance = RankRobotScoreConfig.Instance;
			if (!((Object)(object)instance != (Object)null) || instance.BOTS_NUM <= 0)
			{
				return 50;
			}
			return instance.BOTS_NUM;
		}
	}
}
