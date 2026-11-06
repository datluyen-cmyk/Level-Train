using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RankListActivity : IMemoryPackable<RankListActivity>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RankListActivityFormatter : MemoryPackFormatter<RankListActivity>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RankListActivity value)
		{
			RankListActivity.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RankListActivity value)
		{
			RankListActivity.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Period { get; private set; }

	[MemoryPackOrder(1)]
	public string Name { get; private set; }

	[MemoryPackOrder(2)]
	public int TimeLimite { get; private set; }

	[MemoryPackOrder(3)]
	public string CollectThing { get; private set; }

	static RankListActivity()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RankListActivity>())
		{
			MemoryPackFormatterProvider.Register<RankListActivity>((MemoryPackFormatter<RankListActivity>)(object)new RankListActivityFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RankListActivity[]>())
		{
			MemoryPackFormatterProvider.Register<RankListActivity[]>((MemoryPackFormatter<RankListActivity[]>)(object)new ArrayFormatter<RankListActivity>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RankListActivity? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int period = value.Period;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)4, ref period);
		((MemoryPackWriter)(ref writer)).WriteString(value.Name);
		period = value.TimeLimite;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref period);
		((MemoryPackWriter)(ref writer)).WriteString(value.CollectThing);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RankListActivity? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int period = default(int);
		int timeLimite = default(int);
		string name;
		string collectThing;
		if (b == 4)
		{
			if (value != null)
			{
				period = value.Period;
				name = value.Name;
				timeLimite = value.TimeLimite;
				collectThing = value.CollectThing;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
				name = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref timeLimite);
				collectThing = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_00f8;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
			name = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref timeLimite);
			collectThing = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RankListActivity), (byte)4, b);
				return;
			}
			if (value == null)
			{
				period = 0;
				name = null;
				timeLimite = 0;
				collectThing = null;
			}
			else
			{
				period = value.Period;
				name = value.Name;
				timeLimite = value.TimeLimite;
				collectThing = value.CollectThing;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref period);
				if (b != 1)
				{
					name = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref timeLimite);
						if (b != 3)
						{
							collectThing = ((MemoryPackReader)(ref reader)).ReadString();
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f8;
			}
		}
		value = new RankListActivity
		{
			Period = period,
			Name = name,
			TimeLimite = timeLimite,
			CollectThing = collectThing
		};
		return;
		IL_00f8:
		value.Period = period;
		value.Name = name;
		value.TimeLimite = timeLimite;
		value.CollectThing = collectThing;
	}
}
