using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using Script.Tools;
using UniLog;
using UnityEngine.Events;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

[UIPath("UIGetItem")]
public class UIGetItemController : GameBaseController<UIGetItemView>
{
	private const string OpenAnimName = "anim_popup_open";

	private UIGetItemParam getItemParam;

	public UIGetItemController(UIGetItemView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		if (param == null)
		{
			Log.LogError("UIGetItemController", "必传参数,请检查");
			return;
		}
		getItemParam = param as UIGetItemParam;
		((UnityEvent)((BaseController<UIGetItemView>)this).view.btnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIGetItemView>)this).view.btnClaim.onClick).AddListener(new UnityAction(OnClaimClick));
		((UnityEvent)((BaseController<UIGetItemView>)this).view.btnShop.onClick).AddListener(new UnityAction(OnShopClick));
		if (getItemParam != null)
		{
			string iconNameLarge = PropIconService.GetIconNameLarge(getItemParam.PropType);
			((BaseController<UIGetItemView>)this).view.imgPropIcon.SetAtlasSprite("Prop", iconNameLarge);
		}
		UIAnimUtil.PlayUIOpenAnimation(((BaseController<UIGetItemView>)this).view.UIAnimation, "anim_popup_open");
	}

	private void OnCloseClick()
	{
		((BaseController<UIGetItemView>)this).CloseView(false);
	}

	private void OnClaimClick()
	{
		getItemParam.OnClick?.Invoke();
		((BaseController<UIGetItemView>)this).CloseView(false);
	}

	private void OnShopClick()
	{
		GameApp.UI.Open("UIShop", (UILayer)2, (object)null, (Action)null);
		((BaseController<UIGetItemView>)this).CloseView(false);
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIGetItemView>)this).view.btnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UIGetItemView>)this).view.btnClaim.onClick).RemoveListener(new UnityAction(OnClaimClick));
		((UnityEvent)((BaseController<UIGetItemView>)this).view.btnShop.onClick).RemoveListener(new UnityAction(OnShopClick));
		base.OnRemove();
	}

	public override void OnEscape()
	{
		((BaseController<UIGetItemView>)this).CloseView(false);
	}
}
