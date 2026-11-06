using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniLog;
using UnityEngine;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using XingYun.Render;
using XingYun.RingGame;

public static class RibbonPartBuilder
{
	private const string TAG = "[UnlockRing][RibbonPartBuilder] ";

	private const string CirclePrefabPath = "Circle";

	private const string ArrowPrefabPath = "Arrow";

	public static List<GameObject> BuildPartFeatures(PartDefinition part, PartState state, List<CachedRenderFeature> features, Transform parent, Color baseColor, float coordScale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		List<GameObject> list = new List<GameObject>();
		Pose2D pose = state.Pose;
		Vector2D position = ((Pose2D)(ref pose)).Position;
		float num = (float)((Vector2D)(ref position)).X;
		pose = state.Pose;
		position = ((Pose2D)(ref pose)).Position;
		Vector2 posePos = default(Vector2);
		((Vector2)(ref posePos))._002Ector(num, (float)((Vector2D)(ref position)).Y);
		pose = state.Pose;
		float poseRotRad = (float)((Pose2D)(ref pose)).RotationRadians;
		for (int i = 0; i < features.Count; i++)
		{
			CachedRenderFeature cachedRenderFeature = features[i];
			if (part.Behaviors.Any((BehaviorDefinition b) => b is AxisSlidableBehaviorDefinition))
			{
				FeatureId id = cachedRenderFeature.Feature.Id;
				if (((FeatureId)(ref id)).Value.Equals("guide", StringComparison.Ordinal))
				{
					continue;
				}
			}
			if (cachedRenderFeature.Shape is AnnularArc annular)
			{
				List<(Capsule, Obb?)> list2 = new List<(Capsule, Obb?)>();
				int num2 = i + 1;
				bool flag = false;
				while (num2 < features.Count && features[num2].Shape is Capsule item)
				{
					Obb? item2 = ((num2 + 1 < features.Count && features[num2 + 1].Shape is Obb value) ? new Obb?(value) : ((Obb?)null));
					bool hasValue = item2.HasValue;
					list2.Add((item, item2));
					num2 += ((!hasValue) ? 1 : 2);
					flag = true;
				}
				if (flag)
				{
					Color color = BoardRenderUtils.FeatureColor(cachedRenderFeature, baseColor);
					GameObject val = BuildAnnularArcWithAttachments(annular, list2, color, posePos, poseRotRad, parent, coordScale);
					if ((Object)(object)val != (Object)null)
					{
						list.Add(val);
					}
					i = num2 - 1;
					continue;
				}
			}
			Color color2 = BoardRenderUtils.FeatureColor(cachedRenderFeature, baseColor);
			GameObject val2 = BuildFeature(cachedRenderFeature.Shape, color2, posePos, poseRotRad, parent, coordScale);
			if ((Object)(object)val2 != (Object)null)
			{
				list.Add(val2);
			}
		}
		return list;
	}

	private static GameObject BuildFeature(IGeometryShape shape, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (!(shape is Circle circle))
		{
			if (!(shape is AnnularArc annular))
			{
				if (!(shape is Arc arc))
				{
					if (!(shape is Segment segment))
					{
						if (!(shape is Capsule capsule))
						{
							if (!(shape is Obb obb))
							{
								ConvexPolygon val = (ConvexPolygon)(object)((shape is ConvexPolygon) ? shape : null);
								if (val == null)
								{
									CompoundShape val2 = (CompoundShape)(object)((shape is CompoundShape) ? shape : null);
									if (val2 != null)
									{
										return BuildCompound(val2, color, posePos, poseRotRad, parent, scale);
									}
									return null;
								}
								return BuildConvexPolygon(val, color, posePos, poseRotRad, parent, scale);
							}
							return BuildObb(obb, color, posePos, poseRotRad, parent, scale);
						}
						return BuildCapsule(capsule, color, posePos, poseRotRad, parent, scale);
					}
					return BuildSegment(segment, color, posePos, poseRotRad, parent, scale);
				}
				return BuildArc(arc, color, posePos, poseRotRad, parent, scale);
			}
			return BuildAnnularArc(annular, color, posePos, poseRotRad, parent, scale);
		}
		return BuildCircle(circle, color, posePos, poseRotRad, parent, scale);
	}

	private static GameObject BuildCircle(Circle circle, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)((Circle)(ref circle)).Radius * scale;
		float width = num * 0.5f;
		return CreateRingSegment(CoreToLocal(V3(((Circle)(ref circle)).Center), posePos, poseRotRad, scale), num, width, 0f, 0f, color, parent);
	}

	private static GameObject BuildAnnularArc(AnnularArc annular, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)((AnnularArc)(ref annular)).InnerRadius * scale;
		float num2 = (float)((AnnularArc)(ref annular)).OuterRadius * scale;
		float radius = (num + num2) * 0.5f;
		float num3 = num2 - num;
		float num4 = Mathf.Abs((float)((AnnularArc)(ref annular)).ClockwiseSweepRadians * 57.29578f);
		float num5 = Mathf.Max(0f, 360f - num4);
		float num6 = (float)((AnnularArc)(ref annular)).StartClockwiseRadians * 57.29578f - num5 * 0.5f;
		float gapDirectionDeg = poseRotRad * 57.29578f - num6;
		return CreateRingSegment(CoreToLocal(V3(((AnnularArc)(ref annular)).Center), posePos, poseRotRad, scale), radius, num3 + 0.3f, num5, gapDirectionDeg, color, parent);
	}

	private static GameObject BuildArc(Arc arc, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		float radius = (float)((Arc)(ref arc)).Radius * scale;
		float num = 0.08f * scale;
		float num2 = Mathf.Abs((float)((Arc)(ref arc)).ClockwiseSweepRadians * 57.29578f);
		float gapAngleDeg = Mathf.Max(0f, 360f - num2);
		float num3 = (float)((Arc)(ref arc)).StartClockwiseRadians * 57.29578f + num2 * 0.5f;
		float gapDirectionDeg = poseRotRad * 57.29578f - (num3 + 180f);
		return CreateRingSegment(CoreToLocal(V3(((Arc)(ref arc)).Center), posePos, poseRotRad, scale), radius, Mathf.Max(0.01f, num), gapAngleDeg, gapDirectionDeg, color, parent);
	}

	private static RingAttachment BuildAttachment(Capsule capsule, Obb? obb, RingAttachment.AttachType attachType, Vector2 posePos, float poseRotRad, float scale, Vector3 localCenter, float ringOuterEdge)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		Segment spine = ((Capsule)(ref capsule)).Spine;
		Vector3 val = CoreToLocal(V3(((Segment)(ref spine)).Start), posePos, poseRotRad, scale);
		spine = ((Capsule)(ref capsule)).Spine;
		Vector3 val2 = CoreToLocal(V3(((Segment)(ref spine)).End), posePos, poseRotRad, scale);
		RingAttachment result = new RingAttachment
		{
			type = attachType,
			styleIndex = 0
		};
		result.angleDeg = Mathf.Atan2(val.y - localCenter.y, val.x - localCenter.x) * 57.29578f;
		result.stemLength = Vector3.Distance(val, val2);
		result.stemWidth = Mathf.Max(0.01f, (float)((Capsule)(ref capsule)).Radius * 2f * scale) * 2f;
		float num = Vector3.Distance(val, localCenter);
		result.stemInsertOffset = Mathf.Max(0f, ringOuterEdge - num);
		float num2 = Vector3.Distance(val2, localCenter);
		result.isInnerMount = num2 < num;
		if (attachType == RingAttachment.AttachType.TBar && obb.HasValue)
		{
			Obb value = obb.Value;
			Vector2 halfExtents = ((Obb)(ref value)).HalfExtents;
			float num3 = (float)((Vector2)(ref halfExtents)).X * 2f * scale;
			value = obb.Value;
			halfExtents = ((Obb)(ref value)).HalfExtents;
			float num4 = (float)((Vector2)(ref halfExtents)).Y * 2f * scale;
			result.crossWidth = Mathf.Min(num3, num4) * 2f;
			result.crossLength = Mathf.Max(num3, num4);
		}
		else
		{
			result.crossWidth = result.stemWidth * 1.5f;
			result.crossLength = 0f;
		}
		return result;
	}

	private static GameObject BuildAnnularArcWithAttachments(AnnularArc annular, List<(Capsule capsule, Obb? obb)> groups, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)((AnnularArc)(ref annular)).InnerRadius * scale;
		float num2 = (float)((AnnularArc)(ref annular)).OuterRadius * scale;
		float num3 = (num + num2) * 0.5f;
		float num4 = num2 - num;
		float num5 = Mathf.Abs((float)((AnnularArc)(ref annular)).ClockwiseSweepRadians * 57.29578f);
		float num6 = Mathf.Max(0f, 360f - num5);
		float num7 = (float)((AnnularArc)(ref annular)).StartClockwiseRadians * 57.29578f + num5 * 0.5f;
		float gapDirectionDeg = poseRotRad * 57.29578f - (num7 + 180f);
		Vector3 val = CoreToLocal(V3(((AnnularArc)(ref annular)).Center), posePos, poseRotRad, scale);
		float ringOuterEdge = num3 + num4 * 0.5f;
		List<RingAttachment> list = new List<RingAttachment>(groups.Count);
		for (int i = 0; i < groups.Count; i++)
		{
			(Capsule capsule, Obb? obb) tuple = groups[i];
			Capsule item = tuple.capsule;
			Obb? item2 = tuple.obb;
			RingAttachment.AttachType attachType = (item2.HasValue ? RingAttachment.AttachType.TBar : RingAttachment.AttachType.HeadedBar);
			list.Add(BuildAttachment(item, item2, attachType, posePos, poseRotRad, scale, val, ringOuterEdge));
		}
		GameObject val2 = Resources.Load<GameObject>("Circle");
		if ((Object)(object)val2 == (Object)null)
		{
			Log.LogError("[UnlockRing][RibbonPartBuilder] ", "Cannot load {0}.prefab from Resources.", new object[1] { "Circle" });
			return null;
		}
		GameObject val3 = Object.Instantiate<GameObject>(val2, parent, false);
		((Object)val3).name = "RingSegment_Attachments";
		val3.transform.localPosition = val;
		val3.transform.localRotation = Quaternion.identity;
		val3.transform.localScale = Vector3.one;
		RingSegmentController component = val3.GetComponent<RingSegmentController>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[UnlockRing][RibbonPartBuilder] ", "RingSegmentController not found on Circle.prefab.");
			return val3;
		}
		RingSegmentBuildSettings settings = RingSegmentBuildSettings.Default;
		settings.width = Mathf.Max(0.01f, num4 * 2f);
		settings.arcSubdivisions = RingSegmentArcSubdivision.ComputeFullCircleArcSubdivisions(RingSegmentArcSubdivision.EstimateInnerRadiusCore(num3, settings.width));
		settings.useHeadTail = num6 > 0.1f;
		FieldInfo field = typeof(RingSegmentController).GetField("_attachments", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			field.SetValue(component, list);
		}
		component.Setup(num3, num6, gapDirectionDeg, settings, component.SharedMaterial);
		component.SetColor(color);
		return val3;
	}

	private static GameObject CreateRingSegment(Vector3 localCenter, float radius, float width, float gapAngleDeg, float gapDirectionDeg, Color color, Transform parent)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Resources.Load<GameObject>("Circle");
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][RibbonPartBuilder] ", "Cannot load {0}.prefab from Resources.", new object[1] { "Circle" });
			return null;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val2).name = "RingSegment";
		val2.transform.localPosition = localCenter;
		val2.transform.localRotation = Quaternion.identity;
		val2.transform.localScale = Vector3.one;
		RingSegmentController component = val2.GetComponent<RingSegmentController>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[UnlockRing][RibbonPartBuilder] ", "RingSegmentController not found on Circle.prefab.");
			return val2;
		}
		RingSegmentBuildSettings settings = RingSegmentBuildSettings.Default;
		settings.width = Mathf.Max(0.01f, width);
		settings.arcSubdivisions = RingSegmentArcSubdivision.ComputeFullCircleArcSubdivisions(RingSegmentArcSubdivision.EstimateInnerRadiusCore(radius, settings.width));
		settings.useHeadTail = gapAngleDeg > 0.1f;
		component.Setup(radius, gapAngleDeg, gapDirectionDeg, settings, component.SharedMaterial);
		component.SetColor(color);
		return val2;
	}

	private static GameObject BuildSegment(Segment segment, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		return CreateArrowRibbon(new List<Vector3>
		{
			CoreToLocal(V3(((Segment)(ref segment)).Start), posePos, poseRotRad, scale),
			CoreToLocal(V3(((Segment)(ref segment)).End), posePos, poseRotRad, scale)
		}, 0.08f * scale, color, parent);
	}

	private static GameObject BuildCapsule(Capsule capsule, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>();
		Segment spine = ((Capsule)(ref capsule)).Spine;
		list.Add(CoreToLocal(V3(((Segment)(ref spine)).Start), posePos, poseRotRad, scale));
		spine = ((Capsule)(ref capsule)).Spine;
		list.Add(CoreToLocal(V3(((Segment)(ref spine)).End), posePos, poseRotRad, scale));
		float num = Mathf.Max(0.08f * scale, (float)((Capsule)(ref capsule)).Radius * scale);
		num += 0.33f;
		return CreateArrowRibbon(list, num, color, parent);
	}

	private static GameObject BuildObb(Obb obb, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return BuildFromPolygon(((Obb)(ref obb)).ToPolygon(), color, posePos, poseRotRad, parent, scale);
	}

	private static GameObject BuildConvexPolygon(ConvexPolygon polygon, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return BuildFromPolygon(polygon, color, posePos, poseRotRad, parent, scale);
	}

	private static GameObject BuildFromPolygon(ConvexPolygon polygon, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<Vector2> vertices = polygon.Vertices;
		if (vertices.Count < 2)
		{
			return null;
		}
		List<Vector3> list = new List<Vector3>(vertices.Count + 1);
		for (int i = 0; i < vertices.Count; i++)
		{
			list.Add(CoreToLocal(V3(vertices[i]), posePos, poseRotRad, scale));
		}
		list.Add(CoreToLocal(V3(vertices[0]), posePos, poseRotRad, scale));
		return CreateArrowRibbon(list, 0.15f * scale, color, parent);
	}

	private static GameObject BuildCompound(CompoundShape compound, Color color, Vector2 posePos, float poseRotRad, Transform parent, float scale)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < compound.ChildCount; i++)
		{
			GameObject val = BuildFeature(compound.GetChild(i), color, posePos, poseRotRad, parent, scale);
			if ((Object)(object)val != (Object)null)
			{
				return val;
			}
		}
		return null;
	}

	private static GameObject CreateArrowRibbon(List<Vector3> path, float width, Color color, Transform parent)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (path == null || path.Count < 2)
		{
			return null;
		}
		GameObject val = Resources.Load<GameObject>("Arrow");
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][RibbonPartBuilder] ", "Cannot load {0}.prefab from Resources.", new object[1] { "Arrow" });
			return null;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val2).name = "ArrowRibbon";
		val2.transform.localPosition = Vector3.zero;
		val2.transform.localRotation = Quaternion.identity;
		val2.transform.localScale = Vector3.one;
		ArrowRibbonController component = val2.GetComponent<ArrowRibbonController>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[UnlockRing][RibbonPartBuilder] ", "ArrowRibbonController not found on Arrow.prefab.");
			return val2;
		}
		ArrowRibbonBuildSettings arrowRibbonBuildSettings = ArrowRibbonBuildSettings.Default;
		arrowRibbonBuildSettings.width = Mathf.Max(0.01f, width);
		arrowRibbonBuildSettings.cornerRadius = width * 0.5f;
		arrowRibbonBuildSettings.cornerSegments = 8;
		arrowRibbonBuildSettings.exitExtraLength = 0f;
		arrowRibbonBuildSettings.styleIndex = 0;
		component.SetPath(path);
		MaterialPropertyBlock val3 = new MaterialPropertyBlock();
		MeshRenderer component2 = val2.GetComponent<MeshRenderer>();
		((Renderer)component2).GetPropertyBlock(val3);
		val3.SetColor("_Color", color);
		((Renderer)component2).SetPropertyBlock(val3);
		return val2;
	}

	private static Vector3 CoreToLocal(Vector3 coreWorld, Vector2 posePos, float poseRotRad, float scale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float num = coreWorld.x - posePos.x;
		float num2 = coreWorld.y - posePos.y;
		float num3 = Mathf.Cos(0f - poseRotRad);
		float num4 = Mathf.Sin(0f - poseRotRad);
		float num5 = num * num3 - num2 * num4;
		float num6 = num * num4 + num2 * num3;
		return new Vector3(num5 * scale, (0f - num6) * scale, 0f);
	}

	private static Vector3 V3(Vector2 v)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)((Vector2)(ref v)).X, (float)((Vector2)(ref v)).Y, 0f);
	}
}
