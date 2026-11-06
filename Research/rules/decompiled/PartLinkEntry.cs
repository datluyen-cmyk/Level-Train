using System;
using UnityEngine;

[Serializable]
public struct PartLinkEntry
{
	[field: SerializeField]
	public string PhysicPartId { get; set; }

	[field: SerializeField]
	public string FeatureId { get; set; }
}
