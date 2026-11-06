namespace RotateRings.GamePlay.MultiTouchGuide;

public readonly struct MultiTouchGuideStepPolicy
{
	public MultiTouchGuideStep ExpectedStep { get; }

	public bool AllowZoom { get; }

	public bool AllowPan { get; }

	public MultiTouchGuideStepPolicy(MultiTouchGuideStep expectedStep, bool allowZoom, bool allowPan)
	{
		ExpectedStep = expectedStep;
		AllowZoom = allowZoom;
		AllowPan = allowPan;
	}

	public static MultiTouchGuideStepPolicy ForStep(MultiTouchGuideStep step)
	{
		switch (step)
		{
		case MultiTouchGuideStep.ZoomIn:
		case MultiTouchGuideStep.ZoomOut:
			return new MultiTouchGuideStepPolicy(step, allowZoom: true, allowPan: false);
		case MultiTouchGuideStep.Pan:
			return new MultiTouchGuideStepPolicy(step, allowZoom: false, allowPan: true);
		default:
			return new MultiTouchGuideStepPolicy(step, allowZoom: false, allowPan: false);
		}
	}
}
