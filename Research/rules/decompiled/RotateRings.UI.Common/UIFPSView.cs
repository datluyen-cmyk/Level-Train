using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Common;

public class UIFPSView : GameWidgetView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CUpdateLoopAsync_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIFPSView _003C_003E4__this;

		public CancellationToken cancellationToken;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIFPSView uIFPSView = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					goto IL_0087;
				}
				Awaiter<bool> val = _003C_003Eu__1;
				_003C_003Eu__1 = default(Awaiter<bool>);
				num = (_003C_003E1__state = -1);
				goto IL_007c;
				IL_007c:
				if (!val.GetResult())
				{
					goto IL_0087;
				}
				goto end_IL_000e;
				IL_0087:
				if (!cancellationToken.IsCancellationRequested)
				{
					uIFPSView.UpdateFpsInfo();
					UniTask val2 = UniTask.Yield((PlayerLoopTiming)8, cancellationToken);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CUpdateLoopAsync_003Ed__11>(ref val, ref this);
						return;
					}
					goto IL_007c;
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

	private const float StatisticsIntervalSeconds = 0.5f;

	private const int FpsGreenThreshold = 25;

	private const int FpsYellowThreshold = 18;

	private float statisticsTime;

	private int frameCount;

	private CancellationTokenSource updateCts;

	public TMP_Text TxtM_CountDown;

	protected override void OnWidgetFirstLoaded()
	{
		ResetStatistics();
		if ((Object)(object)TxtM_CountDown != (Object)null)
		{
			TxtM_CountDown.text = string.Empty;
		}
		StartUpdateLoop();
	}

	public override void Dispose()
	{
		StopUpdateLoop();
		base.Dispose();
	}

	private void ResetStatistics()
	{
		statisticsTime = Time.realtimeSinceStartup;
		frameCount = 0;
	}

	private void StartUpdateLoop()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		StopUpdateLoop();
		updateCts = new CancellationTokenSource();
		UniTaskVoid val = UpdateLoopAsync(updateCts.Token);
		((UniTaskVoid)(ref val)).Forget();
	}

	private void StopUpdateLoop()
	{
		if (updateCts != null)
		{
			updateCts.Cancel();
			updateCts.Dispose();
			updateCts = null;
		}
	}

	[AsyncStateMachine(typeof(_003CUpdateLoopAsync_003Ed__11))]
	private UniTaskVoid UpdateLoopAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CUpdateLoopAsync_003Ed__11 _003CUpdateLoopAsync_003Ed__ = default(_003CUpdateLoopAsync_003Ed__11);
		_003CUpdateLoopAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CUpdateLoopAsync_003Ed__._003C_003E4__this = this;
		_003CUpdateLoopAsync_003Ed__.cancellationToken = cancellationToken;
		_003CUpdateLoopAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CUpdateLoopAsync_003Ed__._003C_003Et__builder)).Start<_003CUpdateLoopAsync_003Ed__11>(ref _003CUpdateLoopAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CUpdateLoopAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void UpdateFpsInfo()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TxtM_CountDown == (Object)null))
		{
			frameCount++;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - statisticsTime;
			if (!(num < 0.5f))
			{
				int num2 = (int)((float)frameCount / num);
				TxtM_CountDown.text = $"FPS:{num2}";
				((Graphic)TxtM_CountDown).color = GetFpsColor(num2);
				frameCount = 0;
				statisticsTime = realtimeSinceStartup;
			}
		}
	}

	private static Color GetFpsColor(int fps)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (fps >= 25)
		{
			return Color.green;
		}
		if (fps >= 18)
		{
			return Color.yellow;
		}
		return Color.red;
	}

	public override void Inject(UIBehaviour behaviour)
	{
		base.Inject(behaviour);
		TxtM_CountDown = behaviour.Get<TMP_Text>("TxtM_CountDown");
	}
}
