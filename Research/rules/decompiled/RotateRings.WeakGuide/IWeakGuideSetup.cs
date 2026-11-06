using System.Threading;
using Cysharp.Threading.Tasks;

namespace RotateRings.WeakGuide;

public interface IWeakGuideSetup
{
	UniTask InstallAsync(IWeakGuideInstallContext context, CancellationToken ct);
}
