using System.Collections.Generic;
using System.Diagnostics;
using UniLog;

namespace RotateRings.Common;

public static class LockUIService
{
	private const string Tag = "LockUI";

	private static int lockCount;

	private static readonly List<string> lockSources = new List<string>();

	public static int CurrentLockCount => lockCount;

	public static List<string> GetPendingSources()
	{
		return new List<string>(lockSources);
	}

	public static void ForceRelease(string reason)
	{
		if (lockCount > 0)
		{
			lockCount = 0;
			lockSources.Clear();
			GameApp.Event.Send<EventLockUI>(new EventLockUI
			{
				lockUI = false
			});
		}
	}

	public static void Enable(float duration = 10f)
	{
		lockCount++;
		StackFrame frame = new StackTrace(1, fNeedFileInfo: true).GetFrame(0);
		string item = $"{frame.GetFileName()}:{frame.GetFileLineNumber()} {frame.GetMethod().Name}";
		lockSources.Add(item);
		_ = lockCount;
		_ = 1;
		GameApp.Event.Send<EventLockUI>(new EventLockUI
		{
			lockUI = true,
			durationTime = duration
		});
	}

	public static void Disable()
	{
		StackFrame frame = new StackTrace(1, fNeedFileInfo: true).GetFrame(0);
		string text = $"{frame.GetFileName()}:{frame.GetFileLineNumber()} {frame.GetMethod().Name}";
		if (lockCount <= 0)
		{
			Log.LogError("LockUI", "Disable called without matching Enable! Ignored, from: {0}", new object[1] { text });
			return;
		}
		lockCount--;
		if (lockCount == 0)
		{
			lockSources.Clear();
		}
		GameApp.Event.Send<EventLockUI>(new EventLockUI
		{
			lockUI = false
		});
	}

	public static void LogStatus()
	{
		_ = lockCount;
	}
}
