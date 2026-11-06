using Cysharp.Threading.Tasks;
using Example.Cfg;
using RotateRings.UI.Toast;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.IAP.SDK;

namespace RotateRings.UI.Shop;

public sealed class ShopBuyCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetBuyParam(param, out var shopItemId, out var enterPath))
		{
			PurchaseService.Hide("ShopPurchase");
			return;
		}
		ShopConfig shopConfig = GameApp.Config.GetTable<ExampleTables>()?.TbShopConfig?.GetOrDefault(shopItemId);
		if (shopConfig == null)
		{
			ToastService.Show("商品暂不可用");
			PurchaseService.Hide("ShopPurchase");
		}
		else if (!TryHandleMockPurchase(shopItemId, shopConfig))
		{
			UniTaskVoid val;
			if (shopConfig.NonConsumption)
			{
				val = Singleton<StoreManager>.Instance.BuyVIPShop(shopItemId, (IAPProductType)2, CreateLinkUniqueId(enterPath));
				((UniTaskVoid)(ref val)).Forget();
			}
			else
			{
				val = Singleton<StoreManager>.Instance.BuyShop(shopItemId, 1, CreateLinkUniqueId(enterPath));
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	private static bool TryHandleMockPurchase(int shopItemId, ShopConfig config)
	{
		return false;
	}

	private static bool TryGetBuyParam(object param, out int shopItemId, out EIapEnterPath enterPath)
	{
		if (param is ShopBuyParam shopBuyParam)
		{
			shopItemId = shopBuyParam.ShopItemId;
			enterPath = ((shopBuyParam.EnterPath == (EIapEnterPath)0) ? EIapEnterPath.HomeEntry : shopBuyParam.EnterPath);
			return shopItemId > 0;
		}
		if (param is int num)
		{
			shopItemId = num;
			enterPath = EIapEnterPath.HomeEntry;
			return shopItemId > 0;
		}
		shopItemId = 0;
		enterPath = EIapEnterPath.HomeEntry;
		return false;
	}

	private static string CreateLinkUniqueId(EIapEnterPath enterPath)
	{
		IAPTrackerProvider.IAPTrackingContext context = Singleton<IAPTrackerProvider>.Instance.CreateContext(enterPath);
		return Singleton<IAPTrackerProvider>.Instance.AllocateLinkUniqueId(context);
	}
}
