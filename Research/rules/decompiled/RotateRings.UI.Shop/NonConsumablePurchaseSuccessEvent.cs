using XingYun.IAP.Core;

namespace RotateRings.UI.Shop;

public sealed class NonConsumablePurchaseSuccessEvent
{
	public PurchaseOrder Order { get; init; }
}
