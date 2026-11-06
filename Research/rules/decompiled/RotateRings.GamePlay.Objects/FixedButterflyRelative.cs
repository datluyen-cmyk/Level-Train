using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Objects;

public class FixedButterflyRelative : MonoBehaviour
{
	[SerializeField]
	private string guidePartId;

	[SerializeField]
	private FixedObject hostFixed;

	public FixedObject HostFixed => hostFixed;

	public static void EnsureOnAnchor(Transform anchor, string guidePartId, FixedObject hostFixed)
	{
		if (!((Object)(object)anchor == (Object)null) && !string.IsNullOrEmpty(guidePartId))
		{
			FixedButterflyRelative fixedButterflyRelative = ((Component)anchor).GetComponent<FixedButterflyRelative>();
			if ((Object)(object)fixedButterflyRelative == (Object)null)
			{
				fixedButterflyRelative = ((Component)anchor).gameObject.AddComponent<FixedButterflyRelative>();
			}
			fixedButterflyRelative.Configure(guidePartId, hostFixed);
		}
	}

	public void Configure(string guideId, FixedObject host)
	{
		guidePartId = guideId;
		hostFixed = host;
	}

	public bool TryGetGuidePartId(out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if (string.IsNullOrEmpty(guidePartId))
		{
			return false;
		}
		partId = new PartId(guidePartId);
		return true;
	}

	public bool CollectCatchColliderIds(HashSet<int> output)
	{
		if (output == null)
		{
			return false;
		}
		int num = 0;
		Collider2D[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider2D>(true);
		foreach (Collider2D val in componentsInChildren)
		{
			if (!((Object)(object)val == (Object)null) && RingColliderQuery.TryGetMarker(val, out var marker) && (marker.Role == RingDragOtherKind.CatchGuideBody || marker.Role == RingDragOtherKind.CatchGuideWall))
			{
				output.Add(((Object)val).GetInstanceID());
				num++;
			}
		}
		return num > 0;
	}
}
