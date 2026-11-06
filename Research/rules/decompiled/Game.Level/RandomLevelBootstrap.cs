using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRing.GamePlay;
using RotateRings.Common;
using XingYun.Config;
using XingYun.Level;

namespace Game.Level;

public static class RandomLevelBootstrap
{
	private const string TAG = "[RL][Bootstrap] ";

	public const float TierMin = 1f;

	public const float TierMax = 8f;

	public const int KindDimensionOrder = 1;

	public const int TierDimensionOrder = 2;

	private static bool disabledLogged;

	public static bool IsInitialized { get; private set; }

	public static void Init()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (IsInitialized)
		{
			return;
		}
		if (!FeatureConfig.LevelRandom.ResolveEnable())
		{
			if (!disabledLogged)
			{
				disabledLogged = true;
			}
			return;
		}
		string text = FeatureConfig.LevelRandom.ResolvePoolName();
		try
		{
			LevelSystemRuntimeConfig val = new LevelSystemConfigBuilder().WithRepositoryPath(text).WithDimensions((IDimensionStrategy[])(object)new IDimensionStrategy[2]
			{
				(IDimensionStrategy)new LevelKindDimension(),
				(IDimensionStrategy)new LevelTierDimension()
			}).WithDimensionOrder(new int[2] { 1, 2 })
				.WithSamplerStrategy((IDifficultySamplerStrategy)(object)new RandomLevelSampler())
				.WithMatchAdapter((ILevelMatchAdapter)(object)new RandomLevelMatchAdapter())
				.WithSelectorStrategy((ILevelSelectorStrategy)(object)new RandomLevelSelector())
				.Build();
			GameApp.Level.Initialize(val);
			LevelSystemFacade level = GameApp.Level;
			float? num = 1f;
			float? num2 = 8f;
			level.SetSamplerParams((float?)null, num, num2);
			LevelSystemBusinessExtensions.InitializeRules();
			ConfigManager config = GameApp.Config;
			List<LevelMatchConfig> list = ((config != null) ? config.GetTable<ExampleTables>() : null)?.TbLevelMatchConfig?.DataList;
			FeatureConfig.LevelRandom.ResolveLevelGroupIds();
			FeatureConfig.LevelRandom.ResolveLevelTypeGroupIds();
			FeatureConfig.LevelRandom.ResolvePartsGroupIds();
			FeatureConfig.LevelRandom.ResolveDAWinLoseGroupIds();
			int[] array = FeatureConfig.LevelRandom.ResolveGroupIds();
			if (list != null && list.Count != 0 && array.Length != 0)
			{
				LevelSystemBusinessExtensions.AttachRuleStrategy(array, list);
			}
			IsInitialized = true;
			LevelGapDegExp.Execute();
			int num3 = 0;
			LevelRepository repository = LevelSystem.GetRepository();
			if (repository == null)
			{
				return;
			}
			foreach (string item in repository.AllCsvIds())
			{
				_ = item;
				num3++;
			}
		}
		catch (Exception)
		{
		}
	}
}
