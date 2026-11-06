using RotateRings.GamePlay.Services;

namespace UnlockRing.Presentation;

public sealed class PresentationContext
{
	public RingBoomService BombService { get; }

	public RingSolveSuccessEffectService BreakEffectService { get; }

	public RingLockChainService ChainService { get; }

	public PresentationContext(RingBoomService bombService, RingSolveSuccessEffectService breakEffectService, RingLockChainService chainService)
	{
		BombService = bombService;
		BreakEffectService = breakEffectService;
		ChainService = chainService;
	}
}
