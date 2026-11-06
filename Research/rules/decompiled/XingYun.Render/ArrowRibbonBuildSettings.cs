using System;
using UnityEngine;

namespace XingYun.Render;

[Serializable]
public struct ArrowRibbonBuildSettings
{
	[Tooltip("Constant ribbon width in local units.")]
	public float width;

	[Tooltip("Requested corner rounding radius in local units. Auto-limited on short segments.")]
	public float cornerRadius;

	[Tooltip("Arc subdivisions per rounded corner (>=1). Higher = smoother corners.")]
	public int cornerSegments;

	[Tooltip("Width subdivisions per cross-section (0-8). Splits ribbon width into more vertices to reduce UV stretching at corners. 0 = only left/right edges.")]
	[Range(0f, 8f)]
	public int widthSubdivisions;

	[Tooltip("Extra straight length appended past the Head so the arrow can slide fully off.")]
	public float exitExtraLength;

	[Tooltip("0-based atlas theme row. Baked into mesh UV1.y as 1-based StyleIndex for the shader.")]
	public int styleIndex;

	public static ArrowRibbonBuildSettings Default => new ArrowRibbonBuildSettings
	{
		width = 1f,
		cornerRadius = 0.5f,
		cornerSegments = 6,
		widthSubdivisions = 0,
		exitExtraLength = 2f,
		styleIndex = 0
	};
}
