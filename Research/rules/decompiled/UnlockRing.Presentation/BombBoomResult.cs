namespace UnlockRing.Presentation;

public sealed class BombBoomResult : IPresentationResult
{
	public BombRelative Bomb { get; }

	public BombBoomResult(BombRelative bomb)
	{
		Bomb = bomb;
	}
}
