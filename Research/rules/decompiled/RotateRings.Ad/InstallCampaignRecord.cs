using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Ad;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class InstallCampaignRecord : ISaveDefaultInitializer, IMemoryPackable<InstallCampaignRecord>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InstallCampaignRecordFormatter : MemoryPackFormatter<InstallCampaignRecord>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InstallCampaignRecord value)
		{
			InstallCampaignRecord.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InstallCampaignRecord value)
		{
			InstallCampaignRecord.Deserialize(ref reader, ref value);
		}
	}

	public bool Matched;

	public string MatchedSuffix;

	public int Scheme;

	public bool AttributionResolved;

	public string RawAttribution;

	public string Campaign;

	public void OnCreateDefault()
	{
		Matched = false;
		MatchedSuffix = string.Empty;
		Scheme = 0;
		AttributionResolved = false;
		RawAttribution = string.Empty;
		Campaign = string.Empty;
	}

	static InstallCampaignRecord()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InstallCampaignRecord>())
		{
			MemoryPackFormatterProvider.Register<InstallCampaignRecord>((MemoryPackFormatter<InstallCampaignRecord>)(object)new InstallCampaignRecordFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InstallCampaignRecord[]>())
		{
			MemoryPackFormatterProvider.Register<InstallCampaignRecord[]>((MemoryPackFormatter<InstallCampaignRecord[]>)(object)new ArrayFormatter<InstallCampaignRecord>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InstallCampaignRecord? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<bool>((byte)6, ref value.Matched);
		((MemoryPackWriter)(ref writer)).WriteString(value.MatchedSuffix);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, bool>(ref value.Scheme, ref value.AttributionResolved);
		((MemoryPackWriter)(ref writer)).WriteString(value.RawAttribution);
		((MemoryPackWriter)(ref writer)).WriteString(value.Campaign);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InstallCampaignRecord? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		bool matched = default(bool);
		int scheme = default(int);
		bool attributionResolved = default(bool);
		string matchedSuffix;
		string rawAttribution;
		string campaign;
		if (b == 6)
		{
			if (value != null)
			{
				matched = value.Matched;
				matchedSuffix = value.MatchedSuffix;
				scheme = value.Scheme;
				attributionResolved = value.AttributionResolved;
				rawAttribution = value.RawAttribution;
				campaign = value.Campaign;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref matched);
				matchedSuffix = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref scheme);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref attributionResolved);
				rawAttribution = ((MemoryPackReader)(ref reader)).ReadString();
				campaign = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_015a;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref matched);
			matchedSuffix = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, bool>(ref scheme, ref attributionResolved);
			rawAttribution = ((MemoryPackReader)(ref reader)).ReadString();
			campaign = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 6)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InstallCampaignRecord), (byte)6, b);
				return;
			}
			if (value == null)
			{
				matched = false;
				matchedSuffix = null;
				scheme = 0;
				attributionResolved = false;
				rawAttribution = null;
				campaign = null;
			}
			else
			{
				matched = value.Matched;
				matchedSuffix = value.MatchedSuffix;
				scheme = value.Scheme;
				attributionResolved = value.AttributionResolved;
				rawAttribution = value.RawAttribution;
				campaign = value.Campaign;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref matched);
				if (b != 1)
				{
					matchedSuffix = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref scheme);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref attributionResolved);
							if (b != 4)
							{
								rawAttribution = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									campaign = ((MemoryPackReader)(ref reader)).ReadString();
									_ = 6;
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_015a;
			}
		}
		value = new InstallCampaignRecord
		{
			Matched = matched,
			MatchedSuffix = matchedSuffix,
			Scheme = scheme,
			AttributionResolved = attributionResolved,
			RawAttribution = rawAttribution,
			Campaign = campaign
		};
		return;
		IL_015a:
		value.Matched = matched;
		value.MatchedSuffix = matchedSuffix;
		value.Scheme = scheme;
		value.AttributionResolved = attributionResolved;
		value.RawAttribution = rawAttribution;
		value.Campaign = campaign;
	}
}
