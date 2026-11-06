using System;
using RotateRings.Prop;

namespace RotateRings.UI.Prop;

public sealed class UIPropAwardParam
{
	public EPropType PropType;

	public int GrantCount;

	public Action OnAccept;

	public Action OnClose;
}
