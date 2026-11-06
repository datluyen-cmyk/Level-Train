using UnityEngine;
using UnlockRing.Core.Domain;

namespace UnlockRing.Prop;

public readonly struct PropPointerSnapshot
{
	public CoreBoardRenderer Board { get; init; }

	public Vector2 ScreenPosition { get; init; }

	public Vector2 WorldPosition { get; init; }

	public Vector2 PointerDownScreen { get; init; }

	public PartId? HitPart { get; init; }

	public float ScreenDistanceFromDown { get; init; }
}
