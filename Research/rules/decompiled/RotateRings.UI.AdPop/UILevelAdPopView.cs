using Game.UI;
using RotateRings.Common;
using Rotaterings.Tools;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.AdPop;

public class UILevelAdPopView : GameBaseView
{
	private const string OpenAnimName = "anim_uileveladpop_open";

	private const string CloseAnimName = "anim_uileveladpop_close";

	private const string RetryLangKey = "Button_Retry";

	private const string LoadingLangKey = "Button_Loading";

	private const string ReplayLangKey = "Button_Cancel";

	private UIPopupMaskFader maskFader;

	private WidgetHandle _haveNetHandle;

	private WidgetHandle _loadingHandle;

	private WidgetHandle _noWifiHandle;

	public Animation UIAnimation;

	public UIBehaviour PopupHaveNet;

	public UIBehaviour PopupLoading;

	public UIBehaviour PopupNoWifi;

	public AdPopHaveNetWidget HaveNetWidget => _haveNetHandle?.GetView<AdPopHaveNetWidget>();

	public AdPopLoadingWidget LoadingWidget => _loadingHandle?.GetView<AdPopLoadingWidget>();

	public AdPopNoWifiWidget NoWifiWidget => _noWifiHandle?.GetView<AdPopNoWifiWidget>();

	public void Init()
	{
		BindSubWidgets();
	}

	private void BindSubWidgets()
	{
		_haveNetHandle = AttachWidget<AdPopHaveNetWidget>(PopupHaveNet);
		_loadingHandle = AttachWidget<AdPopLoadingWidget>(PopupLoading);
		_noWifiHandle = AttachWidget<AdPopNoWifiWidget>(PopupNoWifi);
	}

	public void SetBtnState(AdState state)
	{
		AdPopHaveNetWidget haveNetWidget = HaveNetWidget;
		AdPopLoadingWidget loadingWidget = LoadingWidget;
		AdPopNoWifiWidget noWifiWidget = NoWifiWidget;
		loadingWidget?.SetVisible(state == AdState.LoadingAd || state == AdState.NoWifi);
		noWifiWidget?.SetVisible(state == AdState.NoWifi);
		haveNetWidget?.SetGreenRetryVisible(state == AdState.HaveNet);
		haveNetWidget?.SetAdBadgeVisible(state == AdState.HaveNet);
		haveNetWidget?.SetReplayLabel("Button_Cancel");
		switch (state)
		{
		case AdState.HaveNet:
			haveNetWidget?.SetRetryLocalized("Button_Retry");
			haveNetWidget?.SetDescInitial();
			break;
		case AdState.NoWifi:
			haveNetWidget?.SetRetryLocalized("Button_Retry");
			haveNetWidget?.SetDescInitial();
			loadingWidget?.SetRetryLocalized("Button_Retry");
			break;
		case AdState.LoadingAd:
			loadingWidget?.SetRetryLocalized("Button_Loading");
			haveNetWidget?.SetDescLoading();
			break;
		}
	}

	public void SetCountDown(int countDownTime)
	{
		LoadingWidget?.SetCountdown(countDownTime);
	}

	public void PlayOpenAnimation()
	{
		UIAnimUtil.PlayUIOpenAnimation(UIAnimation, "anim_uileveladpop_open");
	}

	public float PlayCloseAnimation()
	{
		return PlayAnimation("anim_uileveladpop_close");
	}

	private float PlayAnimation(string animationName)
	{
		if ((Object)(object)UIAnimation == (Object)null)
		{
			return 0f;
		}
		AnimationState val = UIAnimation[animationName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (UIAnimation.IsPlaying(animationName))
		{
			UIAnimation.Stop(animationName);
			val.time = 0f;
			UIAnimation.Sample();
		}
		UIAnimation.Play(animationName);
		return val.length;
	}

	public void OnUiStackActiveChanged(bool isActive)
	{
		if ((Object)(object)maskFader == (Object)null)
		{
			maskFader = ((Component)((UIBaseView)this).Behaviour).GetComponent<UIPopupMaskFader>();
		}
		maskFader?.OnUiStackActiveChanged(isActive);
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIAnimation = behaviour.Get<Animation>("UIAnimation");
		PopupHaveNet = behaviour.Get<UIBehaviour>("PopupHaveNet");
		PopupLoading = behaviour.Get<UIBehaviour>("PopupLoading");
		PopupNoWifi = behaviour.Get<UIBehaviour>("PopupNoWifi");
	}
}
