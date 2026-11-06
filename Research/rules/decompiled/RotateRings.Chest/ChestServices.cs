using System.Collections.Generic;

namespace RotateRings.Chest;

public static class ChestServices
{
	public static class UiPositionConst
	{
		public const int Bottom = 1;

		public const int Top = 2;
	}

	public static class LoopModeConst
	{
		public const int LoopAll = 1;

		public const int LoopTail = 2;
	}

	public static int UnlockLevel { get; private set; } = 1;

	public static int UiPosition { get; private set; } = 1;

	public static List<int> ProgressList { get; private set; } = new List<int> { 6, 6, 8, 10 };

	public static int LoopMode { get; private set; } = 2;

	public static void Refresh(ChestProxy chestProxy)
	{
		UnlockLevel = chestProxy.GetUnlockLevel();
		UiPosition = chestProxy.GetUiPosition();
		ProgressList = chestProxy.GetProgressList();
		LoopMode = chestProxy.GetLoopMode();
	}
}
