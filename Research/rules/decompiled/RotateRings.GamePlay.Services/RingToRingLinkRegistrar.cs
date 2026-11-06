using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingToRingLinkRegistrar : IPassiveLockRelationRegistrar
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	public void Register(PassiveLockRegistrationContext ctx)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<RingLinkEntry> ringLinks = ctx.RingLinks;
		if (ringLinks == null || ringLinks.Count == 0)
		{
			return;
		}
		RingObjectsService ringObjectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		if (ringObjectsService == null)
		{
			return;
		}
		int num = 0;
		PartId val = default(PartId);
		PartId val2 = default(PartId);
		foreach (RingLinkEntry item in ringLinks)
		{
			if (item.relationKind != 1 && !string.IsNullOrEmpty(item.sourceRingId) && !string.IsNullOrEmpty(item.targetPartId))
			{
				((PartId)(ref val))._002Ector(item.sourceRingId);
				((PartId)(ref val2))._002Ector(item.targetPartId);
				RingObject ring;
				bool flag = ringObjectsService.TryGetRing(val, out ring);
				RingObject ring2;
				bool flag2 = ringObjectsService.TryGetRing(val2, out ring2);
				if (flag && flag2 && !ring.IsSolved && !ring2.IsSolved)
				{
					num += RegisterLinkBinding(ctx, val2, val, ring, item.pinFeatureId);
				}
			}
		}
		_ = 0;
	}

	private int RegisterLinkBinding(PassiveLockRegistrationContext ctx, PartId ownerPartId, PartId partnerPartId, RingObject partnerRing, string partnerFeatureId)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		float initialAngleZ = partnerRing.GetInitialAngleZ();
		ctx.TempIds.Clear();
		partnerRing.CollectSelfRelativeBlockColliderIds(ctx.TempIds, partnerFeatureId);
		return ctx.RingDomain.RegisterPartnerColliders(ownerPartId, partnerPartId, ctx.TempIds, initialAngleZ);
	}
}
