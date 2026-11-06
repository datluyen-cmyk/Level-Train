using System;

namespace XY.Common.ParticleImage;

[Serializable]
public struct XYSpeedRange
{
	public float from;

	public float to;

	public XYSpeedRange(float from, float to)
	{
		this.from = from;
		this.to = to;
	}
}
