using XingYun.Level;

namespace Game.Level;

public class LevelSystemDifficultySamplerExtensions
{
	public static void Initialize()
	{
		LevelSystem.GetConfig().SamplerStrategy = (IDifficultySamplerStrategy)(object)new CustomDifficultySamplerStrategy();
	}
}
