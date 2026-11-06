using System;
using System.Collections.Generic;
using UnlockRing.Core.Queries;

namespace UnlockRing.Game.Gameplay;

public sealed class PartOperationsSnapshot
{
	public IReadOnlyList<AvailableOperationResult> Results { get; }

	internal IReadOnlyDictionary<ClientOperationKey, HeldGestureRange> HeldGestureRanges { get; }

	internal Guid ControllerIdentity { get; }

	internal long SessionEpoch { get; }

	internal bool IsQuick { get; }

	public PartOperationsSnapshot(IReadOnlyList<AvailableOperationResult> results, IReadOnlyDictionary<ClientOperationKey, HeldGestureRange> heldGestureRanges, Guid controllerIdentity, long sessionEpoch, bool isQuick)
	{
		Results = results ?? throw new ArgumentNullException("results");
		HeldGestureRanges = heldGestureRanges ?? throw new ArgumentNullException("heldGestureRanges");
		ControllerIdentity = controllerIdentity;
		SessionEpoch = sessionEpoch;
		IsQuick = isQuick;
	}
}
