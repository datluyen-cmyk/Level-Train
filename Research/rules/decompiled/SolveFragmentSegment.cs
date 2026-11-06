using UnityEngine;

public readonly struct SolveFragmentSegment
{
	public Transform PartTransform { get; }

	public Mesh PartMesh { get; }

	public GameObject MaterialSourceGo { get; }

	public string EffectPrefabName { get; }

	public SolveFragmentSegment(Transform partTransform, Mesh partMesh, GameObject materialSourceGo, string effectPrefabName = null)
	{
		PartTransform = partTransform;
		PartMesh = partMesh;
		MaterialSourceGo = materialSourceGo;
		EffectPrefabName = effectPrefabName;
	}
}
