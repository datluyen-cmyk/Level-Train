using XingYun.Level;

namespace Game.Level;

public class LevelSystemLevelSelectorExtensions
{
	public static void Initialize()
	{
		LevelSystem.GetConfig().SelectorStrategy = (ILevelSelectorStrategy)(object)new CustomLevelSelector();
	}
}
