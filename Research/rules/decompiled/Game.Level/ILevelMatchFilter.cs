using System.Collections.Generic;
using Example.Cfg;

namespace Game.Level;

public interface ILevelMatchFilter
{
	List<LevelMatchConfig> Filter(List<LevelMatchConfig> configs);

	string GetMatchFilterInfo();
}
