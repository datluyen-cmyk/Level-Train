using System;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class PoseDto
{
	public VecDto position;

	public double rotationRadians;

	public Pose2D ToDomain()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return new Pose2D(position?.ToDomain() ?? Vector2D.Zero, rotationRadians);
	}

	public static PoseDto FromDomain(Pose2D p)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return new PoseDto
		{
			position = VecDto.FromDomain(((Pose2D)(ref p)).Position),
			rotationRadians = ((Pose2D)(ref p)).RotationRadians
		};
	}
}
