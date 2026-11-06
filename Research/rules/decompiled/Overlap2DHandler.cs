using System;
using UniLog;
using UnityEngine;

[Serializable]
public class Overlap2DHandler : ILayerMaskHelper
{
	private const string TAG = "[RotateRing][Overlap2DHandler] ";

	[SerializeField]
	private int maxOverlapCollider;

	[SerializeField]
	private Collider2D collider2D;

	[SerializeField]
	private ContactFilter2D contactFilter;

	private Collider2D[] listOverlap;

	private LayerMask originalLayer;

	private bool isOriginalLayerSet;

	public Collider2D Collider => collider2D;

	public void SetCollider(Collider2D cld)
	{
		collider2D = cld;
	}

	public int GetOverlapCount()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)collider2D == (Object)null)
		{
			return 0;
		}
		if (listOverlap == null)
		{
			listOverlap = (Collider2D[])(object)new Collider2D[maxOverlapCollider];
		}
		return collider2D.Overlap(contactFilter, listOverlap);
	}

	public int GetOverlapColliders(Collider2D[] listOverlap)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)collider2D == (Object)null)
		{
			return 0;
		}
		if (listOverlap == null)
		{
			listOverlap = (Collider2D[])(object)new Collider2D[maxOverlapCollider];
		}
		return collider2D.Overlap(contactFilter, listOverlap);
	}

	[Obsolete("只需数量请用 GetOverlapCount()；需要遍历碰撞体请用 GetOverlapColliders(Collider2D[] buffer) 传入复用数组。此重载返回内部共享数组，跨调用持有会被覆盖。")]
	public Collider2D[] GetOverlapColliders(out int numOfColliders)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)collider2D == (Object)null)
		{
			numOfColliders = 0;
			Log.LogError("[RotateRing][Overlap2DHandler] ", "collider2D is null");
			return listOverlap;
		}
		if (listOverlap == null)
		{
			listOverlap = (Collider2D[])(object)new Collider2D[maxOverlapCollider];
		}
		numOfColliders = collider2D.Overlap(contactFilter, listOverlap);
		return listOverlap;
	}

	public void SetLayerFilter(int layer)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!isOriginalLayerSet)
		{
			originalLayer = contactFilter.layerMask;
			isOriginalLayerSet = true;
		}
		((ContactFilter2D)(ref contactFilter)).SetLayerMask(LayerMask.op_Implicit(1 << layer));
	}

	public void ResetLayerFilterToOriginal()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (isOriginalLayerSet)
		{
			((ContactFilter2D)(ref contactFilter)).SetLayerMask(originalLayer);
		}
	}

	public void SetLayer(int layer)
	{
		SetLayerFilter(layer);
	}

	public void ResetToOriginalLayer()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (isOriginalLayerSet)
		{
			((ContactFilter2D)(ref contactFilter)).SetLayerMask(originalLayer);
		}
	}
}
