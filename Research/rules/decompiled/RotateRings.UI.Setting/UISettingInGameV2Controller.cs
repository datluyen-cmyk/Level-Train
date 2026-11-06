using System;
using Game.DataTrack;
using Hot.Model;
using RotateRings.Common;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.UI.HowToPlay;
using RotateRings.UI.Level;
using RotateRings.UI.RestartRetention;
using UnityEngine;
using UnityEngine.Events;
using XingYun.Core.Singletons;
using XingYun.GM;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UISettingInGame")]
public class UISettingInGameV2Controller : BaseController<UISettingInGameV2View>
{
	private const float GmClickWindowSeconds = 5f;

	private const int GmClickTriggerCount = 20;

	protected bool isClosing;

	private bool isRemoved;

	private Action closeInterrupted;

	private float gmFirstClickTime;

	private int gmClickCount;

	private UISetting_IconToggleItem musicToggleItem;

	private UISetting_IconToggleItem soundToggleItem;

	private UISetting_IconToggleItem vibrationToggleItem;

	public UISettingInGameV2Controller(UISettingInGameV2View view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		isClosing = false;
		isRemoved = false;
		closeInterrupted = null;
		gmFirstClickTime = 0f;
		gmClickCount = 0;
		((UnityEvent)base.view.BtnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.BtnGM.onClick).AddListener(new UnityAction(OnGMClick));
		((UnityEvent)base.view.bgMask.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.ButtonHome.onClick).AddListener(new UnityAction(OnButtonHomeClick));
		((UnityEvent)base.view.ButtonReplay.onClick).AddListener(new UnityAction(OnButtonReplayClick));
		((UnityEvent)base.view.btn_HowtoPlay.onClick).AddListener(new UnityAction(OnHowtoplayClick));
		((UnityEvent)base.view.btn_Language.onClick).AddListener(new UnityAction(OnLanguageClick));
		base.ListenEvent<SettingInGameInit>((Action<SettingInGameInit>)OnSettingInGameInit);
		base.ExecuteCommand<SettingInGameInitCommand>((object)null);
		base.view.PlayOpenAnimation();
	}

	private void OnSettingInGameInit(SettingInGameInit arg)
	{
		if (musicToggleItem == null)
		{
			musicToggleItem = new UISetting_IconToggleItem(base.view.btn_Music, base.view.SetMusicToggleVisual);
		}
		if (soundToggleItem == null)
		{
			soundToggleItem = new UISetting_IconToggleItem(base.view.btn_Sound, base.view.SetSoundToggleVisual);
		}
		if (vibrationToggleItem == null)
		{
			vibrationToggleItem = new UISetting_IconToggleItem(base.view.btn_Vibration, base.view.SetVibrationToggleVisual);
		}
		musicToggleItem.Initialize(arg.IsMusicOn, OnMusicToggle);
		soundToggleItem.Initialize(arg.IsSoundOn, OnSoundToggle);
		vibrationToggleItem.Initialize(arg.IsVibrateOn, OnHapticsToggle);
	}

	private void OnCloseClick()
	{
		if (!isClosing)
		{
			CloseWithAnimation();
			string currentModeName = GetCurrentModeName();
			RRDataTrack.Instance.ReportBtnClick("游戏内设置-" + currentModeName + "-关闭");
			RRDataTrack.Instance.ReportUserSettingAudioSnapshot(UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetMusicState()), UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetSoundState()), UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetVibrationState()));
		}
	}

	public void CloseWithAnimation(Action onComplete = null, Action onInterrupted = null)
	{
		if (isClosing || isRemoved)
		{
			onInterrupted?.Invoke();
			return;
		}
		isClosing = true;
		closeInterrupted = onInterrupted;
		base.view.PlayCloseAnimation(delegate
		{
			if (!isRemoved)
			{
				closeInterrupted = null;
				base.CloseView(false);
				onComplete?.Invoke();
			}
		});
	}

	private void OnGMClick()
	{
		if (gmFirstClickTime <= 0f || Time.realtimeSinceStartup - gmFirstClickTime > 5f)
		{
			gmFirstClickTime = Time.realtimeSinceStartup;
			gmClickCount = 0;
		}
		gmClickCount++;
		if (Time.realtimeSinceStartup - gmFirstClickTime <= 5f && gmClickCount >= 20)
		{
			Singleton<VersionViewHelp>.Instance.OpenVersionUI();
			gmClickCount = 0;
			gmFirstClickTime = 0f;
		}
	}

	private void OnHowtoplayClick()
	{
		if (!isClosing)
		{
			RRDataTrack.Instance.ReportBtnClick("主页设置-教程");
			GameApp.UI.Open("UIHowToPlayView", (UILayer)2, (object)HowToPlayType.Arrow, (Action)null);
		}
	}

	private void OnLanguageClick()
	{
		if (!isClosing)
		{
			CloseWithAnimation(delegate
			{
				GameApp.UI.Open("UILanguage", (UILayer)2, (object)null, (Action)null);
			});
		}
	}

	private void OnButtonHomeClick()
	{
		if (isClosing)
		{
			return;
		}
		RRDataTrack.Instance.ReportBtnClick("游戏内设置-[" + GetCurrentModeName() + "]-返回主页");
		if (Gameplay.Active != null && Gameplay.Active.IsLevelCompleted)
		{
			CloseWithAnimation();
		}
		else if (!IsSkyrocketInFlight())
		{
			string location = ((UIBaseView)base.view).Location;
			CloseWithAnimation(delegate
			{
				base.ExecuteCommand<LevelBackHomeCommand>((object)location);
			});
		}
	}

	private static bool IsSkyrocketInFlight()
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (instance == null)
		{
			return false;
		}
		return instance.PropUseService?.HasSkyrocketInFlight == true;
	}

	private void OnButtonReplayClick()
	{
		if (!isClosing)
		{
			AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
			OnReplayClick();
		}
	}

	private void OnReplayClick()
	{
		string currentModeName = GetCurrentModeName();
		RRDataTrack.Instance.ReportBtnClick("游戏内设置-" + currentModeName + "-重新开始");
		if (Gameplay.Active != null && Gameplay.Active.IsLevelCompleted)
		{
			CloseWithAnimation();
		}
		else if (!IsSkyrocketInFlight())
		{
			RestartRetentionFlow.ReplayCurrentLevelExtend(((UIBaseView)base.view).Location);
		}
	}

	private void OnMusicToggle(bool isOn)
	{
		SettingUtil.ToggleMusicState(isOn, userInitiated: true);
		AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
		string currentModeName = GetCurrentModeName();
		RRDataTrack.Instance.ReportBtnClick("游戏内设置-[" + currentModeName + "]-背景音乐-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnSoundToggle(bool isOn)
	{
		SettingUtil.ToggleSoundState(isOn);
		AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
		string currentModeName = GetCurrentModeName();
		RRDataTrack.Instance.ReportBtnClick("游戏内设置-[" + currentModeName + "]-声音-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnHapticsToggle(bool isOn)
	{
		SettingUtil.ToggleVibrationState(isOn);
		AudioService.PlayUiSound(AudioKeys.BtnGroupClick.Value);
		string currentModeName = GetCurrentModeName();
		RRDataTrack.Instance.ReportBtnClick("游戏内设置-[" + currentModeName + "]-震动-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private static string GetCurrentModeName()
	{
		return GameInfo.CurrentGameMode switch
		{
			GameMode.Main => "主关卡", 
			GameMode.Endless => "无尽模式", 
			_ => string.Empty, 
		};
	}

	public override void OnRemove()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		isRemoved = true;
		Action action = closeInterrupted;
		closeInterrupted = null;
		((UnityEvent)base.view.BtnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.BtnGM.onClick).RemoveListener(new UnityAction(OnGMClick));
		((UnityEvent)base.view.bgMask.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)base.view.ButtonHome.onClick).RemoveListener(new UnityAction(OnButtonHomeClick));
		((UnityEvent)base.view.ButtonReplay.onClick).RemoveListener(new UnityAction(OnButtonReplayClick));
		((UnityEvent)base.view.btn_HowtoPlay.onClick).RemoveListener(new UnityAction(OnHowtoplayClick));
		((UnityEvent)base.view.btn_Language.onClick).RemoveListener(new UnityAction(OnLanguageClick));
		musicToggleItem?.Dispose();
		soundToggleItem?.Dispose();
		vibrationToggleItem?.Dispose();
		musicToggleItem = null;
		soundToggleItem = null;
		vibrationToggleItem = null;
		base.UnlistenEvent<SettingInGameInit>((Action<SettingInGameInit>)OnSettingInGameInit);
		action?.Invoke();
	}

	public override void OnActive(bool isActive)
	{
		base.OnActive(isActive);
		base.view.NotifyPopupMaskStackActive(isActive);
		if (isActive)
		{
			RRDataTrack.Instance.ReportBtnClick("游戏内设置-点击设置展现");
		}
	}

	public override void OnEscape()
	{
		OnCloseClick();
	}
}
