using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Authoring;
using XingYun.Render;
using XingYun.RingGame;

public static class RingColliderBuilder
{
	private readonly struct RingCapParams
	{
		public readonly float TailLength;

		public readonly float HeadLength;

		public readonly float CapRatio;

		public readonly float RenderGapDeg;

		public bool HasCaps
		{
			get
			{
				if (RenderGapDeg > 0.1f && CapRatio > 0.001f)
				{
					if (!(TailLength > 0.001f))
					{
						return HeadLength > 0.001f;
					}
					return true;
				}
				return false;
			}
		}

		public RingCapParams(float tailLength, float headLength, float capRatio, float renderGapDeg)
		{
			TailLength = tailLength;
			HeadLength = headLength;
			CapRatio = capRatio;
			RenderGapDeg = renderGapDeg;
		}
	}

	private const string TAG = "[UnlockRing][RingColliderBuilder] ";

	private static readonly ButterflyColliderBuilder butterflyColliderBuilder = new ButterflyColliderBuilder();

	private const float DegreesPerSegment = 8f;

	private const float EndCapLength = 0.5f;

	private const float ColliderCapRatio = 0.5f;

	private const float DragAreaPadding = 0.48f;

	private const int StickCapSegments = 8;

	private const float GuideChannelExtraHalfSpan = 0.1f;

	private const float GuideWallInwardInset = 0.04f;

	private const float GuideWallThickness = 0.07f;

	private const float GuideWallLengthShrink = 0.05f;

	private static void MarkColliderRole(Collider2D collider, RingDragOtherKind kind, Component owner)
	{
		if (!((Object)(object)collider == (Object)null))
		{
			ColliderRoleMarker colliderRoleMarker = ((Component)collider).GetComponent<ColliderRoleMarker>();
			if ((Object)(object)colliderRoleMarker == (Object)null)
			{
				colliderRoleMarker = ((Component)collider).gameObject.AddComponent<ColliderRoleMarker>();
			}
			colliderRoleMarker.AssignRole(kind);
			colliderRoleMarker.AssignOwner(owner);
		}
	}

	private static RingCapParams ResolveRingCapParams(RingSegmentController controller, float physicalGapDeg, float centerRadius)
	{
		float tailLength = 1f;
		float headLength = 1f;
		float num = RingGapGeometry.ComputeRenderGapDeg(physicalGapDeg, centerRadius, controller?.SharedMaterial, ((Object)(object)controller != (Object)null) ? controller.CapRatio : 0f);
		if ((Object)(object)controller != (Object)null)
		{
			num = controller.GapAngleDeg;
			RingGapGeometry.ReadCapLengths(controller.SharedMaterial, out tailLength, out headLength);
		}
		float capRatio = 0f;
		if (num > 0.1f)
		{
			capRatio = (((Object)(object)controller != (Object)null && controller.CapRatio > 0.001f) ? controller.CapRatio : 0.5f);
		}
		return new RingCapParams(tailLength, headLength, capRatio, num);
	}

	public static Collider2D[] BuildPhysic(RotatingRingDraft ringDraft, RingSegmentController controller, float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, float coordScale)
	{
		PolygonCollider2D component = ((Component)controller).gameObject.GetComponent<PolygonCollider2D>();
		if ((Object)(object)component == (Object)null)
		{
			return Array.Empty<Collider2D>();
		}
		CleanupHardBlockChildren(((Component)controller).transform);
		ApplyRingBodyPolygonCollider(component, controller, innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg, (Component)(object)((Component)controller).GetComponent<RingObject>());
		List<Vector2[]> list = new List<Vector2[]>();
		Collider2D[] result = SpawnAttachmentHardBlockColliders(controller, ringDraft, innerRadius, outerRadius, coordScale, list);
		ApplyRingDragAreaPolygonCollider(((Component)component).transform, innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg, list);
		return result;
	}

	public static void ApplyRingBodyPolygonCollider(PolygonCollider2D polygon, RingSegmentController capReferenceController, float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, Component owner)
	{
		if (!((Object)(object)polygon == (Object)null) && !((Object)(object)capReferenceController == (Object)null))
		{
			NormalizeRadii(innerRadius, outerRadius, out innerRadius, out outerRadius);
			float centerRadius = (innerRadius + outerRadius) * 0.5f;
			RingCapParams value = ResolveRingCapParams(capReferenceController, gapAngleDeg, centerRadius);
			Vector2[][] array = BuildRingBandPaths(innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg, ignoreVisualCap: false, value);
			polygon.pathCount = array.Length;
			for (int i = 0; i < array.Length; i++)
			{
				polygon.SetPath(i, array[i]);
			}
			MarkColliderRole((Collider2D)(object)polygon, RingDragOtherKind.RingBody, owner);
		}
	}

	public static void ApplyRingDragAreaPolygonCollider(Transform colliderRoot, float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, IReadOnlyList<Vector2[]> extraPaths = null)
	{
		if ((Object)(object)colliderRoot == (Object)null)
		{
			return;
		}
		Transform val = colliderRoot.Find("DragArea");
		if ((Object)(object)val == (Object)null)
		{
			Debug.LogWarning((object)"[RingColliderBuilder] 未找到 DragArea 子节点，跳过拖拽热区设置。");
			return;
		}
		PolygonCollider2D component = ((Component)val).GetComponent<PolygonCollider2D>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogWarning((object)"[RingColliderBuilder] DragArea 上未找到 PolygonCollider2D，跳过拖拽热区设置。");
			return;
		}
		float innerRadius2 = innerRadius - 0.48f;
		float outerRadius2 = outerRadius + 0.48f;
		BuildPolygonysicDragArea(component, innerRadius2, outerRadius2, gapAngleDeg, gapDirectionDeg, extraPaths);
	}

	public static void BuildPolygonysicDragArea(PolygonCollider2D polygon, float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, IReadOnlyList<Vector2[]> extraPaths = null)
	{
		Vector2[][] array = BuildRingBandPaths(innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg, ignoreVisualCap: true);
		int num = array.Length;
		int num2 = extraPaths?.Count ?? 0;
		polygon.pathCount = num + num2;
		for (int i = 0; i < num; i++)
		{
			polygon.SetPath(i, array[i]);
		}
		for (int j = 0; j < num2; j++)
		{
			polygon.SetPath(num + j, extraPaths[j]);
		}
		SetupDragArea(((Component)polygon).transform, outerRadius);
	}

	private static void SetupDragArea(Transform root, float outerRadius)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Transform val = root.Find("DragArea");
		if (!((Object)(object)val == (Object)null))
		{
			CircleCollider2D component = ((Component)val).GetComponent<CircleCollider2D>();
			if (!((Object)(object)component == (Object)null))
			{
				((Collider2D)component).offset = Vector2.zero;
				component.radius = outerRadius;
				ApplyDragAreaNormalLocalZ(val);
			}
		}
	}

	public static void BuildPhysic(SlidingStickDraft stickDraft, ArrowRibbonController controller, List<Vector3> path, float radius, float coordScale)
	{
		PolygonCollider2D component = ((Component)controller).gameObject.GetComponent<PolygonCollider2D>();
		if (!((Object)(object)component == (Object)null) && path != null && path.Count >= 2)
		{
			float num = Mathf.Max(0.01f, radius);
			float cornerRadius = num * 2f;
			int cornerSegments = Mathf.Max(1, controller.BuildSettings.cornerSegments);
			Vector2[] array = BuildRibbonColliderPath(path, num, cornerRadius, cornerSegments);
			component.pathCount = 1;
			component.SetPath(0, array);
			MarkColliderRole((Collider2D)(object)component, RingDragOtherKind.StickBody, (Component)(object)((Component)controller).GetComponent<PullObject>());
			float halfWidth = num + 0.48f;
			Vector2[] colliderPath = BuildRibbonColliderPath(path, halfWidth, cornerRadius, cornerSegments, num);
			SetUpStickDragArea(((Component)component).transform, colliderPath);
		}
	}

	private static void SetUpStickDragArea(Transform root, Vector2[] colliderPath)
	{
		Transform val = root.Find("DragArea");
		if (!((Object)(object)val == (Object)null))
		{
			PolygonCollider2D component = ((Component)val).GetComponent<PolygonCollider2D>();
			if (!((Object)(object)component == (Object)null))
			{
				component.pathCount = 1;
				component.SetPath(0, colliderPath);
				ApplyDragAreaNormalLocalZ(val);
			}
		}
	}

	private static void ApplyDragAreaNormalLocalZ(Transform dragArea)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)dragArea == (Object)null))
		{
			Vector3 localPosition = dragArea.localPosition;
			localPosition.z = 0.1f;
			dragArea.localPosition = localPosition;
		}
	}

	private static Vector2[] BuildRibbonColliderPath(List<Vector3> path, float halfWidth, float cornerRadius, int cornerSegments, float endpointRetractHalfWidth = -1f)
	{
		float endpointRetractHalfWidth2 = ((endpointRetractHalfWidth >= 0f) ? endpointRetractHalfWidth : halfWidth);
		float num = halfWidth * 1.15f;
		if (cornerRadius < num)
		{
			cornerRadius = num;
		}
		List<Vector3> list = CleanPathPoints(path);
		if (list.Count < 2)
		{
			return BuildStraightColliderPath(path, halfWidth, endpointRetractHalfWidth2);
		}
		List<Vector3> list2;
		if (list.Count >= 3)
		{
			list2 = BuildRoundedCenterline(list, cornerRadius, cornerSegments);
			if (list2.Count < 2)
			{
				return BuildStraightColliderPath(path, halfWidth, endpointRetractHalfWidth2);
			}
		}
		else
		{
			list2 = list;
		}
		return AssembleRibbonColliderOutline(list2, halfWidth, endpointRetractHalfWidth2);
	}

	private static void RetractCenterlineEndpoints(List<Vector3> centerline, float retractHalfWidth)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		int count = centerline.Count;
		if (count >= 2)
		{
			float num = retractHalfWidth * 2f;
			Vector3 val = centerline[1] - centerline[0];
			if (((Vector3)(ref val)).sqrMagnitude > 1E-08f)
			{
				List<Vector3> list = centerline;
				list[0] = list[0] + ((Vector3)(ref val)).normalized * num;
			}
			Vector3 val2 = centerline[count - 1] - centerline[count - 2];
			if (((Vector3)(ref val2)).sqrMagnitude > 1E-08f)
			{
				List<Vector3> list = centerline;
				int index = count - 1;
				list[index] -= ((Vector3)(ref val2)).normalized * num;
			}
		}
	}

	private static Vector2[] AssembleRibbonColliderOutline(List<Vector3> centerline, float halfWidth, float endpointRetractHalfWidth = -1f)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		float retractHalfWidth = ((endpointRetractHalfWidth >= 0f) ? endpointRetractHalfWidth : halfWidth);
		List<Vector3> list = new List<Vector3>(centerline);
		RetractCenterlineEndpoints(list, retractHalfWidth);
		int count = list.Count;
		List<Vector2> list2 = new List<Vector2>(count);
		List<Vector2> list3 = new List<Vector2>(count);
		Vector2 val = default(Vector2);
		for (int i = 0; i < count; i++)
		{
			Vector2 centerlineTangent = GetCenterlineTangent(list, i);
			((Vector2)(ref val))._002Ector(0f - centerlineTangent.y, centerlineTangent.x);
			Vector2 val2 = Vector2.op_Implicit(list[i]);
			list2.Add(val2 + val * halfWidth);
			list3.Add(val2 - val * halfWidth);
		}
		List<Vector2> list4 = new List<Vector2>(count * 2 + 16);
		list4.AddRange(list2);
		Vector2 center = Vector2.op_Implicit(list[count - 1]);
		Vector2 endpointOutward = GetEndpointOutward(list, count - 1);
		AppendSemicircleCapBetween(list4, center, list2[count - 1], list3[count - 1], endpointOutward, halfWidth, 8, 1, 8, halfWidth);
		for (int num = count - 2; num >= 0; num--)
		{
			list4.Add(list3[num]);
		}
		Vector2 center2 = Vector2.op_Implicit(list[0]);
		Vector2 endpointOutward2 = GetEndpointOutward(list, 0);
		AppendSemicircleCapBetween(list4, center2, list3[0], list2[0], endpointOutward2, halfWidth, 8, 1, 7, halfWidth);
		return list4.ToArray();
	}

	private static Vector2 GetEndpointOutward(List<Vector3> centerline, int index)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (index <= 0)
		{
			Vector2 val = Vector2.op_Implicit(centerline[0]) - Vector2.op_Implicit(centerline[1]);
			if (((Vector2)(ref val)).sqrMagnitude < 1E-08f)
			{
				return Vector2.right;
			}
			return ((Vector2)(ref val)).normalized;
		}
		Vector2 val2 = Vector2.op_Implicit(centerline[index]) - Vector2.op_Implicit(centerline[index - 1]);
		if (((Vector2)(ref val2)).sqrMagnitude < 1E-08f)
		{
			return Vector2.right;
		}
		return ((Vector2)(ref val2)).normalized;
	}

	private static Vector2 GetCenterlineTangent(List<Vector3> centerline, int index)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		int count = centerline.Count;
		Vector2 val = Vector2.op_Implicit((index > 0) ? centerline[index - 1] : centerline[index]);
		Vector2 val2 = Vector2.op_Implicit((index < count - 1) ? centerline[index + 1] : centerline[index]) - val;
		if (((Vector2)(ref val2)).sqrMagnitude < 1E-08f)
		{
			val2 = Vector2.right;
		}
		return ((Vector2)(ref val2)).normalized;
	}

	private static void AppendSemicircleCapBetween(List<Vector2> result, Vector2 center, Vector2 fromRail, Vector2 toRail, Vector2 outward, float halfWidth, int segments, int firstSegment, int lastSegment, float capRadius)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(2, segments);
		Vector2 val = ((((Vector2)(ref outward)).sqrMagnitude > 1E-08f) ? ((Vector2)(ref outward)).normalized : Vector2.right);
		Vector2 val2 = fromRail - center;
		if (((Vector2)(ref val2)).sqrMagnitude < 1E-08f)
		{
			val2 = new Vector2(0f - val.y, val.x) * capRadius;
		}
		else
		{
			((Vector2)(ref val2)).Normalize();
		}
		bool flag = val2.x * val.y - val2.y * val.x > 0f;
		float num2 = Mathf.Atan2(val2.y, val2.x);
		for (int i = firstSegment; i <= lastSegment; i++)
		{
			float num3 = (float)i / (float)num;
			float num4 = (flag ? (num2 + MathF.PI * num3) : (num2 - MathF.PI * num3));
			result.Add(center + new Vector2(Mathf.Cos(num4), Mathf.Sin(num4)) * capRadius);
		}
	}

	private static Vector2[] BuildStraightColliderPath(List<Vector3> path, float halfWidth, float endpointRetractHalfWidth = -1f)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.Count < 2)
		{
			return Array.Empty<Vector2>();
		}
		return AssembleRibbonColliderOutline(new List<Vector3>(2)
		{
			path[0],
			path[path.Count - 1]
		}, halfWidth, endpointRetractHalfWidth);
	}

	private static List<Vector3> CleanPathPoints(IList<Vector3> input)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>(input.Count);
		Vector3 val2;
		for (int i = 0; i < input.Count; i++)
		{
			Vector3 val = input[i];
			if (list.Count != 0)
			{
				val2 = val - list[list.Count - 1];
				if (!(((Vector3)(ref val2)).sqrMagnitude > 9.999999E-09f))
				{
					continue;
				}
			}
			list.Add(val);
		}
		if (list.Count >= 3)
		{
			int num = 1;
			int num2 = list.Count - 1;
			for (int j = 1; j < num2; j++)
			{
				Vector3 val3 = list[j] - list[num - 1];
				Vector3 val4 = list[j + 1] - list[j];
				if (!(((Vector3)(ref val3)).sqrMagnitude < 9.999999E-09f) && !(((Vector3)(ref val4)).sqrMagnitude < 9.999999E-09f))
				{
					((Vector3)(ref val3)).Normalize();
					((Vector3)(ref val4)).Normalize();
					val2 = Vector3.Cross(val3, val4);
					if (((Vector3)(ref val2)).magnitude >= 0.001f || Vector3.Dot(val3, val4) <= 0f)
					{
						list[num++] = list[j];
					}
				}
			}
			list[num++] = list[num2];
			list.RemoveRange(num, list.Count - num);
		}
		return list;
	}

	private static List<Vector3> BuildRoundedCenterline(List<Vector3> points, float cornerRadius, int segments)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>(points.Count * 2);
		int count = points.Count;
		if (count < 3)
		{
			list.AddRange(points);
			return list;
		}
		int num = Mathf.Max(1, segments);
		float num2 = Mathf.Max(0f, cornerRadius);
		list.Add(points[0]);
		Vector3 val15;
		for (int i = 1; i < count - 1; i++)
		{
			Vector3 val = points[i - 1];
			Vector3 val2 = points[i];
			Vector3 val3 = points[i + 1];
			Vector3 val4 = val2 - val;
			Vector3 val5 = val3 - val2;
			float magnitude = ((Vector3)(ref val4)).magnitude;
			float magnitude2 = ((Vector3)(ref val5)).magnitude;
			if (magnitude < 0.0001f || magnitude2 < 0.0001f)
			{
				list.Add(val2);
				continue;
			}
			Vector3 val6 = val4 / magnitude;
			Vector3 val7 = val5 / magnitude2;
			float num3 = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-val6, val7), -1f, 1f));
			if (num3 < 0.02f || num3 > 3.1215928f)
			{
				list.Add(val2);
				continue;
			}
			float num4 = num3 * 0.5f;
			float num5 = Mathf.Tan(num4);
			float num6 = Mathf.Sin(num4);
			float num7 = ((num5 > 0.0001f) ? (num2 / num5) : 0f);
			float num8 = 0.5f * Mathf.Min(magnitude, magnitude2);
			float num9 = Mathf.Min(num7, num8);
			float num10 = num9 * num5;
			if (num9 < 0.0001f || num10 < 0.0001f)
			{
				list.Add(val2);
				continue;
			}
			Vector3 val8 = val2 - val6 * num9;
			Vector3 val9 = val2 + val7 * num9;
			Vector3 val10 = -val6 + val7;
			if (((Vector3)(ref val10)).sqrMagnitude < 9.999999E-09f)
			{
				list.Add(val2);
				continue;
			}
			((Vector3)(ref val10)).Normalize();
			Vector3 val11 = val2 + val10 * (num10 / num6);
			Vector3 val12 = val8 - val11;
			Vector3 val13 = val9 - val11;
			for (int j = 0; j <= num; j++)
			{
				float num11 = (float)j / (float)num;
				Vector3 val14 = val11 + Vector3.Slerp(val12, val13, num11);
				val15 = val14 - list[list.Count - 1];
				if (((Vector3)(ref val15)).sqrMagnitude > 9.999999E-09f)
				{
					list.Add(val14);
				}
			}
		}
		val15 = points[count - 1] - list[list.Count - 1];
		if (((Vector3)(ref val15)).sqrMagnitude > 9.999999E-09f)
		{
			list.Add(points[count - 1]);
		}
		return list;
	}

	private static float ApproximatePathRadius(List<Vector3> path)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.Count < 2)
		{
			return 1f;
		}
		Vector2 val = Vector2.op_Implicit(path[0]);
		Vector2 val2 = Vector2.op_Implicit(path[path.Count - 1]) - val;
		float magnitude = ((Vector2)(ref val2)).magnitude;
		return Mathf.Max(1f, magnitude * 0.5f);
	}

	public static void BuildPhysicButterflyBar(FixedButterflyObject brow, FixedButterflyMeshController mesh, float slotGuideLength, float slotGuideThickness, float bridgeLength, float bridgeWidth)
	{
		butterflyColliderBuilder.BuildPhysic(brow, mesh, slotGuideLength, slotGuideThickness, bridgeLength, bridgeWidth);
	}

	public static void BuildPhysic(FixedCatchGuideDraft draft, GameObject go, float guideLength, float guideThickness, float radius, float localScale)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider2D component = go.GetComponent<BoxCollider2D>();
		if ((Object)(object)component == (Object)null || localScale <= 0f)
		{
			return;
		}
		float num = 1f / localScale;
		if ((int)draft.Kind == 0)
		{
			FixedObject component2 = go.GetComponent<FixedObject>();
			FixedButterflyRelative.EnsureOnAnchor(go.transform, ((EditablePartDraft)draft).Id, component2);
			if (!draft.IsReleasable)
			{
				float num2 = Mathf.Max(0.01f, guideLength);
				float num3 = Mathf.Max(0.01f, guideThickness * num);
				component.size = new Vector2(num2, num3);
				((Collider2D)component).offset = Vector2.op_Implicit(new Vector3(0f, 0f));
				MarkColliderRole((Collider2D)(object)component, RingDragOtherKind.CatchGuideBody, (Component)(object)go.GetComponent<FixedObject>());
				SetupGuideWallBoxColliders(go.transform, Vector2.op_Implicit(component.size));
			}
		}
		else
		{
			float num4 = Mathf.Max(0.01f, radius * num);
			component.size = new Vector2(num4 * 2f, num4 * 2f);
			((Collider2D)component).offset = Vector2.op_Implicit(new Vector3(0f, 0f));
			MarkColliderRole((Collider2D)(object)component, RingDragOtherKind.CatchGuideBody, (Component)(object)go.GetComponent<FixedObject>());
		}
	}

	private static void SetupGuideWallBoxColliders(Transform root, Vector3 bodySize, FixedObject fixedOwner = null)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		if (fixedOwner == null)
		{
			fixedOwner = ((Component)root).GetComponent<FixedObject>();
		}
		float num = bodySize.y * 0.5f + 0.1f - 0.04f;
		float num2 = 0.07f;
		float num3 = ResolveGuideWallWidth(root, Vector2.op_Implicit(bodySize));
		Transform val = root.Find("Top");
		if ((Object)(object)val != (Object)null)
		{
			Vector3 localPosition = val.localPosition;
			localPosition.y = num;
			val.localPosition = localPosition;
			BoxCollider2D val2 = ((Component)val).GetComponent<BoxCollider2D>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)val).gameObject.AddComponent<BoxCollider2D>();
			}
			((Collider2D)val2).isTrigger = false;
			val2.size = new Vector2(num3, num2);
			((Collider2D)val2).offset = new Vector2(0f, num2 * 0.5f);
			MarkColliderRole((Collider2D)(object)val2, RingDragOtherKind.CatchGuideWall, (Component)(object)fixedOwner);
			((Component)val).gameObject.layer = 7;
		}
		Transform val3 = root.Find("Bottom");
		if ((Object)(object)val3 != (Object)null)
		{
			Vector3 localPosition2 = val3.localPosition;
			localPosition2.y = 0f - num;
			val3.localPosition = localPosition2;
			BoxCollider2D val4 = ((Component)val3).GetComponent<BoxCollider2D>();
			if ((Object)(object)val4 == (Object)null)
			{
				val4 = ((Component)val3).gameObject.AddComponent<BoxCollider2D>();
			}
			((Collider2D)val4).isTrigger = false;
			val4.size = new Vector2(num3, num2);
			((Collider2D)val4).offset = new Vector2(0f, (0f - num2) * 0.5f);
			MarkColliderRole((Collider2D)(object)val4, RingDragOtherKind.CatchGuideWall, (Component)(object)fixedOwner);
			((Component)val3).gameObject.layer = 7;
		}
	}

	private static float ResolveGuideWallWidth(Transform root, Vector2 bodySize)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		FixedButterflyMeshController fixedButterflyMeshController = ((Component)root).GetComponent<FixedButterflyMeshController>() ?? ((Component)root).GetComponentInParent<FixedButterflyMeshController>();
		if ((Object)(object)fixedButterflyMeshController != (Object)null)
		{
			return Mathf.Max(0.01f, fixedButterflyMeshController.MeshCapSpan - 0.1f);
		}
		QuadPlaneController component = ((Component)root).GetComponent<QuadPlaneController>();
		float num = (((Object)(object)component != (Object)null) ? component.Height : bodySize.x);
		return Mathf.Max(0.01f, num - 0.1f);
	}

	private static void CleanupHardBlockChildren(Transform ringRoot)
	{
		for (int num = ringRoot.childCount - 1; num >= 0; num--)
		{
			Transform child = ringRoot.GetChild(num);
			if ((Object)(object)child != (Object)null && ((Object)child).name.StartsWith("HardBlock_", StringComparison.Ordinal))
			{
				Object.DestroyImmediate((Object)(object)((Component)child).gameObject);
			}
		}
	}

	private static Collider2D[] SpawnAttachmentHardBlockColliders(RingSegmentController controller, RotatingRingDraft ringDraft, float innerRadius, float outerRadius, float coordScale, List<Vector2[]> pinDragPaths)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Invalid comparison between Unknown and I4
		if (((ringDraft != null) ? ((EditablePartDraft)ringDraft).Components : null) == null || ((EditablePartDraft)ringDraft).Components.Count == 0)
		{
			return Array.Empty<Collider2D>();
		}
		List<Collider2D> list = new List<Collider2D>(((EditablePartDraft)ringDraft).Components.Count);
		for (int i = 0; i < ((EditablePartDraft)ringDraft).Components.Count; i++)
		{
			EditableBodyComponentDraft obj = ((EditablePartDraft)ringDraft).Components[i];
			EditableRadialPinDraft val = (EditableRadialPinDraft)(object)((obj is EditableRadialPinDraft) ? obj : null);
			if (val != null)
			{
				Vector2[] path = BuildRadialPinStemPath(val, innerRadius, outerRadius, coordScale);
				string childName = "HardBlock_" + ((EditableBodyComponentDraft)val).Id;
				PolygonCollider2D item = SpawnHardBlockCollider(((Component)controller).transform, childName, path);
				list.Add((Collider2D)(object)item);
				if ((int)val.Kind == 0)
				{
					SpawnRingRelative(controller, val, innerRadius, outerRadius, coordScale);
				}
				else if ((int)val.Kind == 1)
				{
					pinDragPaths.Add(BuildRadialPinStemPath(val, innerRadius, outerRadius, coordScale, 0.48f));
				}
			}
		}
		return list.ToArray();
	}

	private static PolygonCollider2D SpawnHardBlockCollider(Transform parent, string childName, Vector2[] path)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Transform val = parent.Find(childName);
		GameObject val2 = (GameObject)(((Object)(object)val != (Object)null) ? ((object)((Component)val).gameObject) : ((object)new GameObject(childName)));
		if ((Object)(object)val == (Object)null)
		{
			val2.transform.SetParent(parent, false);
			val2.layer = ((Component)parent).gameObject.layer;
		}
		PolygonCollider2D val3 = val2.GetComponent<PolygonCollider2D>();
		if ((Object)(object)val3 == (Object)null)
		{
			val3 = val2.AddComponent<PolygonCollider2D>();
		}
		val3.pathCount = 1;
		val3.SetPath(0, path);
		MarkColliderRole((Collider2D)(object)val3, RingDragOtherKind.HardBlockPin, (Component)(object)((Component)parent).GetComponent<RingObject>());
		return val3;
	}

	private static void SpawnRingRelative(RingSegmentController controller, EditableRadialPinDraft pin, float innerRadius, float outerRadius, float coordScale)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Resources.Load<GameObject>("PhysicRingRelative");
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		GameObject obj = Object.Instantiate<GameObject>(val, ((Component)controller).transform, false);
		float num = (float)pin.HeadLength * coordScale;
		float num2 = (float)pin.HeadWidth * coordScale;
		float num3 = 0f - (float)pin.MountAngleDegrees;
		bool flag = (int)pin.MountSide == 0;
		float num4 = (float)pin.StemLength * coordScale;
		float num5 = (innerRadius + outerRadius) / 2f;
		float num6 = (flag ? (num5 + num4 + num / 2f) : (num5 - num4 - num / 2f));
		float num7 = num3 * (MathF.PI / 180f);
		obj.transform.localPosition = new Vector3(Mathf.Cos(num7) * num6, Mathf.Sin(num7) * num6, 0f);
		float num8 = (flag ? num3 : (num3 + 180f));
		obj.transform.localRotation = Quaternion.Euler(0f, 0f, num8);
		float num9 = Mathf.Max(0.01f, num2 - 0.15f);
		BoxCollider2D component = obj.GetComponent<BoxCollider2D>();
		if ((Object)(object)component != (Object)null)
		{
			component.size = new Vector2(num, num9);
			((Collider2D)component).offset = Vector2.zero;
		}
		MarkColliderRole((Collider2D)(object)component, RingDragOtherKind.RingRelativeBody, (Component)(object)((Component)controller).GetComponent<RingObject>());
		float num10 = num * 0.5f + 0.025f + 0.0145808f;
		Component component2 = (Component)(object)((Component)controller).GetComponent<RingObject>();
		Transform val2 = obj.transform.Find("Top");
		if ((Object)(object)val2 != (Object)null)
		{
			val2.localPosition = Vector3.zero;
			BoxCollider2D component3 = ((Component)val2).GetComponent<BoxCollider2D>();
			if ((Object)(object)component3 != (Object)null)
			{
				component3.size = new Vector2(0.05f, num9);
				((Collider2D)component3).offset = new Vector2(flag ? num10 : (0f - num10), 0f);
			}
			MarkColliderRole((Collider2D)(object)component3, RingDragOtherKind.RingRelativeWall, component2);
			((Component)val2).gameObject.layer = 7;
		}
		Transform val3 = obj.transform.Find("Bottom");
		if ((Object)(object)val3 != (Object)null)
		{
			val3.localPosition = Vector3.zero;
			BoxCollider2D component4 = ((Component)val3).GetComponent<BoxCollider2D>();
			if ((Object)(object)component4 != (Object)null)
			{
				component4.size = new Vector2(0.05f, num9);
				((Collider2D)component4).offset = new Vector2(flag ? (0f - num10) : num10, 0f);
			}
			MarkColliderRole((Collider2D)(object)component4, RingDragOtherKind.RingRelativeWall, component2);
			((Component)val3).gameObject.layer = 7;
		}
		RingRelative component5 = obj.GetComponent<RingRelative>();
		if (!((Object)(object)component5 == (Object)null))
		{
			component5.FeatureId = ((EditableBodyComponentDraft)pin).PrimaryFeatureId;
			RingObject component6 = ((Component)controller).GetComponent<RingObject>();
			if ((Object)(object)component6 != (Object)null)
			{
				component6.AddRingRelative(component5);
			}
		}
	}

	private static Vector2[] BuildRadialPinStemPath(EditableRadialPinDraft pin, float innerRadius, float outerRadius, float coordScale, float padding = 0f)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Invalid comparison between Unknown and I4
		float angleDeg = 0f - (float)pin.MountAngleDegrees;
		bool flag = (int)pin.MountSide == 0;
		float num = (float)pin.StemLength * coordScale;
		float num2 = Mathf.Max(0.01f, (float)pin.StemRadius * 2f * coordScale);
		float startR = (flag ? outerRadius : innerRadius);
		float num3 = (flag ? ((outerRadius + innerRadius) / 2f + num) : Mathf.Max(0f, (outerRadius + innerRadius) / 2f - num));
		if ((int)pin.Kind == 1)
		{
			float num4 = num2 * 0.5f + 0.125f - 0.03f;
			num3 = (flag ? (num3 + num4) : Mathf.Max(0f, num3 - num4));
			float width = num2 + padding * 2f;
			float endR = (flag ? (num3 + padding) : Mathf.Max(0f, num3 - padding));
			return BuildRadialBarPathWithTipCap(angleDeg, startR, endR, width);
		}
		return BuildRadialBarPath(angleDeg, startR, num3, num2);
	}

	private static Vector2[] BuildRadialBarPath(float angleDeg, float startR, float endR, float width)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		float num = angleDeg * (MathF.PI / 180f);
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		float num4 = 0f - num3;
		float num5 = num2;
		float num6 = width * 0.5f;
		return (Vector2[])(object)new Vector2[4]
		{
			new Vector2(startR * num2 - num6 * num4, startR * num3 - num6 * num5),
			new Vector2(endR * num2 - num6 * num4, endR * num3 - num6 * num5),
			new Vector2(endR * num2 + num6 * num4, endR * num3 + num6 * num5),
			new Vector2(startR * num2 + num6 * num4, startR * num3 + num6 * num5)
		};
	}

	private static Vector2[] BuildRadialBarPathWithTipCap(float angleDeg, float startR, float endR, float width)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		float num = angleDeg * (MathF.PI / 180f);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Mathf.Cos(num), Mathf.Sin(num));
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(0f - val.y, val.x);
		float num2 = width * 0.5f;
		Vector2 val3 = val * startR;
		Vector2 val4 = val * endR;
		Vector2 val5 = ((endR >= startR) ? val : (-val));
		Vector2 val6 = val4 - val5 * num2;
		Vector2 item = val3 - val2 * num2;
		Vector2 item2 = val3 + val2 * num2;
		Vector2 val7 = val6 - val2 * num2;
		Vector2 toRail = val6 + val2 * num2;
		List<Vector2> obj = new List<Vector2>(11) { item, val7 };
		AppendSemicircleCapBetween(obj, val6, val7, toRail, val5, num2, 8, 1, 8, num2);
		obj.Add(item2);
		return obj.ToArray();
	}

	public static Vector2[][] BuildRingBandPaths(float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, bool ignoreVisualCap = false)
	{
		return BuildRingBandPaths(innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg, ignoreVisualCap, null);
	}

	private static Vector2[][] BuildRingBandPaths(float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, bool ignoreVisualCap, RingCapParams? capParams)
	{
		NormalizeRadii(innerRadius, outerRadius, out innerRadius, out outerRadius);
		gapAngleDeg = NormalizeGapDeg(gapAngleDeg);
		if (gapAngleDeg <= 0f)
		{
			return BuildClosedRingPaths(innerRadius, outerRadius);
		}
		RingCapParams capParams2 = capParams ?? ResolveRingCapParams(null, gapAngleDeg, (innerRadius + outerRadius) * 0.5f);
		return new Vector2[1][] { BuildCShapeOutline(innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg, ignoreVisualCap, capParams2) };
	}

	private static Vector2[][] BuildClosedRingPaths(float innerRadius, float outerRadius)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		int num = SegmentsForSweep(360f);
		Vector2[] array = (Vector2[])(object)new Vector2[num];
		Vector2[] array2 = (Vector2[])(object)new Vector2[num];
		for (int i = 0; i < num; i++)
		{
			float num2 = (float)i / (float)num * MathF.PI * 2f;
			array[i] = new Vector2(Mathf.Cos(num2) * outerRadius, Mathf.Sin(num2) * outerRadius);
			array2[i] = new Vector2(Mathf.Cos(0f - num2) * innerRadius, Mathf.Sin(0f - num2) * innerRadius);
		}
		return new Vector2[2][] { array, array2 };
	}

	private static Vector2[] BuildCShapeOutline(float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg, bool ignoreVisualCap, RingCapParams capParams)
	{
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		if (ignoreVisualCap || !capParams.HasCaps)
		{
			return BuildPlainCShapeOutline(innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg);
		}
		float num = (innerRadius + outerRadius) * 0.5f;
		float num2 = gapDirectionDeg * (MathF.PI / 180f);
		float num3 = capParams.RenderGapDeg * 0.5f * (MathF.PI / 180f);
		float num4 = (360f - capParams.RenderGapDeg) * (MathF.PI / 180f);
		float num5 = num2 + num3;
		float num6 = num5 + num4;
		float num7 = ((num > 0.001f) ? (capParams.TailLength * capParams.CapRatio / num) : 0f);
		float num8 = ((num > 0.001f) ? (capParams.HeadLength * capParams.CapRatio / num) : 0f);
		float num9 = capParams.TailLength * capParams.CapRatio;
		float num10 = capParams.HeadLength * capParams.CapRatio;
		bool flag = capParams.CapRatio > 0.001f && capParams.TailLength > 0.001f && num9 > 0.001f;
		bool flag2 = capParams.CapRatio > 0.001f && capParams.HeadLength > 0.001f && num10 > 0.001f;
		float num11 = (flag ? (num5 + num7) : num5);
		float num12 = (flag2 ? (num6 - num8) : num6);
		if (num12 <= num11 + 0.0001f)
		{
			return BuildPlainCShapeOutline(innerRadius, outerRadius, gapAngleDeg, gapDirectionDeg);
		}
		int num13 = SegmentsForSweep((num12 - num11) * 57.29578f);
		float num14 = num12 - num11;
		List<Vector2> list = new List<Vector2>((num13 + 1) * 2 + 16);
		for (int i = 0; i <= num13; i++)
		{
			float angleRad = num11 + num14 * ((float)i / (float)num13);
			list.Add(RadialPoint(angleRad, outerRadius));
		}
		if (flag2)
		{
			AppendRingSemicircleCap(list, num12, innerRadius, outerRadius, isEndCap: true);
		}
		for (int j = 0; j <= num13; j++)
		{
			float angleRad2 = num12 - num14 * ((float)j / (float)num13);
			list.Add(RadialPoint(angleRad2, innerRadius));
		}
		if (flag)
		{
			AppendRingSemicircleCap(list, num11, innerRadius, outerRadius, isEndCap: false);
		}
		return list.ToArray();
	}

	private static void AppendRingSemicircleCap(List<Vector2> result, float angleRad, float innerRadius, float outerRadius, bool isEndCap)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		float num = (outerRadius - innerRadius) * 0.5f;
		float num2 = (innerRadius + outerRadius) * 0.5f;
		Vector2 val = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
		Vector2 outward = (isEndCap ? GapOutwardEnd(angleRad) : GapOutwardStart(angleRad));
		Vector2 val2 = val * outerRadius;
		Vector2 val3 = val * innerRadius;
		Vector2 center = val * num2;
		Vector2 fromRail = (isEndCap ? val2 : val3);
		Vector2 toRail = (isEndCap ? val3 : val2);
		AppendSemicircleCapBetween(result, center, fromRail, toRail, outward, num, 8, 1, 7, num);
	}

	private static Vector2[] BuildPlainCShapeOutline(float innerRadius, float outerRadius, float gapAngleDeg, float gapDirectionDeg)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		float num = gapDirectionDeg * (MathF.PI / 180f);
		float num2 = gapAngleDeg * 0.5f * (MathF.PI / 180f);
		float num3 = (360f - gapAngleDeg) * (MathF.PI / 180f);
		float num4 = num + num2;
		float num5 = num4 + num3;
		int num6 = SegmentsForSweep(360f - gapAngleDeg);
		Vector2[] array = (Vector2[])(object)new Vector2[num6 * 2 + 2];
		int num7 = 0;
		for (int i = 0; i <= num6; i++)
		{
			float angleRad = num4 + num3 * ((float)i / (float)num6);
			array[num7++] = RadialPoint(angleRad, outerRadius);
		}
		for (int j = 0; j <= num6; j++)
		{
			float angleRad2 = num5 - num3 * ((float)j / (float)num6);
			array[num7++] = RadialPoint(angleRad2, innerRadius);
		}
		return array;
	}

	private static Vector2 RadialPoint(float angleRad, float radius)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius);
	}

	private static Vector2 GapOutwardEnd(float angleRad)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f - Mathf.Sin(angleRad), Mathf.Cos(angleRad));
	}

	private static Vector2 GapOutwardStart(float angleRad)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Sin(angleRad), 0f - Mathf.Cos(angleRad));
	}

	private static int SegmentsForSweep(float sweepDeg)
	{
		return Mathf.Max(8, Mathf.CeilToInt(sweepDeg / 8f));
	}

	public static float MeasureMaxRadius(Vector2[] pts)
	{
		if (pts == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < pts.Length; i++)
		{
			float sqrMagnitude = ((Vector2)(ref pts[i])).sqrMagnitude;
			if (sqrMagnitude > num)
			{
				num = sqrMagnitude;
			}
		}
		return Mathf.Sqrt(num);
	}

	public static void NormalizeRadii(float innerRadius, float outerRadius, out float inner, out float outer)
	{
		inner = Mathf.Abs(innerRadius);
		outer = Mathf.Abs(outerRadius);
		if (outer < inner)
		{
			float num = inner;
			inner = outer;
			outer = num;
		}
		if (outer <= 0f)
		{
			outer = Mathf.Max(inner + 0.01f, 0.01f);
		}
	}

	public static float NormalizeGapDeg(float gapAngleDeg)
	{
		return Mathf.Repeat(Mathf.Abs(gapAngleDeg), 360f);
	}
}
