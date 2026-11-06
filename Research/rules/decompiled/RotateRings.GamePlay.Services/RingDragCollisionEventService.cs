using System;
using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingDragCollisionEventService : GameplayServiceBase<GameplayManager>
{
	private readonly struct PhysicsDragContactKey : IEquatable<PhysicsDragContactKey>
	{
		public Component MovingRoot { get; }

		public Collider2D OtherCollider { get; }

		public PhysicsDragContactKey(Component movingRoot, Collider2D otherCollider)
		{
			MovingRoot = movingRoot;
			OtherCollider = otherCollider;
		}

		public bool Equals(PhysicsDragContactKey other)
		{
			if ((Object)(object)MovingRoot == (Object)(object)other.MovingRoot)
			{
				return (Object)(object)OtherCollider == (Object)(object)other.OtherCollider;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PhysicsDragContactKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = (((Object)(object)MovingRoot != (Object)null) ? ((Object)MovingRoot).GetInstanceID() : 0);
			int num2 = (((Object)(object)OtherCollider != (Object)null) ? ((Object)OtherCollider).GetInstanceID() : 0);
			return (num * 397) ^ num2;
		}
	}

	private readonly struct PhysicsDragStopEffectKey : IEquatable<PhysicsDragStopEffectKey>
	{
		private readonly int movingInstanceId;

		private readonly int blockingInstanceId;

		public PhysicsDragStopEffectKey(Component movingRoot, Collider2D blockingCollider)
		{
			movingInstanceId = (((Object)(object)movingRoot != (Object)null) ? ((Object)movingRoot).GetInstanceID() : 0);
			blockingInstanceId = (((Object)(object)blockingCollider != (Object)null) ? ((Object)blockingCollider).GetInstanceID() : 0);
		}

		public bool Equals(PhysicsDragStopEffectKey other)
		{
			if (movingInstanceId == other.movingInstanceId)
			{
				return blockingInstanceId == other.blockingInstanceId;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PhysicsDragStopEffectKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (movingInstanceId * 397) ^ blockingInstanceId;
		}
	}

	private readonly HashSet<PhysicsDragContactKey> currentColliders = new HashSet<PhysicsDragContactKey>();

	private readonly Dictionary<int, HashSet<int>> dragChangeColliderIds = new Dictionary<int, HashSet<int>>();

	private readonly Dictionary<int, HashSet<int>> startEnterColliderIds = new Dictionary<int, HashSet<int>>();

	private readonly List<PhysicsDragContactKey> pendingExitKeys = new List<PhysicsDragContactKey>();

	private readonly HashSet<int> currentFrameIds = new HashSet<int>();

	private PhysicsDragStopEffectKey? activeStopKey;

	private bool canTriggerStop;

	private readonly HashSet<RingRelative> activeFixedPinRelativeScratch = new HashSet<RingRelative>();

	private readonly HashSet<FixedButterflyRelative> activeFixedPinGuideScratch = new HashSet<FixedButterflyRelative>();

	private readonly HashSet<int> activeFixedPinColliderFallbackScratch = new HashSet<int>();

	public RingDragCollisionEventService(GameplayManager host)
		: base(host)
	{
	}

	public void BeginDragEvent(Component moving, IReadOnlyList<Collider2D> initialColliders)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("RingDragCollisionEventService.BeginDragEvent"))
		{
			if ((Object)(object)moving == (Object)null)
			{
				return;
			}
			RingDragMoveKind moveKind = ((moving is PullObject) ? RingDragMoveKind.Translate : RingDragMoveKind.Rotate);
			int instanceID = ((Object)moving).GetInstanceID();
			if (!startEnterColliderIds.TryGetValue(instanceID, out var value))
			{
				value = new HashSet<int>();
				startEnterColliderIds[instanceID] = value;
			}
			else
			{
				value.Clear();
			}
			canTriggerStop = true;
			if (initialColliders != null)
			{
				for (int i = 0; i < initialColliders.Count; i++)
				{
					Collider2D val = initialColliders[i];
					if (!((Object)(object)val == (Object)null) && IsValidCollider(moving, val))
					{
						value.Add(((Object)val).GetInstanceID());
						currentColliders.Add(new PhysicsDragContactKey(moving, val));
						Vector2 enterPos = val.ClosestPoint(Vector2.op_Implicit(moving.transform.position));
						SendEnterEvent(moving, val, enterPos, moveKind, isNewEnter: false);
					}
				}
			}
			if (!dragChangeColliderIds.TryGetValue(instanceID, out var value2))
			{
				value2 = new HashSet<int>();
				dragChangeColliderIds[instanceID] = value2;
			}
			else
			{
				value2.Clear();
			}
			foreach (int item in value)
			{
				value2.Add(item);
			}
		}
	}

	public void SendAndClearData(Component movingRoot = null)
	{
		currentColliders.Clear();
		dragChangeColliderIds.Clear();
		startEnterColliderIds.Clear();
		activeStopKey = null;
		canTriggerStop = false;
		GameApp.Event.Send<RingDragCollisionResetEvent>(new RingDragCollisionResetEvent(movingRoot));
	}

	public void SyncCollidersAndSendEvents(Component moving, IReadOnlyList<Collider2D> current, IReadOnlyList<Collider2D> contactProbes, RingDragMoveKind moveKind)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("RingDragCollisionEventService.SyncCollidersAndSendEvents"))
		{
			if ((Object)(object)moving == (Object)null)
			{
				return;
			}
			HashSet<int> value = null;
			HashSet<int> value2 = null;
			int instanceID = ((Object)moving).GetInstanceID();
			startEnterColliderIds.TryGetValue(instanceID, out value);
			if (!dragChangeColliderIds.TryGetValue(instanceID, out value2))
			{
				value2 = new HashSet<int>();
				dragChangeColliderIds[instanceID] = value2;
			}
			HashSet<int> hashSet = currentFrameIds;
			hashSet.Clear();
			if (current != null)
			{
				for (int i = 0; i < current.Count; i++)
				{
					Collider2D val = current[i];
					if ((Object)(object)val == (Object)null || !IsValidCollider(moving, val))
					{
						continue;
					}
					int instanceID2 = ((Object)val).GetInstanceID();
					hashSet.Add(instanceID2);
					if (value != null && value.Contains(instanceID2))
					{
						PhysicsDragContactKey item = new PhysicsDragContactKey(moving, val);
						if (!currentColliders.Contains(item))
						{
							currentColliders.Add(item);
							TryFindColliderPos(contactProbes, val, out var contactWorld);
							SendEnterEvent(moving, val, contactWorld, moveKind, isNewEnter: true);
						}
					}
					else
					{
						TryFindColliderPos(contactProbes, val, out var contactWorld2);
						CheckAndSendColliderEvent(moving, val, contactWorld2, inContact: true, moveKind);
					}
				}
			}
			if (value2 == null)
			{
				return;
			}
			foreach (int item2 in value2)
			{
				if (!hashSet.Contains(item2) && TryFindActiveCollider(moving, item2, out var key, out var other))
				{
					currentColliders.Remove(key);
					SendExitEvent(moving, other, moveKind);
				}
			}
			value2.Clear();
			foreach (int item3 in hashSet)
			{
				value2.Add(item3);
			}
		}
	}

	private void CheckAndSendColliderEvent(Component moving, Collider2D otherCollider, Vector2 contact, bool inContact, RingDragMoveKind moveKind)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)moving == (Object)null)
		{
			return;
		}
		if (!inContact || (Object)(object)otherCollider == (Object)null)
		{
			if ((Object)(object)otherCollider != (Object)null)
			{
				ExitColliderAndSendEvent(moving, otherCollider, moveKind);
			}
			else
			{
				ExitAllColliders(moving, moveKind);
			}
			return;
		}
		if (!IsValidCollider(moving, otherCollider))
		{
			ExitColliderAndSendEvent(moving, otherCollider, moveKind);
			return;
		}
		canTriggerStop = true;
		PhysicsDragContactKey item = new PhysicsDragContactKey(moving, otherCollider);
		if (!currentColliders.Contains(item))
		{
			currentColliders.Add(item);
			SendEnterEvent(moving, otherCollider, contact, moveKind, isNewEnter: true);
		}
	}

	private bool TryFindActiveCollider(Component moving, int otherInstanceId, out PhysicsDragContactKey key, out Collider2D other)
	{
		foreach (PhysicsDragContactKey currentCollider in currentColliders)
		{
			if (!((Object)(object)currentCollider.MovingRoot != (Object)(object)moving))
			{
				other = currentCollider.OtherCollider;
				if ((Object)(object)other != (Object)null && ((Object)other).GetInstanceID() == otherInstanceId)
				{
					key = currentCollider;
					return true;
				}
			}
		}
		key = default(PhysicsDragContactKey);
		other = null;
		return false;
	}

	private void SendExitEvents(Component moving, List<PhysicsDragContactKey> keys, RingDragMoveKind moveKind)
	{
		for (int i = 0; i < keys.Count; i++)
		{
			PhysicsDragContactKey item = keys[i];
			if (currentColliders.Remove(item))
			{
				Collider2D otherCollider = item.OtherCollider;
				if (!((Object)(object)otherCollider == (Object)null))
				{
					SendExitEvent(moving, otherCollider, moveKind);
				}
			}
		}
	}

	public int CountActiveFixedPinContacts(Component movingRoot)
	{
		int ringRelativeCount;
		int catchGuideCount;
		int fallbackColliderCount;
		return CountActiveFixedPinContacts(movingRoot, out ringRelativeCount, out catchGuideCount, out fallbackColliderCount);
	}

	public int CountActiveFixedPinContacts(Component movingRoot, out int ringRelativeCount, out int catchGuideCount, out int fallbackColliderCount)
	{
		ringRelativeCount = 0;
		catchGuideCount = 0;
		fallbackColliderCount = 0;
		if ((Object)(object)movingRoot == (Object)null)
		{
			return 0;
		}
		activeFixedPinRelativeScratch.Clear();
		activeFixedPinGuideScratch.Clear();
		activeFixedPinColliderFallbackScratch.Clear();
		foreach (PhysicsDragContactKey currentCollider in currentColliders)
		{
			if ((Object)(object)currentCollider.MovingRoot != (Object)(object)movingRoot)
			{
				continue;
			}
			Collider2D otherCollider = currentCollider.OtherCollider;
			if ((Object)(object)otherCollider == (Object)null || !IsFixedPinContactKind(GetOtherKind(otherCollider)))
			{
				continue;
			}
			if (RingColliderQuery.TryGetMarker(otherCollider, out var marker))
			{
				RingRelative ringRelative = marker.GetRingRelative();
				if ((Object)(object)ringRelative != (Object)null)
				{
					activeFixedPinRelativeScratch.Add(ringRelative);
					continue;
				}
				FixedButterflyRelative fixedButterflyRelative = marker.GetFixedButterflyRelative();
				if ((Object)(object)fixedButterflyRelative != (Object)null)
				{
					activeFixedPinGuideScratch.Add(fixedButterflyRelative);
					continue;
				}
			}
			activeFixedPinColliderFallbackScratch.Add(((Object)otherCollider).GetInstanceID());
		}
		ringRelativeCount = activeFixedPinRelativeScratch.Count;
		catchGuideCount = activeFixedPinGuideScratch.Count;
		fallbackColliderCount = activeFixedPinColliderFallbackScratch.Count;
		return ringRelativeCount + catchGuideCount + fallbackColliderCount;
	}

	private static bool IsFixedPinContactKind(RingDragOtherKind kind)
	{
		if (kind != RingDragOtherKind.RingRelativeBody && kind != RingDragOtherKind.RingRelativeWall && kind != RingDragOtherKind.CatchGuideBody)
		{
			return kind == RingDragOtherKind.CatchGuideWall;
		}
		return true;
	}

	public void SendStopEvent(Component moving, Collider2D other, Vector2 colliderPos, float moveDirection, RingDragMoveKind moveKind)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("RingDragCollisionEventService.SendStopEvent"))
		{
			if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null)
			{
				return;
			}
			CheckAndSendColliderEvent(moving, other, colliderPos, inContact: true, moveKind);
			if (!RingColliderQuery.IsCatchGuideBodyCollider(other) && (canTriggerStop || !IsAtDragStartCollider(moving, other)))
			{
				PhysicsDragStopEffectKey physicsDragStopEffectKey = new PhysicsDragStopEffectKey(moving, other);
				if (!activeStopKey.HasValue || !activeStopKey.Value.Equals(physicsDragStopEffectKey))
				{
					activeStopKey = physicsDragStopEffectKey;
					SendStopEvent(moving, other, colliderPos, moveKind, moveDirection);
				}
			}
		}
	}

	public void NotifyStopRearmed()
	{
		canTriggerStop = true;
		activeStopKey = null;
	}

	private void SendStopEvent(Component moving, Collider2D other, Vector2 colliderPos, RingDragMoveKind moveKind, float moveDirection)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GameApp.Event.Send<RingDragCollisionStopEvent>(new RingDragCollisionStopEvent(moving, other, colliderPos, moveKind, moveDirection, GetOtherKind(other), GetOtherOwner(other)));
	}

	public void CollectSelfColliders(List<Collider2D> output, IReadOnlyList<RingRelative> relatives, int excludeLayer, params Collider2D[] primaryProbes)
	{
		output.Clear();
		if (primaryProbes != null)
		{
			for (int i = 0; i < primaryProbes.Length; i++)
			{
				TryAddSelfCollider(output, primaryProbes[i], excludeLayer);
			}
		}
		if (relatives == null)
		{
			return;
		}
		for (int j = 0; j < relatives.Count; j++)
		{
			RingRelative ringRelative = relatives[j];
			if ((Object)(object)ringRelative == (Object)null)
			{
				continue;
			}
			Collider2D[] blockColliders = ringRelative.BlockColliders;
			if (blockColliders != null)
			{
				for (int k = 0; k < blockColliders.Length; k++)
				{
					TryAddSelfCollider(output, blockColliders[k], excludeLayer);
				}
			}
		}
	}

	public bool TryGetPosFromBlockerCollider(Collider2D blocker, IReadOnlyList<Collider2D> colliders, Collider2D fallbackCollider, out Collider2D outCollider, out Vector2 pos)
	{
		if (TryFindBestColliderPos(blocker, colliders, out outCollider, out pos))
		{
			return true;
		}
		outCollider = fallbackCollider;
		return TryFindColliderPos(fallbackCollider, blocker, out pos);
	}

	public bool TryGetPosFromCollision(Collision2D collider, IReadOnlyList<Collider2D> colliders, Collider2D fallbackProbe, out Collider2D outCollider, out Vector2 pos)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		outCollider = fallbackProbe;
		pos = default(Vector2);
		if (collider != null && collider.contactCount > 0)
		{
			ContactPoint2D contact = collider.GetContact(0);
			pos = ((ContactPoint2D)(ref contact)).point;
			if ((Object)(object)((ContactPoint2D)(ref contact)).collider != (Object)null)
			{
				outCollider = ((ContactPoint2D)(ref contact)).collider;
			}
			return true;
		}
		Collider2D val = ((collider != null) ? collider.collider : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		return TryGetPosFromBlockerCollider(val, colliders, fallbackProbe, out outCollider, out pos);
	}

	private bool TryFindColliderPos(Collider2D probe, Collider2D blocker, out Vector2 contactWorld)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		contactWorld = default(Vector2);
		if ((Object)(object)blocker == (Object)null)
		{
			return false;
		}
		if ((Object)(object)probe != (Object)null)
		{
			ColliderDistance2D val = Physics2D.Distance(probe, blocker);
			if (((ColliderDistance2D)(ref val)).isValid)
			{
				contactWorld = (((ColliderDistance2D)(ref val)).pointA + ((ColliderDistance2D)(ref val)).pointB) * 0.5f;
				return true;
			}
		}
		Vector2 val2 = Vector2.op_Implicit(((Object)(object)probe != (Object)null) ? ((Component)probe).transform.position : ((Component)blocker).transform.position);
		contactWorld = blocker.ClosestPoint(val2);
		return true;
	}

	private bool TryFindBestColliderPos(Collider2D blocker, IReadOnlyList<Collider2D> colliders, out Collider2D outBestCollider, out Vector2 contactWorld)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		outBestCollider = null;
		contactWorld = default(Vector2);
		if ((Object)(object)blocker == (Object)null || colliders == null || colliders.Count == 0)
		{
			return false;
		}
		float num = float.MaxValue;
		bool result = false;
		for (int i = 0; i < colliders.Count; i++)
		{
			Collider2D val = colliders[i];
			if (!((Object)(object)val == (Object)null) && ((Behaviour)val).enabled && !val.isTrigger)
			{
				ColliderDistance2D val2 = Physics2D.Distance(val, blocker);
				if (((ColliderDistance2D)(ref val2)).isValid && !(((ColliderDistance2D)(ref val2)).distance >= num))
				{
					num = ((ColliderDistance2D)(ref val2)).distance;
					outBestCollider = val;
					contactWorld = (((ColliderDistance2D)(ref val2)).pointA + ((ColliderDistance2D)(ref val2)).pointB) * 0.5f;
					result = true;
				}
			}
		}
		return result;
	}

	private bool TryFindColliderPos(IReadOnlyList<Collider2D> contactProbes, Collider2D blocker, out Vector2 contactWorld)
	{
		if (TryFindBestColliderPos(blocker, contactProbes, out var _, out contactWorld))
		{
			return true;
		}
		Collider2D probe = ((contactProbes != null && contactProbes.Count > 0) ? contactProbes[0] : null);
		return TryFindColliderPos(probe, blocker, out contactWorld);
	}

	private void TryAddSelfCollider(List<Collider2D> output, Collider2D col, int excludeLayer)
	{
		if ((Object)(object)col == (Object)null || !((Behaviour)col).enabled || col.isTrigger || (excludeLayer >= 0 && ((Component)col).gameObject.layer == excludeLayer))
		{
			return;
		}
		for (int i = 0; i < output.Count; i++)
		{
			if ((Object)(object)output[i] == (Object)(object)col)
			{
				return;
			}
		}
		output.Add(col);
	}

	private bool IsValidCollider(Component moving, Collider2D other)
	{
		if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null)
		{
			return false;
		}
		if (((Component)other).gameObject.layer == 6)
		{
			return false;
		}
		if (!(moving is RingObject) && !(moving is PullObject))
		{
			return false;
		}
		if (!RingColliderQuery.TryGetMarker(other, out var marker))
		{
			return false;
		}
		return marker.Owner != moving;
	}

	private bool IsAtDragStartCollider(Component moving, Collider2D other)
	{
		if ((Object)(object)moving == (Object)null || (Object)(object)other == (Object)null)
		{
			return false;
		}
		if (!startEnterColliderIds.TryGetValue(((Object)moving).GetInstanceID(), out var value))
		{
			return false;
		}
		return value.Contains(((Object)other).GetInstanceID());
	}

	private void SendEnterEvent(Component moving, Collider2D other, Vector2 enterPos, RingDragMoveKind moveKind, bool isNewEnter)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GameApp.Event.Send<RingDragCollisionEnterEvent>(new RingDragCollisionEnterEvent(moving, other, enterPos, moveKind, isNewEnter, GetOtherKind(other), GetOtherOwner(other)));
	}

	private void ExitAllColliders(Component moving, RingDragMoveKind moveKind)
	{
		pendingExitKeys.Clear();
		foreach (PhysicsDragContactKey currentCollider in currentColliders)
		{
			if (!((Object)(object)currentCollider.MovingRoot != (Object)(object)moving))
			{
				pendingExitKeys.Add(currentCollider);
			}
		}
		SendExitEvents(moving, pendingExitKeys, moveKind);
	}

	private void ExitColliderAndSendEvent(Component moving, Collider2D other, RingDragMoveKind moveKind)
	{
		if ((Object)(object)other == (Object)null)
		{
			ExitAllColliders(moving, moveKind);
			return;
		}
		PhysicsDragContactKey item = new PhysicsDragContactKey(moving, other);
		if (currentColliders.Remove(item))
		{
			SendExitEvent(moving, other, moveKind);
		}
	}

	private void SendExitEvent(Component moving, Collider2D other, RingDragMoveKind moveKind)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		GameApp.Event.Send<RingDragCollisionExitEvent>(new RingDragCollisionExitEvent(moving, other, default(Vector2), moveKind, GetOtherKind(other), GetOtherOwner(other)));
	}

	private RingDragOtherKind GetOtherKind(Collider2D other)
	{
		if ((Object)(object)other == (Object)null)
		{
			return RingDragOtherKind.Unknown;
		}
		if (!RingColliderQuery.TryGetMarker(other, out var marker))
		{
			return RingDragOtherKind.Unknown;
		}
		return marker.Role;
	}

	private IGameplayObject GetOtherOwner(Collider2D other)
	{
		if ((Object)(object)other == (Object)null)
		{
			return null;
		}
		if (!RingColliderQuery.TryGetMarker(other, out var marker))
		{
			return null;
		}
		return marker.Owner;
	}
}
