using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Feature.Demo;
using XingYun.Feature;

namespace Game.Feature.Gameplay;

public class GameplayFeatureManager : FeatureManager<GameplayFeatureManager>
{
	public override string GroupName => "Gameplay";

	protected override IEnumerable<Type> GetFeatureTypes()
	{
		return new Type[8]
		{
			typeof(OriginBlockClearFeature),
			typeof(ClearEffectFeature),
			typeof(ComboEffectFeature),
			typeof(WildcardFeature),
			typeof(MultiLineClearFeature),
			typeof(AsyncShakeEffectFeature),
			typeof(SmallRewardFeature),
			typeof(BigRewardFeature)
		};
	}

	public void OnLevelStart(DemoLevelData data)
	{
		base.FireAll<IHookOnLevelStart>((Func<IHookOnLevelStart, FeatureResult>)((IHookOnLevelStart f) => f.OnLevelStart(data)));
	}

	public void OnMatchMap(DemoMatchData data)
	{
		base.FireAll<IHookOnMatchMap>((Func<IHookOnMatchMap, FeatureResult>)((IHookOnMatchMap f) => f.OnMatchMap(data)));
	}

	public IEnumerator OnMatchMapSequential(DemoMatchData data)
	{
		return base.FireSequential<IHookOnMatchMap>((Func<IHookOnMatchMap, FeatureResult>)((IHookOnMatchMap f) => f.OnMatchMap(data)));
	}

	public UniTask OnMatchMapSequentialAsync(DemoMatchData data, CancellationToken ct = default(CancellationToken))
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return base.FireSequentialAsync<IHookOnMatchMap>((Func<IHookOnMatchMap, FeatureResult>)((IHookOnMatchMap f) => f.OnMatchMap(data)), ct);
	}

	public FeatureResult OnValidatePlacement(DemoCard card, DemoSlotColumn column)
	{
		return base.FireFirst<IHookOnValidatePlacement>((Func<IHookOnValidatePlacement, FeatureResult>)((IHookOnValidatePlacement f) => f.OnValidatePlacement(card, column)));
	}

	public void OnCardPlaced(DemoCard card, DemoSlotColumn column)
	{
		base.FireAll<IHookOnCardPlaced>((Func<IHookOnCardPlaced, FeatureResult>)((IHookOnCardPlaced f) => f.OnCardPlaced(card, column)));
	}

	public void OnTilePut(DemoTilePutInfo info)
	{
		base.FireAll<IHookOnTilePut>((Func<IHookOnTilePut, FeatureResult>)((IHookOnTilePut f) => f.OnTilePut(info)));
	}

	public void OnSettleCustomOrder()
	{
		List<IHookOnSettle> hooks = base.GetHooks<IHookOnSettle>();
		hooks.Sort((IHookOnSettle a, IHookOnSettle b) => a.SettleOrder.CompareTo(b.SettleOrder));
		foreach (IHookOnSettle item in hooks)
		{
			item.Settle();
		}
	}
}
