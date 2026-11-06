using System.Collections.Generic;
using RotateRings.Common;
using UnityEngine;

public static class OpenRingThreadableGapQuery
{
	public struct RingGapGeometry
	{
		public float GapAngleDeg;

		public float GapDirectionDeg;

		public float InnerRadius;

		public float OuterRadius;

		public bool IsValid;
	}

	private sealed class PinGeometryCache
	{
		public readonly Transform Root;

		public readonly List<ColliderSlot> Slots = new List<ColliderSlot>(4);

		public float CoarseRadius;

		public bool HasCoarseRadius;

		public PinGeometryCache(Transform root)
		{
			Root = root;
		}
	}

	private sealed class ColliderSlot
	{
		public Collider2D Collider;

		public Transform Transform;

		public bool IsDragArea;

		public bool Active;

		public bool HasSnapshot;

		public Bounds Bounds;

		public Matrix4x4 Pose;

		public bool BoundsResyncPending;

		public float BoundsSampleFixedTime;

		public bool RadialValid;

		public Vector2 RadialCenter;

		public float MinR;

		public float MaxR;
	}

	private const float GapEpsilonDeg = 0.5f;

	private const float RadialSlop = 0.05f;

	private const float CoarseRadiusMargin = 1.2f;

	private static readonly Dictionary<Component, PinGeometryCache> pinGeometryCaches = new Dictionary<Component, PinGeometryCache>(32);

	public static bool HasThreadablePinInGap(RingObject ring, in RingGapGeometry geometry, Overlap2DHandler ringOverlapHandler, out int openCount, out int fullCount)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		using (new ProfilerScope("OpenRingThreadableGapQuery.HasThreadablePinInGap"))
		{
			openCount = 0;
			fullCount = 0;
			if ((Object)(object)ring == (Object)null || !geometry.IsValid || geometry.GapAngleDeg <= 0.5f)
			{
				return false;
			}
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return false;
			}
			IReadOnlyList<IGameplayObject> gameplayObjects = instance.GetGameplayObjects();
			if (gameplayObjects == null || gameplayObjects.Count == 0)
			{
				return false;
			}
			Transform transform = ((Component)ring).transform;
			Vector2 center = Vector2.op_Implicit(transform.position);
			float num = Mathf.Abs(transform.lossyScale.x);
			float num2 = Mathf.Abs(transform.lossyScale.y);
			float num3 = Mathf.Max(num, num2);
			float num4 = Mathf.Min(num, num2);
			if (num3 < 1E-06f)
			{
				num3 = 1f;
			}
			if (num4 < 1E-06f)
			{
				num4 = num3;
			}
			float inner = Mathf.Max(0f, geometry.InnerRadius * num3 - 0.05f);
			float outer = geometry.OuterRadius * num3 + 0.05f;
			float gapHalf = geometry.GapAngleDeg * 0.5f;
			float gapDirectionDeg = geometry.GapDirectionDeg;
			Matrix4x4 worldToLocalMatrix = transform.worldToLocalMatrix;
			for (int i = 0; i < gameplayObjects.Count; i++)
			{
				IGameplayObject gameplayObject = gameplayObjects[i];
				if (gameplayObject == null || gameplayObject.IsSolved)
				{
					continue;
				}
				IGameplayObject gameplayObject2 = gameplayObject;
				if (!(gameplayObject2 is PullObject pinRoot))
				{
					if (!(gameplayObject2 is FixedObject pinRoot2))
					{
						if (!(gameplayObject2 is RingObject ringObject) || !((Object)(object)ringObject != (Object)(object)ring))
						{
							continue;
						}
						IReadOnlyList<RingRelative> ringRelatives = ringObject.RingRelatives;
						if (ringRelatives == null)
						{
							continue;
						}
						for (int j = 0; j < ringRelatives.Count; j++)
						{
							RingRelative pinRoot3 = ringRelatives[j];
							if (IsPinCandidateActive((Component)(object)pinRoot3) && AccumulatePin((Component)(object)pinRoot3, center, inner, outer, num4, worldToLocalMatrix, gapDirectionDeg, gapHalf, ref openCount, ref fullCount))
							{
								return true;
							}
						}
					}
					else if (IsPinCandidateActive((Component)(object)pinRoot2) && AccumulatePin((Component)(object)pinRoot2, center, inner, outer, num4, worldToLocalMatrix, gapDirectionDeg, gapHalf, ref openCount, ref fullCount))
					{
						return true;
					}
				}
				else if (IsPinCandidateActive((Component)(object)pinRoot) && AccumulatePin((Component)(object)pinRoot, center, inner, outer, num4, worldToLocalMatrix, gapDirectionDeg, gapHalf, ref openCount, ref fullCount))
				{
					return true;
				}
			}
			return openCount >= 1;
		}
	}

	public static bool HasThreadablePinInGap(RingObject ring, in RingGapGeometry geometry, Overlap2DHandler ringOverlapHandler)
	{
		int openCount;
		int fullCount;
		return HasThreadablePinInGap(ring, in geometry, ringOverlapHandler, out openCount, out fullCount);
	}

	public static void ReleasePinCaches()
	{
		pinGeometryCaches.Clear();
	}

	private static bool IsPinCandidateActive(Component pinRoot)
	{
		if ((Object)(object)pinRoot == (Object)null)
		{
			return false;
		}
		Behaviour val = (Behaviour)(object)((pinRoot is Behaviour) ? pinRoot : null);
		if (val == null || !val.isActiveAndEnabled)
		{
			return false;
		}
		if (!(pinRoot is RingRelative ringRelative))
		{
			if (!(pinRoot is PullObject pullObject))
			{
				if (pinRoot is FixedObject fixedObject)
				{
					return !fixedObject.IsSolved;
				}
				return true;
			}
			return !pullObject.IsSolved;
		}
		if (ringRelative.IsSolved)
		{
			return false;
		}
		IGameplayObject ownerGameplayObject = ringRelative.GetOwnerGameplayObject();
		if (ownerGameplayObject != null)
		{
			return !ownerGameplayObject.IsSolved;
		}
		return true;
	}

	private static bool AccumulatePin(Component pinRoot, Vector2 center, float inner, float outer, float ringMinScale, Matrix4x4 worldToLocal, float gapDirectionDeg, float gapHalf, ref int openCount, ref int fullCount)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		PinGeometryCache orCreatePinGeometryCache = GetOrCreatePinGeometryCache(pinRoot);
		Vector2 val = Vector2.op_Implicit(orCreatePinGeometryCache.Root.position);
		if (IsPinFarFromGapSector(orCreatePinGeometryCache, val, center, inner, outer, ringMinScale, worldToLocal, gapDirectionDeg, gapHalf))
		{
			return false;
		}
		RefreshColliderSlots(orCreatePinGeometryCache);
		if (!IsWorldPointInGapSector(val, worldToLocal, gapDirectionDeg, gapHalf) && !AnySlotCenterInGapSector(orCreatePinGeometryCache, worldToLocal, gapDirectionDeg, gapHalf))
		{
			return false;
		}
		if (!IntersectsFullAnnulus(orCreatePinGeometryCache, center, inner, outer))
		{
			return false;
		}
		fullCount++;
		openCount++;
		return true;
	}

	private static PinGeometryCache GetOrCreatePinGeometryCache(Component pinRoot)
	{
		if (pinGeometryCaches.TryGetValue(pinRoot, out var value))
		{
			return value;
		}
		value = new PinGeometryCache(pinRoot.transform);
		List<Collider2D> list = GameApp.Pool.SpawnList<Collider2D>();
		pinRoot.GetComponentsInChildren<Collider2D>(true, list);
		for (int i = 0; i < list.Count; i++)
		{
			Collider2D val = list[i];
			if (!((Object)(object)val == (Object)null))
			{
				value.Slots.Add(new ColliderSlot
				{
					Collider = val,
					Transform = ((Component)val).transform,
					IsDragArea = (((Component)val).gameObject.layer == 6)
				});
			}
		}
		GameApp.Pool.RecycleList<Collider2D>(list);
		pinGeometryCaches[pinRoot] = value;
		return value;
	}

	private static void RefreshColliderSlots(PinGeometryCache cache)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		for (int i = 0; i < cache.Slots.Count; i++)
		{
			ColliderSlot colliderSlot = cache.Slots[i];
			Collider2D collider = colliderSlot.Collider;
			bool active = colliderSlot.Active;
			colliderSlot.Active = (Object)(object)collider != (Object)null && ((Behaviour)collider).enabled && !colliderSlot.IsDragArea;
			if (active != colliderSlot.Active)
			{
				flag = true;
			}
			if (!colliderSlot.Active)
			{
				continue;
			}
			Matrix4x4 localToWorldMatrix = colliderSlot.Transform.localToWorldMatrix;
			bool flag2 = !colliderSlot.HasSnapshot || colliderSlot.Pose != localToWorldMatrix;
			if (flag2 || (colliderSlot.BoundsResyncPending && Time.fixedTime != colliderSlot.BoundsSampleFixedTime))
			{
				Bounds bounds = collider.bounds;
				colliderSlot.BoundsResyncPending = flag2;
				colliderSlot.BoundsSampleFixedTime = Time.fixedTime;
				if (flag2 || colliderSlot.Bounds != bounds)
				{
					colliderSlot.RadialValid = false;
				}
				colliderSlot.HasSnapshot = true;
				colliderSlot.Pose = localToWorldMatrix;
				colliderSlot.Bounds = bounds;
				flag = true;
			}
		}
		if (flag || !cache.HasCoarseRadius)
		{
			RefreshCoarseRadius(cache);
		}
	}

	private static void RefreshCoarseRadius(PinGeometryCache cache)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		Vector2 center = Vector2.op_Implicit(cache.Root.position);
		float maxR = 0f;
		bool hasCoarseRadius = cache.Slots.Count > 0;
		for (int i = 0; i < cache.Slots.Count; i++)
		{
			ColliderSlot colliderSlot = cache.Slots[i];
			if (!colliderSlot.Active || !colliderSlot.HasSnapshot || colliderSlot.BoundsResyncPending)
			{
				hasCoarseRadius = false;
				continue;
			}
			Bounds bounds = colliderSlot.Bounds;
			Vector3 min = ((Bounds)(ref bounds)).min;
			Vector3 max = ((Bounds)(ref bounds)).max;
			ExpandMaxRadius(center, new Vector2(min.x, min.y), ref maxR);
			ExpandMaxRadius(center, new Vector2(min.x, max.y), ref maxR);
			ExpandMaxRadius(center, new Vector2(max.x, min.y), ref maxR);
			ExpandMaxRadius(center, new Vector2(max.x, max.y), ref maxR);
			ExpandMaxRadius(center, Vector2.op_Implicit(((Bounds)(ref bounds)).center), ref maxR);
		}
		cache.HasCoarseRadius = hasCoarseRadius;
		cache.CoarseRadius = maxR * 1.2f + 0.05f;
	}

	private static bool IsPinFarFromGapSector(PinGeometryCache cache, Vector2 rootWorld, Vector2 center, float inner, float outer, float ringMinScale, Matrix4x4 worldToLocal, float gapDirectionDeg, float gapHalf)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!cache.HasCoarseRadius)
		{
			return false;
		}
		float coarseRadius = cache.CoarseRadius;
		float num = rootWorld.x - center.x;
		float num2 = rootWorld.y - center.y;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		if (num3 - coarseRadius > outer || num3 + coarseRadius < inner)
		{
			return true;
		}
		if (num3 <= coarseRadius || ringMinScale < 1E-06f)
		{
			return false;
		}
		Vector3 val = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(Vector2.op_Implicit(rootWorld));
		float num4 = Mathf.Sqrt(val.x * val.x + val.y * val.y);
		if (num4 <= 1E-06f)
		{
			return false;
		}
		float num5 = Mathf.Asin(Mathf.Clamp01(coarseRadius / ringMinScale / num4)) * 57.29578f;
		return Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(val.y, val.x) * 57.29578f, gapDirectionDeg)) > gapHalf + num5 + 0.5f;
	}

	private static bool AnySlotCenterInGapSector(PinGeometryCache cache, Matrix4x4 worldToLocal, float gapDirectionDeg, float gapHalfDeg)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < cache.Slots.Count; i++)
		{
			ColliderSlot colliderSlot = cache.Slots[i];
			if (colliderSlot.Active && IsWorldPointInGapSector(Vector2.op_Implicit(((Bounds)(ref colliderSlot.Bounds)).center), worldToLocal, gapDirectionDeg, gapHalfDeg))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IntersectsFullAnnulus(PinGeometryCache cache, Vector2 center, float inner, float outer)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		float num2 = 0f;
		bool flag = false;
		for (int i = 0; i < cache.Slots.Count; i++)
		{
			ColliderSlot colliderSlot = cache.Slots[i];
			if (colliderSlot.Active)
			{
				EnsureRadialRange(colliderSlot, center);
				flag = true;
				if (colliderSlot.MinR < num)
				{
					num = colliderSlot.MinR;
				}
				if (colliderSlot.MaxR > num2)
				{
					num2 = colliderSlot.MaxR;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		if (num <= outer)
		{
			return num2 >= inner;
		}
		return false;
	}

	private static bool IsWorldPointInGapSector(Vector2 worldPoint, Matrix4x4 worldToLocal, float gapDirectionDeg, float gapHalfDeg)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(Vector2.op_Implicit(worldPoint));
		if (((Vector3)(ref val)).sqrMagnitude < 1E-08f)
		{
			return true;
		}
		return Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(val.y, val.x) * 57.29578f, gapDirectionDeg)) <= gapHalfDeg + 0.5f;
	}

	private static void EnsureRadialRange(ColliderSlot slot, Vector2 center)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (!slot.RadialValid || !(slot.RadialCenter == center))
		{
			Collider2D collider = slot.Collider;
			if (collider.OverlapPoint(center))
			{
				slot.MinR = 0f;
			}
			else
			{
				Vector2 val = collider.ClosestPoint(center);
				slot.MinR = Vector2.Distance(val, center);
			}
			Bounds bounds = slot.Bounds;
			Vector3 min = ((Bounds)(ref bounds)).min;
			Vector3 max = ((Bounds)(ref bounds)).max;
			float maxR = 0f;
			ExpandMaxRadius(center, new Vector2(min.x, min.y), ref maxR);
			ExpandMaxRadius(center, new Vector2(min.x, max.y), ref maxR);
			ExpandMaxRadius(center, new Vector2(max.x, min.y), ref maxR);
			ExpandMaxRadius(center, new Vector2(max.x, max.y), ref maxR);
			ExpandMaxRadius(center, Vector2.op_Implicit(((Bounds)(ref bounds)).center), ref maxR);
			slot.MaxR = maxR;
			slot.RadialCenter = center;
			slot.RadialValid = true;
		}
	}

	private static void ExpandMaxRadius(Vector2 center, Vector2 point, ref float maxR)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector2.Distance(center, point);
		if (num > maxR)
		{
			maxR = num;
		}
	}
}
