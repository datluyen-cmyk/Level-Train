using UnityEngine;
using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class BoardPartPointerDownEvent
{
	public BoardPartPointerDownKind Kind { get; }

	public PartId? PartId { get; }

	public Vector2 ScreenPosition { get; }

	public Vector2 WorldPosition { get; }

	public BoardPartPointerDownEvent(BoardPartPointerDownKind kind, PartId? partId, Vector2 screenPosition, Vector2 worldPosition)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Kind = kind;
		PartId = partId;
		ScreenPosition = screenPosition;
		WorldPosition = worldPosition;
	}
}
