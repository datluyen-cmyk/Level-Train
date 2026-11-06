using System.Collections.Generic;
using UniLog;
using UnityEngine;
using UnlockRing.Core.Authoring;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockKeyColors
{
	private const string Tag = "[RingPaletteLockKeyColors] ";

	private static readonly PaletteLockKeyColor[] KeyColorTryOrder = new PaletteLockKeyColor[4]
	{
		PaletteLockKeyColor.Blue,
		PaletteLockKeyColor.Green,
		PaletteLockKeyColor.Red,
		PaletteLockKeyColor.Yellow
	};

	private readonly Dictionary<int, PaletteLockKeyColor> keyColorByAssemblyId = new Dictionary<int, PaletteLockKeyColor>();

	private IReadOnlyList<PaletteLockKeyBindingEntry> cachedBindings;

	public void Clear()
	{
		keyColorByAssemblyId.Clear();
		cachedBindings = null;
	}

	public void Rebuild(IReadOnlyList<PaletteLockKeyBindingEntry> bindings, IReadOnlyDictionary<int, GameplayPartData> partByAssemblyId)
	{
		Clear();
		cachedBindings = bindings;
		if (bindings == null || bindings.Count == 0)
		{
			return;
		}
		List<PaletteLockKeyBindingEntry> list = new List<PaletteLockKeyBindingEntry>(bindings.Count);
		for (int i = 0; i < bindings.Count; i++)
		{
			list.Add(bindings[i]);
		}
		list.Sort(CompareBindingsByKeyAssemblyId);
		HashSet<PaletteLockKeyColor> hashSet = new HashSet<PaletteLockKeyColor>();
		for (int j = 0; j < list.Count; j++)
		{
			PaletteLockKeyBindingEntry entry = list[j];
			int num = EditablePartDraft.ParseAssemblyId(entry.keyAssemblyName);
			if (num > 0)
			{
				HashSet<PaletteLockKeyColor> forbidden = BuildBindingForbidden(entry, partByAssemblyId);
				if (!TryPickKeyColor(hashSet, forbidden, out var picked) && !TryPickKeyColorRelaxed(hashSet, out picked))
				{
					Log.LogError("[RingPaletteLockKeyColors] ", "钥匙色分配失败 assembly={0}，使用 Blue", new object[1] { entry.keyAssemblyName });
					picked = PaletteLockKeyColor.Blue;
				}
				hashSet.Add(picked);
				keyColorByAssemblyId[num] = picked;
			}
		}
	}

	public bool TryGetKeyColor(RingObject keyRing, out PaletteLockKeyColor color)
	{
		color = PaletteLockKeyColor.Blue;
		if ((Object)(object)keyRing == (Object)null)
		{
			return false;
		}
		int id = keyRing.PartData.Id;
		if (id > 0)
		{
			return keyColorByAssemblyId.TryGetValue(id, out color);
		}
		return false;
	}

	public bool TryGetLockHeartColor(RingObject lockedRing, out PaletteLockKeyColor color)
	{
		color = PaletteLockKeyColor.Blue;
		if ((Object)(object)lockedRing == (Object)null)
		{
			return false;
		}
		int id = lockedRing.PartData.Id;
		if (id <= 0)
		{
			return false;
		}
		if (!TryFindKeyAssemblyIdForLockedAssembly(id, out var keyAssemblyId))
		{
			return false;
		}
		return keyColorByAssemblyId.TryGetValue(keyAssemblyId, out color);
	}

	private bool TryFindKeyAssemblyIdForLockedAssembly(int lockedAssemblyId, out int keyAssemblyId)
	{
		keyAssemblyId = 0;
		if (cachedBindings == null)
		{
			return false;
		}
		for (int i = 0; i < cachedBindings.Count; i++)
		{
			PaletteLockKeyBindingEntry paletteLockKeyBindingEntry = cachedBindings[i];
			string[] lockedAssemblyNames = paletteLockKeyBindingEntry.lockedAssemblyNames;
			if (lockedAssemblyNames == null)
			{
				continue;
			}
			for (int j = 0; j < lockedAssemblyNames.Length; j++)
			{
				if (EditablePartDraft.ParseAssemblyId(lockedAssemblyNames[j]) == lockedAssemblyId)
				{
					keyAssemblyId = EditablePartDraft.ParseAssemblyId(paletteLockKeyBindingEntry.keyAssemblyName);
					return keyAssemblyId > 0;
				}
			}
		}
		return false;
	}

	private int CompareBindingsByKeyAssemblyId(PaletteLockKeyBindingEntry a, PaletteLockKeyBindingEntry b)
	{
		int num = EditablePartDraft.ParseAssemblyId(a.keyAssemblyName);
		int value = EditablePartDraft.ParseAssemblyId(b.keyAssemblyName);
		return num.CompareTo(value);
	}

	private HashSet<PaletteLockKeyColor> BuildBindingForbidden(PaletteLockKeyBindingEntry entry, IReadOnlyDictionary<int, GameplayPartData> partByAssemblyId)
	{
		HashSet<PaletteLockKeyColor> hashSet = new HashSet<PaletteLockKeyColor>();
		TryAddForbiddenForAssembly(entry.keyAssemblyName, partByAssemblyId, hashSet);
		string[] lockedAssemblyNames = entry.lockedAssemblyNames;
		if (lockedAssemblyNames == null)
		{
			return hashSet;
		}
		for (int i = 0; i < lockedAssemblyNames.Length; i++)
		{
			TryAddForbiddenForAssembly(lockedAssemblyNames[i], partByAssemblyId, hashSet);
		}
		return hashSet;
	}

	private bool TryPickKeyColor(HashSet<PaletteLockKeyColor> used, HashSet<PaletteLockKeyColor> forbidden, out PaletteLockKeyColor picked)
	{
		for (int i = 0; i < KeyColorTryOrder.Length; i++)
		{
			PaletteLockKeyColor paletteLockKeyColor = KeyColorTryOrder[i];
			if (!used.Contains(paletteLockKeyColor) && !forbidden.Contains(paletteLockKeyColor))
			{
				picked = paletteLockKeyColor;
				return true;
			}
		}
		picked = PaletteLockKeyColor.Blue;
		return false;
	}

	private bool TryPickKeyColorRelaxed(HashSet<PaletteLockKeyColor> used, out PaletteLockKeyColor picked)
	{
		for (int i = 0; i < KeyColorTryOrder.Length; i++)
		{
			PaletteLockKeyColor paletteLockKeyColor = KeyColorTryOrder[i];
			if (!used.Contains(paletteLockKeyColor))
			{
				picked = paletteLockKeyColor;
				return true;
			}
		}
		picked = PaletteLockKeyColor.Blue;
		return false;
	}

	private void TryAddForbiddenForAssembly(string assemblyName, IReadOnlyDictionary<int, GameplayPartData> partByAssemblyId, HashSet<PaletteLockKeyColor> forbidden)
	{
		int num = EditablePartDraft.ParseAssemblyId(assemblyName);
		if (num > 0 && partByAssemblyId != null && partByAssemblyId.TryGetValue(num, out var value))
		{
			AddForbiddenFromPartData(value, forbidden);
		}
	}

	private void AddForbiddenFromPartData(GameplayPartData partData, HashSet<PaletteLockKeyColor> forbidden)
	{
		if (forbidden != null && TryMapPaletteColorToKeyColor(partData.PaletteColor, out var keyColor))
		{
			forbidden.Add(keyColor);
		}
	}

	private static bool TryMapPaletteColorToKeyColor(PartPaletteColor paletteColor, out PaletteLockKeyColor keyColor)
	{
		switch (paletteColor)
		{
		case PartPaletteColor.LightBlue:
		case PartPaletteColor.DarkBlue:
			keyColor = PaletteLockKeyColor.Blue;
			return true;
		case PartPaletteColor.Green:
		case PartPaletteColor.DarkGreen:
			keyColor = PaletteLockKeyColor.Green;
			return true;
		case PartPaletteColor.Red:
			keyColor = PaletteLockKeyColor.Red;
			return true;
		case PartPaletteColor.LightYellow:
			keyColor = PaletteLockKeyColor.Yellow;
			return true;
		default:
			keyColor = PaletteLockKeyColor.Blue;
			return false;
		}
	}
}
