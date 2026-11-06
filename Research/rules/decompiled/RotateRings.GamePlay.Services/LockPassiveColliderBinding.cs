using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal readonly struct LockPassiveColliderBinding
{
	public readonly PartId PartId;

	public readonly int RingRelativeId;

	public readonly float BaselinePartnerAngleZ;

	public LockPassiveColliderBinding(PartId partId, int ringRelativeId, float baselinePartnerAngleZ = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		PartId = partId;
		RingRelativeId = ringRelativeId;
		BaselinePartnerAngleZ = baselinePartnerAngleZ;
	}
}
