namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleMatteSmoothHandler : PartStyleHandlerBase
{
	private const string RingMaterialPath = "mat_ring_matte_smooth";

	private const string PullMaterialPath = "mat_ring_matte_smooth";

	private const string FixedMaterialPath = "mat_catch_matte_smooth";

	public override PartStyleType Style => PartStyleType.MatteSmooth;

	protected override string GetRingMaterialPath()
	{
		return "mat_ring_matte_smooth";
	}

	protected override string GetPullMaterialPath()
	{
		return "mat_ring_matte_smooth";
	}

	protected override string GetFixedMaterialPath()
	{
		return "mat_catch_matte_smooth";
	}
}
