using UniLog;

namespace RotateRings.Services;

public sealed class UniLogAudioForgeSink : IAudioForgeLogSink
{
	public void Log(AudioForgeLogLevel level, string tag, string message)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		switch ((int)level)
		{
		case 2:
			Log.LogError(tag, message);
			break;
		case 0:
		case 1:
			break;
		}
	}
}
