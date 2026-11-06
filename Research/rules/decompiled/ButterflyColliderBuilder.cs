using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using XingYun.Render;

public class ButterflyColliderBuilder
{
	private const float GuideWallThickness = 0.07f;

	private const float GuideWallLengthShrink = 0.05f;

	private const float ButterflySlotBodyInset = 0.04f;

	public void BuildPhysic(FixedButterflyObject brow, FixedButterflyMeshController mesh, float slotGuideLength, float slotGuideThickness, float bridgeLength, float bridgeWidth)
	{
		if (!((Object)(object)brow == (Object)null))
		{
			Transform transform = ((Component)brow).transform;
			RemoveLegacyButterflySlotNodes(transform);
			BoxCollider2D val = ConfigureButterflyRootBoxCollider(brow, bridgeLength, bridgeWidth);
			List<Collider2D> list = new List<Collider2D> { (Collider2D)(object)val };
			float num = (((Object)(object)mesh != (Object)null) ? mesh.MeshSlotCenterOffset : 0.35f);
			string guidePartId;
			string slotName = (brow.TryGetSlotGuidePartId(0, out guidePartId) ? guidePartId : "SlotBottom");
			string guidePartId2;
			string slotName2 = (brow.TryGetSlotGuidePartId(1, out guidePartId2) ? guidePartId2 : "SlotTop");
			Transform slotAnchor = EnsureButterflySlotAnchor(transform, slotName, "SlotBottom", 0f - num);
			Transform slotAnchor2 = EnsureButterflySlotAnchor(transform, slotName2, "SlotTop", num);
			brow.SetColliders((Collider2D)(object)val, (Collider2D[])(object)new Collider2D[2]
			{
				(Collider2D)SetupButterflySlotCatch(slotAnchor, brow, slotGuideLength, slotGuideThickness, list),
				(Collider2D)SetupButterflySlotCatch(slotAnchor2, brow, slotGuideLength, slotGuideThickness, list)
			}, list.ToArray());
		}
	}

	private void MarkColliderRole(Collider2D collider, RingDragOtherKind kind, Component owner)
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

	private float ResolveGuideWallWidth(Transform root, Vector2 bodySize)
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

	private void RemoveLegacyButterflySlotNodes(Transform root)
	{
		for (int num = root.childCount - 1; num >= 0; num--)
		{
			Transform child = root.GetChild(num);
			if (((Object)child).name == "Top" || ((Object)child).name == "Bottom")
			{
				DestroyBakedObject(((Component)child).gameObject);
			}
		}
	}

	private Transform EnsureButterflySlotAnchor(Transform root, string slotName, string legacySlotName, float localX)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Transform val = root.Find(slotName);
		if ((Object)(object)val == (Object)null && !string.IsNullOrEmpty(legacySlotName) && legacySlotName != slotName)
		{
			val = root.Find(legacySlotName);
			if ((Object)(object)val != (Object)null)
			{
				((Object)val).name = slotName;
			}
		}
		if ((Object)(object)val == (Object)null)
		{
			val = new GameObject(slotName).transform;
			val.SetParent(root, false);
			val.localRotation = Quaternion.identity;
			val.localScale = Vector3.one;
		}
		Vector3 localPosition = val.localPosition;
		localPosition.x = localX;
		localPosition.y = 0f;
		val.localPosition = localPosition;
		val.localRotation = Quaternion.identity;
		EnsureWallChildTransform(val, "Top");
		EnsureWallChildTransform(val, "Bottom");
		return val;
	}

	private void EnsureWallChildTransform(Transform slotAnchor, string wallName)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)slotAnchor.Find(wallName) != (Object)null))
		{
			Transform transform = new GameObject(wallName).transform;
			transform.SetParent(slotAnchor, false);
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			transform.localPosition = Vector3.zero;
		}
	}

	private BoxCollider2D ConfigureButterflyRootBoxCollider(FixedButterflyObject brow, float bridgeLength, float bridgeWidth)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)brow).transform;
		Transform val = transform.Find("BridgeBlock");
		if ((Object)(object)val != (Object)null)
		{
			DestroyBakedObject(((Component)val).gameObject);
		}
		Collider2D selfCollider = brow.SelfCollider;
		BoxCollider2D val2 = (BoxCollider2D)(object)((selfCollider is BoxCollider2D) ? selfCollider : null);
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = ((Component)transform).GetComponent<BoxCollider2D>();
		}
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = ((Component)transform).gameObject.AddComponent<BoxCollider2D>();
		}
		RemoveExtraRootBoxColliders(((Component)transform).gameObject, val2);
		((Collider2D)val2).isTrigger = false;
		val2.size = new Vector2(Mathf.Max(0.01f, bridgeLength), Mathf.Max(0.01f, bridgeWidth));
		((Collider2D)val2).offset = Vector2.zero;
		((Component)transform).gameObject.layer = 7;
		MarkColliderRole((Collider2D)(object)val2, RingDragOtherKind.CatchGuideWall, (Component)(object)brow);
		return val2;
	}

	private void RemoveExtraRootBoxColliders(GameObject root, BoxCollider2D keep)
	{
		BoxCollider2D[] components = root.GetComponents<BoxCollider2D>();
		foreach (BoxCollider2D val in components)
		{
			if (!((Object)(object)val == (Object)null) && !((Object)(object)val == (Object)(object)keep))
			{
				if (Application.isPlaying)
				{
					Object.Destroy((Object)(object)val);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)val);
				}
			}
		}
	}

	private BoxCollider2D SetupButterflySlotCatch(Transform slotAnchor, FixedButterflyObject owner, float guideLength, float guideThickness, List<Collider2D> blockColliders)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		FixedButterflyRelative.EnsureOnAnchor(slotAnchor, ((Object)slotAnchor).name, owner);
		BoxCollider2D val = ((Component)slotAnchor).GetComponent<BoxCollider2D>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)slotAnchor).gameObject.AddComponent<BoxCollider2D>();
		}
		float num = Mathf.Max(0.01f, guideLength);
		float num2 = Mathf.Max(0.01f, guideThickness);
		float num3 = 0.08f;
		val.size = new Vector2(Mathf.Max(0.01f, num2 - num3), Mathf.Max(0.01f, num - num3));
		((Collider2D)val).offset = Vector2.zero;
		((Collider2D)val).isTrigger = true;
		MarkColliderRole((Collider2D)(object)val, RingDragOtherKind.CatchGuideBody, (Component)(object)owner);
		SetupButterflyGuideWallBoxColliders(slotAnchor, new Vector2(num2, num), owner);
		AppendGuideWallBlockColliders(slotAnchor, blockColliders);
		return val;
	}

	private void SetupButterflyGuideWallBoxColliders(Transform slotAnchor, Vector2 bodySize, FixedButterflyObject owner)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (owner == null)
		{
			owner = ((Component)slotAnchor).GetComponentInParent<FixedButterflyObject>();
		}
		float num = bodySize.x * 0.5f;
		float wallThickness = 0.07f;
		float wallWidth = ResolveGuideWallWidth(slotAnchor, bodySize);
		PlaceButterflyGuideWall(slotAnchor, "Top", num, wallWidth, wallThickness, owner, offsetAlongPositiveX: true);
		PlaceButterflyGuideWall(slotAnchor, "Bottom", 0f - num, wallWidth, wallThickness, owner, offsetAlongPositiveX: false);
	}

	private void PlaceButterflyGuideWall(Transform slotAnchor, string wallName, float localX, float wallWidth, float wallThickness, FixedObject fixedOwner, bool offsetAlongPositiveX)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Transform val = slotAnchor.Find(wallName);
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 localPosition = val.localPosition;
			localPosition.x = localX;
			localPosition.y = 0f;
			val.localPosition = localPosition;
			BoxCollider2D val2 = ((Component)val).GetComponent<BoxCollider2D>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)val).gameObject.AddComponent<BoxCollider2D>();
			}
			((Collider2D)val2).isTrigger = false;
			val2.size = new Vector2(wallThickness, wallWidth);
			((Collider2D)val2).offset = new Vector2(offsetAlongPositiveX ? (wallThickness * 0.5f) : ((0f - wallThickness) * 0.5f), 0f);
			MarkColliderRole((Collider2D)(object)val2, RingDragOtherKind.CatchGuideWall, (Component)(object)fixedOwner);
			((Component)val).gameObject.layer = 7;
		}
	}

	private void AppendGuideWallBlockColliders(Transform slotAnchor, List<Collider2D> blockColliders)
	{
		TryAppendWallCollider(slotAnchor, "Top", blockColliders);
		TryAppendWallCollider(slotAnchor, "Bottom", blockColliders);
	}

	private void TryAppendWallCollider(Transform slotAnchor, string wallName, List<Collider2D> blockColliders)
	{
		Transform val = slotAnchor.Find(wallName);
		if (!((Object)(object)val == (Object)null))
		{
			BoxCollider2D component = ((Component)val).GetComponent<BoxCollider2D>();
			if ((Object)(object)component != (Object)null && !blockColliders.Contains((Collider2D)(object)component))
			{
				blockColliders.Add((Collider2D)(object)component);
			}
		}
	}

	private void DestroyBakedObject(GameObject target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)target);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)target);
			}
		}
	}
}
