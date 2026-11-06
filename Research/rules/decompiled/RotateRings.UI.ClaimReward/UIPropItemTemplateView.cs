using Example.Cfg;
using Game.UI;
using RotateRings.Prop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.ClaimReward;

public class UIPropItemTemplateView : GameWidgetView
{
	public Image ImgProp;

	public TMP_Text TxtPropCount;

	public RectTransform NodeRect;

	public Animation Anim;

	public GameObject bg;

	public GameObject ImgBg1;

	public GameObject ImgBg2;

	public int PropId { get; private set; }

	public int CurrentCount { get; private set; }

	public void OnRender(int propId, int count)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		EPropType ePropType = propModelProxy?.GetRewardPropType(propId) ?? EPropType.None;
		PropConfig propConfig = ((ePropType == EPropType.None) ? null : propModelProxy.GetPropConfig(ePropType));
		if (propConfig != null)
		{
			SetSprite(ImgProp, propConfig.IconLarge);
		}
		PropId = propId;
		SetPropCount(count);
	}

	public void SetPropCount(int count)
	{
		CurrentCount = count;
		if ((Object)(object)TxtPropCount != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtPropCount, count.ToString());
		}
	}

	public void SetNodeActive(bool active)
	{
		if ((Object)(object)NodeRect != (Object)null)
		{
			((Component)NodeRect).gameObject.SetActive(active);
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ImgProp = behaviour.Get<Image>("ImgProp");
		TxtPropCount = behaviour.Get<TMP_Text>("TxtPropCount");
		NodeRect = behaviour.Get<RectTransform>("NodeRect");
		Anim = behaviour.Get<Animation>("Anim");
		bg = behaviour.Get<GameObject>("bg");
		ImgBg1 = behaviour.Get<GameObject>("ImgBg1");
		ImgBg2 = behaviour.Get<GameObject>("ImgBg2");
	}
}
