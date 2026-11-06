using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Services;
using UnityEngine;

namespace RotateRings.GamePlay.Save;

public sealed class RingLevelSaveService : GameplayServiceBase<GameplayManager>
{
	private const string Tag = "[RingLevelSaveService] ";

	private RotateRingPartSave[] initialBoardBaseline;

	private bool isResumeSession;

	private bool boardDirty;

	private readonly List<RotateRingPartSave> partManifest = new List<RotateRingPartSave>();

	private readonly List<string> bombManifest = new List<string>();

	public bool IsResumeSession => isResumeSession;

	private RingLevelSaveProxy SaveProxy => GameApp.Proxy.Get<RingLevelSaveProxy>();

	public IReadOnlyList<RotateRingPartSave> PartManifest => partManifest;

	public IReadOnlyList<string> BombManifest => bombManifest;

	public RingLevelSaveService(GameplayManager host)
		: base(host)
	{
	}

	public void ClearManifest()
	{
		partManifest.Clear();
		bombManifest.Clear();
		initialBoardBaseline = null;
		isResumeSession = false;
		boardDirty = false;
	}

	public void RebuildPartManifest(IReadOnlyList<IGameplayObject> gameplayObjects)
	{
		partManifest.Clear();
		if (gameplayObjects == null)
		{
			return;
		}
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			if (gameplayObject == null || string.IsNullOrEmpty(gameplayObject.PartData.PhysicPartId))
			{
				continue;
			}
			RotateRingPartSave.PartKindType partKind;
			if (!(gameplayObject is RingObject))
			{
				if (!(gameplayObject is PullObject))
				{
					if (!(gameplayObject is FixedObject))
					{
						continue;
					}
					partKind = RotateRingPartSave.PartKindType.Fixed;
				}
				else
				{
					partKind = RotateRingPartSave.PartKindType.Pull;
				}
			}
			else
			{
				partKind = RotateRingPartSave.PartKindType.Ring;
			}
			partManifest.Add(new RotateRingPartSave
			{
				PartId = gameplayObject.PartData.PhysicPartId,
				PartKind = partKind
			});
		}
	}

	public void RebuildBombManifest(IReadOnlyList<BombRelative> bombs)
	{
		bombManifest.Clear();
		if (bombs == null)
		{
			return;
		}
		for (int i = 0; i < bombs.Count; i++)
		{
			BombRelative bombRelative = bombs[i];
			if (!((Object)(object)bombRelative == (Object)null))
			{
				string bombId = bombRelative.GetBombId();
				if (!string.IsNullOrEmpty(bombId))
				{
					bombManifest.Add(bombId);
				}
			}
		}
	}

	public void OnLevelLoaded(bool isNewGame)
	{
		isResumeSession = !isNewGame;
		initialBoardBaseline = null;
		boardDirty = false;
		if (!isResumeSession)
		{
			initialBoardBaseline = SaveProxy.CaptureBoardSnapshot(base.Host);
			if (initialBoardBaseline == null || initialBoardBaseline.Length == 0)
			{
				initialBoardBaseline = null;
			}
		}
	}

	public void ReapplySavedLockCounts(int level)
	{
		if (!isResumeSession || !SaveProxy.TryGetSave(level, out var save) || save.Parts == null || save.Parts.Length == 0)
		{
			return;
		}
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList == null)
		{
			return;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			IGameplayObject gameplayObject = readOnlyList[i];
			if (gameplayObject == null || string.IsNullOrEmpty(gameplayObject.PartData.PhysicPartId))
			{
				continue;
			}
			for (int j = 0; j < save.Parts.Length; j++)
			{
				RotateRingPartSave rotateRingPartSave = save.Parts[j];
				if (rotateRingPartSave != null && !rotateRingPartSave.IsSolved && string.Equals(rotateRingPartSave.PartId, gameplayObject.PartData.PhysicPartId, StringComparison.Ordinal))
				{
					gameplayObject.LockCount = rotateRingPartSave.LockCount;
					break;
				}
			}
		}
	}

	public bool TryApplyAfterLevelLoad(bool isNewGame, int level, out bool resolvedIsNewGame)
	{
		resolvedIsNewGame = isNewGame;
		if (isNewGame)
		{
			return true;
		}
		if (!SaveProxy.TryGetSave(level, out var save))
		{
			resolvedIsNewGame = true;
			SaveProxy.ClearSave(writeToDisk: false);
			return false;
		}
		if (!SaveProxy.RestoreFromSave(base.Host, save))
		{
			resolvedIsNewGame = true;
			SaveProxy.ClearSave(writeToDisk: false);
			return false;
		}
		base.Host.PurgeDestroyedGameplayObjects();
		return true;
	}

	public bool HasValidSave(int level)
	{
		return SaveProxy.HasValidSave(level);
	}

	public void ClearSave(bool writeToDisk)
	{
		SaveProxy.ClearSave(writeToDisk);
	}

	public void PrepareForCapture()
	{
		base.Host.PurgeDestroyedGameplayObjects();
	}

	public void TryFlushToDisk()
	{
		if (base.Host.IsLevelCompleted || (Object)(object)base.Host.CurrentLevelObject == (Object)null)
		{
			return;
		}
		if (isResumeSession)
		{
			SaveProxy.SaveCurrent(base.Host, writeToDisk: false);
		}
		else if (initialBoardBaseline != null)
		{
			if (boardDirty)
			{
				SaveProxy.SaveCurrent(base.Host, writeToDisk: false);
			}
			else if (SaveProxy.HasBoardChanged(base.Host, initialBoardBaseline))
			{
				boardDirty = true;
				SaveProxy.SaveCurrent(base.Host, writeToDisk: false);
			}
		}
	}

	public void TrySaveInProgress(bool writeToDisk = false)
	{
		if (base.Host.IsLevelCompleted || (Object)(object)base.Host.CurrentLevelObject == (Object)null)
		{
			return;
		}
		if (isResumeSession)
		{
			SaveProxy.SaveCurrent(base.Host, writeToDisk);
		}
		else if (initialBoardBaseline != null)
		{
			if (boardDirty)
			{
				SaveProxy.SaveCurrent(base.Host, writeToDisk);
			}
			else if (SaveProxy.HasBoardChanged(base.Host, initialBoardBaseline))
			{
				boardDirty = true;
				SaveProxy.SaveCurrent(base.Host, writeToDisk);
			}
		}
	}
}
