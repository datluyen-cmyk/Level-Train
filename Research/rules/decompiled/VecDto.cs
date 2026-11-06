using System;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class VecDto
{
	public double x;

	public double y;

	public Vector2D ToDomain()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2D(x, y);
	}

	public static VecDto FromDomain(Vector2D v)
	{
		return new VecDto
		{
			x = ((Vector2D)(ref v)).X,
			y = ((Vector2D)(ref v)).Y
		};
	}
}
