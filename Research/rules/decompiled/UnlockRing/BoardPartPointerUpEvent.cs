using UnityEngine;
using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class BoardPartPointerUpEvent
{
	public PartId PartId { get; }

	public Vector2 ScreenPosition { get; }

	public Vector2 WorldPosition { get; }

	public BoardPartPointerUpEvent(PartId partId, Vector2 screenPosition, Vector2 worldPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		PartId = partId;
		ScreenPosition = screenPosition;
		WorldPosition = worldPosition;
	}
}
