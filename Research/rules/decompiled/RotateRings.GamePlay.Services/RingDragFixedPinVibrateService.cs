using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using UnityEngine;
using Vibration;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services;

public sealed class RingDragFixedPinVibrateService : GameplayServiceBase<GameplayManager>
{
	private int? lastTrackedPinCount;

	public RingDragFixedPinVibrateService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnDragSettleReset);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnDragSettleReset);
			ResetState();
		}
	}

	public void TickFrameEnd()
	{
		if (!IsFixedPinVibrateExperimentActive())
		{
			return;
		}
		IGameplayObject activeGameplayObject = base.Host.ActiveGameplayObject;
		if (base.Host.ActiveOperationSource != LegacyOperationSource.PointerUp || !IsDragMover(activeGameplayObject))
		{
			return;
		}
		int num = ResolveTrackedPinCount(activeGameplayObject);
		if (!lastTrackedPinCount.HasValue)
		{
			lastTrackedPinCount = num;
			return;
		}
		int value = lastTrackedPinCount.Value;
		if (num < value)
		{
			VibrateUtility.Vibrate(1107);
		}
		lastTrackedPinCount = num;
	}

	private void OnDragSettleReset(RingDragSettleResetEvent evt)
	{
		if (!base.IsDisposed)
		{
			ResetState();
		}
	}

	private void ResetState()
	{
		lastTrackedPinCount = null;
	}

	private int ResolveTrackedPinCount(IGameplayObject moving)
	{
		CountOwnChildBlockedPinCount(moving, out var blockedCount, out var relativeTotal);
		int num = 0;
		Component val = (Component)((moving is Component) ? moving : null);
		if (val != null)
		{
			num = base.Host.RingDragCollisionEventService?.CountActiveFixedPinContacts(val) ?? 0;
		}
		if (relativeTotal <= 0)
		{
			return num;
		}
		return Mathf.Max(blockedCount, num);
	}

	private static void CountOwnChildBlockedPinCount(IGameplayObject moving, out int blockedCount, out int relativeTotal)
	{
		blockedCount = 0;
		relativeTotal = 0;
		IReadOnlyList<RingRelative> readOnlyList = ((moving is RingObject ringObject) ? ringObject.RingRelatives : ((!(moving is PullObject pullObject)) ? null : pullObject.RingRelatives));
		IReadOnlyList<RingRelative> readOnlyList2 = readOnlyList;
		if (readOnlyList2 == null)
		{
			return;
		}
		relativeTotal = readOnlyList2.Count;
		for (int i = 0; i < readOnlyList2.Count; i++)
		{
			RingRelative ringRelative = readOnlyList2[i];
			if ((Object)(object)ringRelative != (Object)null && !ringRelative.IsSolved && ringRelative.GetOverlapCount() > 0)
			{
				blockedCount++;
			}
		}
	}

	private static bool IsDragMover(IGameplayObject obj)
	{
		if (!(obj is RingObject))
		{
			return obj is PullObject;
		}
		return true;
	}

	private static bool IsFixedPinVibrateExperimentActive()
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if ((Object)(object)instance == (Object)null || instance.SolveOnDragExit)
		{
			return false;
		}
		GameBord gameBord = FunctionDataCenter.Get<GameBord>();
		if (gameBord != null)
		{
			return gameBord.GameBordFixedPinVibrate != 0;
		}
		return false;
	}
}
