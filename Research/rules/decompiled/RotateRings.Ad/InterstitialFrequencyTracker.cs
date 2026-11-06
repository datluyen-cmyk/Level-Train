using System.Collections.Generic;

namespace RotateRings.Ad;

public static class InterstitialFrequencyTracker
{
	private const double HighThreshold = 160.0;

	private const double MediumThreshold = 250.0;

	private static readonly List<double> showActionTimestamps = new List<double>();

	public static IReadOnlyList<double> ShowActionTimestamps => showActionTimestamps;

	public static InterstitialFrequencyTier CurrentTier { get; private set; } = InterstitialFrequencyTier.Low;

	public static InterstitialFrequencyTier FromGmUiTier(int uiTier)
	{
		return uiTier switch
		{
			1 => InterstitialFrequencyTier.Medium, 
			2 => InterstitialFrequencyTier.High, 
			_ => InterstitialFrequencyTier.Low, 
		};
	}

	public static int ToGmUiTier(InterstitialFrequencyTier tier)
	{
		return tier switch
		{
			InterstitialFrequencyTier.High => 2, 
			InterstitialFrequencyTier.Medium => 1, 
			_ => 0, 
		};
	}

	public static void Init()
	{
		CurrentTier = Classify(GameApp.Save.GetOrCreate<InterstitialFrequencyRecord>().PreviousAverageInterval);
		showActionTimestamps.Clear();
	}

	public static void RecordShowAction(double timestamp)
	{
		showActionTimestamps.Add(timestamp);
		SaveToRecord();
	}

	public static void SaveToRecord()
	{
		GameApp.Save.GetOrCreate<InterstitialFrequencyRecord>().PreviousAverageInterval = CalculateAverageInterval();
	}

	public static double CalculateAverageInterval()
	{
		int count = showActionTimestamps.Count;
		if (count < 2)
		{
			return -1.0;
		}
		double num = 0.0;
		for (int i = 1; i < count; i++)
		{
			num += showActionTimestamps[i] - showActionTimestamps[i - 1];
		}
		return num / (double)(count - 1);
	}

	private static InterstitialFrequencyTier Classify(double averageInterval)
	{
		if (averageInterval <= 0.0)
		{
			return InterstitialFrequencyTier.Low;
		}
		if (averageInterval <= 160.0)
		{
			return InterstitialFrequencyTier.High;
		}
		if (averageInterval <= 250.0)
		{
			return InterstitialFrequencyTier.Medium;
		}
		return InterstitialFrequencyTier.Low;
	}
}
