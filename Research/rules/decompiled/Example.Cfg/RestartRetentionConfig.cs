using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RestartRetentionConfig : IMemoryPackable<RestartRetentionConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RestartRetentionConfigFormatter : MemoryPackFormatter<RestartRetentionConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RestartRetentionConfig value)
		{
			RestartRetentionConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RestartRetentionConfig value)
		{
			RestartRetentionConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public string Name { get; private set; }

	[MemoryPackOrder(2)]
	public int StreakMin { get; private set; }

	[MemoryPackOrder(3)]
	public int RestartMax { get; private set; }

	[MemoryPackOrder(4)]
	public int LowStreakContent { get; private set; }

	[MemoryPackOrder(5)]
	public int HighStreakContent { get; private set; }

	[MemoryPackOrder(6)]
	public bool LowUseBaseArt { get; private set; }

	[MemoryPackOrder(7)]
	public bool HighUseBaseArt { get; private set; }

	[MemoryPackOrder(8)]
	public int RankInactiveContent { get; private set; }

	[MemoryPackOrder(9)]
	public bool RankInactiveUseBaseArt { get; private set; }

	[MemoryPackOrder(10)]
	public int CancelTarget { get; private set; }

	static RestartRetentionConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RestartRetentionConfig>())
		{
			MemoryPackFormatterProvider.Register<RestartRetentionConfig>((MemoryPackFormatter<RestartRetentionConfig>)(object)new RestartRetentionConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RestartRetentionConfig[]>())
		{
			MemoryPackFormatterProvider.Register<RestartRetentionConfig[]>((MemoryPackFormatter<RestartRetentionConfig[]>)(object)new ArrayFormatter<RestartRetentionConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RestartRetentionConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)11, ref id);
		((MemoryPackWriter)(ref writer)).WriteString(value.Name);
		id = value.StreakMin;
		int restartMax = value.RestartMax;
		int lowStreakContent = value.LowStreakContent;
		int highStreakContent = value.HighStreakContent;
		bool lowUseBaseArt = value.LowUseBaseArt;
		bool highUseBaseArt = value.HighUseBaseArt;
		int rankInactiveContent = value.RankInactiveContent;
		bool rankInactiveUseBaseArt = value.RankInactiveUseBaseArt;
		int cancelTarget = value.CancelTarget;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, int, int, bool, bool, int, bool, int>(ref id, ref restartMax, ref lowStreakContent, ref highStreakContent, ref lowUseBaseArt, ref highUseBaseArt, ref rankInactiveContent, ref rankInactiveUseBaseArt, ref cancelTarget);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RestartRetentionConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int streakMin = default(int);
		int restartMax = default(int);
		int lowStreakContent = default(int);
		int highStreakContent = default(int);
		bool lowUseBaseArt = default(bool);
		bool highUseBaseArt = default(bool);
		int rankInactiveContent = default(int);
		bool rankInactiveUseBaseArt = default(bool);
		int cancelTarget = default(int);
		string name;
		if (b == 11)
		{
			if (value != null)
			{
				id = value.Id;
				name = value.Name;
				streakMin = value.StreakMin;
				restartMax = value.RestartMax;
				lowStreakContent = value.LowStreakContent;
				highStreakContent = value.HighStreakContent;
				lowUseBaseArt = value.LowUseBaseArt;
				highUseBaseArt = value.HighUseBaseArt;
				rankInactiveContent = value.RankInactiveContent;
				rankInactiveUseBaseArt = value.RankInactiveUseBaseArt;
				cancelTarget = value.CancelTarget;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				name = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref streakMin);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref restartMax);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lowStreakContent);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref highStreakContent);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref lowUseBaseArt);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref highUseBaseArt);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankInactiveContent);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref rankInactiveUseBaseArt);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref cancelTarget);
				goto IL_022e;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
			name = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int, int, bool, bool, int, bool, int>(ref streakMin, ref restartMax, ref lowStreakContent, ref highStreakContent, ref lowUseBaseArt, ref highUseBaseArt, ref rankInactiveContent, ref rankInactiveUseBaseArt, ref cancelTarget);
		}
		else
		{
			if (b > 11)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RestartRetentionConfig), (byte)11, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				name = null;
				streakMin = 0;
				restartMax = 0;
				lowStreakContent = 0;
				highStreakContent = 0;
				lowUseBaseArt = false;
				highUseBaseArt = false;
				rankInactiveContent = 0;
				rankInactiveUseBaseArt = false;
				cancelTarget = 0;
			}
			else
			{
				id = value.Id;
				name = value.Name;
				streakMin = value.StreakMin;
				restartMax = value.RestartMax;
				lowStreakContent = value.LowStreakContent;
				highStreakContent = value.HighStreakContent;
				lowUseBaseArt = value.LowUseBaseArt;
				highUseBaseArt = value.HighUseBaseArt;
				rankInactiveContent = value.RankInactiveContent;
				rankInactiveUseBaseArt = value.RankInactiveUseBaseArt;
				cancelTarget = value.CancelTarget;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					name = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref streakMin);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref restartMax);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lowStreakContent);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref highStreakContent);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref lowUseBaseArt);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref highUseBaseArt);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref rankInactiveContent);
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref rankInactiveUseBaseArt);
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref cancelTarget);
														_ = 11;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_022e;
			}
		}
		value = new RestartRetentionConfig
		{
			Id = id,
			Name = name,
			StreakMin = streakMin,
			RestartMax = restartMax,
			LowStreakContent = lowStreakContent,
			HighStreakContent = highStreakContent,
			LowUseBaseArt = lowUseBaseArt,
			HighUseBaseArt = highUseBaseArt,
			RankInactiveContent = rankInactiveContent,
			RankInactiveUseBaseArt = rankInactiveUseBaseArt,
			CancelTarget = cancelTarget
		};
		return;
		IL_022e:
		value.Id = id;
		value.Name = name;
		value.StreakMin = streakMin;
		value.RestartMax = restartMax;
		value.LowStreakContent = lowStreakContent;
		value.HighStreakContent = highStreakContent;
		value.LowUseBaseArt = lowUseBaseArt;
		value.HighUseBaseArt = highUseBaseArt;
		value.RankInactiveContent = rankInactiveContent;
		value.RankInactiveUseBaseArt = rankInactiveUseBaseArt;
		value.CancelTarget = cancelTarget;
	}
}
