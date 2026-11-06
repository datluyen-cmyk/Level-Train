using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class PullToRingLinkRegistrar : IPassiveLockRelationRegistrar
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	public void Register(PassiveLockRegistrationContext ctx)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
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
			if (!string.IsNullOrEmpty(item.sourceRingId) && !string.IsNullOrEmpty(item.targetPartId))
			{
				((PartId)(ref val))._002Ector(item.sourceRingId);
				((PartId)(ref val2))._002Ector(item.targetPartId);
				RingObject ring2;
				PullObject pull2;
				if (ringObjectsService.TryGetRing(val, out var ring) && !ring.IsSolved && ringObjectsService.TryGetPull(val2, out var pull) && !pull.IsSolved)
				{
					num += RegisterLinkBinding(ctx, val2, val, ring, item.pinFeatureId);
				}
				else if (ringObjectsService.TryGetRing(val2, out ring2) && !ring2.IsSolved && ringObjectsService.TryGetPull(val, out pull2) && !pull2.IsSolved)
				{
					num += RegisterLinkBinding(ctx, val, val2, ring2, item.targetFeatureId);
				}
			}
		}
		_ = 0;
	}

	private int RegisterLinkBinding(PassiveLockRegistrationContext ctx, PartId ownerPartId, PartId partnerPartId, RingObject partnerRing, string partnerFeatureId)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		ctx.TempIds.Clear();
		partnerRing.CollectSelfRelativeBlockColliderIds(ctx.TempIds, partnerFeatureId);
		return ctx.PullDomain.RegisterPartnerColliders(ownerPartId, partnerPartId, ctx.TempIds);
	}
}
