using System.Collections.Generic;
using XingYun.IAP.Core;

namespace RotateRings.UI.Shop;

public sealed class SkuDetailsUpdatedEvent
{
	public List<XingYunSkuDetailItem> SkuDetails { get; init; }
}
