using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Common.Services;

namespace RotateRings.GamePlay.Services;

internal sealed class ResetLevelStartPipelineStateStep : ILevelEnterPipelineStep
{
	public int Priority => 50;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		LevelStartPipeline.Reset();
		ActivityModuleInit.EnsureInitialized();
		HardLevelToastPreEnterService.ResetState();
		return UniTask.CompletedTask;
	}
}
