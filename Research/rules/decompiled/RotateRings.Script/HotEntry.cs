using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Example.Cfg;
using Game.DataTrack;
using Game.Feature;
using Game.GM;
using Game.Input;
using Game.Lang;
using Game.Provider;
using Game.UI;
using RotateRing.GamePlay;
using RotateRings.Ad;
using RotateRings.Common;
using RotateRings.Common.Performance;
using RotateRings.GameWinIQ;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.SpecialLevel;
using RotateRings.StartEntry;
using RotateRings.Theme;
using RotateRings.UI.Privacy;
using RotateRings.UI.RewardedLuckyDraw;
using RotateRings.UI.Shop;
using Rotaterings.Tools;
using Rotaterings.UI.RankList;
using UniLog;
using UnityEngine;
using XingYun.Account;
using XingYun.Aot;
using XingYun.Aot.Device;
using XingYun.Core.Entry;
using XingYun.Core.Singletons;
using XingYun.Experiment;
using XingYun.GM;
using XingYun.Lang;
using XingYun.Rendering;
using XingYun.UI.Core;

namespace RotateRings.Script;

public class HotEntry : BaseEntry
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__16 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				try
				{
					Awaiter<LoginResult> val;
					if (num != 0)
					{
						val = CloudSaveFlowService.WaitForLoginAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__16>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
					}
					LoginResult result = val.GetResult();
					if (result != null && result.IsSuccess)
					{
						UniTaskVoid val2 = SyncCloudSaveOnLoginAsync();
						((UniTaskVoid)(ref val2)).Forget();
					}
				}
				catch (Exception ex)
				{
					Log.LogError("HotEntry", "冷启动登录与云存档同步异常: {0}", new object[1] { ex });
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
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
	private struct _003CSyncCloudSaveOnLoginAsync_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				_ = 1;
				try
				{
					Awaiter<CloudSaveOperationResult> val;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
							num = (_003C_003E1__state = -1);
							goto IL_00d4;
						}
						val = CloudSaveFlowService.PullAndDecideAsync(CloudSaveSyncMode.DeferCloudApply).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CSyncCloudSaveOnLoginAsync_003Ed__17>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
					}
					CloudSaveOperationResult result = val.GetResult();
					if (!result.IsSuccess)
					{
						val = CloudSaveFlowService.PullAndDecideAsync(CloudSaveSyncMode.DeferCloudApply).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CSyncCloudSaveOnLoginAsync_003Ed__17>(ref val, ref this);
							return;
						}
						goto IL_00d4;
					}
					goto IL_00dc;
					IL_00d4:
					result = val.GetResult();
					goto IL_00dc;
					IL_00dc:
					CloudSaveFlowService.ReportLastSyncTrackData("cold_start");
					if (!CloudSaveFlowService.IsMarkedLocalDataBehindResult(result))
					{
						_ = result.IsSuccess;
					}
				}
				catch (Exception ex)
				{
					Log.LogError("HotEntry", "冷启动云存档同步异常: {0}", new object[1] { ex });
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
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

	private const string HotEntryProduceProcedure = "HotEntryProduceProcedure";

	private const int HomeEntryRequiredLevel = 1;

	private bool hasEnteredMainFlow;

	protected override IEnumerator InitBefore()
	{
		AotStartProcedureTrack.StartMeasure("HotEntry.InitBefore");
		_003C_003En__0();
		RRDataTrack.Instance.ReportPageShow("加载页");
		UICameraOverlayHandler.EnsureInitialized();
		CanvasMatchAdapter.EnsureInitialized();
		Singleton<GameInputManager>.Instance.EnsureInitialized();
		UIControllerResolver.Register();
		AotStartProcedureTrack.StopMeasure("HotEntry.InitBefore", false);
		yield return null;
	}

	protected override void InitFont()
	{
		AotStartProcedureTrack.StartMeasure("HotEntry.InitFont");
		((BaseEntry)this).InitFont();
		Singleton<LangManager>.Instance.SetTextProvider((ITextProvider)(object)new LangProvider());
		StyleFontFallbackService.RebuildWithPlayerName(GameApp.Lang.CurrentLanguage);
		AotStartProcedureTrack.StopMeasure("HotEntry.InitFont", false);
	}

	protected override void InitPreload()
	{
		((BaseEntry)this).InitPreload();
		AotStartProcedureTrack.StartMeasure("HotEntry.InitPreload.Audio");
		AudioService.Init();
		AotStartProcedureTrack.StopMeasure("HotEntry.InitPreload.Audio", false);
	}

	protected override void InitSDK()
	{
		((BaseEntry)this).InitSDK();
	}

	protected override void LoadExperimentConfig()
	{
		AotStartProcedureTrack.StartMeasure("HotEntry.LoadExperimentConfig");
		((BaseEntry)this).LoadExperimentConfig();
		ExampleTables table = GameApp.Config.GetTable<ExampleTables>();
		LanguageCompat.MigrateHiddenLanguageIfNeeded();
		ExperimentConfigLoader.LoadConfig((object)table);
		AotStartProcedureTrack.StopMeasure("HotEntry.LoadExperimentConfig", false);
	}

	protected override void InitExperiment()
	{
		AotStartProcedureTrack.StartMeasure("HotEntry.InitExperiment");
		((BaseEntry)this).InitExperiment();
		LevelConfigService.Init();
		AdService.Release();
		AdService.Init();
		_ = FunctionDataCenter.Get<UIReviveFunction>().ReviveViewType;
		FeatureBootstrap.Start();
		HalloweenFeatureService.SaveLoadingEnabledForNextLaunch();
		GameWinIQService.Instance.ApplyCrossDayRulesIfNeeded();
		SpecialLevelModule.Initialize();
		SpecialLevelModule.DropExpiredSessionOnStartup();
		RefreshLevelGameModeAfterSpecialLevelStartupCleanup();
		AdService.OnFeaturesReady();
		CloudSaveFlowService.EnsureInitialized();
		RewardedLuckyDrawService.Initialize();
		ThemeService.Init();
		if (!ThemeButtonConfig.IsDarkModeEnabled && ThemeService.CurrentThemeId == ThemeId.Night)
		{
			ThemeService.SetTheme(ThemeId.Day);
		}
		AotStartProcedureTrack.StopMeasure("HotEntry.InitExperiment", false);
	}

	private static void RefreshLevelGameModeAfterSpecialLevelStartupCleanup()
	{
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy != null)
		{
			levelModelProxy.Data.RefreshCurrentGameMode(LevelConfigService.MainLevelMax, LevelConfigService.CurrentEndlessSeason);
			levelModelProxy.SyncGameInfo();
		}
	}

	protected override void InitTrack()
	{
		((BaseEntry)this).InitTrack();
		SessionManager.Instance.Initialize();
	}

	protected override IEnumerator InitGMRoutine()
	{
		GMRegister.Init();
		yield return _003C_003En__1();
	}

	protected override IGMExperimentDataProvider GetGMDataProvider()
	{
		return (IGMExperimentDataProvider)(object)new GMExperimentDataProvider();
	}

	protected override void EnterGame()
	{
		((BaseEntry)this).EnterGame();
		Singleton<StoreManager>.Instance.Init();
		Screen.sleepTimeout = -2;
		if (SocLevelDetector.GetTier() < 3)
		{
			StyleLineUnderlayProxy.GlobalEnabled = false;
		}
		ProjectFrameRatePolicy.Apply("EnterGame");
		AotStartProcedureTrack.StartMeasure("HotEntry.EnterGame");
		SettingUtil.ApplyAllSettings();
		RunEnterAfterPrivacyGate();
		AotStartProcedureTrack.StopMeasure("HotEntry.EnterGame", false);
	}

	private void RunEnterAfterPrivacyGate()
	{
		try
		{
			UIPrivacyGate.WaitIfNeeded(OnPrivacyGatePassed);
		}
		catch (Exception ex)
		{
			Log.LogError("HotEntry", "隐私弹窗门闩异常: {0}", new object[1] { ex });
			OnPrivacyGatePassed();
		}
	}

	private void OnPrivacyGatePassed()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!hasEnteredMainFlow)
		{
			hasEnteredMainFlow = true;
			ContinueEnterAfterPrivacyGate();
			if (CloudSaveFlowService.IsCloudSaveEnabled())
			{
				UniTaskVoid val = LoginAndSyncCloudSaveInBackgroundAsync();
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	[AsyncStateMachine(typeof(_003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__16))]
	private UniTaskVoid LoginAndSyncCloudSaveInBackgroundAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__16 _003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__ = default(_003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__16);
		_003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__._003C_003Et__builder)).Start<_003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__16>(ref _003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CLoginAndSyncCloudSaveInBackgroundAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CSyncCloudSaveOnLoginAsync_003Ed__17))]
	private static UniTaskVoid SyncCloudSaveOnLoginAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CSyncCloudSaveOnLoginAsync_003Ed__17 _003CSyncCloudSaveOnLoginAsync_003Ed__ = default(_003CSyncCloudSaveOnLoginAsync_003Ed__17);
		_003CSyncCloudSaveOnLoginAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CSyncCloudSaveOnLoginAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CSyncCloudSaveOnLoginAsync_003Ed__._003C_003Et__builder)).Start<_003CSyncCloudSaveOnLoginAsync_003Ed__17>(ref _003CSyncCloudSaveOnLoginAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CSyncCloudSaveOnLoginAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ContinueEnterAfterPrivacyGate()
	{
		PlayerModelData playerModelData = GameApp.Proxy.Get<PlayerModelProxy>()?.Data;
		if (playerModelData != null && playerModelData.IsNewUser)
		{
			RRDataTrack.Instance.ReportScreenData();
			if (playerModelData.RankOpenLevelLocked <= 0)
			{
				playerModelData.RankOpenLevelLocked = RankFunctionResolver.GetRankOpenLevel();
			}
			playerModelData.IsNewUser = false;
		}
		StartEntryColdLaunch.RunAfterPrivacyGate(HasCompletedRequiredLevel, EnterHome, delegate
		{
			EnterGameScene();
		});
	}

	private bool HasCompletedRequiredLevel()
	{
		return GameApp.Proxy.Get<LevelModelProxy>().Level > 1;
	}

	private void EnterHome(StartEntryHomeLaunchParam launchParam = null)
	{
		AotStartProcedureTrack.StartMeasure("HotEntry.EnterHome");
		EnsureAudioListener();
		GameApp.Proxy.Get<LevelModelProxy>().PrepareEnterGame();
		GameApp.Proxy.Get<PropModelProxy>().InitForEntry();
		GameplayCameraManager.IsPlayFromLoading = true;
		GameplaySceneLoader.LoadSceneAsync(delegate
		{
			RRDataTrack.Instance.ReportPageShow("加载页进主页");
			GameApp.UI.Open("UIHome", (UILayer)1, (object)launchParam, (Action)null);
			GameApp.UI.Open("UILockOverlay", (UILayer)4, (object)null, (Action)null);
			UICameraOverlayHandler.TryAttachOverlay();
			SDKManager.SetBugSnagContext("RotateRings-Entry");
			LoadingManager.Instance.HideLoading();
			AotStartProcedureTrack.StopMeasure("HotEntry.EnterHome", false);
		}, delegate
		{
			LoadingManager.Instance.HideLoading();
		});
	}

	private static void EnsureAudioListener()
	{
		AudioService.EnsureAudioListener();
	}

	private void EnterGameScene(Action onSceneLoaded = null)
	{
		AotStartProcedureTrack.StartMeasure("HotEntry.EnterGameScene");
		EnsureAudioListener();
		GameApp.Proxy.Get<LevelModelProxy>().PrepareEnterGame();
		GameApp.Proxy.Get<PropModelProxy>().InitForEntry();
		GameplayCameraManager.IsPlayFromLoading = true;
		GameplaySceneLoader.LoadSceneAsync(delegate
		{
			GameApp.UI.Open("UIGame", (UILayer)1, (object)null, (Action)null);
			GameApp.UI.Open("UILockOverlay", (UILayer)4, (object)null, (Action)null);
			UICameraOverlayHandler.TryAttachOverlay();
			GameplaySceneLoader.LoadLevel(GameApp.Proxy.Get<LevelModelProxy>().Level);
			LoadingManager.Instance.HideLoading();
			onSceneLoaded?.Invoke();
			AotStartProcedureTrack.StopMeasure("HotEntry.EnterGameScene", false);
		}, delegate
		{
			LoadingManager.Instance.HideLoading();
		});
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__0()
	{
		return ((BaseEntry)this).InitBefore();
	}

	[CompilerGenerated]
	[DebuggerHidden]
	private IEnumerator _003C_003En__1()
	{
		return ((BaseEntry)this).InitGMRoutine();
	}
}
