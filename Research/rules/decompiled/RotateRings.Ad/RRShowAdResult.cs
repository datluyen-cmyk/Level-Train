namespace RotateRings.Ad;

public class RRShowAdResult
{
	public bool IsSuccess { get; }

	public string Message { get; }

	public string AdPosition { get; }

	public bool IsGuarantee { get; }

	public bool IsFinishedAd { get; }

	public RRShowAdResult(bool isSuccess, string message, string adPosition, bool isGuarantee = false, bool isFinishedAd = false)
	{
		IsSuccess = isSuccess;
		Message = message;
		AdPosition = adPosition;
		IsGuarantee = isGuarantee;
		IsFinishedAd = isFinishedAd;
	}

	public RewardAdResult ToRewardAdResult()
	{
		if (!IsSuccess)
		{
			return RewardAdResult.Fail();
		}
		return RewardAdResult.Success(IsFinishedAd);
	}
}
