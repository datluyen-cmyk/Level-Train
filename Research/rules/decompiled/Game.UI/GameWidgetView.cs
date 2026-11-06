using XingYun.UI.Core;

namespace Game.UI;

public class GameWidgetView : GameBaseView
{
	public override void Inject(UIBehaviour behaviour)
	{
	}

	protected virtual void OnWidgetFirstLoaded()
	{
	}

	internal void NotifyWidgetFirstLoaded()
	{
		OnWidgetFirstLoaded();
	}

	public override void Dispose()
	{
	}
}
