using XingYun.Utility;

public static class StringParseExtensions
{
	public static double ToDouble(this string str, double defaultValue = 0.0)
	{
		if (string.IsNullOrEmpty(str))
		{
			return defaultValue;
		}
		double result = default(double);
		if (Parser.TryParse(str, ref result))
		{
			return result;
		}
		return defaultValue;
	}

	public static float ToFloat(this string str, float defaultValue = 0f)
	{
		if (string.IsNullOrEmpty(str))
		{
			return defaultValue;
		}
		float result = default(float);
		if (Parser.TryParse(str, ref result))
		{
			return result;
		}
		return defaultValue;
	}

	public static int ToInt(this string str, int defaultValue = 0)
	{
		if (string.IsNullOrEmpty(str))
		{
			return defaultValue;
		}
		int result = default(int);
		if (Parser.TryParse(str, ref result))
		{
			return result;
		}
		return defaultValue;
	}

	public static long ToLong(this string str, long defaultValue = 0L)
	{
		if (string.IsNullOrEmpty(str))
		{
			return defaultValue;
		}
		long result = default(long);
		if (Parser.TryParse(str, ref result))
		{
			return result;
		}
		return defaultValue;
	}
}
