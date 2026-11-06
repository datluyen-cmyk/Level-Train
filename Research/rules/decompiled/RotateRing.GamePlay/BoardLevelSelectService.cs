using RotateRings.Model;

namespace RotateRing.GamePlay;

public sealed class BoardLevelSelectService : ILevelSelectService
{
	public bool IsAvailable(GameMode mode, int level)
	{
		return true;
	}

	public int SelectLevelId(GameMode mode, int level, int boardLevelId, bool isCore)
	{
		return boardLevelId;
	}
}
