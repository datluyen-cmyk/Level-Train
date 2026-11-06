using System;
using Game.UI;

namespace RotateRings.UI.Game;

public abstract class UIGameBaseService : UIBaseService<UIGameController, UIGameView>, IUIGameBaseService, IUIBaseService, IDisposable
{
	protected UIGameBaseService(UIGameController controller)
		: base(controller)
	{
	}
}
