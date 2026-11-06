using System;
using System.Collections.Generic;
using RotateRings.WeakGuide.StepHandles.Handles;

namespace RotateRings.WeakGuide.StepHandles;

public static class WeakGuideStepHandleRegistry
{
	private const string TAG = "[WeakGuide][HandleRegistry] ";

	private static readonly Dictionary<string, IWeakGuideStepHandle> Map = new Dictionary<string, IWeakGuideStepHandle>(StringComparer.OrdinalIgnoreCase);

	private static bool defaultsRegistered;

	public static IEnumerable<IWeakGuideStepHandle> AllHandles => Map.Values;

	public static void Register(IWeakGuideStepHandle handle)
	{
		if (handle != null && !string.IsNullOrEmpty(handle.HandleId))
		{
			Map[handle.HandleId] = handle;
		}
	}

	public static void RegisterDefaults()
	{
		if (!defaultsRegistered)
		{
			defaultsRegistered = true;
			Register(new RotateRingStepHandle());
		}
	}

	public static IWeakGuideStepHandle GetByName(string handleId)
	{
		if (string.IsNullOrEmpty(handleId))
		{
			return null;
		}
		Map.TryGetValue(handleId, out var value);
		return value;
	}

	public static void Clear()
	{
		Map.Clear();
		defaultsRegistered = false;
	}
}
