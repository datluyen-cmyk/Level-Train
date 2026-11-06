using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class GameWinIQLevelSuffixCoe : IMemoryPackable<GameWinIQLevelSuffixCoe>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class GameWinIQLevelSuffixCoeFormatter : MemoryPackFormatter<GameWinIQLevelSuffixCoe>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref GameWinIQLevelSuffixCoe value)
		{
			GameWinIQLevelSuffixCoe.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref GameWinIQLevelSuffixCoe value)
		{
			GameWinIQLevelSuffixCoe.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int Group { get; private set; }

	[MemoryPackOrder(2)]
	public string LevelSuffix0 { get; private set; }

	[MemoryPackOrder(3)]
	public string LevelSuffix1 { get; private set; }

	[MemoryPackOrder(4)]
	public string LevelSuffix2 { get; private set; }

	[MemoryPackOrder(5)]
	public string LevelSuffix3 { get; private set; }

	[MemoryPackOrder(6)]
	public string LevelSuffix4 { get; private set; }

	[MemoryPackOrder(7)]
	public string LevelSuffix5 { get; private set; }

	[MemoryPackOrder(8)]
	public string LevelSuffix6 { get; private set; }

	[MemoryPackOrder(9)]
	public string LevelSuffix7 { get; private set; }

	[MemoryPackOrder(10)]
	public string LevelSuffix8 { get; private set; }

	[MemoryPackOrder(11)]
	public string LevelSuffix9 { get; private set; }

	static GameWinIQLevelSuffixCoe()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQLevelSuffixCoe>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQLevelSuffixCoe>((MemoryPackFormatter<GameWinIQLevelSuffixCoe>)(object)new GameWinIQLevelSuffixCoeFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<GameWinIQLevelSuffixCoe[]>())
		{
			MemoryPackFormatterProvider.Register<GameWinIQLevelSuffixCoe[]>((MemoryPackFormatter<GameWinIQLevelSuffixCoe[]>)(object)new ArrayFormatter<GameWinIQLevelSuffixCoe>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref GameWinIQLevelSuffixCoe? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int num = value.Group;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)12, ref id, ref num);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix0);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix1);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix2);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix3);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix4);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix5);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix6);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix7);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix8);
		((MemoryPackWriter)(ref writer)).WriteString(value.LevelSuffix9);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref GameWinIQLevelSuffixCoe? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int num = default(int);
		string levelSuffix;
		string levelSuffix2;
		string levelSuffix3;
		string levelSuffix4;
		string levelSuffix5;
		string levelSuffix6;
		string levelSuffix7;
		string levelSuffix8;
		string levelSuffix9;
		string levelSuffix10;
		if (b == 12)
		{
			if (value != null)
			{
				id = value.Id;
				num = value.Group;
				levelSuffix = value.LevelSuffix0;
				levelSuffix2 = value.LevelSuffix1;
				levelSuffix3 = value.LevelSuffix2;
				levelSuffix4 = value.LevelSuffix3;
				levelSuffix5 = value.LevelSuffix4;
				levelSuffix6 = value.LevelSuffix5;
				levelSuffix7 = value.LevelSuffix6;
				levelSuffix8 = value.LevelSuffix7;
				levelSuffix9 = value.LevelSuffix8;
				levelSuffix10 = value.LevelSuffix9;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				levelSuffix = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix2 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix3 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix4 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix5 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix6 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix7 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix8 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix9 = ((MemoryPackReader)(ref reader)).ReadString();
				levelSuffix10 = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_028d;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref id, ref num);
			levelSuffix = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix2 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix3 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix4 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix5 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix6 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix7 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix8 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix9 = ((MemoryPackReader)(ref reader)).ReadString();
			levelSuffix10 = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 12)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(GameWinIQLevelSuffixCoe), (byte)12, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				num = 0;
				levelSuffix = null;
				levelSuffix2 = null;
				levelSuffix3 = null;
				levelSuffix4 = null;
				levelSuffix5 = null;
				levelSuffix6 = null;
				levelSuffix7 = null;
				levelSuffix8 = null;
				levelSuffix9 = null;
				levelSuffix10 = null;
			}
			else
			{
				id = value.Id;
				num = value.Group;
				levelSuffix = value.LevelSuffix0;
				levelSuffix2 = value.LevelSuffix1;
				levelSuffix3 = value.LevelSuffix2;
				levelSuffix4 = value.LevelSuffix3;
				levelSuffix5 = value.LevelSuffix4;
				levelSuffix6 = value.LevelSuffix5;
				levelSuffix7 = value.LevelSuffix6;
				levelSuffix8 = value.LevelSuffix7;
				levelSuffix9 = value.LevelSuffix8;
				levelSuffix10 = value.LevelSuffix9;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
					if (b != 2)
					{
						levelSuffix = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							levelSuffix2 = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								levelSuffix3 = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									levelSuffix4 = ((MemoryPackReader)(ref reader)).ReadString();
									if (b != 6)
									{
										levelSuffix5 = ((MemoryPackReader)(ref reader)).ReadString();
										if (b != 7)
										{
											levelSuffix6 = ((MemoryPackReader)(ref reader)).ReadString();
											if (b != 8)
											{
												levelSuffix7 = ((MemoryPackReader)(ref reader)).ReadString();
												if (b != 9)
												{
													levelSuffix8 = ((MemoryPackReader)(ref reader)).ReadString();
													if (b != 10)
													{
														levelSuffix9 = ((MemoryPackReader)(ref reader)).ReadString();
														if (b != 11)
														{
															levelSuffix10 = ((MemoryPackReader)(ref reader)).ReadString();
															_ = 12;
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
			}
			if (value != null)
			{
				goto IL_028d;
			}
		}
		value = new GameWinIQLevelSuffixCoe
		{
			Id = id,
			Group = num,
			LevelSuffix0 = levelSuffix,
			LevelSuffix1 = levelSuffix2,
			LevelSuffix2 = levelSuffix3,
			LevelSuffix3 = levelSuffix4,
			LevelSuffix4 = levelSuffix5,
			LevelSuffix5 = levelSuffix6,
			LevelSuffix6 = levelSuffix7,
			LevelSuffix7 = levelSuffix8,
			LevelSuffix8 = levelSuffix9,
			LevelSuffix9 = levelSuffix10
		};
		return;
		IL_028d:
		value.Id = id;
		value.Group = num;
		value.LevelSuffix0 = levelSuffix;
		value.LevelSuffix1 = levelSuffix2;
		value.LevelSuffix2 = levelSuffix3;
		value.LevelSuffix3 = levelSuffix4;
		value.LevelSuffix4 = levelSuffix5;
		value.LevelSuffix5 = levelSuffix6;
		value.LevelSuffix6 = levelSuffix7;
		value.LevelSuffix7 = levelSuffix8;
		value.LevelSuffix8 = levelSuffix9;
		value.LevelSuffix9 = levelSuffix10;
	}
}
