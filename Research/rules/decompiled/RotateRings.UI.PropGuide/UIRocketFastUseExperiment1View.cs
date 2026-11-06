using System;
using Game.UI;
using Game.UI.Common;
using RotateRings.UI.PropItem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Adapt;
using XingYun.UI.Core;

namespace RotateRings.UI.PropGuide;

public class UIRocketFastUseExperiment1View : GameBaseView
{
	private const string Tag = "UI";

	private const string ImgTopBgOpenAnim = "anim_img_top_bg_open_bg";

	private const string ImgTopBgCloseAnim = "anim_img_top_bg_close_bg";

	private const string TopContentOpenAnim = "anim_SingleToast_open";

	private const string TopContentCloseAnim = "anim_SingleToast_close";

	private const string PropItemAppearAnim = "Anim_RocketFastUseExperiment1_appear_BG";

	private const string PropItemCloseAnim = "Anim_RocketFastUseExperiment1_close_BG";

	private WidgetHandle propItemHandle;

	private Animation imgTopBgAni;

	private Animation topContentAni;

	private Animation propItemAni;

	private bool topCloseAnimationPlayed;

	public Button Btn_close;

	public Image ImgProp;

	public TMP_Text TxtPropName;

	public TMP_Text TxtUnlockProp;

	public Button BtnPropRocket;

	public TMP_Text TextBtnName;

	public UIBehaviour PropItem;

	public XYButton Panel;

	public RectTransform ImgTopBg;

	public UIRocketPropItemView BindPropItemWidget()
	{
		UIBehaviour val = (((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((UIBaseView)this).Behaviour.Get<UIBehaviour>("PropItem") : null);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		propItemHandle = AttachWidget<UIRocketPropItemView>(val);
		return propItemHandle?.GetView<UIRocketPropItemView>();
	}

	public void CacheAnimationComponents()
	{
		if (!((Object)(object)Panel == (Object)null))
		{
			imgTopBgAni = FindChildAnimation("ImgTopBg");
			topContentAni = FindChildAnimation("TopContent");
			propItemAni = FindChildAnimation("PropItem");
			ApplyTopBgSafeAreaCover();
		}
	}

	public void ApplyTopBgSafeAreaCover()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ImgTopBg == (Object)null))
		{
			SafeArea val = (((Object)(object)UIRoot.Instance != (Object)null) ? UIRoot.Instance.UISafeArea : null);
			float num = (((Object)(object)val != (Object)null) ? val.SafeAreaTop : 0f);
			ImgTopBg.offsetMax = new Vector2(0f, num);
		}
	}

	private Animation FindChildAnimation(string childName)
	{
		Transform val = ((Component)Panel).transform.Find(childName);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		return ((Component)val).GetComponent<Animation>();
	}

	public void PlayOpenAnimation()
	{
		topCloseAnimationPlayed = false;
		PlayAnimation(imgTopBgAni, "anim_img_top_bg_open_bg");
		PlayAnimation(topContentAni, "anim_SingleToast_open");
		PlayAnimation(propItemAni, "Anim_RocketFastUseExperiment1_appear_BG");
	}

	public float PlayCloseAnimation()
	{
		float num = 0f;
		if (!topCloseAnimationPlayed)
		{
			float num2 = PlayAnimation(imgTopBgAni, "anim_img_top_bg_close_bg");
			float num3 = PlayAnimation(topContentAni, "anim_SingleToast_close");
			num = Mathf.Max(num2, num3);
		}
		float num4 = PlayAnimation(propItemAni, "Anim_RocketFastUseExperiment1_close_BG");
		return Mathf.Max(num, num4);
	}

	public void PlayTopCloseAnimation()
	{
		if (!topCloseAnimationPlayed)
		{
			PlayAnimation(imgTopBgAni, "anim_img_top_bg_close_bg");
			PlayAnimation(topContentAni, "anim_SingleToast_close");
			topCloseAnimationPlayed = true;
		}
	}

	private float PlayAnimation(Animation ani, string animName)
	{
		if ((Object)(object)ani == (Object)null)
		{
			return 0f;
		}
		AnimationState val = ani[animName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (ani.IsPlaying(animName))
		{
			ani.Stop(animName);
			val.time = 0f;
			ani.Sample();
		}
		ani.Play(animName);
		return val.length;
	}

	public UIRocketPropItemView GetPropItemView()
	{
		return propItemHandle?.GetView<UIRocketPropItemView>();
	}

	public void SetTxtPropName(string key)
	{
		if ((Object)(object)TxtPropName != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtPropName, key, Array.Empty<object>());
		}
	}

	public void SetTxtUnlockProp(string key)
	{
		if ((Object)(object)TxtUnlockProp != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtUnlockProp, key, Array.Empty<object>());
		}
	}

	public override void Dispose()
	{
		propItemHandle = null;
		imgTopBgAni = null;
		topContentAni = null;
		propItemAni = null;
		topCloseAnimationPlayed = false;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Btn_close = behaviour.Get<Button>("Btn_close");
		ImgProp = behaviour.Get<Image>("ImgProp");
		TxtPropName = behaviour.Get<TMP_Text>("TxtPropName");
		TxtUnlockProp = behaviour.Get<TMP_Text>("TxtUnlockProp");
		BtnPropRocket = behaviour.Get<Button>("BtnPropRocket");
		TextBtnName = behaviour.Get<TMP_Text>("TextBtnName");
		PropItem = behaviour.Get<UIBehaviour>("PropItem");
		Panel = behaviour.Get<XYButton>("Panel");
		ImgTopBg = behaviour.Get<RectTransform>("ImgTopBg");
	}
}
