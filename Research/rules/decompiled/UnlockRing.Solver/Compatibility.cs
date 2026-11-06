using System;

namespace UnlockRing.Solver;

internal static class Compatibility
{
	public static void ThrowIfNull(object? value, string parameterName)
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}
	}

	public static void ThrowIfNullOrWhiteSpace(string? value, string parameterName)
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value cannot be empty or whitespace.", parameterName);
		}
	}

	public static bool IsFinite(double value)
	{
		if (!double.IsNaN(value))
		{
			return !double.IsInfinity(value);
		}
		return false;
	}
}
