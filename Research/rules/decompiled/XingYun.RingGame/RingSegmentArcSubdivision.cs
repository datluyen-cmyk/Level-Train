using System;
using UnityEngine;

namespace XingYun.RingGame;

public static class RingSegmentArcSubdivision
{
	public const float RefInnerRadiusCore = 36f;

	public const int RefFullCircleArcSubdivisions = 64;

	public const int MinFullCircleArcSubdivisions = 64;

	public const int MaxFullCircleArcSubdivisions = 384;

	public const int MaxArcDivsPerRing = 96;

	public const float MaxArcChordWorld = 0.35f;

	public const float InnerRadiusScaleExponent = 0.7f;

	public const float CoreUnitsPerRenderRadius = 24f;

	public const float CoreBandWidthPerRenderWidth = 12f;

	public static int ComputeFullCircleArcSubdivisions(float innerRadiusCore)
	{
		float num = Mathf.Pow(Mathf.Max(1f, innerRadiusCore / 36f), 0.7f);
		return Mathf.Clamp(Mathf.RoundToInt(64f * num), 64, 384);
	}

	public static int ComputeArcDivisionsForVisibleArc(int fullCircleArcSubdivisions, float visibleAngleDeg)
	{
		return ResolveArcDivisionsForVisibleArc(fullCircleArcSubdivisions, visibleAngleDeg, 0f, applyChordFloor: false);
	}

	public static int ResolveArcDivisionsForVisibleArc(int fullCircleArcSubdivisions, float visibleAngleDeg, float centerRadiusRender)
	{
		return ResolveArcDivisionsForVisibleArc(fullCircleArcSubdivisions, visibleAngleDeg, centerRadiusRender, applyChordFloor: true);
	}

	public static int ResolveClosedLoopArcDivisions(int fullCircleArcSubdivisions, float centerRadiusRender)
	{
		int num = ArcDivisionsFromChord(centerRadiusRender, 360f);
		return Mathf.Clamp(Mathf.Max(fullCircleArcSubdivisions, num), 64, 384);
	}

	public static float EstimateInnerRadiusCore(float centerRadiusRender, float bandWidthRender)
	{
		return centerRadiusRender * 24f - bandWidthRender * 12f;
	}

	private static int ResolveArcDivisionsForVisibleArc(int fullCircleArcSubdivisions, float visibleAngleDeg, float centerRadiusRender, bool applyChordFloor)
	{
		visibleAngleDeg = Mathf.Clamp(visibleAngleDeg, 0f, 360f);
		int num = Mathf.Max(1, Mathf.RoundToInt((float)fullCircleArcSubdivisions * visibleAngleDeg / 360f));
		int num2 = num;
		if (applyChordFloor && centerRadiusRender > 0.0001f)
		{
			int num3 = ArcDivisionsFromChord(centerRadiusRender, visibleAngleDeg);
			num2 = Mathf.Max(num, num3);
		}
		return Mathf.Clamp(num2, 1, 96);
	}

	private static int ArcDivisionsFromChord(float centerRadiusRender, float visibleAngleDeg)
	{
		visibleAngleDeg = Mathf.Clamp(visibleAngleDeg, 0f, 360f);
		float num = MathF.PI * 2f * centerRadiusRender * (visibleAngleDeg / 360f);
		if (num <= 0.0001f)
		{
			return 1;
		}
		return Mathf.Max(1, Mathf.CeilToInt(num / 0.35f));
	}
}
