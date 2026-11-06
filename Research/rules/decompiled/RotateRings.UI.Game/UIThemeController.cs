using Game.DataTrack;
using Game.UI;
using RotateRings.Theme;
using UnityEngine.Events;
using XingYun.UI.Controller;

namespace RotateRings.UI.Game;

public class UIThemeController : GameBaseController<UIThemeView>
{
	private const string TAG = "[UI][UIThemeController] ";

	public UIThemeController(UIThemeView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIThemeView>)this).view.BtnTheme.onClick).AddListener(new UnityAction(OnThemeClick));
	}

	private void OnThemeClick()
	{
		RRDataTrack.Instance.ReportBtnClick("黑暗模式切换按钮");
		ThemeService.CycleNextTheme();
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIThemeView>)this).view.BtnTheme.onClick).RemoveListener(new UnityAction(OnThemeClick));
		base.OnRemove();
	}

	public override void OnEscape()
	{
	}
}
