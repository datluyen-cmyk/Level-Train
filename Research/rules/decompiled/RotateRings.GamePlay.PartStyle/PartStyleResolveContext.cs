using RotateRings.Model;

namespace RotateRings.GamePlay.PartStyle;

public readonly struct PartStyleResolveContext
{
	public int Level { get; }

	public GameMode GameMode { get; }

	public bool IsModelLevel { get; }

	public bool IsNewGame { get; }

	public int SavedLevelPartStyle { get; }

	public PartStyleResolveContext(int level, GameMode gameMode, bool isModelLevel, bool isNewGame, int savedLevelPartStyle = -1)
	{
		Level = level;
		GameMode = gameMode;
		IsModelLevel = isModelLevel;
		IsNewGame = isNewGame;
		SavedLevelPartStyle = savedLevelPartStyle;
	}
}
