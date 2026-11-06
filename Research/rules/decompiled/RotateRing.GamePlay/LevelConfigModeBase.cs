using Example.Cfg;
using RotateRings.Model;

namespace RotateRing.GamePlay;

public abstract class LevelConfigModeBase : ILevelConfigMode
{
	private static readonly string fallbackPathFmt = "Assets/02_Res/Art/Level/LevelScene/Level{0}.prefab";

	private static readonly string physicsPathFmt = "Assets/02_Res/Art/Level/PhysicLevelScene/{0}.prefab";

	private static readonly string corePathFmt = "Assets/02_Res/Art/Level/CoreLevelJson/{0}.json";

	public int LevelGapTableCount => LevelGapDegExp.LevelGapTableCount;

	public abstract GameMode Mode { get; }

	public virtual string FallbackPathFmt => fallbackPathFmt;

	public virtual string PhysicsPathFmt => physicsPathFmt;

	public virtual string CorePathFmt => corePathFmt;

	public abstract int GetConfigLevelId(int playerLevel);

	public abstract LevelConfig GetLevelConfig(int levelId);

	protected ExampleTables GetTables()
	{
		return GameApp.Config.GetTable<ExampleTables>();
	}
}
