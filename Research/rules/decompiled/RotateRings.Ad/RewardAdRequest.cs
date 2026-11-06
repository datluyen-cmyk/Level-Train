using System;

namespace RotateRings.Ad;

public class RewardAdRequest
{
	public AdPopPosition PopPosition { get; set; }

	public Action<RewardAdResult> Callback { get; set; }
}
