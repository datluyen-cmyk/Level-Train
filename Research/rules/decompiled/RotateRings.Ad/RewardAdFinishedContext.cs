using XingYun.Ad;

namespace RotateRings.Ad;

public readonly struct RewardAdFinishedContext
{
	public IAdConfiguration Config { get; }

	public string AdPosition { get; }

	public bool IsSuccess { get; }

	public bool IsFinished { get; }

	public int Level { get; }

	public RewardAdFinishedContext(IAdConfiguration config, string adPosition, bool isSuccess, bool isFinished, int level)
	{
		Config = config;
		AdPosition = adPosition;
		IsSuccess = isSuccess;
		IsFinished = isFinished;
		Level = level;
	}
}
