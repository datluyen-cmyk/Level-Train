using System;
using Game.Experiment.Strategy;
using UnityEngine;
using XingYun.Ad;
using XingYun.BT;
using XingYun.Experiment;

namespace Game.Experiment.Extensions;

public static class AdInterExtensions
{
	private static BTStrategy tree;

	private static AdInterStrategyNode strategy;

	private static float lastShowTime;

	public static void InitInterExtend(this AdManager adManager)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		adManager.Init();
		strategy = AdInterStrategyFactory.Create(FunctionDataCenter.Get<InsertAdFunction>().StrategyId);
		tree = new BTStrategy();
		tree.AddNode((BTBaseNode)(object)strategy);
	}

	public static void ShowInterExtend(this AdManager adManager, IAdConfiguration config, Action<InterstitialAdResult> callback)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		AdInterContext context = new AdInterContext
		{
			LevelCount = 1,
			TimeSinceLast = Time.time - lastShowTime
		};
		strategy.SetContext(context);
		strategy.Reset();
		tree.Execute();
		AdInterDecision result = strategy.GetResult();
		if (!result.CanShow)
		{
			callback?.Invoke(new InterstitialAdResult(false, result.BlockReason));
			return;
		}
		adManager.ShowInter(config, (Action<InterstitialAdResult>)delegate(InterstitialAdResult val)
		{
			if (((AdResult)val).IsSuccess)
			{
				lastShowTime = Time.time;
			}
			callback?.Invoke(val);
		});
	}
}
