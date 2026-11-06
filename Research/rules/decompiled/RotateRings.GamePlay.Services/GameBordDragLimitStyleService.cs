namespace RotateRings.GamePlay.Services;

public sealed class GameBordDragLimitStyleService : GameplayServiceBase<GameplayManager>
{
	private IGameplayObject activeTarget;

	private IDragLimitStyleHandler activeHandler;

	public GameBordDragLimitStyleService(GameplayManager host)
		: base(host)
	{
		ApplyGuardPresetFromExperiment();
	}

	public void ApplyGuardPresetFromExperiment()
	{
		int num = DragLimitGuardStyleExp.ReadStyleExp();
		RefreshActiveHandler(num);
		if (num == 1 || num == 2)
		{
			DragLimitGuardStyleExp.ApplyPreset(num);
		}
	}

	public void OnPointerDown(IGameplayObject target, GameplayPointerData data)
	{
		if (target != null && GameplayObjectQuery.IsRingOrStick(target))
		{
			activeTarget = target;
			activeHandler?.OnPointerDown(target, data);
		}
	}

	public void OnPointerDrag(IGameplayObject target, GameplayPointerData data)
	{
		if (target != null && target == activeTarget && activeHandler != null)
		{
			activeHandler.OnPointerDrag(target, data);
		}
	}

	public void OnPointerUp(IGameplayObject target)
	{
		if (target != null && target == activeTarget && activeHandler != null)
		{
			activeHandler.OnPointerUp(target);
			activeHandler.ClearSession();
			activeTarget = null;
		}
	}

	public void TickLateUpdate()
	{
		if (activeTarget != null && activeHandler != null)
		{
			activeHandler.TickLateUpdate(activeTarget);
		}
	}

	public override void ClearLevel()
	{
		activeTarget = null;
		activeHandler?.ClearSession();
	}

	private void RefreshActiveHandler(int styleExp)
	{
		activeHandler?.ClearSession();
		IDragLimitStyleHandler dragLimitStyleHandler2;
		if (styleExp != 1 && styleExp != 2)
		{
			IDragLimitStyleHandler dragLimitStyleHandler = new DragDefaultLimitStyleHandler();
			dragLimitStyleHandler2 = dragLimitStyleHandler;
		}
		else
		{
			IDragLimitStyleHandler dragLimitStyleHandler = new DragGuardLimitStyleHandler();
			dragLimitStyleHandler2 = dragLimitStyleHandler;
		}
		activeHandler = dragLimitStyleHandler2;
	}
}
