using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class RegisterLevelInteractablesStep : ILevelEnterPipelineStep
{
	public int Priority => 140;

	public bool ShouldRun(LevelEnterPipelineContext ctx)
	{
		if (!ctx.Aborted)
		{
			return ctx.Manager.HasActiveLevelRoot;
		}
		return false;
	}

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager mgr = ctx.Manager;
		MonoBehaviour[] componentsInChildren = mgr.CurrentLevelObject.GetComponentsInChildren<MonoBehaviour>(true);
		foreach (MonoBehaviour val in componentsInChildren)
		{
			if ((Object)(object)val == (Object)null)
			{
				continue;
			}
			IGameplayObject gameplayObject = val as IGameplayObject;
			if (!(val is IInteractableObject interactableObject))
			{
				continue;
			}
			if (gameplayObject != null)
			{
				interactableObject.RegisterOnPointerDown(delegate
				{
					mgr.OnInteractionStart(gameplayObject);
				});
			}
			else
			{
				interactableObject.RegisterOnPointerDown(mgr.OnInteractionStart);
			}
			RingObject ringObject = val as RingObject;
			if (ringObject != null)
			{
				interactableObject.RegisterOnPointerUp(delegate
				{
					mgr.OnRingPointerUp(ringObject);
				});
			}
			else if (gameplayObject != null)
			{
				interactableObject.RegisterOnPointerUp(delegate
				{
					mgr.OnPlayerAction(gameplayObject);
				});
			}
			else
			{
				interactableObject.RegisterOnPointerUp(mgr.OnPlayerAction);
			}
		}
		return UniTask.CompletedTask;
	}
}
