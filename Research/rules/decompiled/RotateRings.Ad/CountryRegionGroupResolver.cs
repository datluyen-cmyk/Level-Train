using System.Collections.Generic;

namespace RotateRings.Ad;

public static class CountryRegionGroupResolver
{
	private static readonly HashSet<string> EuropeOceaniaCaCountries = new HashSet<string>
	{
		"NZ", "AS", "PW", "CK", "TL", "AU", "MH", "FM", "PG", "SB",
		"TV", "NR", "VU", "NC", "NF", "FJ", "PF", "PN", "KI", "TK",
		"TO", "WF", "WS", "NU", "MP", "GU", "UM", "BA", "CZ", "CY",
		"NL", "RU", "GR", "EE", "LV", "LT", "SJ", "MD", "BY", "FI",
		"AX", "UA", "MK", "HU", "BG", "AL", "PL", "RO", "XK", "PT",
		"GI", "ES", "MT", "FO", "DK", "IS", "UK", "GB", "CH", "SE",
		"AT", "BE", "DE", "LU", "IE", "MC", "FR", "AD", "LI", "JE",
		"IM", "GG", "SK", "NO", "VA", "SM", "IT", "SI", "ME", "HR",
		"RS", "CA"
	};

	private static readonly HashSet<string> UsCountries = new HashSet<string> { "US" };

	private static readonly HashSet<string> JapanKoreaTaiwanCountries = new HashSet<string> { "TW", "JP", "KR" };

	private static readonly HashSet<string> SouthAmericaAfricaMxCountries = new HashSet<string>
	{
		"DO", "SV", "PR", "BB", "PM", "GL", "JM", "CU", "MQ", "BS",
		"BM", "AI", "TT", "KN", "DM", "AG", "LC", "TC", "AW", "VG",
		"VC", "MS", "MF", "BL", "GP", "GD", "KY", "BZ", "GT", "HN",
		"NI", "CR", "PA", "HT", "VI", "SX", "CW", "BQ", "MX", "TF",
		"GS", "AQ", "AR", "GY", "GF", "SR", "PY", "UY", "BR", "FK",
		"VE", "EC", "CO", "CL", "BO", "PE", "RW", "SO", "TZ", "KE",
		"CD", "DJ", "UG", "CF", "SC", "ET", "ER", "EG", "SD", "BI",
		"ZW", "ZM", "KM", "MW", "LS", "BW", "MU", "SZ", "RE", "ZA",
		"YT", "MZ", "MG", "LY", "CM", "SN", "CG", "LR", "CI", "GH",
		"GQ", "NG", "BF", "TG", "GW", "MR", "BJ", "GA", "SL", "ST",
		"GM", "GN", "TD", "NE", "ML", "TN", "MA", "DZ", "AO", "NA",
		"SH", "CV", "SS"
	};

	private static readonly HashSet<string> AsiaExcludingJktCountries = new HashSet<string>
	{
		"AF", "AM", "AZ", "BH", "BD", "BT", "BN", "KH", "KP", "IN",
		"ID", "IR", "IQ", "IL", "JO", "KW", "LA", "LB", "MY", "MV",
		"MN", "MM", "NP", "OM", "PK", "PS", "PH", "QA", "SA", "SG",
		"LK", "SY", "TJ", "TH", "TR", "TM", "AE", "UZ", "VN", "YE"
	};

	public static string ResolveEffectiveRegionGroup()
	{
		string text = CountryGdpGroupResolver.ResolveEffectiveCountryCode();
		if (!string.IsNullOrEmpty(text))
		{
			return Resolve(text);
		}
		return "其他";
	}

	public static string Resolve(string countryCode)
	{
		if (string.IsNullOrEmpty(countryCode))
		{
			return "其他";
		}
		countryCode = countryCode.ToUpperInvariant();
		if (EuropeOceaniaCaCountries.Contains(countryCode))
		{
			return "欧洲组 + 大洋洲 + CA";
		}
		if (UsCountries.Contains(countryCode))
		{
			return "US";
		}
		if (JapanKoreaTaiwanCountries.Contains(countryCode))
		{
			return "日韩台";
		}
		if (SouthAmericaAfricaMxCountries.Contains(countryCode))
		{
			return "南美 + 非洲 + MX";
		}
		if (AsiaExcludingJktCountries.Contains(countryCode))
		{
			return "亚洲除日韩台";
		}
		return "其他";
	}
}
