using System;

namespace Game.UI;

public abstract class UIBaseService<TController, TView> : IUIBaseService, IDisposable where TController : GameBaseController<TView> where TView : GameBaseView
{
	protected TController Controller { get; }

	protected TView View { get; }

	protected UIBaseService(TController controller)
	{
		Controller = controller;
		View = controller.View;
		controller.RegisterUIService(this);
	}

	public virtual void Dispose()
	{
	}
}
