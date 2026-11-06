using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.Input;
using Game.Level;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.Model;
using RotateRings.Services;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.UI.Core;

namespace RotateRings.GamePlay.Services.LevelStepLimit;

public sealed class LevelStepLimitService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBroadcastBombStepVisualDelayed_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public LevelStepLimitService _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelStepLimitService levelStepLimitService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(0.25), false, (PlayerLoopTiming)8, levelStepLimitService.DestroyToken);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CBroadcastBombStepVisualDelayed_003Ed__34>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult() && !levelStepLimitService.DestroyToken.IsCancellationRequested && !levelStepLimitService.IsDisposed && levelStepLimitService.IsEnabled)
				{
					levelStepLimitService.SendChanged(isConsumed: true);
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
	private struct _003COnExhaustedDeferredAsync_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public LevelStepLimitService _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LevelStepLimitService levelStepLimitService = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(levelStepLimitService.lastPointerUpSolved ? 1f : 0.5f), false, (PlayerLoopTiming)8, levelStepLimitService.DestroyToken);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003COnExhaustedDeferredAsync_003Ed__41>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult() && !levelStepLimitService.DestroyToken.IsCancellationRequested && !levelStepLimitService.IsDisposed && levelStepLimitService.IsEnabled && !levelStepLimitService.Host.IsLevelCompleted && levelStepLimitService.Host.HasUnsolvedGameplayObjects)
				{
					levelStepLimitService.OnExhausted();
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

	private const string Tag = "[LevelStepLimit] ";

	private const int OutAlertThreshold = 5;

	private const float SolvedEffectWaitSeconds = 1f;

	private const float BombStepVisualDelaySeconds = 0.25f;

	private LevelStepLimitConfigSnapshot config;

	private int remainingSteps = int.MaxValue;

	private bool lastPointerUpSolved;

	private bool bombStepConsumedThisGesture;

	private bool freeGiftClaimed;

	private bool tipsShown;

	public const int FreeGiftSteps = 5;

	public bool IsEnabled
	{
		get
		{
			if (config.Enable)
			{
				return !base.IsDisposed;
			}
			return false;
		}
	}

	public int RemainingSteps => remainingSteps;

	public int MaxSteps => config.MaxSteps;

	public int AddStepsPerAd => config.AddStepsPerAd;

	public bool IsExhausted => RemainingSteps <= 0;

	public bool CanClaimFreeGift
	{
		get
		{
			if (IsEnabled)
			{
				return !freeGiftClaimed;
			}
			return false;
		}
	}

	public bool ShouldShowTips
	{
		get
		{
			if (IsEnabled)
			{
				return !tipsShown;
			}
			return false;
		}
	}

	public string TipsLangKey => config.LevelMode switch
	{
		LevelStepLimitLevelMode.All => "Tutorial_Moves_Tip1", 
		LevelStepLimitLevelMode.Endless => "Tutorial_Moves_Tip1", 
		LevelStepLimitLevelMode.HardLevel => "Tutorial_Moves_Tip2", 
		_ => null, 
	};

	public LevelStepLimitService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnDragSettleReset);
	}

	public void Initialize(LevelStepLimitConfigSnapshot snapshot)
	{
		Initialize(snapshot, -1);
	}

	public void Initialize(LevelStepLimitConfigSnapshot snapshot, int remainingStepsFromSave)
	{
		config = snapshot;
		bool flag = remainingStepsFromSave >= 0;
		remainingSteps = (flag ? Mathf.Max(0, remainingStepsFromSave) : Mathf.Max(0, snapshot.MaxSteps));
		LevelStepLimitRecord orCreate = GameApp.Save.GetOrCreate<LevelStepLimitRecord>();
		freeGiftClaimed = orCreate.FreeGiftClaimed;
		tipsShown = orCreate.TipsShown;
		if (IsEnabled)
		{
			GameModeData modeData = GetModeData();
			if (modeData != null)
			{
				modeData.StepLimitInitialSteps = snapshot.MaxSteps;
			}
			SendChanged();
		}
	}

	public void OnPointerUp(RingObject ring, bool hasSolved)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled && !((Object)(object)ring == (Object)null) && !bombStepConsumedThisGesture)
		{
			float angleDelta = Mathf.Abs(ring.GetRotateAngle());
			float positionDelta = Vector3.Distance(ring.GetDragStartWorldPosition(), ((Component)ring).transform.position);
			OnDragReleased(angleDelta, positionDelta, hasSolved);
		}
	}

	public void OnPointerUp(PullObject pull, bool hasSolved)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled && !((Object)(object)pull == (Object)null) && !bombStepConsumedThisGesture)
		{
			float positionDelta = Vector3.Distance(pull.GetDragStartWorldPosition(), ((Component)pull).transform.position);
			OnDragReleased(0f, positionDelta, hasSolved);
		}
	}

	private void OnDragReleased(float angleDelta, float positionDelta, bool hasSolved)
	{
		lastPointerUpSolved = hasSolved;
		float num = WorldUnitsToCanvasPoints(positionDelta);
		if (Mathf.Abs(angleDelta) > config.AngleThreshold || num > config.DistanceThreshold)
		{
			ConsumeStep();
		}
	}

	public void OnBombExplosionStep()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled && !bombStepConsumedThisGesture)
		{
			bombStepConsumedThisGesture = true;
			ConsumeStep();
			UniTaskVoid val = BroadcastBombStepVisualDelayed();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CBroadcastBombStepVisualDelayed_003Ed__34))]
	private UniTaskVoid BroadcastBombStepVisualDelayed()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CBroadcastBombStepVisualDelayed_003Ed__34 _003CBroadcastBombStepVisualDelayed_003Ed__ = default(_003CBroadcastBombStepVisualDelayed_003Ed__34);
		_003CBroadcastBombStepVisualDelayed_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CBroadcastBombStepVisualDelayed_003Ed__._003C_003E4__this = this;
		_003CBroadcastBombStepVisualDelayed_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CBroadcastBombStepVisualDelayed_003Ed__._003C_003Et__builder)).Start<_003CBroadcastBombStepVisualDelayed_003Ed__34>(ref _003CBroadcastBombStepVisualDelayed_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CBroadcastBombStepVisualDelayed_003Ed__._003C_003Et__builder)).Task;
	}

	private void OnDragSettleReset(RingDragSettleResetEvent evt)
	{
		if (!base.IsDisposed && evt != null && evt.OperationSource == LegacyOperationSource.PointerUp)
		{
			bombStepConsumedThisGesture = false;
		}
	}

	private static float WorldUnitsToCanvasPoints(float worldUnits)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (worldUnits <= 0f)
		{
			return 0f;
		}
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null || !gameplayCamera.orthographic || Screen.height <= 0)
		{
			return 0f;
		}
		UIRoot instance = UIRoot.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			CanvasScaler uICanvasScaler = instance.UICanvasScaler;
			obj = ((uICanvasScaler != null) ? ((Component)uICanvasScaler).GetComponent<Canvas>() : null);
		}
		Canvas val = (Canvas)obj;
		if ((Object)(object)val == (Object)null || Mathf.Approximately(val.scaleFactor, 0f))
		{
			return 0f;
		}
		float num = worldUnits * (float)Screen.height / (2f * gameplayCamera.orthographicSize);
		Rect rect = ((Component)val).GetComponent<RectTransform>().rect;
		return num * ((Rect)(ref rect)).width / (float)Screen.width;
	}

	public void OnPropUsed()
	{
		if (IsEnabled)
		{
			lastPointerUpSolved = true;
		}
	}

	public bool CanUseProp()
	{
		if (IsEnabled)
		{
			return remainingSteps > 0;
		}
		return true;
	}

	private void ConsumeStep()
	{
		if (remainingSteps > 0)
		{
			remainingSteps--;
			GameModeData modeData = GetModeData();
			if (modeData != null)
			{
				modeData.StepLimitUsedSteps++;
			}
			if (remainingSteps == 5)
			{
				AudioService.PlayUiSound(AudioKeys.MovesOutAlert.Value);
			}
			if (!bombStepConsumedThisGesture)
			{
				SendChanged(isConsumed: true);
			}
			if (IsExhausted && !base.Host.IsLevelCompleted && base.Host.HasUnsolvedGameplayObjects)
			{
				OnExhaustedDeferred();
			}
		}
	}

	private void OnExhaustedDeferred()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Singleton<GameInputManager>.Instance.DisableGameplayInput();
		UniTaskVoid val = OnExhaustedDeferredAsync();
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003COnExhaustedDeferredAsync_003Ed__41))]
	private UniTaskVoid OnExhaustedDeferredAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003COnExhaustedDeferredAsync_003Ed__41 _003COnExhaustedDeferredAsync_003Ed__ = default(_003COnExhaustedDeferredAsync_003Ed__41);
		_003COnExhaustedDeferredAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003COnExhaustedDeferredAsync_003Ed__._003C_003E4__this = this;
		_003COnExhaustedDeferredAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003COnExhaustedDeferredAsync_003Ed__._003C_003Et__builder)).Start<_003COnExhaustedDeferredAsync_003Ed__41>(ref _003COnExhaustedDeferredAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003COnExhaustedDeferredAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public void OnExhausted(Action onPopupClosed = null)
	{
		RandomLevelFirstClearWindow.OnStepsExhausted();
		base.Host.ReportGameEndTrack("失败");
		AudioService.PlayUiSound(AudioKeys.MovesOutFail.Value);
		Singleton<GameInputManager>.Instance.DisableGameplayInput();
		Singleton<EventManager>.Instance.Send<LevelStepLimitExhaustedEvent>(new LevelStepLimitExhaustedEvent
		{
			CanClaimFreeGift = CanClaimFreeGift
		});
		GameApp.UI.Open("UILevelStepLimitPop", (UILayer)2, (object)null, onPopupClosed);
		if (!GameApp.UI.IsOpen("UILevelStepLimitPop"))
		{
			Log.LogError("[LevelStepLimit] ", "耗尽弹窗打开失败，回滚耗尽状态以避免软锁");
			Singleton<GameInputManager>.Instance.EnableGameplayInput();
			onPopupClosed?.Invoke();
		}
	}

	public void AddStepsFromAd()
	{
		if (IsEnabled)
		{
			remainingSteps += config.AddStepsPerAd;
			UpdateAdAddStepsRecord(config.AddStepsPerAd, incrementAdCount: true);
			SendChanged(isConsumed: false, config.AddStepsPerAd);
			RandomLevelFirstClearWindow.OnStepsAdded();
			GameApp.Proxy.Get<LevelModelProxy>()?.SetGameBeginState();
			if (!IsExhausted)
			{
				Singleton<GameInputManager>.Instance.EnableGameplayInput();
			}
		}
	}

	public bool ClaimFreeGift()
	{
		if (!CanClaimFreeGift)
		{
			return false;
		}
		remainingSteps += 5;
		freeGiftClaimed = true;
		GameApp.Save.GetOrCreate<LevelStepLimitRecord>().FreeGiftClaimed = true;
		UpdateAdAddStepsRecord(5, incrementAdCount: false);
		SendChanged(isConsumed: false, 5);
		RandomLevelFirstClearWindow.OnStepsAdded();
		GameApp.Proxy.Get<LevelModelProxy>()?.SetGameBeginState();
		if (!IsExhausted)
		{
			Singleton<GameInputManager>.Instance.EnableGameplayInput();
		}
		return true;
	}

	public void MarkTipsShown()
	{
		if (!tipsShown)
		{
			tipsShown = true;
			GameApp.Save.GetOrCreate<LevelStepLimitRecord>().TipsShown = true;
		}
	}

	public bool CanWatchAd()
	{
		return IsEnabled;
	}

	private void SendChanged(bool isConsumed = false, int addedSteps = 0)
	{
		if (addedSteps > 0)
		{
			AudioService.PlayUiSound(AudioKeys.MovesAddUp.Value);
		}
		Singleton<EventManager>.Instance.Send<LevelStepLimitChangedEvent>(new LevelStepLimitChangedEvent
		{
			RemainingSteps = remainingSteps,
			MaxSteps = config.MaxSteps,
			IsConsumed = isConsumed,
			AddedSteps = addedSteps
		});
	}

	public override void ClearLevel()
	{
		config = default(LevelStepLimitConfigSnapshot);
		remainingSteps = int.MaxValue;
		lastPointerUpSolved = false;
		bombStepConsumedThisGesture = false;
		freeGiftClaimed = false;
		tipsShown = false;
	}

	public override void Dispose()
	{
		GameApp.Event.Unlisten<RingDragSettleResetEvent>((Action<RingDragSettleResetEvent>)OnDragSettleReset);
		config = default(LevelStepLimitConfigSnapshot);
		base.Dispose();
	}

	private static GameModeData GetModeData()
	{
		return GameApp.Proxy.Get<LevelModelProxy>()?.Data?.GameModeData;
	}

	private static void UpdateAdAddStepsRecord(int addedSteps, bool incrementAdCount)
	{
		GameModeData modeData = GetModeData();
		if (modeData != null)
		{
			if (modeData.StepLimitAdAddSteps == null)
			{
				modeData.StepLimitAdAddSteps = new int[2];
			}
			if (incrementAdCount)
			{
				modeData.StepLimitAdAddSteps[0]++;
			}
			modeData.StepLimitAdAddSteps[1] += addedSteps;
		}
	}
}
