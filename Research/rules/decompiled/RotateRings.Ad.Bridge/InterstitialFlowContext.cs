using System;

namespace RotateRings.Ad.Bridge;

public sealed class InterstitialFlowContext
{
	private readonly Action<InterstitialFlowContext> onBlocked;

	public bool IsFrontAd { get; }

	public string BlockReason { get; private set; }

	public bool IsBlocked { get; private set; }

	public InterstitialFlowContext(bool isFrontAd, Action<InterstitialFlowContext> onBlocked)
	{
		IsFrontAd = isFrontAd;
		this.onBlocked = onBlocked;
	}

	public void MarkBlocked(string reason)
	{
		if (!IsBlocked)
		{
			IsBlocked = true;
			BlockReason = reason;
			onBlocked?.Invoke(this);
		}
	}
}
