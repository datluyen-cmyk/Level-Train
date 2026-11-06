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
using Lean.Touch;
using RotateRing.GamePlay;
using RotateRings.Ad;
using RotateRings.GamePlay;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Save;
using RotateRings.GameWinIQ;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.SpecialLevel;
using RotateRings.UI.Game;
using RotateRings.UI.Level;
using Rotaterings.Rank;
using UnityEngine;
using UnityEngine.EventSystems;
using UnlockRing;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Runtime;
using UnlockRing.Game.Gameplay;
using UnlockRing.IntegrationSample;
using UnlockRing.Prop;
using UnlockRing.Serialization;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.Experiment;

[RequireComponent(typeof(CoreBoardRenderer))]
public class RingGameManager : MonoBehaviour, IGameplayManager, IPropGameManager, IRingGuideRTManager
{
	private readonly struct HammerHitCandidate
	{
		public PartId PartId { get; }

		public HammerPartCategory Category { get; }

		public int ThreadingCount { get; }

		public double Distance { get; }

		public HammerHitCandidate(PartId partId, HammerPartCategory category, int threadingCount, double distance)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			PartId = partId;
			Category = category;
			ThreadingCount = threadingCount;
			Distance = distance;
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CCallDelayedAsync_003Ed__103 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public float delay;

		public CancellationToken ct;

		public Action action;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(delay), false, (PlayerLoopTiming)8, ct);
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CCallDelayedAsync_003Ed__103>(ref val2, ref this);
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
				if (!ct.IsCancellationRequested)
				{
					action?.Invoke();
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
	private struct _003COnLoadLevel_003Ed__79 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RingGameManager _003C_003E4__this;

		public int levelId;

		private CancellationToken _003CloadCt_003E5__2;

		private Camera _003Ccam_003E5__3;

		private LevelDefinition _003Clevel_003E5__4;

		private Awaiter<string> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingGameManager ringGameManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				Awaiter<string> val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_01cf;
					}
					_003CloadCt_003E5__2 = ringGameManager.ringLevelEnterService.BeginSession();
					_003Ccam_003E5__3 = ringGameManager.gameplayCamera ?? ringGameManager.boardRenderer.Cam;
					if ((Object)(object)_003Ccam_003E5__3 != (Object)null)
					{
						GameplayCameraManager.GameplayCamera = _003Ccam_003E5__3;
						if ((Object)(object)ringGameManager.boardRenderer.Cam == (Object)null)
						{
							ringGameManager.boardRenderer.Cam = _003Ccam_003E5__3;
						}
					}
					ringGameManager.isLevelCompleted = false;
					ringGameManager.scoredDisappearedParts.Clear();
					val2 = LevelConfigService.GetCoreLevelJson(levelId).GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<string>, _003COnLoadLevel_003Ed__79>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<string>);
					num = (_003C_003E1__state = -1);
				}
				string result = val2.GetResult();
				if (!_003CloadCt_003E5__2.IsCancellationRequested)
				{
					UnifiedLevelPackReadResult levelPackReadResult = UnifiedLevelPackReader.Read(result);
					_003Clevel_003E5__4 = RenderModeComparator.ReadLevelDefinition(levelPackReadResult);
					ringGameManager.boardRenderer.renderMode = BoardRenderMode.Ribbon;
					ringGameManager.ReleaseGuideRenderTexture();
					ringGameManager.boardRenderer.LoadLevel(_003Clevel_003E5__4, levelPackReadResult, LevelConfigService.GetPhysicsLevelPathKey(levelId));
					if (!_003CloadCt_003E5__2.IsCancellationRequested)
					{
						UniTask val3 = ringGameManager.ringLevelEnterService.PlayAsync(ringGameManager.boardRenderer.GetAllBoardPartTransforms());
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003COnLoadLevel_003Ed__79>(ref val, ref this);
							return;
						}
						goto IL_01cf;
					}
				}
				goto end_IL_000e;
				IL_01cf:
				((Awaiter)(ref val)).GetResult();
				if (!_003CloadCt_003E5__2.IsCancellationRequested)
				{
					Singleton<EventManager>.Instance.Send<LevelEnterCompletedEvent>(new LevelEnterCompletedEvent
					{
						Level = levelId
					});
					AdService.NotifyFrontAdEnterPresentationReady();
					GameApp.Proxy.Get<LevelModelProxy>().ResetCurrentScore();
					Singleton<EventManager>.Instance.Send<LevelChanged>(new LevelChanged
					{
						Level = levelId
					});
					ringGameManager.EnableGameplayInput(_003Ccam_003E5__3);
					ringGameManager.ReportGameStartTrack(_003Clevel_003E5__4);
					GameWinIQService.Instance.OnLevelLoaded(levelId);
					SpecialLevelModule.CommitInsertOnLevelLoaded();
					if (AdService.IsFrontAdHasPending())
					{
						AdService.NotifyFrontAdLevelReady();
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CloadCt_003E5__2 = default(CancellationToken);
				_003Ccam_003E5__3 = null;
				_003Clevel_003E5__4 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CloadCt_003E5__2 = default(CancellationToken);
			_003Ccam_003E5__3 = null;
			_003Clevel_003E5__4 = null;
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CReplayEnterAnimAsync_003Ed__81 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingGameManager _003C_003E4__this;

		private Camera _003Ccam_003E5__2;

		private CancellationToken _003CloadCt_003E5__3;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingGameManager ringGameManager = _003C_003E4__this;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_00d8;
				}
				if (!((Object)(object)ringGameManager.boardRenderer == (Object)null) && ringGameManager.ringLevelEnterService != null)
				{
					IReadOnlyList<Transform> allBoardPartTransforms = ringGameManager.boardRenderer.GetAllBoardPartTransforms();
					if (allBoardPartTransforms.Count != 0)
					{
						_003Ccam_003E5__2 = ringGameManager.gameplayCamera ?? ringGameManager.boardRenderer.Cam;
						ringGameManager.DisableGameplayInput();
						_003CloadCt_003E5__3 = ringGameManager.ringLevelEnterService.BeginSession();
						UniTask val2 = ringGameManager.ringLevelEnterService.PlayAsync(allBoardPartTransforms);
						val = ((UniTask)(ref val2)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CReplayEnterAnimAsync_003Ed__81>(ref val, ref this);
							return;
						}
						goto IL_00d8;
					}
				}
				goto end_IL_000e;
				IL_00d8:
				((Awaiter)(ref val)).GetResult();
				if (!_003CloadCt_003E5__3.IsCancellationRequested)
				{
					ringGameManager.EnableGameplayInput(_003Ccam_003E5__2);
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ccam_003E5__2 = null;
				_003CloadCt_003E5__3 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ccam_003E5__2 = null;
			_003CloadCt_003E5__3 = default(CancellationToken);
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
	private struct _003CRunLevelEnterAsync_003Ed__78 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingGameManager _003C_003E4__this;

		public int level;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingGameManager ringGameManager = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_002b;
				}
				if (!ringGameManager.isLoadingLevel)
				{
					ringGameManager.isLoadingLevel = true;
					goto IL_002b;
				}
				EnterLevelCommand.ReleaseEnterGate();
				goto end_IL_000e;
				IL_002b:
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						Singleton<EventManager>.Instance.Send<LevelLoadingStartedEvent>(new LevelLoadingStartedEvent
						{
							Level = level
						});
						level = SpecialLevelModule.ResolveEnterLevel(level);
						LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
						levelModelProxy.ResetLevel(level);
						levelModelProxy.SyncLevel(level);
						ringGameManager.ClearLevel();
						UniTask val = ringGameManager.OnLoadLevel(level);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CRunLevelEnterAsync_003Ed__78>(ref val2, ref this);
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
						ringGameManager.isLoadingLevel = false;
						EnterLevelCommand.ReleaseEnterGate();
					}
				}
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

	private RingGuideRTService guideRTService;

	private const int ScorePerPlayerPart = 100;

	private const string LevelPackGroup = "LevelPackGroup";

	private const string TAG = "[RingGameController]";

	[Header("UnlockRing")]
	[SerializeField]
	private CoreBoardRenderer boardRenderer;

	[SerializeField]
	private SimpleGestureInput gestureInput;

	[Header("Camera")]
	[SerializeField]
	private Camera gameplayCamera;

	[Header("Level Pack")]
	[Tooltip("PlayableLevelPack 二进制资源。留空则通过资源系统异步加载。")]
	[SerializeField]
	private TextAsset levelPackAsset;

	[Header("Config")]
	[SerializeField]
	private float completeLevelDelay = 1f;

	[SerializeField]
	private EventSystem eventSystem;

	[SerializeField]
	private int testLevel = 1;

	private PlayableLevelPackUnityLoader levelPackLoader;

	private bool isLevelCompleted;

	private bool isApplicationBackground;

	private bool levelPackLoadedViaYooAssets;

	private CancellationTokenSource completeLevelCts;

	private RingSolveSuccessService ringSolveSuccessService;

	private RingLevelEnterService ringLevelEnterService;

	private RingHintService ringHintService;

	private bool isLoadingLevel;

	private RingPropUseService propUseService;

	private RingDragBlockEffectService dragBlockEffectService;

	private readonly HashSet<PartId> scoredDisappearedParts = new HashSet<PartId>();

	public int GuideRenderLayer => 0;

	public bool UsesHammerPointerTargeting => true;

	public static RingGameManager Instance { get; private set; }

	public GameplayMode Mode => GameplayMode.UnlockRing;

	public bool IsLevelCompleted => isLevelCompleted;

	public bool HasUnsolvedGameplayObjects
	{
		get
		{
			if ((Object)(object)boardRenderer != (Object)null)
			{
				return !boardRenderer.IsSolved;
			}
			return false;
		}
	}

	public CoreBoardRenderer BoardRenderer => boardRenderer;

	public RingHintService RingHint => ringHintService;

	public RingPropUseService PropUseService => propUseService;

	public RingDragBlockEffectService DragBlockEffectService => dragBlockEffectService;

	public IBoardGestureOverride BoardGestureOverride => propUseService;

	public RingGuideRTService GuideRT => guideRTService;

	public GameObject CurrentLevelObject => boardRenderer?.BoardRoot;

	public bool IsLoadingLevel => isLoadingLevel;

	public SimpleGestureInput GestureInput => gestureInput;

	public Camera ResolveBoardCamera()
	{
		if (!((Object)(object)boardRenderer != (Object)null))
		{
			return null;
		}
		return boardRenderer.Cam;
	}

	public IReadOnlyList<Transform> GetAllGuideTargetTransforms()
	{
		if (!((Object)(object)boardRenderer != (Object)null))
		{
			return Array.Empty<Transform>();
		}
		return boardRenderer.GetAllBoardPartTransforms();
	}

	public bool TryGetPartGuideTarget(PartId partId, out Transform target)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		target = null;
		if ((Object)(object)boardRenderer == (Object)null || !boardRenderer.TryGetPartGameObject(partId, out var gameObject))
		{
			return false;
		}
		target = gameObject.transform;
		return (Object)(object)target != (Object)null;
	}

	private void EnsureGuideRT()
	{
		DisposeGuideRT();
		guideRTService = new RingGuideRTService(this);
	}

	private void DisposeGuideRT()
	{
		guideRTService?.Dispose();
		guideRTService = null;
	}

	private void ReleaseGuideRenderTexture()
	{
		guideRTService?.ReleaseGuideRenderTexture();
	}

	public bool CanUseHammer()
	{
		if (!IsLevelCompleted)
		{
			return (Object)(object)boardRenderer != (Object)null;
		}
		return false;
	}

	public void BeginHammerRemoveMode(Action<HammerRemoveSelection> onSelected)
	{
	}

	public void EndHammerRemoveMode()
	{
	}

	public bool TryExecuteHammerRemove(IGameplayObject target)
	{
		return false;
	}

	public PartId? ResolveHammerHit(Vector2 world)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		CoreBoardRenderer coreBoardRenderer = boardRenderer;
		CoreGameController coreGameController = coreBoardRenderer?.Controller;
		if ((Object)(object)coreBoardRenderer == (Object)null || coreGameController == null)
		{
			return null;
		}
		double defaultHitSlopWorld = coreBoardRenderer.GetDefaultHitSlopWorld();
		return ResolveHammerHit(coreGameController.Level, coreGameController.State, coreGameController, world, defaultHitSlopWorld);
	}

	public bool TryHammerForceRemove(PartId partId, out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = default(Vector3);
		if (IsLevelCompleted || (Object)(object)boardRenderer == (Object)null)
		{
			return false;
		}
		CoreBoardRenderer coreBoardRenderer = boardRenderer;
		CoreGameController controller = coreBoardRenderer.Controller;
		if (controller == null)
		{
			return false;
		}
		if (controller.IsGestureActive)
		{
			controller.CancelGesture();
			coreBoardRenderer.ClearGesture();
		}
		ForceRemoveResult val = controller.ForceRemove(partId);
		if (val == null || (int)val.Status != 0)
		{
			return false;
		}
		if (!coreBoardRenderer.ApplyForceRemoveResult(val))
		{
			return false;
		}
		gestureInput?.RequestDomainRebuild();
		return coreBoardRenderer.TryGetPartWorldPosition(partId, out worldPosition);
	}

	public bool TryGetHammerTargetWorldPosition(PartId partId, out Vector3 worldPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		worldPosition = default(Vector3);
		if ((Object)(object)boardRenderer != (Object)null)
		{
			return boardRenderer.TryGetPartWorldPosition(partId, out worldPosition);
		}
		return false;
	}

	public void ResetPropGestureCapture()
	{
		gestureInput?.ResetPointerCapture();
	}

	private static PartId? ResolveHammerHit(LevelDefinition level, BoardState state, CoreGameController controller, Vector2 world, double hitSlop)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (level == null || state == null || controller == null)
		{
			return null;
		}
		if (!double.IsFinite(hitSlop) || hitSlop < 0.0)
		{
			return null;
		}
		Vector2 point = default(Vector2);
		((Vector2)(ref point))._002Ector((double)world.x, (double)world.y);
		List<HammerHitCandidate> list = new List<HammerHitCandidate>();
		foreach (PartDefinition part in level.Parts)
		{
			if (state.Parts.TryGetValue(part.Id, out var value) && !value.Released && !value.Removed && CoreHitTester.IsPlayerMovablePart(part) && CoreHitTester.TryMeasurePartHitDistance(part, value, point, hitSlop, out var distance))
			{
				list.Add(new HammerHitCandidate(part.Id, ClassifyHammerCategory(part, controller), CountThreadingRelations(state, part.Id), distance));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return SelectBestHammerHit(list);
	}

	private static PartId? SelectBestHammerHit(List<HammerHitCandidate> candidates)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		HammerHitCandidate current = candidates[0];
		for (int i = 1; i < candidates.Count; i++)
		{
			HammerHitCandidate candidate = candidates[i];
			if (IsHammerHitBetter(in candidate, in current))
			{
				current = candidate;
			}
		}
		return current.PartId;
	}

	private static bool IsHammerHitBetter(in HammerHitCandidate candidate, in HammerHitCandidate current)
	{
		if (candidate.Category != current.Category)
		{
			return candidate.Category < current.Category;
		}
		if (candidate.ThreadingCount != current.ThreadingCount)
		{
			return candidate.ThreadingCount > current.ThreadingCount;
		}
		return candidate.Distance < current.Distance;
	}

	private static int CountThreadingRelations(BoardState state, PartId partId)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (ConstraintRelation constraint in state.Constraints)
		{
			if ((int)constraint.Kind != 0)
			{
				continue;
			}
			PartId val = constraint.FirstPart;
			if (!((PartId)(ref val)).Equals(partId))
			{
				val = constraint.SecondPart;
				if (!((PartId)(ref val)).Equals(partId))
				{
					continue;
				}
			}
			num++;
		}
		return num;
	}

	private static HammerPartCategory ClassifyHammerCategory(PartDefinition part, CoreGameController controller)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		foreach (BehaviorDefinition behavior in part.Behaviors)
		{
			if (behavior is RotatableBehaviorDefinition)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (!HasAvailableContinuousGesture(controller, part.Id))
			{
				return HammerPartCategory.LockedRing;
			}
			return HammerPartCategory.RotatableRing;
		}
		if (BoardRenderUtils.IsSliding(part))
		{
			return HammerPartCategory.SlidingRod;
		}
		return HammerPartCategory.RotatableRing;
	}

	private static bool HasAvailableContinuousGesture(CoreGameController controller, PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<AvailableOperation> operations = controller.GetOperations(partId);
		if (operations.Count == 0)
		{
			return true;
		}
		foreach (AvailableOperation item in operations)
		{
			if ((int)item.Availability == 0)
			{
				AvailableOperationKind kind = item.Kind;
				if ((int)kind == 1 || (int)kind == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Awake()
	{
		if ((Object)(object)Instance != (Object)null && (Object)(object)Instance != (Object)(object)this)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		Instance = this;
		Gameplay.Register(this);
		if ((Object)(object)boardRenderer == (Object)null)
		{
			boardRenderer = ((Component)this).GetComponent<CoreBoardRenderer>();
		}
		if ((Object)(object)gestureInput == (Object)null)
		{
			gestureInput = ((Component)this).GetComponent<SimpleGestureInput>();
		}
		if ((Object)(object)gameplayCamera != (Object)null && (Object)(object)boardRenderer.Cam == (Object)null)
		{
			boardRenderer.Cam = gameplayCamera;
		}
		ringSolveSuccessService = new RingSolveSuccessService(UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)this));
		ringLevelEnterService = new RingLevelEnterService();
		ringHintService = new RingHintService(this);
		propUseService = new RingPropUseService(this);
		dragBlockEffectService = new RingDragBlockEffectService(this);
		EnsureGuideRT();
	}

	private void Start()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		boardRenderer.VictoryChanged += OnVictoryChanged;
		boardRenderer.PlayerPartsRemoved += OnPlayerPartsRemoved;
		if (!GameplayCameraManager.IsPlayFromLoading)
		{
			EventSystem obj = eventSystem;
			if (obj != null)
			{
				((Component)obj).gameObject.SetActive(true);
			}
			testLevel = Math.Max(testLevel, 1);
			UniTaskVoid val = RunLevelEnterAsync(testLevel);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CRunLevelEnterAsync_003Ed__78))]
	public UniTaskVoid RunLevelEnterAsync(int level)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRunLevelEnterAsync_003Ed__78 _003CRunLevelEnterAsync_003Ed__ = default(_003CRunLevelEnterAsync_003Ed__78);
		_003CRunLevelEnterAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunLevelEnterAsync_003Ed__._003C_003E4__this = this;
		_003CRunLevelEnterAsync_003Ed__.level = level;
		_003CRunLevelEnterAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunLevelEnterAsync_003Ed__._003C_003Et__builder)).Start<_003CRunLevelEnterAsync_003Ed__78>(ref _003CRunLevelEnterAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunLevelEnterAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003COnLoadLevel_003Ed__79))]
	private UniTask OnLoadLevel(int levelId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003COnLoadLevel_003Ed__79 _003COnLoadLevel_003Ed__ = default(_003COnLoadLevel_003Ed__79);
		_003COnLoadLevel_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003COnLoadLevel_003Ed__._003C_003E4__this = this;
		_003COnLoadLevel_003Ed__.levelId = levelId;
		_003COnLoadLevel_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003COnLoadLevel_003Ed__._003C_003Et__builder)).Start<_003COnLoadLevel_003Ed__79>(ref _003COnLoadLevel_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003COnLoadLevel_003Ed__._003C_003Et__builder)).Task;
	}

	public void ReplayEnterAnim()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = ReplayEnterAnimAsync();
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CReplayEnterAnimAsync_003Ed__81))]
	private UniTaskVoid ReplayEnterAnimAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CReplayEnterAnimAsync_003Ed__81 _003CReplayEnterAnimAsync_003Ed__ = default(_003CReplayEnterAnimAsync_003Ed__81);
		_003CReplayEnterAnimAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CReplayEnterAnimAsync_003Ed__._003C_003E4__this = this;
		_003CReplayEnterAnimAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CReplayEnterAnimAsync_003Ed__._003C_003Et__builder)).Start<_003CReplayEnterAnimAsync_003Ed__81>(ref _003CReplayEnterAnimAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CReplayEnterAnimAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ClearLevel()
	{
		DisableGameplayInput();
		ringLevelEnterService?.Cancel();
		completeLevelCts?.Cancel();
		completeLevelCts = null;
		isLevelCompleted = false;
		ringHintService?.ClearHintSession();
	}

	private void OnVictoryChanged(bool isVictory)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		if (isVictory && !isLevelCompleted)
		{
			isLevelCompleted = true;
			boardRenderer.ClearOperateHighlights();
			ReportGameEndTrack("成功");
			LevelVictoryService.VictoryProgressSnapshot snapshot = default(LevelVictoryService.VictoryProgressSnapshot);
			if (GameplayCameraManager.IsPlayFromLoading)
			{
				GameApp.Proxy.Get<PlayerProxy>().RecordLevelEnd();
				snapshot = LevelVictoryService.PersistVictoryProgress();
			}
			completeLevelCts?.Cancel();
			completeLevelCts = new CancellationTokenSource();
			UniTaskVoid val = CallDelayedAsync(completeLevelDelay, delegate
			{
				ContinueVictorySettlement(snapshot);
			}, completeLevelCts.Token);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	public bool TryPrepareForGmVictory()
	{
		if (isLevelCompleted || (Object)(object)CurrentLevelObject == (Object)null || isLoadingLevel)
		{
			return false;
		}
		isLevelCompleted = true;
		GameApp.Proxy.Get<RingLevelSaveProxy>()?.ClearSave(writeToDisk: false);
		return true;
	}

	private void ContinueVictorySettlement(LevelVictoryService.VictoryProgressSnapshot snapshot)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!GameplayCameraManager.IsPlayFromLoading)
		{
			testLevel++;
			UniTaskVoid val = RunLevelEnterAsync(testLevel);
			((UniTaskVoid)(ref val)).Forget();
		}
		else
		{
			UniTaskExtensions.Forget(SettlementPresentationGate.WaitThen(delegate
			{
				LevelVictoryService.ContinueToSettlement(snapshot.CompletedLevel, snapshot.CompletedScore, snapshot.IsMainToEndlessTransition, snapshot.CompletedPropUses, snapshot.GameWinIqSnapshot);
				CloseOverlayUIs();
			}, completeLevelCts?.Token ?? default(CancellationToken)));
		}
	}

	private void CloseOverlayUIs()
	{
		if (GameApp.UI.IsOpen("UISettingGame"))
		{
			GameApp.UI.CloseView("UISettingGame", (object)null, false);
		}
		if (GameApp.UI.IsOpen("WeakGuideView"))
		{
			GameApp.UI.CloseView("WeakGuideView", (object)null, false);
		}
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
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		ReportGameEndTrack("重新开始");
		int level = GameApp.Proxy.Get<LevelModelProxy>().Level;
		RRAdContext context = RRAdContext.FromLevelProxy();
		AdService.RequestLevelTransitionBeforeInterstitial(InterstitialTrigger.Restart, context);
		GameApp.Proxy.Get<LevelModelProxy>().TryLevelAgain();
		GameWinIQService.Instance.RecordLevelRestart(level);
		GameApp.Proxy.Get<RankModelProxy>()?.RestartGame();
		UniTaskVoid val = RunLevelEnterAsync(GameApp.Proxy.Get<LevelModelProxy>().Level);
		((UniTaskVoid)(ref val)).Forget();
	}

	public void ExitToHome()
	{
		ReportGameEndTrack("放弃");
		ClearLevel();
		ReleaseGuideRenderTexture();
		boardRenderer?.ReleaseLoadedLevel();
		isLevelCompleted = false;
		AdService.ClearPendingFrontAd("ExitToHome");
	}

	private void OnPlayerPartsRemoved(IReadOnlyList<PartId> partIds)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (partIds == null)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < partIds.Count; i++)
		{
			if (scoredDisappearedParts.Add(partIds[i]))
			{
				num++;
			}
		}
		if (num > 0)
		{
			GameApp.Proxy.Get<LevelModelProxy>().AddScore(100 * num);
		}
	}

	private void EnableGameplayInput(Camera camera)
	{
		InputManager.EnableStartedOverGUI = false;
		Singleton<GameInputManager>.Instance.EnableGameplayInput(camera);
	}

	private void DisableGameplayInput()
	{
		InputManager.EnableStartedOverGUI = true;
		Singleton<GameInputManager>.Instance.DisableGameplayInput();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		UpdateBackgroundState(pauseStatus);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		UpdateBackgroundState(!hasFocus);
	}

	private void UpdateBackgroundState(bool isBackground)
	{
		if (isApplicationBackground != isBackground)
		{
			isApplicationBackground = isBackground;
		}
	}

	private void OnDestroy()
	{
		boardRenderer.VictoryChanged -= OnVictoryChanged;
		boardRenderer.PlayerPartsRemoved -= OnPlayerPartsRemoved;
		ringSolveSuccessService?.Dispose();
		ringSolveSuccessService = null;
		ringLevelEnterService?.Dispose();
		ringLevelEnterService = null;
		dragBlockEffectService?.Dispose();
		dragBlockEffectService = null;
		DisableGameplayInput();
		completeLevelCts?.Cancel();
		completeLevelCts = null;
		if (propUseService != null)
		{
			propUseService.Dispose();
			propUseService.End();
		}
		ringHintService?.ClearHintSession();
		DisposeGuideRT();
		if (levelPackLoadedViaYooAssets)
		{
			GameApp.Res.ReleaseGroup("LevelPackGroup");
		}
		GameApp.Res.ReleaseGroup("UnlockRing_Effect");
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		Gameplay.Unregister(this);
	}

	private void ReportGameStartTrack(LevelDefinition levelDefinition)
	{
		string puzzle = GameApp.Proxy.Get<LevelModelProxy>()?.Data?.GameModeData?.LevelSlotId.ToString() ?? string.Empty;
		GamePartTrackSnapshot partSnapshot = GamePartTrackSnapshot.FromCoreLevel(levelDefinition, boardRenderer?.Controller?.State, puzzle);
		RRDataTrack.Instance.ReportGameStart(partSnapshot);
	}

	public void NotifyLevelFailed()
	{
		ReportGameFailureTrack();
	}

	private void ReportGameEndTrack(string status)
	{
		LevelModelProxy levelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (!RRDataTrack.ShouldSkipDuplicateGameEnd(status, isLevelCompleted, levelProxy))
		{
			RandomLevelFirstClearWindow.OnGameEnd(status);
			if (TryBuildRemainingSnapshot(out var remainingSnapshot))
			{
				RRDataTrack.Instance.ReportGameEnd(status, remainingSnapshot);
			}
			else
			{
				RRDataTrack.Instance.ReportGameEnd(status, BuildPuzzleOnlySnapshot(levelProxy));
			}
		}
	}

	private void ReportGameFailureTrack()
	{
		LevelModelProxy levelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		if (!RRDataTrack.ShouldSkipDuplicateGameEnd("失败", isLevelCompleted, levelProxy))
		{
			if (TryBuildRemainingSnapshot(out var remainingSnapshot))
			{
				RRDataTrack.Instance.ReportGameFailure(remainingSnapshot);
			}
			else
			{
				RRDataTrack.Instance.ReportGameFailure(BuildPuzzleOnlySnapshot(levelProxy));
			}
		}
	}

	private bool TryBuildRemainingSnapshot(out GamePartTrackSnapshot remainingSnapshot)
	{
		remainingSnapshot = default(GamePartTrackSnapshot);
		CoreGameController coreGameController = boardRenderer?.Controller;
		if (coreGameController?.Level == null || coreGameController.State == null)
		{
			return false;
		}
		GamePartTrackSnapshot.CountRemainingCoreParts(coreGameController.Level, coreGameController.State, out var ringNum, out var stickNum, out var plugNum);
		remainingSnapshot = new GamePartTrackSnapshot
		{
			AssemblyNumEnd = ringNum + stickNum + plugNum,
			RingNum = ringNum,
			StickNum = stickNum,
			BombNum = plugNum
		};
		return true;
	}

	private static GamePartTrackSnapshot BuildPuzzleOnlySnapshot(LevelModelProxy levelProxy)
	{
		return default(GamePartTrackSnapshot);
	}

	[AsyncStateMachine(typeof(_003CCallDelayedAsync_003Ed__103))]
	private static UniTaskVoid CallDelayedAsync(float delay, Action action, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CCallDelayedAsync_003Ed__103 _003CCallDelayedAsync_003Ed__ = default(_003CCallDelayedAsync_003Ed__103);
		_003CCallDelayedAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CCallDelayedAsync_003Ed__.delay = delay;
		_003CCallDelayedAsync_003Ed__.action = action;
		_003CCallDelayedAsync_003Ed__.ct = ct;
		_003CCallDelayedAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CCallDelayedAsync_003Ed__._003C_003Et__builder)).Start<_003CCallDelayedAsync_003Ed__103>(ref _003CCallDelayedAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CCallDelayedAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
