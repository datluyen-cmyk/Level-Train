using Game.UI;
using RotateRings.GameWinIQ;
using XingYun.UI.Controller;

namespace RotateRings.UI.LevelEnd;

public class UIZhiShangXiTongController : GameBaseController<UIZhiShangXiTongView>
{
	private const string Tag = "[UIZhiShangXiTongController] ";

	public UIZhiShangXiTongController(UIZhiShangXiTongView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		GameWinIQSettlementSnapshot snapshot = ((param is GameWinIQSettlementSnapshot gameWinIQSettlementSnapshot) ? gameWinIQSettlementSnapshot : GameWinIQSettlementSnapshot.Empty);
		if (!snapshot.ShouldShowUi)
		{
			((BaseController<UIZhiShangXiTongView>)this).view.SetRootVisible(visible: false);
			return;
		}
		((BaseController<UIZhiShangXiTongView>)this).view.SetRootVisible(visible: true);
		((BaseController<UIZhiShangXiTongView>)this).view.ApplySettlement(snapshot);
	}

	public override void OnRemove()
	{
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIZhiShangXiTongView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
	}
}
