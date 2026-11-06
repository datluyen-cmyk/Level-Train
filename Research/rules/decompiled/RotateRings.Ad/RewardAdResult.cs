namespace RotateRings.Ad;

public readonly struct RewardAdResult
{
	public readonly bool IsSuccess;

	public readonly bool IsFinishedAd;

	private RewardAdResult(bool isSuccess, bool isFinishedAd)
	{
		IsSuccess = isSuccess;
		IsFinishedAd = isFinishedAd;
	}

	public static RewardAdResult Success(bool isFinishedAd)
	{
		return new RewardAdResult(isSuccess: true, isFinishedAd);
	}

	public static RewardAdResult Fail()
	{
		return new RewardAdResult(isSuccess: false, isFinishedAd: false);
	}
}
