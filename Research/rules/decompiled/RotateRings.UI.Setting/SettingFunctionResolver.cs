using XingYun.Experiment;

namespace RotateRings.UI.Setting;

public static class SettingFunctionResolver
{
	public static SettingFunction Resolve()
	{
		return FunctionDataCenter.Get<SettingFunction>();
	}
}
