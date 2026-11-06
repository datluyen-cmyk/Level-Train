using System;
using Example.Cfg;
using RotateRings.Prop;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.RewardedLuckyDraw;

[UIPath("UIRewardedLuckyDrawWheel")]
public sealed class UIRewardedLuckyDrawWheelController : UIRewardedLuckyDrawController<UIRewardedLuckyDrawWheelView>
{
	public UIRewardedLuckyDrawWheelController(UIRewardedLuckyDrawWheelView view)
		: base(view)
	{
	}

	protected override void PlayOpenPresentation(Action onCompleted)
	{
		((BaseController<UIRewardedLuckyDrawWheelView>)this).view.PlayOpenAnimation(onCompleted);
	}

	protected override void PlayClosePresentation(Action onCompleted)
	{
		((BaseController<UIRewardedLuckyDrawWheelView>)this).view.PlayCloseAnimation(onCompleted);
	}

	protected override void PlayClaimExitPresentation(Action onCompleted)
	{
		((BaseController<UIRewardedLuckyDrawWheelView>)this).view.PlayCloseAnimation(onCompleted);
	}

	public void HideMaskForClaimClose()
	{
		((BaseController<UIRewardedLuckyDrawWheelView>)this).view.HideMaskForClaimClose();
	}

	protected override string[] GetPropIconPaths(RewardedLuckyDrawProbability[] slots)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		string[] array = new string[slots.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			array[i] = propModelProxy.GetPropConfig(slots[i].PropType)?.Icon;
		}
		return array;
	}
}
