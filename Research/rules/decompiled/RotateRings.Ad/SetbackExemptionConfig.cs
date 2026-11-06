using System.Globalization;

namespace RotateRings.Ad;

public readonly struct SetbackExemptionConfig
{
	private const char Separator = '|';

	private const string DisabledValue = "0";

	public string Raw { get; }

	public int RestartThreshold { get; }

	public int ExitThreshold { get; }

	public bool TrackExit { get; }

	public int ExemptCount { get; }

	public static SetbackExemptionConfig Disabled { get; } = new SetbackExemptionConfig(string.Empty, 0, 0, trackExit: false, 0);

	private SetbackExemptionConfig(string raw, int restartThreshold, int exitThreshold, bool trackExit, int exemptCount)
	{
		Raw = raw;
		RestartThreshold = restartThreshold;
		ExitThreshold = exitThreshold;
		TrackExit = trackExit;
		ExemptCount = exemptCount;
	}

	public static bool TryParse(string raw, out SetbackExemptionConfig config)
	{
		config = Disabled;
		if (string.IsNullOrWhiteSpace(raw))
		{
			return false;
		}
		string text = raw.Trim();
		if (text == "0")
		{
			return false;
		}
		string[] array = text.Split('|');
		if (array.Length != 2 && array.Length != 3)
		{
			return false;
		}
		if (!TryReadPositive(array[0], out var parsed) || !TryReadPositive(array[^1], out var parsed2))
		{
			return false;
		}
		bool flag = array.Length == 3;
		int parsed3 = 0;
		if (flag && !TryReadPositive(array[1], out parsed3))
		{
			return false;
		}
		config = new SetbackExemptionConfig(text, parsed, parsed3, flag, parsed2);
		return true;
	}

	private static bool TryReadPositive(string value, out int parsed)
	{
		if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
		{
			return parsed >= 1;
		}
		return false;
	}

	public override string ToString()
	{
		if (!string.IsNullOrEmpty(Raw))
		{
			return Raw;
		}
		return "0";
	}
}
