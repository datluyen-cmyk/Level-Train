using Example.Cfg;
using RotateRings.Model;
using RotateRings.SpecialLevel;

namespace RotateRing.GamePlay;

public sealed class HalloweenLevelConfigMode : LevelConfigModeBase
{
	private const string TAG = "[GamePlay][HalloweenLevelConfigMode] ";

	private readonly MainLevelConfigMode mainLevelConfigMode;

	public override GameMode Mode => GameMode.Halloween;

	public HalloweenLevelConfigMode(MainLevelConfigMode mainLevelConfigMode)
	{
		this.mainLevelConfigMode = mainLevelConfigMode;
	}

	public override int GetConfigLevelId(int playerLevel)
	{
		int num = SpecialLevelModule.ResolveCurrentLevelResourceId();
		if (num > 0)
		{
			return num;
		}
		int num2 = ((playerLevel <= 0) ? 1 : playerLevel);
		int coreLevelCount = mainLevelConfigMode.GetCoreLevelCount();
		if (num2 <= coreLevelCount)
		{
			return num2;
		}
		return mainLevelConfigMode.GetConfigLevelId(num2);
	}

	public override LevelConfig GetLevelConfig(int levelId)
	{
		ExampleTables tables = GetTables();
		if (tables == null)
		{
			return null;
		}
		TbHalloweenLevelConfig tbHalloweenLevelConfig = tables.TbHalloweenLevelConfig;
		if (tbHalloweenLevelConfig == null || tbHalloweenLevelConfig.TableRowCount <= 0)
		{
			return null;
		}
		return tbHalloweenLevelConfig.Get(levelId);
	}
}
