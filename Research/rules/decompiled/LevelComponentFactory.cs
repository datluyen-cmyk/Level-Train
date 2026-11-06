using System.Collections.Generic;
using RotateRings;
using UniLog;
using UnityEngine;
using UnlockRing.Core.Authoring;
using UnlockRing.Core.Geometry;
using XingYun.Render;
using XingYun.RingGame;

public static class LevelComponentFactory
{
	public readonly struct CoordSpace
	{
		public readonly float Scale;

		public float InverseScale => 1f / Scale;

		public CoordSpace(float scale)
		{
			Scale = scale;
		}

		public float ToUnityScalar(double value)
		{
			return (float)value * Scale;
		}

		public Vector2 ToUnityPosition(double x, double y)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2((float)x * Scale, (0f - (float)y) * Scale);
		}

		public float ToUnityRotationDeg(double deg)
		{
			return 0f - (float)deg;
		}
	}

	public readonly struct CreateResult
	{
		public readonly GameObject GameObject;

		public readonly Aabb? Bounds;

		public static CreateResult Empty => default(CreateResult);

		public CreateResult(GameObject go, Aabb? bounds)
		{
			GameObject = go;
			Bounds = bounds;
		}
	}

	private const string TAG = "[UnlockRing][LevelComponentFactory] ";

	private static readonly ButterflyComponentBuilder butterflyComponentBuilder = new ButterflyComponentBuilder();

	public const float EndCapLength = 0.5f;

	private const string CirclePrefabPath = "Circle";

	private const string ArrowPrefabPath = "Arrow";

	private const string CatchPrefabPath = "Catch";

	private const string BombPrefabPath = "Bomb";

	private const string PhysicCirclePrefabPath = "PhysicCircle";

	private const string PhysicArrowPrefabPath = "PhysicArrow";

	private const string PhysicCatchPrefabPath = "PhysicCatch";

	private const string PhysicBombPrefabPath = "PhysicBomb";

	public static string GetCirclePrefabPath()
	{
		if (!GameConst.IsCreatePhysic)
		{
			return "Circle";
		}
		return "PhysicCircle";
	}

	public static string GetArrowPrefabPath()
	{
		if (!GameConst.IsCreatePhysic)
		{
			return "Arrow";
		}
		return "PhysicArrow";
	}

	public static string GetCatchPrefabPath()
	{
		if (!GameConst.IsCreatePhysic)
		{
			return "Catch";
		}
		return "PhysicCatch";
	}

	public static string GetBombPrefabPath()
	{
		if (!GameConst.IsCreatePhysic)
		{
			return "Bomb";
		}
		return "PhysicBomb";
	}

	public static CreateResult CreatePart(EditablePartDraft part, int partIndex, Transform parent, CoordSpace cs, Color vertexColor = default(Color), PartPaletteColor paletteColor = PartPaletteColor.None)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		RotatingRingDraft val = (RotatingRingDraft)(object)((part is RotatingRingDraft) ? part : null);
		if (val == null)
		{
			SlidingStickDraft val2 = (SlidingStickDraft)(object)((part is SlidingStickDraft) ? part : null);
			if (val2 == null)
			{
				FixedCatchGuideDraft val3 = (FixedCatchGuideDraft)(object)((part is FixedCatchGuideDraft) ? part : null);
				if (val3 == null)
				{
					ButterflyClipDraft val4 = (ButterflyClipDraft)(object)((part is ButterflyClipDraft) ? part : null);
					if (val4 == null)
					{
						BombDraft val5 = (BombDraft)(object)((part is BombDraft) ? part : null);
						if (val5 != null)
						{
							return CreateBomb(val5, parent, cs);
						}
						return CreateResult.Empty;
					}
					return CreateButterflyClip(val4, partIndex, parent, cs);
				}
				return CreateFixedCatchGuide(val3, partIndex, parent, cs);
			}
			return CreateSlidingStick(val2, partIndex, parent, cs, vertexColor, paletteColor);
		}
		return CreateRing(val, partIndex, parent, cs, vertexColor, paletteColor);
	}

	private static CreateResult CreateRing(RotatingRingDraft ringDraft, int partIndex, Transform parent, CoordSpace cs, Color vertexColor = default(Color), PartPaletteColor paletteColor = PartPaletteColor.None)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		float num = cs.ToUnityScalar(ringDraft.InnerRadius);
		float num2 = cs.ToUnityScalar(ringDraft.OuterRadius);
		float num3 = (num + num2) * 0.5f;
		float num4 = num2 - num;
		float num5 = (float)ringDraft.GapDegrees;
		Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditablePartDraft)ringDraft).X, ((EditablePartDraft)ringDraft).Y));
		localPosition.z = 0f;
		float num6 = cs.ToUnityRotationDeg(((EditablePartDraft)ringDraft).RotationDegrees);
		List<RingAttachment> attachments = BuildRingAttachmentsFromDraft(ringDraft, num4, cs);
		GameObject val = Resources.Load<GameObject>(GetCirclePrefabPath());
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelComponentFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { GetCirclePrefabPath() });
			return CreateResult.Empty;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val2).name = "Ring_" + ((EditablePartDraft)ringDraft).Id;
		val2.transform.localPosition = localPosition;
		val2.transform.localRotation = Quaternion.Euler(0f, 0f, num6);
		val2.transform.localScale = Vector3.one;
		RingSegmentController component = val2.GetComponent<RingSegmentController>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelComponentFactory] ", "RingSegmentController not found on Circle.prefab.");
			return new CreateResult(val2, null);
		}
		RingSegmentBuildSettings buildSettings = component.BuildSettings;
		buildSettings.width = Mathf.Max(0.01f, num4 * 2f);
		buildSettings.arcSubdivisions = RingSegmentArcSubdivision.ComputeFullCircleArcSubdivisions((float)ringDraft.InnerRadius);
		buildSettings.useHeadTail = num5 > 0.1f;
		buildSettings.styleIndex = 1;
		float capRatio = component.CapRatio;
		float gapAngleDeg = (buildSettings.useHeadTail ? RingGapGeometry.ComputeRenderGapDeg(num5, num3, component.SharedMaterial, capRatio) : 0f);
		component.ResetAttachments(attachments);
		Color vertexColor2 = ((vertexColor == default(Color)) ? PartPalette.For(partIndex) : vertexColor);
		component.Setup(num3, gapAngleDeg, 0f, buildSettings, component.SharedMaterial, vertexColor2);
		component.SetZLayerOffsets(0f, 0f, -0.2f);
		if (GameConst.IsCreatePhysic)
		{
			PartPaletteColor paletteColor2 = ((paletteColor != PartPaletteColor.None) ? paletteColor : PartPalette.FromPaletteIndex(partIndex));
			RingObject component2 = val2.GetComponent<RingObject>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.PartData = GameplayPartData.Create(((EditablePartDraft)ringDraft).Id, ((EditablePartDraft)ringDraft).AssemblyId, ((EditablePartDraft)ringDraft).CanCollect, 0, num5, (float)ringDraft.InnerRadius, (float)ringDraft.OuterRadius, randomBombCanSelect: true, hasBomb: false, paletteColor2);
				_ = ((EditablePartDraft)ringDraft).AssemblyId;
				_ = 0;
				GameplayPartData partData = component2.PartData;
				partData.CanCollect = ((EditablePartDraft)ringDraft).CanCollect;
				component2.PartData = partData;
			}
			Collider2D[] array = RingColliderBuilder.BuildPhysic(ringDraft, component, num, num2, num5, 0f, cs.Scale);
			if ((Object)(object)component2 != (Object)null && array != null && array.Length != 0)
			{
				component2.SetHardBlockColliders(array);
			}
		}
		CreateRingBombAttachments(ringDraft, val2.transform, cs);
		float num7 = (float)ringDraft.OuterRadius + (float)ringDraft.OuterRadius * 0.5f;
		Aabb value = default(Aabb);
		((Aabb)(ref value))._002Ector(new Vector2((double)((float)((EditablePartDraft)ringDraft).X - num7), (double)((float)((EditablePartDraft)ringDraft).Y - num7)), new Vector2((double)((float)((EditablePartDraft)ringDraft).X + num7), (double)((float)((EditablePartDraft)ringDraft).Y + num7)));
		return new CreateResult(val2, value);
	}

	private static CreateResult CreateSlidingStick(SlidingStickDraft stickDraft, int partIndex, Transform parent, CoordSpace cs, Color vertexColor = default(Color), PartPaletteColor paletteColor = PartPaletteColor.None)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unsupported input type for neg.
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		float num = cs.ToUnityScalar(stickDraft.Length);
		float num2 = cs.ToUnityScalar(stickDraft.Radius);
		float num3 = num2 * 2f;
		Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditablePartDraft)stickDraft).X, ((EditablePartDraft)stickDraft).Y));
		localPosition.z = 0f;
		float num4 = cs.ToUnityRotationDeg(((EditablePartDraft)stickDraft).RotationDegrees);
		Vector3 item = default(Vector3);
		((Vector3)(ref item))._002Ector((0f - num) * 0.5f - num3, 0f, 0f);
		Vector3 item2 = default(Vector3);
		((Vector3)(ref item2))._002Ector(num * 0.5f + num3, 0f, 0f);
		List<Vector3> list = new List<Vector3>();
		List<EditableSourceElbowAttachmentDraft> list2 = new List<EditableSourceElbowAttachmentDraft>();
		for (int i = 0; i < ((EditablePartDraft)stickDraft).Components.Count; i++)
		{
			EditableBodyComponentDraft obj = ((EditablePartDraft)stickDraft).Components[i];
			EditableSourceElbowAttachmentDraft val = (EditableSourceElbowAttachmentDraft)(object)((obj is EditableSourceElbowAttachmentDraft) ? obj : null);
			if (val != null)
			{
				list2.Add(val);
			}
		}
		if (list2.Count == 0)
		{
			list.Add(item);
			list.Add(item2);
		}
		else
		{
			list2.Sort(delegate(EditableSourceElbowAttachmentDraft a, EditableSourceElbowAttachmentDraft b)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				int num10 = (((int)a.End != 0) ? 1 : 0);
				int value2 = (((int)b.End != 0) ? 1 : 0);
				return num10.CompareTo(value2);
			});
			Vector3 val2 = default(Vector3);
			Vector3 val3 = default(Vector3);
			foreach (EditableSourceElbowAttachmentDraft item3 in list2)
			{
				float num5 = cs.ToUnityScalar(item3.BendRadius);
				float num6 = cs.ToUnityScalar(item3.ArmLength);
				float num7 = 0 - item3.Direction;
				if ((int)item3.End == 0)
				{
					float num8 = (0f - num) * 0.5f;
					((Vector3)(ref val2))._002Ector(num8 - num5, (num5 + num6) * num7 + num7 * num3, 0f);
					item.x = val2.x;
					list.Add(val2);
					list.Add(item);
					continue;
				}
				if (list.Count == 0)
				{
					list.Add(item);
				}
				float num9 = num * 0.5f;
				((Vector3)(ref val3))._002Ector(num9 + num5, (num5 + num6) * num7 + num7 * num3, 0f);
				item2.x = val3.x;
				list.Add(item2);
				list.Add(val3);
			}
			if (list.Count <= 2)
			{
				list.Add(item2);
			}
		}
		GameObject val4 = Resources.Load<GameObject>(GetArrowPrefabPath());
		if ((Object)(object)val4 == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelComponentFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { GetArrowPrefabPath() });
			return CreateResult.Empty;
		}
		GameObject val5 = Object.Instantiate<GameObject>(val4, parent, false);
		((Object)val5).name = "Stick_" + ((EditablePartDraft)stickDraft).Id;
		val5.transform.localPosition = localPosition;
		val5.transform.localRotation = Quaternion.Euler(0f, 0f, num4);
		val5.transform.localScale = Vector3.one;
		ArrowRibbonController component = val5.GetComponent<ArrowRibbonController>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelComponentFactory] ", "ArrowRibbonController not found on Arrow.prefab.");
			return new CreateResult(val5, null);
		}
		ArrowRibbonBuildSettings buildSettings = component.BuildSettings;
		buildSettings.width = num2 * 4f;
		Color vertexColor2 = ((vertexColor == default(Color)) ? PartPalette.For(partIndex, 10) : vertexColor);
		component.Setup(list, buildSettings, vertexColor2);
		if (GameConst.IsCreatePhysic)
		{
			PullObject component2 = val5.GetComponent<PullObject>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.PartData = GameplayPartData.Create(((EditablePartDraft)stickDraft).Id, ((EditablePartDraft)stickDraft).AssemblyId, ((EditablePartDraft)stickDraft).CanCollect, 0, 0f, 0f, 0f, randomBombCanSelect: true, hasBomb: false, paletteColor);
				_ = ((EditablePartDraft)stickDraft).AssemblyId;
				_ = 0;
			}
			FixedObject component3 = val5.GetComponent<FixedObject>();
			if ((Object)(object)component3 != (Object)null)
			{
				component3.PartData = GameplayPartData.Create(((EditablePartDraft)stickDraft).Id, ((EditablePartDraft)stickDraft).AssemblyId, canCollect: false, 0, 0f, 0f, 0f, randomBombCanSelect: true, hasBomb: false, paletteColor);
			}
			RingColliderBuilder.BuildPhysic(stickDraft, component, list, num2, cs.Scale);
		}
		CreateStickBombAttachments(stickDraft, val5.transform, cs);
		Aabb value = default(Aabb);
		((Aabb)(ref value))._002Ector(new Vector2((double)(float)((EditablePartDraft)stickDraft).X - (double)(float)stickDraft.Length * 0.5 - (double)(float)stickDraft.Radius, (double)((float)((EditablePartDraft)stickDraft).Y - (float)stickDraft.Radius)), new Vector2((double)(float)((EditablePartDraft)stickDraft).X + (double)(float)stickDraft.Length * 0.5 + (double)(float)stickDraft.Radius, (double)((float)((EditablePartDraft)stickDraft).Y + (float)stickDraft.Radius)));
		return new CreateResult(val5, value);
	}

	private static CreateResult CreateFixedCatchGuide(FixedCatchGuideDraft draft, int partIndex, Transform parent, CoordSpace cs)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Resources.Load<GameObject>(GetCatchPrefabPath());
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelComponentFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { GetCatchPrefabPath() });
			return CreateResult.Empty;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val2).name = "Catch_" + ((EditablePartDraft)draft).Id;
		Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditablePartDraft)draft).X, ((EditablePartDraft)draft).Y));
		localPosition.z = -1f;
		val2.transform.localPosition = localPosition;
		val2.transform.localRotation = Quaternion.Euler(0f, 0f, cs.ToUnityRotationDeg(((EditablePartDraft)draft).RotationDegrees));
		float num = cs.ToUnityScalar(draft.Radius) * 2f * 1.6f;
		val2.transform.localScale = new Vector3(num, num, 1f);
		if (GameConst.IsCreatePhysic)
		{
			FixedObject component = val2.GetComponent<FixedObject>();
			if (component != null)
			{
				component.PartData = GameplayPartData.Create(((EditablePartDraft)draft).Id, ((EditablePartDraft)draft).AssemblyId);
				_ = ((EditablePartDraft)draft).AssemblyId;
				_ = 0;
			}
			RingColliderBuilder.BuildPhysic(draft, val2, cs.ToUnityScalar(draft.GuideLength), cs.ToUnityScalar(draft.ResolvedGuideThickness), cs.ToUnityScalar(draft.Radius), num);
		}
		float num2 = (float)draft.Radius;
		Aabb value = default(Aabb);
		((Aabb)(ref value))._002Ector(new Vector2((double)((float)((EditablePartDraft)draft).X - num2), (double)((float)((EditablePartDraft)draft).Y - num2)), new Vector2((double)((float)((EditablePartDraft)draft).X + num2), (double)((float)((EditablePartDraft)draft).Y + num2)));
		return new CreateResult(val2, value);
	}

	private static CreateResult CreateButterflyClip(ButterflyClipDraft draft, int partIndex, Transform parent, CoordSpace cs)
	{
		if (!GameConst.IsCreatePhysic)
		{
			return CreateResult.Empty;
		}
		return butterflyComponentBuilder.Create(draft, parent, cs);
	}

	private static CreateResult CreateBomb(BombDraft bombDraft, Transform parent, CoordSpace cs)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Resources.Load<GameObject>(GetBombPrefabPath());
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelComponentFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { GetBombPrefabPath() });
			return CreateResult.Empty;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val2).name = "Bomb_" + ((EditablePartDraft)bombDraft).Id;
		Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditablePartDraft)bombDraft).X, ((EditablePartDraft)bombDraft).Y));
		localPosition.z = -1f;
		val2.transform.localPosition = localPosition;
		val2.transform.localRotation = Quaternion.Euler(0f, 0f, cs.ToUnityRotationDeg(((EditablePartDraft)bombDraft).RotationDegrees));
		QuadPlaneController component = val2.GetComponent<QuadPlaneController>();
		float num = cs.ToUnityScalar(bombDraft.TriggerRadius);
		float num2 = num * 2f;
		component.SetupWidthAndHeight(num2, num2);
		val2.transform.localScale = Vector3.one;
		if (GameConst.IsCreatePhysic)
		{
			CircleCollider2D component2 = val2.GetComponent<CircleCollider2D>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.radius = num;
			}
		}
		float num3 = (float)bombDraft.TriggerRadius;
		Aabb value = default(Aabb);
		((Aabb)(ref value))._002Ector(new Vector2((double)((float)((EditablePartDraft)bombDraft).X - num3), (double)((float)((EditablePartDraft)bombDraft).Y - num3)), new Vector2((double)((float)((EditablePartDraft)bombDraft).X + num3), (double)((float)((EditablePartDraft)bombDraft).Y + num3)));
		return new CreateResult(val2, value);
	}

	private static List<RingAttachment> BuildRingAttachmentsFromDraft(RotatingRingDraft ringDraft, float parentWidth, CoordSpace cs)
	{
		List<RingAttachment> list = new List<RingAttachment>();
		for (int i = 0; i < ((EditablePartDraft)ringDraft).Components.Count; i++)
		{
			EditableBodyComponentDraft val = ((EditablePartDraft)ringDraft).Components[i];
			EditableElbowAttachmentDraft val2 = (EditableElbowAttachmentDraft)(object)((val is EditableElbowAttachmentDraft) ? val : null);
			RingAttachment? ringAttachment;
			if (val2 == null)
			{
				EditableRadialPinDraft val3 = (EditableRadialPinDraft)(object)((val is EditableRadialPinDraft) ? val : null);
				ringAttachment = ((val3 != null) ? new RingAttachment?(BuildTBarFromRadialPin(val3, parentWidth, cs)) : ((!(val is EditableBombAttachmentDraft)) ? ((RingAttachment?)null) : ((RingAttachment?)null)));
			}
			else
			{
				ringAttachment = BuildHeadedBarFromElbow(val2, parentWidth, cs);
			}
			RingAttachment? ringAttachment2 = ringAttachment;
			if (ringAttachment2.HasValue)
			{
				list.Add(ringAttachment2.Value);
			}
		}
		return list;
	}

	private static void CreateRingBombAttachments(RotatingRingDraft ringDraft, Transform ringTransform, CoordSpace cs)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((EditablePartDraft)ringDraft).Components.Count; i++)
		{
			EditableBodyComponentDraft obj = ((EditablePartDraft)ringDraft).Components[i];
			EditableBombAttachmentDraft val = (EditableBombAttachmentDraft)(object)((obj is EditableBombAttachmentDraft) ? obj : null);
			if (val == null)
			{
				continue;
			}
			GameObject val2 = Resources.Load<GameObject>(GetBombPrefabPath());
			if ((Object)(object)val2 == (Object)null)
			{
				Log.LogError("[UnlockRing][LevelComponentFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { GetBombPrefabPath() });
				continue;
			}
			GameObject val3 = Object.Instantiate<GameObject>(val2, ringTransform, false);
			((Object)val3).name = "Bomb_" + ((EditableBodyComponentDraft)val).Id;
			Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditableBodyComponentDraft)val).LocalX, ((EditableBodyComponentDraft)val).LocalY));
			localPosition.z = -1f;
			val3.transform.localPosition = localPosition;
			val3.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			val3.transform.localScale = Vector3.one;
			float num = cs.ToUnityScalar(val.Radius);
			float num2 = num * 2f;
			QuadPlaneController component = val3.GetComponent<QuadPlaneController>();
			if ((Object)(object)component != (Object)null)
			{
				component.SetupWidthAndHeight(num2, num2);
			}
			if (GameConst.IsCreatePhysic)
			{
				CircleCollider2D component2 = val3.GetComponent<CircleCollider2D>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.radius = num;
					((Collider2D)component2).offset = Vector2.zero;
				}
			}
		}
	}

	private static void CreateStickBombAttachments(SlidingStickDraft stickDraft, Transform stickTransform, CoordSpace cs)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < ((EditablePartDraft)stickDraft).Components.Count; i++)
		{
			EditableBodyComponentDraft obj = ((EditablePartDraft)stickDraft).Components[i];
			EditableBombAttachmentDraft val = (EditableBombAttachmentDraft)(object)((obj is EditableBombAttachmentDraft) ? obj : null);
			if (val == null)
			{
				continue;
			}
			GameObject val2 = Resources.Load<GameObject>(GetBombPrefabPath());
			if ((Object)(object)val2 == (Object)null)
			{
				Log.LogError("[UnlockRing][LevelComponentFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { GetBombPrefabPath() });
				continue;
			}
			GameObject val3 = Object.Instantiate<GameObject>(val2, stickTransform, false);
			((Object)val3).name = "Bomb_" + ((EditableBodyComponentDraft)val).Id;
			Vector3 localPosition = Vector2.op_Implicit(cs.ToUnityPosition(((EditableBodyComponentDraft)val).LocalX, ((EditableBodyComponentDraft)val).LocalY));
			localPosition.z = -1f;
			val3.transform.localPosition = localPosition;
			val3.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			val3.transform.localScale = Vector3.one;
			float num = cs.ToUnityScalar(val.Radius);
			float num2 = num * 2f;
			QuadPlaneController component = val3.GetComponent<QuadPlaneController>();
			if ((Object)(object)component != (Object)null)
			{
				component.SetupWidthAndHeight(num2, num2);
			}
			if (GameConst.IsCreatePhysic)
			{
				CircleCollider2D component2 = val3.GetComponent<CircleCollider2D>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.radius = num;
					((Collider2D)component2).offset = Vector2.zero;
				}
			}
		}
	}

	private static RingAttachment BuildHeadedBarFromElbow(EditableElbowAttachmentDraft elbow, float parentWidth, CoordSpace cs)
	{
		float num = Mathf.Max(0.01f, cs.ToUnityScalar(elbow.Radius) * 2f);
		return new RingAttachment
		{
			type = RingAttachment.AttachType.HeadedBar,
			styleIndex = 1,
			angleDeg = NormalizeAngle360(cs.ToUnityRotationDeg(elbow.DirectionDegrees)),
			stemLength = cs.ToUnityScalar(elbow.Length),
			stemWidth = num * 2f,
			stemInsertOffset = parentWidth,
			crossWidth = num * 1.5f,
			crossLength = 0f
		};
	}

	private static RingAttachment BuildTBarFromRadialPin(EditableRadialPinDraft pin, float parentWidth, CoordSpace cs)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		float crossLength = cs.ToUnityScalar(pin.HeadWidth);
		float num = cs.ToUnityScalar(pin.HeadLength);
		float num2 = (((int)pin.Kind == 0) ? 0f : 0.25f);
		return new RingAttachment
		{
			type = (((int)pin.Kind == 0) ? RingAttachment.AttachType.TBar : RingAttachment.AttachType.HeadedBar),
			styleIndex = 1,
			angleDeg = NormalizeAngle360(cs.ToUnityRotationDeg(pin.MountAngleDegrees)),
			isInnerMount = ((int)pin.MountSide == 1),
			crossWidth = num * 2f,
			crossLength = crossLength,
			stemLength = cs.ToUnityScalar(pin.StemLength) - parentWidth + num / 2f + num2,
			stemWidth = Mathf.Max(0.5f, cs.ToUnityScalar(pin.StemRadius) * 2f * 2f),
			stemInsertOffset = parentWidth
		};
	}

	private static float NormalizeAngle360(float angleDeg)
	{
		return Mathf.Repeat(angleDeg, 360f);
	}
}
