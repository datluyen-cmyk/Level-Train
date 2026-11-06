using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.Theme;

[RequireComponent(typeof(Image))]
public class ThemeImageElement : ThemeElementBase
{
	[SerializeField]
	private List<ThemeEntry<Sprite>> sprites = new List<ThemeEntry<Sprite>>();

	private Image image;

	private void Awake()
	{
		image = ((Component)this).GetComponent<Image>();
	}

	public override void ApplyTheme(ThemeId themeId)
	{
		if (!((Object)(object)image == (Object)null))
		{
			Sprite val = ResolveSprite(themeId);
			if ((Object)(object)val != (Object)null)
			{
				image.sprite = val;
			}
		}
	}

	private Sprite ResolveSprite(ThemeId themeId)
	{
		for (int i = 0; i < sprites.Count; i++)
		{
			if (sprites[i].themeId == themeId)
			{
				return sprites[i].value;
			}
		}
		return null;
	}
}
