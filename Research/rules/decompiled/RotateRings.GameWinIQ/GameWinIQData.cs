using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.GameWinIQ;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameWinIQData : ISaveDefaultInitializer, IMemoryPackable<GameWinIQData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameWinIQDataFormatter : MemoryPackFormatter<GameWinIQData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameWinIQData value)
		{
			GameWinIQData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameWinIQData value)
		{
			GameWinIQData.Deserialize(ref reader, ref value);
		}
	}

	public bool FeatureUnlocked;

	public int BaseScore = 70;

	public int LastDisplayScore = 70;

	public int LastActiveDate;

	public bool ReachedPeakInCareer;

	public void OnCreateDefault()
	{
		FeatureUnlocked = false;
		BaseScore = 70;
		LastDisplayScore = 70;
		LastActiveDate = 0;
		ReachedPeakInCareer = false;
	}

	static GameWinIQData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQData>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQData>((MemoryPackFormatter<GameWinIQData>)(object)new GameWinIQDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQData[]>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQData[]>((MemoryPackFormatter<GameWinIQData[]>)(object)new ArrayFormatter<GameWinIQData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameWinIQData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool, int, int, int, bool>((byte)5, ref value.FeatureUnlocked, ref value.BaseScore, ref value.LastDisplayScore, ref value.LastActiveDate, ref value.ReachedPeakInCareer);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameWinIQData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool featureUnlocked = default(bool);
		int baseScore = default(int);
		int lastDisplayScore = default(int);
		int lastActiveDate = default(int);
		bool reachedPeakInCareer = default(bool);
		if (b == 5)
		{
			if (value != null)
			{
				featureUnlocked = value.FeatureUnlocked;
				baseScore = value.BaseScore;
				lastDisplayScore = value.LastDisplayScore;
				lastActiveDate = value.LastActiveDate;
				reachedPeakInCareer = value.ReachedPeakInCareer;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref featureUnlocked);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref baseScore);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastDisplayScore);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastActiveDate);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref reachedPeakInCareer);
				goto IL_0117;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, int, int, int, bool>(ref featureUnlocked, ref baseScore, ref lastDisplayScore, ref lastActiveDate, ref reachedPeakInCareer);
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameWinIQData), (byte)5, b);
				return;
			}
			if (value == null)
			{
				featureUnlocked = false;
				baseScore = 0;
				lastDisplayScore = 0;
				lastActiveDate = 0;
				reachedPeakInCareer = false;
			}
			else
			{
				featureUnlocked = value.FeatureUnlocked;
				baseScore = value.BaseScore;
				lastDisplayScore = value.LastDisplayScore;
				lastActiveDate = value.LastActiveDate;
				reachedPeakInCareer = value.ReachedPeakInCareer;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref featureUnlocked);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref baseScore);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastDisplayScore);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastActiveDate);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref reachedPeakInCareer);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0117;
			}
		}
		value = new GameWinIQData
		{
			FeatureUnlocked = featureUnlocked,
			BaseScore = baseScore,
			LastDisplayScore = lastDisplayScore,
			LastActiveDate = lastActiveDate,
			ReachedPeakInCareer = reachedPeakInCareer
		};
		return;
		IL_0117:
		value.FeatureUnlocked = featureUnlocked;
		value.BaseScore = baseScore;
		value.LastDisplayScore = lastDisplayScore;
		value.LastActiveDate = lastActiveDate;
		value.ReachedPeakInCareer = reachedPeakInCareer;
	}
}
