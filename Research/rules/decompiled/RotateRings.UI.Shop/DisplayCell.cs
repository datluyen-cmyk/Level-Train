using Script.Tools;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.UI.Shop;

public class DisplayCell : MonoBehaviour
{
	public TMP_Text txtCount;

	public Image imgIcon;

	private Image background;

	private string resGroup;

	private bool isInit;

	public void Init(int count, string spriteName, string group = null)
	{
		resGroup = group;
		if (!isInit)
		{
			isInit = true;
			SetCount(count);
			SetIcon(spriteName);
		}
	}

	public void SetCount(int count)
	{
		if ((Object)(object)txtCount != (Object)null)
		{
			txtCount.text = count.ToString();
		}
	}

	public void SetIcon(string iconName)
	{
		if (!((Object)(object)imgIcon == (Object)null) && !string.IsNullOrEmpty(iconName))
		{
			Sprite val = SpriteManager.LoadSprite(iconName, resGroup);
			if ((Object)(object)val == (Object)null)
			{
				Log.LogError("GoodsCell", "加载图片失败 location: " + iconName);
			}
			else
			{
				imgIcon.sprite = val;
			}
		}
	}

	public void Dispose()
	{
	}
}
