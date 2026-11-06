namespace RotateRings.UI.Shop;

public sealed class ShopBuyResultEvent
{
	public int BuyGoodsId { get; init; }

	public int ExclusionId { get; init; }

	public bool IsSuccess { get; init; }
}
