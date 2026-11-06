using System;
using System.Collections.Generic;
using UnityEngine;

public interface IGameplayObject
{
	bool IsSolved { get; }

	GameplayPartData PartData { get; set; }

	int LockCount { get; set; }

	bool IsCurrentPaletteLock { get; }

	bool IsCurrentPaletteLockKey { get; set; }

	void EnableRemoveMode(Action<IGameplayObject, Vector3> onFinishedRemove = null, bool deferSolve = false);

	void DisableRemoveMode();

	Vector3 GetPosition();

	void RegisterOnSolved(Action<IGameplayObject> action);

	void SetBlockedByLayer(LayerArea layerArea, bool isBlocked);

	void CompleteHammerRemove();

	void CollectSolveFragmentSegments(List<SolveFragmentSegment> buffer)
	{
	}
}
