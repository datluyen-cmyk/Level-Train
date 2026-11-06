using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Example.Cfg;
using Game.DataTrack;
using Game.UI;
using Game.UI.Common;
using RotateRings.Ad;
using RotateRings.GamePlay.Events;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Toast;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.PropItem;

public class UIPropItemView : GameWidgetView
{
	private const float DefaultScrollDuration = 0.15f;

	private const int MaxDisplayCount = 99;

	private const string MaxDisplayCountText = "99+";

	private const string PropItemReceiveAnimName = "anim_PropItem_receive";

	private const string PropItemIdleAnimName = "anim_PropItem_idle";

	private const string ButtonPromptEffectLocation = "eff_button_prompt";

	private const float ButtonPromptHideDuration = 1f;

	private const float AppearFadeDuration = 0.35f;

	private bool isClickBound;

	private bool isSessionEventBound;

	private bool presentationLocked;

	private bool pendingOpenGuide;

	private Action scrollCompleteCallback;

	private bool isPropUsing;

	private Tween countTween;

	private Tween appearFadeTween;

	private GameObject buttonPromptPrefab;

	private GameObject buttonPromptTarget;

	private int buttonPromptHideTimerId = -1;

	private bool reportedAdBadgeShow;

	private const string GrayShaderName = "UI/Grayscale";

	private static Material grayMaterial;

	private bool isGrayed;

	private Material iconOriginalMaterial;

	private IPropBadgeProvider badgeProvider = DefaultBadgeProvider;

	private static readonly IPropBadgeProvider DefaultBadgeProvider = new DefaultPropBadgeProvider();

	public RectTransform Node;

	public RectTransform Bg;

	public Image Icon;

	public RectTransform Badges;

	public RectTransform badge_ad;

	public RectTransform badge_free;

	public TMP_Text txtbadgeFree_num;

	public RectTransform badge_num;

	public RectTransform badge_lock;

	public TMP_Text txtbadge_num;

	public CanvasGroup CanvasGroup;

	public XYButton btnProp;

	public Animation PropItemAni;

	public EPropType PropType { get; private set; }

	protected bool IsPropUsing => isPropUsing;

	protected bool IsCountTweenActive => countTween != null;

	public override void Dispose()
	{
		StopCountTween();
		StopAppearFade(resetAlpha: true);
		StopPropItemIdleAni();
		RecycleButtonPromptEffect();
		UnbindEvents();
		RestoreGray();
		iconOriginalMaterial = null;
		base.Dispose();
	}

	public void BindEvents()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if ((Object)(object)btnProp != (Object)null && !isClickBound)
		{
			((UnityEvent)((Button)btnProp).onClick).AddListener(new UnityAction(OnPropClick));
			isClickBound = true;
		}
		if (!isSessionEventBound)
		{
			GameApp.Event.Listen<PropUseSessionStateChangedEvent>((Action<PropUseSessionStateChangedEvent>)OnPropUseSessionStateChanged);
			isSessionEventBound = true;
		}
	}

	private void UnbindEvents()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (isClickBound && (Object)(object)btnProp != (Object)null)
		{
			((UnityEvent)((Button)btnProp).onClick).RemoveListener(new UnityAction(OnPropClick));
			isClickBound = false;
		}
		if (isSessionEventBound)
		{
			GameApp.Event.Unlisten<PropUseSessionStateChangedEvent>((Action<PropUseSessionStateChangedEvent>)OnPropUseSessionStateChanged);
			isSessionEventBound = false;
		}
	}

	protected virtual void OnPropClick()
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null || isPropUsing || countTween != null || PropType == EPropType.None || presentationLocked)
		{
			return;
		}
		if (!propModelProxy.IsPropUnlock(PropType))
		{
			ShowUnlockLevelToast(propModelProxy);
			return;
		}
		RectTransform guideSourceAnchor = GetGuideSourceAnchor();
		if (!((Object)(object)guideSourceAnchor == (Object)null))
		{
			RRDataTrack.Instance.ReportBoardPropClick(PropType);
			GameApp.Event.Send<PropUseEvent>(new PropUseEvent
			{
				PropType = PropType,
				SourceAnchor = guideSourceAnchor
			});
		}
	}

	private void OnPropUseSessionStateChanged(PropUseSessionStateChangedEvent eventData)
	{
		if (eventData != null)
		{
			isPropUsing = eventData.IsUsing;
		}
	}

	public void RefreshProp(EPropType propType)
	{
		PropType = propType;
		if (propType != EPropType.None && GameApp.Proxy.Get<PropModelProxy>() != null)
		{
			SetIcon(PropIconService.GetIconName(propType));
			StopCountTween();
			Refresh();
		}
	}

	public void Refresh()
	{
		PropBadgeState? state = badgeProvider?.GetState(PropType);
		if (state.HasValue)
		{
			RefeshCountText(state.Value.DisplayCount);
		}
		RefreshBadges(state);
	}

	public void SetBadgeProvider(IPropBadgeProvider provider)
	{
		badgeProvider = provider ?? DefaultBadgeProvider;
		Refresh();
	}

	public void PlayReceivePresentation()
	{
		PlayButtonPromptEffect();
	}

	public void PlayAppearFadeIn(float duration = 0.35f, Action onComplete = null)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		if ((Object)(object)CanvasGroup == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		StopAppearFade();
		GameObject gameObject = ((Component)CanvasGroup).gameObject;
		CanvasGroup.alpha = 0f;
		appearFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetLink<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(CanvasGroup, 1f, duration), (Ease)6), (object)gameObject), gameObject, (LinkBehaviour)5), (TweenCallback)delegate
		{
			appearFadeTween = null;
			if (onComplete == null)
			{
				return;
			}
			try
			{
				onComplete();
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[UIPropItemView] 渐显完成回调异常: {0}", new object[1] { ex });
			}
		});
	}

	public void StopAppearFade(bool resetAlpha = false)
	{
		if (appearFadeTween != null)
		{
			TweenExtensions.Kill(appearFadeTween, false);
			appearFadeTween = null;
		}
		if (resetAlpha && (Object)(object)CanvasGroup != (Object)null)
		{
			CanvasGroup.alpha = 1f;
		}
	}

	public void ApplyScroll(EPropType propType, int fromCount, int toCount, float duration, bool openGuideOnComplete = false, Action onComplete = null)
	{
		PropType = propType;
		pendingOpenGuide = openGuideOnComplete;
		scrollCompleteCallback = onComplete;
		ScrollCount(fromCount, toCount, duration);
	}

	public RectTransform GetGuideSourceAnchor()
	{
		return ((UIBaseView)this).NodeRect ?? Node;
	}

	public void SetPresentationLocked(bool locked)
	{
		presentationLocked = locked;
		Refresh();
	}

	public void SetBadgesVisible(bool visible)
	{
		if ((Object)(object)Badges != (Object)null)
		{
			((Component)Badges).gameObject.SetActive(visible);
		}
	}

	public void SetGray(bool grayed)
	{
		if (isGrayed != grayed)
		{
			isGrayed = grayed;
			if (grayed)
			{
				ApplyGray();
			}
			else
			{
				RestoreGray();
			}
		}
	}

	private void ApplyGray()
	{
		if (!((Object)(object)Icon == (Object)null))
		{
			Material val = GetGrayMaterial();
			if (!((Object)(object)val == (Object)null))
			{
				iconOriginalMaterial = ((Graphic)Icon).material;
				((Graphic)Icon).material = val;
			}
		}
	}

	private void RestoreGray()
	{
		if (!((Object)(object)Icon == (Object)null) && !((Object)(object)iconOriginalMaterial == (Object)null))
		{
			((Graphic)Icon).material = iconOriginalMaterial;
			iconOriginalMaterial = null;
		}
	}

	private static Material GetGrayMaterial()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		if ((Object)(object)grayMaterial != (Object)null)
		{
			return grayMaterial;
		}
		Shader val = Shader.Find("UI/Grayscale");
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("UI", "[UIPropItemView] 置灰着色器未找到: {0}", new object[1] { "UI/Grayscale" });
			return null;
		}
		grayMaterial = new Material(val);
		return grayMaterial;
	}

	private void SetIcon(string iconName)
	{
		if (!((Object)(object)Icon == (Object)null))
		{
			SetSprite(Icon, iconName);
		}
	}

	private void ShowUnlockLevelToast(PropModelProxy propProxy)
	{
		PropConfig propConfig = propProxy.GetPropConfig(PropType);
		if (propConfig != null)
		{
			ToastService.Show(GameApp.Lang.GetText("Booster_UnlockPrompt", new object[1] { propProxy.GetPropUnlockLv(propConfig) }));
		}
	}

	public void ShowCount(int count)
	{
		StopCountTween();
		RefeshCountText(count);
		RefreshBadges(badgeProvider?.GetState(PropType, count));
	}

	private void ScrollCount(int fromCount, int toCount, float duration)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Expected O, but got Unknown
		StopCountTween();
		if (fromCount == toCount)
		{
			ShowCount(toCount);
			TrySendUnlockIntroUseRequest();
			InvokeScrollCompleteCallback();
			return;
		}
		if ((Object)(object)txtbadge_num == (Object)null)
		{
			ShowCount(toCount);
			TrySendUnlockIntroUseRequest();
			InvokeScrollCompleteCallback();
			return;
		}
		if (duration <= 0f)
		{
			duration = 0.15f;
		}
		RefeshCountText(fromCount);
		PlayButtonPromptEffect();
		float current = fromCount;
		countTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetTarget<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => current), (DOSetter<float>)delegate(float value)
		{
			current = value;
			int count = Mathf.RoundToInt(current);
			RefeshCountText(count);
		}, (float)toCount, duration), (Ease)1), (object)(((Object)(object)((UIBaseView)this).Behaviour != (Object)null) ? ((Component)((UIBaseView)this).Behaviour).gameObject : null)), (TweenCallback)delegate
		{
			OnCountScrollComplete(toCount);
		});
		RefreshBadges(badgeProvider?.GetState(PropType, fromCount));
	}

	private void OnCountScrollComplete(int toCount)
	{
		RefeshCountText(toCount);
		countTween = null;
		RefreshBadges(badgeProvider?.GetState(PropType, toCount));
		TrySendUnlockIntroUseRequest();
		InvokeScrollCompleteCallback();
	}

	private void InvokeScrollCompleteCallback()
	{
		if (scrollCompleteCallback == null)
		{
			return;
		}
		Action action = scrollCompleteCallback;
		scrollCompleteCallback = null;
		try
		{
			action();
		}
		catch (Exception ex)
		{
			Log.LogError("UI", "[UIPropItemView] 滚动完成回调异常: {0}", new object[1] { ex });
		}
	}

	private void RefeshCountText(int count)
	{
		if ((Object)(object)txtbadge_num != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtbadge_num, (count > 99) ? "99+" : count.ToString());
		}
	}

	private void StopCountTween()
	{
		if (countTween == null)
		{
			scrollCompleteCallback = null;
			return;
		}
		TweenExtensions.Kill(countTween, false);
		countTween = null;
		scrollCompleteCallback = null;
	}

	private void PlayButtonPromptEffect()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		PlayPropItemReceiveAni();
		if ((Object)(object)Node == (Object)null)
		{
			return;
		}
		if (buttonPromptPrefab == null)
		{
			buttonPromptPrefab = LoadAsset<GameObject>("eff_button_prompt");
		}
		if (!((Object)(object)buttonPromptPrefab == (Object)null))
		{
			RecycleButtonPromptEffect();
			buttonPromptTarget = GameApp.Pool.Spawn(buttonPromptPrefab, (Transform)(object)Node);
			if (!((Object)(object)buttonPromptTarget == (Object)null))
			{
				Transform transform = buttonPromptTarget.transform;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				buttonPromptHideTimerId = GameApp.Timer.Delay(1f, (Action)RecycleButtonPromptEffect);
			}
		}
	}

	private void PlayPropItemReceiveAni()
	{
		if (!((Object)(object)PropItemAni == (Object)null) && !((Object)(object)PropItemAni.GetClip("anim_PropItem_receive") == (Object)null))
		{
			PropItemAni.Stop();
			PropItemAni.Rewind("anim_PropItem_receive");
			PropItemAni.Sample();
			PropItemAni.Play("anim_PropItem_receive", (PlayMode)4);
		}
	}

	public void PlayPropItemIdleAni()
	{
		if (!((Object)(object)PropItemAni == (Object)null) && !((Object)(object)PropItemAni.GetClip("anim_PropItem_idle") == (Object)null))
		{
			PropItemAni.Stop();
			PropItemAni.Rewind("anim_PropItem_idle");
			PropItemAni.Sample();
			PropItemAni.Play("anim_PropItem_idle", (PlayMode)4);
		}
	}

	public void StopPropItemIdleAni()
	{
		if (!((Object)(object)PropItemAni == (Object)null) && !((Object)(object)PropItemAni.GetClip("anim_PropItem_idle") == (Object)null))
		{
			if (PropItemAni.IsPlaying("anim_PropItem_idle"))
			{
				PropItemAni.Stop("anim_PropItem_idle");
			}
			AnimationState val = PropItemAni["anim_PropItem_idle"];
			if ((TrackedReference)(object)val != (TrackedReference)null)
			{
				val.time = 0f;
				PropItemAni.Sample();
			}
		}
	}

	public float TryGetPropItemIdleAnimDuration()
	{
		if ((Object)(object)PropItemAni == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = PropItemAni.GetClip("anim_PropItem_idle");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	private void RecycleButtonPromptEffect()
	{
		if (buttonPromptHideTimerId >= 0)
		{
			GameApp.Timer.Kill(buttonPromptHideTimerId);
			buttonPromptHideTimerId = -1;
		}
		if (!((Object)(object)buttonPromptTarget == (Object)null))
		{
			GameApp.Pool.Recycle(buttonPromptTarget);
			buttonPromptTarget = null;
		}
	}

	private void RefreshBadges(PropBadgeState? state = null)
	{
		bool num = !state.HasValue || state.Value.IsLocked || PropType == EPropType.None || presentationLocked;
		bool flag = countTween != null;
		bool flag2 = state.HasValue && state.Value.HasPendingFree;
		HideAllBadges();
		if (num)
		{
			ShowBadge(badge_lock);
			return;
		}
		if (flag)
		{
			ShowBadge(badge_num);
			return;
		}
		if (flag2)
		{
			ShowBadge(badge_free);
			if ((Object)(object)txtbadgeFree_num != (Object)null)
			{
				TMPTextExtensions.SetLocalizedText(txtbadgeFree_num, "Common_Free", Array.Empty<object>());
			}
			return;
		}
		if (state.HasValue && state.Value.DisplayCount > 0)
		{
			reportedAdBadgeShow = false;
			ShowBadge(badge_num);
			return;
		}
		ShowBadge(badge_ad);
		if (!reportedAdBadgeShow && PropType != EPropType.None)
		{
			reportedAdBadgeShow = true;
			AdPopPosition adPopPosition = ((PropType != EPropType.Skyrocket) ? AdPopPosition.MidRewardHammer : AdPopPosition.MidRewardRocket);
			RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Show, adPopPosition);
		}
	}

	private void HideAllBadges()
	{
		if ((Object)(object)badge_lock != (Object)null)
		{
			((Component)badge_lock).gameObject.SetActive(false);
		}
		if ((Object)(object)badge_free != (Object)null)
		{
			((Component)badge_free).gameObject.SetActive(false);
		}
		if ((Object)(object)badge_num != (Object)null)
		{
			((Component)badge_num).gameObject.SetActive(false);
		}
		if ((Object)(object)badge_ad != (Object)null)
		{
			((Component)badge_ad).gameObject.SetActive(false);
		}
	}

	private static void ShowBadge(RectTransform badge)
	{
		if ((Object)(object)badge != (Object)null)
		{
			((Component)badge).gameObject.SetActive(true);
		}
	}

	private void TrySendUnlockIntroUseRequest()
	{
		if (!pendingOpenGuide)
		{
			return;
		}
		pendingOpenGuide = false;
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null || PropType == EPropType.None || !propModelProxy.IsPropFirstUse(PropType))
		{
			NotifyUnlockPresentationComplete();
			return;
		}
		RectTransform guideSourceAnchor = GetGuideSourceAnchor();
		if ((Object)(object)guideSourceAnchor == (Object)null)
		{
			NotifyUnlockPresentationComplete();
			return;
		}
		GameApp.Event.Send<PropUseEvent>(new PropUseEvent
		{
			PropType = PropType,
			SourceAnchor = guideSourceAnchor,
			EntrySource = PropUseEntrySource.UnlockIntro
		});
	}

	private void NotifyUnlockPresentationComplete()
	{
		GameApp.Event.Send<PropUnlockPresentationCompleteEvent>(new PropUnlockPresentationCompleteEvent());
		GameApp.Event.Send<PropUnlockTutorialBlockingUiClosedEvent>(new PropUnlockTutorialBlockingUiClosedEvent());
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		Node = behaviour.Get<RectTransform>("Node");
		Bg = behaviour.Get<RectTransform>("Bg");
		Icon = behaviour.Get<Image>("Icon");
		Badges = behaviour.Get<RectTransform>("Badges");
		badge_ad = behaviour.Get<RectTransform>("badge_ad");
		badge_free = behaviour.Get<RectTransform>("badge_free");
		txtbadgeFree_num = behaviour.Get<TMP_Text>("txtbadgeFree_num");
		badge_num = behaviour.Get<RectTransform>("badge_num");
		badge_lock = behaviour.Get<RectTransform>("badge_lock");
		txtbadge_num = behaviour.Get<TMP_Text>("txtbadge_num");
		CanvasGroup = behaviour.Get<CanvasGroup>("CanvasGroup");
		btnProp = behaviour.Get<XYButton>("btnProp");
		PropItemAni = behaviour.Get<Animation>("PropItemAni");
	}
}
