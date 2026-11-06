using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace Game.UI;

public class WidgetHandle
{
	public string Location { get; internal set; }

	public GameWidgetView View { get; internal set; }

	public GameObject GameObject { get; internal set; }

	public object Param { get; internal set; }

	public IController Controller { get; internal set; }

	public bool IsDisposed { get; private set; }

	public bool OwnsGameObject { get; internal set; } = true;

	public TView GetView<TView>() where TView : GameWidgetView
	{
		return View as TView;
	}

	public TController WithController<TController>(TController controller) where TController : IController
	{
		Controller = (IController)(object)controller;
		((IController)controller/*cast due to .constrained prefix*/).OnRegister(Param);
		return controller;
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			IController controller = Controller;
			if (controller != null)
			{
				controller.OnRemove();
			}
			View?.OnUnloadAsset();
			GameWidgetView view = View;
			if (view != null)
			{
				((UIBaseView)view).Dispose();
			}
			if (OwnsGameObject && (Object)(object)GameObject != (Object)null)
			{
				Object.Destroy((Object)(object)GameObject);
			}
			View = null;
			GameObject = null;
			Controller = null;
		}
	}
}
