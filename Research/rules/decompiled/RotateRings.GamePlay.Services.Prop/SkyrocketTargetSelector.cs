using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services.Prop;

public static class SkyrocketTargetSelector
{
	private const string Tag = "[SkyrocketTargetSelector] ";

	public static IGameplayObject Select(IReadOnlyList<IGameplayObject> candidates, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		if (candidates == null || candidates.Count == 0)
		{
			return null;
		}
		if (GetTargetChooseMode() != 1)
		{
			return SelectDefault(candidates, reservedTargets, cascadeExcluded);
		}
		return SelectByPriority(candidates, reservedTargets, cascadeExcluded);
	}

	public static bool HasSelectable(IReadOnlyList<IGameplayObject> candidates, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		if (candidates == null || candidates.Count == 0)
		{
			return false;
		}
		if (GetTargetChooseMode() != 1)
		{
			return HasSelectableDefault(candidates, reservedTargets, cascadeExcluded);
		}
		for (int i = 0; i < candidates.Count; i++)
		{
			if (GetPriorityCategory(candidates[i], reservedTargets, cascadeExcluded) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static int GetTargetChooseMode()
	{
		return FunctionDataCenter.Get<PropFunction>().SkyrocketTargetChoose;
	}

	public static int GetPriorityCategory(IGameplayObject candidate, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		if (!IsBaseSelectable(candidate, reservedTargets, cascadeExcluded))
		{
			return 0;
		}
		if (candidate.PartData.RandomBombCanSelect)
		{
			return 1;
		}
		return GetNonRandomBombPriorityCategory(candidate);
	}

	private static int GetNonRandomBombPriorityCategory(IGameplayObject candidate)
	{
		bool flag = HasChainLock(candidate);
		bool flag2 = HasPaletteLockKey(candidate);
		bool flag3 = HasPaletteLockSeal(candidate);
		if (flag && !flag2 && !flag3)
		{
			return 2;
		}
		if (!flag && ((flag2 && !flag3) || (flag3 && !flag2)))
		{
			return 3;
		}
		if (flag && flag2 && !flag3)
		{
			return 4;
		}
		if (flag && flag3 && !flag2)
		{
			return 5;
		}
		if (flag2 && flag3 && !flag)
		{
			return 6;
		}
		if (flag && flag2 && flag3)
		{
			return 7;
		}
		if (!flag && !flag2 && !flag3 && !candidate.PartData.HasBomb)
		{
			return 8;
		}
		if (candidate.PartData.HasBomb)
		{
			return 9;
		}
		return 0;
	}

	private static IGameplayObject SelectDefault(IReadOnlyList<IGameplayObject> candidates, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		List<IGameplayObject> list = new List<IGameplayObject>(candidates.Count);
		for (int i = 0; i < candidates.Count; i++)
		{
			IGameplayObject gameplayObject = candidates[i];
			if (IsBaseSelectable(gameplayObject, reservedTargets, cascadeExcluded) && gameplayObject.PartData.RandomBombCanSelect)
			{
				list.Add(gameplayObject);
			}
		}
		if (list.Count != 0)
		{
			return list[Random.Range(0, list.Count)];
		}
		return null;
	}

	private static bool HasSelectableDefault(IReadOnlyList<IGameplayObject> candidates, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		for (int i = 0; i < candidates.Count; i++)
		{
			IGameplayObject gameplayObject = candidates[i];
			if (IsBaseSelectable(gameplayObject, reservedTargets, cascadeExcluded) && gameplayObject.PartData.RandomBombCanSelect)
			{
				return true;
			}
		}
		return false;
	}

	private static IGameplayObject SelectByPriority(IReadOnlyList<IGameplayObject> candidates, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		int num = int.MaxValue;
		List<IGameplayObject> list = new List<IGameplayObject>(candidates.Count);
		for (int i = 0; i < candidates.Count; i++)
		{
			IGameplayObject gameplayObject = candidates[i];
			int priorityCategory = GetPriorityCategory(gameplayObject, reservedTargets, cascadeExcluded);
			if (priorityCategory != 0)
			{
				if (priorityCategory < num)
				{
					num = priorityCategory;
					list.Clear();
					list.Add(gameplayObject);
				}
				else if (priorityCategory == num)
				{
					list.Add(gameplayObject);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	private static bool IsBaseSelectable(IGameplayObject candidate, HashSet<IGameplayObject> reservedTargets, HashSet<PartId> cascadeExcluded)
	{
		if (candidate != null && !candidate.IsSolved && !reservedTargets.Contains(candidate) && !IsInCascadeExcluded(candidate, cascadeExcluded))
		{
			return IsHammerRemovable(candidate);
		}
		return false;
	}

	private static bool IsHammerRemovable(IGameplayObject obj)
	{
		if (!(obj is RingObject))
		{
			return obj is PullObject;
		}
		return true;
	}

	private static bool IsInCascadeExcluded(IGameplayObject candidate, HashSet<PartId> cascadeExcluded)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (cascadeExcluded != null && cascadeExcluded.Count > 0 && candidate != null && candidate.TryGetPartId(out var partId))
		{
			return cascadeExcluded.Contains(partId);
		}
		return false;
	}

	private static bool HasChainLock(IGameplayObject candidate)
	{
		if (candidate != null)
		{
			return candidate.LockCount > 0;
		}
		return false;
	}

	private static bool HasPaletteLockKey(IGameplayObject candidate)
	{
		return candidate?.IsCurrentPaletteLockKey ?? false;
	}

	private static bool HasPaletteLockSeal(IGameplayObject candidate)
	{
		return candidate?.IsCurrentPaletteLock ?? false;
	}
}
