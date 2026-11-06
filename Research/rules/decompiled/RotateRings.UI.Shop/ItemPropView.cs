using Example.Cfg;
using Script.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.UI.Shop;

public class ItemPropView : ShopItemView
{
	[Header("道具直购")]
	[SerializeField]
	private Image imgPropIcon;

	[SerializeField]
	private TMP_Text txtCount;

	[SerializeField]
	private TMP_Text txtPopular;

	[SerializeField]
	private GameObject goPopular;

	public override void Initialize(int shopId)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		base.Initialize(shopId);
		GameObject obj = goPopular;
		if (obj != null)
		{
			ShopConfig shopConfig = base.ShopConfig;
			obj.SetActive(shopConfig != null && shopConfig.tag > 0);
		}
		if ((Object)(object)txtPopular != (Object)null && base.ShopConfig != null)
		{
			txtPopular.text = ShopService.GetPopularContent(base.ShopConfig.tag);
		}
		if (!TryGetFirstBuyItem(out var _, out var count))
		{
			return;
		}
		if ((Object)(object)imgPropIcon != (Object)null && base.ShopConfig != null)
		{
			string giftIconName = base.ShopConfig.giftIconName;
			imgPropIcon.sprite = SpriteManager.LoadSprite(giftIconName);
			if (giftIconName == "common_item_skyrocket256" || giftIconName == "common_item_hammer256")
			{
				((Graphic)imgPropIcon).rectTransform.sizeDelta = new Vector2(170f, 162f);
			}
			else
			{
				((Graphic)imgPropIcon).rectTransform.sizeDelta = new Vector2(260f, 250f);
			}
		}
		if ((Object)(object)txtCount != (Object)null)
		{
			txtCount.text = count.ToString();
		}
	}
}
