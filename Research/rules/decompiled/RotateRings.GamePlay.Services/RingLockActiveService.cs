using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public class RingLockActiveService : GameplayServiceBase<GameplayManager>
{
	private const string TAG = "[GamePlay][RingLockActive] ";

	private readonly Dictionary<PartId, HashSet<PartId>> lockedByMap = new Dictionary<PartId, HashSet<PartId>>();

	private readonly Dictionary<PartId, HashSet<PartId>> activeLockedByMap = new Dictionary<PartId, HashSet<PartId>>();

	private readonly List<PartId> emptyActiveKeyBuffer = new List<PartId>();

	private readonly List<PartId> resetLockedBuffer = new List<PartId>();

	private readonly List<PartId> resetLockerBuffer = new List<PartId>();

	private bool eventsRegistered;

	public Action<PartId, IReadOnlyList<PartId>> OnLockClearedBySolve { get; set; }

	public RingLockActiveService(GameplayManager host)
		: base(host)
	{
		RegisterEvents();
	}

	public void InitFromRingLinks(IReadOnlyList<RingLinkEntry> ringLinks)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		lockedByMap.Clear();
		activeLockedByMap.Clear();
		if (ringLinks == null || ringLinks.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		PartId val = default(PartId);
		PartId val2 = default(PartId);
		foreach (RingLinkEntry ringLink in ringLinks)
		{
			if (ringLink.relationKind == 1)
			{
				num++;
				continue;
			}
			if (string.IsNullOrEmpty(ringLink.sourceRingId) || string.IsNullOrEmpty(ringLink.targetPartId))
			{
				num2++;
				continue;
			}
			((PartId)(ref val))._002Ector(ringLink.sourceRingId);
			((PartId)(ref val2))._002Ector(ringLink.targetPartId);
			if (IsSolvedOrMissing(val) || IsSolvedOrMissing(val2))
			{
				num3++;
				continue;
			}
			if (!lockedByMap.TryGetValue(val, out var value))
			{
				value = new HashSet<PartId>();
				lockedByMap[val] = value;
			}
			value.Add(val2);
		}
		CloneLockedToActive();
	}

	private void CloneLockedToActive()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		activeLockedByMap.Clear();
		Collider2D[] array = (Collider2D[])(object)new Collider2D[32];
		foreach (KeyValuePair<PartId, HashSet<PartId>> item in lockedByMap)
		{
			PartId key = item.Key;
			HashSet<PartId> hashSet = new HashSet<PartId>();
			foreach (PartId item2 in item.Value)
			{
				if (TryGetRingObjectByPartId(item2, out var _))
				{
					if (RingsStillConnected(key, item2))
					{
						hashSet.Add(item2);
					}
				}
				else
				{
					if (!TryGetRingObjectByPartId(key, out var ring2))
					{
						continue;
					}
					IReadOnlyList<RingRelative> ringRelatives = ring2.RingRelatives;
					if (ringRelatives == null)
					{
						continue;
					}
					bool flag = false;
					for (int i = 0; i < ringRelatives.Count; i++)
					{
						if (flag)
						{
							break;
						}
						RingRelative ringRelative = ringRelatives[i];
						if ((Object)(object)ringRelative == (Object)null || ringRelative.IsSolved)
						{
							continue;
						}
						int overlapColliders = ringRelative.GetOverlapColliders(array);
						for (int j = 0; j < overlapColliders; j++)
						{
							Collider2D val = array[j];
							if (!((Object)(object)val == (Object)null))
							{
								IGameplayObject gameplayObject = ResolveTopLevelObject(val);
								if (gameplayObject != null && gameplayObject.TryGetPartId(out var partId) && ((PartId)(ref partId)).Equals(item2))
								{
									flag = true;
									break;
								}
							}
						}
					}
					if (flag)
					{
						hashSet.Add(item2);
					}
				}
			}
			if (hashSet.Count > 0)
			{
				activeLockedByMap[key] = hashSet;
			}
		}
	}

	private void RegisterEvents()
	{
		if (!eventsRegistered)
		{
			GameApp.Event.Listen<RingDragCollisionEnterEvent>((Action<RingDragCollisionEnterEvent>)OnDragCollisionEnter);
			GameApp.Event.Listen<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnDragCollisionExit);
			GameApp.Event.Listen<RingDragCollisionResetEvent>((Action<RingDragCollisionResetEvent>)OnDragCollisionReset);
			GameApp.Event.Listen<RingAfterSolveEvent>((Action<RingAfterSolveEvent>)OnObjectAfterSolve);
			eventsRegistered = true;
		}
	}

	private void UnRegisterEvents()
	{
		if (eventsRegistered)
		{
			GameApp.Event.Unlisten<RingDragCollisionEnterEvent>((Action<RingDragCollisionEnterEvent>)OnDragCollisionEnter);
			GameApp.Event.Unlisten<RingDragCollisionExitEvent>((Action<RingDragCollisionExitEvent>)OnDragCollisionExit);
			GameApp.Event.Unlisten<RingDragCollisionResetEvent>((Action<RingDragCollisionResetEvent>)OnDragCollisionReset);
			GameApp.Event.Unlisten<RingAfterSolveEvent>((Action<RingAfterSolveEvent>)OnObjectAfterSolve);
			eventsRegistered = false;
		}
	}

	public override void ClearLevel()
	{
		lockedByMap.Clear();
		activeLockedByMap.Clear();
		base.ClearLevel();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			UnRegisterEvents();
			ClearLevel();
		}
	}

	private void OnObjectAfterSolve(RingAfterSolveEvent evt)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (evt?.GameplayObject != null && evt.GameplayObject.TryGetPartId(out var partId))
		{
			OnObjectSolved(partId);
			if (evt.GameplayObject is RingObject { IsSolved: not false } ringObject)
			{
				base.Host.LockFeature?.NotifyObjectSolved(ringObject);
				base.Host.RefreshRingDragAreaLockZ();
			}
		}
	}

	private void OnDragCollisionEnter(RingDragCollisionEnterEvent evt)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (evt != null && evt.IsNewEnter && (evt.OtherKind == RingDragOtherKind.CatchGuideBody || evt.OtherKind == RingDragOtherKind.RingRelativeBody))
		{
			TryAddLock(evt.MovingRoot, evt.OtherCollider, evt.PosWorld);
		}
	}

	private void OnDragCollisionExit(RingDragCollisionExitEvent evt)
	{
		if (evt != null && (evt.OtherKind == RingDragOtherKind.CatchGuideBody || evt.OtherKind == RingDragOtherKind.RingRelativeBody))
		{
			TryRemoveLock(evt.MovingRoot, evt.OtherCollider);
		}
	}

	private void OnDragCollisionReset(RingDragCollisionResetEvent evt)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)evt?.MovingRoot == (Object)null) && TryGetPartIdByComponent(evt.MovingRoot, out var partId))
		{
			CleanupActiveLocks(partId);
		}
	}

	private void TryAddLock(Component moving, Collider2D other, Vector2 contactWorld)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (HasRelative(other) && TryResolveLockPair(moving, other, out var lockedPartId, out var lockerPartId))
		{
			if (!activeLockedByMap.TryGetValue(lockedPartId, out var value))
			{
				value = new HashSet<PartId>();
				activeLockedByMap[lockedPartId] = value;
			}
			value.Add(lockerPartId);
		}
	}

	private void TryRemoveLock(Component moving, Collider2D other)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (HasRelative(other) && TryResolveLockPair(moving, other, out var lockedPartId, out var lockerPartId) && !RingsStillConnected(lockedPartId, lockerPartId) && activeLockedByMap.TryGetValue(lockedPartId, out var value) && value.Remove(lockerPartId) && value.Count == 0)
		{
			activeLockedByMap.Remove(lockedPartId);
		}
	}

	private void TryRemoveLock(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetRingObjectByPartId(lockedPartId, out var ring) && TryGetRingObjectByPartId(lockerPartId, out ring) && !RingsStillConnected(lockedPartId, lockerPartId) && activeLockedByMap.TryGetValue(lockedPartId, out var value) && value.Remove(lockerPartId) && value.Count == 0)
		{
			activeLockedByMap.Remove(lockedPartId);
		}
	}

	private void CleanupActiveLocks(PartId movingPartId)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		resetLockedBuffer.Clear();
		resetLockerBuffer.Clear();
		foreach (KeyValuePair<PartId, HashSet<PartId>> item in activeLockedByMap)
		{
			PartId key = item.Key;
			foreach (PartId item2 in item.Value)
			{
				PartId current2 = item2;
				if (((PartId)(ref key)).Equals(movingPartId) || ((PartId)(ref current2)).Equals(movingPartId))
				{
					resetLockedBuffer.Add(key);
					resetLockerBuffer.Add(current2);
				}
			}
		}
		for (int i = 0; i < resetLockedBuffer.Count; i++)
		{
			TryRemoveLock(resetLockedBuffer[i], resetLockerBuffer[i]);
		}
	}

	public void PruneActiveLocksForLockedRing(RingObject lockedRing)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)lockedRing == (Object)null || lockedRing.IsSolved || !lockedRing.TryGetPartId(out var partId) || !activeLockedByMap.TryGetValue(partId, out var value) || value.Count == 0)
		{
			return;
		}
		resetLockerBuffer.Clear();
		foreach (PartId item in value)
		{
			resetLockerBuffer.Add(item);
		}
		for (int i = 0; i < resetLockerBuffer.Count; i++)
		{
			PartId val = resetLockerBuffer[i];
			if (TryGetRingObjectByPartId(val, out var _))
			{
				TryRemoveLock(partId, val);
			}
			else if (!LockerStillOverlapsLockedRing(lockedRing, val))
			{
				RemoveActiveLockPair(partId, val);
			}
		}
	}

	private void RemoveActiveLockPair(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (activeLockedByMap.TryGetValue(lockedPartId, out var value) && value.Remove(lockerPartId) && value.Count == 0)
		{
			activeLockedByMap.Remove(lockedPartId);
		}
	}

	private bool LockerStillOverlapsLockedRing(RingObject lockedRing, PartId lockerPartId)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<RingRelative> ringRelatives = lockedRing.RingRelatives;
		if (ringRelatives == null)
		{
			return false;
		}
		Collider2D[] array = (Collider2D[])(object)new Collider2D[32];
		for (int i = 0; i < ringRelatives.Count; i++)
		{
			RingRelative ringRelative = ringRelatives[i];
			if ((Object)(object)ringRelative == (Object)null || ringRelative.IsSolved)
			{
				continue;
			}
			int overlapColliders = ringRelative.GetOverlapColliders(array);
			for (int j = 0; j < overlapColliders; j++)
			{
				Collider2D val = array[j];
				if (!((Object)(object)val == (Object)null))
				{
					IGameplayObject gameplayObject = ResolveTopLevelObject(val);
					if (gameplayObject != null && gameplayObject.TryGetPartId(out var partId) && ((PartId)(ref partId)).Equals(lockerPartId))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool BothRingsStillOverlap(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetRingObjectByPartId(lockedPartId, out var ring) || !TryGetRingObjectByPartId(lockerPartId, out var ring2))
		{
			return false;
		}
		return ring.HasRingOverlapWith(ring2);
	}

	private bool RingsStillConnected(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (BothRingsStillOverlap(lockedPartId, lockerPartId))
		{
			return true;
		}
		return base.Host.LockFeature?.ArePartsConnected(lockedPartId, lockerPartId) ?? false;
	}

	private void OnObjectSolved(PartId solvedPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		ClearActiveLocksForSolvedRing(solvedPartId);
		RemoveLocker(solvedPartId);
		lockedByMap.Remove(solvedPartId);
	}

	private void ClearActiveLocksForSolvedRing(PartId solvedPartId)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		resetLockedBuffer.Clear();
		resetLockerBuffer.Clear();
		foreach (KeyValuePair<PartId, HashSet<PartId>> item in activeLockedByMap)
		{
			PartId key = item.Key;
			HashSet<PartId> value = item.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			if (((PartId)(ref key)).Equals(solvedPartId))
			{
				foreach (PartId item2 in value)
				{
					resetLockedBuffer.Add(key);
					resetLockerBuffer.Add(item2);
				}
			}
			else if (value.Contains(solvedPartId))
			{
				resetLockedBuffer.Add(key);
				resetLockerBuffer.Add(solvedPartId);
			}
		}
		for (int i = 0; i < resetLockedBuffer.Count; i++)
		{
			RemoveActiveLockPair(resetLockedBuffer[i], resetLockerBuffer[i]);
		}
		activeLockedByMap.Remove(solvedPartId);
	}

	private void RemoveLocker(PartId lockerPartId)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (KeyValuePair<PartId, HashSet<PartId>> item in lockedByMap)
		{
			if (item.Value.Remove(lockerPartId))
			{
				num++;
			}
		}
		emptyActiveKeyBuffer.Clear();
		int num2 = 0;
		foreach (KeyValuePair<PartId, HashSet<PartId>> item2 in activeLockedByMap)
		{
			if (item2.Value.Remove(lockerPartId))
			{
				num2++;
			}
			if (item2.Value.Count == 0)
			{
				emptyActiveKeyBuffer.Add(item2.Key);
			}
		}
		for (int i = 0; i < emptyActiveKeyBuffer.Count; i++)
		{
			activeLockedByMap.Remove(emptyActiveKeyBuffer[i]);
		}
		if (emptyActiveKeyBuffer.Count > 0)
		{
			OnLockClearedBySolve?.Invoke(lockerPartId, emptyActiveKeyBuffer);
		}
	}

	public bool IsBlockedByFeatureActiveLock(RingObject ring)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ring == (Object)null || ring.IsSolved || !ring.TryGetPartId(out var partId))
		{
			return false;
		}
		RingLockFeatureService lockFeature = base.Host.LockFeature;
		if (lockFeature == null)
		{
			return false;
		}
		foreach (KeyValuePair<PartId, HashSet<PartId>> item in activeLockedByMap)
		{
			PartId key = item.Key;
			HashSet<PartId> value = item.Value;
			if (value == null || value.Count == 0 || !((PartId)(ref partId)).Equals(key) || IsSolvedOrMissing(key))
			{
				continue;
			}
			bool flag = lockFeature.ShouldPreserveBindingOnDragExit(key);
			foreach (PartId item2 in value)
			{
				if (!IsSolvedOrMissing(item2) && (flag || lockFeature.ShouldPreserveBindingOnDragExit(item2)) && RingsStillConnected(key, item2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForceClearActiveLockPairsForLockedRing(RingObject ring)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)ring == (Object)null) && ring.TryGetPartId(out var partId))
		{
			ClearActiveLocksForSolvedRing(partId);
			base.Host.RefreshRingDragAreaLockZ();
		}
	}

	public bool IsLockedBy(RingObject ring, Collider2D col)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ring == (Object)null || (Object)(object)col == (Object)null)
		{
			return false;
		}
		if (!ring.TryGetPartId(out var partId))
		{
			return false;
		}
		IGameplayObject gameplayObject = ResolveTopLevelObject(col);
		if (gameplayObject == null || !gameplayObject.TryGetPartId(out var partId2))
		{
			return false;
		}
		if (!lockedByMap.TryGetValue(partId, out var value))
		{
			return false;
		}
		return value.Contains(partId2);
	}

	public bool HasActiveLockTargets(PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (activeLockedByMap.TryGetValue(lockedPartId, out var value))
		{
			return value.Count > 0;
		}
		return false;
	}

	public int GetActiveLockerCount(PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (!activeLockedByMap.TryGetValue(lockedPartId, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	public bool TryGetActiveLockers(PartId lockedPartId, out IReadOnlyCollection<PartId> lockers)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (activeLockedByMap.TryGetValue(lockedPartId, out var value))
		{
			lockers = value;
			return true;
		}
		lockers = null;
		return false;
	}

	private IGameplayObject ResolveTopLevelObject(Collider2D col)
	{
		if ((Object)(object)col == (Object)null)
		{
			return null;
		}
		if (RingColliderQuery.TryGetMarker(col, out var marker) && marker.Owner != null)
		{
			return marker.Owner;
		}
		return ((Component)col).GetComponentInParent<IGameplayObject>();
	}

	private bool TryGetPartIdByComponent(Component component, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		if (component is IGameplayObject gameplayObject)
		{
			return gameplayObject.TryGetPartId(out partId);
		}
		return false;
	}

	private bool TryResolveLockPair(Component moving, Collider2D other, out PartId lockedPartId, out PartId lockerPartId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		lockedPartId = default(PartId);
		lockerPartId = default(PartId);
		if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null)
		{
			return false;
		}
		if (!TryGetPartIdByComponent(moving, out var partId))
		{
			return false;
		}
		IGameplayObject gameplayObject = ResolveTopLevelObject(other);
		if (gameplayObject == null || !gameplayObject.TryGetPartId(out var partId2))
		{
			return false;
		}
		if (IsJudgeInLockDic(partId, partId2))
		{
			lockedPartId = partId;
			lockerPartId = partId2;
			return true;
		}
		if (IsJudgeInLockDic(partId2, partId))
		{
			lockedPartId = partId2;
			lockerPartId = partId;
			return true;
		}
		return false;
	}

	private bool IsJudgeInLockDic(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (lockedByMap.TryGetValue(lockedPartId, out var value))
		{
			return value.Contains(lockerPartId);
		}
		return false;
	}

	private bool IsSolvedOrMissing(PartId partId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		RingObjectsService ringObjectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		if (ringObjectsService == null || !ringObjectsService.TryGetPartObject(partId, out var gameplayObject))
		{
			return true;
		}
		return gameplayObject.IsSolved;
	}

	private bool HasRelative(Collider2D other)
	{
		if ((Object)(object)other == (Object)null)
		{
			return true;
		}
		if (!RingColliderQuery.TryGetMarker(other, out var marker))
		{
			return (Object)(object)((Component)other).GetComponent<RingRelative>() != (Object)null;
		}
		return marker.Role == RingDragOtherKind.RingRelativeBody;
	}

	private bool TryGetRingObjectByPartId(PartId partId, out RingObject ring)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		ring = null;
		return (SingletonComponent<GameplayManager>.Instance?.ObjectsService)?.TryGetRing(partId, out ring) ?? false;
	}
}
