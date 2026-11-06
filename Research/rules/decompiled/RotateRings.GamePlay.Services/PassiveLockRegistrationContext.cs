using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class PassiveLockRegistrationContext
{
	internal readonly HashSet<int> TempIds = new HashSet<int>();

	internal IReadOnlyList<RingLinkEntry> RingLinks { get; }

	internal IReadOnlyList<GuideLinkEntry> GuideLinks { get; }

	internal RingPassiveLockDomain RingDomain { get; }

	internal PullPassiveLockDomain PullDomain { get; }

	internal CatchLockRegistry CatchLockRegistry { get; }

	internal PassiveLockRegistrationContext(IReadOnlyList<RingLinkEntry> ringLinks, IReadOnlyList<GuideLinkEntry> guideLinks, RingPassiveLockDomain ringDomain, PullPassiveLockDomain pullDomain, CatchLockRegistry catchLockRegistry)
	{
		RingLinks = ringLinks;
		GuideLinks = guideLinks;
		RingDomain = ringDomain;
		PullDomain = pullDomain;
		CatchLockRegistry = catchLockRegistry;
	}

	internal bool TryResolveCatchPartnerFromLink(GuideLinkEntry link, out FixedObject partnerFixed)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		partnerFixed = null;
		RingObjectsService ringObjectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		if (ringObjectsService == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(link.hostPartId) && ringObjectsService.TryGetPartObject(new PartId(link.hostPartId), out var gameplayObject))
		{
			partnerFixed = gameplayObject as FixedObject;
			if ((Object)(object)partnerFixed != (Object)null)
			{
				return true;
			}
		}
		if (string.IsNullOrEmpty(link.guidePartId))
		{
			return false;
		}
		if (!ringObjectsService.TryGetPartObject(new PartId(link.guidePartId), out var gameplayObject2))
		{
			return false;
		}
		partnerFixed = gameplayObject2 as FixedObject;
		return (Object)(object)partnerFixed != (Object)null;
	}
}
