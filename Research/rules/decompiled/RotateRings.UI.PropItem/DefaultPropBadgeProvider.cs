using RotateRings.Prop;

namespace RotateRings.UI.PropItem;

public sealed class DefaultPropBadgeProvider : IPropBadgeProvider
{
	public PropBadgeState? GetState(EPropType propType, int? overridePropCount = null)
	{
		if (propType == EPropType.None)
		{
			return null;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy == null)
		{
			return null;
		}
		return new PropBadgeState(!propModelProxy.IsPropUnlock(propType), overridePropCount ?? propModelProxy.GetPropCount(propType), propModelProxy.HasPendingFreeProp(propType));
	}
}
