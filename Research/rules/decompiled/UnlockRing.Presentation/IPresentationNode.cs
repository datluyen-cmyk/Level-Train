using System.Threading;
using Cysharp.Threading.Tasks;

namespace UnlockRing.Presentation;

public interface IPresentationNode
{
	bool Excute();

	UniTask ExecuteAsync(CancellationToken ct);
}
