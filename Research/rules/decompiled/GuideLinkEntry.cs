using System;
using UnityEngine.Serialization;

[Serializable]
public struct GuideLinkEntry
{
	public string guidePartId;

	[FormerlySerializedAs("stickPartId")]
	public string lockedPartId;

	public string hostPartId;
}
