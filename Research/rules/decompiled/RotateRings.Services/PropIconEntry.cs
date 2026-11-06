using System;
using System.Collections.Generic;
using RotateRings.Prop;

namespace RotateRings.Services;

[Serializable]
public class PropIconEntry
{
	public EPropType propType;

	public List<PropIconStyleOverride> styleOverrides = new List<PropIconStyleOverride>();
}
