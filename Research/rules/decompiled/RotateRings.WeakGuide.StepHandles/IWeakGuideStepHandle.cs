using System;
using Cysharp.Threading.Tasks;
using RotateRings.WeakGuide.Setup;

namespace RotateRings.WeakGuide.StepHandles;

public interface IWeakGuideStepHandle
{
	string HandleId { get; }

	string HandleName { get; }

	Type ParametersType { get; }

	string DefaultParametersJson { get; }

	UniTask<GuideStep> BuildGuideStepAsync(WeakGuideBuildContext ctx, string paramsJson);

	void OnStepStart(WeakGuideManager manager, GuideStep step);

	void OnStepFinish(WeakGuideManager manager, GuideStep step);

	bool TryFinishStep(WeakGuideStepTrigger trigger, object args);
}
