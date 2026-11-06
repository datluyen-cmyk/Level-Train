using System.Collections.Generic;

internal readonly struct ChainBreakRequest
{
	public readonly List<ChainInstanceData> Instances;

	public readonly int BreakIndex;

	public readonly int Slot;

	public readonly int RingOrder;

	public readonly bool MuteBreakSound;

	public readonly RingObject LockOpenRing;

	public ChainBreakRequest(List<ChainInstanceData> instances, int breakIndex, int slot = 0, int ringOrder = 0, bool muteBreakSound = false, RingObject lockOpenRing = null)
	{
		Instances = instances;
		BreakIndex = breakIndex;
		Slot = slot;
		RingOrder = ringOrder;
		MuteBreakSound = muteBreakSound;
		LockOpenRing = lockOpenRing;
	}
}
