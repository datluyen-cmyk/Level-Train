using System;
using Game.UI;
using RotateRings.Prop;
using RotateRings.UI.Game;
using RotateRings.UI.PropItem;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.PropGuide;

[UIPath("RocketFastUseExperiment1")]
public class UIRocketFastUseExperiment1Controller : GameBaseController<UIRocketFastUseExperiment1View>
{
	private const string Tag = "UI";

	private RocketFastUseOpenData openData;

	private UIGamePropUseService propUseService;

	private bool isClosing;

	private int closeTimerId = -1;

	private UnityAction onPropUseClick;

	public UIRocketFastUseExperiment1Controller(UIRocketFastUseExperiment1View view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		openData = param as RocketFastUseOpenData;
		IController val = default(IController);
		if (GameApp.UI.IsOpen("UIGame", ref val) && val is UIGameController uIGameController)
		{
			propUseService = uIGameController.PropUseService;
		}
		_ = propUseService;
		((UnityEvent)((BaseController<UIRocketFastUseExperiment1View>)this).view.Btn_close.onClick).AddListener(new UnityAction(OnCloseClick));
		if ((Object)(object)((BaseController<UIRocketFastUseExperiment1View>)this).view.Panel != (Object)null)
		{
			((UnityEvent)((Button)((BaseController<UIRocketFastUseExperiment1View>)this).view.Panel).onClick).AddListener(new UnityAction(OnCloseClick));
		}
		UIRocketPropItemView uIRocketPropItemView = ((BaseController<UIRocketFastUseExperiment1View>)this).view.BindPropItemWidget();
		if (uIRocketPropItemView != null && openData != null)
		{
			uIRocketPropItemView.SetPropUseService(propUseService);
			uIRocketPropItemView.SetBadgeProvider(openData.BadgeProvider);
			uIRocketPropItemView.BindEvents();
			uIRocketPropItemView.RefreshProp(openData.PropType);
			PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
			if (propModelProxy != null)
			{
				EPropType propType = openData.PropType;
				((BaseController<UIRocketFastUseExperiment1View>)this).view.SetTxtPropName(propModelProxy.GetPropName(propType));
				string propDescByKey = propModelProxy.GetPropDescByKey(propType, 0);
				((BaseController<UIRocketFastUseExperiment1View>)this).view.SetTxtUnlockProp(propDescByKey);
			}
			if ((Object)(object)uIRocketPropItemView.btnProp != (Object)null)
			{
				onPropUseClick = new UnityAction(OnPropUseClick);
				((UnityEvent)((Button)uIRocketPropItemView.btnProp).onClick).AddListener(onPropUseClick);
			}
		}
		((BaseController<UIRocketFastUseExperiment1View>)this).view.CacheAnimationComponents();
		AlignPropItemToSource();
		((BaseController<UIRocketFastUseExperiment1View>)this).view.PlayOpenAnimation();
	}

	public override void OnRemove()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if (closeTimerId >= 0)
		{
			GameApp.Timer.Kill(closeTimerId);
			closeTimerId = -1;
		}
		((UnityEvent)((BaseController<UIRocketFastUseExperiment1View>)this).view.Btn_close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		if ((Object)(object)((BaseController<UIRocketFastUseExperiment1View>)this).view.Panel != (Object)null)
		{
			((UnityEvent)((Button)((BaseController<UIRocketFastUseExperiment1View>)this).view.Panel).onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		UIRocketPropItemView propItemView = ((BaseController<UIRocketFastUseExperiment1View>)this).view.GetPropItemView();
		if (propItemView != null && (Object)(object)propItemView.btnProp != (Object)null && onPropUseClick != null)
		{
			((UnityEvent)((Button)propItemView.btnProp).onClick).RemoveListener(onPropUseClick);
		}
		onPropUseClick = null;
		propUseService = null;
		base.OnRemove();
	}

	public override void OnEscape()
	{
		CloseWithAnimation();
	}

	private void OnCloseClick()
	{
		CloseWithAnimation();
	}

	private void OnPropUseClick()
	{
		((BaseController<UIRocketFastUseExperiment1View>)this).view.PlayTopCloseAnimation();
	}

	private void CloseWithAnimation()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		try
		{
			openData?.OnCloseAnimationStart?.Invoke();
		}
		catch (Exception ex)
		{
			Log.LogError("UI", "[UIRocketFastUseExperiment1Controller] OnCloseAnimationStart 回调异常: {0}", new object[1] { ex });
		}
		float num = ((BaseController<UIRocketFastUseExperiment1View>)this).view.PlayCloseAnimation();
		if (num > 0f)
		{
			closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIRocketFastUseExperiment1View>)this).view).Behaviour, num, (Action)delegate
			{
				closeTimerId = -1;
				((BaseController<UIRocketFastUseExperiment1View>)this).CloseView(false);
			});
		}
		else
		{
			((BaseController<UIRocketFastUseExperiment1View>)this).CloseView(false);
		}
	}

	private void AlignPropItemToSource()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		UIRocketPropItemView propItemView = ((BaseController<UIRocketFastUseExperiment1View>)this).view.GetPropItemView();
		RectTransform val = ((propItemView != null) ? ((UIBaseView)propItemView).NodeRect : null) ?? propItemView?.Node;
		RectTransform val2 = null;
		IController val3 = default(IController);
		if (openData != null && GameApp.UI.IsOpen("UIGame", ref val3) && val3 is UIGameController uIGameController)
		{
			val2 = uIGameController.View?.PropsView?.GetPropItemAlignAnchor(openData.PropType);
		}
		if (!((Object)(object)val == (Object)null) && !((Object)(object)val2 == (Object)null))
		{
			Camera canvasCamera = GetCanvasCamera(val);
			Camera canvasCamera2 = GetCanvasCamera(val2);
			Rect rect = val.rect;
			Vector3 val4 = ((Transform)val).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
			RectTransform obj = val2;
			rect = val2.rect;
			Vector3 val5 = ((Transform)obj).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center));
			Vector2 val6 = RectTransformUtility.WorldToScreenPoint(canvasCamera, val4);
			Vector2 val7 = RectTransformUtility.WorldToScreenPoint(canvasCamera2, val5);
			Transform parent = ((Transform)val).parent;
			Transform obj2 = ((parent is RectTransform) ? parent : null);
			Vector2 val8 = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)obj2, val6, canvasCamera, ref val8);
			Vector2 val9 = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)obj2, val7, canvasCamera, ref val9);
			Vector2 val10 = val9 - val8;
			val.anchoredPosition += val10;
		}
	}

	private static Camera GetCanvasCamera(RectTransform rect)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		Canvas componentInParent = ((Component)rect).GetComponentInParent<Canvas>();
		if (!((Object)(object)componentInParent != (Object)null) || (int)componentInParent.renderMode != 1)
		{
			return null;
		}
		return componentInParent.worldCamera;
	}
}
