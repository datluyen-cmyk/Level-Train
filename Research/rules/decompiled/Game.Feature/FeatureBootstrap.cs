using Game.Feature.Ad;
using Game.Feature.Demo;
using Game.Feature.Gameplay;
using Game.Feature.UI;
using UnityEngine;
using XingYun.Feature;

namespace Game.Feature;

public static class FeatureBootstrap
{
	public static void Start()
	{
		FeatureManager<GameplayFeatureManager>.Create();
		FeatureManager<AdFeatureManager>.Create();
		FeatureManager<UIFeatureManager>.Create();
	}

	public static void Refresh()
	{
		FeatureManager<GameplayFeatureManager>.Refresh();
		FeatureManager<AdFeatureManager>.Refresh();
		FeatureManager<UIFeatureManager>.Refresh();
	}

	public static void CreateTestRunner()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		GameObject val = new GameObject("[FeatureTestRunner]");
		val.AddComponent<FeatureTestRunner>();
		Object.DontDestroyOnLoad((Object)val);
	}
}
