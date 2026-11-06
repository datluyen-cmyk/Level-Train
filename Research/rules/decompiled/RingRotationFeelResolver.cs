using XingYun.Experiment;

public static class RingRotationFeelResolver
{
	private const float DefaultDampingK1 = 0.75f;

	private const float DefaultCompensationK2 = 1.2f;

	private static readonly RingRotationFeelFunction DefaultSource = new RingRotationFeelFunction();

	private static RingRotationFeelFunction cachedConfig;

	private static RingRotationFeelFunction cachedSource;

	public static RingRotationFeelFunction Resolve()
	{
		RingRotationFeelFunction ringRotationFeelFunction = FunctionDataCenter.Get<RingRotationFeelFunction>();
		if (cachedConfig != null && cachedSource == ringRotationFeelFunction)
		{
			return cachedConfig;
		}
		cachedSource = ringRotationFeelFunction;
		cachedConfig = ApplyDefaults(ringRotationFeelFunction);
		return cachedConfig;
	}

	public static void InvalidateCache()
	{
		cachedConfig = null;
		cachedSource = null;
	}

	public static RingRotationFeelFunction CreateBaselineDefaults()
	{
		return new RingRotationFeelFunction
		{
			EnableFeel = true,
			DampingK1 = 0.75f,
			CompensationK2 = 1.2f,
			CameraZoomAndPan = 0
		};
	}

	private static RingRotationFeelFunction ApplyDefaults(RingRotationFeelFunction source)
	{
		RingRotationFeelFunction ringRotationFeelFunction = new RingRotationFeelFunction
		{
			EnableFeel = source.EnableFeel,
			DampingK1 = source.DampingK1,
			CompensationK2 = source.CompensationK2,
			CameraZoomAndPan = source.CameraZoomAndPan
		};
		if (ringRotationFeelFunction.DampingK1 <= 0f || ringRotationFeelFunction.DampingK1 > 1f)
		{
			ringRotationFeelFunction.DampingK1 = 0.75f;
		}
		if (ringRotationFeelFunction.CompensationK2 < 1f || ringRotationFeelFunction.CompensationK2 > 2.5f)
		{
			ringRotationFeelFunction.CompensationK2 = 1.2f;
		}
		return ringRotationFeelFunction;
	}
}
