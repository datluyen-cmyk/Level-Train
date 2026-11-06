using System;
using UnityEngine;

namespace XingYun.RingGame;

[Serializable]
public struct RingSegmentBuildSettings
{
	public float width;

	[Range(0f, 8f)]
	public int widthSubdivisions;

	public int arcSubdivisions;

	public int styleIndex;

	public bool useHeadTail;

	public static RingSegmentBuildSettings Default => new RingSegmentBuildSettings
	{
		width = 0.5f,
		widthSubdivisions = 0,
		arcSubdivisions = 32,
		styleIndex = 0,
		useHeadTail = true
	};
}
