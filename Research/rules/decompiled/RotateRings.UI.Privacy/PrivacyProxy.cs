using RotateRings.UI.Privacy.Model;
using XingYun.Proxy;

namespace RotateRings.UI.Privacy;

public class PrivacyProxy : BaseProxy
{
	private PrivacySave save;

	public PrivacyProxy()
		: base("PrivacyProxy")
	{
		save = GameApp.Save.GetOrCreate<PrivacySave>();
	}

	public bool HasAgreedStartupPrivacy()
	{
		return save.HasAgreedStartupPrivacy;
	}

	public void MarkStartupPrivacyAgreed()
	{
		if (!save.HasAgreedStartupPrivacy)
		{
			save.HasAgreedStartupPrivacy = true;
		}
	}
}
