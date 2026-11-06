using UnityEngine;
using XingYun.Level;

namespace Game.Level;

public class RandomLevelSampler : IDifficultySamplerStrategy
{
	private const string TAG = "[RL][Sampler] ";

	public const int TierCount = 8;

	public const int FallbackTier = 4;

	public const string DATA_KEY_MATCHED_CSV = "matched_csv";

	public float Sample(LevelContext ctx)
	{
		int num = ResolveBaseTier(ctx);
		int num2 = ResolveOffset(ctx);
		int num3 = Mathf.Clamp(num + num2, 1, 8);
		if (ctx != null)
		{
			ctx.Data["Tier_Fixed"] = num3.ToString("D2");
			ctx.SampledDifficulty = num3;
		}
		return num3;
	}

	private static int ResolveBaseTier(LevelContext ctx)
	{
		string key = "DAWinLose_Fixed";
		if (ctx != null && ctx.Data.TryGetValue(key, out var value) && value != null && int.TryParse(value.ToString(), out var result))
		{
			return Mathf.Clamp(result, 1, 8);
		}
		return 4;
	}

	private static int ResolveOffset(LevelContext ctx)
	{
		string key = "DAWinLose_Offset";
		if (ctx != null && ctx.Data.TryGetValue(key, out var value) && value != null && int.TryParse(value.ToString(), out var result))
		{
			return result;
		}
		return 0;
	}
}
