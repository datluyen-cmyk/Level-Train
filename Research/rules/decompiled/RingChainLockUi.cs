using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using RotateRings.UI.Game;
using UnityEngine;
using XingYun.UI.Controller;

internal class RingChainLockUi
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDetachChainLockDelayedAsync_003Ed__15 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public RingChainLockUi _003C_003E4__this;

		public float openDuration;

		public WidgetHandle handle;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			RingChainLockUi ringChainLockUi = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					CancellationToken cancelToken = ringChainLockUi.owner.CancelToken;
					UniTask val = UniTask.Delay(TimeSpan.FromSeconds(openDuration), false, (PlayerLoopTiming)8, cancelToken);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CDetachChainLockDelayedAsync_003Ed__15>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult())
				{
					ringChainLockUi.DetachChainLock(handle);
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

	private const string TAG = "[GamePlay][RingLockChain] ";

	private const string ChainLockLocation = "ChainLock";

	private readonly List<WidgetHandle> chainLockHandles = new List<WidgetHandle>();

	private readonly Dictionary<RingObject, WidgetHandle> ringToChainLock = new Dictionary<RingObject, WidgetHandle>();

	private UIGameView chainLockHostView;

	private readonly HashSet<RingObject> pendingLockOpenRings = new HashSet<RingObject>();

	private readonly RingLockChainService owner;

	internal RingChainLockUi(RingLockChainService owner)
	{
		this.owner = owner ?? throw new ArgumentNullException("owner");
		GameApp.Event.Listen<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
	}

	internal void OnRingScaleFactor(RingObject ring, float factor)
	{
		if ((Object)(object)ring == (Object)null || ring.LockCount <= 0)
		{
			return;
		}
		if (!ringToChainLock.TryGetValue(ring, out var value))
		{
			CreateChainLockForRing(ring);
			if (!ringToChainLock.TryGetValue(ring, out value))
			{
				return;
			}
		}
		value.GetView<UIChainLockView>()?.SetEnterScaleFactor(factor);
	}

	private void CreateChainLockForRing(RingObject ring)
	{
		IController val = default(IController);
		if ((Object)(object)ring == (Object)null || ring.LockCount <= 0 || ringToChainLock.ContainsKey(ring) || !GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController { View: var view }))
		{
			return;
		}
		RectTransform val2 = view?.ChainLockContent;
		if (!((Object)(object)val2 == (Object)null))
		{
			chainLockHostView = view;
			WidgetHandle widgetHandle = view.AttachWidget<UIChainLockView>("ChainLock", (Transform)(object)val2);
			if (widgetHandle != null)
			{
				chainLockHandles.Add(widgetHandle);
				ringToChainLock[ring] = widgetHandle;
				widgetHandle.GetView<UIChainLockView>()?.Bind(ring, val2);
			}
		}
	}

	internal void SetupChainLockUiOnLevelLoaded()
	{
		IReadOnlyList<RingObject> readOnlyList = owner.HostInstance.ObjectsService?.Rings;
		if (readOnlyList != null)
		{
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				CreateChainLockForRing(readOnlyList[i]);
			}
		}
	}

	internal void RefreshChainLockUi()
	{
		if (ringToChainLock.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<RingObject, WidgetHandle> item in ringToChainLock)
		{
			RingObject key = item.Key;
			if (!((Object)(object)key == (Object)null))
			{
				item.Value?.GetView<UIChainLockView>()?.UpdateLockCount(key.LockCount);
				if (key.LockCount <= 0)
				{
					pendingLockOpenRings.Add(key);
				}
			}
		}
	}

	internal void PlayHuangdongAnim(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null) && ringToChainLock.TryGetValue(ring, out var value))
		{
			value.GetView<UIChainLockView>()?.PlayHuangdongAnim();
		}
	}

	internal void PlayChainLockOpen(RingObject ring)
	{
		pendingLockOpenRings.Remove(ring);
		if (!((Object)(object)ring == (Object)null) && ringToChainLock.TryGetValue(ring, out var value))
		{
			float openDuration = value.GetView<UIChainLockView>()?.PlayOpenAnim() ?? 0f;
			chainLockHandles.Remove(value);
			ringToChainLock.Remove(ring);
			DetachChainLockDelayed(value, openDuration);
		}
	}

	private void DetachChainLockDelayed(WidgetHandle handle, float openDuration)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (openDuration <= 0f || handle == null)
		{
			DetachChainLock(handle);
			return;
		}
		UniTaskVoid val = DetachChainLockDelayedAsync(handle, openDuration);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CDetachChainLockDelayedAsync_003Ed__15))]
	private UniTaskVoid DetachChainLockDelayedAsync(WidgetHandle handle, float openDuration)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CDetachChainLockDelayedAsync_003Ed__15 _003CDetachChainLockDelayedAsync_003Ed__ = default(_003CDetachChainLockDelayedAsync_003Ed__15);
		_003CDetachChainLockDelayedAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDetachChainLockDelayedAsync_003Ed__._003C_003E4__this = this;
		_003CDetachChainLockDelayedAsync_003Ed__.handle = handle;
		_003CDetachChainLockDelayedAsync_003Ed__.openDuration = openDuration;
		_003CDetachChainLockDelayedAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDetachChainLockDelayedAsync_003Ed__._003C_003Et__builder)).Start<_003CDetachChainLockDelayedAsync_003Ed__15>(ref _003CDetachChainLockDelayedAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDetachChainLockDelayedAsync_003Ed__._003C_003Et__builder)).Task;
	}

	internal void ClearChainLockUi()
	{
		pendingLockOpenRings.Clear();
		owner.ResetBreakSlot();
		for (int num = chainLockHandles.Count - 1; num >= 0; num--)
		{
			DetachChainLock(chainLockHandles[num]);
		}
		chainLockHandles.Clear();
		ringToChainLock.Clear();
	}

	internal void Dispose()
	{
		ClearChainLockUi();
		chainLockHostView = null;
		GameApp.Event.Unlisten<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
	}

	private void OnCameraAdaptationEnd(GameCameraAdaptationEndEvent evt)
	{
		RefreshAllChainLocks();
	}

	internal void RefreshAllChainLocks()
	{
		if (chainLockHostView == null || ringToChainLock.Count == 0)
		{
			return;
		}
		RectTransform chainLockContent = chainLockHostView.ChainLockContent;
		if ((Object)(object)chainLockContent == (Object)null)
		{
			return;
		}
		float cameraScaleFactor = ComputeCameraScaleFactor();
		foreach (KeyValuePair<RingObject, WidgetHandle> item in ringToChainLock)
		{
			RingObject key = item.Key;
			if (!((Object)(object)key == (Object)null))
			{
				item.Value?.GetView<UIChainLockView>()?.RefreshPosition(key, chainLockContent, cameraScaleFactor);
			}
		}
	}

	private float ComputeCameraScaleFactor()
	{
		Camera gameplayCamera = GameplayCameraManager.GameplayCamera;
		if ((Object)(object)gameplayCamera == (Object)null)
		{
			return 1f;
		}
		float num = owner.HostInstance.GetGameService<RotateCameraAdaptation>()?.CachedOrthographicSize ?? 0f;
		if (num <= 0f)
		{
			return 1f;
		}
		return num / gameplayCamera.orthographicSize;
	}

	private void DetachChainLock(WidgetHandle handle)
	{
		if (handle != null && !handle.IsDisposed)
		{
			chainLockHostView?.DetachWidget(handle);
		}
	}
}
