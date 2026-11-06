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

public class CloudSaveApplySettlementStep : IPostSettlementStep, IActivityStep
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRunAsync_003Ed__6 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0155: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				_ = 2;
				try
				{
					Awaiter<CloudSaveOperationResult> val;
					switch (num)
					{
					default:
						if (CloudSaveFlowService.IsPullAndDecideInProgress)
						{
							PurchaseService.Show("CloudSaveSettlementApply");
							goto case 0;
						}
						goto IL_00b0;
					case 0:
						try
						{
							if (num != 0)
							{
								val = CloudSaveFlowService.PullAndDecideAsync().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CRunAsync_003Ed__6>(ref val, ref this);
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
							CloudSaveFlowService.ReportLastSyncTrackData("level_end");
						}
						finally
						{
							if (num < 0)
							{
								PurchaseService.Hide("CloudSaveSettlementApply");
							}
						}
						goto IL_00b0;
					case 1:
						try
						{
							if (num != 1)
							{
								val = CloudSaveFlowService.PullAndDecideAsync().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 1);
									_003C_003Eu__1 = val;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CRunAsync_003Ed__6>(ref val, ref this);
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
							CloudSaveFlowService.ReportLastSyncTrackData("level_end");
						}
						finally
						{
							if (num < 0)
							{
								PurchaseService.Hide("CloudSaveSettlementApply");
							}
						}
						goto end_IL_000c;
					case 2:
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
							num = (_003C_003E1__state = -1);
							break;
						}
						IL_00b0:
						if (CloudSaveFlowService.IsCloudSaveEnabled())
						{
							if (CloudSaveFlowService.IsLocalDataBehindCloud)
							{
								PurchaseService.Show("CloudSaveSettlementApply");
								goto case 1;
							}
							val = CloudSaveFlowService.PullAndDecideAsync(CloudSaveSyncMode.DeferCloudApply).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 2);
								_003C_003Eu__1 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CRunAsync_003Ed__6>(ref val, ref this);
								return;
							}
							break;
						}
						goto end_IL_000c;
					}
					val.GetResult();
					CloudSaveFlowService.ReportLastSyncTrackData("level_end");
					end_IL_000c:;
				}
				catch (Exception ex)
				{
					Log.LogError("[CloudSaveApplySettlementStep] ", "结算流程云存档同步异常: {0}", new object[1] { ex });
				}
				finally
				{
					if (num < 0)
					{
						PostSettlementPipeline.ContinueNext();
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

	private const string Tag = "[CloudSaveApplySettlementStep] ";

	private const string LoadingSource = "CloudSaveSettlementApply";

	public int Priority => int.MaxValue;

	public bool ShouldRun()
	{
		if (!CloudSaveFlowService.IsCloudSaveEnabled())
		{
			return CloudSaveFlowService.IsPullAndDecideInProgress;
		}
		return true;
	}

	public void Execute()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = RunAsync();
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CRunAsync_003Ed__6))]
	private static UniTaskVoid RunAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CRunAsync_003Ed__6 _003CRunAsync_003Ed__ = default(_003CRunAsync_003Ed__6);
		_003CRunAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRunAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Start<_003CRunAsync_003Ed__6>(ref _003CRunAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRunAsync_003Ed__._003C_003Et__builder)).Task;
	}
}
