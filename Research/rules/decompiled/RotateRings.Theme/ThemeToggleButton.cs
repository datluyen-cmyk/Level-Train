using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RotateRings.Theme;

public class ThemeToggleButton : ThemeElementBase
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private List<ThemeEntry<Sprite>> iconSprites = new List<ThemeEntry<Sprite>>();

	protected override void OnEnable()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		base.OnEnable();
		if ((Object)(object)button != (Object)null)
		{
			((UnityEvent)button.onClick).RemoveListener(new UnityAction(OnClick));
			((UnityEvent)button.onClick).AddListener(new UnityAction(OnClick));
		}
	}

	protected override void OnDisable()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if ((Object)(object)button != (Object)null)
		{
			((UnityEvent)button.onClick).RemoveListener(new UnityAction(OnClick));
		}
		base.OnDisable();
	}

	private void OnClick()
	{
		ThemeService.CycleNextTheme();
	}

	public override void ApplyTheme(ThemeId themeId)
	{
		if (!((Object)(object)iconImage == (Object)null))
		{
			Sprite val = ResolveSprite(iconSprites, themeId);
			if ((Object)(object)val != (Object)null)
			{
				iconImage.sprite = val;
			}
		}
	}

	private static Sprite ResolveSprite(List<ThemeEntry<Sprite>> list, ThemeId themeId)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].themeId == themeId)
			{
				return list[i].value;
			}
		}
		return null;
	}
}
