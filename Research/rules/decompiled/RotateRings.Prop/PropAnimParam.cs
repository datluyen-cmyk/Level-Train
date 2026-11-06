namespace RotateRings.Prop;

public readonly struct PropAnimParam
{
	public static readonly PropAnimParam Default = new PropAnimParam(0f, 0.15f);

	public readonly float Delay;

	public readonly float Duration;

	public PropAnimParam(float delay = 0f, float duration = 0.15f)
	{
		Delay = delay;
		Duration = duration;
	}
}
