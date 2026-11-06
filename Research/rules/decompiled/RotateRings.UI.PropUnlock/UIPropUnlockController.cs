using System;
using System.Collections.Generic;
using Game.UI;
using RotateRings.Common;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Game;
using RotateRings.UI.PropItem;
using RotateRings.UI.Props;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vibration;
using XY.Common.ParticleImage;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.PropUnlock;

[UIPath("UIPropUnlock")]
public class UIPropUnlockController : GameBaseController<UIPropUnlockView>
{
	private const string Tag = "UI";

	private const string EffectLocation = "eff_tool_fly_01";

	private const int MaxEmitCount = 6;

	private const float UnlockFlowLockDuration = 20f;

	private const float FlyPresentationCompleteDelay = 0.5f;

	private PropUnlockOpenData openData;

	private bool closedCallbackInvoked;

	private bool isClosing;

	private bool isPresentationCompleted;

	private int closeTimerId = -1;

	private bool isUnlockFlowUiLocked;

	private bool hasReceivePresentationStarted;

	private bool isFlyPresentationDelayPending;

	private int flyPresentationTimerId = -1;

	private float closeAnimDuration;

	private float closeAnimStartTime;

	private GameObject flyEffectPrefab;

	private readonly List<GameObject> activeFlyEffects = new List<GameObject>();

	public UIPropUnlockController(UIPropUnlockView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		openData = param as PropUnlockOpenData;
		closedCallbackInvoked = false;
		isClosing = false;
		isPresentationCompleted = false;
		hasReceivePresentationStarted = false;
		isFlyPresentationDelayPending = false;
		closeTimerId = -1;
		flyPresentationTimerId = -1;
		closeAnimDuration = 0f;
		closeAnimStartTime = 0f;
		((BaseController<UIPropUnlockView>)this).view.Init(openData);
		if (openData != null)
		{
			((BaseController<UIPropUnlockView>)this).view.SetupUnlockPropItem(openData);
		}
		AudioService.PlayUiSound(AudioKeys.ItemHammerGet.Value);
		if ((Object)(object)((BaseController<UIPropUnlockView>)this).view.MaskBtn != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropUnlockView>)this).view.MaskBtn.onClick).AddListener(new UnityAction(OnMaskBtnClick));
		}
		if ((Object)(object)((BaseController<UIPropUnlockView>)this).view.ContinueBtn != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropUnlockView>)this).view.ContinueBtn.onClick).AddListener(new UnityAction(OnMaskBtnClick));
		}
		((BaseController<UIPropUnlockView>)this).view.PlayOpenAnimation();
	}

	public override void OnRemove()
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		if (closeTimerId > 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
		if (flyPresentationTimerId > 0)
		{
			GameApp.Timer.Kill(flyPresentationTimerId);
			flyPresentationTimerId = -1;
		}
		isFlyPresentationDelayPending = false;
		if ((Object)(object)((BaseController<UIPropUnlockView>)this).view.MaskBtn != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropUnlockView>)this).view.MaskBtn.onClick).RemoveListener(new UnityAction(OnMaskBtnClick));
		}
		if ((Object)(object)((BaseController<UIPropUnlockView>)this).view.ContinueBtn != (Object)null)
		{
			((UnityEvent)((BaseController<UIPropUnlockView>)this).view.ContinueBtn.onClick).RemoveListener(new UnityAction(OnMaskBtnClick));
		}
		RecycleAllFlyEffects();
		UnlockUI();
		((BaseController<UIPropUnlockView>)this).view.ClearUnlockPropItem();
		ReleaseHoldIfNeeded();
		InvokeClosedCallback();
		base.OnRemove();
	}

	public override void OnEscape()
	{
		ClosePopup();
	}

	private void OnMaskBtnClick()
	{
		AudioService.PlayUiSound(AudioKeys.ItemConfirm.Value);
		ClosePopup();
	}

	private void ClosePopup()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		if (openData == null || !openData.NeedFlyPresentation)
		{
			SchedulePresentationComplete(((BaseController<UIPropUnlockView>)this).view.PlayCloseAnimation());
			return;
		}
		LockUI();
		closeAnimDuration = ((BaseController<UIPropUnlockView>)this).view.PlayCloseAnimation();
		closeAnimStartTime = Time.time;
		((BaseController<UIPropUnlockView>)this).view.SetUnlockPropItemVisible(visible: true);
		if (!TryPlayFlyReward(openData))
		{
			CompletePresentationWithoutFly(openData);
		}
	}

	private float GetRemainingCloseAnimDuration()
	{
		if (closeAnimDuration <= 0f)
		{
			return 0f;
		}
		return Mathf.Max(0f, closeAnimDuration - (Time.time - closeAnimStartTime));
	}

	private void SchedulePresentationComplete(float closeDuration)
	{
		if (closeTimerId > 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
		if (closeDuration > 0f)
		{
			closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIPropUnlockView>)this).view).Behaviour, closeDuration, (Action)OnPresentationComplete);
		}
		else
		{
			OnPresentationComplete();
		}
	}

	private void OnPresentationComplete()
	{
		if (!isPresentationCompleted && !isFlyPresentationDelayPending)
		{
			isPresentationCompleted = true;
			if (closeTimerId > 0)
			{
				GameApp.Timer.Kill(closeTimerId);
				closeTimerId = -1;
			}
			RecycleAllFlyEffects();
			UnlockUI();
			((BaseController<UIPropUnlockView>)this).view.ClearUnlockPropItem();
			InvokeClosedCallback();
			((BaseController<UIPropUnlockView>)this).CloseView(false);
		}
	}

	private void InvokeClosedCallback()
	{
		if (!closedCallbackInvoked)
		{
			closedCallbackInvoked = true;
			openData?.OnClosed?.Invoke();
		}
	}

	private bool TryPlayFlyReward(PropUnlockOpenData data)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return false;
		}
		UIPropItemView propItemView = ((BaseController<UIPropUnlockView>)this).view.TryGetUnlockPropItemView();
		if ((Object)(object)propItemView?.Icon == (Object)null)
		{
			return false;
		}
		RectTransform panel = ((BaseController<UIPropUnlockView>)this).view.Panel;
		if ((Object)(object)panel == (Object)null || !((BaseController<UIPropUnlockView>)this).view.TryGetFlyEffectLocalPosition(out var localPosition))
		{
			return false;
		}
		if (data.GrantCount <= 0)
		{
			return false;
		}
		Transform rectTransform = (Transform)(object)((Graphic)propItemView.Icon).rectTransform;
		string loadAssetGroup = ((BaseController<UIPropUnlockView>)this).view.LoadAssetGroup;
		if (flyEffectPrefab == null)
		{
			flyEffectPrefab = GameApp.Res.LoadAsset<GameObject>("eff_tool_fly_01", loadAssetGroup);
		}
		if ((Object)(object)flyEffectPrefab == (Object)null)
		{
			return false;
		}
		GameObject instance = GameApp.Pool.Spawn(flyEffectPrefab, (Transform)(object)panel);
		if ((Object)(object)instance == (Object)null)
		{
			return false;
		}
		Transform transform = instance.transform;
		RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (val != null)
		{
			val.anchoredPosition = localPosition;
		}
		else
		{
			transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
		}
		transform.localRotation = Quaternion.identity;
		transform.localScale = flyEffectPrefab.transform.localScale;
		instance.SetActive(false);
		activeFlyEffects.Add(instance);
		XYParticleImage component = instance.GetComponent<XYParticleImage>();
		if ((Object)(object)component == (Object)null)
		{
			RecycleFlyEffect(instance);
			return false;
		}
		component.PlayMode = PlayMode.None;
		if (!string.IsNullOrEmpty(data.FlyIconName))
		{
			component.LoadRuntimeSprite(data.FlyIconName);
		}
		instance.SetActive(true);
		component.Stop(stopAndClear: true);
		((BaseController<UIPropUnlockView>)this).view.ReAlignUnlockPropItem(data.PropType);
		int emitCount = Mathf.Clamp(data.GrantCount, 1, 6);
		component.Play(emitCount, rectTransform, delegate
		{
			OnFirstParticleFinished(data, propItemView);
		}, delegate
		{
			OnParticleArrived(emitCount);
		}, delegate
		{
			OnLastParticleFinished(instance, data);
		});
		if (!component.isPlaying && !component.isEmitting)
		{
			RecycleFlyEffect(instance);
			return false;
		}
		return true;
	}

	private void OnFirstParticleFinished(PropUnlockOpenData data, UIPropItemView propItemView)
	{
		try
		{
			if (!isPresentationCompleted && !hasReceivePresentationStarted && data != null && propItemView != null)
			{
				hasReceivePresentationStarted = true;
				propItemView.PlayReceivePresentation();
			}
		}
		catch (Exception ex)
		{
			Log.LogError("UI", "[UIPropUnlockController] 首粒子回调异常: {0}", new object[1] { ex });
			CompletePresentationWithoutFly(data);
		}
	}

	private void OnParticleArrived(int emitCount)
	{
		try
		{
			if (!isPresentationCompleted)
			{
				VibrateUtility.VibrateRewardFlyInOnArrive($"UIPropUnlock-解锁新道具飞入道具栏 emitCount={emitCount}");
			}
		}
		catch (Exception ex)
		{
			Log.LogError("UI", "[UIPropUnlockController] 粒子抵达震动异常: {0}", new object[1] { ex });
		}
	}

	private void OnLastParticleFinished(GameObject instance, PropUnlockOpenData data)
	{
		try
		{
			if (isPresentationCompleted || data == null)
			{
				RecycleFlyEffect(instance);
				return;
			}
			AudioService.PlayUiSound(AudioKeys.ItemAdd.Value);
			ScheduleFlyPresentationComplete(data);
		}
		catch (Exception ex)
		{
			Log.LogError("UI", "[UIPropUnlockController] 末粒子回调异常: {0}", new object[1] { ex });
			RecycleFlyEffect(instance);
			CompletePresentationWithoutFly(data);
		}
	}

	private void ScheduleFlyPresentationComplete(PropUnlockOpenData data)
	{
		isFlyPresentationDelayPending = true;
		if (flyPresentationTimerId > 0)
		{
			GameApp.Timer.Kill(flyPresentationTimerId);
		}
		flyPresentationTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIPropUnlockView>)this).view).Behaviour, 0.5f, (Action)delegate
		{
			try
			{
				flyPresentationTimerId = -1;
				isFlyPresentationDelayPending = false;
				OnFlyPresentationComplete(data);
			}
			catch (Exception ex)
			{
				Log.LogError("UI", "[UIPropUnlockController] 飞行演出延迟收尾异常: {0}", new object[1] { ex });
				isFlyPresentationDelayPending = false;
				CompletePresentationWithoutFly(data);
			}
		});
	}

	private void OnFlyPresentationComplete(PropUnlockOpenData data)
	{
		EndHold(data.PropType);
		TryOpenUnlockIntro(data.PropType, ((BaseController<UIPropUnlockView>)this).view.TryGetUnlockPropItemView());
		SchedulePresentationComplete(GetRemainingCloseAnimDuration());
	}

	private static void TryOpenUnlockIntro(EPropType propType, UIPropItemView propItemView)
	{
		if (ShouldOpenUnlockIntro(propType) && propItemView != null)
		{
			RectTransform guideSourceAnchor = propItemView.GetGuideSourceAnchor();
			if (!((Object)(object)guideSourceAnchor == (Object)null))
			{
				GameApp.Proxy.Get<PropModelProxy>()?.EnsurePendingFreeProp(propType);
				GameApp.Event.Send<PropUseEvent>(new PropUseEvent
				{
					PropType = propType,
					SourceAnchor = guideSourceAnchor,
					EntrySource = PropUseEntrySource.UnlockIntro,
					IsFromUnlockView = true
				});
			}
		}
	}

	private static bool ShouldOpenUnlockIntro(EPropType propType)
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy != null && propModelProxy.IsInGameProp((int)propType))
		{
			return propModelProxy.IsPropFirstUse(propType);
		}
		return false;
	}

	private void ReleaseHoldIfNeeded()
	{
		if (!isPresentationCompleted && openData != null && openData.NeedFlyPresentation)
		{
			EndHold(openData.PropType);
		}
	}

	private void CompletePresentationWithoutFly(PropUnlockOpenData data)
	{
		EndHold(data.PropType);
		((BaseController<UIPropUnlockView>)this).view.TryGetUnlockPropItemView()?.RefreshProp(data.PropType);
		SchedulePresentationComplete(GetRemainingCloseAnimDuration());
	}

	private static void EndHold(EPropType propType, bool refreshFromData = true)
	{
		if (!TryGetUIGameController(out var gameController))
		{
			return;
		}
		UIPropsView uIPropsView = gameController.View?.PropsView;
		if (uIPropsView != null)
		{
			uIPropsView.SetPresentationLocked(propType, locked: false);
			if (refreshFromData)
			{
				uIPropsView.RefreshProp(propType);
			}
		}
	}

	private void RecycleFlyEffect(GameObject instance)
	{
		if (!((Object)(object)instance == (Object)null))
		{
			activeFlyEffects.Remove(instance);
			XYParticleImage component = instance.GetComponent<XYParticleImage>();
			if ((Object)(object)component != (Object)null)
			{
				component.Stop(stopAndClear: true);
			}
			GameApp.Pool.Recycle(instance);
		}
	}

	private void RecycleAllFlyEffects()
	{
		if (activeFlyEffects.Count == 0)
		{
			return;
		}
		for (int num = activeFlyEffects.Count - 1; num >= 0; num--)
		{
			GameObject val = activeFlyEffects[num];
			if (!((Object)(object)val == (Object)null))
			{
				XYParticleImage component = val.GetComponent<XYParticleImage>();
				if ((Object)(object)component != (Object)null)
				{
					component.Stop(stopAndClear: true);
				}
				GameApp.Pool.Recycle(val);
			}
		}
		activeFlyEffects.Clear();
	}

	private void LockUI()
	{
		if (!isUnlockFlowUiLocked)
		{
			isUnlockFlowUiLocked = true;
			LockUIService.Enable(20f);
		}
	}

	private void UnlockUI()
	{
		if (isUnlockFlowUiLocked)
		{
			isUnlockFlowUiLocked = false;
			LockUIService.Disable();
		}
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
}
