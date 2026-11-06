namespace RotateRings.UI.PropItem;

public readonly struct PropBadgeState
{
	public readonly bool IsLocked;

	public readonly int DisplayCount;

	public readonly bool HasPendingFree;

	public PropBadgeState(bool isLocked, int displayCount, bool hasPendingFree)
	{
		IsLocked = isLocked;
		DisplayCount = displayCount;
		HasPendingFree = hasPendingFree;
	}
}
