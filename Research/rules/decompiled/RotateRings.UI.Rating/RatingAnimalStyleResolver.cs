using System;
using XingYun.Experiment;

namespace RotateRings.UI.Rating;

public static class RatingAnimalStyleResolver
{
	private const string LogTag = "Rating";

	private const string MultiAnimalResourcePrefix = "MultiAnimals_";

	private const string MultiHouResourceSuffix = "_hou";

	private const string MultiQianResourceSuffix = "_qian";

	public const int SingleAnimalStyle = 1;

	public const int PopupSkinSingle = 0;

	public const int PopupSkinMulti = 1;

	public static bool IsMultiAnimal(int animalStyle)
	{
		return animalStyle != 1;
	}

	public static int ToPopupSkin(int animalStyle)
	{
		if (!IsMultiAnimal(animalStyle))
		{
			return 0;
		}
		return 1;
	}

	public static int Resolve()
	{
		try
		{
			return Normalize(FunctionDataCenter.Get<RatingFunction>()?.AnimalStyle ?? 1);
		}
		catch (Exception)
		{
		}
		return 1;
	}

	public static int Normalize(int animalStyle)
	{
		if (animalStyle >= 1)
		{
			return animalStyle;
		}
		return 1;
	}

	public static string GetMultiAnimalResourceName(int animalStyle, bool isHou)
	{
		return "MultiAnimals_" + animalStyle + (isHou ? "_hou" : "_qian");
	}
}
