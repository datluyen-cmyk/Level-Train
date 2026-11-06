using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class PullCatchGuideRegistrar : IPassiveLockRelationRegistrar
{
	private const string TAG = "[GamePlay][RingLockPassive] ";

	public void Register(PassiveLockRegistrationContext ctx)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
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
		Dictionary<PartId, List<GuideLinkEntry>> dictionary = GroupGuidesByStick(guideLinks);
		if (dictionary.Count == 0)
		{
			return;
		}
		int num = 0;
		PartId val = default(PartId);
		foreach (KeyValuePair<PartId, List<GuideLinkEntry>> item in dictionary)
		{
			PartId key = item.Key;
			List<GuideLinkEntry> value = item.Value;
			if (!ringObjectsService.TryGetPull(key, out var pull) || pull.IsSolved)
			{
				continue;
			}
			int num2 = 0;
			for (int i = 0; i < value.Count; i++)
			{
				GuideLinkEntry link = value[i];
				((PartId)(ref val))._002Ector(link.guidePartId);
				if (ctx.TryResolveCatchPartnerFromLink(link, out var partnerFixed) && !partnerFixed.IsSolved)
				{
					ctx.PullDomain.MapCatchGuide(val, pull);
					num2 += RegisterPullObjectFixedBinding(ctx, key, val, partnerFixed, link.guidePartId);
				}
			}
			if (num2 > 0)
			{
				ctx.PullDomain.GetStaticBindingCount(key);
				List<PartId> list = new List<PartId>(value.Count);
				for (int j = 0; j < value.Count; j++)
				{
					list.Add(new PartId(value[j].guidePartId));
				}
				num += num2;
			}
		}
		_ = 0;
	}

	private Dictionary<PartId, List<GuideLinkEntry>> GroupGuidesByStick(IReadOnlyList<GuideLinkEntry> guideLinks)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<PartId, List<GuideLinkEntry>> dictionary = new Dictionary<PartId, List<GuideLinkEntry>>();
		PartId key = default(PartId);
		for (int i = 0; i < guideLinks.Count; i++)
		{
			GuideLinkEntry link = guideLinks[i];
			if (!string.IsNullOrEmpty(link.guidePartId) && !string.IsNullOrEmpty(link.lockedPartId))
			{
				((PartId)(ref key))._002Ector(link.lockedPartId);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<GuideLinkEntry>(2));
				}
				if (!value.Exists((GuideLinkEntry e) => e.guidePartId == link.guidePartId))
				{
					value.Add(link);
				}
			}
		}
		return dictionary;
	}

	private int RegisterPullObjectFixedBinding(PassiveLockRegistrationContext ctx, PartId ownerPartId, PartId partnerPartId, FixedObject partnerFixed, string slotGuidePartId = null)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		ctx.TempIds.Clear();
		if (partnerFixed is FixedButterflyObject fixedButterflyObject)
		{
			if (!fixedButterflyObject.TryGetSlotColliderIdByGuideId(slotGuidePartId, ctx.TempIds))
			{
				return 0;
			}
		}
		else
		{
			partnerFixed.TryCollectSelfColliderId(ctx.TempIds);
		}
		return ctx.PullDomain.RegisterPartnerColliders(ownerPartId, partnerPartId, ctx.TempIds);
	}

	private string FormatGuidePartIds(IReadOnlyList<PartId> guidePartIds)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (guidePartIds == null || guidePartIds.Count == 0)
		{
			return string.Empty;
		}
		PartId val = guidePartIds[0];
		string text = ((PartId)(ref val)).Value;
		for (int i = 1; i < guidePartIds.Count; i++)
		{
			string text2 = text;
			val = guidePartIds[i];
			text = text2 + "," + ((PartId)(ref val)).Value;
		}
		return text;
	}
}
