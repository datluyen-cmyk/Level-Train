using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hot.Model;
using RotateRing.GamePlay;
using RotateRings.Common;
using RotateRings.GamePlay.Save;
using RotateRings.Model;

namespace RotateRings.GamePlay.Services.LevelStepLimit;

internal sealed class LevelStepLimitInitStep : ILevelEnterPipelineStep
{
	public int Priority => 25;

	public bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		if (ctx.Aborted)
		{
			return false;
		}
		LevelStepLimitConfigSnapshot levelStepLimitConfigSnapshot = LevelStepLimitConfigResolver.Resolve(GetCurrentLevelKey());
		if (!levelStepLimitConfigSnapshot.Enable)
		{
			return false;
		}
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		return levelStepLimitConfigSnapshot.LevelMode switch
		{
			LevelStepLimitLevelMode.All => currentGameMode != GameMode.Main || ctx.Level > 1, 
			LevelStepLimitLevelMode.Main => currentGameMode == GameMode.Main, 
			LevelStepLimitLevelMode.Endless => currentGameMode == GameMode.Endless, 
			LevelStepLimitLevelMode.HardLevel => LevelDifficultyUtil.IsHardLevel(currentGameMode, ctx.Level), 
			_ => false, 
		};
	}

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		LevelStepLimitConfigSnapshot snapshot = LevelStepLimitConfigResolver.Resolve(GetCurrentLevelKey());
		if (!snapshot.Enable)
		{
			return UniTask.CompletedTask;
		}
		LevelStepLimitService levelStepLimitService = ctx.Manager?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService == null)
		{
			return UniTask.CompletedTask;
		}
		int remainingStepsFromSave = -1;
		if (!ctx.IsNewGame)
		{
			RingLevelSaveProxy ringLevelSaveProxy = GameApp.Proxy.Get<RingLevelSaveProxy>();
			if (ringLevelSaveProxy != null && ringLevelSaveProxy.TryGetSave(ctx.Level, out var save, validateContentHash: false) && save.StepLimitRemainingSteps >= 0)
			{
				remainingStepsFromSave = save.StepLimitRemainingSteps;
			}
		}
		levelStepLimitService.Initialize(snapshot, remainingStepsFromSave);
		return UniTask.CompletedTask;
	}

	private static int GetCurrentLevelKey()
	{
		try
		{
			return LevelConfigService.GetCoreLevelPathKey(GameInfo.LevelId);
		}
		catch (Exception)
		{
			return 0;
		}
	}
}
