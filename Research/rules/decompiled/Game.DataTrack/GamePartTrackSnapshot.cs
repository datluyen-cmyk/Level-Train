using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Services;
using UnlockRing.Core.Domain;

namespace Game.DataTrack;

public struct GamePartTrackSnapshot
{
	public int AssemblyNumStart;

	public int AssemblyNumEnd;

	public int RingNum;

	public int StickNum;

	public int BombNum;

	public int[] LevelElementArray;

	[Obsolete("使用 LevelElementArray")]
	public string LevelElement
	{
		get
		{
			if (LevelElementArray != null && LevelElementArray.Length != 0)
			{
				return string.Join(",", LevelElementArray);
			}
			return string.Empty;
		}
	}

	public static GamePartTrackSnapshot FromLegacyServices(RingObjectsService services, bool hasLockElement)
	{
		if (services == null)
		{
			return default(GamePartTrackSnapshot);
		}
		services.CountAssemblyStats(includeSolved: false, out var ringNum, out var stickNum, out var bombNum, out var totalNum);
		return new GamePartTrackSnapshot
		{
			AssemblyNumStart = totalNum,
			RingNum = ringNum,
			StickNum = stickNum,
			BombNum = bombNum,
			LevelElementArray = BuildLevelElementArray(hasLockElement)
		};
	}

	public static GamePartTrackSnapshot FromLegacyRemaining(RingObjectsService services)
	{
		if (services == null)
		{
			return default(GamePartTrackSnapshot);
		}
		services.CountRemainingAssemblyStats(out var ringNum, out var stickNum, out var bombNum, out var totalNum);
		return new GamePartTrackSnapshot
		{
			AssemblyNumEnd = totalNum,
			RingNum = ringNum,
			StickNum = stickNum,
			BombNum = bombNum
		};
	}

	public static GamePartTrackSnapshot FromCoreLevel(LevelDefinition level, BoardState initialState, string puzzle)
	{
		if (level == null)
		{
			return default(GamePartTrackSnapshot);
		}
		CountCoreParts(level, initialState, includeRemovedOrReleased: false, out var ringNum, out var stickNum, out var _);
		return new GamePartTrackSnapshot
		{
			RingNum = ringNum,
			StickNum = stickNum,
			LevelElementArray = BuildCoreLevelElementArray(level, initialState)
		};
	}

	public static void CountRemainingCoreParts(LevelDefinition level, BoardState state, out int ringNum, out int stickNum, out int plugNum)
	{
		CountCoreParts(level, state, includeRemovedOrReleased: true, out ringNum, out stickNum, out plugNum);
	}

	public string[] BuildAssemblyNumArray()
	{
		return new string[3]
		{
			RingNum.ToString(),
			StickNum.ToString(),
			BombNum.ToString()
		};
	}

	private static int[] BuildCoreLevelElementArray(LevelDefinition level, BoardState initialState)
	{
		if (HasCoreLockElement(level, initialState))
		{
			return new int[1];
		}
		return Array.Empty<int>();
	}

	private static int[] BuildLevelElementArray(bool hasLockElement)
	{
		if (!hasLockElement)
		{
			return Array.Empty<int>();
		}
		return new int[1];
	}

	private static void CountCoreParts(LevelDefinition level, BoardState state, bool includeRemovedOrReleased, out int ringNum, out int stickNum, out int plugNum)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		ringNum = 0;
		stickNum = 0;
		plugNum = 0;
		for (int i = 0; i < level.Parts.Count; i++)
		{
			PartDefinition val = level.Parts[i];
			if (!includeRemovedOrReleased || state == null || !state.Parts.TryGetValue(val.Id, out var value) || (!value.Removed && !value.Released))
			{
				ClassifyCorePartType(val, out var isRing, out var isStick, out var isPlug);
				if (isRing)
				{
					ringNum++;
				}
				else if (isStick)
				{
					stickNum++;
				}
				else if (isPlug)
				{
					plugNum++;
				}
			}
		}
	}

	private static void ClassifyCorePartType(PartDefinition part, out bool isRing, out bool isStick, out bool isPlug)
	{
		isRing = false;
		isStick = false;
		isPlug = false;
		for (int i = 0; i < part.Behaviors.Count; i++)
		{
			BehaviorDefinition val = part.Behaviors[i];
			if (val is RotatableBehaviorDefinition)
			{
				isRing = true;
			}
			else if (val is AxisSlidableBehaviorDefinition)
			{
				isStick = true;
			}
			else if (val is ForceRemovableBehaviorDefinition)
			{
				isPlug = true;
			}
		}
	}

	private static bool HasCoreLockElement(LevelDefinition level, BoardState initialState)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if (initialState != null)
		{
			for (int i = 0; i < initialState.Constraints.Count; i++)
			{
				if ((int)initialState.Constraints[i].Kind == 1)
				{
					return true;
				}
			}
		}
		for (int j = 0; j < level.Parts.Count; j++)
		{
			IReadOnlyList<BehaviorDefinition> behaviors = level.Parts[j].Behaviors;
			for (int k = 0; k < behaviors.Count; k++)
			{
				if (behaviors[k] is CatchBehaviorDefinition)
				{
					return true;
				}
			}
		}
		return false;
	}
}
