using UnityEngine;

namespace RotateRings.GamePlay.Services;

public static class SolveRingEffect3Exp
{
	private readonly struct FragmentCountProfile
	{
		public readonly float D;

		public readonly int NMin;

		public readonly int NMax;

		public FragmentCountProfile(float density, int nMin, int nMax)
		{
			D = density;
			NMin = nMin;
			NMax = nMax;
		}
	}

	private const int SolveRingEffectFragmentGroup1 = 7;

	private const int SolveRingEffectFragmentGroup2 = 8;

	private static readonly FragmentCountProfile Group1Profile = new FragmentCountProfile(3f, 1, 120);

	private static readonly FragmentCountProfile Group2Profile = new FragmentCountProfile(3f, 30, 80);

	public static bool UseGmFragmentTuningOverride { get; set; }

	public static float GmFragmentDensity { get; set; } = Group1Profile.D;

	public static int GmFragmentNMin { get; set; } = Group1Profile.NMin;

	public static int GmFragmentNMax { get; set; } = Group1Profile.NMax;

	public static void ResetGmFragmentTuningToGroup1Defaults()
	{
		GmFragmentDensity = Group1Profile.D;
		GmFragmentNMin = Group1Profile.NMin;
		GmFragmentNMax = Group1Profile.NMax;
	}

	public static void ResetGmFragmentTuningToGroup2Defaults()
	{
		GmFragmentDensity = Group2Profile.D;
		GmFragmentNMin = Group2Profile.NMin;
		GmFragmentNMax = Group2Profile.NMax;
	}

	public static bool TryResolveFragmentCount(int solveRingEffect, IGameplayObject source, out int count)
	{
		count = 0;
		if (solveRingEffect != 7 && solveRingEffect != 8)
		{
			return false;
		}
		if (!(source is RingObject) && !(source is PullObject))
		{
			return false;
		}
		FragmentCountProfile fragmentCountProfile = ((solveRingEffect == 7) ? Group1Profile : Group2Profile);
		if (UseGmFragmentTuningOverride)
		{
			fragmentCountProfile = new FragmentCountProfile(GmFragmentDensity, GmFragmentNMin, GmFragmentNMax);
		}
		if (!SolveFragmentPathLengthUtil.TryCompute(source, out var length))
		{
			return false;
		}
		int num = Mathf.RoundToInt(length * fragmentCountProfile.D);
		if (num < 1)
		{
			num = 1;
		}
		int nMin = fragmentCountProfile.NMin;
		int num2 = fragmentCountProfile.NMax;
		if (num2 < nMin)
		{
			num2 = nMin;
		}
		count = Mathf.Clamp(num, nMin, num2);
		return true;
	}
}
