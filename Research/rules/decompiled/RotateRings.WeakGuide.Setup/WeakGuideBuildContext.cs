using System.Threading;

namespace RotateRings.WeakGuide.Setup;

public sealed class WeakGuideBuildContext
{
	public IWeakGuideInstallContext InstallContext { get; }

	public GameplayManager GameplayManager { get; }

	public WeakGuideManager Manager { get; }

	public GuideStep Step { get; set; }

	public WeakGuideGlobalConfig GlobalConfig { get; }

	public CancellationToken InstallToken { get; }

	public WeakGuideBuildContext(IWeakGuideInstallContext installContext, WeakGuideManager manager, WeakGuideGlobalConfig globalConfig, CancellationToken installToken = default(CancellationToken))
	{
		InstallContext = installContext;
		GameplayManager = installContext?.GameplayManager;
		Manager = manager;
		GlobalConfig = globalConfig;
		InstallToken = installToken;
	}
}
