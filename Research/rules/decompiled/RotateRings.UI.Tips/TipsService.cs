using System;
using System.Collections.Generic;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Tips;

public static class TipsService
{
	public static void CloseIfOpen()
	{
		if (GameApp.UI.IsOpen("UITips"))
		{
			GameApp.UI.CloseView("UITips", (object)null, false);
		}
	}

	public static void Open(UITipsOpenParam param)
	{
		if (param != null && !((Object)(object)param.Target == (Object)null))
		{
			IController val = default(IController);
			if (GameApp.UI.IsOpen("UITips", ref val) && val is UITipsController uITipsController)
			{
				uITipsController.ShowInstance(param);
			}
			else
			{
				GameApp.UI.Open("UITips", (UILayer)4, (object)param, (Action)null);
			}
		}
	}

	public static void ShowChestRewards(RectTransform target, List<ShowRewardItem> rewards, string title = null, UITips_CommonTips.TipsPlacement? preferredPlacement = null)
	{
		if (!((Object)(object)target == (Object)null) && rewards != null && rewards.Count != 0)
		{
			Open(new UITipsOpenParam
			{
				Target = target,
				Rewards = rewards,
				PreferredPlacement = preferredPlacement
			});
		}
	}

	public static void ShowPropDetail(RectTransform target, string detailText, string title = null)
	{
		if (!((Object)(object)target == (Object)null) && !string.IsNullOrEmpty(detailText))
		{
			Open(new UITipsOpenParam
			{
				Target = target,
				Title = title,
				DetailText = detailText
			});
		}
	}
}
