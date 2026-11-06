using System.Collections.Generic;
using UnityEngine;
using UnlockRing;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services.CharacterExpression;

internal static class CharacterExpressionWorldAnchor
{
	public static bool TryGetReleasedPartsCenterWorldPosition(RingObjectsService objectsService, IReadOnlyList<GameplayReleasedPart> releasedParts, out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = Vector3.zero;
		if (objectsService == null || releasedParts == null || releasedParts.Count == 0)
		{
			return false;
		}
		Vector3 val = Vector3.zero;
		int num = 0;
		for (int i = 0; i < releasedParts.Count; i++)
		{
			GameplayReleasedPart gameplayReleasedPart = releasedParts[i];
			if (gameplayReleasedPart != null && TryGetPartVisualCenterWorldPosition(objectsService, gameplayReleasedPart.PartId, out var worldPosition2))
			{
				val += worldPosition2;
				num++;
			}
		}
		if (num == 0)
		{
			return false;
		}
		worldPosition = val / (float)num;
		return true;
	}

	public static bool TryGetPartVisualCenterWorldPosition(RingObjectsService objectsService, PartId partId, out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = Vector3.zero;
		if (objectsService != null && objectsService.TryGetPartObject(partId, out var gameplayObject))
		{
			Component val = (Component)((gameplayObject is Component) ? gameplayObject : null);
			if (val != null)
			{
				Renderer component = val.GetComponent<Renderer>();
				if ((Object)(object)component != (Object)null)
				{
					Bounds bounds = component.bounds;
					worldPosition = ((Bounds)(ref bounds)).center;
					return true;
				}
				worldPosition = val.transform.position;
				return true;
			}
		}
		return false;
	}
}
