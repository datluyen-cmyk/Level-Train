using System;

namespace RotateRings.Theme;

[Serializable]
public class ThemeEntry<T>
{
	public ThemeId themeId;

	public T value;
}
