using RotateRings.Prop;
using RotateRings.UI.PropItem;

namespace RotateRings.UI.Game;

public sealed class SkyrocketPropBadgeProvider : IPropBadgeProvider
{
	private readonly SkyrocketPropUseFlowStrategy strategy;

	public SkyrocketPropBadgeProvider(SkyrocketPropUseFlowStrategy strategy)
	{
		this.strategy = strategy;
	}

	public PropBadgeState? GetState(EPropType propType, int? overridePropCount = null)
	{
		if (propType != EPropType.Skyrocket)
		{
			return null;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			return null;
		}
		int num;
		if (overridePropCount.HasValue)
		{
			num = overridePropCount.Value;
		}
		else
		{
			num = propModelProxy.GetPropCount(EPropType.Skyrocket) - strategy.CountInFlight(TrackPropUseReason.Stock);
			if (num < 0)
			{
				num = 0;
			}
		}
		bool hasPendingFree = !strategy.HasFreeInFlight && propModelProxy.HasPendingFreeProp(EPropType.Skyrocket);
		return new PropBadgeState(!propModelProxy.IsPropUnlock(EPropType.Skyrocket), num, hasPendingFree);
	}
}
