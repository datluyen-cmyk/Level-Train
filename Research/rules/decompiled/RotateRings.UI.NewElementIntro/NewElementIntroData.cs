using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.UI.NewElementIntro;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class NewElementIntroData : ISaveDefaultInitializer, IMemoryPackable<NewElementIntroData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class NewElementIntroDataFormatter : MemoryPackFormatter<NewElementIntroData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref NewElementIntroData value)
		{
			NewElementIntroData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref NewElementIntroData value)
		{
			NewElementIntroData.Deserialize(ref reader, ref value);
		}
	}

	public List<int> IntroducedElementIds;

	public void OnCreateDefault()
	{
		IntroducedElementIds = new List<int>();
	}

	static NewElementIntroData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<NewElementIntroData>())
		{
			MemoryPackFormatterProvider.Register<NewElementIntroData>((MemoryPackFormatter<NewElementIntroData>)(object)new NewElementIntroDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<NewElementIntroData[]>())
		{
			MemoryPackFormatterProvider.Register<NewElementIntroData[]>((MemoryPackFormatter<NewElementIntroData[]>)(object)new ArrayFormatter<NewElementIntroData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref NewElementIntroData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)1);
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref value.IntroducedElementIds);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref NewElementIntroData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		List<int> introducedElementIds;
		if (b == 1)
		{
			if (value != null)
			{
				introducedElementIds = value.IntroducedElementIds;
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref introducedElementIds);
				goto IL_006a;
			}
			introducedElementIds = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(NewElementIntroData), (byte)1, b);
				return;
			}
			introducedElementIds = ((value != null) ? value.IntroducedElementIds : null);
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref introducedElementIds);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new NewElementIntroData
		{
			IntroducedElementIds = introducedElementIds
		};
		return;
		IL_006a:
		value.IntroducedElementIds = introducedElementIds;
	}
}
