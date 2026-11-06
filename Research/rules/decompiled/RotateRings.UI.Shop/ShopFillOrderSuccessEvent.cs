using System.Collections.Generic;

namespace RotateRings.UI.Shop;

public sealed class ShopFillOrderSuccessEvent
{
	public IReadOnlyList<ShopRewardItem> Rewards { get; init; }
}
