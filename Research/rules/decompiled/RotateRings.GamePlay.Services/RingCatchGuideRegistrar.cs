using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class RingCatchGuideRegistrar : IPassiveLockRelationRegistrar
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	public void Register(PassiveLockRegistrationContext ctx)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<GuideLinkEntry> guideLinks = ctx.GuideLinks;
		if (guideLinks == null || guideLinks.Count == 0)
		{
			return;
		}
		RingObjectsService ringObjectsService = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		if (ringObjectsService == null)
		{
			return;
		}
		Dictionary<PartId, List<GuideLinkEntry>> dictionary = GroupGuidesByRing(guideLinks);
		if (dictionary.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int skippedButterflySlot = 0;
		PartId val = default(PartId);
		foreach (KeyValuePair<PartId, List<GuideLinkEntry>> item in dictionary)
		{
			PartId key = item.Key;
			List<GuideLinkEntry> value = item.Value;
			if (!ringObjectsService.TryGetRing(key, out var ring) || ring.IsSolved)
			{
				continue;
			}
			int num3 = 0;
			for (int i = 0; i < value.Count; i++)
			{
				GuideLinkEntry link = value[i];
				((PartId)(ref val))._002Ector(link.guidePartId);
				if (!ctx.TryResolveCatchPartnerFromLink(link, out var partnerFixed))
				{
					num2++;
				}
				else if (!partnerFixed.IsSolved)
				{
					ctx.RingDomain.MapCatchGuide(val, ring);
					num3 += RegisterRingObjectFixedBinding(ctx, key, val, partnerFixed, link.guidePartId, ref skippedButterflySlot);
				}
			}
			if (num3 > 0)
			{
				num += num3;
			}
		}
		_ = 0;
		_ = 0;
		_ = 0;
	}

	private Dictionary<PartId, List<GuideLinkEntry>> GroupGuidesByRing(IReadOnlyList<GuideLinkEntry> guideLinks)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<PartId, List<GuideLinkEntry>> dictionary = new Dictionary<PartId, List<GuideLinkEntry>>();
		PartId key = default(PartId);
		for (int i = 0; i < guideLinks.Count; i++)
		{
			GuideLinkEntry item = guideLinks[i];
			if (!string.IsNullOrEmpty(item.guidePartId) && !string.IsNullOrEmpty(item.lockedPartId))
			{
				((PartId)(ref key))._002Ector(item.lockedPartId);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<GuideLinkEntry>(2));
				}
				if (!ContainsGuideId(value, item.guidePartId))
				{
					value.Add(item);
				}
			}
		}
		return dictionary;
	}

	private static bool ContainsGuideId(List<GuideLinkEntry> entries, string guidePartId)
	{
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].guidePartId == guidePartId)
			{
				return true;
			}
		}
		return false;
	}

	private int RegisterRingObjectFixedBinding(PassiveLockRegistrationContext ctx, PartId ownerPartId, PartId partnerGuidePartId, FixedObject partnerFixed, string slotGuidePartId, ref int skippedButterflySlot)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		ctx.TempIds.Clear();
		if (partnerFixed is FixedButterflyObject fixedButterflyObject)
		{
			if (!fixedButterflyObject.TryGetSlotColliderIdByGuideId(slotGuidePartId, ctx.TempIds))
			{
				skippedButterflySlot++;
				return 0;
			}
		}
		else
		{
			partnerFixed.TryCollectSelfColliderId(ctx.TempIds);
		}
		return ctx.RingDomain.RegisterPartnerColliders(ownerPartId, partnerGuidePartId, ctx.TempIds);
	}
}
