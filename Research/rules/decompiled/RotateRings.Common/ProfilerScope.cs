using System;
using System.Runtime.InteropServices;

namespace RotateRings.Common;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct ProfilerScope : IDisposable
{
	public ProfilerScope(string sampleName)
	{
	}

	public static ProfilerScope Begin(string sampleName)
	{
		return new ProfilerScope(sampleName);
	}

	public void Dispose()
	{
	}
}
