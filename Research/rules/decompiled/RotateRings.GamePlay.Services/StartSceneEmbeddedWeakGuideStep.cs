using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class StartSceneEmbeddedWeakGuideStep : ILevelEnterPipelineStep
{
	public int Priority => 255;

	public bool IncludeInRunPipeline => false;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		GameLevelInfo gameLevelInfo = ctx.LevelInfo;
		if ((Object)(object)gameLevelInfo == (Object)null && (Object)(object)ctx.Manager?.CurrentLevelObject != (Object)null)
		{
			gameLevelInfo = ctx.Manager.CurrentLevelObject.GetComponent<GameLevelInfo>();
		}
		if ((Object)(object)gameLevelInfo?.weakGuideManager != (Object)null)
		{
			gameLevelInfo.weakGuideManager.StartGuide();
		}
		return UniTask.CompletedTask;
	}
}
