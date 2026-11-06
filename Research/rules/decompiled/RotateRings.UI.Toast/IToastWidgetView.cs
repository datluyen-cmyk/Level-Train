using System.Threading;
using Cysharp.Threading.Tasks;

namespace RotateRings.UI.Toast;

public interface IToastWidgetView
{
	UniTask Show(string content, float duration, CancellationToken cancellationToken = default(CancellationToken));

	UniTask Hide(CancellationToken cancellationToken = default(CancellationToken));

	void CancelShow();
}
