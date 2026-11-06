using System;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.NewElementIntro;

public class UINewElementIntroView : GameBaseView
{
	private const string TitleLanguageKey = "NewElement_Title";

	private const string TapToContinueLanguageKey = "NewElement_TapToContinue";

	private const string OpenAnimationName_Mask = "anim_bgmask_open";

	private const string OpenAnimationName_Content = "Anim_UINewElementIntro_open";

	private const string CloseAnimationName_Mask = "anim_bgmask_close";

	private const string CloseAnimationName_Content = "Anim_UINewElementIntro_close";

	public Button btn_Close;

	public TMP_Text txtm_Title;

	public TMP_Text txtm_Name;

	public TMP_Text txtm_Desc;

	public TMP_Text txtm_TapToContinue;

	public RectTransform rect_ElementVisualRoot;

	public Image img_Element;

	public RectTransform rect_ElementLoopFxRoot;

	public GameObject go_Effect;

	public Animation ani_Mask;

	public Animation ani_Content;

	public void Init(NewElementIntroOpenData data)
	{
		if (data != null)
		{
			TMP_Text obj = txtm_Title;
			if (obj != null)
			{
				TMPTextExtensions.SetLocalizedText(obj, "NewElement_Title", Array.Empty<object>());
			}
			TMP_Text obj2 = txtm_Name;
			if (obj2 != null)
			{
				TMPTextExtensions.SetLocalizedText(obj2, data.NameKey, Array.Empty<object>());
			}
			TMP_Text obj3 = txtm_Desc;
			if (obj3 != null)
			{
				TMPTextExtensions.SetLocalizedText(obj3, data.DescKey, Array.Empty<object>());
			}
			TMP_Text obj4 = txtm_TapToContinue;
			if (obj4 != null)
			{
				TMPTextExtensions.SetLocalizedText(obj4, "NewElement_TapToContinue", Array.Empty<object>());
			}
			if ((Object)(object)img_Element != (Object)null)
			{
				Sprite val = LoadAsset<Sprite>(data.Icon);
				img_Element.sprite = val;
				((Graphic)img_Element).SetNativeSize();
				((Component)img_Element).gameObject.SetActive((Object)(object)val != (Object)null);
				_ = (Object)(object)val == (Object)null;
			}
			PlayOpenAnimation();
		}
	}

	private void PlayOpenAnimation()
	{
		if (!((Object)(object)ani_Mask == (Object)null) && !((Object)(object)ani_Mask.GetClip("anim_bgmask_open") == (Object)null) && !((Object)(object)ani_Content == (Object)null) && !((Object)(object)ani_Content.GetClip("Anim_UINewElementIntro_open") == (Object)null))
		{
			ani_Mask.Play("anim_bgmask_open");
			ani_Content.Play("Anim_UINewElementIntro_open");
		}
	}

	public void PlayCloseAnimation(Action onComplete)
	{
		AnimationClip obj = (((Object)(object)ani_Mask != (Object)null) ? ani_Mask.GetClip("anim_bgmask_close") : null);
		AnimationClip clip_Content = (((Object)(object)ani_Content != (Object)null) ? ani_Content.GetClip("Anim_UINewElementIntro_close") : null);
		if ((Object)(object)obj == (Object)null || (Object)(object)clip_Content == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		GameApp.Timer.Delay((Object)(object)((UIBaseView)this).Behaviour, 0.25f, (Action)delegate
		{
			ani_Mask.Stop();
			ani_Mask.Rewind("anim_bgmask_close");
			ani_Mask.Play("anim_bgmask_close", (PlayMode)4);
			GameApp.Timer.Delay((Object)(object)((UIBaseView)this).Behaviour, clip_Content.length, (Action)delegate
			{
				onComplete?.Invoke();
			});
		});
		ani_Content.Stop();
		ani_Content.Rewind("Anim_UINewElementIntro_close");
		ani_Content.Play("Anim_UINewElementIntro_close", (PlayMode)4);
	}

	public override void Dispose()
	{
		if ((Object)(object)img_Element != (Object)null)
		{
			img_Element.sprite = null;
		}
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btn_Close = behaviour.Get<Button>("btn_Close");
		txtm_Title = behaviour.Get<TMP_Text>("txtm_Title");
		txtm_Name = behaviour.Get<TMP_Text>("txtm_Name");
		txtm_Desc = behaviour.Get<TMP_Text>("txtm_Desc");
		txtm_TapToContinue = behaviour.Get<TMP_Text>("txtm_TapToContinue");
		rect_ElementVisualRoot = behaviour.Get<RectTransform>("rect_ElementVisualRoot");
		img_Element = behaviour.Get<Image>("img_Element");
		rect_ElementLoopFxRoot = behaviour.Get<RectTransform>("rect_ElementLoopFxRoot");
		go_Effect = behaviour.Get<GameObject>("go_Effect");
		ani_Mask = behaviour.Get<Animation>("ani_Mask");
		ani_Content = behaviour.Get<Animation>("ani_Content");
	}
}
