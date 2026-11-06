using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using Game.Input;
using Game.Level;
using RotateRings.Ad;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.MultiTouchGuide;
using RotateRings.GamePlay.PartStyle;
using RotateRings.GamePlay.Save;
using RotateRings.GamePlay.Services;
using RotateRings.GamePlay.Services.CharacterExpression;
using RotateRings.GamePlay.Services.LevelStepLimit;
using RotateRings.GamePlay.Services.Presentation;
using RotateRings.GamePlay.Services.Prop;
using RotateRings.GameWinIQ;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.StartEntry;
using RotateRings.UI.Level;
using RotateRings.WeakGuide;
using Rotaterings.Rank;
using UniLog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnlockRing;
using UnlockRing.Presentation;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.Experiment;
using XingYun.UI.Adapt;

public class GameplayManager : SingletonComponent<GameplayManager>, IGameplayServiceHost, IGameplayManager, IGameCameraSource
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CReplayEnterAnimAsync_003Ed__180 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameplayManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameplayManager gameplayManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00da;
				}
				if (gameplayManager.LevelEnterService != null && !((Object)(object)gameplayManager.gameLevelObject == (Object)null))
				{
					LevelEnterPipelineService gameService = gameplayManager.GetGameService<LevelEnterPipelineService>();
					ReplayEntranceAnimationStep replayEntranceAnimationStep = gameService?.GetStep<ReplayEntranceAnimationStep>();
					if (replayEntranceAnimationStep != null)
					{
						LevelEnterPipelineContext ctx = new LevelEnterPipelineContext
						{
							Manager = gameplayManager,
							Level = (GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0)
						};
						CancellationToken cancellationTokenOnDestroy = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)gameplayManager);
						UniTask val2 = gameService.ExecuteStepAsync(replayEntranceAnimationStep, ctx, cancellationTokenOnDestroy);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CReplayEnterAnimAsync_003Ed__180>(ref val, ref this);
							return;
						}
						goto IL_00da;
					}
				}
				goto end_IL_000e;
				IL_00da:
				((Awaiter)(ref val)).GetResult();
				end_IL_000e:;
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
	private struct _003CRunLevelEnterAsync_003Ed__176 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameplayManager _003C_003E4__this;

		public int level;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameplayManager gameplayManager = _003C_003E4__this;
			try
			{
				if (num == 0 || gameplayManager.TryBeginLevelEnterLoad())
				{
					try
					{
						Awaiter val2;
						if (num != 0)
						{
							CancellationToken cancellationTokenOnDestroy = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)gameplayManager);
							UniTask val = gameplayManager.GetGameService<LevelEnterPipelineService>().RunAsync(level, releaseGroupOnClear: true, "YooAssets", cancellationTokenOnDestroy);
							val2 = ((UniTask)(ref val)).GetAwaiter();
							if (!((Awaiter)(ref val2)).IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val2;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunLevelEnterAsync_003Ed__176>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
						}
						((Awaiter)(ref val2)).GetResult();
					}
					finally
					{
						if (num < 0)
						{
							gameplayManager.EndLevelEnterLoad();
						}
					}
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
	private struct _003CRunPreplacedLevelAsync_003Ed__185 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public GameplayManager _003C_003E4__this;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameplayManager gameplayManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00b2;
				}
				LevelEnterPipelineService gameService = gameplayManager.GetGameService<LevelEnterPipelineService>();
				if (gameService != null)
				{
					LevelEnterPipelineContext ctx = new LevelEnterPipelineContext
					{
						Manager = gameplayManager,
						Level = gameplayManager.testLevel,
						LevelRoot = gameplayManager.gameLevelObject,
						IsNewGame = true
					};
					CancellationToken cancellationTokenOnDestroy = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)gameplayManager);
					UniTask val2 = gameService.RunPreplacedLevelAsync(ctx, cancellationTokenOnDestroy);
					val = ((UniTask)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunPreplacedLevelAsync_003Ed__185>(ref val, ref this);
						return;
					}
					goto IL_00b2;
				}
				goto end_IL_000e;
				IL_00b2:
				((Awaiter)(ref val)).GetResult();
				end_IL_000e:;
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

	private readonly Dictionary<Type, IGameplayService> gameplayServices = new Dictionary<Type, IGameplayService>();

	private Rect _worldBounds = new Rect(-100f, -100f, 200f, 200f);

	public const float WorldMarginFactor = 0.01f;

	private string _cameraLevelKey;

	private Camera _gameplayCamera;

	private IGameplayObject activeGameplayObject;

	private LegacyOperationSource activeOperationSource;

	public readonly List<IGameplayObject> LevelEnterObjectsCache = new List<IGameplayObject>();

	private bool isApplicationBackground;

	private bool isSleepDisabled;

	private bool isLevelCompleted;

	private bool hasPersistedVictoryProgress;

	private LevelVictoryService.VictoryProgressSnapshot victorySnapshot;

	private Texture2D levelScreenShot;

	private GameObject gameLevelObject;

	private GameLevelInfo currentLevelInfo;

	private bool isLoadingLevel;

	[SerializeField]
	private float completeLevelDelay;

	[SerializeField]
	private Camera levelCaptureCamera;

	[SerializeField]
	private bool isArtLevel;

	[SerializeField]
	private int rescueTime;

	[SerializeField]
	private EventSystem eventSystem;

	[SerializeField]
	private Transform levelContent;

	[SerializeField]
	private Transform background;

	[SerializeField]
	private Material backgroundMaskMaterial;

	[SerializeField]
	private int testLevel = 1;

	public UIWeakGuideService WeakGuideService => GetGameService<UIWeakGuideService>();

	public RingObjectsService ObjectsService => GetGameService<RingObjectsService>();

	public RingLevelSaveService RingSave => GetGameService<RingLevelSaveService>();

	internal RotateRings.GamePlay.Services.RingGuideRTService GuideRT => GetGameService<RotateRings.GamePlay.Services.RingGuideRTService>();

	public RingAcornCollectService AcornCollect => GetGameService<RingAcornCollectService>();

	public RingLockPassiveService LockPassive => GetGameService<RingLockPassiveService>();

	public RingObjectSolveService SolveService => GetGameService<RingObjectSolveService>();

	public RingLockActiveService LockActive => GetGameService<RingLockActiveService>();

	public RingFillPartDataService FillPartData => GetGameService<RingFillPartDataService>();

	public RingLevelConfigService LevelConfig => GetGameService<RingLevelConfigService>();

	public RingLockChainService LockChain => GetGameService<RingLockChainService>();

	public RingInputBlockService InputBlock => GetGameService<RingInputBlockService>();

	public RingSolveBreakDelayService SolveBreakDelay => GetGameService<RingSolveBreakDelayService>();

	public RingSolveSuccessEffectService SolveSuccessEffect => GetGameService<RingSolveSuccessEffectService>();

	public RingBoomService RingBoom => GetGameService<RingBoomService>();

	public RotateRingPropUseService PropUseService => GetGameService<RotateRingPropUseService>();

	public RotateRingOperateHighlightService OperateHighlightService => GetGameService<RotateRingOperateHighlightService>();

	public RingPartStyleService PartStyle => GetGameService<RingPartStyleService>();

	public RotateRingLevelEnterService LevelEnterService => GetGameService<RotateRingLevelEnterService>();

	public RingDragEffectService DragEffectService => GetGameService<RingDragEffectService>();

	public RingDragCollisionEventService RingDragCollisionEventService => GetGameService<RingDragCollisionEventService>();

	public RingDragFixedPinVibrateService DragFixedPinVibrate => GetGameService<RingDragFixedPinVibrateService>();

	public RotateRingLockShakeService LockShakeService => GetGameService<RotateRingLockShakeService>();

	public ScreenShakeService ScreenShake => GetGameService<ScreenShakeService>();

	public GameBordDragLimitStyleService DragLimitStyle => GetGameService<GameBordDragLimitStyleService>();

	public RingAutoEscapeService AutoEscape => GetGameService<RingAutoEscapeService>();

	public CumulativeReleaseService CumulativeRelease => GetGameService<CumulativeReleaseService>();

	public RingPaletteLockService PaletteLock => GetGameService<RingPaletteLockService>();

	public LevelVictorySettlementService VictorySettlement => GetGameService<LevelVictorySettlementService>();

	public RingLockFeatureService LockFeature => GetGameService<RingLockFeatureService>();

	public PresentationResultCollectService PresentationCollect => GetGameService<PresentationResultCollectService>();

	public bool SolveOnDragExit { get; set; }

	internal float CoordScale => 1f / 24f;

	internal IGameplayObject ActiveGameplayObject
	{
		get
		{
			return activeGameplayObject;
		}
		set
		{
			activeGameplayObject = value;
		}
	}

	internal LegacyOperationSource ActiveOperationSource
	{
		get
		{
			return activeOperationSource;
		}
		set
		{
			activeOperationSource = value;
		}
	}

	public bool IsNewGameRun { get; set; } = true;

	public LevelMapFlipMode CurrentLevelMapFlipMode { get; set; }

	public string LoadedLevelPath { get; set; }

	public GameplayMode Mode => GameplayMode.RotateRing;

	public bool HasUnsolvedGameplayObjects => ObjectsService.HasUnsolvedGameplayObjects;

	public bool IsLevelCompleted => isLevelCompleted;

	public GameObject CurrentLevelObject => gameLevelObject;

	public GameLevelInfo CurrentLevelInfo => currentLevelInfo;

	public bool IsLevelEnterPlaying { get; set; }

	internal Transform EffectRoot => levelContent;

	internal Camera GameplayCam => _gameplayCamera;

	internal string LevelCameraKey => _cameraLevelKey;

	public Rect WorldBounds
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _worldBounds;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_worldBounds = value;
		}
	}

	public bool IsLoadingLevel => isLoadingLevel;

	Camera IGameCameraSource.Cam => _gameplayCamera;

	string IGameCameraSource.CameraLevelKey => _cameraLevelKey;

	Rect IGameCameraSource.WorldBounds => _worldBounds;

	public Transform LevelContent => levelContent;

	public Transform LevelBackground => background;

	public Material BackgroundMaskMaterial => backgroundMaskMaterial;

	internal bool HasActiveLevelRoot => (Object)(object)gameLevelObject != (Object)null;

	private void RegisterAllGameServices()
	{
		RegisterGameService(new RingDraggingSolveService(this));
		RegisterGameService(new RingDragFixedPinVibrateService(this));
		RegisterGameService(new RotateRings.GamePlay.Services.RingGuideRTService(this));
		RegisterGameService(new RotateCameraAdaptation(this));
		RegisterGameService(new RotateRingBackgroundAdaptation(this));
		RegisterGameService(new HalloweenGameBackgroundService(this));
		RegisterGameService(new RotateCameraGestureController(this));
		RegisterGameService(new MultiTouchGuideService(this));
		RegisterGameService(new RingObjectsService(this));
		RegisterGameService(new RingLevelSaveService(this));
		RegisterGameService(new RingSolveSuccessEffectService(this));
		RegisterGameService(new RingBoomService(this));
		RegisterGameService(new RingAcornCollectService(this));
		RegisterGameService(new RotateRingPropUseService(this));
		RegisterGameService(new RotateRingOperateHighlightService(this));
		RegisterGameService(new RotateRingLevelEnterService(this));
		RegisterGameService(new RingDragEffectService(this));
		RegisterGameService(new RingDragCollisionEventService(this));
		RegisterGameService(new RingObjectSolveService(this));
		RegisterGameService(new RingPaletteLockService(this));
		RegisterGameService(new RingLockFeatureService(this));
		RegisterGameService(new RingPartStyleService(this));
		RegisterGameService(new RingLockActiveService(this));
		RegisterGameService(new RingLockPassiveService(this));
		RegisterGameService(new RingFillPartDataService(this));
		RegisterGameService(new RingLevelConfigService(this));
		RegisterGameService(new RingInputBlockService(this));
		RegisterGameService(new RingLockChainService(this));
		RegisterGameService(new RingSolveBreakDelayService(this));
		RegisterGameService(new UIWeakGuideService(this));
		RegisterGameService(new RotateRingLockShakeService(this));
		RegisterGameService(new ScreenShakeService(this));
		RegisterGameService(new GameBordDragLimitStyleService(this));
		RegisterGameService(new RingAutoEscapeService(this));
		RegisterGameService(new CumulativeReleaseService(this));
		RegisterGameService(new LevelStepLimitService(this));
		RegisterGameService(new RotateRingOperationScoreFacade(this));
		RegisterGameService(new CharacterExpressionEncourageService(this));
		RegisterGameService(new LevelEnterPipelineService(this));
		RegisterGameService(new LevelVictorySettlementService(this));
		RegisterGameService(new PresentationResultCollectService(this));
	}

	public void RegisterGameService(IGameplayService service)
	{
		if (service != null)
		{
			Type type = service.GetType();
			if (gameplayServices.TryGetValue(type, out var _))
			{
			}
			gameplayServices[type] = service;
		}
	}

	public void UnregisterGameService(IGameplayService service)
	{
		if (service != null)
		{
			Type type = service.GetType();
			if (gameplayServices.TryGetValue(type, out var value) && value == service)
			{
				gameplayServices.Remove(type);
			}
		}
	}

	public T GetGameService<T>() where T : class, IGameplayService
	{
		if (!gameplayServices.TryGetValue(typeof(T), out var value))
		{
			return null;
		}
		return (T)value;
	}

	public void CollectServices<T>(List<T> buffer) where T : class
	{
		if (buffer == null)
		{
			return;
		}
		buffer.Clear();
		foreach (IGameplayService value in gameplayServices.Values)
		{
			if (value is T item)
			{
				buffer.Add(item);
			}
		}
	}

	private T GetOrCreateService<T>(Func<T> factory) where T : class, IGameplayService
	{
		T gameService = GetGameService<T>();
		if (gameService != null)
		{
			return gameService;
		}
		gameService = factory();
		RegisterGameService(gameService);
		return gameService;
	}

	public void DisposeGameServices()
	{
		if (gameplayServices.Count == 0)
		{
			return;
		}
		List<IGameplayService> list = new List<IGameplayService>(gameplayServices.Values);
		gameplayServices.Clear();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			IGameplayService gameplayService = list[num];
			if (gameplayService != null)
			{
				try
				{
					gameplayService.Dispose();
				}
				catch (Exception ex)
				{
					Log.LogError("[GamePlay][Manager]", "Dispose GameplayService 失败: {0} {1}", new object[2]
					{
						gameplayService.GetType().Name,
						ex.Message
					});
				}
			}
		}
	}

	public IReadOnlyList<IGameplayObject> GetGameplayObjects()
	{
		if (!((Object)(object)gameLevelObject != (Object)null))
		{
			return null;
		}
		return ObjectsService.GameplayObjects;
	}

	protected override void Awake()
	{
		base.Awake();
		SolveOnDragExit = FunctionDataCenter.Get<GameBord>()?.GameBordDrag ?? false;
		Gameplay.Register(this);
		if ((Object)(object)levelCaptureCamera != (Object)null)
		{
			((Behaviour)levelCaptureCamera).enabled = false;
		}
		RegisterAllGameServices();
		SubscribeScreenModeChanged();
	}

	private void SubscribeScreenModeChanged()
	{
		if ((Object)(object)MonoSingleton<ScreenManager>.Instance != (Object)null)
		{
			ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
			instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Combine(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
		}
	}

	private void OnScreenModeChanged(EScreenMode screenMode, int width, int height)
	{
		if (!isLoadingLevel && !((Object)(object)gameLevelObject == (Object)null) && !((Object)(object)_gameplayCamera == (Object)null))
		{
			Canvas.ForceUpdateCanvases();
			GetGameService<RotateCameraAdaptation>()?.BindFromUIGame();
		}
	}

	private void Start()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!GameplayCameraManager.IsPlayFromLoading)
		{
			((Component)eventSystem).gameObject.SetActive(true);
			testLevel = Math.Max(testLevel, 1);
			UniTaskVoid val;
			if ((Object)(object)levelContent != (Object)null && levelContent.childCount <= 0)
			{
				val = RunLevelEnterAsync(testLevel);
				((UniTaskVoid)(ref val)).Forget();
			}
			else
			{
				gameLevelObject = ((Component)levelContent.GetChild(0)).gameObject;
				val = RunPreplacedLevelAsync();
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	private void Update()
	{
		bool sleepTimeout = GameApp.UI.IsOpen("UIGame");
		SetSleepTimeout(sleepTimeout);
	}

	private void LateUpdate()
	{
		GetGameService<RingDraggingSolveService>()?.TickFrame();
		GetGameService<RingDragFixedPinVibrateService>()?.TickFrameEnd();
		GetGameService<RingAutoEscapeService>()?.TickFrame();
		GetGameService<GameBordDragLimitStyleService>()?.TickLateUpdate();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (isApplicationBackground != pauseStatus)
		{
			isApplicationBackground = pauseStatus;
			if (pauseStatus)
			{
				FlushVictoryProgressOnKill();
				RingSave.TryFlushToDisk();
				WeakGuideService?.OnEnterBackground();
			}
			else
			{
				WeakGuideService?.OnEnterForeground();
				StartEntryColdLaunch.ClearLevelExitRecord();
			}
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		bool flag = !hasFocus;
		if (isApplicationBackground != flag)
		{
			isApplicationBackground = flag;
			if (flag)
			{
				FlushVictoryProgressOnKill();
				RingSave.TryFlushToDisk();
				WeakGuideService?.OnEnterBackground();
			}
			else
			{
				WeakGuideService?.OnEnterForeground();
				StartEntryColdLaunch.ClearLevelExitRecord();
			}
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)MonoSingleton<ScreenManager>.Instance != (Object)null)
		{
			ScreenManager instance = MonoSingleton<ScreenManager>.Instance;
			instance.ScreenModeChanged = (Action<EScreenMode, int, int>)Delegate.Remove(instance.ScreenModeChanged, new Action<EScreenMode, int, int>(OnScreenModeChanged));
		}
		FlushVictoryProgressOnKill();
		RingSave.TryFlushToDisk();
		Singleton<GameInputManager>.Instance.DisableGameplayInput(notifyCapturedHandler: false);
		GetGameService<RotateRingPropUseService>()?.End();
		RingColliderQuery.Clear();
		DisposeGameServices();
		GameApp.Res.ReleaseGroup("UnlockRing_Effect");
		Gameplay.Unregister(this);
		SetSleepTimeout(disable: false);
	}

	private void SetSleepTimeout(bool disable)
	{
		if (disable != isSleepDisabled)
		{
			isSleepDisabled = disable;
			Screen.sleepTimeout = (disable ? (-1) : (-2));
		}
	}

	internal void AssignCurrentLevelRoot(GameObject levelRoot)
	{
		gameLevelObject = levelRoot;
	}

	internal void AssignCurrentLevelInfo(GameLevelInfo levelInfo)
	{
		currentLevelInfo = levelInfo;
	}

	internal void AssignLevelGameplayCameraState(string cameraLevelKey, Camera gameplayCamera)
	{
		_cameraLevelKey = cameraLevelKey;
		_gameplayCamera = gameplayCamera;
	}

	public void GoToNextLevel()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		RRAdContext context = RRAdContext.FromLevelProxy();
		AdService.RequestLevelTransitionBeforeInterstitial(InterstitialTrigger.NewGame, context);
		UniTaskVoid val = RunLevelEnterAsync(GameApp.Proxy.Get<LevelModelProxy>().Level);
		((UniTaskVoid)(ref val)).Forget();
	}

	public void ReplayCurrentLevel()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		ReportGameEndTrack("重新开始");
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		RRAdContext context = RRAdContext.FromLevelProxy();
		AdService.RequestLevelTransitionBeforeInterstitial(InterstitialTrigger.Restart, context);
		GameApp.Proxy.Get<LevelModelProxy>().TryLevelAgain();
		GameWinIQService.Instance.RecordLevelRestart(level);
		GameApp.Proxy.Get<RankModelProxy>()?.RestartGame();
		RingSave.ClearSave(writeToDisk: true);
		UniTaskVoid val = RunLevelEnterAsync(GameApp.Proxy.Get<LevelModelProxy>().Level);
		((UniTaskVoid)(ref val)).Forget();
	}

	public void ExitToHome()
	{
		if (ShouldReportMidGameExit())
		{
			ReportGameEndTrack("中途退出");
			AdService.NotifyMidGameExit(GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0);
		}
		RingSave.TrySaveInProgress();
		GetGameService<LevelVictorySettlementService>()?.ClearLevel();
		GetGameService<RotateRingPropUseService>()?.End();
		GetGameService<UIWeakGuideService>()?.OnLevelExit();
		ClearLevel(releaseGroup: true);
		isLevelCompleted = false;
		hasPersistedVictoryProgress = false;
		AdService.ClearPendingFrontAd("ExitToHome");
	}

	UniTaskVoid IGameplayManager.RunLevelEnterAsync(int level)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return RunLevelEnterAsync(level);
	}

	private bool TryBeginLevelEnterLoad()
	{
		if (isLoadingLevel)
		{
			EnterLevelCommand.ReleaseEnterGate();
			return false;
		}
		isLoadingLevel = true;
		return true;
	}

	private void EndLevelEnterLoad()
	{
		isLoadingLevel = false;
		EnterLevelCommand.ReleaseEnterGate();
	}

	[AsyncStateMachine(typeof(_003CRunLevelEnterAsync_003Ed__176))]
	private UniTaskVoid RunLevelEnterAsync(int level)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunLevelEnterAsync_003Ed__176 _003CRunLevelEnterAsync_003Ed__ = default(_003CRunLevelEnterAsync_003Ed__176);
		_003CRunLevelEnterAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunLevelEnterAsync_003Ed__._003C_003E4__this = this;
		_003CRunLevelEnterAsync_003Ed__.level = level;
		_003CRunLevelEnterAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunLevelEnterAsync_003Ed__._003C_003Et__builder)).Start<_003CRunLevelEnterAsync_003Ed__176>(ref _003CRunLevelEnterAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunLevelEnterAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void ClearLevel(bool releaseGroup)
	{
		GetGameService<LevelVictorySettlementService>()?.ClearLevel();
		GetGameService<RingAutoEscapeService>()?.ClearLevel();
		Singleton<GameInputManager>.Instance.DisableGameplayInput();
		GetGameService<RotateRingPropUseService>()?.ClearLevel();
		RingDragCollisionEventService?.SendAndClearData();
		GuideRT?.ClearLevel();
		GetGameService<MultiTouchGuideService>()?.ClearLevel();
		GetGameService<RotateRingLevelEnterService>()?.Cancel();
		GetGameService<RingLockPassiveService>()?.ClearLevel();
		GetGameService<RingLockActiveService>()?.ClearLevel();
		RingBoom?.ClearLevel();
		GetGameService<RingObjectSolveService>()?.ClearLevel();
		GetGameService<GameBordDragLimitStyleService>()?.ClearLevel();
		GetGameService<RotateRingOperateHighlightService>()?.ClearLevel();
		GetGameService<RotateRingBackgroundAdaptation>()?.ClearLevel();
		GetGameService<RingAcornCollectService>()?.Clear();
		GetGameService<LevelStepLimitService>()?.ClearLevel();
		LockChain?.ClearChainLockUi();
		PaletteLock?.ClearLevel();
		RingColliderQuery.Clear();
		RingSave.ClearManifest();
		PartStyle?.ClearLevel();
		FillPartData?.ClearLevel();
		LevelConfig?.ClearLevel();
		ObjectsService.ClearLevel();
		if ((Object)(object)gameLevelObject == (Object)null)
		{
			return;
		}
		Object.Destroy((Object)(object)gameLevelObject);
		gameLevelObject = null;
		currentLevelInfo = null;
		PresentationMode.SuppressByPaletteLock = false;
		if (releaseGroup)
		{
			if (!string.IsNullOrEmpty(LoadedLevelPath))
			{
				GameApp.Res.UnloadAsset(LoadedLevelPath, (string)null, 1);
			}
			LoadedLevelPath = null;
		}
	}

	public void PurgeDestroyedGameplayObjects()
	{
		ObjectsService.DestroyedReferences();
	}

	public void ReplayEnterAnim()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = ReplayEnterAnimAsync();
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CReplayEnterAnimAsync_003Ed__180))]
	private UniTaskVoid ReplayEnterAnimAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CReplayEnterAnimAsync_003Ed__180 _003CReplayEnterAnimAsync_003Ed__ = default(_003CReplayEnterAnimAsync_003Ed__180);
		_003CReplayEnterAnimAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CReplayEnterAnimAsync_003Ed__._003C_003E4__this = this;
		_003CReplayEnterAnimAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CReplayEnterAnimAsync_003Ed__._003C_003Et__builder)).Start<_003CReplayEnterAnimAsync_003Ed__180>(ref _003CReplayEnterAnimAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CReplayEnterAnimAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void EnableLevelGameplayInput(Camera camera = null)
	{
		Singleton<GameInputManager>.Instance.EnableGameplayInput(camera ?? GameplayCameraManager.GameplayCamera);
	}

	internal void ResetVictoryFlags()
	{
		isLevelCompleted = false;
		hasPersistedVictoryProgress = false;
	}

	[AsyncStateMachine(typeof(_003CRunPreplacedLevelAsync_003Ed__185))]
	private UniTaskVoid RunPreplacedLevelAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRunPreplacedLevelAsync_003Ed__185 _003CRunPreplacedLevelAsync_003Ed__ = default(_003CRunPreplacedLevelAsync_003Ed__185);
		_003CRunPreplacedLevelAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunPreplacedLevelAsync_003Ed__._003C_003E4__this = this;
		_003CRunPreplacedLevelAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunPreplacedLevelAsync_003Ed__._003C_003Et__builder)).Start<_003CRunPreplacedLevelAsync_003Ed__185>(ref _003CRunPreplacedLevelAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunPreplacedLevelAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void CompleteLegacyOperation(IGameplayObject gameplayObject, LegacyOperationSource operationSource)
	{
		CompleteLegacyOperation(gameplayObject, operationSource, Array.Empty<GameplayReleasedPart>());
	}

	public void CompleteLegacyOperation(IGameplayObject gameplayObject, LegacyOperationSource operationSource, IReadOnlyList<GameplayReleasedPart> releasedParts)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (gameplayObject != null && releasedParts != null && ObjectsService.TryGetPartId(gameplayObject, out var partId))
		{
			bool flag = operationSource == LegacyOperationSource.Blocked;
			GameplayOperationResult operation = new GameplayOperationResult(partId, operationSource, (!flag) ? GameplayOperationCompletion.Committed : GameplayOperationCompletion.Blocked, releasedParts.Count, releasedParts, releasedParts.Count > 0);
			GetGameService<RotateRingOperationScoreFacade>()?.Record(operation);
			RingSave.TrySaveInProgress();
			GetGameService<PresentationResultCollectService>()?.FlushAndSubmit();
		}
	}

	public void CompleteBombExplosion(Action performExplosion)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (performExplosion == null)
		{
			return;
		}
		BeginLegacyOperation(null, LegacyOperationSource.Bomb);
		IReadOnlyList<GameplayReleasedPart> readOnlyList;
		try
		{
			performExplosion();
		}
		finally
		{
			readOnlyList = EndLegacyOperation(null, LegacyOperationSource.Bomb);
			RRDataTrack.Instance.LegacySessionCollector.CommitBombBatch();
			RingBoom?.ClearRegisteredBombExplosionObjects();
		}
		if (readOnlyList != null && readOnlyList.Count != 0)
		{
			GetGameService<LevelStepLimitService>()?.OnBombExplosionStep();
			GameplayOperationResult operation = new GameplayOperationResult(readOnlyList[0].PartId, LegacyOperationSource.Bomb, GameplayOperationCompletion.Committed, readOnlyList.Count, readOnlyList, hasSolved: true);
			RotateRingOperationScoreFacade gameService = GetGameService<RotateRingOperationScoreFacade>();
			CumulativeReleaseService gameService2 = GetGameService<CumulativeReleaseService>();
			gameService?.BeginDeferEncouragements();
			gameService2?.BeginDeferPresentations();
			try
			{
				gameService?.Record(operation);
			}
			finally
			{
				gameService2?.EndDeferPresentations();
				gameService?.EndDeferEncouragements();
			}
			GetGameService<PresentationResultCollectService>()?.FlushAndSubmit();
		}
	}

	private LegacyOperationSource ResolveLegacyTrackOperationSource(IGameplayObject gameplayObject)
	{
		return activeOperationSource;
	}

	private void BeginLegacyOperation(IGameplayObject gameplayObject, LegacyOperationSource operationSource)
	{
		GameApp.Event.Send<RingDragSettleResetEvent>(new RingDragSettleResetEvent(gameplayObject, operationSource));
		activeGameplayObject = gameplayObject;
		activeOperationSource = operationSource;
		ObjectsService.CollectActiveUnsolvedPartIds();
	}

	internal void BeginHammerLegacyOperation(IGameplayObject gameplayObject)
	{
		BeginLegacyOperation(gameplayObject, LegacyOperationSource.Hammer);
	}

	internal IReadOnlyList<GameplayReleasedPart> EndLegacyOperation(IGameplayObject gameplayObject, LegacyOperationSource operationSource)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (gameplayObject != activeGameplayObject || operationSource != activeOperationSource)
		{
			return Array.Empty<GameplayReleasedPart>();
		}
		List<GameplayReleasedPart> list = new List<GameplayReleasedPart>();
		IReadOnlyList<IGameplayObject> gameplayObjects = ObjectsService.GameplayObjects;
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject2 = gameplayObjects[i];
			if (gameplayObject2 != null && gameplayObject2.IsSolved && gameplayObject2 is IThreadingScoreProvider && ObjectsService.TryGetPartId(gameplayObject2, out var partId) && ObjectsService.IsActiveUnsolvedPartId(partId) && (!(gameplayObject2 is IGameplaySolveReason { SolveReason: var solveReason }) || (!solveReason.IsColorLockBatchRelated() && solveReason.IsLegacyGestureStepScorable())))
			{
				GameplayPartData partData = gameplayObject2.PartData;
				int num = 0;
				if (partData.LockedBy != null)
				{
					num += partData.LockedBy.Count;
				}
				if (partData.Locks != null)
				{
					num += partData.Locks.Count;
				}
				string reason = operationSource switch
				{
					LegacyOperationSource.Bomb => "Bomb", 
					LegacyOperationSource.Hammer => "Hammer", 
					_ => "LegacySolve", 
				};
				if (gameplayObject2 is IGameplaySolveReason { SolveReason: var solveReason2 } && solveReason2.IsLegacyGestureStepScorable())
				{
					reason = solveReason2.ToReleasedPartReason();
				}
				list.Add(new GameplayReleasedPart(partId, reason, num));
			}
		}
		ClearLegacyOperationSnapshot();
		return list;
	}

	internal void ClearLegacyOperationSnapshot()
	{
		ObjectsService.ClearActiveUnsolvedPartIds();
		activeGameplayObject = null;
		activeOperationSource = LegacyOperationSource.Blocked;
	}

	internal void OnInteractionStart(IGameplayObject gameplayObject)
	{
		GetGameService<RingAutoEscapeService>()?.Abort(settle: true);
		RRDataTrack.Instance.LegacySessionCollector.RecordFirstOperation();
		BeginLegacyOperation(gameplayObject, LegacyOperationSource.PointerUp);
		WeakGuideService?.OnInteractionStart();
		Singleton<EventManager>.Instance.Send<GameplayGestureEvent>(new GameplayGestureEvent(gameplayObject));
	}

	internal void OnInteractionStart()
	{
		RRDataTrack.Instance.LegacySessionCollector.RecordFirstOperation();
		WeakGuideService?.OnInteractionStart();
		Singleton<EventManager>.Instance.Send<GameplayGestureEvent>(new GameplayGestureEvent());
	}

	internal void OnPlayerAction(IGameplayObject gameplayObject)
	{
		RRDataTrack.Instance.LegacySessionCollector.EndMoveGesture(gameplayObject);
		bool hasSolved = SolveAndGetHasSolved();
		LevelStepLimitService gameService = GetGameService<LevelStepLimitService>();
		if (gameService != null && gameService.IsEnabled && gameplayObject is PullObject pull)
		{
			gameService.OnPointerUp(pull, hasSolved);
		}
		IReadOnlyList<GameplayReleasedPart> readOnlyList = EndLegacyOperation(gameplayObject, LegacyOperationSource.PointerUp);
		if (readOnlyList.Count <= 0)
		{
			RingDraggingSolveService gameService2 = GetGameService<RingDraggingSolveService>();
			if (gameService2 != null && gameService2.IsReleaseSettleDone)
			{
				goto IL_006a;
			}
		}
		CompleteLegacyOperation(gameplayObject, LegacyOperationSource.PointerUp, readOnlyList);
		goto IL_006a;
		IL_006a:
		OnPlayerAction();
	}

	internal void OnPlayerAction()
	{
		WeakGuideService?.OnPlayerAction();
		Singleton<EventManager>.Instance.Send<GameplayPointerReleasedEvent>(new GameplayPointerReleasedEvent());
	}

	internal void OnRingPointerUp(RingObject ring)
	{
		RRDataTrack.Instance.LegacySessionCollector.EndMoveGesture(ring);
		WeakGuideService?.OnPlayerAction();
		Singleton<EventManager>.Instance.Send<GameplayPointerReleasedEvent>(new GameplayPointerReleasedEvent());
		GameApp.Event.Send<RingPointerUpEvent>(new RingPointerUpEvent(ring));
		bool hasSolved = SolveAndGetHasSolved();
		LevelStepLimitService gameService = GetGameService<LevelStepLimitService>();
		if (gameService != null && gameService.IsEnabled)
		{
			gameService.OnPointerUp(ring, hasSolved);
		}
		IReadOnlyList<GameplayReleasedPart> readOnlyList = EndLegacyOperation(ring, LegacyOperationSource.PointerUp);
		if (readOnlyList.Count <= 0)
		{
			RingDraggingSolveService gameService2 = GetGameService<RingDraggingSolveService>();
			if (gameService2 != null && gameService2.IsReleaseSettleDone)
			{
				return;
			}
		}
		CompleteLegacyOperation(ring, LegacyOperationSource.PointerUp, readOnlyList);
	}

	internal void OnInteractableSolved(IGameplayObject gameplayObject)
	{
		int id = gameplayObject.PartData.Id;
		if (id > 0)
		{
			RRDataTrack.Instance.LegacySessionCollector.RecordAssemblyCleared(id, ResolveLegacyTrackOperationSource(gameplayObject));
		}
		Singleton<EventManager>.Instance.Send<RingSolvedEvent>(new RingSolvedEvent(gameplayObject));
		GetGameService<RingAcornCollectService>()?.TryCollect(gameplayObject, RingBoom?.IsInBombCascadeScope ?? false);
		ObjectsSolveAndCheckVictory();
		RingSave.TrySaveInProgress();
	}

	public void ObjectsSolveAndCheckVictory()
	{
		if (!isLevelCompleted && !((Object)(object)gameLevelObject == (Object)null))
		{
			RingObjectSolveService solveService = SolveService;
			solveService?.TryUpdateAll();
			if (solveService != null && solveService.UnsolvedCount == 0 && !isLevelCompleted)
			{
				GetGameService<LevelVictorySettlementService>()?.TryScheduleSettlement(completeLevelDelay);
			}
			RefreshRingDragAreaLockZ();
		}
	}

	internal bool SolveAndGetHasSolved()
	{
		RingObjectSolveService solveService = SolveService;
		int num = solveService?.UnsolvedCount ?? 0;
		ObjectsSolveAndCheckVictory();
		return (solveService?.UnsolvedCount ?? 0) < num;
	}

	internal bool ShouldAttemptVictorySettlement()
	{
		if (!isLevelCompleted)
		{
			return (Object)(object)gameLevelObject != (Object)null;
		}
		return false;
	}

	internal void MarkVictorySettlementStarted()
	{
		if (!isLevelCompleted)
		{
			isLevelCompleted = true;
			OperateHighlightService?.ClearLevel();
			WeakGuideService?.OnLevelFinished();
		}
	}

	internal void RunVictorySettlementContinuation(CancellationToken settlementCt)
	{
		ContinueVictorySettlement(settlementCt);
	}

	internal void RefreshRingDragAreaLockZ()
	{
		IReadOnlyList<RingObject> readOnlyList = ObjectsService?.Rings;
		if (readOnlyList == null)
		{
			return;
		}
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			RingObject ringObject = readOnlyList[i];
			if ((Object)(object)ringObject != (Object)null && !ringObject.IsSolved)
			{
				ringObject.UpdateDragAreaLockZ();
			}
		}
	}

	private void ContinueVictorySettlement(CancellationToken settlementCt)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		ReportGameEndTrack("成功");
		RingSave.ClearSave(writeToDisk: false);
		TryPersistVictoryProgress();
		LevelVictoryService.VictoryProgressSnapshot snapshot = victorySnapshot;
		if (!GameplayCameraManager.IsPlayFromLoading)
		{
			testLevel++;
			return;
		}
		UniTaskExtensions.Forget(SettlementPresentationGate.WaitThen(delegate
		{
			WeakGuideService?.FinishGuidePresentation();
			LevelVictoryService.ContinueToSettlement(snapshot.CompletedLevel, snapshot.CompletedScore, snapshot.IsMainToEndlessTransition, snapshot.CompletedPropUses, snapshot.GameWinIqSnapshot);
			if (GameApp.UI.IsOpen("UISettingGame"))
			{
				GameApp.UI.CloseView("UISettingGame", (object)null, false);
			}
			if (GameApp.UI.IsOpen("WeakGuideView"))
			{
				GameApp.UI.CloseView("WeakGuideView", (object)null, false);
			}
		}, settlementCt));
	}

	private void TryPersistVictoryProgress()
	{
		if (isLevelCompleted && !hasPersistedVictoryProgress)
		{
			hasPersistedVictoryProgress = true;
			victorySnapshot = LevelVictoryService.PersistVictoryProgress();
		}
	}

	public bool TryPrepareForGmVictory()
	{
		if (isLevelCompleted || (Object)(object)gameLevelObject == (Object)null || isLoadingLevel)
		{
			return false;
		}
		isLevelCompleted = true;
		hasPersistedVictoryProgress = true;
		RingSave.ClearSave(writeToDisk: false);
		return true;
	}

	public void FlushVictoryProgressOnKill()
	{
		TryPersistVictoryProgress();
	}

	private bool ShouldReportMidGameExit()
	{
		if ((Object)(object)gameLevelObject == (Object)null || isLevelCompleted)
		{
			return false;
		}
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (levelModelProxy == null)
		{
			return false;
		}
		int gameIntState = levelModelProxy.GetGameIntState();
		if (gameIntState != 1)
		{
			return gameIntState == 3;
		}
		return true;
	}

	internal void ReportGameEndTrack(string status)
	{
		LevelModelProxy levelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (!RRDataTrack.ShouldSkipDuplicateGameEnd(status, isLevelCompleted, levelProxy))
		{
			RandomLevelFirstClearWindow.OnGameEnd(status);
			GamePartTrackSnapshot remainingSnapshot = GamePartTrackSnapshot.FromLegacyRemaining(ObjectsService);
			RRDataTrack.Instance.ReportGameEnd(status, remainingSnapshot);
		}
	}
}
