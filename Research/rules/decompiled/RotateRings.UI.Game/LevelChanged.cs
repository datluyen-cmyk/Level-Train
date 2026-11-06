namespace RotateRings.UI.Game;

public class LevelChanged
{
	public int Level { get; set; }

	public bool IsNewGame { get; set; } = true;

	public int CollectableCount { get; set; }
}
