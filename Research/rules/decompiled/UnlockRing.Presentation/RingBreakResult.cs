using UnityEngine;

namespace UnlockRing.Presentation;

public sealed class RingBreakResult : IPresentationResult
{
	public IGameplayObject GameplayObject { get; }

	public Mesh SourceMesh { get; }

	public Transform SourceTransform { get; }

	public GameObject PartGo { get; }

	public Transform EffectParent { get; }

	public IGameplayObject SourcePart { get; }

	public bool IsFromBomb { get; }

	public RingBreakResult(IGameplayObject gameplayObject, Mesh sourceMesh, Transform sourceTransform, GameObject partGo, Transform effectParent, IGameplayObject sourcePart, bool isFromBomb)
	{
		GameplayObject = gameplayObject;
		SourceMesh = sourceMesh;
		SourceTransform = sourceTransform;
		PartGo = partGo;
		EffectParent = effectParent;
		SourcePart = sourcePart;
		IsFromBomb = isFromBomb;
	}
}
