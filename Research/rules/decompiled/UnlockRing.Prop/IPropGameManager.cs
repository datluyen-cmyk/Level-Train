using System;
using RotateRings.GamePlay;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace UnlockRing.Prop;

public interface IPropGameManager
{
	bool IsLevelCompleted { get; }

	bool UsesHammerPointerTargeting { get; }

	bool CanUseHammer();

	void BeginHammerRemoveMode(Action<HammerRemoveSelection> onSelected);

	void EndHammerRemoveMode();

	bool TryExecuteHammerRemove(IGameplayObject target);

	PartId? ResolveHammerHit(Vector2 world);

	bool TryHammerForceRemove(PartId partId, out Vector3 worldPosition);

	bool TryGetHammerTargetWorldPosition(PartId partId, out Vector3 worldPosition);

	void ResetPropGestureCapture();
}
