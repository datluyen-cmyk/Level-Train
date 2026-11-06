using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class CatchButterflyLockRegistry
{
	private readonly Dictionary<PartId, PartId> hostPartIdByGuidePartId = new Dictionary<PartId, PartId>();

	internal void Build(IReadOnlyList<GuideLinkEntry> guideLinks)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		hostPartIdByGuidePartId.Clear();
		if (guideLinks == null || guideLinks.Count == 0)
		{
			return;
		}
		for (int i = 0; i < guideLinks.Count; i++)
		{
			GuideLinkEntry guideLinkEntry = guideLinks[i];
			if (!string.IsNullOrEmpty(guideLinkEntry.guidePartId) && !string.IsNullOrEmpty(guideLinkEntry.hostPartId))
			{
				hostPartIdByGuidePartId[new PartId(guideLinkEntry.guidePartId)] = new PartId(guideLinkEntry.hostPartId);
			}
		}
	}

	internal void Clear()
	{
		hostPartIdByGuidePartId.Clear();
	}

	internal bool IsButterflySlotGuide(PartId guidePartId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(((PartId)(ref guidePartId)).Value))
		{
			return hostPartIdByGuidePartId.ContainsKey(guidePartId);
		}
		return false;
	}

	internal bool TryGetHost(PartId guidePartId, out PartId hostPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return hostPartIdByGuidePartId.TryGetValue(guidePartId, out hostPartId);
	}
}
