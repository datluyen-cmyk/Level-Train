using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

internal sealed class CatchGuideLockRegistry
{
	private readonly Dictionary<PartId, PartId> guideToLockedPartId = new Dictionary<PartId, PartId>();

	private readonly HashSet<PartId> lockedPartIds = new HashSet<PartId>();

	private readonly HashSet<PartId> solvedLockedPartIds = new HashSet<PartId>();

	internal int SolvedCount => solvedLockedPartIds.Count;

	internal IEnumerable<KeyValuePair<PartId, PartId>> GuideLockEntries => guideToLockedPartId;

	internal void Build(IReadOnlyList<GuideLinkEntry> guideLinks)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		guideToLockedPartId.Clear();
		lockedPartIds.Clear();
		solvedLockedPartIds.Clear();
		if (guideLinks == null || guideLinks.Count == 0)
		{
			return;
		}
		PartId val = default(PartId);
		for (int i = 0; i < guideLinks.Count; i++)
		{
			GuideLinkEntry guideLinkEntry = guideLinks[i];
			if (!string.IsNullOrEmpty(guideLinkEntry.guidePartId) && !string.IsNullOrEmpty(guideLinkEntry.lockedPartId))
			{
				((PartId)(ref val))._002Ector(guideLinkEntry.lockedPartId);
				guideToLockedPartId[new PartId(guideLinkEntry.guidePartId)] = val;
				lockedPartIds.Add(val);
			}
		}
	}

	internal void Clear()
	{
		guideToLockedPartId.Clear();
		lockedPartIds.Clear();
		solvedLockedPartIds.Clear();
	}

	internal bool TryGetLockedPartIdForGuide(PartId guidePartId, out PartId lockedPartId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return guideToLockedPartId.TryGetValue(guidePartId, out lockedPartId);
	}

	internal bool IsLockedPartSolved(PartId lockedPartId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(((PartId)(ref lockedPartId)).Value))
		{
			return solvedLockedPartIds.Contains(lockedPartId);
		}
		return false;
	}

	internal void MarkLockedPartSolvedIfRegistered(PartId lockedPartId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(((PartId)(ref lockedPartId)).Value) && lockedPartIds.Contains(lockedPartId))
		{
			solvedLockedPartIds.Add(lockedPartId);
		}
	}

	internal void SeedSolvedFromGameplay()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		RingObjectsService objects = SingletonComponent<GameplayManager>.Instance?.ObjectsService;
		foreach (PartId lockedPartId in lockedPartIds)
		{
			if (!solvedLockedPartIds.Contains(lockedPartId) && IsLockedPartAlreadyClearedOnBoard(lockedPartId, objects))
			{
				solvedLockedPartIds.Add(lockedPartId);
			}
		}
	}

	private static bool IsLockedPartAlreadyClearedOnBoard(PartId lockedPartId, RingObjectsService objects)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (objects == null || !objects.TryGetPartObject(lockedPartId, out var gameplayObject))
		{
			return true;
		}
		Component val = (Component)((gameplayObject is Component) ? gameplayObject : null);
		if (val != null && (Object)(object)val == (Object)null)
		{
			return true;
		}
		return gameplayObject.IsSolved;
	}
}
