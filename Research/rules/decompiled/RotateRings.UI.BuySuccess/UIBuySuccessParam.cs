using System.Collections.Generic;
using RotateRings.UI.Shop;

namespace RotateRings.UI.BuySuccess;

public sealed class UIBuySuccessParam
{
	public int ShopId { get; init; } = -1;

	public IReadOnlyList<ShopRewardItem> Rewards { get; init; }
}
