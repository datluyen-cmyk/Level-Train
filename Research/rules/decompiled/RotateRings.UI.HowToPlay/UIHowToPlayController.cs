using UnityEngine;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.HowToPlay;

[UIPath("UIHowToPlayView")]
public class UIHowToPlayController : BaseController<UIHowToPlayView>
{
	private const int TotalPages = 3;

	private int currentPageIndex;

	public UIHowToPlayController(UIHowToPlayView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		currentPageIndex = 0;
		base.view.InitToggleDot();
		if ((Object)(object)base.view.BtnClose != (Object)null)
		{
			((UnityEvent)base.view.BtnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)base.view.BtnNextFirst != (Object)null)
		{
			((UnityEvent)base.view.BtnNextFirst.onClick).AddListener(new UnityAction(OnNextClick));
		}
		if ((Object)(object)base.view.BtnNextMid != (Object)null)
		{
			((UnityEvent)base.view.BtnNextMid.onClick).AddListener(new UnityAction(OnNextClick));
		}
		if ((Object)(object)base.view.BtnPreviousPage != (Object)null)
		{
			((UnityEvent)base.view.BtnPreviousPage.onClick).AddListener(new UnityAction(OnPreviousClick));
		}
		if ((Object)(object)base.view.BtnCompletePage != (Object)null)
		{
			((UnityEvent)base.view.BtnCompletePage.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		base.view.OnSwipePageChanged = OnSwipePageChanged;
		base.view.InitSwipeDetection(3);
		base.view.ShowPage(currentPageIndex, 3);
		base.view.PlayOpenAnimation();
	}

	public override void OnRemove()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		if ((Object)(object)base.view.BtnClose != (Object)null)
		{
			((UnityEvent)base.view.BtnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		if ((Object)(object)base.view.BtnNextFirst != (Object)null)
		{
			((UnityEvent)base.view.BtnNextFirst.onClick).RemoveListener(new UnityAction(OnNextClick));
		}
		if ((Object)(object)base.view.BtnNextMid != (Object)null)
		{
			((UnityEvent)base.view.BtnNextMid.onClick).RemoveListener(new UnityAction(OnNextClick));
		}
		if ((Object)(object)base.view.BtnPreviousPage != (Object)null)
		{
			((UnityEvent)base.view.BtnPreviousPage.onClick).RemoveListener(new UnityAction(OnPreviousClick));
		}
		if ((Object)(object)base.view.BtnCompletePage != (Object)null)
		{
			((UnityEvent)base.view.BtnCompletePage.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		base.view.OnSwipePageChanged = null;
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}

	private void OnNextClick()
	{
		if (currentPageIndex < 2)
		{
			currentPageIndex++;
			RefreshPage();
		}
	}

	private void OnPreviousClick()
	{
		if (currentPageIndex > 0)
		{
			currentPageIndex--;
			RefreshPage();
		}
	}

	private void OnSwipePageChanged(int direction)
	{
		if (direction > 0)
		{
			OnNextClick();
		}
		else if (direction < 0)
		{
			OnPreviousClick();
		}
	}

	private void RefreshPage()
	{
		base.view.ShowPage(currentPageIndex, 3);
	}

	private void OnCloseClick()
	{
		base.CloseView(false);
	}
}
