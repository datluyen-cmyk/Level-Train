using System;

namespace RotateRings.Model;

[Flags]
public enum CardExtraTag : byte
{
	None = 0,
	RankTag = 1
}
