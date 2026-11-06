using System;
using XingYun.Save;

namespace RotateRings.Theme;

[Serializable]
public class ThemeState : ISaveDefaultInitializer
{
	public int CurrentThemeId;

	public void OnCreateDefault()
	{
		CurrentThemeId = 0;
	}
}
