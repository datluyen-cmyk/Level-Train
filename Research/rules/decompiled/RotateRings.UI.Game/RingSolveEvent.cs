using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.UI.Game;

public class RingSolveEvent
{
	public int CollectCount { get; set; }

	public Vector3 WorldPosition { get; set; }

	public bool HasSolvedPartId { get; set; }

	public PartId SolvedPartId { get; set; }

	public bool HasCollectScreenPosition { get; set; }

	public Vector2 CollectScreenPosition { get; set; }
}
