using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class GameplayReleasedPart
{
	public PartId PartId { get; }

	public string Reason { get; }

	public int ThreadingCount { get; }

	public GameplayReleasedPart(PartId partId, string reason, int threadingCount)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		PartId = partId;
		Reason = reason ?? string.Empty;
		ThreadingCount = threadingCount;
	}
}
