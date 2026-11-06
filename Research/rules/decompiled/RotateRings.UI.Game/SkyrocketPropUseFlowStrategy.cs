using System;
using System.Collections.Generic;
using Game.Level;
using RotateRings.GamePlay.Services.LevelStepLimit;
using RotateRings.GamePlay.Services.Prop;
using RotateRings.Prop;
using RotateRings.UI.PropGuide;
using RotateRings.UI.PropItem;
using RotateRings.UI.Props;
using RotateRings.UI.Toast;
using UnityEngine;
using XingYun.Experiment;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public sealed class SkyrocketPropUseFlowStrategy : IPropUseFlowStrategy
{
	private const string Tag = "[UIGame][SkyrocketStrategy] ";

	private const float HammerFadeInDuration = 0.4f;

	private const float HammerFadeInDelay = 0.15f;

	private readonly IPropUseServiceHost host;

	private readonly List<TrackPropUseReason> pendingFastFireShots = new List<TrackPropUseReason>();

	private int hammerFadeInTimerId = -1;

	public EPropType PropType => EPropType.Skyrocket;

	public bool HasPendingFastFire => pendingFastFireShots.Count > 0;

	internal bool HasFreeInFlight => pendingFastFireShots.Contains(TrackPropUseReason.Free);

	public SkyrocketPropUseFlowStrategy(IPropUseServiceHost host)
	{
		this.host = host;
	}

	public bool TryHandlePropUse(PropUseEvent eventData)
	{
		switch (GetSkyrocketStrategyId())
		{
		case 1:
			TryFastFireSkyrocketExperiment1(eventData);
			return true;
		case 2:
			TryFastFireSkyrocket(eventData);
			return true;
		default:
			if (HasPendingFastFire)
			{
				return true;
			}
			if (!host.CanStartProp(eventData.PropType))
			{
				ToastService.Show(GameApp.Lang.GetText("FloatMsg_AnotherBooster", Array.Empty<object>()));
				host.NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
				return true;
			}
			return false;
		}
	}

	public void RefreshUIState()
	{
		UIPropsView uIPropsView = host.View?.PropsView;
		if (uIPropsView == null)
		{
			return;
		}
		int skyrocketStrategyId = GetSkyrocketStrategyId();
		if (skyrocketStrategyId != 1 && skyrocketStrategyId != 2)
		{
			uIPropsView.SetPropItemGray(EPropType.Skyrocket, grayed: false);
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null || !propModelProxy.IsPropUnlock(EPropType.Skyrocket))
		{
			uIPropsView.SetPropItemGray(EPropType.Skyrocket, grayed: false);
			GetRocketFastUsePropItemView()?.SetGray(grayed: false);
		}
		else
		{
			bool flag = host.CanStartProp(EPropType.Skyrocket);
			uIPropsView.SetPropItemGray(EPropType.Skyrocket, !flag);
			GetRocketFastUsePropItemView()?.SetGray(!flag);
		}
	}

	public void OnLevelChanged()
	{
		RefreshUIState();
	}

	public void OnPropChanged(PropChangeEvent eventData)
	{
		if (eventData != null && eventData.PropType == EPropType.Skyrocket && GameApp.UI.IsOpen("RocketFastUseExperiment1"))
		{
			RefreshSkyrocketDisplayCount();
			RefreshUIState();
		}
	}

	public void OnRingSolved()
	{
		RefreshUIState();
	}

	public void OnSessionEnd()
	{
		pendingFastFireShots.Clear();
		RestoreUI();
		SyncSkyrocketToRealCount();
	}

	public void FireFromPanel(EPropType propType)
	{
		if (!host.IsUsingByAd)
		{
			if (!host.CanStartProp(propType))
			{
				ToastService.Show(GameApp.Lang.GetText("Item_Firework_NotAvailable", Array.Empty<object>()), ToastVerticalAlign.Bottom, 410f);
				return;
			}
			ResolveAndFireSkyrocket(new PropUseEvent
			{
				PropType = propType
			});
		}
	}

	public void PrepareForSettlement()
	{
		RestoreUI();
	}

	private void RestoreUI()
	{
		if (GameApp.UI.IsOpen("RocketFastUseExperiment1"))
		{
			GameApp.UI.CloseView("RocketFastUseExperiment1", (object)null, false);
		}
	}

	internal void OnRocketShotHit(TrackPropUseReason reason, bool success)
	{
		int num = pendingFastFireShots.IndexOf(reason);
		if (num >= 0)
		{
			pendingFastFireShots.RemoveAt(num);
		}
		else if (pendingFastFireShots.Count > 0)
		{
			pendingFastFireShots.RemoveAt(0);
		}
		if (!success)
		{
			RefreshSkyrocketDisplayCount();
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			RefreshSkyrocketDisplayCount();
			return;
		}
		switch (reason)
		{
		case TrackPropUseReason.Free:
			propModelProxy.TryConsumePendingFreeProp(EPropType.Skyrocket);
			propModelProxy.UseFreeProp(EPropType.Skyrocket, reason);
			break;
		case TrackPropUseReason.Ad:
			propModelProxy.UseFreeProp(EPropType.Skyrocket, reason);
			break;
		default:
			propModelProxy.UseProp(EPropType.Skyrocket, 1, reason);
			break;
		}
		RandomLevelFirstClearWindow.OnPropUsed();
		RefreshSkyrocketDisplayCount();
	}

	internal void OnFastFireSessionComplete()
	{
		pendingFastFireShots.Clear();
		SingletonComponent<GameplayManager>.Instance?.PropUseService?.End();
		host.SendPropUsingState(EPropType.None, isUsing: false);
		SyncSkyrocketToRealCount();
		RefreshUIState();
	}

	private static int GetSkyrocketStrategyId()
	{
		return FunctionDataCenter.Get<PropFunction>().SkyrocketFireStrategy;
	}

	private void TryFastFireSkyrocket(PropUseEvent eventData)
	{
		if (host.IsUsingByAd)
		{
			return;
		}
		if (!host.CanStartProp(eventData.PropType))
		{
			ToastService.Show(GameApp.Lang.GetText("Item_Firework_NotAvailable", Array.Empty<object>()), ToastVerticalAlign.Bottom, 410f);
			host.NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
			return;
		}
		if (eventData.EntrySource != PropUseEntrySource.UnlockIntro)
		{
			PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
			if (propModelProxy == null || propModelProxy.IsPropGuided(eventData.PropType))
			{
				ResolveAndFireSkyrocket(eventData);
				return;
			}
		}
		OpenFastFireFirstGuide(eventData);
	}

	private bool CanFireByStepLimit()
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService == null || !levelStepLimitService.IsEnabled)
		{
			return true;
		}
		if (levelStepLimitService.IsExhausted)
		{
			ToastService.Show(GameApp.Lang.GetText("Level_OutOfMoves", Array.Empty<object>()), ToastVerticalAlign.Bottom, 410f);
			return false;
		}
		return true;
	}

	private void ResolveAndFireSkyrocket(PropUseEvent eventData)
	{
		if (!CanFireByStepLimit())
		{
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		RotateRingPropUseService rotateRingPropUseService = SingletonComponent<GameplayManager>.Instance?.PropUseService;
		if (propModelProxy == null || rotateRingPropUseService == null)
		{
			return;
		}
		if (propModelProxy.HasPendingFreeProp(eventData.PropType) && !pendingFastFireShots.Contains(TrackPropUseReason.Free))
		{
			TrackPropUseReason pendingFreePropUseReason = propModelProxy.GetPendingFreePropUseReason(eventData.PropType);
			if (rotateRingPropUseService.TryFireSkyrocket(host.Callback, pendingFreePropUseReason))
			{
				pendingFastFireShots.Add(pendingFreePropUseReason);
				RefreshSkyrocketDisplayCount();
			}
			return;
		}
		int num = CountInFlight(TrackPropUseReason.Stock);
		if (propModelProxy.GetPropCount(eventData.PropType) - num > 0)
		{
			if (rotateRingPropUseService.TryFireSkyrocket(host.Callback, TrackPropUseReason.Stock))
			{
				pendingFastFireShots.Add(TrackPropUseReason.Stock);
				RefreshSkyrocketDisplayCount();
			}
		}
		else
		{
			UsePropByAdFastFire(eventData);
		}
	}

	private void UsePropByAdFastFire(PropUseEvent eventData)
	{
		host.UsePropByAd(eventData, RefreshSkyrocketDisplayCount);
	}

	internal bool TryDirectUseAfterAd(PropUseEvent eventData)
	{
		if (!CanFireByStepLimit())
		{
			return false;
		}
		if (!host.CanStartProp(eventData.PropType))
		{
			RefreshSkyrocketDisplayCount();
			return false;
		}
		RotateRingPropUseService rotateRingPropUseService = SingletonComponent<GameplayManager>.Instance?.PropUseService;
		if (rotateRingPropUseService == null)
		{
			return false;
		}
		if (rotateRingPropUseService.TryFireSkyrocket(host.Callback, TrackPropUseReason.Ad))
		{
			pendingFastFireShots.Add(TrackPropUseReason.Ad);
			RefreshSkyrocketDisplayCount();
			return true;
		}
		RefreshSkyrocketDisplayCount();
		return false;
	}

	private void OpenFastFireFirstGuide(PropUseEvent eventData)
	{
		host.DismissPropGuideSilently();
		if (!host.TryOpenPropGuide(new PropGuideOpenData
		{
			PropType = eventData.PropType,
			SourceAnchor = eventData.SourceAnchor,
			OnClosed = delegate
			{
				host.OnFastFireGuideClosed(eventData.EntrySource);
			},
			IsFirstGuide = true,
			OnItemTipConfirmed = delegate
			{
				OnFastFireFirstGuideConfirm(eventData);
			},
			EntrySource = eventData.EntrySource,
			IsFromUnlockView = eventData.IsFromUnlockView,
			FireOnItemTipConfirm = true
		}))
		{
			ResolveAndFireSkyrocket(eventData);
		}
	}

	private void OnFastFireFirstGuideConfirm(PropUseEvent eventData)
	{
		ResolveAndFireSkyrocket(eventData);
		host.ClosePropGuide(isSuccessUse: true);
	}

	private void TryFastFireSkyrocketExperiment1(PropUseEvent eventData)
	{
		if (host.IsUsingByAd)
		{
			return;
		}
		if (!host.CanStartProp(eventData.PropType))
		{
			ToastService.Show(GameApp.Lang.GetText("Item_Firework_NotAvailable", Array.Empty<object>()), ToastVerticalAlign.Bottom, 410f);
			host.NotifyUnlockIntroCompleteIfNeeded(eventData.EntrySource);
			return;
		}
		if (eventData.EntrySource != PropUseEntrySource.UnlockIntro)
		{
			PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
			if (propModelProxy == null || propModelProxy.IsPropGuided(eventData.PropType))
			{
				DispatchExperiment1BarClick(eventData);
				return;
			}
		}
		OpenExperiment1FirstGuide(eventData);
	}

	private void DispatchExperiment1BarClick(PropUseEvent eventData)
	{
		if (ShouldExpandExperiment1PanelOnBarClick())
		{
			OpenRocketFastUsePanel(eventData);
		}
		else
		{
			ResolveAndFireSkyrocket(eventData);
		}
	}

	private bool ShouldExpandExperiment1PanelOnBarClick()
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			return false;
		}
		if (!HasFreeInFlight && propModelProxy.HasPendingFreeProp(EPropType.Skyrocket))
		{
			return true;
		}
		int propCount = propModelProxy.GetPropCount(EPropType.Skyrocket);
		int num = CountInFlight(TrackPropUseReason.Stock);
		return propCount - num > 0;
	}

	private void OpenExperiment1FirstGuide(PropUseEvent eventData)
	{
		host.DismissPropGuideSilently();
		if (!host.TryOpenPropGuide(new PropGuideOpenData
		{
			PropType = eventData.PropType,
			SourceAnchor = eventData.SourceAnchor,
			OnClosed = delegate
			{
				host.OnFastFireGuideClosed(eventData.EntrySource);
			},
			IsFirstGuide = true,
			OnItemTipConfirmed = delegate
			{
				OnExperiment1FirstGuideConfirm(eventData);
			},
			EntrySource = eventData.EntrySource,
			IsFromUnlockView = eventData.IsFromUnlockView,
			FireOnItemTipConfirm = true
		}))
		{
			DispatchExperiment1BarClick(eventData);
		}
	}

	private void OnExperiment1FirstGuideConfirm(PropUseEvent eventData)
	{
		host.ClosePropGuide(isSuccessUse: true);
		DispatchExperiment1BarClick(eventData);
	}

	private void OpenRocketFastUsePanel(PropUseEvent eventData)
	{
		host.View?.PropsView?.SetPropsContainerVisible(visible: false);
		GameApp.UI.Open("RocketFastUseExperiment1", (UILayer)2, (object)new RocketFastUseOpenData
		{
			PropType = eventData.PropType,
			SourceAnchor = eventData.SourceAnchor,
			BadgeProvider = host.SkyrocketBadgeProvider,
			OnCloseAnimationStart = OnRocketFastUsePanelCloseAnimationStart
		}, (Action)OnRocketFastUsePanelClosed);
		if (!GameApp.UI.IsOpen("RocketFastUseExperiment1"))
		{
			host.View?.PropsView?.SetPropsContainerVisible(visible: true);
			return;
		}
		RefreshSkyrocketDisplayCount();
		RefreshUIState();
	}

	private void OnRocketFastUsePanelCloseAnimationStart()
	{
		UIGameView view = host.View;
		UIBehaviour val = ((view != null) ? ((UIBaseView)view).Behaviour : null);
		if (!((Object)(object)val == (Object)null))
		{
			hammerFadeInTimerId = GameApp.Timer.Delay((Object)(object)val, 0.15f, (Action)delegate
			{
				hammerFadeInTimerId = -1;
				host.View?.PropsView?.SetPropsContainerVisible(visible: true);
				host.View?.PropsView?.SetPropItemCanvasAlpha(EPropType.Skyrocket, 0f);
				host.View?.PropsView?.PlaySinglePropAppearFadeIn(EPropType.Hammer, 0.4f);
			});
		}
	}

	private void OnRocketFastUsePanelClosed()
	{
		if (hammerFadeInTimerId != -1)
		{
			GameApp.Timer.Kill(hammerFadeInTimerId);
			hammerFadeInTimerId = -1;
		}
		host.View?.PropsView?.SetPropsContainerVisible(visible: true);
		host.View?.PropsView?.SetPropItemCanvasAlpha(EPropType.Skyrocket, 1f);
	}

	private void RefreshSkyrocketDisplayCount()
	{
		host.View?.PropsView?.RefreshPropBadge(EPropType.Skyrocket);
		GetRocketFastUsePropItemView()?.Refresh();
	}

	internal int CountInFlight(TrackPropUseReason reason)
	{
		int num = 0;
		foreach (TrackPropUseReason pendingFastFireShot in pendingFastFireShots)
		{
			if (pendingFastFireShot == reason)
			{
				num++;
			}
		}
		return num;
	}

	private void SyncSkyrocketToRealCount()
	{
		RefreshSkyrocketDisplayCount();
	}

	private UIRocketPropItemView GetRocketFastUsePropItemView()
	{
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("RocketFastUseExperiment1", ref val) || !(val is UIRocketFastUseExperiment1Controller uIRocketFastUseExperiment1Controller))
		{
			return null;
		}
		return uIRocketFastUseExperiment1Controller.View?.GetPropItemView();
	}
}
