using System.Collections.Generic;
using RotateRings.GamePlay.Objects;
using UnityEngine;
using UnlockRing.Core.Authoring;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public class RingFillPartDataService : GameplayServiceBase<GameplayManager>
{
	private const string TAG = "[GamePlay][RingPartLockData] ";

	public RingFillPartDataService(GameplayManager host)
		: base(host)
	{
	}

	public void FillData(GameLevelInfo levelInfo)
	{
		if (!((Object)(object)levelInfo == (Object)null))
		{
			ClearLevel();
			ResetLockCounts();
			ResetPaletteLockStateBeforeBindings();
			ApplyPaletteLockKeyBindings(levelInfo.PaletteLockKeyBindings);
			FillFromRingLinks(levelInfo.RingLinks);
			FillFromGuideLinks(levelInfo.GuideLinks);
		}
	}

	public void AdjustPaletteLockCurrentForSolvedRings(GameLevelInfo levelInfo)
	{
		if ((Object)(object)levelInfo == (Object)null)
		{
			return;
		}
		IReadOnlyList<PaletteLockKeyBindingEntry> paletteLockKeyBindings = levelInfo.PaletteLockKeyBindings;
		if (paletteLockKeyBindings == null || paletteLockKeyBindings.Count == 0)
		{
			return;
		}
		RingPaletteLockService paletteLock = base.Host.PaletteLock;
		Dictionary<RingObject, int> dictionary = new Dictionary<RingObject, int>();
		for (int i = 0; i < paletteLockKeyBindings.Count; i++)
		{
			PaletteLockKeyBindingEntry entry = paletteLockKeyBindings[i];
			TryFindRingByAssemblyName(entry.keyAssemblyName, out var ring);
			string[] lockedAssemblyNames = entry.lockedAssemblyNames;
			bool flag = IsPaletteKeyRingConsumed(entry, ring);
			if (flag)
			{
				if ((Object)(object)ring != (Object)null)
				{
					ClearRingPaletteLockKeyState(ring);
				}
			}
			else if ((Object)(object)ring != (Object)null)
			{
				ring.IsCurrentPaletteLockKey = true;
			}
			if (lockedAssemblyNames == null)
			{
				continue;
			}
			for (int j = 0; j < lockedAssemblyNames.Length; j++)
			{
				if (TryFindRingByAssemblyName(lockedAssemblyNames[j], out var ring2))
				{
					if (!dictionary.TryGetValue(ring2, out var value))
					{
						value = 0;
					}
					dictionary[ring2] = (flag ? value : (value + 1));
				}
			}
		}
		foreach (KeyValuePair<RingObject, int> item in dictionary)
		{
			RingObject key = item.Key;
			int num = ((!key.IsSolved) ? item.Value : 0);
			if (num <= 0)
			{
				ClearRingPaletteLockLockedState(key);
				paletteLock?.SetRemainingLockCount(key, 0);
			}
			else
			{
				paletteLock?.SetRemainingLockCount(key, num);
			}
		}
	}

	public override void ClearLevel()
	{
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList == null)
		{
			base.ClearLevel();
			return;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			IGameplayObject gameplayObject = readOnlyList[i];
			if (gameplayObject != null)
			{
				GameplayPartData partData = gameplayObject.PartData;
				partData.LockedBy?.Clear();
				partData.Locks?.Clear();
				gameplayObject.PartData = partData;
			}
		}
		base.ClearLevel();
	}

	private void ResetLockCounts()
	{
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList == null)
		{
			return;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			IGameplayObject gameplayObject = readOnlyList[i];
			if (gameplayObject != null)
			{
				gameplayObject.LockCount = gameplayObject.PartData.LockCount;
			}
		}
	}

	private void ResetPaletteLockStateBeforeBindings()
	{
		base.Host.PaletteLock?.ResetRemainingLockCounts();
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList == null)
		{
			return;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			if (readOnlyList[i] is RingObject { PartData: var partData } ringObject)
			{
				partData.IsPaletteLock = false;
				partData.IsPaletteLockKey = false;
				ringObject.PartData = partData;
				ringObject.PaletteLockFlags = PaletteLockFlags.None;
				ringObject.IsCurrentPaletteLockKey = false;
			}
		}
	}

	private void ApplyPaletteLockKeyBindings(IReadOnlyList<PaletteLockKeyBindingEntry> bindings)
	{
		if (bindings == null || bindings.Count == 0)
		{
			return;
		}
		for (int i = 0; i < bindings.Count; i++)
		{
			PaletteLockKeyBindingEntry paletteLockKeyBindingEntry = bindings[i];
			if (TryFindRingByAssemblyName(paletteLockKeyBindingEntry.keyAssemblyName, out var ring))
			{
				MarkRingAsPaletteLockKey(ring);
			}
			else
			{
				string.IsNullOrEmpty(paletteLockKeyBindingEntry.keyAssemblyName);
			}
			string[] lockedAssemblyNames = paletteLockKeyBindingEntry.lockedAssemblyNames;
			if (lockedAssemblyNames == null)
			{
				continue;
			}
			foreach (string text in lockedAssemblyNames)
			{
				if (TryFindRingByAssemblyName(text, out var ring2))
				{
					MarkRingAsPaletteLock(ring2);
				}
				else
				{
					string.IsNullOrEmpty(text);
				}
			}
		}
	}

	private bool IsPaletteKeyRingConsumed(PaletteLockKeyBindingEntry entry, RingObject keyRingOnBoard)
	{
		if ((Object)(object)keyRingOnBoard != (Object)null)
		{
			return keyRingOnBoard.IsSolved;
		}
		if (!base.Host.RingSave.IsResumeSession)
		{
			return false;
		}
		RingObject ring;
		return !TryFindRingByAssemblyName(entry.keyAssemblyName, out ring);
	}

	private static void ClearRingPaletteLockLockedState(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			GameplayPartData partData = ring.PartData;
			partData.IsPaletteLock = false;
			ring.PartData = partData;
			ring.PaletteLockFlags = PaletteLockFlags.None;
		}
	}

	private static void ClearRingPaletteLockKeyState(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			GameplayPartData partData = ring.PartData;
			partData.IsPaletteLockKey = false;
			ring.PartData = partData;
			ring.IsCurrentPaletteLockKey = false;
		}
	}

	private static void MarkRingAsPaletteLockKey(RingObject ring)
	{
		GameplayPartData partData = ring.PartData;
		partData.IsPaletteLockKey = true;
		ring.PartData = partData;
		ring.IsCurrentPaletteLockKey = true;
	}

	private static void MarkRingAsPaletteLock(RingObject ring)
	{
		GameplayPartData partData = ring.PartData;
		partData.IsPaletteLock = true;
		ring.PartData = partData;
		ring.PaletteLockFlags = PaletteLockFlags.ColorLock;
	}

	private bool TryFindRingByAssemblyName(string assemblyName, out RingObject ring)
	{
		ring = null;
		int num = EditablePartDraft.ParseAssemblyId(assemblyName);
		if (num <= 0)
		{
			return false;
		}
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList == null)
		{
			return false;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			if (readOnlyList[i] is RingObject { PartData: var partData } ringObject && partData.Id == num)
			{
				ring = ringObject;
				return true;
			}
		}
		return false;
	}

	private void FillFromRingLinks(IReadOnlyList<RingLinkEntry> ringLinks)
	{
		if (ringLinks == null)
		{
			return;
		}
		for (int i = 0; i < ringLinks.Count; i++)
		{
			RingLinkEntry ringLinkEntry = ringLinks[i];
			if (!string.IsNullOrEmpty(ringLinkEntry.sourceRingId) && !string.IsNullOrEmpty(ringLinkEntry.targetPartId))
			{
				if (TryGetObject(ringLinkEntry.sourceRingId, out var obj))
				{
					AddLink(obj, new PartLinkEntry
					{
						PhysicPartId = ringLinkEntry.targetPartId,
						FeatureId = ringLinkEntry.targetFeatureId
					}, isLockedBy: false);
				}
				if (TryGetObject(ringLinkEntry.targetPartId, out var obj2))
				{
					AddLink(obj2, new PartLinkEntry
					{
						PhysicPartId = ringLinkEntry.sourceRingId,
						FeatureId = ringLinkEntry.pinFeatureId
					}, isLockedBy: true);
				}
			}
		}
	}

	private void FillFromGuideLinks(IReadOnlyList<GuideLinkEntry> guideLinks)
	{
		if (guideLinks == null)
		{
			return;
		}
		for (int i = 0; i < guideLinks.Count; i++)
		{
			GuideLinkEntry guideLinkEntry = guideLinks[i];
			if (!string.IsNullOrEmpty(guideLinkEntry.guidePartId) && !string.IsNullOrEmpty(guideLinkEntry.lockedPartId))
			{
				if (TryGetObject(guideLinkEntry.guidePartId, out var obj))
				{
					AddLink(obj, new PartLinkEntry
					{
						PhysicPartId = guideLinkEntry.lockedPartId,
						FeatureId = null
					}, isLockedBy: true);
				}
				if (TryGetObject(guideLinkEntry.lockedPartId, out var obj2))
				{
					AddLink(obj2, new PartLinkEntry
					{
						PhysicPartId = guideLinkEntry.guidePartId,
						FeatureId = null
					}, isLockedBy: false);
				}
			}
		}
	}

	private void AddLink(IGameplayObject obj, PartLinkEntry entry, bool isLockedBy)
	{
		if (obj == null || string.IsNullOrEmpty(entry.PhysicPartId))
		{
			return;
		}
		GameplayPartData partData = obj.PartData;
		List<PartLinkEntry> list = (isLockedBy ? partData.LockedBy : partData.Locks);
		if (list == null)
		{
			list = new List<PartLinkEntry>(4);
			if (isLockedBy)
			{
				partData.LockedBy = list;
			}
			else
			{
				partData.Locks = list;
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].PhysicPartId == entry.PhysicPartId)
			{
				return;
			}
		}
		list.Add(entry);
		obj.PartData = partData;
	}

	private bool TryGetObject(string physicPartId, out IGameplayObject obj)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		obj = null;
		if (string.IsNullOrEmpty(physicPartId) || base.Host.ObjectsService == null)
		{
			return false;
		}
		return base.Host.ObjectsService.TryGetPartObject(new PartId(physicPartId), out obj);
	}
}
