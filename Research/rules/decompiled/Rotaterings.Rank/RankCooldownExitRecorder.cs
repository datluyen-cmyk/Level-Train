using UnityEngine;

namespace Rotaterings.Rank;

[DefaultExecutionOrder(-1000)]
public sealed class RankCooldownExitRecorder : MonoBehaviour
{
	private static bool isInitialized;

	private bool isApplicationBackground;

	public static void EnsureInitialized()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (!isInitialized)
		{
			isInitialized = true;
			GameObject val = new GameObject("[RankCooldownExitRecorder]");
			Object.DontDestroyOnLoad((Object)val);
			val.AddComponent<RankCooldownExitRecorder>();
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		UpdateApplicationBackgroundState(pauseStatus);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		UpdateApplicationBackgroundState(!hasFocus);
	}

	private void OnApplicationQuit()
	{
		RecordCooldownExitTimestamp();
	}

	private void UpdateApplicationBackgroundState(bool isBackground)
	{
		if (isApplicationBackground != isBackground)
		{
			isApplicationBackground = isBackground;
			if (isBackground)
			{
				RecordCooldownExitTimestamp();
			}
		}
	}

	private static void RecordCooldownExitTimestamp()
	{
		RankActivityEntry.Service?.RecordCooldownExitTimestamp();
	}
}
