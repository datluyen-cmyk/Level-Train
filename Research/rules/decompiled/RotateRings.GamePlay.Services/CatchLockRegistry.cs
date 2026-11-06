using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class CatchLockRegistry
{
	private readonly CatchGuideLockRegistry guideLock = new CatchGuideLockRegistry();

	private readonly CatchButterflyLockRegistry catchButterflyLock = new CatchButterflyLockRegistry();

	internal int SolvedCount => guideLock.SolvedCount;

	internal IEnumerable<KeyValuePair<PartId, PartId>> GuideLockEntries => guideLock.GuideLockEntries;

	internal void Build(IReadOnlyList<GuideLinkEntry> guideLinks)
	{
		guideLock.Build(guideLinks);
		catchButterflyLock.Build(guideLinks);
	}

	internal void Clear()
	{
		guideLock.Clear();
		catchButterflyLock.Clear();
	}

	internal bool IsButterflySlotGuide(PartId guidePartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return catchButterflyLock.IsButterflySlotGuide(guidePartId);
	}

	internal bool TryGetButterflyHost(PartId guidePartId, out PartId hostPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return catchButterflyLock.TryGetHost(guidePartId, out hostPartId);
	}

	internal bool TryGetCatchLockedPartId(PartId guidePartId, out PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return guideLock.TryGetLockedPartIdForGuide(guidePartId, out lockedPartId);
	}

	internal bool IsCatchLockedPartSolved(PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return guideLock.IsLockedPartSolved(lockedPartId);
	}

	internal void MarkLockedPartSolvedIfRegistered(PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		guideLock.MarkLockedPartSolvedIfRegistered(lockedPartId);
	}

	internal void SeedSolvedFromGameplay()
	{
		guideLock.SeedSolvedFromGameplay();
	}
}
