using System;
using RotateRings.Prop;
using RotateRings.UI.Game;

namespace RotateRings.UI.Prop;

public sealed class PropAdRewardRequest
{
	public PropUseEvent EventData;

	public IPropUseServiceHost Host;

	public Action OnGrantOnlyComplete;
}
