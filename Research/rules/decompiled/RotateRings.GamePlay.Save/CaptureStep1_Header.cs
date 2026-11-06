using RotateRing.GamePlay;

namespace RotateRings.GamePlay.Save;

internal sealed class CaptureStep1_Header : IInLevelSaveStep<RotateRingCaptureContext>
{
	public void Execute(RotateRingCaptureContext ctx)
	{
		RotateRingInLevelSaveData save = ctx.Save;
		save.Version = 4;
		save.LevelNumber = ctx.Level;
		save.LevelSlotId = LevelConfigService.GetLevelId(ctx.Level);
		save.CoreLevelJsonHash = ctx.CoreLevelJsonHash ?? string.Empty;
		save.HasData = true;
	}
}
