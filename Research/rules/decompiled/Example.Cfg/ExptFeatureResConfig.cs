using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class ExptFeatureResConfig : IMemoryPackable<ExptFeatureResConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ExptFeatureResConfigFormatter : MemoryPackFormatter<ExptFeatureResConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ExptFeatureResConfig value)
		{
			ExptFeatureResConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ExptFeatureResConfig value)
		{
			ExptFeatureResConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public string FeatureId { get; private set; }

	[MemoryPackOrder(1)]
	public string FeatureValue { get; private set; }

	[MemoryPackOrder(2)]
	public List<string> Addresses { get; private set; }

	static ExptFeatureResConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ExptFeatureResConfig>())
		{
			MemoryPackFormatterProvider.Register<ExptFeatureResConfig>((MemoryPackFormatter<ExptFeatureResConfig>)(object)new ExptFeatureResConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ExptFeatureResConfig[]>())
		{
			MemoryPackFormatterProvider.Register<ExptFeatureResConfig[]>((MemoryPackFormatter<ExptFeatureResConfig[]>)(object)new ArrayFormatter<ExptFeatureResConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
		{
			MemoryPackFormatterProvider.Register<List<string>>((MemoryPackFormatter<List<string>>)(object)new ListFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ExptFeatureResConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)3);
		((MemoryPackWriter)(ref writer)).WriteString(value.FeatureId);
		((MemoryPackWriter)(ref writer)).WriteString(value.FeatureValue);
		List<string> addresses = value.Addresses;
		((MemoryPackWriter)(ref writer)).WriteValue<List<string>>(ref addresses);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ExptFeatureResConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<string> addresses;
		string featureId;
		string featureValue;
		if (b == 3)
		{
			if (value != null)
			{
				featureId = value.FeatureId;
				featureValue = value.FeatureValue;
				addresses = value.Addresses;
				featureId = ((MemoryPackReader)(ref reader)).ReadString();
				featureValue = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref addresses);
				goto IL_00c3;
			}
			featureId = ((MemoryPackReader)(ref reader)).ReadString();
			featureValue = ((MemoryPackReader)(ref reader)).ReadString();
			addresses = ((MemoryPackReader)(ref reader)).ReadValue<List<string>>();
		}
		else
		{
			if (b > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ExptFeatureResConfig), (byte)3, b);
				return;
			}
			if (value == null)
			{
				featureId = null;
				featureValue = null;
				addresses = null;
			}
			else
			{
				featureId = value.FeatureId;
				featureValue = value.FeatureValue;
				addresses = value.Addresses;
			}
			if (b != 0)
			{
				featureId = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					featureValue = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref addresses);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c3;
			}
		}
		value = new ExptFeatureResConfig
		{
			FeatureId = featureId,
			FeatureValue = featureValue,
			Addresses = addresses
		};
		return;
		IL_00c3:
		value.FeatureId = featureId;
		value.FeatureValue = featureValue;
		value.Addresses = addresses;
	}
}
