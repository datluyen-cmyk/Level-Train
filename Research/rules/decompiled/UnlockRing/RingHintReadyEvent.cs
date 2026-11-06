using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class RingHintReadyEvent
{
	public PartId TargetPartId { get; }

	public RingHintReadyEvent(PartId targetPartId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		TargetPartId = targetPartId;
	}
}
