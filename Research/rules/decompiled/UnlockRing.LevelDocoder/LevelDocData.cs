using System;
using System.Collections.Generic;

namespace UnlockRing.LevelDocoder;

[Serializable]
public class LevelDocData
{
	public Dictionary<string, GraphData> graphs;

	public List<Dictionary<string, AssemblyData>> assemblies;
}
