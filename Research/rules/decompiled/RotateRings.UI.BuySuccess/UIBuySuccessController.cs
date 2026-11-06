using Example.Cfg;
using Game.UI;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Shop;
using Script.Tools;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.BuySuccess;

[UIPath("UIBuySuccess")]
public class UIBuySuccessController : GameBaseController<UIBuySuccessView>
{
	private const string Tag = "[UIBuySuccessController]";

	private bool isClosing;

	public int shopId = -1;

	public UIBuySuccessController(UIBuySuccessView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		if (param == null)
		{
			Log.LogError("UIBuySuccessController", "参数必须传值");
			return;
		}
		if (!(param is UIBuySuccessParam uIBuySuccessParam))
		{
			Log.LogError("[UIBuySuccessController]", "参数类型错误: " + param.GetType().Name);
			return;
		}
		shopId = uIBuySuccessParam.ShopId;
		SetGiftIcon(uIBuySuccessParam);
		isClosing = false;
		((UnityEvent)((BaseController<UIBuySuccessView>)this).view.btnContinue.onClick).AddListener(new UnityAction(OnContinueClick));
		Button btnClose = ((BaseController<UIBuySuccessView>)this).view.btnClose;
		if (btnClose != null)
		{
			((UnityEvent)btnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		((BaseController<UIBuySuccessView>)this).view.Init();
		((BaseController<UIBuySuccessView>)this).view.PlayOpenAnimation();
	}

	private void SetGiftIcon(UIBuySuccessParam param)
	{
		ShopConfig shopConfig = ((param.ShopId > 0) ? ShopService.GetShopConfig(param.ShopId) : null);
		if (shopConfig != null)
		{
			Sprite val = SpriteManager.LoadSprite(shopConfig.giftIconName);
			if ((Object)(object)val != (Object)null)
			{
				((BaseController<UIBuySuccessView>)this).view.imgGift.sprite = val;
				((Graphic)((BaseController<UIBuySuccessView>)this).view.imgGift).SetNativeSize();
			}
		}
		else if (param.Rewards != null && param.Rewards.Count != 0)
		{
			string iconNameLarge = PropIconService.GetIconNameLarge((EPropType)param.Rewards[0].PropId);
			if (!string.IsNullOrEmpty(iconNameLarge))
			{
				((BaseController<UIBuySuccessView>)this).view.imgGift.SetAtlasSprite("Prop", iconNameLarge);
			}
		}
	}

	public override void OnRemove()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		((UnityEvent)((BaseController<UIBuySuccessView>)this).view.btnContinue.onClick).RemoveListener(new UnityAction(OnContinueClick));
		Button btnClose = ((BaseController<UIBuySuccessView>)this).view.btnClose;
		if (btnClose != null)
		{
			((UnityEvent)btnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		isClosing = false;
		base.OnRemove();
	}

	public override void OnEscape()
	{
		OnContinueClick();
	}

	private void OnContinueClick()
	{
		CloseViewHandler();
	}

	private void OnCloseClick()
	{
		CloseViewHandler();
	}

	private void CloseViewHandler()
	{
		if (!isClosing)
		{
			isClosing = true;
			((BaseController<UIBuySuccessView>)this).view.PlayCloseAnimation(delegate
			{
				((BaseController<UIBuySuccessView>)this).CloseView(false);
			});
		}
	}
}
