using System.Threading;
using Cysharp.Threading.Tasks;
using Hot.Model;
using RotateRing.GamePlay;
using RotateRings.Model;
using RotateRings.SpecialLevel;
using RotateRings.WeakGuide;

namespace RotateRings.GamePlay.Services;

internal sealed class PrepareLevelLoadStep : ILevelEnterPipelineStep
{
	private const string LogTag = "[PrepareLevelLoadStep] ";

	public int Priority => 10;

	public UniTask ExecuteAsync(LevelEnterPipelineContext ctx, CancellationToken ct)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		GameplayManager manager = ctx.Manager;
		int level = ctx.Level;
		level = (ctx.Level = SpecialLevelModule.ResolveEnterLevel(level));
		bool isNewGame = !manager.RingSave.HasValidSave(level);
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		levelModelProxy.ResetLevel(level, isNewGame);
		if (GameInfo.CurrentGameMode == GameMode.Main)
		{
			int mainLevelMax = LevelConfigService.MainLevelMax;
			if (mainLevelMax > 0 && level > mainLevelMax)
			{
				levelModelProxy.Data.RefreshCurrentGameMode(mainLevelMax, LevelConfigService.CurrentEndlessSeason);
			}
		}
		int levelId = LevelConfigService.GetLevelId(level);
		levelModelProxy.SyncLevel(levelId);
		manager.GetGameService<UIWeakGuideService>()?.OnLevelExit();
		ctx.IsNewGame = isNewGame;
		return UniTask.CompletedTask;
	}
}
