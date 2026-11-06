using System;
using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Prop;
using RotateRings.Services;
using TMPro;
using UnityEngine;

namespace RotateRings.UI.Shop;

public class ItemSpecialOfferView : ShopItemView
{
	[Header("专有")]
	[SerializeField]
	private DisplayCell itemTemp;

	[SerializeField]
	private TMP_Text txtDiscount;

	[SerializeField]
	private GameObject goDiscount;

	private readonly List<DisplayCell> displayList = new List<DisplayCell>();

	public override void Initialize(int shopId)
	{
		base.Initialize(shopId);
		if ((Object)(object)txtTitleName != (Object)null)
		{
			txtTitleName.text = GameApp.Lang.GetText("Shop_Pack_SpecialOffer", Array.Empty<object>());
		}
		ShopConfig shopConfig = base.ShopConfig;
		bool flag = shopConfig != null && shopConfig.discount > 0;
		GameObject obj = goDiscount;
		if (obj != null)
		{
			obj.SetActive(flag);
		}
		if (flag && (Object)(object)txtDiscount != (Object)null)
		{
			txtDiscount.text = $"{base.ShopConfig.discount}%";
		}
		CreateDisplayList();
	}

	private void CreateDisplayList()
	{
		if ((Object)(object)itemTemp == (Object)null || base.BaseGoods?.BuyItems == null || base.BaseGoods.BuyItems.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> buyItem in base.BaseGoods.BuyItems)
		{
			EPropType key = (EPropType)buyItem.Key;
			int value = buyItem.Value;
			GameObject val = Object.Instantiate<GameObject>(((Component)itemTemp).gameObject, ((Component)itemTemp).transform.parent);
			if ((Object)(object)val != (Object)null)
			{
				DisplayCell component = val.GetComponent<DisplayCell>();
				string iconName = PropIconService.GetIconName(key);
				component.Init(value, iconName, "DisplayCell");
				displayList.Add(component);
				val.SetActive(true);
				((Component)itemTemp).gameObject.SetActive(false);
			}
		}
	}

	public override void Dispose()
	{
		foreach (DisplayCell display in displayList)
		{
			display.Dispose();
			Object.Destroy((Object)(object)((Component)display).gameObject);
		}
		displayList.Clear();
		base.Dispose();
	}
}
