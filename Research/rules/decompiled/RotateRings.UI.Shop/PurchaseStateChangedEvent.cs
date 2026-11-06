using XingYun.IAP.Core;

namespace RotateRings.UI.Shop;

public sealed class PurchaseStateChangedEvent
{
	public PurchaseState NewState { get; init; }

	public PurchaseState OldState { get; init; }

	public string OrderId { get; init; }
}
