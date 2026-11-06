using System.Threading;
using Cysharp.Threading.Tasks;

namespace RotateRings.GamePlay.Services;

public interface ILevelEnterPipelineStep
{
	int Priority { get; }

	bool IncludeInRunPipeline => true;

	bool AwaitCompletion => true;

	bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		return !ctx.Aborted;
	}

	UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct);
}
