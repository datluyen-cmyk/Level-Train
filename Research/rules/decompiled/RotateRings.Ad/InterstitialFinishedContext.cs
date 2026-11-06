namespace RotateRings.Ad;

public readonly struct InterstitialFinishedContext
{
	public string AdPosition { get; }

	public bool IsSuccess { get; }

	public bool IsAfterShow { get; }

	public InterstitialFinishedContext(string adPosition, bool isSuccess, bool isAfterShow)
	{
		AdPosition = adPosition;
		IsSuccess = isSuccess;
		IsAfterShow = isAfterShow;
	}
}
