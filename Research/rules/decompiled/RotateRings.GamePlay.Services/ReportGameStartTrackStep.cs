using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.DataTrack;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class ReportGameStartTrackStep : ILevelEnterPipelineStep
{
	public int Priority => 270;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		GamePartTrackSnapshot partSnapshot = GamePartTrackSnapshot.FromLegacyServices(manager.ObjectsService, HasLegacyLockElement(ctx.LevelInfo));
		RRDataTrack.Instance.ReportGameStart(partSnapshot, manager.IsNewGameRun);
		return UniTask.CompletedTask;
	}

	private bool HasLegacyLockElement(GameLevelInfo gameLevelInfo)
	{
		if ((Object)(object)gameLevelInfo == (Object)null)
		{
			return false;
		}
		if (gameLevelInfo.GuideLinks != null && gameLevelInfo.GuideLinks.Count > 0)
		{
			return true;
		}
		IReadOnlyList<RingLinkEntry> ringLinks = gameLevelInfo.RingLinks;
		for (int i = 0; i < ringLinks.Count; i++)
		{
			if (ringLinks[i].relationKind == 1)
			{
				return true;
			}
		}
		return false;
	}
}
