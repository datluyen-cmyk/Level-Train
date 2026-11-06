using RotateRings.Level;
using UniLog;
using XingYun.Level;

namespace Game.Level;

public class LevelRepositoryExtensionsTest
{
	public void Test()
	{
		LevelRepositoryExtensions<LevelBoardEntry> val = new LevelRepositoryExtensions<LevelBoardEntry>("ExptLevelStyle1", "Board");
		if (val == null)
		{
			Log.LogError("LevelRepositoryExtensionsTest", "repo is null");
		}
		else if (val.GetCsvEntry("02_02", 1) == null)
		{
			Log.LogError("LevelRepositoryExtensionsTest", "entry is null");
		}
	}
}
