using System;

namespace RotateRings.Ad;

public class UIAdPopParam
{
	public AdPopPosition PopPosition { get; set; }

	public Action<RewardAdResult> OnAdResult { get; set; }

	public Action OnPopClosed { get; set; }
}
