using RotateRings.Common;
using UnityEngine;

namespace RotateRings.WeakGuide;

public sealed class HandWorldToUIFollower : MonoBehaviour
{
	[SerializeField]
	private Transform worldSource;

	[SerializeField]
	private RectTransform targetRect;

	private RectTransform cachedParentRect;

	public void Bind(Transform source, RectTransform target)
	{
		worldSource = source;
		targetRect = target;
		cachedParentRect = (RectTransform)(((Object)(object)target != (Object)null) ? /*isinst with value type is only supported in some contexts*/: null);
	}

	private void LateUpdate()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)targetRect == (Object)null) && !((Object)(object)worldSource == (Object)null))
		{
			if ((Object)(object)cachedParentRect == (Object)null || (Object)(object)cachedParentRect != (Object)(object)((Transform)targetRect).parent)
			{
				ref RectTransform reference = ref cachedParentRect;
				Transform parent = ((Transform)targetRect).parent;
				reference = (RectTransform)(object)((parent is RectTransform) ? parent : null);
			}
			if (!((Object)(object)cachedParentRect == (Object)null) && WorldToUIPostionUtil.WorldToLocalPoint(worldSource.position, cachedParentRect, out var localPoint))
			{
				targetRect.anchoredPosition = localPoint;
			}
		}
	}
}
