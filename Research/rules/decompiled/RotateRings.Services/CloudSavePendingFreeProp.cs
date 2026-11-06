using System;
using RotateRings.Prop;

namespace RotateRings.Services;

[Serializable]
public class CloudSavePendingFreeProp
{
	public int PropId;

	public int Count;

	public TrackPropUseReason Reason;
}
