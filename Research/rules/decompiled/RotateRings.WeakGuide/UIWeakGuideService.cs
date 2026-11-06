using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Hot.Model;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using RotateRings.Model;
using RotateRings.WeakGuide.Data;
using RotateRings.WeakGuide.EventHandlers;
using RotateRings.WeakGuide.Setup;
using UnityEngine;

namespace RotateRings.WeakGuide;

public sealed class UIWeakGuideService : GameplayServiceBase<GameplayManager>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CInstallSetupAsync_003Ed__33 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIWeakGuideService _003C_003E4__this;

		public GameObject levelRoot;

		public int level;

		public CancellationToken ct;

		private Awaiter<WeakGuideManager> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIWeakGuideService uIWeakGuideService = _003C_003E4__this;
			try
			{
				if (num == 0 || (!((Object)(object)uIWeakGuideService.Host == (Object)null) && !((Object)(object)levelRoot == (Object)null)))
				{
					try
					{
						Awaiter<WeakGuideManager> val;
						if (num == 0)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<WeakGuideManager>);
							num = (_003C_003E1__state = -1);
							goto IL_00e4;
						}
						if (IsWeakGuideAvailable(level))
						{
							GuideStepsAsset.Entry entry = WeakGuideStepsLocator.Default.FindEntry(level);
							WeakGuideGlobalConfig weakGuideGlobalConfig = WeakGuideGlobalConfig.Default;
							if (!((Object)(object)weakGuideGlobalConfig == (Object)null))
							{
								val = WeakGuideManager.InstallForLevelAsync(uIWeakGuideService.Host, level, ct, new GenericWeakGuideSetup(entry, weakGuideGlobalConfig)).GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<WeakGuideManager>, _003CInstallSetupAsync_003Ed__33>(ref val, ref this);
									return;
								}
								goto IL_00e4;
							}
						}
						goto end_IL_0033;
						IL_00e4:
						WeakGuideManager result = val.GetResult();
						if (!ct.IsCancellationRequested && uIWeakGuideService.weakGuideSetupCts != null && !(uIWeakGuideService.weakGuideSetupCts.Token != ct) && uIWeakGuideService.currentLevel == level && !((Object)(object)uIWeakGuideService.currentLevelRoot != (Object)(object)levelRoot) && !((Object)(object)uIWeakGuideService.Host.CurrentLevelObject != (Object)(object)levelRoot) && !((Object)(object)result == (Object)null))
						{
							uIWeakGuideService.isInstallCompleted = true;
							uIWeakGuideService.TryStartWeakGuide();
						}
						end_IL_0033:;
					}
					catch (OperationCanceledException)
					{
					}
					catch (Exception)
					{
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

	private const string TAG = "[WeakGuide][Service] ";

	private int currentLevel = -1;

	private GameObject currentLevelRoot;

	private bool isInstallCompleted;

	private bool isLevelEnterCompleted;

	private bool isWeakGuideStarted;

	private readonly List<IWeakGuideHandler> activeHandlers = new List<IWeakGuideHandler>(1);

	private CancellationTokenSource weakGuideSetupCts;

	private static bool isGlobalConfigPrewarmed;

	public static bool IsWeakGuideModeSupported()
	{
		return GameInfo.CurrentGameMode == GameMode.Main;
	}

	public static bool HasWeakGuideConfig(int level)
	{
		_ = WeakGuideGlobalConfig.Default;
		if (level > 0)
		{
			return WeakGuideStepsLocator.Default.FindEntry(level) != null;
		}
		return false;
	}

	public static bool IsWeakGuideAvailable(int level)
	{
		if (IsWeakGuideModeSupported())
		{
			return HasWeakGuideConfig(level);
		}
		return false;
	}

	public static bool IsWeakGuideAvailable()
	{
		return IsWeakGuideAvailable(GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 0);
	}

	public static bool TryGetCurrentLevelWeakGuide(out WeakGuideManager weakGuide)
	{
		weakGuide = null;
		GameObject val = (((Object)(object)SingletonComponent<GameplayManager>.Instance != (Object)null) ? SingletonComponent<GameplayManager>.Instance.CurrentLevelObject : null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		GameLevelInfo component = val.GetComponent<GameLevelInfo>();
		weakGuide = (((Object)(object)component != (Object)null) ? component.weakGuideManager : null);
		return (Object)(object)weakGuide != (Object)null;
	}

	public UIWeakGuideService(GameplayManager gameplayManager)
		: base(gameplayManager)
	{
		GameApp.Event.Listen<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
	}

	public void OnLevelInit(int level)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		StopWeakGuide();
		CancelWeakGuideSetup();
		ResetStartGate();
		currentLevel = level;
		currentLevelRoot = (((Object)(object)base.Host != (Object)null) ? base.Host.CurrentLevelObject : null);
		if (!((Object)(object)base.Host == (Object)null) && !((Object)(object)currentLevelRoot == (Object)null) && base.Host.HasUnsolvedGameplayObjects)
		{
			if (!isGlobalConfigPrewarmed)
			{
				isGlobalConfigPrewarmed = true;
				_ = WeakGuideGlobalConfig.Default;
			}
			weakGuideSetupCts = new CancellationTokenSource();
			UniTaskVoid val = InstallSetupAsync(currentLevel, currentLevelRoot, weakGuideSetupCts.Token);
			((UniTaskVoid)(ref val)).Forget();
			Broadcast(delegate(IWeakGuideHandler handler)
			{
				handler.OnSelected();
			});
		}
	}

	public void OnPlayerAction()
	{
		Broadcast(delegate(IWeakGuideHandler handler)
		{
			handler.OnPlayerAction();
		});
	}

	public void OnInteractionStart()
	{
		Broadcast(delegate(IWeakGuideHandler handler)
		{
			handler.OnInteractionStart();
		});
	}

	public void OnInteractionEnd()
	{
		Broadcast(delegate(IWeakGuideHandler handler)
		{
			handler.OnInteractionEnd();
		});
	}

	public void OnEnterForeground()
	{
		Broadcast(delegate(IWeakGuideHandler handler)
		{
			handler.OnEnterForeground();
		});
	}

	public void OnEnterBackground()
	{
		Broadcast(delegate(IWeakGuideHandler handler)
		{
			handler.OnEnterBackground();
		});
	}

	public void OnLevelFinished()
	{
		CancelWeakGuideSetup();
		Broadcast(delegate(IWeakGuideHandler handler)
		{
			handler.OnLevelFinished();
		});
		StopWeakGuide();
		FinishGuidePresentation();
		ResetStartGate();
	}

	public void FinishGuidePresentation()
	{
		if (!((Object)(object)base.Host == (Object)null) && !((Object)(object)base.Host.CurrentLevelObject == (Object)null))
		{
			GameLevelInfo component = base.Host.CurrentLevelObject.GetComponent<GameLevelInfo>();
			if (!((Object)(object)component == (Object)null) && !((Object)(object)component.weakGuideManager == (Object)null))
			{
				component.weakGuideManager.FinishGuideOnLevelEnd();
			}
		}
	}

	public void OnLevelExit()
	{
		CancelWeakGuideSetup();
		ResetStartGate();
		StopWeakGuide();
		currentLevel = -1;
	}

	public void StopWeakGuide()
	{
		for (int i = 0; i < activeHandlers.Count; i++)
		{
			activeHandlers[i].OnDeselected();
			activeHandlers[i].Dispose();
		}
		activeHandlers.Clear();
	}

	public bool IsWeakGuideActive()
	{
		for (int i = 0; i < activeHandlers.Count; i++)
		{
			if (activeHandlers[i].IsActive)
			{
				return true;
			}
		}
		return false;
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			CancelWeakGuideSetup();
			ResetStartGate();
			StopWeakGuide();
			GameApp.Event.Unlisten<LevelEnterCompletedEvent>((Action<LevelEnterCompletedEvent>)OnLevelEnterCompleted);
			base.Dispose();
		}
	}

	private void Broadcast(Action<IWeakGuideHandler> action)
	{
		if (activeHandlers.Count != 0)
		{
			IWeakGuideHandler[] array = activeHandlers.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				action(array[i]);
			}
		}
	}

	private void OnLevelEnterCompleted(LevelEnterCompletedEvent eventData)
	{
		if (IsCurrentLevelEvent(eventData?.Level ?? 0))
		{
			isLevelEnterCompleted = true;
			TryStartWeakGuide();
		}
	}

	private void TryStartWeakGuide()
	{
		if (!isWeakGuideStarted && isInstallCompleted && isLevelEnterCompleted && weakGuideSetupCts != null && !weakGuideSetupCts.IsCancellationRequested && !((Object)(object)base.Host == (Object)null) && !((Object)(object)currentLevelRoot == (Object)null) && !((Object)(object)base.Host.CurrentLevelObject != (Object)(object)currentLevelRoot))
		{
			GameLevelInfo component = currentLevelRoot.GetComponent<GameLevelInfo>();
			WeakGuideManager weakGuideManager = (((Object)(object)component != (Object)null) ? component.weakGuideManager : null);
			if (!((Object)(object)weakGuideManager == (Object)null))
			{
				isWeakGuideStarted = true;
				weakGuideManager.StartGuide();
			}
		}
	}

	private bool IsCurrentLevelEvent(int level)
	{
		if (level == currentLevel && (Object)(object)currentLevelRoot != (Object)null && (Object)(object)base.Host != (Object)null)
		{
			return (Object)(object)base.Host.CurrentLevelObject == (Object)(object)currentLevelRoot;
		}
		return false;
	}

	private void ResetStartGate()
	{
		currentLevelRoot = null;
		isInstallCompleted = false;
		isLevelEnterCompleted = false;
		isWeakGuideStarted = false;
	}

	private void CancelWeakGuideSetup()
	{
		if (weakGuideSetupCts != null)
		{
			weakGuideSetupCts.Cancel();
			weakGuideSetupCts.Dispose();
			weakGuideSetupCts = null;
		}
	}

	[AsyncStateMachine(typeof(_003CInstallSetupAsync_003Ed__33))]
	private UniTaskVoid InstallSetupAsync(int level, GameObject levelRoot, CancellationToken ct)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CInstallSetupAsync_003Ed__33 _003CInstallSetupAsync_003Ed__ = default(_003CInstallSetupAsync_003Ed__33);
		_003CInstallSetupAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CInstallSetupAsync_003Ed__._003C_003E4__this = this;
		_003CInstallSetupAsync_003Ed__.level = level;
		_003CInstallSetupAsync_003Ed__.levelRoot = levelRoot;
		_003CInstallSetupAsync_003Ed__.ct = ct;
		_003CInstallSetupAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CInstallSetupAsync_003Ed__._003C_003Et__builder)).Start<_003CInstallSetupAsync_003Ed__33>(ref _003CInstallSetupAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CInstallSetupAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
