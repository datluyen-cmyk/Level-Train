using RotateRings.Prop;

namespace RotateRings.UI.PropItem;

public interface IPropBadgeProvider
{
	PropBadgeState? GetState(EPropType propType, int? overridePropCount = null);
}
