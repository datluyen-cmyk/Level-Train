using System;
using System.Collections.Generic;

namespace UnlockRing.LevelDocoder;

[Serializable]
public class AssemblyData
{
	public float position_x;

	public float position_y;

	public int type;

	public float rotation;

	public MainComponentData main_component;

	public List<SubComponentData> sub_component;
}
