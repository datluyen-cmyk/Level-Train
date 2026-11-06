using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public interface IPartLockFeature
{
	bool ShouldPreserveBindingOnDragExit(PartId partId);

	bool ArePartsConnected(PartId lockedPartId, PartId lockerPartId);

	void OnObjectSolved(RingObject solvedRing);
}
