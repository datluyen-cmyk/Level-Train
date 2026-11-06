using System.Threading;
using Cysharp.Threading.Tasks;

namespace UnlockRing.Presentation;

public interface IPresentationTask
{
	string DebugName { get; }

	PresentationTaskKind Kind { get; }

	UniTask RunAsync(CancellationToken ct);
}
