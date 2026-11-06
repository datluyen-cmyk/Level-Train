using Game.UI;
using XingYun.UI.Controller;

namespace RotateRings.UI.Game;

public class UILevelProgressController : GameBaseController<UILevelProgressView>
{
	public UILevelProgressController(UILevelProgressView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
	}

	public override void OnRemove()
	{
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UILevelProgressView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
	}
}
