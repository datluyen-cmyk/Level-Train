using System;
using System.Collections.Generic;
using RotateRing.GamePlay;
using RotateRings.GamePlay.PartStyle;
using RotateRings.GamePlay.Services;
using RotateRings.Model;
using RotateRings.StartEntry;
using UnityEngine;
using XingYun.Proxy;

namespace RotateRings.GamePlay.Save;

public class RingLevelSaveProxy : BaseProxy
{
	private const string Tag = "[RingLevelSaveProxy]";

	private readonly RotateRingCaptureSaveBuilder captureSaveBuilder = new RotateRingCaptureSaveBuilder();

	private readonly RotateRingRestoreBuilder restoreBuilder = new RotateRingRestoreBuilder();

	private const float BoardCompareEpsilonAngle = 0.05f;

	private const float BoardCompareEpsilonSqr = 0.0004f;

	public RotateRingInLevelSaveData Data { get; }

	public RingLevelFlipSaveData FlipData { get; }

	public RingLevelSaveProxy()
		: base("RingLevelSaveProxy")
	{
		Data = GameApp.Save.GetOrCreate<RotateRingInLevelSaveData>();
		FlipData = GameApp.Save.GetOrCreate<RingLevelFlipSaveData>();
	}

	public bool TryGetDecision(int level, out int flipMode)
	{
		flipMode = -1;
		if (FlipData == null || FlipData.LevelNumber != level || FlipData.FlipMode == -1)
		{
			return false;
		}
		flipMode = FlipData.FlipMode;
		return true;
	}

	public void SetDecision(int level, int flipMode)
	{
		if (FlipData != null)
		{
			FlipData.LevelNumber = level;
			FlipData.FlipMode = flipMode;
		}
	}

	public bool HasValidSave(int level)
	{
		RotateRingInLevelSaveData save;
		return TryGetSave(level, out save, validateContentHash: false);
	}

	public bool TryGetSave(int level, out RotateRingInLevelSaveData save, bool validateContentHash = true)
	{
		save = Data;
		if (save == null || !save.HasData)
		{
			save = null;
			return false;
		}
		if (save.Version != 4)
		{
			save = null;
			return false;
		}
		if (save.LevelNumber != level)
		{
			save = null;
			return false;
		}
		int levelId = LevelConfigService.GetLevelId(level);
		if (save.LevelSlotId != levelId)
		{
			save = null;
			return false;
		}
		if (save.Parts == null || save.Parts.Length == 0)
		{
			save = null;
			return false;
		}
		if (validateContentHash)
		{
			GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
			RingLevelConfigService ringLevelConfigService = instance?.LevelConfig;
			if (ringLevelConfigService != null && ringLevelConfigService.IsNewLevelWithoutContentHash(level))
			{
				save = null;
				return false;
			}
			if (!string.IsNullOrEmpty(save.CoreLevelJsonHash))
			{
				string levelJsonHash = GetLevelJsonHash(instance, level);
				if (!string.IsNullOrEmpty(levelJsonHash) && !string.Equals(save.CoreLevelJsonHash, levelJsonHash, StringComparison.Ordinal))
				{
					save = null;
					return false;
				}
			}
		}
		return true;
	}

	public void ClearSave(bool writeToDisk)
	{
		if (Data != null)
		{
			Data.HasData = false;
			Data.Parts = Array.Empty<RotateRingPartSave>();
			Data.Bombs = Array.Empty<RotateRingBombSave>();
			Data.AcornPartIds = Array.Empty<string>();
			Data.CoreLevelJsonHash = string.Empty;
			Data.StepLimitRemainingSteps = -1;
			StartEntryColdLaunch.ClearLevelExitRecord();
			PartStyleResumeStore.ClearActiveResume();
		}
	}

	public void SaveCurrent(GameplayManager gameplayManager, bool writeToDisk)
	{
		if (!((Object)(object)gameplayManager == (Object)null) && !gameplayManager.IsLevelCompleted && Data != null)
		{
			int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
			if (num > 0)
			{
				gameplayManager.RingSave.PrepareForCapture();
				CaptureToSave(gameplayManager, num, Data);
				StartEntryColdLaunch.RecordLevelExitNow();
			}
		}
	}

	public void CaptureToSave(GameplayManager gameplayManager, int level, RotateRingInLevelSaveData save)
	{
		if (save != null && !((Object)(object)gameplayManager == (Object)null))
		{
			RotateRingCaptureContext ctx = new RotateRingCaptureContext
			{
				Save = save,
				GameplayManager = gameplayManager,
				Level = level,
				CoreLevelJsonHash = GetLevelJsonHash(gameplayManager, level)
			};
			captureSaveBuilder.Build(ctx);
		}
	}

	private string GetLevelJsonHash(GameplayManager gameplayManager, int level)
	{
		if (level <= 0)
		{
			return string.Empty;
		}
		return ((gameplayManager ?? SingletonComponent<GameplayManager>.Instance)?.LevelConfig)?.GetCoreLevelJsonHash(level) ?? string.Empty;
	}

	public bool RestoreFromSave(GameplayManager gameplayManager, RotateRingInLevelSaveData save)
	{
		if ((Object)(object)gameplayManager == (Object)null || save == null)
		{
			return false;
		}
		RotateRingRestoreContext rotateRingRestoreContext = new RotateRingRestoreContext
		{
			Save = save,
			GameplayManager = gameplayManager
		};
		restoreBuilder.Build(rotateRingRestoreContext);
		return rotateRingRestoreContext.IsValid;
	}

	public RotateRingPartSave[] CaptureBoardSnapshot(GameplayManager gameplayManager)
	{
		if ((Object)(object)gameplayManager == (Object)null || (Object)(object)gameplayManager.CurrentLevelObject == (Object)null)
		{
			return Array.Empty<RotateRingPartSave>();
		}
		int num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		if (num <= 0)
		{
			return Array.Empty<RotateRingPartSave>();
		}
		RotateRingInLevelSaveData rotateRingInLevelSaveData = new RotateRingInLevelSaveData();
		CaptureToSave(gameplayManager, num, rotateRingInLevelSaveData);
		return rotateRingInLevelSaveData.Parts ?? Array.Empty<RotateRingPartSave>();
	}

	public bool HasBoardChanged(GameplayManager gameplayManager, RotateRingPartSave[] baseline)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)gameplayManager == (Object)null || baseline == null || baseline.Length == 0)
		{
			return false;
		}
		IReadOnlyList<IGameplayObject> gameplayObjects = gameplayManager.GetGameplayObjects();
		if (gameplayObjects == null)
		{
			return true;
		}
		foreach (RotateRingPartSave rotateRingPartSave in baseline)
		{
			if (rotateRingPartSave == null || string.IsNullOrEmpty(rotateRingPartSave.PartId))
			{
				continue;
			}
			if (TryFindLiveObject(gameplayObjects, rotateRingPartSave.PartId, out var obj))
			{
				if (obj.IsSolved != rotateRingPartSave.IsSolved)
				{
					return true;
				}
				if (rotateRingPartSave.IsSolved)
				{
					continue;
				}
				if (!(obj is RingObject ringObject))
				{
					if (obj is PullObject pullObject && PositionChangedSquared(rotateRingPartSave, ((Component)pullObject).transform.position) > 0.0004f)
					{
						return true;
					}
				}
				else if (Mathf.Abs(Mathf.DeltaAngle(rotateRingPartSave.RotationZ, ((Component)ringObject).transform.localEulerAngles.z)) > 0.05f)
				{
					return true;
				}
			}
			else if (!rotateRingPartSave.IsSolved)
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryFindLiveObject(IReadOnlyList<IGameplayObject> objects, string partId, out IGameplayObject obj)
	{
		obj = null;
		if (objects == null || string.IsNullOrEmpty(partId))
		{
			return false;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			IGameplayObject gameplayObject = objects[i];
			MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val != null && !((Object)(object)val == (Object)null) && !(gameplayObject.PartData.PhysicPartId != partId) && (gameplayObject is RingObject || gameplayObject is PullObject || gameplayObject is FixedObject))
			{
				obj = gameplayObject;
				return true;
			}
		}
		return false;
	}

	private static float PositionChangedSquared(RotateRingPartSave a, Vector3 b)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		float num = a.PosX - b.x;
		float num2 = a.PosY - b.y;
		return num * num + num2 * num2;
	}
}
