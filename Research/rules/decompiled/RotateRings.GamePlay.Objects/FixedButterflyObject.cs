using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Services;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Objects;

public class FixedButterflyObject : FixedObject
{
	public readonly struct FragmentPartInfo
	{
		public readonly Transform PartTransform;

		public readonly Mesh PartMesh;

		public readonly bool IsBridge;

		public readonly float PartLength;

		public FragmentPartInfo(Transform partTransform, Mesh partMesh, bool isBridge, float partLength)
		{
			PartTransform = partTransform;
			PartMesh = partMesh;
			IsBridge = isBridge;
			PartLength = partLength;
		}
	}

	[SerializeField]
	private Collider2D[] slotColliders;

	[SerializeField]
	private Collider2D bridgeCollider;

	[SerializeField]
	private string slotGuidePartId0;

	[SerializeField]
	private string slotGuidePartId1;

	internal Collider2D[] SlotColliders => slotColliders;

	internal Collider2D BridgeCollider => bridgeCollider;

	public void SetSlotGuidePartIds(string guideIdForSlot0, string guideIdForSlot1)
	{
		slotGuidePartId0 = guideIdForSlot0;
		slotGuidePartId1 = guideIdForSlot1;
	}

	public bool TryGetSlotGuidePartId(int slotIndex, out string guidePartId)
	{
		guidePartId = slotIndex switch
		{
			0 => slotGuidePartId0, 
			1 => slotGuidePartId1, 
			_ => null, 
		};
		return !string.IsNullOrEmpty(guidePartId);
	}

	private bool TryGetSlotIndexByGuideId(string guideId, out int slotIndex)
	{
		slotIndex = 0;
		if (string.IsNullOrEmpty(guideId))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(slotGuidePartId0) && slotGuidePartId0 == guideId)
		{
			slotIndex = 0;
			return true;
		}
		if (!string.IsNullOrEmpty(slotGuidePartId1) && slotGuidePartId1 == guideId)
		{
			slotIndex = 1;
			return true;
		}
		return false;
	}

	public void RegisterSlotGuideBindings(RingObjectsService objectsService)
	{
		EnsureSlotRelatives();
		if (objectsService != null)
		{
			RegisterGuidePartIdBinding(objectsService, slotGuidePartId0);
			RegisterGuidePartIdBinding(objectsService, slotGuidePartId1);
		}
	}

	private void EnsureSlotRelatives()
	{
		EnsureSlotRelative(0);
		EnsureSlotRelative(1);
	}

	private void EnsureSlotRelative(int slotIndex)
	{
		if (!TryGetSlotGuidePartId(slotIndex, out var guidePartId))
		{
			return;
		}
		Transform val = ((Component)this).transform.Find(guidePartId);
		if (!((Object)(object)val == (Object)null))
		{
			FixedButterflyRelative fixedButterflyRelative = ((Component)val).GetComponent<FixedButterflyRelative>();
			if ((Object)(object)fixedButterflyRelative == (Object)null)
			{
				fixedButterflyRelative = ((Component)val).gameObject.AddComponent<FixedButterflyRelative>();
			}
			fixedButterflyRelative.Configure(guidePartId, this);
		}
	}

	private void RegisterGuidePartIdBinding(RingObjectsService objectsService, string guidePartId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(guidePartId))
		{
			objectsService.RegisterGuidePartIdBinding(new PartId(guidePartId), this);
		}
	}

	public bool TryGetSlotColliderIdByGuideId(string guideId, HashSet<int> output)
	{
		if (!TryGetSlotIndexByGuideId(guideId, out var slotIndex))
		{
			return false;
		}
		return TryGetSlotColliderId(slotIndex, output);
	}

	private bool TryGetSlotColliderId(int slotIndex, HashSet<int> output)
	{
		if (output == null || slotColliders == null || slotIndex < 0 || slotIndex >= slotColliders.Length)
		{
			return false;
		}
		Collider2D val = slotColliders[slotIndex];
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		output.Add(((Object)val).GetInstanceID());
		return true;
	}

	public override void DoFade()
	{
		base.DoFade();
		SetSlotAndBridgeCollidersEnabled(enabled: false);
	}

	private void SetSlotAndBridgeCollidersEnabled(bool enabled)
	{
		if (slotColliders != null)
		{
			for (int i = 0; i < slotColliders.Length; i++)
			{
				Collider2D val = slotColliders[i];
				if ((Object)(object)val != (Object)null)
				{
					((Behaviour)val).enabled = enabled;
				}
			}
		}
		if ((Object)(object)bridgeCollider != (Object)null)
		{
			((Behaviour)bridgeCollider).enabled = enabled;
		}
	}

	internal void SetColliders(Collider2D bridge, Collider2D[] slotBodies, Collider2D[] blockColliders)
	{
		bridgeCollider = bridge;
		slotColliders = slotBodies;
		SetBlockColliders(blockColliders);
	}

	public FragmentPartInfo[] GetButterflyFragmentParts()
	{
		FixedButterflyMeshController component = ((Component)this).GetComponent<FixedButterflyMeshController>();
		if ((Object)(object)component == (Object)null)
		{
			return Array.Empty<FragmentPartInfo>();
		}
		FixedButterflyMeshPart[] meshParts = component.GetMeshParts();
		if (meshParts == null || meshParts.Length == 0)
		{
			return Array.Empty<FragmentPartInfo>();
		}
		FragmentPartInfo[] array = new FragmentPartInfo[meshParts.Length];
		for (int i = 0; i < meshParts.Length; i++)
		{
			FixedButterflyMeshPart fixedButterflyMeshPart = meshParts[i];
			if (!((Object)(object)fixedButterflyMeshPart == (Object)null))
			{
				MeshFilter component2 = ((Component)fixedButterflyMeshPart).GetComponent<MeshFilter>();
				Mesh partMesh = (((Object)(object)component2 != (Object)null) ? component2.sharedMesh : null);
				bool isBridge = fixedButterflyMeshPart.Role == FixedButterflyMeshPart.PartRole.Bridge;
				array[i] = new FragmentPartInfo(((Component)fixedButterflyMeshPart).transform, partMesh, isBridge, fixedButterflyMeshPart.QuadWidth);
			}
		}
		return array;
	}
}
