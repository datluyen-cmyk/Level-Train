using System.Collections.Generic;
using RotateRings.GamePlay.Services;
using RotateRings.GamePlay.Services.CharacterExpression;
using UnityEngine;

namespace UnlockRing.Presentation;

public static class PresentationTaskFactory
{
	public static IPresentationTask CreateTask(ResultDataCollector collector, GameplayManager host, SolveCauseForest forest)
	{
		if (collector == null || (Object)(object)host == (Object)null)
		{
			return null;
		}
		IReadOnlyList<IPresentationResult> readOnlyList = collector.CreateSnapshot();
		if (readOnlyList.Count == 0 && (forest == null || forest.Roots.Count == 0))
		{
			return null;
		}
		PresentationContext context = new PresentationContext(host.RingBoom, host.SolveSuccessEffect, host.LockChain);
		PresentationNodeBase rootNode = PresentationBuilder.Build(forest, context);
		return new CauseTreePresentationTask("CauseTree", rootNode, host.GetGameService<RotateRingOperationScoreFacade>(), host.GetGameService<CharacterExpressionEncourageService>(), readOnlyList);
	}
}
