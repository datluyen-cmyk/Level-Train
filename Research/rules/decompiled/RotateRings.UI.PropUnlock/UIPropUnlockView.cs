using System;
using Game.UI;
using RotateRings.Prop;
using RotateRings.UI.Game;
using RotateRings.UI.PropItem;
using RotateRings.UI.Props;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.PropUnlock;

public class UIPropUnlockView : GameBaseView
{
	private const string OpenAnimName = "anim_uipropunlock_open";

	private const string CloseAnimName = "anim_uipropunlock_close";

	private const string PropItemLocation = "PropItem";

	private WidgetHandle propItemHandle;

	private EPropType maskedUIGamePropType;

	public Animation UIPropUnlockAni;

	public Button MaskBtn;

	public RectTransform Panel;

	public TMP_Text TxtPropName;

	public TMP_Text TxtUnlockProp;

	public TMP_Text Txtcontinue;

	public Image ImgProp;

	public TMP_Text TxtPropNum;

	public Button ContinueBtn;

	public void Init(PropUnlockOpenData data)
	{
		if (data != null)
		{
			if ((Object)(object)TxtPropName != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(TxtPropName, data.PropName, Array.Empty<object>());
			}
			if ((Object)(object)TxtUnlockProp != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(TxtUnlockProp, "Item_Unlock_Title", Array.Empty<object>());
			}
			if ((Object)(object)Txtcontinue != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(Txtcontinue, "Item_Continue", Array.Empty<object>());
			}
			if ((Object)(object)TxtPropNum != (Object)null)
			{
				TMPTextExtensions.SetLocalizedTextValue(TxtPropNum, (data.GrantCount > 0) ? $"{data.GrantCount}" : string.Empty);
				((Component)TxtPropNum).gameObject.SetActive(data.GrantCount > 0);
			}
			if ((Object)(object)ImgProp != (Object)null && !string.IsNullOrEmpty(data.IconName))
			{
				SetSprite(ImgProp, data.IconName);
			}
		}
	}

	public void PlayOpenAnimation()
	{
		PlayAnimation("anim_uipropunlock_open");
	}

	public float PlayCloseAnimation()
	{
		return PlayAnimation("anim_uipropunlock_close");
	}

	public bool TryGetFlyEffectLocalPosition(out Vector2 localPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		localPosition = Vector2.zero;
		if ((Object)(object)Panel == (Object)null || (Object)(object)ImgProp == (Object)null)
		{
			return false;
		}
		RectTransform rectTransform = ((Graphic)ImgProp).rectTransform;
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector3 val = (array[0] + array[2]) * 0.5f;
		Vector2 val2 = RectTransformUtility.WorldToScreenPoint((Camera)null, val);
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(Panel, val2, (Camera)null, ref localPosition);
	}

	public bool SetupUnlockPropItem(PropUnlockOpenData data)
	{
		if (data == null || (Object)(object)Panel == (Object)null)
		{
			return false;
		}
		UIPropsView uIPropsView = TryGetUIGamePropsView();
		RectTransform val = uIPropsView?.GetPropItemAlignAnchor(data.PropType);
		if (uIPropsView == null || (Object)(object)val == (Object)null)
		{
			return false;
		}
		ClearUnlockPropItem();
		propItemHandle = AttachWidget<UIPropItemView>("PropItem", (Transform)(object)Panel);
		UIPropItemView uIPropItemView = TryGetUnlockPropItemView();
		if (uIPropItemView == null)
		{
			return false;
		}
		uIPropItemView.RefreshProp(data.PropType);
		uIPropItemView.SetBadgesVisible(visible: false);
		SetPropItemInteractable(uIPropItemView, interactable: false);
		AlignPropItemToSource(val, Panel, ((UIBaseView)uIPropItemView).NodeRect ?? uIPropItemView.Node);
		MaskUIGamePropBadges(data.PropType, uIPropsView);
		SetUnlockPropItemVisible(visible: false);
		return true;
	}

	public bool ReAlignUnlockPropItem(EPropType propType)
	{
		if ((Object)(object)Panel == (Object)null || propType == EPropType.None)
		{
			return false;
		}
		UIPropsView uIPropsView = TryGetUIGamePropsView();
		RectTransform val = uIPropsView?.GetPropItemAlignAnchor(propType);
		if (uIPropsView == null || (Object)(object)val == (Object)null)
		{
			return false;
		}
		UIPropItemView uIPropItemView = TryGetUnlockPropItemView();
		if (uIPropItemView == null)
		{
			return false;
		}
		AlignPropItemToSource(val, Panel, ((UIBaseView)uIPropItemView).NodeRect ?? uIPropItemView.Node);
		return true;
	}

	public UIPropItemView TryGetUnlockPropItemView()
	{
		return propItemHandle?.GetView<UIPropItemView>();
	}

	public void SetUnlockPropItemVisible(bool visible)
	{
		if (propItemHandle != null && !((Object)(object)propItemHandle.GameObject == (Object)null))
		{
			UIPropItemView uIPropItemView = TryGetUnlockPropItemView();
			if (!visible)
			{
				uIPropItemView?.StopAppearFade();
				propItemHandle.GameObject.SetActive(false);
			}
			else
			{
				propItemHandle.GameObject.SetActive(true);
				uIPropItemView?.PlayAppearFadeIn();
			}
		}
	}

	public void ClearUnlockPropItem()
	{
		RestoreUIGamePropBadges();
		if (propItemHandle != null)
		{
			DetachWidget(propItemHandle);
			propItemHandle = null;
		}
	}

	private float PlayAnimation(string animName)
	{
		if ((Object)(object)UIPropUnlockAni == (Object)null)
		{
			return 0f;
		}
		AnimationState val = UIPropUnlockAni[animName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (UIPropUnlockAni.IsPlaying(animName))
		{
			UIPropUnlockAni.Stop(animName);
			val.time = 0f;
			UIPropUnlockAni.Sample();
		}
		UIPropUnlockAni.Play(animName);
		return val.length;
	}

	private static void AlignPropItemToSource(RectTransform source, RectTransform parent, RectTransform target)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = default(Vector2);
		if (!((Object)(object)source == (Object)null) && !((Object)(object)parent == (Object)null) && !((Object)(object)target == (Object)null) && TryGetSourceScreenCenter(source, out var screenCenter) && RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenCenter, (Camera)null, ref anchoredPosition))
		{
			((Transform)target).SetParent((Transform)(object)parent, false);
			target.anchorMin = new Vector2(0.5f, 0.5f);
			target.anchorMax = new Vector2(0.5f, 0.5f);
			target.pivot = source.pivot;
			Rect rect = source.rect;
			target.sizeDelta = ((Rect)(ref rect)).size;
			((Transform)target).localScale = Vector3.one;
			target.anchoredPosition = anchoredPosition;
		}
	}

	private static bool TryGetSourceScreenCenter(RectTransform source, out Vector2 screenCenter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		screenCenter = default(Vector2);
		if ((Object)(object)source == (Object)null)
		{
			return false;
		}
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			return uIGameController.View.TryGetGuideAlignScreenCenter(source, out screenCenter);
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		source.GetWorldCorners(array);
		Vector3 val2 = (array[0] + array[2]) * 0.5f;
		screenCenter = RectTransformUtility.WorldToScreenPoint((Camera)null, val2);
		return true;
	}

	private static void SetPropItemInteractable(UIPropItemView propItemView, bool interactable)
	{
		if (propItemView != null)
		{
			if ((Object)(object)propItemView.CanvasGroup != (Object)null)
			{
				propItemView.CanvasGroup.interactable = interactable;
				propItemView.CanvasGroup.blocksRaycasts = interactable;
			}
			if ((Object)(object)propItemView.btnProp != (Object)null)
			{
				((Selectable)propItemView.btnProp).interactable = interactable;
			}
		}
	}

	private static UIPropsView TryGetUIGamePropsView()
	{
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return null;
		}
		return uIGameController.View?.PropsView;
	}

	private void MaskUIGamePropBadges(EPropType propType, UIPropsView propsView)
	{
		if (propType != EPropType.None && propsView != null)
		{
			propsView.SetPropItemBadgesVisible(propType, visible: false);
			maskedUIGamePropType = propType;
		}
	}

	private void RestoreUIGamePropBadges()
	{
		if (maskedUIGamePropType != EPropType.None)
		{
			TryGetUIGamePropsView()?.SetPropItemBadgesVisible(maskedUIGamePropType, visible: true);
			maskedUIGamePropType = EPropType.None;
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIPropUnlockAni = behaviour.Get<Animation>("UIPropUnlockAni");
		MaskBtn = behaviour.Get<Button>("MaskBtn");
		Panel = behaviour.Get<RectTransform>("Panel");
		TxtPropName = behaviour.Get<TMP_Text>("TxtPropName");
		TxtUnlockProp = behaviour.Get<TMP_Text>("TxtUnlockProp");
		Txtcontinue = behaviour.Get<TMP_Text>("Txtcontinue");
		ImgProp = behaviour.Get<Image>("ImgProp");
		TxtPropNum = behaviour.Get<TMP_Text>("TxtPropNum");
		ContinueBtn = behaviour.Get<Button>("ContinueBtn");
	}
}
