using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

public static class DebugNdjsonLog
{
	private const string DEFAULT_LOG_PATH = "/Users/admin/Documents/WorkSpace_Fork/xy_rotaterings_unity/01_Project/.cursor/debug-d708cc.log";

	private const string SESSION_ID = "d708cc";

	private static string currentRunId = "initial-debug";

	private static string currentLogPath = "/Users/admin/Documents/WorkSpace_Fork/xy_rotaterings_unity/01_Project/.cursor/debug-d708cc.log";

	private static int bindCallCounter;

	private static int lateUpdateCounter;

	public static void SetRunId(string runId)
	{
		currentRunId = (string.IsNullOrEmpty(runId) ? "run" : runId);
	}

	public static void SetLogPath(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			currentLogPath = path;
		}
	}

	public static void ResetBindCounters()
	{
		bindCallCounter = 0;
		lateUpdateCounter = 0;
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEBUG_MODE_LOGGING")]
	public static void Write(string location, string message, string dataJson = "null", string hypothesisId = "DEBUG")
	{
		try
		{
			long num = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			string text = "log_" + num + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
			string text2 = EscapeJsonString(message);
			string text3 = EscapeJsonString(location);
			string contents = "{\"sessionId\":\"d708cc\",\"id\":\"" + text + "\",\"timestamp\":" + num + ",\"location\":\"" + text3 + "\",\"message\":\"" + text2 + "\",\"data\":" + (dataJson ?? "null") + ",\"runId\":\"" + currentRunId + "\",\"hypothesisId\":\"" + hypothesisId + "\"}\n";
			File.AppendAllText(currentLogPath, contents);
		}
		catch
		{
		}
	}

	public static int NextBindTicket()
	{
		return Interlocked.Increment(ref bindCallCounter);
	}

	public static int NextLateUpdateTicket()
	{
		return Interlocked.Increment(ref lateUpdateCounter);
	}

	private static string EscapeJsonString(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")
			.Replace("\r", "\\r");
	}
}
