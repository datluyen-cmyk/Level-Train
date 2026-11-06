using Game.UI;
using RotateRings.Prop;
using RotateRings.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.Tips;

public class UITips_CommonTips_ItemCell : GameWidgetView
{
	public Image ImgItem;

	public TMP_Text TxtNum;

	public GameObject gameObject
	{
		get
		{
			if (!((Object)(object)((UIBaseView)this).Behaviour != (Object)null))
			{
				return null;
			}
			return ((Component)((UIBaseView)this).Behaviour).gameObject;
		}
	}

	public void SetData(int propId, int count)
	{
		if ((Object)(object)ImgItem != (Object)null)
		{
			string iconName = PropIconService.GetIconName((EPropType)propId);
			if (!string.IsNullOrEmpty(iconName))
			{
				Sprite val = LoadAsset<Sprite>(iconName);
				if ((Object)(object)val != (Object)null)
				{
					ImgItem.sprite = val;
				}
			}
		}
		if ((Object)(object)TxtNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtNum, $"{count}");
		}
	}

	public override void Dispose()
	{
		base.Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		ImgItem = behaviour.Get<Image>("ImgItem");
		TxtNum = behaviour.Get<TMP_Text>("TxtNum");
	}
}
