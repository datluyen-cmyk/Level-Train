using System;

[Serializable]
public struct RingLinkEntry
{
	public string sourceRingId;

	public string pinFeatureId;

	public string targetPartId;

	public string targetFeatureId;

	public int relationKind;

	public int activityKind;
}
