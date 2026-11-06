using UnityEngine;

namespace RotateRings.Theme;

public abstract class ThemeElementBase : MonoBehaviour, IThemeElement
{
	private bool isRegistered;

	protected virtual void OnEnable()
	{
		ThemeService.Register(this);
		isRegistered = true;
		if (ThemeService.HasInitialized)
		{
			ApplyTheme(ThemeService.CurrentThemeId);
		}
	}

	protected virtual void OnDisable()
	{
		if (isRegistered)
		{
			ThemeService.Unregister(this);
			isRegistered = false;
		}
	}

	public abstract void ApplyTheme(ThemeId themeId);
}
