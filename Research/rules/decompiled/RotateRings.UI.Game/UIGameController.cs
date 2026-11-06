using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using Game.UI;
using Game.UI.Language;
using RotateRings.Ad;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using RotateRings.GamePlay.Services.LevelStepLimit;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.Theme;
using RotateRings.UI.Level;
using RotateRings.UI.Shop;
using RotateRings.UI.Tips;
using RotateRings.WeakGuide;
using Script.Tools;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Experiment;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

[UIPath("UIGame")]
public class UIGameController : GameBaseController<UIGameView>
{
	private enum WeakGuideUiVisibilityMode
	{
		Normal,
		PendingHidePose,
		FullyHidden
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIGameController _003C_003E4__this;

		private CancellationToken _003CcancellationToken_003E5__2;

		private bool _003CmaskVisible_003E5__3;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameController uIGameController = _003C_003E4__this;
			try
			{
				if ((uint)num > 1u)
				{
					_003CcancellationToken_003E5__2 = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)((UIBaseView)((BaseController<UIGameView>)uIGameController).view).Behaviour);
					_003CmaskVisible_003E5__3 = false;
				}
				try
				{
					UniTask val2;
					Awaiter<bool> val;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<bool>);
							num = (_003C_003E1__state = -1);
							goto IL_0149;
						}
						val2 = UniTask.Yield((PlayerLoopTiming)13, _003CcancellationToken_003E5__2);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val.GetResult())
					{
						float num2 = ((BaseController<UIGameView>)uIGameController).view.PlayEnterTransitionStart();
						if (num2 > 0f)
						{
							((BaseController<UIGameView>)uIGameController).view.SetLoadingMaskVisible(visible: true);
							_003CmaskVisible_003E5__3 = true;
							val2 = UniTask.Delay(TimeSpan.FromSeconds(num2), true, (PlayerLoopTiming)8, _003CcancellationToken_003E5__2);
							val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78>(ref val, ref this);
								return;
							}
							goto IL_0149;
						}
						goto IL_0154;
					}
					goto end_IL_0030;
					IL_0149:
					if (!val.GetResult())
					{
						goto IL_0154;
					}
					goto end_IL_0030;
					IL_0154:
					AdService.NotifyFrontAdEnterPresentationReady();
					end_IL_0030:;
				}
				finally
				{
					if (num < 0)
					{
						if (_003CmaskVisible_003E5__3)
						{
							((BaseController<UIGameView>)uIGameController).view.SetLoadingMaskVisible(visible: false);
						}
						uIGameController.isEnterTransitionRunning = false;
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CcancellationToken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CcancellationToken_003E5__2 = default(CancellationToken);
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayShopSpineOpenAsync_003Ed__32 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIGameController _003C_003E4__this;

		public CancellationToken cancellationToken;

		private RectTransform _003Cparent_003E5__2;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIGameController uIGameController = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_003d;
				}
				_003Cparent_003E5__2 = ((BaseController<UIGameView>)uIGameController).view?.btnShopEffectTrans;
				if (!((Object)(object)_003Cparent_003E5__2 == (Object)null))
				{
					goto IL_003d;
				}
				goto end_IL_000e;
				IL_003d:
				GameObject result;
				try
				{
					Awaiter<GameObject> val;
					if (num != 0)
					{
						val = UniTaskExtensions.AttachExternalCancellation<GameObject>(GameApp.Res.LoadAssetAsync<GameObject>("eff_spineicon_loop_gp5", ((BaseController<UIGameView>)uIGameController).view.LoadAssetGroup), cancellationToken).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPlayShopSpineOpenAsync_003Ed__32>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					result = val.GetResult();
				}
				catch (OperationCanceledException)
				{
					goto end_IL_000e;
				}
				if (!((Object)(object)result == (Object)null) && !cancellationToken.IsCancellationRequested && !((Object)(object)_003Cparent_003E5__2 == (Object)null))
				{
					uIGameController.shopSpineInstance = Object.Instantiate<GameObject>(result, (Transform)(object)_003Cparent_003E5__2, false);
					Transform transform = uIGameController.shopSpineInstance.transform;
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
					transform.localScale = result.transform.localScale;
					SkeletonGraphic componentInChildren = uIGameController.shopSpineInstance.GetComponentInChildren<SkeletonGraphic>(true);
					if ((Object)(object)componentInChildren == (Object)null)
					{
						uIGameController.DestroyShopSpineInstance();
					}
					else
					{
						componentInChildren.Initialize(false);
						AnimationState animationState = componentInChildren.AnimationState;
						if (animationState == null)
						{
							uIGameController.DestroyShopSpineInstance();
						}
						else
						{
							animationState.SetAnimation(0, "in", false);
							componentInChildren.Update(0f);
							componentInChildren.UpdateMesh();
						}
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cparent_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cparent_003E5__2 = null;
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string ShopSpineEffectLocation = "eff_spineicon_loop_gp5";

	private const string ShopSpineOpenAnim = "in";

	private const string ThemeWidgetLocation = "Theme";

	private const int ThemeStyleIdInGameHUD = 0;

	private const int ThemeStyleIdInSettingPanel = 1;

	private const float LevelStepLimitTipsDelay = 1.5f;

	private UIGamePropsService propsService;

	private UIGamePropUseService propUseService;

	private UIGameRankStarService rankStarService;

	private UIGameGuideHandService guideHandService;

	private bool isEnterTransitionRunning;

	private bool isLevelLoadingUiLocked;

	private bool isSettlementHiddenPose;

	private int openAnimationTimerId = -1;

	private int settlementHideTimerId = -1;

	private int shopEntryRefreshTimerId = -1;

	private int levelStepLimitTipsTimerId = -1;

	private CancellationTokenSource shopSpineCts;

	private GameObject shopSpineInstance;

	private bool? shopEntryVisible;

	private WidgetHandle themeWidgetHandle;

	public UIGamePropsService PropsService => propsService;

	public UIGamePropUseService PropUseService => propUseService;

	public UIGameRankStarService RankStarService => rankStarService;

	public UIGameGuideHandService GuideHandService => guideHandService;

	public UIGameController(UIGameView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		((BaseController<UIGameView>)this).view.Init();
		RefreshGameplayBackgrounds();
		SetShopBg(ThemeService.CurrentThemeId == ThemeId.Night);
		propsService = new UIGamePropsService(this);
		propUseService = new UIGamePropUseService(this);
		rankStarService = new UIGameRankStarService(this);
		guideHandService = new UIGameGuideHandService(this);
		Button val = ((BaseController<UIGameView>)this).view.SettingView?.btnSetting;
		if ((Object)(object)val != (Object)null)
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(OnSettingClick));
		}
		if ((Object)(object)((BaseController<UIGameView>)this).view.btnShop != (Object)null)
		{
			((UnityEvent)((BaseController<UIGameView>)this).view.btnShop.onClick).AddListener(new UnityAction(OnShopClick));
		}
		if ((Object)(object)((BaseController<UIGameView>)this).view.zoomRestoreBtn != (Object)null)
		{
			((UnityEvent)((BaseController<UIGameView>)this).view.zoomRestoreBtn.onClick).AddListener(new UnityAction(OnZoomRestoreClick));
		}
		((BaseController<UIGameView>)this).ListenEvent<ScoreAdded>((Action<ScoreAdded>)OnScoreAdded);
		((BaseController<UIGameView>)this).ListenEvent<ScoreReset>((Action<ScoreReset>)OnScoreReset);
		((BaseController<UIGameView>)this).ListenEvent<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		((BaseController<UIGameView>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UIGameView>)this).ListenEvent<LevelLoadingStartedEvent>((Action<LevelLoadingStartedEvent>)OnLevelLoadingStarted);
		((BaseController<UIGameView>)this).ListenEvent<LevelPreEntryStartedEvent>((Action<LevelPreEntryStartedEvent>)OnLevelPreEntryStarted);
		((BaseController<UIGameView>)this).ListenEvent<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
		((BaseController<UIGameView>)this).ListenEvent<CameraControlChangedEvent>((Action<CameraControlChangedEvent>)OnCameraControlChanged);
		((BaseController<UIGameView>)this).ListenEvent<LevelStepLimitChangedEvent>((Action<LevelStepLimitChangedEvent>)OnLevelStepLimitChanged);
		((BaseController<UIGameView>)this).ListenEvent<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		((BaseController<UIGameView>)this).ListenEvent<PropChangeEvent>((Action<PropChangeEvent>)OnPropChange);
		((BaseController<UIGameView>)this).ListenEvent<ShopEntryVisibilityChangedEvent>((Action<ShopEntryVisibilityChangedEvent>)OnShopEntryVisibilityChanged);
		((BaseController<UIGameView>)this).ListenEvent<WeakGuideStartedEvent>((Action<WeakGuideStartedEvent>)OnWeakGuideStarted);
		((BaseController<UIGameView>)this).ListenEvent<GameplayPointerCaptureChangedEvent>((Action<GameplayPointerCaptureChangedEvent>)OnGameplayPointerCaptureChanged);
		RefreshShopEntry();
		shopEntryRefreshTimerId = GameApp.Timer.Repeat((Object)(object)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour, 0.5f, (Action)RefreshShopEntry);
		SyncLeftWidget();
		RefreshLevel();
		((BaseController<UIGameView>)this).view.LevelView?.RefreshLevelLabel();
		SyncHudWidget();
		RefreshScore();
		PlayEnterTransitionIfNeeded();
		SyncUIGameVisibilityForWeakGuide();
		BgmController.EnterLevel();
		TryLoadThemeWidget();
	}

	private void StartShopSpineOpenAnimation()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		CancelShopSpineTask();
		DestroyShopSpineInstance();
		shopSpineCts = new CancellationTokenSource();
		UniTaskVoid val = PlayShopSpineOpenAsync(shopSpineCts.Token);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CPlayShopSpineOpenAsync_003Ed__32))]
	private UniTaskVoid PlayShopSpineOpenAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayShopSpineOpenAsync_003Ed__32 _003CPlayShopSpineOpenAsync_003Ed__ = default(_003CPlayShopSpineOpenAsync_003Ed__32);
		_003CPlayShopSpineOpenAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayShopSpineOpenAsync_003Ed__._003C_003E4__this = this;
		_003CPlayShopSpineOpenAsync_003Ed__.cancellationToken = cancellationToken;
		_003CPlayShopSpineOpenAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayShopSpineOpenAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayShopSpineOpenAsync_003Ed__32>(ref _003CPlayShopSpineOpenAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayShopSpineOpenAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void RefreshShopEntry()
	{
		SetShopEntryVisible(ShopService.ShowBtnShopEntry());
	}

	private void SetShopEntryVisible(bool visible)
	{
		if (shopEntryVisible != visible)
		{
			shopEntryVisible = visible;
			if ((Object)(object)((BaseController<UIGameView>)this).view?.btnShop != (Object)null)
			{
				((Component)((Component)((BaseController<UIGameView>)this).view.btnShop).transform.parent).gameObject.SetActive(visible);
			}
			if (visible)
			{
				StartShopSpineOpenAnimation();
				return;
			}
			CancelShopSpineTask();
			DestroyShopSpineInstance();
		}
	}

	private void OnPropChange(PropChangeEvent _)
	{
		RefreshShopEntry();
	}

	private void OnShopEntryVisibilityChanged(ShopEntryVisibilityChangedEvent eventData)
	{
		SetShopEntryVisible(eventData?.IsVisible ?? false);
	}

	private void CancelShopSpineTask()
	{
		shopSpineCts?.Cancel();
		shopSpineCts?.Dispose();
		shopSpineCts = null;
	}

	private void DestroyShopSpineInstance()
	{
		if ((Object)(object)shopSpineInstance != (Object)null)
		{
			Object.Destroy((Object)(object)shopSpineInstance);
		}
		shopSpineInstance = null;
	}

	private void OnShopClick()
	{
		GameApp.UI.Open("UIShop", (UILayer)2, (object)null, (Action)null);
	}

	private void SetShopBg(bool isDark)
	{
		if (!((Object)(object)((BaseController<UIGameView>)this).view?.imgShop == (Object)null))
		{
			string spriteName = (isDark ? "shop_bg_dark" : "shop_bg_bright");
			((BaseController<UIGameView>)this).view.imgShop.SetAtlasSprite("Shop", spriteName);
		}
	}

	private void OnThemeChanged(ThemeChangedEvent eventData)
	{
		SetShopBg(eventData != null && eventData.CurrentThemeId == ThemeId.Night);
	}

	private void TryLoadThemeWidget()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((BaseController<UIGameView>)this).view?.ThemeNode == (Object)null)
		{
			return;
		}
		if (!ThemeButtonConfig.IsDarkModeEnabled)
		{
			DetachThemeWidget();
			return;
		}
		DayOrNightModeChoose dayOrNightModeChoose = FunctionDataCenter.Get<DayOrNightModeChoose>();
		if ((dayOrNightModeChoose == null || dayOrNightModeChoose.StyleId == 0) && (themeWidgetHandle == null || themeWidgetHandle.IsDisposed))
		{
			themeWidgetHandle = ((BaseController<UIGameView>)this).view.AttachWidget<UIThemeView>("Theme", (Transform)(object)((BaseController<UIGameView>)this).view.ThemeNode);
			if ((Object)(object)themeWidgetHandle?.GameObject != (Object)null)
			{
				themeWidgetHandle.GameObject.transform.localScale = Vector3.one;
				themeWidgetHandle.WithController<UIThemeController>(new UIThemeController(themeWidgetHandle.GetView<UIThemeView>()));
			}
		}
	}

	private void DetachThemeWidget()
	{
		if (themeWidgetHandle != null)
		{
			((BaseController<UIGameView>)this).view?.DetachWidget(themeWidgetHandle);
			themeWidgetHandle = null;
		}
	}

	private void RefreshGameplayBackgrounds()
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			instance.GetGameService<RotateRingBackgroundAdaptation>()?.RefreshThemeMaterial();
			instance.GetGameService<HalloweenGameBackgroundService>()?.SyncBackgroundForLevelEntry();
		}
	}

	private void ApplyBackgroundRectMaskBinding()
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)((BaseController<UIGameView>)this).view.ContentRectMask == (Object)null))
		{
			instance.GetGameService<RotateRingBackgroundAdaptation>()?.BindUiRectMask(((BaseController<UIGameView>)this).view.ContentRectMask);
		}
	}

	private void ClearBackgroundRectMaskBinding()
	{
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		if (!((Object)(object)instance == (Object)null))
		{
			instance.GetGameService<RotateRingBackgroundAdaptation>()?.ClearUiRectMask();
		}
	}

	private void OnCameraControlChanged(CameraControlChangedEvent eventData)
	{
		if (((BaseController<UIGameView>)this).view != null && !((BaseController<UIGameView>)this).view.IsZoomRestoreButtonVisible())
		{
			RRDataTrack.Instance.LegacySessionCollector.RecordRestoreButtonAppeared();
		}
		((BaseController<UIGameView>)this).view?.SetZoomRestoreButtonVisible(visible: true);
	}

	private void OnZoomRestoreClick()
	{
		RRDataTrack.Instance.ReportBtnClick("盘面-一键还原");
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		RotateCameraGestureController rotateCameraGestureController = (((Object)(object)instance != (Object)null) ? instance.GetGameService<RotateCameraGestureController>() : null);
		if (rotateCameraGestureController != null && rotateCameraGestureController.ReleaseCameraControl())
		{
			((BaseController<UIGameView>)this).view?.SetZoomRestoreButtonVisible(visible: false);
		}
	}

	private void OnScoreAdded(ScoreAdded eventData)
	{
		((BaseController<UIGameView>)this).view.ScoreView?.AddScore(eventData.Score);
	}

	private void OnScoreReset(ScoreReset eventData)
	{
		((BaseController<UIGameView>)this).view.ScoreView?.SetScore(eventData.Score);
	}

	private void OnLevelStepLimitChanged(LevelStepLimitChangedEvent eventData)
	{
		SyncLeftWidget();
		SyncHudWidget();
		((BaseController<UIGameView>)this).view.MovesView?.SetRemaining(eventData.RemainingSteps);
		TryShowLevelStepLimitTips();
	}

	private void TryShowLevelStepLimitTips()
	{
		LevelStepLimitService service = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (service == null || !service.ShouldShowTips)
		{
			return;
		}
		string langKey = service.TipsLangKey;
		if (string.IsNullOrEmpty(langKey) || (Object)(object)((BaseController<UIGameView>)this).view.MovesView?.Node == (Object)null || levelStepLimitTipsTimerId >= 0)
		{
			return;
		}
		levelStepLimitTipsTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour, 1.5f, (Action)delegate
		{
			levelStepLimitTipsTimerId = -1;
			if (service.ShouldShowTips)
			{
				RectTransform val = ((BaseController<UIGameView>)this).view?.MovesView?.Node;
				if (!((Object)(object)val == (Object)null))
				{
					string text = GameApp.Lang.GetText(langKey, Array.Empty<object>());
					TipsService.Open(new UITipsOpenParam
					{
						Target = val,
						DetailText = text,
						PreferredPlacement = UITips_CommonTips.TipsPlacement.Below
					});
					service.MarkTipsShown();
				}
			}
		});
	}

	private void CancelLevelStepLimitTipsTimer()
	{
		if (levelStepLimitTipsTimerId != -1)
		{
			GameApp.Timer.Kill(levelStepLimitTipsTimerId);
			levelStepLimitTipsTimerId = -1;
		}
	}

	private void SyncHudWidget()
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService != null && levelStepLimitService.IsEnabled)
		{
			((BaseController<UIGameView>)this).view.EnsureHudWidgetAsMoves();
			((BaseController<UIGameView>)this).view.MovesView?.SetRemaining(levelStepLimitService.RemainingSteps);
		}
		else
		{
			((BaseController<UIGameView>)this).view.EnsureHudWidgetAsScore();
		}
	}

	private void SyncLeftWidget()
	{
		LevelStepLimitService levelStepLimitService = SingletonComponent<GameplayManager>.Instance?.GetGameService<LevelStepLimitService>();
		if (levelStepLimitService != null && levelStepLimitService.IsEnabled)
		{
			((BaseController<UIGameView>)this).view.EnsureLeftWidgetAsScoreByStepLimit();
		}
		else
		{
			((BaseController<UIGameView>)this).view.EnsureLeftWidgetAsLevel();
		}
	}

	private void OnLevelChanged(LevelChanged eventData)
	{
		RefreshShopEntry();
		SyncLeftWidget();
		RefreshLevel();
		((BaseController<UIGameView>)this).view?.SetZoomRestoreButtonVisible(visible: false);
		SyncHudWidget();
		RefreshScore();
		SyncUIGameVisibilityForWeakGuide();
	}

	private void PlayOpenAnimationAndNotify()
	{
		isSettlementHiddenPose = false;
		CancelOpenAnimationTimer();
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		float num = ((BaseController<UIGameView>)this).view.PlayOpenAnimation();
		if (num <= 0f)
		{
			NotifyUIGameLayoutReady(level);
			return;
		}
		openAnimationTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour, num, (Action)delegate
		{
			openAnimationTimerId = -1;
			UIGameView view = ((BaseController<UIGameView>)this).view;
			if (!((Object)(object)((view != null) ? ((UIBaseView)view).Behaviour : null) == (Object)null) && ((Component)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour).gameObject.activeInHierarchy && !((Object)(object)((BaseController<UIGameView>)this).view.Anim == (Object)null) && GameApp.Proxy.Get<LevelModelProxy>().Level == level)
			{
				NotifyUIGameLayoutReady(level);
			}
		});
	}

	private void CancelOpenAnimationTimer()
	{
		if (openAnimationTimerId != -1)
		{
			GameApp.Timer.Kill(openAnimationTimerId);
			openAnimationTimerId = -1;
		}
	}

	private void CancelSettlementHideTimer()
	{
		if (settlementHideTimerId != -1)
		{
			GameApp.Timer.Kill(settlementHideTimerId);
			settlementHideTimerId = -1;
		}
	}

	private void NotifyUIGameLayoutReady(int level)
	{
		Canvas.ForceUpdateCanvases();
		GameApp.Event.Send<UIGameLayoutReadyEvent>(new UIGameLayoutReadyEvent
		{
			Level = level
		});
	}

	private void OnLevelLoadingStarted(LevelLoadingStartedEvent eventData)
	{
		if (isLevelLoadingUiLocked)
		{
			if (LockUIService.CurrentLockCount > 0)
			{
				return;
			}
			isLevelLoadingUiLocked = false;
		}
		isLevelLoadingUiLocked = true;
		LockUIService.Enable();
	}

	private void ReleaseLevelLoadingUiLock(string reason)
	{
		if (isLevelLoadingUiLocked)
		{
			isLevelLoadingUiLocked = false;
			if (LockUIService.CurrentLockCount > 0)
			{
				LockUIService.Disable();
			}
		}
	}

	private void OnGameplayPointerCaptureChanged(GameplayPointerCaptureChangedEvent eventData)
	{
		if (eventData != null && ((BaseController<UIGameView>)this).view != null)
		{
			((BaseController<UIGameView>)this).view.SetGameplayDragInputBlocked(eventData.IsCaptured);
		}
	}

	private void OnLevelPreEntryStarted(LevelPreEntryStartedEvent eventData)
	{
		ReleaseLevelLoadingUiLock("OnLevelPreEntryStarted");
	}

	private void OnLevelEnterCompleted(LevelEnterCompletedEvent eventData)
	{
		LevelWinPreloadService.TryStartPhotoshopWarmupOnLevelLoaded(eventData.Level);
		ReleaseLevelLoadingUiLock("OnLevelEnterCompleted");
	}

	private void OnWeakGuideStarted(WeakGuideStartedEvent eventData)
	{
		SyncUIGameVisibilityForWeakGuide(weakGuideStarted: true);
	}

	private WeakGuideUiVisibilityMode ResolveWeakGuideUiMode(bool weakGuideStarted)
	{
		if (!UIWeakGuideService.IsWeakGuideAvailable())
		{
			return WeakGuideUiVisibilityMode.Normal;
		}
		if (weakGuideStarted)
		{
			return WeakGuideUiVisibilityMode.FullyHidden;
		}
		GameplayManager instance = SingletonComponent<GameplayManager>.Instance;
		GameLevelInfo gameLevelInfo = (((Object)(object)instance != (Object)null && (Object)(object)instance.CurrentLevelObject != (Object)null) ? instance.CurrentLevelObject.GetComponent<GameLevelInfo>() : null);
		WeakGuideManager weakGuideManager = (((Object)(object)gameLevelInfo != (Object)null) ? gameLevelInfo.weakGuideManager : null);
		if ((Object)(object)weakGuideManager != (Object)null && weakGuideManager.IsRunning)
		{
			return WeakGuideUiVisibilityMode.FullyHidden;
		}
		if ((Object)(object)instance != (Object)null && (Object)(object)instance.CurrentLevelObject != (Object)null && !instance.HasUnsolvedGameplayObjects)
		{
			return WeakGuideUiVisibilityMode.Normal;
		}
		if ((Object)(object)weakGuideManager == (Object)null || !weakGuideManager.IsCompleted)
		{
			return WeakGuideUiVisibilityMode.PendingHidePose;
		}
		return WeakGuideUiVisibilityMode.Normal;
	}

	private void SyncUIGameVisibilityForWeakGuide(bool weakGuideStarted = false)
	{
		switch (ResolveWeakGuideUiMode(weakGuideStarted))
		{
		case WeakGuideUiVisibilityMode.FullyHidden:
			CancelOpenAnimationTimer();
			((BaseController<UIGameView>)this).view.SetVisible(visible: false);
			break;
		case WeakGuideUiVisibilityMode.PendingHidePose:
			isSettlementHiddenPose = false;
			((BaseController<UIGameView>)this).view.SetVisible(visible: true);
			((BaseController<UIGameView>)this).view.SnapCloseAnimationToEnd();
			break;
		case WeakGuideUiVisibilityMode.Normal:
			((BaseController<UIGameView>)this).view.SetVisible(visible: true);
			PlayOpenAnimationAndNotify();
			break;
		}
	}

	private void OnSettingClick()
	{
		if (!((Object)(object)((BaseController<UIGameView>)this).view.SettingView?.btnSetting == (Object)null) && (Gameplay.Active == null || !Gameplay.Active.IsLevelCompleted) && !IsSkyrocketInFlight())
		{
			GameApp.UI.Open(GetSettingInGamePath(), (UILayer)2, (object)null, (Action)null);
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

	private static string GetSettingInGamePath()
	{
		if (!ThemeButtonConfig.IsDarkModeEnabled)
		{
			return "UISettingInGame";
		}
		if ((FunctionDataCenter.Get<DayOrNightModeChoose>()?.StyleId ?? 0) == 1)
		{
			return "UISettingInGame_DarkModeBtnLocationExperiment";
		}
		return "UISettingInGame";
	}

	private void RefreshLevel()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		((BaseController<UIGameView>)this).view?.LevelView?.SetLevel(level);
	}

	private void RefreshScore()
	{
		int currentScore = GameApp.Proxy.Get<LevelModelProxy>().GetCurrentScore();
		((BaseController<UIGameView>)this).view.ScoreView?.SetScore(currentScore);
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		RefreshUI();
	}

	private void RefreshUI()
	{
		RefreshLevel();
		RefreshScore();
		((BaseController<UIGameView>)this).view.LevelView?.RefreshLevelLabel();
		((BaseController<UIGameView>)this).view.PropsView?.RefreshAllProp();
	}

	public void PlayEnterTransitionIfNeeded()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!isEnterTransitionRunning && AdService.ShouldDeferPendingConsumeToEnterAnimation())
		{
			isEnterTransitionRunning = true;
			UniTaskVoid val = PlayEnterTransitionAndConsumePendingInterstitial();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78))]
	private UniTaskVoid PlayEnterTransitionAndConsumePendingInterstitial()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78 _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__ = default(_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78);
		_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__._003C_003E4__this = this;
		_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__._003C_003Et__builder)).Start<_003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__78>(ref _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayEnterTransitionAndConsumePendingInterstitial_003Ed__._003C_003Et__builder)).Task;
	}

	public override void OnRemove()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		GameApp.Event.Send<UIGameRemovedEvent>(new UIGameRemovedEvent());
		CancelOpenAnimationTimer();
		CancelSettlementHideTimer();
		CancelLevelStepLimitTipsTimer();
		if (shopEntryRefreshTimerId >= 0)
		{
			GameApp.Timer.Kill(shopEntryRefreshTimerId);
			shopEntryRefreshTimerId = -1;
		}
		SetShopEntryVisible(visible: false);
		ReleaseLevelLoadingUiLock("OnRemove");
		((BaseController<UIGameView>)this).view?.SetGameplayDragInputBlocked(blocked: false);
		DetachThemeWidget();
		AdService.ClearPendingFrontAd("UIGame.OnRemove");
		base.OnRemove();
		propsService?.Dispose();
		propsService = null;
		propUseService?.Dispose();
		propUseService = null;
		rankStarService?.Dispose();
		rankStarService = null;
		guideHandService?.Dispose();
		guideHandService = null;
		Button val = ((BaseController<UIGameView>)this).view.SettingView?.btnSetting;
		if ((Object)(object)val != (Object)null)
		{
			((UnityEvent)val.onClick).RemoveListener(new UnityAction(OnSettingClick));
		}
		if ((Object)(object)((BaseController<UIGameView>)this).view.btnShop != (Object)null)
		{
			((UnityEvent)((BaseController<UIGameView>)this).view.btnShop.onClick).RemoveListener(new UnityAction(OnShopClick));
		}
		if ((Object)(object)((BaseController<UIGameView>)this).view.zoomRestoreBtn != (Object)null)
		{
			((UnityEvent)((BaseController<UIGameView>)this).view.zoomRestoreBtn.onClick).RemoveListener(new UnityAction(OnZoomRestoreClick));
		}
		((BaseController<UIGameView>)this).UnlistenEvent<ScoreAdded>((Action<ScoreAdded>)OnScoreAdded);
		((BaseController<UIGameView>)this).UnlistenEvent<ScoreReset>((Action<ScoreReset>)OnScoreReset);
		((BaseController<UIGameView>)this).UnlistenEvent<LevelChanged>((Action<LevelChanged>)OnLevelChanged);
		((BaseController<UIGameView>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UIGameView>)this).UnlistenEvent<LevelLoadingStartedEvent>((Action<LevelLoadingStartedEvent>)OnLevelLoadingStarted);
		((BaseController<UIGameView>)this).UnlistenEvent<LevelPreEntryStartedEvent>((Action<LevelPreEntryStartedEvent>)OnLevelPreEntryStarted);
		((BaseController<UIGameView>)this).UnlistenEvent<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
		((BaseController<UIGameView>)this).UnlistenEvent<CameraControlChangedEvent>((Action<CameraControlChangedEvent>)OnCameraControlChanged);
		((BaseController<UIGameView>)this).UnlistenEvent<LevelStepLimitChangedEvent>((Action<LevelStepLimitChangedEvent>)OnLevelStepLimitChanged);
		((BaseController<UIGameView>)this).UnlistenEvent<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
		((BaseController<UIGameView>)this).UnlistenEvent<PropChangeEvent>((Action<PropChangeEvent>)OnPropChange);
		((BaseController<UIGameView>)this).UnlistenEvent<ShopEntryVisibilityChangedEvent>((Action<ShopEntryVisibilityChangedEvent>)OnShopEntryVisibilityChanged);
		((BaseController<UIGameView>)this).UnlistenEvent<WeakGuideStartedEvent>((Action<WeakGuideStartedEvent>)OnWeakGuideStarted);
		((BaseController<UIGameView>)this).UnlistenEvent<GameplayPointerCaptureChangedEvent>((Action<GameplayPointerCaptureChangedEvent>)OnGameplayPointerCaptureChanged);
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIGameView>)this).OnActive(isActive);
		if (isActive)
		{
			PlayEnterTransitionIfNeeded();
			RefreshShopEntry();
		}
	}

	public void PlayHideAnimationForSettlement()
	{
		using (new ProfilerScope("UIGameController.PlayHideAnimationForSettlement"))
		{
			AdService.ClearPendingFrontAd("UIGameSettlementHide");
			CancelOpenAnimationTimer();
			CancelSettlementHideTimer();
			UIGameView view = ((BaseController<UIGameView>)this).view;
			if ((Object)(object)((view != null) ? ((UIBaseView)view).Behaviour : null) == (Object)null || !((Component)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour).gameObject.activeSelf)
			{
				return;
			}
			if (isSettlementHiddenPose)
			{
				((BaseController<UIGameView>)this).view.SnapCloseAnimationToEnd();
				return;
			}
			float num = ((BaseController<UIGameView>)this).view.PlayCloseAnimation();
			if (num <= 0f)
			{
				((BaseController<UIGameView>)this).view.SnapCloseAnimationToEnd();
				isSettlementHiddenPose = true;
				return;
			}
			settlementHideTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour, num, (Action)delegate
			{
				settlementHideTimerId = -1;
				UIGameView view2 = ((BaseController<UIGameView>)this).view;
				if (!((Object)(object)((view2 != null) ? ((UIBaseView)view2).Behaviour : null) == (Object)null))
				{
					((BaseController<UIGameView>)this).view.SnapCloseAnimationToEnd();
					isSettlementHiddenPose = true;
				}
			});
		}
	}

	public void CloseWithAnimation(Action onComplete = null)
	{
		float num = ((BaseController<UIGameView>)this).view.PlayCloseAnimation();
		if (num <= 0f)
		{
			GameApp.UI.CloseView("UIGame", (object)null, false);
			onComplete?.Invoke();
			return;
		}
		GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour, num, (Action)delegate
		{
			((Component)((UIBaseView)((BaseController<UIGameView>)this).view).Behaviour).gameObject.SetActive(false);
			GameApp.UI.CloseView("UIGame", (object)null, false);
			onComplete?.Invoke();
		});
	}

	public override void OnEscape()
	{
		if (!IsWeakGuideBlockingEscape() && (Gameplay.Active == null || !Gameplay.Active.IsLevelCompleted) && !IsSkyrocketInFlight())
		{
			((BaseController<UIGameView>)this).ExecuteCommand<LevelBackHomeCommand>((object)((UIBaseView)((BaseController<UIGameView>)this).view).Location);
		}
	}

	private bool IsWeakGuideBlockingEscape()
	{
		if (!UIWeakGuideService.IsWeakGuideAvailable())
		{
			return false;
		}
		if (UIWeakGuideService.TryGetCurrentLevelWeakGuide(out var weakGuide))
		{
			return !weakGuide.IsCompleted;
		}
		return true;
	}
}
