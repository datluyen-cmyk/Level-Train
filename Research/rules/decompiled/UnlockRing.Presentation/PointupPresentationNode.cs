using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UnlockRing.Presentation;

public class PointupPresentationNode : PresentationNodeBase
{
	public GameObject Source { get; }

	public PointupPresentationNode(GameObject source)
	{
		Source = source;
	}

	public override UniTask ExecuteAsync(CancellationToken ct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return UniTask.CompletedTask;
	}
}
