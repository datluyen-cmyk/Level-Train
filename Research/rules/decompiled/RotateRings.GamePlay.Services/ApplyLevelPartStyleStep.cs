using System.Threading;
using Cysharp.Threading.Tasks;
using Hot.Model;
using RotateRings.GamePlay.PartStyle;
using RotateRings.Model;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class ApplyLevelPartStyleStep : ILevelEnterPipelineStep
{
	public int Priority => 81;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ctx?.Manager == (Object)null)
		{
			return UniTask.CompletedTask;
		}
		GameplayManager manager = ctx.Manager;
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		int savedLevelPartStyle = ResolveSavedPartStyle(manager, currentGameMode, ctx.Level, ctx.IsNewGame);
		bool isModelLevel = (Object)(object)ctx.LevelInfo != (Object)null && ctx.LevelInfo.isModelLevel;
		PartStyleResolveContext context = new PartStyleResolveContext(ctx.Level, currentGameMode, isModelLevel, ctx.IsNewGame, savedLevelPartStyle);
		manager.PartStyle?.InitializeLevelStyle(in context);
		if (manager.PartStyle != null)
		{
			PartStyleResumeStore.CommitAfterEnter(currentGameMode, ctx.Level, manager.PartStyle.CurrentLevelStyle);
		}
		return UniTask.CompletedTask;
	}

	private static int ResolveSavedPartStyle(GameplayManager mgr, GameMode mode, int level, bool isNewGame)
	{
		bool hasValidSave = !isNewGame && mgr?.RingSave != null && mgr.RingSave.HasValidSave(level);
		if (!PartStyleResumeStore.TryGetForEnter(mode, level, isNewGame, hasValidSave, out var styleRaw))
		{
			return -1;
		}
		return styleRaw;
	}
}
