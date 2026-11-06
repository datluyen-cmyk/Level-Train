using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Services;
using RotateRings.UI.Shop;
using UniLog;

namespace Game.Common.Services;

public class CloudSaveLoginApplyHomePopupStep : IHomePopupStep, IActivityStep, ICancellableStep
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunSyncAsync_003Ed__8 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public CloudSaveLoginApplyHomePopupStep _003C_003E4__this;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveLoginApplyHomePopupStep cloudSaveLoginApplyHomePopupStep = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					PurchaseService.Show("CloudSaveLoginApply");
				}
				try
				{
					Awaiter<CloudSaveOperationResult> val;
					if (num != 0)
					{
						val = CloudSaveFlowService.PullAndDecideAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CRunSyncAsync_003Ed__8>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
					}
					val.GetResult();
					CloudSaveFlowService.ReportLastSyncTrackData("home_entry");
				}
				catch (Exception ex)
				{
					Log.LogError("[CloudSaveLoginApplyHomePopupStep] ", "主界面云存档同步异常: {0}", new object[1] { ex });
				}
				finally
				{
					if (num < 0)
					{
						PurchaseService.Hide("CloudSaveLoginApply");
					}
				}
				if (!cloudSaveLoginApplyHomePopupStep.isCancelled)
				{
					HomePopupPipeline.ContinueNext();
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

	private const string Tag = "[CloudSaveLoginApplyHomePopupStep] ";

	private const string LoadingSource = "CloudSaveLoginApply";

	private bool isCancelled;

	public int Priority => 10;

	public bool ShouldRun()
	{
		if (CloudSaveFlowService.IsLocalDataBehindCloud)
		{
			return !CloudSaveFlowService.IsOperationInProgress;
		}
		return false;
	}

	public void Execute()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		isCancelled = false;
		UniTaskVoid val = RunSyncAsync();
		((UniTaskVoid)(ref val)).Forget();
	}

	public void Cancel()
	{
		isCancelled = true;
	}

	[AsyncStateMachine(typeof(_003CRunSyncAsync_003Ed__8))]
	private UniTaskVoid RunSyncAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CRunSyncAsync_003Ed__8 _003CRunSyncAsync_003Ed__ = default(_003CRunSyncAsync_003Ed__8);
		_003CRunSyncAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunSyncAsync_003Ed__._003C_003E4__this = this;
		_003CRunSyncAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunSyncAsync_003Ed__._003C_003Et__builder)).Start<_003CRunSyncAsync_003Ed__8>(ref _003CRunSyncAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunSyncAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
