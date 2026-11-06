namespace RotateRings.GamePlay.PartStyle;

public sealed class PartStyleMatteFacetedHandler : PartStyleHandlerBase
{
	public override PartStyleType Style => PartStyleType.MatteFaceted;

	protected override string GetRingMaterialPath()
	{
		return null;
	}

	protected override string GetPullMaterialPath()
	{
		return null;
	}

	protected override string GetFixedMaterialPath()
	{
		return null;
	}
}
