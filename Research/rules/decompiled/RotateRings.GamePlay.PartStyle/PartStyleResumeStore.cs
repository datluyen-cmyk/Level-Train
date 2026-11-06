using RotateRings.Model;

namespace RotateRings.GamePlay.PartStyle;

public static class PartStyleResumeStore
{
	private const int Unset = -1;

	private const int MinResumeStyleExp = 13;

	private const int MaxResumeStyleExp = 15;

	public static bool TryGetForEnter(GameMode mode, int level, bool isNewGame, bool hasValidSave, out int styleRaw)
	{
		styleRaw = -1;
		if (isNewGame || !hasValidSave)
		{
			return false;
		}
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave == null)
		{
			return false;
		}
		if (gameBordPartStyleSave.ActiveResumeLevel != level || gameBordPartStyleSave.ActiveResumeGameMode != (int)mode)
		{
			return false;
		}
		if (!PartStyleExpCommon.TryParseStyle(gameBordPartStyleSave.ActiveResumePartStyle, out var _))
		{
			return false;
		}
		styleRaw = gameBordPartStyleSave.ActiveResumePartStyle;
		return true;
	}

	public static bool TryGetMatchingLevelStyle(GameMode mode, int level, out int styleRaw)
	{
		styleRaw = -1;
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave == null)
		{
			return false;
		}
		if (gameBordPartStyleSave.ActiveResumeLevel != level || gameBordPartStyleSave.ActiveResumeGameMode != (int)mode)
		{
			return false;
		}
		if (!PartStyleExpCommon.TryParseStyle(gameBordPartStyleSave.ActiveResumePartStyle, out var _))
		{
			return false;
		}
		styleRaw = gameBordPartStyleSave.ActiveResumePartStyle;
		return true;
	}

	public static void CommitAfterEnter(GameMode mode, int level, PartStyleType style)
	{
		int num = GameBordPartStyleResolver.ReadGameBordStyleExp();
		if (num >= 13 && num <= 15 && (mode == GameMode.Main || mode == GameMode.Endless))
		{
			GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
			if (gameBordPartStyleSave != null)
			{
				gameBordPartStyleSave.ActiveResumeGameMode = (int)mode;
				gameBordPartStyleSave.ActiveResumeLevel = level;
				gameBordPartStyleSave.ActiveResumePartStyle = (int)style;
			}
		}
	}

	public static void ClearActiveResume()
	{
		GameBordPartStyleModelData gameBordPartStyleSave = PartStyleExpCommon.GetGameBordPartStyleSave();
		if (gameBordPartStyleSave != null)
		{
			gameBordPartStyleSave.ActiveResumeGameMode = -1;
			gameBordPartStyleSave.ActiveResumeLevel = -1;
			gameBordPartStyleSave.ActiveResumePartStyle = -1;
		}
	}
}
