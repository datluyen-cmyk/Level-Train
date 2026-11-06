using System;

namespace RotateRings.WeakGuide.Data;

[Serializable]
public class GuideStepHandleData
{
	public int StepIndex;

	public string StepName;

	public string GuideTextKey;

	public string HandleId;

	public string ParamsJson;

	public float InitialDelay = 0.8f;

	public float LoopDelay = 0.8f;

	public float AfterOpenDelay = 0.15f;

	public float BeforeCloseDelay = 0.3f;

	public string OpenAnimation;

	public string CloseAnimation;
}
