using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.Common.Services;
using Rotaterings.Rank;
using Rotaterings.UI.RankList;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.Services;

public static class RankPreloadService
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass8_0
	{
		public UniTaskCompletionSource<bool> ready;

		internal void _003CWarmupInstanceAsync_003Eb__0(bool success)
		{
			ready.TrySetResult(success);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPrewarmRankListAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter _003C_003Eu__1;

		private Awaiter<GameObject> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
					goto IL_0070;
				}
				if (num == 1)
				{
					goto IL_0078;
				}
				if (!RankClaimHomePopupStep.IsRankClaimPopupNeeded())
				{
					YieldAwaitable val2 = UniTask.Yield();
					val = ((YieldAwaitable)(ref val2)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPrewarmRankListAsync_003Ed__7>(ref val, ref this);
						return;
					}
					goto IL_0070;
				}
				goto end_IL_0007;
				IL_0078:
				try
				{
					Awaiter<GameObject> val3;
					if (num != 1)
					{
						string rankListUIPath = RankActivityEntry.GetRankListUIPath();
						val3 = GameApp.Res.LoadAssetAsync<GameObject>(rankListUIPath, "RankListPreloadGroup").GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val3;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CPrewarmRankListAsync_003Ed__7>(ref val3, ref this);
							return;
						}
					}
					else
					{
						val3 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					if (!((Object)(object)val3.GetResult() == (Object)null))
					{
					}
				}
				catch (Exception)
				{
				}
				goto end_IL_0007;
				IL_0070:
				((Awaiter)(ref val)).GetResult();
				goto IL_0078;
				end_IL_0007:;
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
	private struct _003CWarmupInstanceAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public string location;

		private bool _003Copened_003E5__2;

		private IController _003CwarmupController_003E5__3;

		private GameObject _003CwarmupGameObject_003E5__4;

		private bool _003CisBroken_003E5__5;

		private RectTransform _003CinstanceRect_003E5__6;

		private Stopwatch _003Cstopwatch_003E5__7;

		private Awaiter<(bool IsTimeout, bool Result)> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_029a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Unknown result type (might be due to invalid IL or missing references)
			//IL_0222: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_0080;
				}
				_003C_003Ec__DisplayClass8_0 _003C_003Ec__DisplayClass8_ = new _003C_003Ec__DisplayClass8_0();
				if (!string.IsNullOrEmpty(location) && !RankClaimHomePopupStep.IsRankClaimPopupNeeded() && !GameApp.UI.IsOpen(location))
				{
					_003Copened_003E5__2 = false;
					_003CwarmupController_003E5__3 = null;
					_003CwarmupGameObject_003E5__4 = null;
					_003CisBroken_003E5__5 = false;
					_003CinstanceRect_003E5__6 = null;
					_003Cstopwatch_003E5__7 = Stopwatch.StartNew();
					_003C_003Ec__DisplayClass8_.ready = new UniTaskCompletionSource<bool>();
					goto IL_0080;
				}
				goto end_IL_0007;
				IL_0080:
				try
				{
					Awaiter<(bool, bool)> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<(bool, bool)>);
						num = (_003C_003E1__state = -1);
						goto IL_01d3;
					}
					Awaiter val2;
					if (num == 1)
					{
						val2 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_0257;
					}
					GameApp.UI.Open(location, (UILayer)0, (object)new UIRankListParam
					{
						Content = ERankContent.Main,
						Prewarm = true,
						OnPrewarmReady = _003C_003Ec__DisplayClass8_._003CWarmupInstanceAsync_003Eb__0
					}, (Action)null);
					IController val3 = default(IController);
					_003Copened_003E5__2 = GameApp.UI.IsOpen(location, ref val3);
					_003CwarmupController_003E5__3 = val3;
					if (_003Copened_003E5__2 && val3 is UIRankListController { View: not null } uIRankListController)
					{
						_003CwarmupGameObject_003E5__4 = (((Object)(object)((UIBaseView)uIRankListController.View).Behaviour != (Object)null) ? ((Component)((UIBaseView)uIRankListController.View).Behaviour).gameObject : null);
						_003CinstanceRect_003E5__6 = ((UIBaseView)uIRankListController.View).NodeRect;
						if ((Object)(object)_003CinstanceRect_003E5__6 != (Object)null)
						{
							_003CinstanceRect_003E5__6.anchoredPosition = OffscreenPosition;
						}
						val = UniTaskExtensions.TimeoutWithoutException<bool>(_003C_003Ec__DisplayClass8_.ready.Task, TimeSpan.FromSeconds(3.0), (DelayType)0, (PlayerLoopTiming)8, (CancellationTokenSource)null).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<(bool, bool)>, _003CWarmupInstanceAsync_003Ed__8>(ref val, ref this);
							return;
						}
						goto IL_01d3;
					}
					goto end_IL_0080;
					IL_0257:
					((Awaiter)(ref val2)).GetResult();
					goto end_IL_0080;
					IL_01d3:
					var (flag, flag2) = val.GetResult();
					if (flag || !flag2)
					{
						_003CisBroken_003E5__5 = true;
					}
					UniTask val4 = UniTask.Yield(Application.exitCancellationToken);
					val2 = ((UniTask)(ref val4)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__2 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CWarmupInstanceAsync_003Ed__8>(ref val2, ref this);
						return;
					}
					goto IL_0257;
					end_IL_0080:;
				}
				catch (Exception)
				{
					_003CisBroken_003E5__5 = true;
				}
				finally
				{
					if (num < 0)
					{
						if ((Object)(object)_003CinstanceRect_003E5__6 != (Object)null)
						{
							_003CinstanceRect_003E5__6.anchorMin = Vector2.zero;
							_003CinstanceRect_003E5__6.anchorMax = Vector2.one;
							_003CinstanceRect_003E5__6.offsetMin = Vector2.zero;
							_003CinstanceRect_003E5__6.offsetMax = Vector2.zero;
						}
						IController val5 = default(IController);
						if (_003Copened_003E5__2 && GameApp.UI.IsOpen(location, ref val5) && val5 == _003CwarmupController_003E5__3)
						{
							GameApp.UI.CloseView(location, (object)null, !_003CisBroken_003E5__5);
						}
						else
						{
							DestroyOrphanWarmupInstance(_003CwarmupController_003E5__3 as UIRankListController, _003CwarmupGameObject_003E5__4);
						}
						_003Cstopwatch_003E5__7.Stop();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CwarmupController_003E5__3 = null;
				_003CwarmupGameObject_003E5__4 = null;
				_003CinstanceRect_003E5__6 = null;
				_003Cstopwatch_003E5__7 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CwarmupController_003E5__3 = null;
			_003CwarmupGameObject_003E5__4 = null;
			_003CinstanceRect_003E5__6 = null;
			_003Cstopwatch_003E5__7 = null;
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

	private const string Tag = "[RankPreloadService]";

	private const string PreloadResGroup = "RankListPreloadGroup";

	private static readonly Vector2 OffscreenPosition = new Vector2(10000f, 10000f);

	private const float WarmupTimeoutSeconds = 3f;

	private const int MinDeviceLevelForInstanceCache = 3;

	private static bool isPrewarmStarted;

	public static void PrewarmRankList()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!isPrewarmStarted)
		{
			isPrewarmStarted = true;
			UniTaskVoid val = PrewarmRankListAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CPrewarmRankListAsync_003Ed__7))]
	private static UniTaskVoid PrewarmRankListAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPrewarmRankListAsync_003Ed__7 _003CPrewarmRankListAsync_003Ed__ = default(_003CPrewarmRankListAsync_003Ed__7);
		_003CPrewarmRankListAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPrewarmRankListAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPrewarmRankListAsync_003Ed__._003C_003Et__builder)).Start<_003CPrewarmRankListAsync_003Ed__7>(ref _003CPrewarmRankListAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPrewarmRankListAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CWarmupInstanceAsync_003Ed__8))]
	private static UniTask WarmupInstanceAsync(string location)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CWarmupInstanceAsync_003Ed__8 _003CWarmupInstanceAsync_003Ed__ = default(_003CWarmupInstanceAsync_003Ed__8);
		_003CWarmupInstanceAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CWarmupInstanceAsync_003Ed__.location = location;
		_003CWarmupInstanceAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CWarmupInstanceAsync_003Ed__._003C_003Et__builder)).Start<_003CWarmupInstanceAsync_003Ed__8>(ref _003CWarmupInstanceAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CWarmupInstanceAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void DestroyOrphanWarmupInstance(UIRankListController controller, GameObject instance)
	{
		if ((Object)(object)instance == (Object)null || (controller != null && controller.IsRemoved))
		{
			return;
		}
		instance.SetActive(false);
		((BaseController<UIRankListView>)controller)?.OnActive(false);
		((BaseController<UIRankListView>)controller)?.OnRemove();
		((BaseController<UIRankListView>)controller)?.OnUnloadAsset();
		if (controller != null)
		{
			UIRankListView view = controller.View;
			if (view != null)
			{
				((UIBaseView)view).Dispose();
			}
		}
		Object.Destroy((Object)(object)instance);
	}
}
