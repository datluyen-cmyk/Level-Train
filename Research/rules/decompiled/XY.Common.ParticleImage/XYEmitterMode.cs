using UnityEngine;

namespace XY.Common.ParticleImage;

public enum XYEmitterMode
{
	[InspectorName("标准发射")]
	Standard,
	[InspectorName("运行时定点个数")]
	RuntimeFixedCount
}
