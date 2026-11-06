using XingYun.Experiment;

namespace UnlockRing.Presentation;

public static class PresentationMode
{
	public static PresentationModeKind Mode
	{
		get
		{
			int num = FunctionDataCenter.Get<GameBord>()?.RingBreakPresentationStrategy ?? 0;
			if (num < 0 || num > 2)
			{
				return PresentationModeKind.Off;
			}
			return (PresentationModeKind)num;
		}
	}

	public static bool SuppressByPaletteLock { get; internal set; }

	public static bool UseTaskPresentation
	{
		get
		{
			if (Mode != PresentationModeKind.Off)
			{
				return !SuppressByPaletteLock;
			}
			return false;
		}
	}
}
