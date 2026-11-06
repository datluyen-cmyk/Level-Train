namespace RotateRings.UI.Shop;

public static class RestorePurchasesState
{
	public static bool IsRestoring { get; private set; }

	public static void Begin()
	{
		IsRestoring = true;
	}

	public static void Finish()
	{
		IsRestoring = false;
	}
}
