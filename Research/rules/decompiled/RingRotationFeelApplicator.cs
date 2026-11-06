using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.Services;
using UnityEngine;

public sealed class RingRotationFeelApplicator
{
	private struct FingerScreenSample
	{
		public float Time;

		public Vector2 ScreenPos;
	}

	private const float RatchetStepDeg = 15f;

	private const float AppliedRotationEpsilon = 0.3f;

	private const float RatchetActiveWindowSec = 0.15f;

	private const float RatchetActiveWindowTravelPx = 18f;

	private const float RatchetActiveRecentFrameTravelPx = 1.5f;

	private const float RatchetMinPlayIntervalSec = 0.12f;

	private const float MaxLossReleaseDegPerFrame = 12f;

	private const float GapOccupancyRecheckDeg = 1f;

	private const int GapOccupancyMaxAgeFrames = 6;

	private readonly List<FingerScreenSample> fingerScreenSamples = new List<FingerScreenSample>(8);

	private float accumulatedAngleLoss;

	private float ratchetAppliedAccumDeg;

	private float lastRatchetPlayUnscaledTime = float.NegativeInfinity;

	private bool gapOccupancy;

	private bool gapOccupancyValid;

	private RingObject gapOccupancyRing;

	private float gapOccupancyRingAngleDeg;

	private int gapOccupancyAgeFrames;

	public void BeginGesture(string ringName)
	{
		accumulatedAngleLoss = 0f;
		ratchetAppliedAccumDeg = 0f;
		fingerScreenSamples.Clear();
		ResetGapOccupancySample();
		OpenRingThreadableGapQuery.ReleasePinCaches();
	}

	public void EndGesture()
	{
		accumulatedAngleLoss = 0f;
		ratchetAppliedAccumDeg = 0f;
		fingerScreenSamples.Clear();
		ResetGapOccupancySample();
		OpenRingThreadableGapQuery.ReleasePinCaches();
		AudioService.StopSound(AudioKeys.RingRatchetClick.Value);
	}

	public float Adjust(float alpha, RingObject ring, in OpenRingThreadableGapQuery.RingGapGeometry geometry, Overlap2DHandler ringOverlapHandler)
	{
		if (Mathf.Abs(alpha) < 1E-06f)
		{
			return alpha;
		}
		RingRotationFeelFunction ringRotationFeelFunction = RingRotationFeelResolver.Resolve();
		if (!ringRotationFeelFunction.EnableFeel)
		{
			return alpha;
		}
		bool num = ResolveGapOccupancy(ring, in geometry, ringOverlapHandler);
		float dampingK = ringRotationFeelFunction.DampingK1;
		float compensationK = ringRotationFeelFunction.CompensationK2;
		if (num)
		{
			float num2 = alpha * dampingK;
			accumulatedAngleLoss += alpha - num2;
			return num2;
		}
		if (Mathf.Abs(accumulatedAngleLoss) > 0.0001f && Mathf.Sign(accumulatedAngleLoss) == Mathf.Sign(alpha))
		{
			float num3 = Mathf.Abs(alpha) * (compensationK - 1f);
			float num4 = Mathf.Abs(accumulatedAngleLoss);
			if (num3 <= num4)
			{
				float result = alpha * compensationK;
				accumulatedAngleLoss = Mathf.Sign(accumulatedAngleLoss) * (num4 - num3);
				return result;
			}
			float num5 = Mathf.Min(Mathf.Abs(accumulatedAngleLoss), 12f);
			float num6 = Mathf.Sign(accumulatedAngleLoss) * num5;
			accumulatedAngleLoss -= num6;
			return alpha + num6;
		}
		return alpha;
	}

	private bool ResolveGapOccupancy(RingObject ring, in OpenRingThreadableGapQuery.RingGapGeometry geometry, Overlap2DHandler ringOverlapHandler)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		float num = (((Object)(object)ring != (Object)null) ? ((Component)ring).transform.eulerAngles.z : 0f);
		if (gapOccupancyValid && gapOccupancyRing == ring && gapOccupancyAgeFrames < 6 && Mathf.Abs(Mathf.DeltaAngle(gapOccupancyRingAngleDeg, num)) < 1f)
		{
			gapOccupancyAgeFrames++;
			return gapOccupancy;
		}
		gapOccupancy = OpenRingThreadableGapQuery.HasThreadablePinInGap(ring, in geometry, ringOverlapHandler);
		gapOccupancyRing = ring;
		gapOccupancyRingAngleDeg = num;
		gapOccupancyAgeFrames = 0;
		gapOccupancyValid = true;
		return gapOccupancy;
	}

	private void ResetGapOccupancySample()
	{
		gapOccupancy = false;
		gapOccupancyValid = false;
		gapOccupancyRing = null;
		gapOccupancyRingAngleDeg = 0f;
		gapOccupancyAgeFrames = 0;
	}

	public void NotifyApplied(float ringAppliedDeltaDeg, Vector2 fingerScreenPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		float unscaledTime = Time.unscaledTime;
		if (!IsFingerActivelyRotating(fingerScreenPos, unscaledTime))
		{
			ratchetAppliedAccumDeg = 0f;
			AudioService.StopSound(AudioKeys.RingRatchetClick.Value);
		}
		else
		{
			if (Mathf.Abs(ringAppliedDeltaDeg) < 0.3f)
			{
				return;
			}
			ratchetAppliedAccumDeg += Mathf.Abs(ringAppliedDeltaDeg);
			if (ratchetAppliedAccumDeg < 15f)
			{
				return;
			}
			if (unscaledTime - lastRatchetPlayUnscaledTime < 0.12f)
			{
				ratchetAppliedAccumDeg = Mathf.Min(ratchetAppliedAccumDeg, 15f);
				return;
			}
			AudioService.StopSound(AudioKeys.RingRatchetClick.Value);
			if (AudioService.PlaySoundSync(AudioKeys.RingRatchetClick.Value) != 0L)
			{
				lastRatchetPlayUnscaledTime = unscaledTime;
				ratchetAppliedAccumDeg -= 15f;
			}
		}
	}

	private void RecordFingerScreenSample(Vector2 screenPos, float now)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		fingerScreenSamples.Add(new FingerScreenSample
		{
			Time = now,
			ScreenPos = screenPos
		});
		float num = now - 0.15f;
		int num2 = 0;
		for (int i = 0; i < fingerScreenSamples.Count && fingerScreenSamples[i].Time < num; i++)
		{
			num2++;
		}
		if (num2 > 0)
		{
			fingerScreenSamples.RemoveRange(0, num2);
		}
	}

	private bool IsFingerActivelyRotating(Vector2 screenPos, float now)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		RecordFingerScreenSample(screenPos, now);
		if (fingerScreenSamples.Count <= 1)
		{
			return true;
		}
		int num = fingerScreenSamples.Count - 1;
		float num2 = Vector2.Distance(fingerScreenSamples[num].ScreenPos, fingerScreenSamples[num - 1].ScreenPos);
		float num3 = Vector2.Distance(fingerScreenSamples[num].ScreenPos, fingerScreenSamples[0].ScreenPos);
		if (num3 >= 18f && num2 < 1.5f)
		{
			return false;
		}
		if (!(num3 >= 18f))
		{
			return num2 >= 1.5f;
		}
		return true;
	}
}
