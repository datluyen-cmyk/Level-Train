namespace RotateRings.GamePlay.Services;

public interface ISolvableSolver
{
	bool CanSolve(IGameplayObject obj);

	void Solve(IGameplayObject obj, BombRelative sourceBomb = null, GameplaySolveReasonContext releaseContext = default(GameplaySolveReasonContext));
}
