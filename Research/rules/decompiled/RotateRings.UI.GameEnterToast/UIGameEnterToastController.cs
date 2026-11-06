using System;
using Game.UI;
using RotateRings.Common;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.GameEnterToast;

[UIPath("UIGameEnterToastView1")]
[UIPath("UIGameEnterToastView2")]
[UIPath("UIGameEnterToastViewShape1")]
[UIPath("UIGameEnterToastViewShape2")]
[UIPath("UIGameEnterToastView_Halloween")]
public class UIGameEnterToastController : GameBaseController<UIGameEnterToastView>
{
	public sealed class OpenParam
	{
		public string LangKey;

		public object[] Args;

		public int StyleId;

		public bool ShowBgMask;

		public bool ShowShapeBanner;

		public Action OnClosed;
	}

	private const string TAG = "[UIGameEnterToast] ";

	private const float AutoCloseDuration = 1.6f;

	private const float SpineWaitWatchdogSeconds = 1f;

	private const float SpineWaitWatchdogMarginSeconds = 0.5f;

	private int autoCloseTimerId;

	private int spineWaitWatchdogTimerId;

	private Action onClosedCallback;

	private OpenParam openParam;

	private bool isClosing;

	public UIGameEnterToastController(UIGameEnterToastView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		openParam = param as OpenParam;
		if (openParam == null)
		{
			Log.LogError("[UIGameEnterToast] ", "OpenParam 为 null，关闭界面避免卡 Pipeline");
			((BaseController<UIGameEnterToastView>)this).CloseView(false);
			return;
		}
		((BaseController<UIGameEnterToastView>)this).view.SetStyleId(openParam.StyleId);
		((BaseController<UIGameEnterToastView>)this).view.SetLocalizedText(openParam.LangKey, openParam.Args);
		((BaseController<UIGameEnterToastView>)this).view.SetBgMaskVisible(openParam.ShowBgMask);
		if (openParam.ShowBgMask)
		{
			((BaseController<UIGameEnterToastView>)this).view.PlayBgMaskInAnimation();
		}
		((BaseController<UIGameEnterToastView>)this).view.PlayInAnimation();
		if (openParam.StyleId == 5)
		{
			PlayHalloweenEntranceSound();
			((BaseController<UIGameEnterToastView>)this).view.PlayHalloweenBannerSpine();
		}
		if (openParam.ShowShapeBanner)
		{
			((BaseController<UIGameEnterToastView>)this).view.PlayShapeColorParticle();
			PlayShapeBannerAppearSound();
		}
		onClosedCallback = openParam.OnClosed;
		StartAutoCloseTimer();
	}

	private void PlayHalloweenEntranceSound()
	{
		AudioService.ActivateExperiment("variant_e7a38092");
		AudioService.PlayUiSound(AudioKeys.StartLevelHalloween.Value);
	}

	private void PlayShapeBannerAppearSound()
	{
		AudioService.ActivateExperiment("variant_f0b830e4");
		AudioService.PlayUiSound(AudioKeys.StylingBannerAppear.Value);
	}

	public override void OnRemove()
	{
		StopAutoCloseTimer();
		StopSpineWaitWatchdog();
		((BaseController<UIGameEnterToastView>)this).view.ClearHalloweenSpineComplete();
		((BaseController<UIGameEnterToastView>)this).view.StopShapeColorParticle();
		Action action = onClosedCallback;
		onClosedCallback = null;
		base.OnRemove();
		action?.Invoke();
	}

	private void StartAutoCloseTimer()
	{
		StopAutoCloseTimer();
		autoCloseTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameEnterToastView>)this).view).Behaviour, 1.6f, (Action)OnAutoClose);
	}

	private void StopAutoCloseTimer()
	{
		if (autoCloseTimerId != 0)
		{
			GameApp.Timer.Kill(autoCloseTimerId);
			autoCloseTimerId = 0;
		}
	}

	private void StopSpineWaitWatchdog()
	{
		if (spineWaitWatchdogTimerId != 0)
		{
			GameApp.Timer.Kill(spineWaitWatchdogTimerId);
			spineWaitWatchdogTimerId = 0;
		}
	}

	private void OnAutoClose()
	{
		autoCloseTimerId = 0;
		if (!isClosing)
		{
			if (((BaseController<UIGameEnterToastView>)this).view.IsHalloweenSpinePlaying())
			{
				((BaseController<UIGameEnterToastView>)this).view.RegisterHalloweenSpineComplete(CloseBanner);
				float num = Mathf.Max(1f, ((BaseController<UIGameEnterToastView>)this).view.GetHalloweenSpineDuration() + 0.5f);
				spineWaitWatchdogTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameEnterToastView>)this).view).Behaviour, num, (Action)CloseBanner);
			}
			else
			{
				CloseBanner();
			}
		}
	}

	private void CloseBanner()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		StopSpineWaitWatchdog();
		((BaseController<UIGameEnterToastView>)this).view.ClearHalloweenSpineComplete();
		((BaseController<UIGameEnterToastView>)this).view.PlayOutAnimation();
		float num = ((BaseController<UIGameEnterToastView>)this).view.GetOutAnimationDuration();
		if (openParam.ShowBgMask)
		{
			((BaseController<UIGameEnterToastView>)this).view.PlayBgMaskOutAnimation();
			float bgMaskOutAnimationDuration = ((BaseController<UIGameEnterToastView>)this).view.GetBgMaskOutAnimationDuration();
			num = Mathf.Max(num, bgMaskOutAnimationDuration);
		}
		if (num > 0f)
		{
			GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameEnterToastView>)this).view).Behaviour, num, (Action)delegate
			{
				((BaseController<UIGameEnterToastView>)this).CloseView(false);
			});
		}
		else
		{
			((BaseController<UIGameEnterToastView>)this).CloseView(false);
		}
	}
}
