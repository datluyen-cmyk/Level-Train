namespace RotateRings.GamePlay.Save;

internal interface IInLevelSaveStep<in TContext>
{
	void Execute(TContext ctx);
}
