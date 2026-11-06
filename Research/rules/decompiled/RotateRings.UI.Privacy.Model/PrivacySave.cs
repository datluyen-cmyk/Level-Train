using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace RotateRings.UI.Privacy.Model;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class PrivacySave : IMemoryPackable<PrivacySave>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PrivacySaveFormatter : MemoryPackFormatter<PrivacySave>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PrivacySave value)
		{
			PrivacySave.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PrivacySave value)
		{
			PrivacySave.Deserialize(ref reader, ref value);
		}
	}

	public bool HasAgreedStartupPrivacy;

	static PrivacySave()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PrivacySave>())
		{
			MemoryPackFormatterProvider.Register<PrivacySave>((MemoryPackFormatter<PrivacySave>)(object)new PrivacySaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PrivacySave[]>())
		{
			MemoryPackFormatterProvider.Register<PrivacySave[]>((MemoryPackFormatter<PrivacySave[]>)(object)new ArrayFormatter<PrivacySave>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PrivacySave? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool>((byte)1, ref value.HasAgreedStartupPrivacy);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PrivacySave? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool hasAgreedStartupPrivacy = default(bool);
		if (b == 1)
		{
			if (value != null)
			{
				hasAgreedStartupPrivacy = value.HasAgreedStartupPrivacy;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasAgreedStartupPrivacy);
				goto IL_006b;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasAgreedStartupPrivacy);
		}
		else
		{
			if (b > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PrivacySave), (byte)1, b);
				return;
			}
			hasAgreedStartupPrivacy = value != null && value.HasAgreedStartupPrivacy;
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasAgreedStartupPrivacy);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new PrivacySave
		{
			HasAgreedStartupPrivacy = hasAgreedStartupPrivacy
		};
		return;
		IL_006b:
		value.HasAgreedStartupPrivacy = hasAgreedStartupPrivacy;
	}
}
