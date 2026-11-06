using Example.Cfg;
using RotateRings.Model;

namespace RotateRing.GamePlay;

public interface ILevelConfigMode
{
	GameMode Mode { get; }

	int GetConfigLevelId(int playerLevel);

	LevelConfig GetLevelConfig(int levelId);
}
