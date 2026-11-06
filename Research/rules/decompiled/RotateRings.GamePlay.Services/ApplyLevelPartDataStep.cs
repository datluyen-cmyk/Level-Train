using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.Model;

namespace RotateRings.GamePlay.Services;

internal sealed class ApplyLevelPartDataStep : ILevelEnterPipelineStep
{
	public int Priority => 146;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		manager.FillPartData?.FillData(ctx.LevelInfo);
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		manager.RingSave.ReapplySavedLockCounts(level);
		manager.FillPartData?.AdjustPaletteLockCurrentForSolvedRings(ctx.LevelInfo);
		manager.LockChain?.SetupChainOnLevelLoaded();
		return UniTask.CompletedTask;
	}
}
