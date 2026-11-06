using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hot.Model;
using RotateRings.GamePlay.Save;
using RotateRings.Model;
using UnityEngine;
using XingYun.Experiment;

namespace RotateRings.GamePlay.Services;

internal sealed class ApplyBoardFlipStep : ILevelEnterPipelineStep
{
	private const string Tag = "[ApplyBoardFlip] ";

	private const float FlipRotationCompensateDeg = 180f;

	private const int SchemaAlwaysFlip = 8;

	private const int SchemaRandomFlip = 9;

	private const bool UseLevelRootFlip = true;

	public int Priority => 85;

	public bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		if (!ctx.Aborted)
		{
			return ctx.Manager.HasActiveLevelRoot;
		}
		return false;
	}

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		GameLevelInfo levelInfo = ctx.LevelInfo;
		if (GameInfo.CurrentGameMode != GameMode.Endless || (Object)(object)levelInfo == (Object)null || !levelInfo.isUseCoreLevel)
		{
			manager.CurrentLevelMapFlipMode = LevelMapFlipMode.None;
			return UniTask.CompletedTask;
		}
		RingLevelSaveProxy ringLevelSaveProxy = GameApp.Proxy.Get<RingLevelSaveProxy>();
		if (!ringLevelSaveProxy.TryGetDecision(ctx.Level, out var flipMode))
		{
			if (ringLevelSaveProxy.Data.HasData && ringLevelSaveProxy.Data.LevelNumber == ctx.Level)
			{
				flipMode = 0;
				ringLevelSaveProxy.SetDecision(ctx.Level, flipMode);
			}
			else
			{
				flipMode = (int)RollFlipModeBySchema();
				ringLevelSaveProxy.SetDecision(ctx.Level, flipMode);
			}
		}
		if ((manager.CurrentLevelMapFlipMode = (LevelMapFlipMode)flipMode) == LevelMapFlipMode.None)
		{
			return UniTask.CompletedTask;
		}
		Rect val = BoardRenderUtils.WorldRectFromAabb(levelInfo.Bounds, 0f, manager.CoordScale);
		Vector3 center = Vector2.op_Implicit(((Rect)(ref val)).center);
		ApplyFlipByLevelRootRotation(ctx.LevelRoot, center);
		return UniTask.CompletedTask;
	}

	private static LevelMapFlipMode RollFlipModeBySchema()
	{
		switch (FunctionDataCenter.Get<GameBord>().EndlessLevelFlipSchema)
		{
		case 8:
			return LevelMapFlipMode.FlipXY;
		case 9:
			if (!(Random.value < 0.5f))
			{
				return LevelMapFlipMode.None;
			}
			return LevelMapFlipMode.FlipXY;
		default:
			return LevelMapFlipMode.None;
		}
	}

	internal static void ApplyFlipToGameplayObjects(GameplayManager mgr, Vector3 center)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<IGameplayObject> gameplayObjects = mgr.ObjectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val == null || (Object)(object)val == (Object)null)
			{
				continue;
			}
			Transform transform = ((Component)val).transform;
			Vector3 position = transform.position;
			position.x = 2f * center.x - position.x;
			position.y = 2f * center.y - position.y;
			transform.position = position;
			float num = Mathf.Repeat(transform.localEulerAngles.z + 180f, 360f);
			if (!(gameplayObject is RingObject ringObject))
			{
				if (!(gameplayObject is PullObject pullObject))
				{
					if (gameplayObject is FixedObject)
					{
						SetLocalRotationZ(transform, num);
					}
				}
				else
				{
					pullObject.ApplySavedPosition(transform.position);
					SetLocalRotationZ(transform, num);
				}
			}
			else
			{
				ringObject.ApplySavedRotation(num);
				ringObject.RecaptureInitialAngleZ();
			}
		}
		IReadOnlyList<BombRelative> bombs = mgr.ObjectsService.Bombs;
		for (int j = 0; j < bombs.Count; j++)
		{
			BombRelative bombRelative = bombs[j];
			if (!((Object)(object)bombRelative == (Object)null))
			{
				if (bombRelative.IsStandalone)
				{
					Vector3 position2 = ((Component)bombRelative).transform.position;
					position2.x = 2f * center.x - position2.x;
					position2.y = 2f * center.y - position2.y;
					((Component)bombRelative).transform.position = position2;
				}
				float z = Mathf.Repeat(((Component)bombRelative).transform.localEulerAngles.z + 180f, 360f);
				SetLocalRotationZ(((Component)bombRelative).transform, z);
			}
		}
	}

	internal static void ApplyFlipByLevelRootRotation(GameObject levelRoot, Vector3 center)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)levelRoot == (Object)null))
		{
			Transform transform = levelRoot.transform;
			Vector3 val = transform.position - center;
			transform.position = center - val;
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.z = Mathf.Repeat(localEulerAngles.z + 180f, 360f);
			transform.localEulerAngles = localEulerAngles;
			Physics2D.SyncTransforms();
		}
	}

	private static Vector3 ComputePartsCenter(GameplayManager mgr)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		IReadOnlyList<IGameplayObject> gameplayObjects = mgr.ObjectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			MonoBehaviour val3 = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val3 != null && !((Object)(object)val3 == (Object)null))
			{
				Vector3 position = ((Component)val3).transform.position;
				if (!flag)
				{
					val = (val2 = position);
					flag = true;
				}
				else
				{
					val = Vector3.Min(val, position);
					val2 = Vector3.Max(val2, position);
				}
			}
		}
		IReadOnlyList<BombRelative> bombs = mgr.ObjectsService.Bombs;
		for (int j = 0; j < bombs.Count; j++)
		{
			if (!((Object)(object)bombs[j] == (Object)null))
			{
				Vector3 position2 = ((Component)bombs[j]).transform.position;
				if (!flag)
				{
					val = (val2 = position2);
					flag = true;
				}
				else
				{
					val = Vector3.Min(val, position2);
					val2 = Vector3.Max(val2, position2);
				}
			}
		}
		if (!flag)
		{
			return Vector3.zero;
		}
		return (val + val2) * 0.5f;
	}

	private static void SetLocalRotationZ(Transform t, float z)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = t.localEulerAngles;
		localEulerAngles.z = z;
		t.localEulerAngles = localEulerAngles;
		Physics2D.SyncTransforms();
	}
}
