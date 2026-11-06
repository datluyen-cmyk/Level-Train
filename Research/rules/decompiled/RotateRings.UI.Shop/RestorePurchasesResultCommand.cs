using RotateRings.Prop;
using RotateRings.UI.Toast;
using XingYun.Ad;
using XingYun.Command;
using XingYun.Core.Singletons;
using XingYun.IAP.Core;

namespace RotateRings.UI.Shop;

public sealed class RestorePurchasesResultCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		RestorePurchasesState.Finish();
		PurchaseService.Hide("RestorePurchase");
		if (!(param is ShopVipRestoreCompletedEvent { Status: var status } shopVipRestoreCompletedEvent))
		{
			return;
		}
		if ((int)shopVipRestoreCompletedEvent.ProductType != 2 || status == null)
		{
			ToastService.Show("恢复购买失败");
			return;
		}
		if (!XingYunIAPErrorCodeHelper.IsSuccess(status.code))
		{
			ToastService.Show((status.code == 4) ? "已取消恢复购买" : "恢复购买失败");
			return;
		}
		if (!status.isVip)
		{
			ToastService.Show("没有可恢复的购买记录");
			return;
		}
		int shopItemIdFromSkuId = Singleton<StoreManager>.Instance.GetShopItemIdFromSkuId(status.skuId);
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null || !propModelProxy.NoAd)
		{
			propModelProxy?.AddProp(EPropType.RemoveAd, 1, TrackPropGainReason.ShopBuyReward, shopItemIdFromSkuId.ToString(), "restore_purchases");
		}
		AdManager ad = GameApp.Ad;
		if (ad != null)
		{
			ad.HideBanner();
		}
		ToastService.Show("已恢复购买");
	}
}
