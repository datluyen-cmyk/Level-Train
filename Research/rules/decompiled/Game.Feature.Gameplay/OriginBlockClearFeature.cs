using System.Collections.Generic;
using Game.Feature.Demo;
using UnityEngine;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class OriginBlockClearFeature : BaseFeature, IHookOnLevelStart, IHookOnMatchMap
{
	private HashSet<Vector2Int> originBlocks;

	private bool isDailyChallenge;

	public override string Name => "初始盘面清除";

	public override int Priority => 100;

	public override bool IsActive => !isDailyChallenge;

	public override bool CheckEnabled()
	{
		return true;
	}

	public FeatureResult OnLevelStart(DemoLevelData data)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		isDailyChallenge = data.IsDailyChallenge;
		originBlocks = new HashSet<Vector2Int>();
		if (data.MapData != null)
		{
			for (int i = 0; i < data.MapData.GetLength(0); i++)
			{
				for (int j = 0; j < data.MapData.GetLength(1); j++)
				{
					if (data.MapData[i, j] > 0)
					{
						originBlocks.Add(new Vector2Int(i, j));
					}
				}
			}
		}
		return FeatureResult.None;
	}

	public FeatureResult OnMatchMap(DemoMatchData data)
	{
		if (originBlocks == null || originBlocks.Count == 0)
		{
			return FeatureResult.None;
		}
		if (data.ClearedCount >= data.TotalCount)
		{
			originBlocks.Clear();
			return FeatureResult.Done;
		}
		return FeatureResult.None;
	}
}
