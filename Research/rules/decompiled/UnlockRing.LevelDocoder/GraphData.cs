using System;

namespace UnlockRing.LevelDocoder;

[Serializable]
public abstract class GraphData
{
	public int type;

	public GraphType GraphType => (GraphType)type;
}
