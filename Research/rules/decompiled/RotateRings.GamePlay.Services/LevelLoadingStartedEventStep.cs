using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;

namespace RotateRings.GamePlay.Services;

internal sealed class LevelLoadingStartedEventStep : ILevelEnterPipelineStep
{
	public int Priority => 5;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		GameApp.Event.Send<LevelLoadingStartedEvent>(new LevelLoadingStartedEvent
		{
			Level = ctx.Level
		});
		return UniTask.CompletedTask;
	}
}
