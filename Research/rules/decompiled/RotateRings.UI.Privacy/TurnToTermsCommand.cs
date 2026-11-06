using XingYun.Aot;
using XingYun.Command;

namespace RotateRings.UI.Privacy;

public class TurnToTermsCommand : BaseCommand
{
	public override void Execute(object param = null)
	{
		SDKManager.ShowTermsOfService();
	}
}
