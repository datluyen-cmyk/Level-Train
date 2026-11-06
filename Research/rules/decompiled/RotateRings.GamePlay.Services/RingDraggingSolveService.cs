using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using UnityEngine;
using UnlockRing;

namespace RotateRings.GamePlay.Services;

public sealed class RingDraggingSolveService : GameplayServiceBase<GameplayManager>
{
	private readonly List<Component> exitMoverQueue = new List<Component>();

	private bool releaseSettleScheduled;

	private bool releaseSettleDone;

	public bool IsReleaseSettleDone => releaseSettleDone;

	public RingDraggingSolveService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnDragCollisionExit);
		GameApp.Event.Listen<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnDragSettleReset);
		GameApp.Event.Listen<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			GameApp.Event.Unlisten<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnDragCollisionExit);
			GameApp.Event.Unlisten<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnDragSettleReset);
			GameApp.Event.Unlisten<RingSolvedEvent>((Action<RingSolvedEvent>)OnRingSolved);
			exitMoverQueue.Clear();
			releaseSettleScheduled = false;
			releaseSettleDone = false;
		}
	}

	private void OnDragCollisionExit(RingDragCollisionExitEvent evt)
	{
		if (SingletonComponent<GameplayManager>.Instance.SolveOnDragExit && evt != null && evt.MovingRoot is IGameplayObject { IsSolved: false } gameplayObject && (evt.OtherKind == RingDragOtherKind.CatchGuideBody || evt.OtherKind == RingDragOtherKind.RingRelativeBody) && base.Host.ActiveGameplayObject == gameplayObject)
		{
			exitMoverQueue.Add(evt.MovingRoot);
		}
	}

	public void TickFrame()
	{
		if (exitMoverQueue.Count > 0)
		{
			exitMoverQueue.Clear();
			base.Host.ObjectsSolveAndCheckVictory();
		}
		if (!releaseSettleScheduled)
		{
			return;
		}
		releaseSettleScheduled = false;
		IGameplayObject activeGameplayObject = base.Host.ActiveGameplayObject;
		if (activeGameplayObject != null)
		{
			IReadOnlyList<GameplayReleasedPart> readOnlyList = base.Host.EndLegacyOperation(activeGameplayObject, LegacyOperationSource.PointerUp);
			if (readOnlyList.Count <= 0)
			{
				base.Host.ActiveGameplayObject = activeGameplayObject;
				base.Host.ActiveOperationSource = LegacyOperationSource.PointerUp;
				base.Host.ObjectsService.CollectActiveUnsolvedPartIds();
			}
			else
			{
				releaseSettleDone = true;
				base.Host.CompleteLegacyOperation(activeGameplayObject, LegacyOperationSource.PointerUp, readOnlyList);
				base.Host.ActiveGameplayObject = activeGameplayObject;
				base.Host.ActiveOperationSource = LegacyOperationSource.PointerUp;
				base.Host.ObjectsService.CollectActiveUnsolvedPartIds();
			}
		}
	}

	private void OnDragSettleReset(RingDragSettleResetEvent evt)
	{
		if (!base.IsDisposed)
		{
			exitMoverQueue.Clear();
			releaseSettleScheduled = false;
			releaseSettleDone = false;
		}
	}

	private void OnRingSolved(RingSolvedEvent evt)
	{
		if (!base.IsDisposed && evt != null && base.Host.ActiveOperationSource == LegacyOperationSource.PointerUp && base.Host.ActiveGameplayObject != null)
		{
			releaseSettleScheduled = true;
		}
	}
}
