using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.DailyScore;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class DailyScoreData : ISaveDefaultInitializer, IMemoryPackable<DailyScoreData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DailyScoreDataFormatter : MemoryPackFormatter<DailyScoreData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DailyScoreData value)
		{
			DailyScoreData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DailyScoreData value)
		{
			DailyScoreData.Deserialize(ref reader, ref value);
		}
	}

	public bool FeatureUnlocked;

	public int BestScoreDate;

	public int TodayBestScore;

	public void OnCreateDefault()
	{
		FeatureUnlocked = false;
		BestScoreDate = 0;
		TodayBestScore = 0;
	}

	static DailyScoreData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DailyScoreData>())
		{
			MemoryPackFormatterProvider.Register<DailyScoreData>((MemoryPackFormatter<DailyScoreData>)(object)new DailyScoreDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DailyScoreData[]>())
		{
			MemoryPackFormatterProvider.Register<DailyScoreData[]>((MemoryPackFormatter<DailyScoreData[]>)(object)new ArrayFormatter<DailyScoreData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DailyScoreData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool, int, int>((byte)3, ref value.FeatureUnlocked, ref value.BestScoreDate, ref value.TodayBestScore);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DailyScoreData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool featureUnlocked = default(bool);
		int bestScoreDate = default(int);
		int todayBestScore = default(int);
		if (b == 3)
		{
			if (value != null)
			{
				featureUnlocked = value.FeatureUnlocked;
				bestScoreDate = value.BestScoreDate;
				todayBestScore = value.TodayBestScore;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref featureUnlocked);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref bestScoreDate);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref todayBestScore);
				goto IL_00be;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, int, int>(ref featureUnlocked, ref bestScoreDate, ref todayBestScore);
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DailyScoreData), (byte)3, b);
				return;
			}
			if (value == null)
			{
				featureUnlocked = false;
				bestScoreDate = 0;
				todayBestScore = 0;
			}
			else
			{
				featureUnlocked = value.FeatureUnlocked;
				bestScoreDate = value.BestScoreDate;
				todayBestScore = value.TodayBestScore;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref featureUnlocked);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref bestScoreDate);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref todayBestScore);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00be;
			}
		}
		value = new DailyScoreData
		{
			FeatureUnlocked = featureUnlocked,
			BestScoreDate = bestScoreDate,
			TodayBestScore = todayBestScore
		};
		return;
		IL_00be:
		value.FeatureUnlocked = featureUnlocked;
		value.BestScoreDate = bestScoreDate;
		value.TodayBestScore = todayBestScore;
	}
}
