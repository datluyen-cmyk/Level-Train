using RotateRings.Model;
using XingYun.Experiment;

namespace RotateRings.GamePlay.PartStyle;

public static class GameBordPartStyleResolver
{
	private const int MinStyleExp = 1;

	private const int MaxStyleExp = 15;

	public static PartStyleType Resolve(in PartStyleResolveContext context)
	{
		if (context.GameMode != GameMode.Main && context.GameMode != GameMode.Endless)
		{
			return PartStyleType.MatteFaceted;
		}
		return ResolveByStyleExp(ReadGameBordStyleExp(), in context);
	}

	public static PartStyleType ResolveByStyleExp(int styleExp, in PartStyleResolveContext context)
	{
		styleExp = ClampStyleExp(styleExp);
		return styleExp switch
		{
			2 => PartStyleExpUniform.Resolve(PartStyleType.Jelly), 
			3 => PartStyleExpUniform.Resolve(PartStyleType.Wood), 
			4 => PartStyleExpUniform.Resolve(PartStyleType.MatteSmooth), 
			5 => PartStyleExpUniform.Resolve(PartStyleType.GlossyMetal), 
			6 => PartStyleExpUniform.Resolve(PartStyleType.Marble), 
			7 => PartStyleExpEvery5.Resolve(context.Level), 
			8 => PartStyleExpEvery10.Resolve(context.Level), 
			9 => PartStyleExpRandom.Resolve(context.Level), 
			10 => PartStyleExp10.Resolve(in context), 
			11 => PartStyleExp11.Resolve(in context), 
			12 => PartStyleExp12.Resolve(in context), 
			13 => PartStyleExp13.Resolve(in context), 
			14 => PartStyleExp14.Resolve(in context), 
			15 => PartStyleExp15.Resolve(in context), 
			_ => PartStyleExpBaseboard.Resolve(context.Level), 
		};
	}

	public static int ClampStyleExp(int styleExp)
	{
		if (styleExp < 1 || styleExp > 15)
		{
			return 1;
		}
		return styleExp;
	}

	public static int ReadGameBordStyleExp()
	{
		return ClampStyleExp(FunctionDataCenter.Get<GameBord>()?.GameBordStyle ?? 1);
	}
}
