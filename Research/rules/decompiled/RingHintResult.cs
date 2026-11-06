using System;
using UnlockRing.Core.Domain;

public readonly struct RingHintResult
{
	public PartId PartId { get; init; }

	public HitResultSource Source { get; init; }

	public int Weight { get; init; }

	public string Reason { get; init; }

	public MotionKind MotionKind { get; init; }

	public double MotionAmount { get; init; }

	public Vector2D TranslateAxis { get; init; }

	public double AbsMotionAmount
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if ((int)MotionKind != 0)
			{
				return MotionAmount;
			}
			return Math.Abs(MotionAmount);
		}
	}

	public static RingHintResult FromMotionRequest(PartId partId, HitResultSource source, int weight, MotionRequest motion, string reason)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (motion == null)
		{
			throw new ArgumentNullException("motion");
		}
		if ((int)motion.Kind == 0)
		{
			return new RingHintResult
			{
				PartId = partId,
				Source = source,
				Weight = weight,
				Reason = (reason ?? string.Empty),
				MotionKind = (MotionKind)0,
				MotionAmount = motion.RequestedAmount
			};
		}
		if (!motion.Direction.HasValue)
		{
			throw new ArgumentException("Translate motion requires Direction.", "motion");
		}
		return new RingHintResult
		{
			PartId = partId,
			Source = source,
			Weight = weight,
			Reason = (reason ?? string.Empty),
			MotionKind = (MotionKind)1,
			MotionAmount = motion.RequestedAmount,
			TranslateAxis = motion.Direction.Value
		};
	}

	public MotionRequest ToMotionRequest()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if ((int)MotionKind == 0)
		{
			return MotionRequest.Rotate(PartId, MotionAmount);
		}
		return MotionRequest.Translate(PartId, MotionAmount, TranslateAxis);
	}
}
