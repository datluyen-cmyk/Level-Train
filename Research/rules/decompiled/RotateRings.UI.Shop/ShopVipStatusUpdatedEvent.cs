using XingYun.IAP.Core;
using XingYun.IAP.SDK;

namespace RotateRings.UI.Shop;

public sealed class ShopVipStatusUpdatedEvent
{
	public IAPProductType ProductType { get; init; }

	public XingYunVIPStatusResponse Status { get; init; }
}
