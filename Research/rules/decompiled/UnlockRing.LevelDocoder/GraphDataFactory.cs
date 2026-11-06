using System;
using UnityEngine;

namespace UnlockRing.LevelDocoder;

public static class GraphDataFactory
{
	[Serializable]
	private struct TypeProbe
	{
		public int type;
	}

	public static GraphData FromJson(string json)
	{
		TypeProbe typeProbe = JsonUtility.FromJson<TypeProbe>(json);
		return typeProbe.type switch
		{
			0 => JsonUtility.FromJson<RingGraphData>(json), 
			1 => JsonUtility.FromJson<SlideBarGraphData>(json), 
			2 => JsonUtility.FromJson<GuideGraphData>(json), 
			3 => JsonUtility.FromJson<BombGraphData>(json), 
			4 => JsonUtility.FromJson<PinGraphData>(json), 
			5 => JsonUtility.FromJson<StopPinGraphData>(json), 
			6 => JsonUtility.FromJson<BentGraphData>(json), 
			_ => throw new ArgumentException($"未知的 graph type: {typeProbe.type}"), 
		};
	}
}
