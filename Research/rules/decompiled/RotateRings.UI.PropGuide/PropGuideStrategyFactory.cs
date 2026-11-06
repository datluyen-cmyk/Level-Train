using RotateRings.Prop;

namespace RotateRings.UI.PropGuide;

public static class PropGuideStrategyFactory
{
	public static IPropGuideStrategy Create(EPropType propType)
	{
		return propType switch
		{
			EPropType.Hammer => new HammerGuideStrategy(), 
			EPropType.Skyrocket => new SkyrocketGuideStrategy(), 
			_ => null, 
		};
	}
}
