using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class AudioGroupConfig : IMemoryPackable<AudioGroupConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AudioGroupConfigFormatter : MemoryPackFormatter<AudioGroupConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AudioGroupConfig value)
		{
			AudioGroupConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AudioGroupConfig value)
		{
			AudioGroupConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public int AudioGroupID { get; private set; }

	[MemoryPackOrder(2)]
	public string OrgAudio { get; private set; }

	[MemoryPackOrder(3)]
	public string OverrideAudio { get; private set; }

	static AudioGroupConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AudioGroupConfig>())
		{
			MemoryPackFormatterProvider.Register<AudioGroupConfig>((MemoryPackFormatter<AudioGroupConfig>)(object)new AudioGroupConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AudioGroupConfig[]>())
		{
			MemoryPackFormatterProvider.Register<AudioGroupConfig[]>((MemoryPackFormatter<AudioGroupConfig[]>)(object)new ArrayFormatter<AudioGroupConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AudioGroupConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		int audioGroupID = value.AudioGroupID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)4, ref iD, ref audioGroupID);
		((MemoryPackWriter)(ref writer)).WriteString(value.OrgAudio);
		((MemoryPackWriter)(ref writer)).WriteString(value.OverrideAudio);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AudioGroupConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		int audioGroupID = default(int);
		string orgAudio;
		string overrideAudio;
		if (b == 4)
		{
			if (value != null)
			{
				iD = value.ID;
				audioGroupID = value.AudioGroupID;
				orgAudio = value.OrgAudio;
				overrideAudio = value.OverrideAudio;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref audioGroupID);
				orgAudio = ((MemoryPackReader)(ref reader)).ReadString();
				overrideAudio = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_00f2;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref iD, ref audioGroupID);
			orgAudio = ((MemoryPackReader)(ref reader)).ReadString();
			overrideAudio = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AudioGroupConfig), (byte)4, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				audioGroupID = 0;
				orgAudio = null;
				overrideAudio = null;
			}
			else
			{
				iD = value.ID;
				audioGroupID = value.AudioGroupID;
				orgAudio = value.OrgAudio;
				overrideAudio = value.OverrideAudio;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref audioGroupID);
					if (b != 2)
					{
						orgAudio = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							overrideAudio = ((MemoryPackReader)(ref reader)).ReadString();
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f2;
			}
		}
		value = new AudioGroupConfig
		{
			ID = iD,
			AudioGroupID = audioGroupID,
			OrgAudio = orgAudio,
			OverrideAudio = overrideAudio
		};
		return;
		IL_00f2:
		value.ID = iD;
		value.AudioGroupID = audioGroupID;
		value.OrgAudio = orgAudio;
		value.OverrideAudio = overrideAudio;
	}
}
