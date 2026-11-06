using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Common.Services;

namespace RotateRings.GamePlay.Services;

internal sealed class StartMainLevelStartPipelineStep : ILevelEnterPipelineStep
{
	public int Priority => 220;

	public bool AwaitCompletion => false;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		LevelStartPipeline.BuildAndExecute();
		return UniTask.CompletedTask;
	}
}
