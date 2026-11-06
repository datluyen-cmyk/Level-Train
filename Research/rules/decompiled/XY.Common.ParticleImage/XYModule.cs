using System;

namespace XY.Common.ParticleImage;

[Serializable]
public struct XYModule
{
	public bool enabled;

	public XYModule(bool enabled)
	{
		this.enabled = enabled;
	}
}
