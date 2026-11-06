using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.Model;

namespace RotateRings.GamePlay.Services;

internal sealed class ApplyLevelSaveStateStep : ILevelEnterPipelineStep
{
	public int Priority => 145;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		bool resolvedIsNewGame = ctx.IsNewGame;
		if (!manager.RingSave.TryApplyAfterLevelLoad(resolvedIsNewGame, ctx.Level, out resolvedIsNewGame))
		{
			levelModelProxy.ResetLevel(ctx.Level);
			levelModelProxy.ResetInLevelPropUseCounters();
		}
		ctx.IsNewGame = resolvedIsNewGame;
		manager.IsNewGameRun = resolvedIsNewGame;
		if (resolvedIsNewGame)
		{
			levelModelProxy.ResetCurrentScore();
			manager.RingSave.ClearSave(writeToDisk: false);
		}
		else
		{
			levelModelProxy.SyncCurrentScoreView();
		}
		manager.RingSave.OnLevelLoaded(resolvedIsNewGame);
		return UniTask.CompletedTask;
	}
}
