using Game.UI;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.AdPop;

public class AdPopNoWifiWidget : GameWidgetView
{
	public GameObject goAdBadge;

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		goAdBadge = (behaviour.Contains("goAdBadge") ? behaviour.Get<GameObject>("goAdBadge") : null);
	}

	public void SetVisible(bool visible)
	{
		if ((Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(visible);
		}
	}
}
