using UnityEngine;

namespace UnlockRing;

public class RingRemoveEffectEvent
{
	public Mesh SourceMesh;

	public Transform SourceTransform;

	public GameObject PartGo;

	public Transform EffectParent;

	public IGameplayObject SourcePart;

	public bool IsFromBomb;

	public bool Handled;
}
