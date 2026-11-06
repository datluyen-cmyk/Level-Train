using System;
using RotateRings.Prop;

namespace RotateRings.UI.PropUnlock;

public class PropUnlockOpenData
{
	public EPropType PropType;

	public int GrantCount;

	public string PropName;

	public string IconName;

	public int FromCount;

	public int ToCount;

	public string FlyIconName;

	public bool NeedFlyPresentation;

	public Action OnClosed;
}
