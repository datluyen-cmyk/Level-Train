using System;
using System.Collections.Generic;
using Example.Cfg;
using Game.Common.Services;
using RotateRings.GamePlay.Events;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.PropUnlock;
using RotateRings.UI.Props;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public sealed class UIGamePropsService : UIGameBaseService
{
	private const string Tag = "[UIGame][PropsService] ";

	private readonly Queue<PropUnlockOpenData> pendingPopups = new Queue<PropUnlockOpenData>();

	private bool isShowingPopup;

	private bool isUnlockFlowPending;

	private bool isWaitingUnlockIntroBeforeNextPopup;

	private readonly Queue<EPropType> replayGuideProps = new Queue<EPropType>();

	private bool isShowingReplayGuide;

	private int replayGuideLevel = -1;

	private int pendingPopupLevel = -1;

	private PropModelProxy PropProxy => GameApp.Proxy.Get<PropModelProxy>();

	private bool IsInGameProp(EPropType propType)
	{
		if (PropProxy != null)
		{
			return PropProxy.IsInGameProp((int)propType);
		}
		return false;
	}

	public UIGamePropsService(UIGameController controller)
		: base(controller)
	{
		GameApp.Event.Listen<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		GameApp.Event.Listen<PropChangeEvent>((Action<PropChangeEvent>)OnPropChange);
		GameApp.Event.Listen<PropNumberAniEvent>((Action<PropNumberAniEvent>)OnPropNumberAni);
		GameApp.Event.Listen<PropUnlockPresentationCompleteEvent>((Action<PropUnlockPresentationCompleteEvent>)OnPropUnlockPresentationComplete);
		GameApp.Event.Listen<LevelStartPipelineFinishedEvent>((Action<LevelStartPipelineFinishedEvent>)OnLevelStartPipelineFinished);
		InitPropsView();
	}

	public override void Dispose()
	{
		GameApp.Event.Unlisten<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		GameApp.Event.Unlisten<PropChangeEvent>((Action<PropChangeEvent>)OnPropChange);
		GameApp.Event.Unlisten<PropNumberAniEvent>((Action<PropNumberAniEvent>)OnPropNumberAni);
		GameApp.Event.Unlisten<PropUnlockPresentationCompleteEvent>((Action<PropUnlockPresentationCompleteEvent>)OnPropUnlockPresentationComplete);
		GameApp.Event.Unlisten<LevelStartPipelineFinishedEvent>((Action<LevelStartPipelineFinishedEvent>)OnLevelStartPipelineFinished);
		ClearUnlockFlowState();
		StopReplayGuide();
		base.Dispose();
	}

	private void OnLevelChanged(LevelChanged eventData)
	{
		if (eventData == null)
		{
			return;
		}
		int level = eventData.Level;
		ClearUnlockFlowState();
		StopReplayGuide();
		PropProxy?.ClearPendingFreePropsNotForLevel(eventData.Level);
		PropModelProxy propProxy = PropProxy;
		if (propProxy == null)
		{
			RefreshAllProp();
			return;
		}
		IReadOnlyList<PropConfig> readOnlyList = QueryUnlockable(level);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			propProxy.EnqueuePendingUnlockIntro((EPropType)readOnlyList[i].Id);
		}
		PropConfig propConfig = TakeNextPendingUnlockConfig(propProxy);
		if (propConfig != null)
		{
			HashSet<EPropType> excludePropTypes = new HashSet<EPropType> { (EPropType)propConfig.Id };
			RefreshAllProp(excludePropTypes);
			isUnlockFlowPending = true;
			BeginPresentation(level, new PropConfig[1] { propConfig });
		}
		else
		{
			RefreshAllProp();
			BeginReplayGuide(level);
		}
	}

	private void OnLevelStartPipelineFinished(LevelStartPipelineFinishedEvent eventData)
	{
		if (eventData != null && eventData.Round == LevelStartPipeline.CurrentRound)
		{
			if (eventData.Level == pendingPopupLevel)
			{
				pendingPopupLevel = -1;
				ShowNextPopup();
			}
			if (eventData.Level == replayGuideLevel)
			{
				ScheduleNextReplayGuide();
			}
		}
	}

	private void OnPropChange(PropChangeEvent eventData)
	{
		if (eventData != null)
		{
			HandlePropChange(eventData);
		}
	}

	private void OnPropNumberAni(PropNumberAniEvent eventData)
	{
		if (eventData != null && IsInGameProp(eventData.PropType))
		{
			base.View?.PropsView?.RefreshScrollTo(eventData.PropType, eventData.lastNumber, eventData.currentNumber, eventData.duration, eventData.isExcuteComplete);
		}
	}

	private void OnPropUnlockPresentationComplete(PropUnlockPresentationCompleteEvent eventData)
	{
		if (isShowingReplayGuide)
		{
			isShowingReplayGuide = false;
			ScheduleNextReplayGuide();
		}
		else if (isUnlockFlowPending)
		{
			if (isWaitingUnlockIntroBeforeNextPopup)
			{
				isWaitingUnlockIntroBeforeNextPopup = false;
				ShowNextPopup();
			}
			else if (pendingPopups.Count <= 0 && !isShowingPopup)
			{
				FinishUnlockFlow();
			}
		}
	}

	private void InitPropsView()
	{
		base.View?.PropsView?.BuildPropItems();
		base.View?.PropsView?.RefreshAllProp();
	}

	private void RefreshAllProp()
	{
		base.View?.PropsView?.RefreshAllProp();
	}

	private void RefreshAllProp(IReadOnlyCollection<EPropType> excludePropTypes)
	{
		base.View?.PropsView?.RefreshAllProp(excludePropTypes);
	}

	private void BeginHold(EPropType propType, int? displayCount = null)
	{
		UIPropsView uIPropsView = base.View?.PropsView;
		if (uIPropsView != null)
		{
			uIPropsView.SetPresentationLocked(propType, locked: true);
			if (displayCount.HasValue)
			{
				uIPropsView.RefreshPropCount(propType, displayCount.Value);
			}
		}
	}

	private void EndHold(EPropType propType, bool refreshFromData = true)
	{
		UIPropsView uIPropsView = base.View?.PropsView;
		if (uIPropsView != null)
		{
			uIPropsView.SetPresentationLocked(propType, locked: false);
			if (refreshFromData)
			{
				uIPropsView.RefreshProp(propType);
			}
		}
	}

	private void HandlePropChange(PropChangeEvent eventData)
	{
		if (eventData != null && IsInGameProp(eventData.PropType))
		{
			base.View?.PropsView?.RefreshProp(eventData.PropType);
		}
	}

	private IReadOnlyList<PropConfig> QueryUnlockable(int level)
	{
		return GameApp.Proxy.Get<PropModelProxy>().CollectUnlockableProps(level);
	}

	private PropConfig TakeNextPendingUnlockConfig(PropModelProxy propProxy)
	{
		if (propProxy == null)
		{
			return null;
		}
		propProxy.SanitizePendingUnlockIntro();
		PropConfig propConfig;
		while (true)
		{
			EPropType ePropType = propProxy.PeekPendingUnlockIntroByPriority();
			if (ePropType == EPropType.None)
			{
				return null;
			}
			propProxy.DequeuePendingUnlockIntro(ePropType);
			if (!propProxy.IsPropGuided(ePropType) && !propProxy.IsPropUnlock(ePropType))
			{
				propConfig = propProxy.GetPropConfig(ePropType);
				if (propConfig != null)
				{
					break;
				}
			}
		}
		return propConfig;
	}

	private void BeginReplayGuide(int level)
	{
		StopReplayGuide();
		List<EPropType> list = PropProxy?.CollectReplayGuideProps(level);
		if (list == null || list.Count == 0)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			EPropType ePropType = list[i];
			if (ShouldGuideOnReplay(ePropType))
			{
				PropProxy?.EnsurePendingFreeProp(ePropType);
				replayGuideProps.Enqueue(ePropType);
				base.View?.PropsView?.RefreshProp(ePropType);
			}
		}
		if (replayGuideProps.Count == 0)
		{
			replayGuideLevel = -1;
		}
		else
		{
			replayGuideLevel = level;
		}
	}

	private bool ShouldGuideOnReplay(EPropType propType)
	{
		PropModelProxy propProxy = PropProxy;
		if (propProxy == null)
		{
			return false;
		}
		if (propProxy.IsPropGuided(propType))
		{
			return !propProxy.IsGuideUserInteracted(propType);
		}
		return true;
	}

	private void ScheduleNextReplayGuide()
	{
		if (!isShowingReplayGuide && replayGuideProps.Count != 0)
		{
			StartNextReplayGuide();
		}
	}

	private void StartNextReplayGuide()
	{
		if (!isShowingReplayGuide && replayGuideProps.Count != 0)
		{
			EPropType propType = replayGuideProps.Dequeue();
			Canvas.ForceUpdateCanvases();
			RectTransform val = base.View?.PropsView?.GetPropItemAlignAnchor(propType);
			if ((Object)(object)val == (Object)null)
			{
				ScheduleNextReplayGuide();
				return;
			}
			isShowingReplayGuide = true;
			GameApp.Event.Send<PropUseEvent>(new PropUseEvent
			{
				PropType = propType,
				SourceAnchor = val,
				EntrySource = PropUseEntrySource.UnlockIntro
			});
		}
	}

	private void StopReplayGuide()
	{
		replayGuideProps.Clear();
		replayGuideLevel = -1;
		isShowingReplayGuide = false;
	}

	private void BeginPresentation(int level, IReadOnlyList<PropConfig> unlockableProps)
	{
		if (unlockableProps == null || unlockableProps.Count == 0)
		{
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		for (int i = 0; i < unlockableProps.Count; i++)
		{
			EPropType id = (EPropType)unlockableProps[i].Id;
			int newUserFreeSendCount = propModelProxy.GetNewUserFreeSendCount(id);
			propModelProxy.UnlockProp(id);
			if (newUserFreeSendCount > 0)
			{
				propModelProxy.AddProp(id, newUserFreeSendCount, TrackPropGainReason.NewBieReward, "新手奖励", "新手奖励");
				if (propModelProxy.IsInGameProp((int)id))
				{
					int lastPropCount = propModelProxy.GetLastPropCount(id);
					BeginHold(id, lastPropCount);
				}
			}
			else if (propModelProxy.IsInGameProp((int)id))
			{
				BeginHold(id);
			}
			int propCount = propModelProxy.GetPropCount(id);
			bool flag = newUserFreeSendCount > 0 && propModelProxy.IsInGameProp((int)id);
			int fromCount = (flag ? (propCount - newUserFreeSendCount) : 0);
			PropUnlockOpenData item = new PropUnlockOpenData
			{
				PropType = id,
				GrantCount = newUserFreeSendCount,
				PropName = propModelProxy.GetPropName(id),
				IconName = PropIconService.GetIconNameLarge(id),
				FromCount = fromCount,
				ToCount = propCount,
				FlyIconName = PropIconService.GetIconName(id),
				NeedFlyPresentation = flag
			};
			pendingPopups.Enqueue(item);
		}
		pendingPopupLevel = level;
	}

	private void ShowNextPopup()
	{
		if (!isShowingPopup && pendingPopups.Count != 0)
		{
			isShowingPopup = true;
			PropUnlockOpenData openData = pendingPopups.Dequeue();
			openData.OnClosed = delegate
			{
				OnPopupClosed(openData);
			};
			GameApp.UI.Open("UIPropUnlock", (UILayer)2, (object)openData, (Action)null);
		}
	}

	private void OnPopupClosed(PropUnlockOpenData closedData)
	{
		isShowingPopup = false;
		if (closedData != null && closedData.GrantCount == 0)
		{
			EndHold(closedData.PropType);
		}
		if (pendingPopups.Count > 0)
		{
			if (closedData != null && ShouldWaitForUnlockIntro(closedData.PropType))
			{
				isWaitingUnlockIntroBeforeNextPopup = true;
			}
			else
			{
				ShowNextPopup();
			}
		}
		else if (closedData == null || !ShouldWaitForUnlockIntro(closedData.PropType))
		{
			FinishUnlockFlow();
		}
	}

	private bool ShouldWaitForUnlockIntro(EPropType propType)
	{
		PropModelProxy propProxy = PropProxy;
		if (propProxy != null && propProxy.IsInGameProp((int)propType))
		{
			return propProxy.IsPropFirstUse(propType);
		}
		return false;
	}

	private void FinishUnlockFlow()
	{
		if (isUnlockFlowPending)
		{
			isUnlockFlowPending = false;
			RefreshAllProp();
		}
	}

	private void ClearUnlockFlowState()
	{
		pendingPopups.Clear();
		isShowingPopup = false;
		isUnlockFlowPending = false;
		isWaitingUnlockIntroBeforeNextPopup = false;
		pendingPopupLevel = -1;
	}
}
