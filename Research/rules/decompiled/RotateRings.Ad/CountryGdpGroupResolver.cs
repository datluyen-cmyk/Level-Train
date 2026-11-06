using System.Collections.Generic;
using XingYun.Aot;
using XingYun.Tag;

namespace RotateRings.Ad;

public static class CountryGdpGroupResolver
{
	public const string SdkInactiveDefaultCountryCode = "US";

	private static readonly HashSet<string> PremiumWestCountries = new HashSet<string> { "US", "CA" };

	private static readonly HashSet<string> PremiumOceaniaCountries = new HashSet<string> { "AU", "NZ" };

	private static readonly HashSet<string> PremiumAsiaCountries = new HashSet<string> { "JP", "KR", "SG", "TW" };

	private static readonly HashSet<string> PremiumEuropeCountries = new HashSet<string>
	{
		"DE", "GB", "FR", "IT", "ES", "CH", "SE", "NL", "IE", "AT",
		"BE", "DK", "FI", "NO", "IS", "LU"
	};

	private static readonly HashSet<string> PremiumMideastCountries = new HashSet<string> { "AE", "QA", "KW", "SA", "BH", "OM", "IL" };

	private static readonly HashSet<string> GrowthEuropeCountries = new HashSet<string> { "PL", "HU", "CZ", "GR", "PT", "RO", "BG", "HR", "ZA" };

	private static readonly HashSet<string> GrowthEurasiaCountries = new HashSet<string> { "CN", "RU", "KZ", "TR", "UA" };

	private static readonly HashSet<string> GrowthLatinCountries = new HashSet<string> { "BR", "MX", "CL", "AR", "CR", "PA", "UY", "CO", "PE" };

	private static readonly HashSet<string> EmergingSeaCountries = new HashSet<string> { "MY", "TH" };

	private static readonly HashSet<string> DevelopingSeaCountries = new HashSet<string> { "ID", "PH", "VN" };

	private static readonly HashSet<string> DevelopingSouthasiaCountries = new HashSet<string> { "IN", "LK", "BD", "PK", "NP", "AF" };

	private static readonly HashSet<string> DevelopingAfricaLatinCountries = new HashSet<string>
	{
		"EG", "MA", "DZ", "TN", "YE", "NG", "KE", "GH", "AO", "CU",
		"DO", "SV", "GT", "HN", "NI", "ET", "TZ", "UG", "RW", "BI",
		"CD", "MZ", "MW", "ZM", "MG", "SN", "ML", "NE", "BF", "GN",
		"LR", "SL"
	};

	public static string ResolveEffectiveCountryCode()
	{
		TagManager tag = GameApp.Tag;
		string text = ((tag != null) ? tag.NetworkCountry : null) ?? string.Empty;
		string text2 = ((tag != null) ? tag.SimCountry : null) ?? string.Empty;
		if (!string.IsNullOrEmpty(text))
		{
			return text.ToUpperInvariant();
		}
		if (!string.IsNullOrEmpty(text2))
		{
			return text2.ToUpperInvariant();
		}
		if (ShouldUseSdkInactiveCountryDefault())
		{
			return "US";
		}
		return string.Empty;
	}

	public static string ResolveEffectiveGdpGroup()
	{
		string text = ResolveEffectiveCountryCode();
		if (!string.IsNullOrEmpty(text))
		{
			return Resolve(text);
		}
		TagManager tag = GameApp.Tag;
		return ((tag != null) ? tag.CountryGdpGroup : null) ?? "other_region";
	}

	public static bool ShouldUseSdkInactiveCountryDefault()
	{
		return !Define.IsSdkBuild;
	}

	public static void ApplyEffectiveCountryToTag()
	{
		TagManager tag = GameApp.Tag;
		if (tag != null)
		{
			string text = ResolveEffectiveGdpGroup();
			tag.SetTag<string>("country_gdp_group", text, (ReportTimeType)2);
		}
	}

	public static string Resolve(string countryCode)
	{
		if (string.IsNullOrEmpty(countryCode))
		{
			return "other_region";
		}
		countryCode = countryCode.ToUpperInvariant();
		string result = "other_region";
		if (PremiumWestCountries.Contains(countryCode))
		{
			result = "premium_west";
		}
		else if (PremiumOceaniaCountries.Contains(countryCode))
		{
			result = "premium_oceania";
		}
		else if (PremiumAsiaCountries.Contains(countryCode))
		{
			result = "premium_asia";
		}
		else if (PremiumEuropeCountries.Contains(countryCode))
		{
			result = "premium_europe";
		}
		else if (PremiumMideastCountries.Contains(countryCode))
		{
			result = "premium_mideast";
		}
		else if (GrowthEuropeCountries.Contains(countryCode))
		{
			result = "growth_europe";
		}
		else if (GrowthEurasiaCountries.Contains(countryCode))
		{
			result = "growth_eurasia";
		}
		else if (GrowthLatinCountries.Contains(countryCode))
		{
			result = "growth_latin";
		}
		else if (EmergingSeaCountries.Contains(countryCode))
		{
			result = "emerging_sea";
		}
		else if (DevelopingSeaCountries.Contains(countryCode))
		{
			result = "developing_sea";
		}
		else if (DevelopingSouthasiaCountries.Contains(countryCode))
		{
			result = "developing_southasia";
		}
		else if (DevelopingAfricaLatinCountries.Contains(countryCode))
		{
			result = "developing_africa_latin";
		}
		return result;
	}
}
