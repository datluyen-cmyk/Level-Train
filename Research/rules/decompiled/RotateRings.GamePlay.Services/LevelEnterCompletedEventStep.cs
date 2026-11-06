using System.Threading;
using Cysharp.Threading.Tasks;
using RotateRings.GamePlay.Events;
using RotateRings.Model;

namespace RotateRings.GamePlay.Services;

internal sealed class LevelEnterCompletedEventStep : ILevelEnterPipelineStep
{
	public int Priority => 250;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		int num = ctx.Level;
		if (num <= 0)
		{
			num = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0;
		}
		GameApp.Event.Send<LevelEnterCompletedEvent>(new LevelEnterCompletedEvent
		{
			Level = num
		});
		return UniTask.CompletedTask;
	}
}
