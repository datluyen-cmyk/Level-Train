using System;
using Game.DataTrack;
using Game.UI;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Game;
using RotateRings.UI.PropItem;
using RotateRings.UI.Props;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Adapt;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.PropGuide;

public class UIPropGuideView : GameBaseView
{
	private const string Tag = "UI";

	private const string PropItemLocation = "PropItem";

	private const string ButtonGuideEffectLocation = "eff_button_guide";

	private const string GuideHandButtonEffectLocation = "eff_guidehand_loop_button";

	private const string GuideOpenWithChangeAnim = "anim_UIPropGuide_open_change";

	private const string GuideOpenWithoutChangeAnim = "anim_UIPropGuide_open_nochange";

	private const string GuideCloseAnim = "anim_UIPropGuide_close";

	private const string TipOpenAnim = "anim_SingleToast_open";

	private const float ItemTipOffsetAbovePropItem = 312f;

	private const float UseBtnOffsetAbovePropItem = 289f;

	private const string TipCloseAnim = "anim_SingleToast_close";

	private WidgetHandle propItemHandle;

	private bool isWaitingItemTipConfirm;

	private Action onItemTipConfirmed;

	private bool fireOnItemTipConfirm;

	private UnityAction onPropItemClick;

	private UIPropItemView boundPropItemView;

	private EPropType maskedUIGamePropType;

	private EPropType currentPropType;

	private GameObject buttonGuidePrefab;

	private GameObject buttonGuideEffect;

	private GameObject guideHandButtonPrefab;

	private GameObject guideHandButtonEffect;

	private int itemTipHideTimerId = -1;

	public Action OnTopContentShown;

	public Action OnItemTipPhaseEnded;

	public Animation UIPropGuideAni;

	public RectTransform Panel;

	public RectTransform TopContent;

	public Animation TopContentAni;

	public TMP_Text TxtPropName;

	public TMP_Text TxtUnlockProp;

	public Image ImgProp;

	public RawImage ImgRingPreview;

	public Button Btnclose;

	public Button BtnUse;

	public Button Mask;

	public RectTransform ItemTip;

	public Animation ItemTipAni;

	public TMP_Text TxtItemTip;

	public bool Init(PropGuideOpenData data)
	{
		if (!TryValidateInitData(data, out var propProxy))
		{
			return false;
		}
		ResetItemTipSession(data);
		currentPropType = data.PropType;
		ConfigureMaskInteraction(enableInteraction: true);
		ConfigureUseButton(isVisible: false);
		SetupContentPhase(data.IsFirstGuide);
		SetupPropTexts(propProxy, data.PropType, data.IsFirstGuide);
		SetupPropIcon(data.PropType);
		SetupGuidePropItem(data);
		return true;
	}

	public void RefreshPropItem(EPropType propType)
	{
		GetGuidePropItemView()?.RefreshProp(propType);
	}

	public override void Dispose()
	{
		ClearGuidePropItem();
		ResetSessionState();
		((UIBaseView)this).Dispose();
	}

	public void ConfigureMaskInteraction(bool enableInteraction)
	{
		if (!((Object)(object)Mask == (Object)null))
		{
			((Selectable)Mask).interactable = enableInteraction;
			if ((Object)(object)((Selectable)Mask).targetGraphic != (Object)null)
			{
				((Selectable)Mask).targetGraphic.raycastTarget = enableInteraction;
			}
			CanvasGroup component = ((Component)Mask).GetComponent<CanvasGroup>();
			if ((Object)(object)component != (Object)null)
			{
				component.interactable = enableInteraction;
				component.blocksRaycasts = enableInteraction;
			}
		}
	}

	public void ConfigureUseButton(bool isVisible)
	{
		if (!((Object)(object)BtnUse == (Object)null))
		{
			((Component)BtnUse).gameObject.SetActive(isVisible);
			((Selectable)BtnUse).interactable = isVisible;
		}
	}

	public void SetUseButtonInteractable(bool interactable)
	{
		if ((Object)(object)BtnUse != (Object)null)
		{
			((Selectable)BtnUse).interactable = interactable;
		}
	}

	private void SetupContentPhase(bool showItemTip, bool animateItemTipHide = false)
	{
		if (showItemTip)
		{
			ShowItemTip();
		}
		else if (animateItemTipHide)
		{
			HideItemTipDelayed();
		}
		else
		{
			HideItemTipImmediate();
		}
		if ((Object)(object)TopContent != (Object)null)
		{
			if (!showItemTip)
			{
				((Component)TopContent).gameObject.SetActive(true);
				PlayTopContentAnimation(open: true);
			}
			else
			{
				((Component)TopContent).gameObject.SetActive(false);
			}
		}
		ApplyGuidePropItemInputState(showItemTip);
	}

	private void ShowItemTip()
	{
		if (!((Object)(object)ItemTip == (Object)null))
		{
			((Component)ItemTip).gameObject.SetActive(true);
			PlayItemTipAnimation(open: true);
		}
	}

	private void HideItemTipImmediate()
	{
		if ((Object)(object)ItemTip != (Object)null)
		{
			((Component)ItemTip).gameObject.SetActive(false);
		}
	}

	private void HideItemTipDelayed()
	{
		if ((Object)(object)ItemTip == (Object)null || !((Component)ItemTip).gameObject.activeSelf)
		{
			return;
		}
		float num = PlayItemTipAnimation(open: false);
		if (num <= 0f)
		{
			((Component)ItemTip).gameObject.SetActive(false);
			return;
		}
		itemTipHideTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)this).Behaviour, num, (Action)delegate
		{
			itemTipHideTimerId = -1;
			if ((Object)(object)ItemTip != (Object)null)
			{
				((Component)ItemTip).gameObject.SetActive(false);
			}
		});
	}

	private void SetupPropTexts(PropModelProxy propProxy, EPropType propType, bool showItemTip)
	{
		if ((Object)(object)TxtPropName != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtPropName, propProxy.GetPropName(propType), Array.Empty<object>());
		}
		if (showItemTip && (Object)(object)TxtItemTip != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtItemTip, propProxy.GetPropGuideKey(propType), Array.Empty<object>());
		}
		SetupUseButtonText();
		if (!((Object)(object)TxtUnlockProp == (Object)null))
		{
			string propDescByKey = propProxy.GetPropDescByKey(propType, 0);
			TMPTextExtensions.SetLocalizedTextValue(TxtUnlockProp, string.IsNullOrEmpty(propDescByKey) ? string.Empty : GameApp.Lang.GetText(propDescByKey, Array.Empty<object>()));
		}
	}

	public void UpdateTxtItemTip(string key)
	{
		if ((Object)(object)TxtItemTip != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtItemTip, key, Array.Empty<object>());
		}
	}

	private void SetupUseButtonText()
	{
		if (!((Object)(object)BtnUse == (Object)null))
		{
			TMP_Text componentInChildren = ((Component)BtnUse).GetComponentInChildren<TMP_Text>(true);
			if (!((Object)(object)componentInChildren == (Object)null))
			{
				TMPTextExtensions.SetLocalizedText(componentInChildren, "Button_Use", Array.Empty<object>());
			}
		}
	}

	private void SetupPropIcon(EPropType propType)
	{
		if (!((Object)(object)ImgProp == (Object)null))
		{
			string iconName = PropIconService.GetIconName(propType);
			if (string.IsNullOrEmpty(iconName))
			{
				((Component)ImgProp).gameObject.SetActive(false);
				return;
			}
			((Component)ImgProp).gameObject.SetActive(true);
			SetSprite(ImgProp, iconName);
		}
	}

	public void PrepareRingPreviewLayout()
	{
		ApplyRingPreviewFullScreenLayout();
	}

	public bool TryGetChainLockAnchor(out Transform guideRoot, out RectTransform insertBefore)
	{
		guideRoot = null;
		insertBefore = null;
		if ((Object)(object)Panel == (Object)null || (Object)(object)ImgRingPreview == (Object)null)
		{
			return false;
		}
		guideRoot = ((Component)ImgRingPreview).transform.parent;
		insertBefore = Panel;
		return (Object)(object)guideRoot != (Object)null;
	}

	public void ShowHintRingPreview(RenderTexture rt)
	{
		if (!((Object)(object)ImgRingPreview == (Object)null) && !((Object)(object)rt == (Object)null))
		{
			ImgRingPreview.texture = (Texture)(object)rt;
			((Component)ImgRingPreview).gameObject.SetActive(true);
		}
	}

	public void ClearHintRingPreview()
	{
		if (!((Object)(object)ImgRingPreview == (Object)null))
		{
			ImgRingPreview.texture = null;
			((Component)ImgRingPreview).gameObject.SetActive(false);
		}
	}

	private void ApplyRingPreviewFullScreenLayout()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ImgRingPreview == (Object)null))
		{
			SafeArea val = (((Object)(object)UIRoot.Instance != (Object)null) ? UIRoot.Instance.UISafeArea : null);
			float num = (((Object)(object)val != (Object)null) ? val.SafeAreaTop : 0f);
			float num2 = (((Object)(object)val != (Object)null) ? val.SafeAreaBottom : 0f);
			RectTransform rectTransform = ((Graphic)ImgRingPreview).rectTransform;
			rectTransform.offsetMax = new Vector2(0f, num);
			rectTransform.offsetMin = new Vector2(0f, 0f - num2);
		}
	}

	public bool IsTopContentVisible()
	{
		if ((Object)(object)TopContent != (Object)null)
		{
			return ((Component)TopContent).gameObject.activeSelf;
		}
		return false;
	}

	public bool IsItemTipVisible()
	{
		if ((Object)(object)ItemTip != (Object)null)
		{
			return ((Component)ItemTip).gameObject.activeSelf;
		}
		return false;
	}

	private void SetupGuidePropItem(PropGuideOpenData data)
	{
		if ((Object)(object)Panel == (Object)null || (Object)(object)data?.SourceAnchor == (Object)null)
		{
			return;
		}
		ClearGuidePropItem();
		propItemHandle = AttachWidget<UIPropItemView>("PropItem", (Transform)(object)Panel);
		UIPropItemView guidePropItemView = GetGuidePropItemView();
		if (guidePropItemView != null)
		{
			guidePropItemView.RefreshProp(data.PropType);
			TryAlignGuidePropItemToSource(data.SourceAnchor, guidePropItemView);
			if (data.EntrySource == PropUseEntrySource.Click)
			{
				guidePropItemView.btnProp?.PlayClickFeedback();
			}
			SpawnButtonGuideEffect(guidePropItemView);
			ApplyGuidePropItemInputState(data.IsFirstGuide);
			MaskUIGamePropItem(data.PropType);
		}
	}

	private void ClearGuidePropItem()
	{
		RecycleButtonGuideEffect();
		RecycleGuideHandButtonEffect();
		UnbindGuidePropItemClick();
		RestoreUIGamePropItem();
		if (propItemHandle != null)
		{
			DetachWidget(propItemHandle);
			propItemHandle = null;
		}
	}

	private void TryAlignGuidePropItemToSource(RectTransform sourceAnchor, UIPropItemView propItemView)
	{
		RectTransform val = ((UIBaseView)propItemView).NodeRect ?? propItemView.Node;
		if (!((Object)(object)val == (Object)null))
		{
			AlignPropItemToSource(sourceAnchor, Panel, val);
			AlignItemTipAbovePropItem(val);
			AlignUseButtonAbovePropItem(val);
		}
	}

	private void AlignItemTipAbovePropItem(RectTransform propItemRect)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)propItemRect == (Object)null) && !((Object)(object)ItemTip == (Object)null))
		{
			ItemTip.anchorMin = new Vector2(0.5f, 0.5f);
			ItemTip.anchorMax = new Vector2(0.5f, 0.5f);
			ItemTip.pivot = new Vector2(0.5f, 0.5f);
			ItemTip.anchoredPosition = new Vector2(0f, propItemRect.anchoredPosition.y + 312f);
		}
	}

	private void AlignUseButtonAbovePropItem(RectTransform propItemRect)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)propItemRect == (Object)null) && !((Object)(object)BtnUse == (Object)null) && !((Object)(object)Panel == (Object)null))
		{
			Transform transform = ((Component)BtnUse).transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (!((Object)(object)val == (Object)null))
			{
				val.anchorMin = new Vector2(0.5f, 0.5f);
				val.anchorMax = new Vector2(0.5f, 0.5f);
				val.pivot = new Vector2(0.5f, 0.5f);
				Vector3 val2 = ((Transform)Panel).InverseTransformPoint(((Transform)propItemRect).position);
				Rect rect = Panel.rect;
				val2.x = ((Rect)(ref rect)).center.x;
				val2.y += 289f;
				((Transform)val).position = ((Transform)Panel).TransformPoint(val2);
			}
		}
	}

	public UIPropItemView GetGuidePropItemView()
	{
		return propItemHandle?.GetView<UIPropItemView>();
	}

	private void SpawnButtonGuideEffect(UIPropItemView propItemView)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (propItemView == null || (Object)(object)propItemView.Node == (Object)null)
		{
			return;
		}
		if (buttonGuidePrefab == null)
		{
			buttonGuidePrefab = LoadAsset<GameObject>("eff_button_guide");
		}
		if (!((Object)(object)buttonGuidePrefab == (Object)null))
		{
			RecycleButtonGuideEffect();
			buttonGuideEffect = GameApp.Pool.Spawn(buttonGuidePrefab, (Transform)(object)propItemView.Node);
			if (!((Object)(object)buttonGuideEffect == (Object)null))
			{
				Transform transform = buttonGuideEffect.transform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				transform.SetAsFirstSibling();
			}
		}
	}

	private void RecycleButtonGuideEffect()
	{
		if (!((Object)(object)buttonGuideEffect == (Object)null))
		{
			GameApp.Pool.Recycle(buttonGuideEffect);
			buttonGuideEffect = null;
		}
	}

	public void ShowGuideHandButtonEffect()
	{
		UIPropItemView guidePropItemView = GetGuidePropItemView();
		if (guidePropItemView != null)
		{
			SpawnGuideHandButtonEffect(guidePropItemView);
		}
	}

	public void HideGuideHandButtonEffect()
	{
		RecycleGuideHandButtonEffect();
	}

	private void SpawnGuideHandButtonEffect(UIPropItemView propItemView)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (propItemView == null || (Object)(object)propItemView.Node == (Object)null)
		{
			return;
		}
		if (guideHandButtonPrefab == null)
		{
			guideHandButtonPrefab = LoadAsset<GameObject>("eff_guidehand_loop_button");
		}
		if ((Object)(object)guideHandButtonPrefab == (Object)null)
		{
			return;
		}
		RecycleGuideHandButtonEffect();
		guideHandButtonEffect = GameApp.Pool.Spawn(guideHandButtonPrefab, (Transform)(object)propItemView.Node);
		if (!((Object)(object)guideHandButtonEffect == (Object)null))
		{
			Transform transform = guideHandButtonEffect.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			Graphic[] componentsInChildren = guideHandButtonEffect.GetComponentsInChildren<Graphic>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].raycastTarget = false;
			}
			transform.SetAsLastSibling();
		}
	}

	private void RecycleGuideHandButtonEffect()
	{
		if (!((Object)(object)guideHandButtonEffect == (Object)null))
		{
			GameApp.Pool.Recycle(guideHandButtonEffect);
			guideHandButtonEffect = null;
		}
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
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		screenCenter = default(Vector2);
		if ((Object)(object)source == (Object)null)
		{
			return false;
		}
		if (TryGetUIGameController(out var gameController))
		{
			return gameController.View.TryGetGuideAlignScreenCenter(source, out screenCenter);
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		source.GetWorldCorners(array);
		Vector3 val = (array[0] + array[2]) * 0.5f;
		screenCenter = RectTransformUtility.WorldToScreenPoint((Camera)null, val);
		return true;
	}

	private void ApplyGuidePropItemInputState(bool allowPropItemClick)
	{
		UIPropItemView guidePropItemView = GetGuidePropItemView();
		if (guidePropItemView != null)
		{
			SetPropItemInteractable(guidePropItemView, allowPropItemClick);
			if (allowPropItemClick)
			{
				BindGuidePropItemClick(guidePropItemView);
			}
			else
			{
				UnbindGuidePropItemClick();
			}
		}
	}

	private static void SetPropItemInteractable(UIPropItemView propItemView, bool interactable)
	{
		if (propItemView != null)
		{
			if ((Object)(object)propItemView.CanvasGroup != (Object)null)
			{
				propItemView.CanvasGroup.interactable = interactable;
				propItemView.CanvasGroup.blocksRaycasts = true;
			}
			if ((Object)(object)propItemView.btnProp != (Object)null)
			{
				((Selectable)propItemView.btnProp).interactable = interactable;
			}
		}
	}

	private void BindGuidePropItemClick(UIPropItemView propItemView)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if (!((Object)(object)propItemView?.btnProp == (Object)null))
		{
			UnbindGuidePropItemClick();
			boundPropItemView = propItemView;
			onPropItemClick = new UnityAction(OnGuidePropItemClicked);
			((UnityEvent)((Button)propItemView.btnProp).onClick).AddListener(onPropItemClick);
		}
	}

	private void OnGuidePropItemClicked()
	{
		if (isWaitingItemTipConfirm)
		{
			isWaitingItemTipConfirm = false;
			if (fireOnItemTipConfirm)
			{
				RRDataTrack.Instance.ReportBoardPropClick(currentPropType);
				OnItemTipPhaseEnded?.Invoke();
				onItemTipConfirmed?.Invoke();
			}
			else
			{
				SetupContentPhase(showItemTip: false, animateItemTipHide: true);
				onItemTipConfirmed?.Invoke();
				OnItemTipPhaseEnded?.Invoke();
				TryNotifyTopContentShown();
				RRDataTrack.Instance.ReportBoardPropClick(currentPropType);
			}
		}
	}

	private void UnbindGuidePropItemClick()
	{
		if ((Object)(object)boundPropItemView?.btnProp != (Object)null && onPropItemClick != null)
		{
			((UnityEvent)((Button)boundPropItemView.btnProp).onClick).RemoveListener(onPropItemClick);
		}
		boundPropItemView = null;
		onPropItemClick = null;
	}

	private static UIPropsView TryGetUIGamePropsView()
	{
		if (!TryGetUIGameController(out var gameController))
		{
			return null;
		}
		return gameController.View.PropsView;
	}

	private void MaskUIGamePropItem(EPropType propType)
	{
		if (propType != EPropType.None)
		{
			UIPropsView uIPropsView = TryGetUIGamePropsView();
			if (uIPropsView != null)
			{
				uIPropsView.SetPropItemCanvasAlpha(propType, 0f);
				maskedUIGamePropType = propType;
			}
		}
	}

	private void RestoreUIGamePropItem()
	{
		if (maskedUIGamePropType != EPropType.None)
		{
			TryGetUIGamePropsView()?.SetPropItemCanvasAlpha(maskedUIGamePropType, 1f);
			maskedUIGamePropType = EPropType.None;
		}
	}

	public void PlayOpenAnimation(bool isFromUnlockView)
	{
		string animName = (isFromUnlockView ? "anim_UIPropGuide_open_nochange" : "anim_UIPropGuide_open_change");
		PlayAnimation(UIPropGuideAni, animName);
	}

	public float PlayCloseAnimation()
	{
		float num = PlayAnimation(UIPropGuideAni, "anim_UIPropGuide_close");
		if ((Object)(object)TopContent != (Object)null && ((Component)TopContent).gameObject.activeSelf)
		{
			float num2 = PlayAnimation(TopContentAni, "anim_SingleToast_close");
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public float PlayItemTipAnimation(bool open)
	{
		return PlayAnimation(ItemTipAni, open ? "anim_SingleToast_open" : "anim_SingleToast_close");
	}

	public float PlayTopContentAnimation(bool open)
	{
		return PlayAnimation(TopContentAni, open ? "anim_SingleToast_open" : "anim_SingleToast_close");
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

	private static bool TryGetUIGameController(out UIGameController gameController)
	{
		gameController = null;
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return false;
		}
		gameController = uIGameController;
		return true;
	}

	private static bool TryValidateInitData(PropGuideOpenData data, out PropModelProxy propProxy)
	{
		propProxy = null;
		if (data == null || data.PropType == EPropType.None)
		{
			return false;
		}
		propProxy = GameApp.Proxy.Get<PropModelProxy>();
		return propProxy != null;
	}

	private void ResetItemTipSession(PropGuideOpenData data)
	{
		isWaitingItemTipConfirm = data.IsFirstGuide;
		onItemTipConfirmed = data.OnItemTipConfirmed;
		fireOnItemTipConfirm = data.FireOnItemTipConfirm;
	}

	private void TryNotifyTopContentShown()
	{
		if ((Object)(object)TopContent != (Object)null && ((Component)TopContent).gameObject.activeSelf)
		{
			OnTopContentShown?.Invoke();
		}
	}

	private void ResetSessionState()
	{
		if (itemTipHideTimerId >= 0)
		{
			GameApp.Timer.Kill(itemTipHideTimerId);
			itemTipHideTimerId = -1;
		}
		OnTopContentShown = null;
		OnItemTipPhaseEnded = null;
		onItemTipConfirmed = null;
		isWaitingItemTipConfirm = false;
		fireOnItemTipConfirm = false;
		maskedUIGamePropType = EPropType.None;
		currentPropType = EPropType.None;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIPropGuideAni = behaviour.Get<Animation>("UIPropGuideAni");
		Panel = behaviour.Get<RectTransform>("Panel");
		TopContent = behaviour.Get<RectTransform>("TopContent");
		TopContentAni = behaviour.Get<Animation>("TopContentAni");
		TxtPropName = behaviour.Get<TMP_Text>("TxtPropName");
		TxtUnlockProp = behaviour.Get<TMP_Text>("TxtUnlockProp");
		ImgProp = behaviour.Get<Image>("ImgProp");
		ImgRingPreview = behaviour.Get<RawImage>("ImgRingPreview");
		Btnclose = behaviour.Get<Button>("Btnclose");
		BtnUse = behaviour.Get<Button>("BtnUse");
		Mask = behaviour.Get<Button>("Mask");
		ItemTip = behaviour.Get<RectTransform>("ItemTip");
		ItemTipAni = behaviour.Get<Animation>("ItemTipAni");
		TxtItemTip = behaviour.Get<TMP_Text>("TxtItemTip");
	}
}
