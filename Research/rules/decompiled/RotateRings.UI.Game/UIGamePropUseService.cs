using System;
using System.Collections.Generic;
using Game.Level;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services.LevelStepLimit;
using RotateRings.GamePlay.Services.Prop;
using RotateRings.Prop;
using RotateRings.UI.Prop;
using RotateRings.UI.PropGuide;
using RotateRings.UI.Shop;
using RotateRings.UI.Toast;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public sealed class UIGamePropUseService : UIGameBaseService, IPropUseServiceHost, IRocketPropUseCallback, IRotateRingPropUseCallback
{
	private const string Tag = "[UIGame][PropUseService] ";

	private bool isUsingByAd;

	private EPropType currentPropType;

	private TrackPropUseReason currentUseReason;

	private PropUseEntrySource currentEntrySource;

	private RectTransform currentSourceAnchor;

	private bool hasConsumed;

	private bool isEndingUse;

	private bool suppressGuideClosedCallback;

	private readonly Dictionary<EPropType, IPropUseFlowStrategy> flowStrategies = new Dictionary<EPropType, IPropUseFlowStrategy>();

	private SkyrocketPropUseFlowStrategy skyrocketStrategy;

	private SkyrocketPropBadgeProvider skyrocketBadgeProvider;

	public bool IsPropUsing
	{
		get
		{
			if (currentPropType == EPropType.None)
			{
				return isUsingByAd;
			}
			return true;
		}
	}

	public EPropType CurrentUsingPropType => currentPropType;

	public bool IsUsingByAd
	{
		get
		{
			return isUsingByAd;
		}
		set
		{
			isUsingByAd = value;
		}
	}

	public new UIGameView View => base.View;

	public SkyrocketPropBadgeProvider SkyrocketBadgeProvider => skyrocketBadgeProvider;

	public IRocketPropUseCallback Callback => this;

	public RectTransform PropGuidePanel
	{
		get
		{
			IController val = default(IController);
			if (!GameApp.UI.IsOpen("UIPropGuide", ref val) || !(val is UIPropGuideController uIPropGuideController))
			{
				return null;
			}
			return uIPropGuideController.View?.Panel;
		}
	}

	public bool HasPropConsumed => hasConsumed;

	public bool CanStartProp(EPropType propType)
	{
		RotateRingPropUseService rotateRingPropUseService = SingletonComponent<GameplayManager>.Instance?.PropUseService;
		IRotateRingPropUse rotateRingPropUse = rotateRingPropUseService?.TryGetHandler(propType);
		if (rotateRingPropUseService != null && rotateRingPropUse != null)
		{
			return rotateRingPropUse.CanStart(rotateRingPropUseService);
		}
		return false;
	}

	public void SendPropUsingState(EPropType propType, bool isUsing)
	{
		GameApp.Event.Send<PropUseSessionStateChangedEvent>(new PropUseSessionStateChangedEvent
		{
			PropType = propType,
			IsUsing = isUsing
		});
	}

	public void DismissPropGuideSilently()
	{
		if (GameApp.UI.IsOpen("UIPropGuide"))
		{
			suppressGuideClosedCallback = true;
			GameApp.UI.CloseView("UIPropGuide", (object)null, false);
			suppressGuideClosedCallback = false;
		}
	}

	public bool TryOpenPropGuide(PropGuideOpenData data)
	{
		if (data == null || (Object)(object)data.SourceAnchor == (Object)null)
		{
			return false;
		}
		GameApp.UI.Open("UIPropGuide", (UILayer)2, (object)data, (Action)null);
		return true;
	}

	public void ClosePropGuide(bool isSuccessUse = false)
	{
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIPropGuide", ref val) && val is UIPropGuideController uIPropGuideController)
		{
			uIPropGuideController.ClosePopup(isSuccessUse);
		}
	}

	public void PrepareForSettlement()
	{
		foreach (IPropUseFlowStrategy value in flowStrategies.Values)
		{
			value.PrepareForSettlement();
		}
	}

	public void OnFastFireGuideClosed(PropUseEntrySource entrySource)
	{
		if (!suppressGuideClosedCallback)
		{
			NotifyUnlockIntroCompleteIfNeeded(entrySource);
		}
	}

	public void NotifyUnlockIntroCompleteIfNeeded(PropUseEntrySource entrySource)
	{
		if (entrySource == PropUseEntrySource.UnlockIntro)
		{
			GameApp.Event.Send<PropUnlockPresentationCompleteEvent>(new PropUnlockPresentationCompleteEvent());
		}
	}

	public void PrepareFreePropIfNeeded()
	{
	}

	public void RequestFinish(RotateRingPropUseResult result, bool consumeOnSuccess)
	{
		if (currentPropType == EPropType.None || isEndingUse)
		{
			return;
		}
		switch (result)
		{
		case RotateRingPropUseResult.Success:
			if (consumeOnSuccess)
			{
				CompleteAndUsedProp();
			}
			else
			{
				CompleteUse();
			}
			break;
		case RotateRingPropUseResult.Cancel:
		case RotateRingPropUseResult.Failed:
			if (hasConsumed)
			{
				CompleteUse();
			}
			else
			{
				CancelUseProp();
			}
			break;
		}
	}

	public void OnRocketShotHit(TrackPropUseReason reason, bool success)
	{
		skyrocketStrategy?.OnRocketShotHit(reason, success);
	}

	public void OnFastFireSessionComplete()
	{
		skyrocketStrategy?.OnFastFireSessionComplete();
	}

	public UIGamePropUseService(UIGameController controller)
		: base(controller)
	{
		skyrocketStrategy = new SkyrocketPropUseFlowStrategy(this);
		flowStrategies[EPropType.Skyrocket] = skyrocketStrategy;
		skyrocketBadgeProvider = new SkyrocketPropBadgeProvider(skyrocketStrategy);
		View?.PropsView?.SetPropItemBadgeProvider(EPropType.Skyrocket, skyrocketBadgeProvider);
		GameApp.Event.Listen<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		GameApp.Event.Listen<PropUseEvent>((Action<PropUseEvent>)OnPropUse);
		GameApp.Event.Listen<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
		GameApp.Event.Listen<PropChangeEvent>((Action<PropChangeEvent>)OnPropChanged);
	}

	public override void Dispose()
	{
		GameApp.Event.Unlisten<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		GameApp.Event.Unlisten<PropUseEvent>((Action<PropUseEvent>)OnPropUse);
		GameApp.Event.Unlisten<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
		GameApp.Event.Unlisten<PropChangeEvent>((Action<PropChangeEvent>)OnPropChanged);
		CancelCurrentPropUseState();
		base.Dispose();
	}

	private void OnLevelChanged(LevelChanged eventData)
	{
		CancelCurrentPropUseState();
		foreach (IPropUseFlowStrategy value in flowStrategies.Values)
		{
			value.OnLevelChanged();
		}
	}

	private void OnRingSolved(RingSolvedEvent eventData)
	{
		foreach (IPropUseFlowStrategy value in flowStrategies.Values)
		{
			value.OnRingSolved();
		}
	}

	private void OnPropChanged(PropChangeEvent eventData)
	{
		if (eventData != null && flowStrategies.TryGetValue(eventData.PropType, out var value))
		{
			value.OnPropChanged(eventData);
		}
	}

	private void OnPropUse(PropUseEvent eventData)
	{
		if (eventData == null || (Object)(object)eventData.SourceAnchor == (Object)null || ((Object)(object)SingletonComponent<GameplayManager>.Instance != (Object)null && SingletonComponent<GameplayManager>.Instance.IsLevelCompleted))
		{
			return;
		}
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if ((levelStepLimitService != null && !levelStepLimitService.CanUseProp()) || IsPropUsing || GameApp.UI.IsOpen("UIPropAward"))
		{
			return;
		}
		if (eventData.PropType != EPropType.Skyrocket && skyrocketStrategy != null && skyrocketStrategy.HasPendingFastFire)
		{
			ToastService.Show(GameApp.Lang.GetText("FloatMsg_AnotherBooster", Array.Empty<object>()));
			NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy != null && propModelProxy.IsPropUnlock(eventData.PropType) && (!flowStrategies.TryGetValue(eventData.PropType, out var value) || !value.TryHandlePropUse(eventData)))
		{
			if (propModelProxy.HasPendingFreeProp(eventData.PropType))
			{
				BeginUseProp(eventData, propModelProxy.GetPendingFreePropUseReason(eventData.PropType));
			}
			else if (propModelProxy.GetPropCount(eventData.PropType) > 0)
			{
				BeginUseProp(eventData, TrackPropUseReason.Stock);
			}
			else
			{
				UsePropByAd(eventData);
			}
		}
	}

	internal void TryUsedProp(int cost = 1)
	{
		if (currentPropType != EPropType.None && !hasConsumed)
		{
			hasConsumed = true;
			RandomLevelFirstClearWindow.OnPropUsed();
			PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
			if (propModelProxy != null && propModelProxy.TryConsumePendingFreeProp(currentPropType))
			{
				propModelProxy.UseFreeProp(currentPropType, currentUseReason);
			}
			else
			{
				UseProp(currentPropType, currentUseReason, cost);
			}
		}
	}

	internal void CompleteAndUsedProp(int cost = 1)
	{
		TryUsedProp(cost);
		CompleteUse();
	}

	internal void CompleteUse()
	{
		if (currentPropType != EPropType.None)
		{
			if ((Object)(object)SingletonComponent<GameplayManager>.Instance != (Object)null)
			{
				SingletonComponent<GameplayManager>.Instance.GetGameService<LevelStepLimitService>()?.OnPropUsed();
			}
			EndUse(completed: true);
		}
	}

	internal void CancelUseProp()
	{
		if (currentPropType != EPropType.None)
		{
			EndUse(completed: false);
		}
	}

	public void UsePropByAd(PropUseEvent eventData)
	{
		UsePropByAd(eventData, null);
	}

	public void UsePropByAd(PropUseEvent eventData, Action onGrantOnlyComplete)
	{
		if (ShopService.ShowPopUIShop())
		{
			GameApp.UI.Open("UIGetItem", (UILayer)2, (object)new UIGetItemParam
			{
				PropType = eventData.PropType,
				OnClick = RequestRewardAd
			}, (Action)null);
		}
		else
		{
			RequestRewardAd();
		}
		void RequestRewardAd()
		{
			PropAdRewardService.RequestAcquireByAd(new PropAdRewardRequest
			{
				EventData = eventData,
				Host = this,
				OnGrantOnlyComplete = onGrantOnlyComplete
			});
		}
	}

	public bool TryDirectUseSkyrocketAfterAd(PropUseEvent eventData)
	{
		if (skyrocketStrategy != null)
		{
			return skyrocketStrategy.TryDirectUseAfterAd(eventData);
		}
		return false;
	}

	public void BeginUseProp(PropUseEvent eventData, TrackPropUseReason useReason)
	{
		RotateRingPropUseService rotateRingPropUseService = SingletonComponent<GameplayManager>.Instance?.PropUseService;
		IRotateRingPropUse rotateRingPropUse = rotateRingPropUseService?.TryGetHandler(eventData.PropType);
		if (rotateRingPropUseService == null || rotateRingPropUse == null || !rotateRingPropUse.CanStart(rotateRingPropUseService))
		{
			SendPropUsingState(EPropType.None, isUsing: false);
			NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
			return;
		}
		DismissPropGuideSilently();
		currentPropType = eventData.PropType;
		currentUseReason = useReason;
		currentEntrySource = eventData.EntrySource;
		currentSourceAnchor = eventData.SourceAnchor;
		hasConsumed = false;
		isEndingUse = false;
		if (!rotateRingPropUseService.Begin(eventData.PropType, this))
		{
			ClearUseState();
			SendPropUsingState(EPropType.None, isUsing: false);
			NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
			return;
		}
		SendPropUsingState(eventData.PropType, isUsing: true);
		int num;
		if (eventData.EntrySource != PropUseEntrySource.UnlockIntro)
		{
			PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
			num = ((propModelProxy != null && !propModelProxy.IsPropGuided(eventData.PropType)) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		if (TryOpenPropGuide(new PropGuideOpenData
		{
			PropType = eventData.PropType,
			SourceAnchor = eventData.SourceAnchor,
			OnClosed = OnGameUseGuideClosedEvent,
			IsFirstGuide = flag,
			OnItemTipConfirmed = OnGuideReady,
			OnUseButtonClicked = OnPropGuideUseButtonClicked,
			EntrySource = eventData.EntrySource,
			IsFromUnlockView = eventData.IsFromUnlockView
		}))
		{
			rotateRingPropUseService.OnPropGuideOpened();
		}
		if (!flag)
		{
			OnGuideReady();
		}
	}

	private void OnGuideReady()
	{
		RotateRingPropUseService rotateRingPropUseService = SingletonComponent<GameplayManager>.Instance?.PropUseService;
		if (rotateRingPropUseService != null && rotateRingPropUseService.IsActive)
		{
			rotateRingPropUseService.OnGuideReady();
		}
	}

	private void OnPropGuideUseButtonClicked()
	{
		if (currentPropType != EPropType.None)
		{
			SingletonComponent<GameplayManager>.Instance?.PropUseService?.OnUseButtonClicked();
			ClosePropGuide(isSuccessUse: true);
		}
	}

	private void OnGameUseGuideClosedEvent()
	{
		if (currentPropType != EPropType.None && !suppressGuideClosedCallback)
		{
			bool cancelled = currentPropType == EPropType.Hammer || currentPropType == EPropType.Skyrocket;
			SingletonComponent<GameplayManager>.Instance?.PropUseService?.OnGuideDismissed(cancelled);
		}
	}

	private void CancelCurrentPropUseState()
	{
		foreach (IPropUseFlowStrategy value in flowStrategies.Values)
		{
			value.OnSessionEnd();
		}
		if (currentPropType == EPropType.None)
		{
			isUsingByAd = false;
		}
		else
		{
			CancelUseProp();
		}
	}

	private void UseProp(EPropType propType, TrackPropUseReason reason, int cost)
	{
		GameApp.Proxy.Get<PropModelProxy>()?.UseProp(propType, cost, reason);
	}

	private void EndUse(bool completed)
	{
		if (currentPropType != EPropType.None && !isEndingUse)
		{
			isEndingUse = true;
			bool num = currentEntrySource == PropUseEntrySource.UnlockIntro;
			_ = currentEntrySource;
			SingletonComponent<GameplayManager>.Instance?.PropUseService?.End();
			ClearUseState();
			ClosePropGuide(completed);
			SendPropUsingState(EPropType.None, isUsing: false);
			if (num)
			{
				GameApp.Event.Send<PropUnlockPresentationCompleteEvent>(new PropUnlockPresentationCompleteEvent());
			}
		}
	}

	private void ClearUseState()
	{
		currentPropType = EPropType.None;
		currentUseReason = TrackPropUseReason.None;
		currentEntrySource = PropUseEntrySource.Click;
		currentSourceAnchor = null;
		hasConsumed = false;
		isEndingUse = false;
	}

	internal void FireSkyrocketFromPanel(EPropType propType)
	{
		skyrocketStrategy?.FireFromPanel(propType);
	}
}
