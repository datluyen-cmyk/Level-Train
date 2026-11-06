using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.UI.Tips;

public class UITipsOpenParam
{
	public RectTransform Target;

	public string Title;

	public string DetailText;

	public List<ShowRewardItem> Rewards;

	public RectTransform[] ExcludeClickRects;

	public UITips_CommonTips.TipsPlacement? PreferredPlacement;

	public Action OnClosed;
}
