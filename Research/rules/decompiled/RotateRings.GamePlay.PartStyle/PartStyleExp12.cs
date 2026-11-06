namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleExp12
{
	private const int PrefixEndLevel = 20;

	private const int RotationStartLevel = 21;

	private const int BlockSize = 20;

	public static PartStyleType Resolve(in PartStyleResolveContext context)
	{
		int globalProgressionLevel = PartStyleExpCommon.GetGlobalProgressionLevel(in context);
		if (globalProgressionLevel <= 20)
		{
			return PartStyleType.GlossyMetal;
		}
		int num = (globalProgressionLevel - 21) / 20;
		if (num < 0)
		{
			return PartStyleType.GlossyMetal;
		}
		return PartStyleExpCommon.EightMaterialCycle[num % PartStyleExpCommon.EightMaterialCycle.Length];
	}
}
