using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.Theme;

[RequireComponent(typeof(TMP_Text))]
public class ThemeTextElement : ThemeElementBase
{
	[SerializeField]
	private List<ThemeEntry<Color>> colors = new List<ThemeEntry<Color>>();

	private TMP_Text text;

	private void Awake()
	{
		text = ((Component)this).GetComponent<TMP_Text>();
	}

	public override void ApplyTheme(ThemeId themeId)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)text == (Object)null))
		{
			Color? val = ResolveColor(colors, themeId);
			if (val.HasValue)
			{
				((Graphic)text).color = val.Value;
			}
		}
	}

	private static Color? ResolveColor(List<ThemeEntry<Color>> list, ThemeId themeId)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
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
