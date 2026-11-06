using System;
using UnityEngine;

namespace RotateRings.GamePlay.Objects;

public sealed class FixedButterflyMeshController : MonoBehaviour
{
	public const string SlotTopName = "SlotTop";

	public const string SlotBottomName = "SlotBottom";

	private const float BridgeMeshEndOverlap = 0.014f;

	public static bool suppressAutoRebuild;

	[SerializeField]
	private Material catchMaterial;

	[SerializeField]
	private Material bodyMaterial;

	[SerializeField]
	private float slotSpan = 0.7f;

	[SerializeField]
	private float bridgeWidth = 0.35f;

	[SerializeField]
	private float endCapLength = 0.8f;

	[SerializeField]
	private float endCapWidth = 0.1f;

	[SerializeField]
	private float bridgeZ = 0.001f;

	[SerializeField]
	private float slotZ = -0.002f;

	[SerializeField]
	private FixedButterflyMeshPart bridgePart;

	[SerializeField]
	private FixedButterflyMeshPart slotTopPart;

	[SerializeField]
	private FixedButterflyMeshPart slotBottomPart;

	public float MeshSlotSpan => slotSpan;

	public float MeshCapSpan => endCapLength;

	public float MeshSlotCenterOffset => slotSpan * 0.5f - endCapWidth * 0.5f;

	private void Awake()
	{
		EnsureMeshParts();
		ApplyButterflyMaterials();
		EnsureRenderableMeshes();
	}

	private void EnsureRenderableMeshes()
	{
		if (!IsBakedMeshReady(bridgePart) || !IsBakedMeshReady(slotTopPart) || !IsBakedMeshReady(slotBottomPart))
		{
			RebuildAll();
		}
	}

	private static bool IsBakedMeshReady(FixedButterflyMeshPart part)
	{
		if (!((Object)(object)part == (Object)null))
		{
			return !part.IsRenderableMeshMissing();
		}
		return true;
	}

	public void ConfigureGeometry(float slotSpanUnits, float bridgeWidthUnits, float endCapLengthUnits, float endCapWidthUnits)
	{
		slotSpan = Mathf.Max(0.01f, slotSpanUnits);
		bridgeWidth = Mathf.Max(0.01f, bridgeWidthUnits);
		endCapLength = Mathf.Max(0.01f, endCapLengthUnits);
		endCapWidth = Mathf.Max(0.01f, endCapWidthUnits);
		RebuildAll();
	}

	public void Rebuild()
	{
		RebuildAll();
	}

	public void RebuildAll()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)this == (Object)null))
		{
			EnsureMeshParts();
			ApplyButterflyMaterials();
			float num = Mathf.Max(0.01f, slotSpan - endCapWidth * 2f);
			float num2 = 0f - slotSpan * 0.5f + endCapWidth + num * 0.5f;
			if ((Object)(object)bridgePart != (Object)null)
			{
				bridgePart.SetBridgeFacingLocalX(0);
				float width = num + 0.028f;
				bridgePart.ConfigureQuad(width, bridgeWidth, new Vector3(num2, 0f, 0f), bridgeZ);
				bridgePart.Rebuild();
			}
			if ((Object)(object)slotTopPart != (Object)null)
			{
				slotTopPart.SetBridgeFacingLocalX(-1);
				slotTopPart.ConfigureQuad(endCapWidth, endCapLength, Vector3.zero, slotZ);
				slotTopPart.Rebuild();
			}
			if ((Object)(object)slotBottomPart != (Object)null)
			{
				slotBottomPart.SetBridgeFacingLocalX(1);
				slotBottomPart.ConfigureQuad(endCapWidth, endCapLength, Vector3.zero, slotZ);
				slotBottomPart.Rebuild();
			}
		}
	}

	public void ApplyVertexColor(Color color)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		bridgePart?.ApplyVertexColor(color);
		slotTopPart?.ApplyVertexColor(color);
		slotBottomPart?.ApplyVertexColor(color);
	}

	public void ApplyButterflyMaterials()
	{
		if ((Object)(object)bridgePart != (Object)null && (Object)(object)bodyMaterial != (Object)null)
		{
			bridgePart.SetSharedMaterial(bodyMaterial);
		}
		if (!((Object)(object)catchMaterial == (Object)null))
		{
			slotTopPart?.SetSharedMaterial(catchMaterial);
			slotBottomPart?.SetSharedMaterial(catchMaterial);
		}
	}

	public FixedButterflyMeshPart[] GetMeshParts()
	{
		EnsureMeshParts();
		if ((Object)(object)bridgePart == (Object)null && (Object)(object)slotTopPart == (Object)null && (Object)(object)slotBottomPart == (Object)null)
		{
			return Array.Empty<FixedButterflyMeshPart>();
		}
		FixedButterflyMeshPart[] array = new FixedButterflyMeshPart[(((Object)(object)bridgePart != (Object)null) ? 1 : 0) + (((Object)(object)slotTopPart != (Object)null) ? 1 : 0) + (((Object)(object)slotBottomPart != (Object)null) ? 1 : 0)];
		int num = 0;
		if ((Object)(object)bridgePart != (Object)null)
		{
			array[num++] = bridgePart;
		}
		if ((Object)(object)slotTopPart != (Object)null)
		{
			array[num++] = slotTopPart;
		}
		if ((Object)(object)slotBottomPart != (Object)null)
		{
			array[num++] = slotBottomPart;
		}
		return array;
	}

	private void EnsureMeshParts()
	{
		if ((Object)(object)bridgePart == (Object)null)
		{
			bridgePart = ((Component)this).GetComponent<FixedButterflyMeshPart>();
			if ((Object)(object)bridgePart == (Object)null)
			{
				bridgePart = ((Component)this).gameObject.AddComponent<FixedButterflyMeshPart>();
			}
			bridgePart.SetRole(FixedButterflyMeshPart.PartRole.Bridge);
		}
		FixedButterflyObject component = ((Component)this).GetComponent<FixedButterflyObject>();
		string guidePartId;
		string slotName = (((Object)(object)component != (Object)null && component.TryGetSlotGuidePartId(1, out guidePartId)) ? guidePartId : "SlotTop");
		string guidePartId2;
		string slotName2 = (((Object)(object)component != (Object)null && component.TryGetSlotGuidePartId(0, out guidePartId2)) ? guidePartId2 : "SlotBottom");
		Transform val = FindSlotAnchor(slotName, "SlotTop");
		if ((Object)(object)val != (Object)null && (Object)(object)slotTopPart == (Object)null)
		{
			slotTopPart = GetOrAddPartOn(val, FixedButterflyMeshPart.PartRole.SlotCatch);
		}
		Transform val2 = FindSlotAnchor(slotName2, "SlotBottom");
		if ((Object)(object)val2 != (Object)null && (Object)(object)slotBottomPart == (Object)null)
		{
			slotBottomPart = GetOrAddPartOn(val2, FixedButterflyMeshPart.PartRole.SlotCatch);
		}
	}

	private Transform FindSlotAnchor(string slotName, string legacySlotName)
	{
		Transform val = ((Component)this).transform.Find(slotName);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		if (string.IsNullOrEmpty(legacySlotName) || legacySlotName == slotName)
		{
			return null;
		}
		val = ((Component)this).transform.Find(legacySlotName);
		if ((Object)(object)val != (Object)null)
		{
			((Object)val).name = slotName;
		}
		return val;
	}

	private FixedButterflyMeshPart GetOrAddPartOn(Transform t, FixedButterflyMeshPart.PartRole role)
	{
		FixedButterflyMeshPart fixedButterflyMeshPart = ((Component)t).GetComponent<FixedButterflyMeshPart>();
		if ((Object)(object)fixedButterflyMeshPart == (Object)null)
		{
			if ((Object)(object)((Component)t).GetComponent<MeshFilter>() == (Object)null)
			{
				((Component)t).gameObject.AddComponent<MeshFilter>();
			}
			if ((Object)(object)((Component)t).GetComponent<MeshRenderer>() == (Object)null)
			{
				((Component)t).gameObject.AddComponent<MeshRenderer>();
			}
			fixedButterflyMeshPart = ((Component)t).gameObject.AddComponent<FixedButterflyMeshPart>();
		}
		fixedButterflyMeshPart.SetRole(role);
		return fixedButterflyMeshPart;
	}
}
