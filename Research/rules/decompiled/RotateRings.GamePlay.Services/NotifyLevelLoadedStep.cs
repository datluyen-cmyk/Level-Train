using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.DataTrack;
using RotateRings.SpecialLevel;
using RotateRings.UI.Game;
using RotateRings.WeakGuide;
using XingYun.Core.Singletons;
using XingYun.Event;

namespace RotateRings.GamePlay.Services;

internal sealed class NotifyLevelLoadedStep : ILevelEnterPipelineStep
{
	private const string LogTag = "[NotifyLevelLoadedStep] ";

	public int Priority => 200;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		manager.GetGameService<UIWeakGuideService>()?.OnLevelInit(ctx.Level);
		SpecialLevelModule.IsSpecialLevelMode();
		SpecialLevelModule.CommitInsertOnLevelLoaded();
		IReadOnlyList<IGameplayObject> collectableObjects = manager.ObjectsService.CollectableObjects;
		Singleton<EventManager>.Instance.Send<LevelChanged>(new LevelChanged
		{
			Level = ctx.Level,
			IsNewGame = ctx.IsNewGame,
			CollectableCount = collectableObjects.Count
		});
		RRDataTrack.Instance.ReportGameOpen();
		return UniTask.CompletedTask;
	}
}
