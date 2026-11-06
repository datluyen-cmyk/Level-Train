using System.Collections.Generic;

namespace Example.Cfg;

public interface ILevelTable
{
	int TableRowCount { get; }

	List<LevelConfig> DataList { get; }

	LevelConfig Get(int key);
}
