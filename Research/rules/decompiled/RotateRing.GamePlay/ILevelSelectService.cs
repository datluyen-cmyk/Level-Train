using RotateRings.Model;

namespace RotateRing.GamePlay;

public interface ILevelSelectService
{
	bool IsAvailable(GameMode mode, int level);

	int SelectLevelId(GameMode mode, int level, int boardLevelId, bool isCore);
}
