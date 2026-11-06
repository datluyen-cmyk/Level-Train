using System;

namespace XY.Common.ParticleImage;

[Serializable]
public class XYBurst
{
	public float time;

	public int count = 1;

	public bool used;

	public XYBurst(float time, int count)
	{
		this.time = time;
		this.count = count;
	}
}
