using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class ShowLevelObjectStep : ILevelEnterPipelineStep
{
	public int Priority => 230;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		GameObject currentLevelObject = ctx.Manager.CurrentLevelObject;
		if ((Object)(object)currentLevelObject != (Object)null && !currentLevelObject.activeSelf)
		{
			currentLevelObject.SetActive(true);
		}
		return UniTask.CompletedTask;
	}
}
