using System.Collections.Generic;
using Example.Cfg;
using RotateRings.Prop;
using RotateRings.Services;
using Script.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.IAP.Core;

namespace RotateRings.UI.Shop;

public class ShopItemView : MonoBehaviour
{
	private const string LogTag = "ShopItemView";

	[Header("公共引用")]
	[SerializeField]
	protected Button btnBuy;

	[SerializeField]
	protected TMP_Text txtTitleName;

	[SerializeField]
	protected TMP_Text txtSellPrice;

	[SerializeField]
	private CanvasGroup canvasGroup;

	protected string skuId;

	public int order;

	public int ShopGoodsId { get; private set; }

	public ShopConfig ShopConfig { get; private set; }

	public ShopBaseGoods BaseGoods { get; private set; }

	public EIapEnterPath EnterPath { get; set; } = EIapEnterPath.HomeEntry;

	public virtual void Initialize(int shopId)
	{
		StopAnimation();
		ShopGoodsId = shopId;
		ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
		ShopConfig = table?.TbShopConfig?.GetOrDefault(shopId);
		BaseGoods = table?.TbShopBaseGoods?.GetOrDefault(shopId);
		if (ShopConfig != null && BaseGoods != null)
		{
			BindBuyButton();
			SetCanvasGroupAlpha(1f);
			skuId = ShopService.GetSkuId(BaseGoods);
			RefreshSellPrice();
		}
	}

	public void RefreshSellPrice()
	{
		if (!((Object)(object)txtSellPrice == (Object)null) && ShopConfig != null)
		{
			XingYunSkuDetailItem skuDetailItem = ShopService.GetSkuDetailItem(skuId);
			if (skuDetailItem != null)
			{
				txtSellPrice.text = ShopService.FormatCurrencyWithSpace(skuDetailItem.formattedPrice);
			}
			else
			{
				txtSellPrice.text = ShopConfig.price;
			}
		}
	}

	public void SetCanvasGroupAlpha(float alpha)
	{
		EnsureCanvasGroup();
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = alpha;
		}
	}

	public void PlayAppearAnim(string animName)
	{
		SetCanvasGroupAlpha(1f);
		PlayAnimation(animName);
	}

	public void PlayCloseAnim(string animName)
	{
		PlayAnimation(animName);
	}

	private void PlayAnimation(string animName)
	{
		Animation component = ((Component)this).GetComponent<Animation>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)component.GetClip(animName) == (Object)null))
		{
			component.Stop();
			component.Rewind(animName);
			component.Play(animName);
		}
	}

	private void StopAnimation()
	{
		Animation component = ((Component)this).GetComponent<Animation>();
		if (!((Object)(object)component == (Object)null))
		{
			component.Stop();
			component.Rewind();
		}
	}

	protected virtual void OnBuyClick()
	{
		PurchaseService.RequestPurchase(ShopGoodsId, EnterPath);
	}

	protected bool TryGetFirstBuyItem(out EPropType propType, out int count)
	{
		propType = EPropType.None;
		count = 0;
		if (BaseGoods?.BuyItems == null || BaseGoods.BuyItems.Count == 0)
		{
			return false;
		}
		using (Dictionary<int, int>.Enumerator enumerator = BaseGoods.BuyItems.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				KeyValuePair<int, int> current = enumerator.Current;
				propType = (EPropType)(((int?)GameApp.Proxy.Get<PropModelProxy>()?.GetRewardPropType(current.Key)) ?? current.Key);
				count = current.Value;
				return true;
			}
		}
		return false;
	}

	protected Sprite LoadPropIcon(EPropType propType, bool large)
	{
		string text = (large ? PropIconService.GetIconNameLarge(propType) : PropIconService.GetIconName(propType));
		if (!string.IsNullOrEmpty(text))
		{
			return SpriteManager.LoadSprite(text);
		}
		return null;
	}

	public virtual void Dispose()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if ((Object)(object)btnBuy != (Object)null)
		{
			((UnityEvent)btnBuy.onClick).RemoveListener(new UnityAction(OnBuyClick));
		}
	}

	private void BindBuyButton()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		if ((Object)(object)btnBuy != (Object)null)
		{
			((UnityEvent)btnBuy.onClick).RemoveListener(new UnityAction(OnBuyClick));
			((UnityEvent)btnBuy.onClick).AddListener(new UnityAction(OnBuyClick));
		}
	}

	private void EnsureCanvasGroup()
	{
		if (!((Object)(object)canvasGroup != (Object)null))
		{
			canvasGroup = ((Component)this).GetComponent<CanvasGroup>() ?? ((Component)this).gameObject.AddComponent<CanvasGroup>();
		}
	}
}
