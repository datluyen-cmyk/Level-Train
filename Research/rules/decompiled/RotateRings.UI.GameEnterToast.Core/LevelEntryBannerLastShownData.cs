using System;
using XingYun.Save;

namespace RotateRings.UI.GameEnterToast.Core;

[Serializable]
public class LevelEntryBannerLastShownData : ISaveDefaultInitializer
{
	public string LastSimpleKey;

	public string LastHardKey;

	public string LastShapeKey;

	public void OnCreateDefault()
	{
		LastSimpleKey = string.Empty;
		LastHardKey = string.Empty;
		LastShapeKey = string.Empty;
	}
}
