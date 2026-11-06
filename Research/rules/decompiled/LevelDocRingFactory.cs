using System.Collections.Generic;
using System.Reflection;
using UniLog;
using UnityEngine;
using UnlockRing.LevelDocoder;
using XingYun.RingGame;

public static class LevelDocRingFactory
{
	private const string TAG = "[UnlockRing][LevelDocRingFactory] ";

	private const string CirclePrefabPath = "Circle";

	public static GameObject Create(AssemblyData assembly, Dictionary<string, GraphData> graphs, Transform parent, float coordScale)
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		if (assembly == null || assembly.main_component == null)
		{
			Log.LogError("[UnlockRing][LevelDocRingFactory] ", "assembly 或 main_component 为空");
			return null;
		}
		if (!graphs.TryGetValue(assembly.main_component.graph, out var value))
		{
			Log.LogError("[UnlockRing][LevelDocRingFactory] ", "graphs 中找不到 main_component 引用的图形: {0}", new object[1] { assembly.main_component.graph });
			return null;
		}
		if (!(value is RingGraphData ringGraphData))
		{
			return null;
		}
		float num = ringGraphData.inside_radius * coordScale;
		float num2 = ringGraphData.external_radius * coordScale;
		float num3 = (num + num2) * 0.5f;
		float num4 = num2 - num;
		float gap_degree = ringGraphData.gap_degree;
		float rotation = assembly.rotation;
		float ringOuterEdge = num3 + num4 * 0.5f;
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(assembly.position_x * coordScale, assembly.position_y * coordScale, 0f);
		List<RingAttachment> value2 = BuildAttachments(assembly, graphs, coordScale, ringOuterEdge);
		GameObject val = Resources.Load<GameObject>("Circle");
		if ((Object)(object)val == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelDocRingFactory] ", "Cannot load {0}.prefab from Resources.", new object[1] { "Circle" });
			return null;
		}
		GameObject val2 = Object.Instantiate<GameObject>(val, parent, false);
		((Object)val2).name = "RingSegment_Attachments";
		val2.transform.localPosition = localPosition;
		val2.transform.localRotation = Quaternion.identity;
		val2.transform.localScale = Vector3.one;
		RingSegmentController component = val2.GetComponent<RingSegmentController>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("[UnlockRing][LevelDocRingFactory] ", "RingSegmentController not found on Circle.prefab.");
			return val2;
		}
		RingSegmentBuildSettings settings = RingSegmentBuildSettings.Default;
		settings.width = Mathf.Max(0.01f, num4 * 2f);
		settings.arcSubdivisions = RingSegmentArcSubdivision.ComputeFullCircleArcSubdivisions(ringGraphData.inside_radius);
		settings.useHeadTail = gap_degree > 0.1f;
		FieldInfo field = typeof(RingSegmentController).GetField("_attachments", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field != null)
		{
			field.SetValue(component, value2);
		}
		component.Setup(num3, gap_degree, rotation, settings, component.SharedMaterial);
		return val2;
	}

	private static List<RingAttachment> BuildAttachments(AssemblyData assembly, Dictionary<string, GraphData> graphs, float coordScale, float ringOuterEdge)
	{
		List<RingAttachment> list = new List<RingAttachment>();
		if (assembly.sub_component == null || assembly.sub_component.Count == 0)
		{
			return list;
		}
		for (int i = 0; i < assembly.sub_component.Count; i++)
		{
			SubComponentData subComponentData = assembly.sub_component[i];
			if (subComponentData != null && !string.IsNullOrEmpty(subComponentData.graph) && graphs.TryGetValue(subComponentData.graph, out var value))
			{
				RingAttachment? ringAttachment = ((value is StopPinGraphData stopPin) ? new RingAttachment?(BuildHeadedBar(stopPin, subComponentData, coordScale)) : ((!(value is PinGraphData pin)) ? ((RingAttachment?)null) : new RingAttachment?(BuildTBar(pin, subComponentData, coordScale))));
				RingAttachment? ringAttachment2 = ringAttachment;
				if (ringAttachment2.HasValue)
				{
					list.Add(ringAttachment2.Value);
				}
			}
		}
		return list;
	}

	private static RingAttachment BuildHeadedBar(StopPinGraphData stopPin, SubComponentData sub, float coordScale)
	{
		RingAttachment result = new RingAttachment
		{
			type = RingAttachment.AttachType.HeadedBar,
			styleIndex = 0
		};
		result.angleDeg = sub.orientation;
		result.stemLength = stopPin.stem_length * coordScale;
		result.stemWidth = Mathf.Max(0.01f, stopPin.stem_thickness * coordScale * 2f);
		result.stemInsertOffset = 0f;
		result.crossWidth = result.stemWidth * 1.5f;
		result.crossLength = 0f;
		result.isInnerMount = sub.mounting_position == 1;
		return result;
	}

	private static RingAttachment BuildTBar(PinGraphData pin, SubComponentData sub, float coordScale)
	{
		RingAttachment result = new RingAttachment
		{
			type = RingAttachment.AttachType.TBar,
			styleIndex = 0
		};
		result.angleDeg = sub.orientation;
		result.stemLength = pin.stem_length * coordScale;
		result.stemWidth = Mathf.Max(0.01f, pin.stem_thickness * coordScale * 2f);
		result.stemInsertOffset = 0f;
		result.crossWidth = Mathf.Min(pin.head_width * coordScale, pin.head_length * coordScale) * 2f;
		result.crossLength = Mathf.Max(pin.head_width * coordScale, pin.head_length * coordScale);
		result.isInnerMount = sub.mounting_position == 1;
		return result;
	}
}
