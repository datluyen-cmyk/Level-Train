using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services.Account;
using RotateRings.UI.Level;
using RotateRings.UI.Shop;
using RotateRings.UI.Toast;
using UniLog;
using UnityEngine;
using XingYun.Account;
using XingYun.Account.CloudStorage;

namespace RotateRings.Services;

public static class CloudSaveFlowService
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__45 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		private AccountComponent _003Caccount_003E5__2;

		private Awaiter<UnregisterResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				if (num == 0)
				{
					goto IL_0049;
				}
				if (!IsCloudSaveEnabled())
				{
					CloudSaveOperationResult cloudSaveOperationResult = CreateDisabledOperationResult("删除第三方云存档账号");
					LogResult(cloudSaveOperationResult);
					result = cloudSaveOperationResult;
				}
				else
				{
					if (TryBeginOperation("删除第三方云存档账号", out var blockedResult))
					{
						goto IL_0049;
					}
					LogResult(blockedResult);
					result = blockedResult;
				}
				goto end_IL_0007;
				IL_0049:
				try
				{
					Awaiter<UnregisterResult> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<UnregisterResult>);
						num = (_003C_003E1__state = -1);
						goto IL_00f6;
					}
					_003Caccount_003E5__2 = GameApp.Account;
					if (!((Object)(object)_003Caccount_003E5__2 == (Object)null))
					{
						val = LoginManager.UnregisterThirdPartyAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<UnregisterResult>, _003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__45>(ref val, ref this);
							return;
						}
						goto IL_00f6;
					}
					CloudSaveOperationResult cloudSaveOperationResult2 = new CloudSaveOperationResult("删除第三方云存档账号", isSuccess: false, isInProgress: false, "AccountComponent 未初始化，请重新启动游戏");
					LogResult(cloudSaveOperationResult2);
					result = cloudSaveOperationResult2;
					goto end_IL_0049;
					IL_00f6:
					UnregisterResult result2 = val.GetResult();
					if (!result2.IsSuccess)
					{
						CloudSaveOperationResult cloudSaveOperationResult3 = new CloudSaveOperationResult("删除第三方云存档账号", isSuccess: false, isInProgress: false, $"注销失败 code={result2.Code} http={result2.HttpStatus} guid={result2.Guid} msg={result2.Msg}");
						LogResult(cloudSaveOperationResult3);
						result = cloudSaveOperationResult3;
					}
					else if (!DeleteCloudStorageLocalFile(result2.Guid))
					{
						CloudSaveOperationResult cloudSaveOperationResult4 = new CloudSaveOperationResult("删除第三方云存档账号", isSuccess: false, isInProgress: false, $"第三方账号已注销 guid={result2.Guid}，但本地云存档文件删除失败；当前游客 guid={_003Caccount_003E5__2.Guid}");
						LogResult(cloudSaveOperationResult4);
						result = cloudSaveOperationResult4;
					}
					else
					{
						CloudSaveOperationResult cloudSaveOperationResult5 = new CloudSaveOperationResult("删除第三方云存档账号", isSuccess: true, isInProgress: false, $"已注销第三方云存档账号 guid={result2.Guid} 并删除本地云存档文件，当前已切换为游客 guid={_003Caccount_003E5__2.Guid}");
						LogResult(cloudSaveOperationResult5);
						result = cloudSaveOperationResult5;
					}
					end_IL_0049:;
				}
				catch (Exception ex)
				{
					CloudSaveOperationResult cloudSaveOperationResult6 = new CloudSaveOperationResult("删除第三方云存档账号", isSuccess: false, isInProgress: false, $"注销第三方云存档账号异常: {ex.Message}");
					Log.LogError("[CloudSaveFlow]", "注销第三方云存档账号异常: {0}", new object[1] { ex });
					LogResult(cloudSaveOperationResult6);
					result = cloudSaveOperationResult6;
				}
				finally
				{
					if (num < 0)
					{
						EndOperation();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CForcePushSnapshotAsync_003Ed__50 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public CloudSaveDataSnapshot snapshot;

		public CloudStorageComponent cloud;

		public string operationName;

		private byte[] _003CrawData_003E5__2;

		private Awaiter<StorageUploadResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Invalid comparison between Unknown and I4
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result2;
			try
			{
				Awaiter<StorageUploadResult> val;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<StorageUploadResult>);
						num = (_003C_003E1__state = -1);
						goto IL_0158;
					}
					_003CrawData_003E5__2 = CloudSaveDataHandler.SerializeSnapshot(snapshot);
					cloud.Set("rotaterings_player_progress_v1", _003CrawData_003E5__2);
					val = PushCloudSaveAsync(cloud).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<StorageUploadResult>, _003CForcePushSnapshotAsync_003Ed__50>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<StorageUploadResult>);
					num = (_003C_003E1__state = -1);
				}
				StorageUploadResult result = val.GetResult();
				if (result != null && (int)result.Code == 4005 && result.Conflict != null)
				{
					long num2 = Math.Max(result.Conflict.ServerVersion, cloud.GetVersion("rotaterings_player_progress_v1")) + 1;
					cloud.SetWithVersion("rotaterings_player_progress_v1", _003CrawData_003E5__2, num2);
					val = PushCloudSaveAsync(cloud).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<StorageUploadResult>, _003CForcePushSnapshotAsync_003Ed__50>(ref val, ref this);
						return;
					}
					goto IL_0158;
				}
				goto IL_0160;
				IL_0160:
				if (result != null)
				{
					LoginManager.NotifyAccountNotFoundIfNeeded(result.Code, "CloudSave:ForcePush");
				}
				CloudSaveOperationResult cloudSaveOperationResult = CreateUploadResult(operationName, result);
				lastUploadResult = cloudSaveOperationResult.Summary;
				result2 = cloudSaveOperationResult;
				goto end_IL_0007;
				IL_0158:
				result = val.GetResult();
				goto IL_0160;
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CrawData_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CrawData_003E5__2 = null;
			_003C_003Et__builder.SetResult(result2);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CForceUploadLocalSnapshotAsync_003Ed__48 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public string operationName;

		private Awaiter<CloudSaveSnapshotPullResult> _003C_003Eu__1;

		private CloudSaveDataSnapshot _003Csnapshot_003E5__2;

		private List<CloudSavePurchaseCount> _003CmergedPendingPurchaseCounts_003E5__3;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result2;
			try
			{
				Awaiter<CloudSaveSnapshotPullResult> val;
				if (num != 0)
				{
					if (num == 1)
					{
						goto IL_00d4;
					}
					SetPendingForceCloudUpload(value: true, "beforeForceUpload");
					val = PullCloudSnapshotInternalAsync().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveSnapshotPullResult>, _003CForceUploadLocalSnapshotAsync_003Ed__48>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveSnapshotPullResult>);
					num = (_003C_003E1__state = -1);
				}
				CloudSaveSnapshotPullResult result = val.GetResult();
				if (result.IsSuccess && !CloudSaveDataHandler.IsSchemaNewerThanClient(result.Snapshot))
				{
					goto IL_00d4;
				}
				CloudSaveOperationResult cloudSaveOperationResult = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, result.IsSuccess ? "云端数据结构版本高于当前客户端，已取消本地覆盖云端" : $"拉取云端数据失败，无法确认数据结构版本，已取消本地覆盖云端: {result.FailureReason}", result.ErrorCode);
				lastUploadResult = cloudSaveOperationResult.Summary;
				result2 = cloudSaveOperationResult;
				goto end_IL_0007;
				IL_00d4:
				try
				{
					Awaiter<CloudSaveOperationResult> val2;
					if (num == 1)
					{
						val2 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
						goto IL_01e8;
					}
					CloudSaveOperationResult unavailableResult;
					CloudStorageComponent cloudStorage = GetCloudStorage(out unavailableResult);
					if (!((Object)(object)cloudStorage == (Object)null))
					{
						_003Csnapshot_003E5__2 = CloudSaveDataHandler.CaptureSnapshot();
						_003Csnapshot_003E5__2.Version = Math.Max(_003Csnapshot_003E5__2.Version, result.Snapshot?.Version ?? 0) + 1;
						_003CmergedPendingPurchaseCounts_003E5__3 = _003Csnapshot_003E5__2.PurchaseCounts;
						_003Csnapshot_003E5__2.SyncedPurchaseCounts = CloudSaveDataHandler.SumPurchaseCounts(_003Csnapshot_003E5__2.SyncedPurchaseCounts, _003CmergedPendingPurchaseCounts_003E5__3);
						_003Csnapshot_003E5__2.PurchaseCounts = new List<CloudSavePurchaseCount>();
						val2 = ForcePushSnapshotAsync(cloudStorage, _003Csnapshot_003E5__2, operationName).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val2;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CForceUploadLocalSnapshotAsync_003Ed__48>(ref val2, ref this);
							return;
						}
						goto IL_01e8;
					}
					lastUploadResult = unavailableResult.Summary;
					result2 = unavailableResult;
					goto end_IL_00d4;
					IL_01e8:
					CloudSaveOperationResult result3 = val2.GetResult();
					string error;
					if (!result3.IsSuccess)
					{
						result2 = result3;
					}
					else if (!TryCommitUploadedLocalData(_003Csnapshot_003E5__2, _003CmergedPendingPurchaseCounts_003E5__3, out error))
					{
						result2 = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, $"上传成功，但本地版本提交失败: {error}");
					}
					else
					{
						SetPendingForceCloudUpload(value: false, "forceUploadOk");
						result2 = new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, "已使用本地数据覆盖云端，并清空待上传购买道具及更新本地版本号");
					}
					end_IL_00d4:;
				}
				catch (Exception ex)
				{
					CloudSaveOperationResult cloudSaveOperationResult2 = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, $"本地覆盖云端异常: {ex.Message}");
					lastUploadResult = cloudSaveOperationResult2.Summary;
					Log.LogError("[CloudSaveFlow]", "本地覆盖云端异常（PendingForceCloudUpload 保持为 true，后续同步将重试上传）: {0}", new object[1] { ex });
					result2 = cloudSaveOperationResult2;
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result2);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CProcessDecisionAsync_003Ed__67 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public CloudSaveDataDecisionResult decision;

		public string operationName;

		public CloudSaveSyncMode mode;

		public CloudStorageComponent cloud;

		private bool _003CshowSyncMask_003E5__2;

		private bool _003CshouldReturnToHomeAfterApply_003E5__3;

		private List<CloudSavePurchaseCount> _003CmergedPendingPurchaseCounts_003E5__4;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_0387: Unknown result type (might be due to invalid IL or missing references)
			//IL_038c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0390: Unknown result type (might be due to invalid IL or missing references)
			//IL_0395: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_01cf;
				}
				if (decision == null)
				{
					result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "未取得可处理的数据决策结果");
				}
				else if (decision.MergeResult == CloudSaveMainLevelMergeResult.SyncFailed)
				{
					result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "云存档决策失败，未处理任何数据");
				}
				else if (decision.Data == null)
				{
					result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "决策未返回最终数据");
				}
				else
				{
					if (mode != CloudSaveSyncMode.DeferCloudApply)
					{
						goto IL_00e2;
					}
					CloudSaveMainLevelMergeResult mergeResult = decision.MergeResult;
					if ((mergeResult != CloudSaveMainLevelMergeResult.UseCloudNoUpload && mergeResult != CloudSaveMainLevelMergeResult.UseCloudNeedUpload) || IsHomeReadyForCloudSaveApply())
					{
						goto IL_00e2;
					}
					isLocalDataBehindCloud = true;
					result = new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, "决策为使用云端，已标记本地数据落后云端");
				}
				goto end_IL_0007;
				IL_01a3:
				int num2;
				_003CshouldReturnToHomeAfterApply_003E5__3 = (byte)num2 != 0;
				_003CmergedPendingPurchaseCounts_003E5__4 = decision.LocalData?.PurchaseCounts ?? new List<CloudSavePurchaseCount>();
				goto IL_01cf;
				IL_00e2:
				bool flag = mode == CloudSaveSyncMode.ShopPurchase;
				int num3;
				if (!flag)
				{
					if (mode == CloudSaveSyncMode.DeferCloudApply)
					{
						CloudSaveMainLevelMergeResult mergeResult = decision.MergeResult;
						if (mergeResult == CloudSaveMainLevelMergeResult.UseCloudNoUpload || mergeResult == CloudSaveMainLevelMergeResult.UseCloudNeedUpload)
						{
							num3 = (IsHomeReadyForCloudSaveApply() ? 1 : 0);
							goto IL_011b;
						}
					}
					num3 = 0;
				}
				else
				{
					num3 = 1;
				}
				goto IL_011b;
				IL_01cf:
				try
				{
					Awaiter<CloudSaveOperationResult> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
						goto IL_02e5;
					}
					if (num == 1)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
						goto IL_03e1;
					}
					switch (decision.MergeResult)
					{
					case CloudSaveMainLevelMergeResult.UseCloudNoUpload:
						break;
					case CloudSaveMainLevelMergeResult.UseCloudNeedUpload:
						goto IL_0269;
					case CloudSaveMainLevelMergeResult.UseLocal:
						goto IL_0365;
					default:
						result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "未知云存档决策结果");
						goto end_IL_01cf;
					}
					ReturnToHomeBeforeApplyingShopPurchaseSyncIfNeeded(_003CshouldReturnToHomeAfterApply_003E5__3);
					result = (CloudSaveDataHandler.TryApplySnapshot(decision.Data, commitCloudSyncState: true, _003CmergedPendingPurchaseCounts_003E5__4) ? new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, "已使用云端数据覆盖本地") : new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "云端数据覆盖本地失败"));
					goto end_IL_01cf;
					IL_0365:
					val = UploadDecisionSnapshotAsync(cloud, decision.Data, decision.CloudStorageVersion, operationName).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CProcessDecisionAsync_003Ed__67>(ref val, ref this);
						return;
					}
					goto IL_03e1;
					IL_02e5:
					CloudSaveOperationResult result2 = val.GetResult();
					if (!result2.IsSuccess)
					{
						result = result2;
					}
					else
					{
						ReturnToHomeBeforeApplyingShopPurchaseSyncIfNeeded(_003CshouldReturnToHomeAfterApply_003E5__3);
						result = (CloudSaveDataHandler.TryApplySnapshot(decision.Data, commitCloudSyncState: true, _003CmergedPendingPurchaseCounts_003E5__4) ? new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, "已上传合并数据，并覆盖本地数据及清空待上传购买道具") : new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "上传成功，但合并后的云端数据覆盖本地失败"));
					}
					goto end_IL_01cf;
					IL_0269:
					val = UploadDecisionSnapshotAsync(cloud, decision.Data, decision.CloudStorageVersion, operationName).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CProcessDecisionAsync_003Ed__67>(ref val, ref this);
						return;
					}
					goto IL_02e5;
					IL_03e1:
					CloudSaveOperationResult result3 = val.GetResult();
					result = ((!result3.IsSuccess) ? result3 : ((!CloudSaveDataHandler.TryApplyUploadedPropIncrease(decision.LocalData, decision.Data)) ? new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "上传成功，但补回的购买道具写入本地失败") : (TryCommitUploadedLocalData(decision.Data, _003CmergedPendingPurchaseCounts_003E5__4, out var error) ? new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, "已上传本地数据，并清空待上传购买道具及更新本地版本号") : new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, $"上传成功，但本地版本提交失败: {error}"))));
					end_IL_01cf:;
				}
				finally
				{
					if (num < 0 && _003CshowSyncMask_003E5__2)
					{
						PurchaseService.Hide("CloudSavePull");
					}
				}
				goto end_IL_0007;
				IL_011b:
				_003CshowSyncMask_003E5__2 = (byte)num3 != 0;
				if (_003CshowSyncMask_003E5__2)
				{
					PurchaseService.Show("CloudSavePull");
				}
				if (flag)
				{
					CloudSaveMainLevelMergeResult mergeResult = decision.MergeResult;
					if ((mergeResult == CloudSaveMainLevelMergeResult.UseCloudNoUpload || mergeResult == CloudSaveMainLevelMergeResult.UseCloudNeedUpload) && decision.LocalData?.MainLevelId != decision.Data.MainLevelId)
					{
						num2 = (IsGameplayInProgress() ? 1 : 0);
						goto IL_01a3;
					}
				}
				num2 = 0;
				goto IL_01a3;
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CmergedPendingPurchaseCounts_003E5__4 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CmergedPendingPurchaseCounts_003E5__4 = null;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPullAndDecideAsync_003Ed__57 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public CloudSaveSyncMode mode;

		private long _003CstartedAt_003E5__2;

		private CloudSaveDataSnapshot _003ClocalSnapshot_003E5__3;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				Awaiter<CloudSaveOperationResult> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
					num = (_003C_003E1__state = -1);
					goto IL_0097;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
					num = (_003C_003E1__state = -1);
					goto IL_012e;
				}
				if (IsCloudSaveEnabled())
				{
					if (CloudSaveFlowService.pullAndDecideCompletionSource != null)
					{
						val = CloudSaveFlowService.pullAndDecideCompletionSource.Task.GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPullAndDecideAsync_003Ed__57>(ref val, ref this);
							return;
						}
						goto IL_0097;
					}
					CloudSaveFlowService.pullAndDecideCompletionSource = new UniTaskCompletionSource<CloudSaveOperationResult>();
					CloudSaveResolver.Reset();
					_003CstartedAt_003E5__2 = Stopwatch.GetTimestamp();
					_003ClocalSnapshot_003E5__3 = CloudSaveDataHandler.CaptureSnapshot();
					val = PullAndDecideInternalAsync(mode).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 1);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPullAndDecideAsync_003Ed__57>(ref val, ref this);
						return;
					}
					goto IL_012e;
				}
				CloudSaveOperationResult cloudSaveOperationResult = CreateDisabledOperationResult("拉取并决策");
				LogResult(cloudSaveOperationResult);
				result = cloudSaveOperationResult;
				goto end_IL_0007;
				IL_0097:
				result = val.GetResult();
				goto end_IL_0007;
				IL_012e:
				CloudSaveOperationResult result2 = val.GetResult();
				CloudSaveDataDecisionResult lastResult = CloudSaveResolver.LastResult;
				lastSyncTrackData = CreateCloudSaveSyncTrackData(result2, _003CstartedAt_003E5__2, lastResult?.LocalData ?? _003ClocalSnapshot_003E5__3, lastResult?.CloudData, lastResult?.Data, GetSelectedSaveSource(lastResult), lastResult != null);
				UniTaskCompletionSource<CloudSaveOperationResult> pullAndDecideCompletionSource = CloudSaveFlowService.pullAndDecideCompletionSource;
				CloudSaveFlowService.pullAndDecideCompletionSource = null;
				pullAndDecideCompletionSource.TrySetResult(result2);
				LogResult(result2, lastResult?.LocalData ?? _003ClocalSnapshot_003E5__3, lastResult?.CloudData, lastResult?.Data);
				result = result2;
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003ClocalSnapshot_003E5__3 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003ClocalSnapshot_003E5__3 = null;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPullAndDecideInternalAsync_003Ed__60 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public CloudSaveSyncMode mode;

		private long _003CstartedAt_003E5__2;

		private CloudSaveDataSnapshot _003ClocalSnapshot_003E5__3;

		private CloudSaveDataSnapshot _003CcloudSnapshot_003E5__4;

		private CloudStorageComponent _003Ccloud_003E5__5;

		private CloudSaveDataDecisionResult _003Cdecision_003E5__6;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private Awaiter<StoragePullResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_0377: Unknown result type (might be due to invalid IL or missing references)
			//IL_037f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0334: Unknown result type (might be due to invalid IL or missing references)
			//IL_0339: Unknown result type (might be due to invalid IL or missing references)
			//IL_033d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0342: Unknown result type (might be due to invalid IL or missing references)
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				if ((uint)num <= 3u)
				{
					goto IL_003e;
				}
				_003CstartedAt_003E5__2 = Stopwatch.GetTimestamp();
				_003ClocalSnapshot_003E5__3 = CloudSaveDataHandler.CaptureSnapshot();
				_003CcloudSnapshot_003E5__4 = null;
				if (TryBeginOperation("拉取并决策", out var blockedResult))
				{
					goto IL_003e;
				}
				result = blockedResult;
				goto end_IL_0007;
				IL_003e:
				try
				{
					Awaiter<StoragePullResult> val2;
					Awaiter<CloudSaveOperationResult> val;
					CloudSaveOperationResult unavailableResult;
					CloudSaveOperationResult result2;
					CloudSaveOperationResult result3;
					StoragePullResult result4;
					CloudSaveOperationResult cloudSaveOperationResult;
					switch (num)
					{
					default:
						if (IsPendingBindSelection)
						{
							val = ResolveInterruptedBindSelectionAsync("拉取并决策").GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPullAndDecideInternalAsync_003Ed__60>(ref val, ref this);
								return;
							}
							goto IL_00bd;
						}
						goto IL_00e3;
					case 0:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
						goto IL_00bd;
					case 1:
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
						goto IL_014c;
					case 2:
						val2 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<StoragePullResult>);
						num = (_003C_003E1__state = -1);
						goto IL_0223;
					case 3:
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
							num = (_003C_003E1__state = -1);
							break;
						}
						IL_00e3:
						if (IsPendingForceCloudUpload)
						{
							val = ForceUploadLocalSnapshotAsync("拉取并决策").GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPullAndDecideInternalAsync_003Ed__60>(ref val, ref this);
								return;
							}
							goto IL_014c;
						}
						_003Ccloud_003E5__5 = GetCloudStorage(out unavailableResult);
						if (!((Object)(object)_003Ccloud_003E5__5 == (Object)null))
						{
							WriteCurrentSnapshotIfChanged(_003Ccloud_003E5__5);
							ConfigurePullRequestPolicy();
							val2 = _003Ccloud_003E5__5.PullAsync((IList<string>)CloudSaveSystemTypes, (ICloudStorageConflictResolver)(object)CloudSaveResolver, default(CancellationToken)).GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 2);
								_003C_003Eu__2 = val2;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<StoragePullResult>, _003CPullAndDecideInternalAsync_003Ed__60>(ref val2, ref this);
								return;
							}
							goto IL_0223;
						}
						lastPullResult = unavailableResult.Summary;
						result = unavailableResult;
						goto end_IL_003e;
						IL_014c:
						result2 = val.GetResult();
						lastPullResult = result2.Summary;
						if (result2.IsSuccess)
						{
							hasPullAndDecideSucceededThisSession = true;
						}
						result = result2;
						goto end_IL_003e;
						IL_00bd:
						result3 = val.GetResult();
						if (result3.IsSuccess)
						{
							goto IL_00e3;
						}
						lastPullResult = result3.Summary;
						result = result3;
						goto end_IL_003e;
						IL_0223:
						result4 = val2.GetResult();
						_003Cdecision_003E5__6 = CloudSaveResolver.LastResult;
						_003CcloudSnapshot_003E5__4 = _003Cdecision_003E5__6?.CloudData;
						cloudSaveOperationResult = CreatePullResult("拉取并决策", result4);
						if (!cloudSaveOperationResult.IsSuccess)
						{
							LoginManager.NotifyAccountNotFoundIfNeeded(result4.Code, "CloudSave:PullAndDecide");
							lastPullResult = cloudSaveOperationResult.Summary;
							result = cloudSaveOperationResult;
						}
						else
						{
							if (_003Cdecision_003E5__6 != null)
							{
								val = ProcessDecisionAsync(_003Ccloud_003E5__5, _003Cdecision_003E5__6, "拉取并决策", mode).GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 3);
									_003C_003Eu__1 = val;
									_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPullAndDecideInternalAsync_003Ed__60>(ref val, ref this);
									return;
								}
								break;
							}
							CloudSaveOperationResult cloudSaveOperationResult2 = new CloudSaveOperationResult("拉取并决策", isSuccess: false, isInProgress: false, "云存档拉取成功，但冲突决策器未返回结果");
							lastPullResult = cloudSaveOperationResult2.Summary;
							Log.LogError("[CloudSaveFlow]", "云存档拉取成功但决策结果为空，requested={0} updated={1} keptLocal={2} skipped={3}", new object[4] { result4.RequestedCount, result4.UpdatedCount, result4.KeptLocalCount, result4.SkippedCount });
							result = cloudSaveOperationResult2;
						}
						goto end_IL_003e;
					}
					CloudSaveOperationResult result5 = val.GetResult();
					ShowPullAndDecideResult(result5, _003Cdecision_003E5__6);
					if (result5.IsSuccess && !IsMarkedLocalDataBehindResult(result5))
					{
						hasPullAndDecideSucceededThisSession = true;
						ClearLocalDataBehindCloudAfterSuccessfulPullAndDecide();
					}
					lastSyncTrackData = CreateCloudSaveSyncTrackData(result5, _003CstartedAt_003E5__2, _003Cdecision_003E5__6?.LocalData ?? _003ClocalSnapshot_003E5__3, _003Cdecision_003E5__6?.CloudData ?? _003CcloudSnapshot_003E5__4, _003Cdecision_003E5__6?.Data, GetSelectedSaveSource(_003Cdecision_003E5__6), isConflict: true);
					result = result5;
					end_IL_003e:;
				}
				catch (Exception ex)
				{
					CloudSaveOperationResult cloudSaveOperationResult3 = new CloudSaveOperationResult("拉取并决策", isSuccess: false, isInProgress: false, $"拉取云存档数据异常: {ex.Message}");
					lastPullResult = cloudSaveOperationResult3.Summary;
					Log.LogError("[CloudSaveFlow]", "拉取云存档数据异常: {0}", new object[1] { ex });
					lastSyncTrackData = CreateCloudSaveSyncTrackData(cloudSaveOperationResult3, _003CstartedAt_003E5__2, _003ClocalSnapshot_003E5__3, _003CcloudSnapshot_003E5__4, null, string.Empty, isConflict: false);
					result = cloudSaveOperationResult3;
				}
				finally
				{
					if (num < 0)
					{
						EndOperation();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003ClocalSnapshot_003E5__3 = null;
				_003CcloudSnapshot_003E5__4 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003ClocalSnapshot_003E5__3 = null;
			_003CcloudSnapshot_003E5__4 = null;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPullCloudSnapshotAsync_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveSnapshotPullResult> _003C_003Et__builder;

		private Awaiter<CloudSaveSnapshotPullResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveSnapshotPullResult result;
			try
			{
				if (num == 0)
				{
					goto IL_0077;
				}
				if (!IsCloudSaveEnabled())
				{
					CloudSaveOperationResult cloudSaveOperationResult = CreateDisabledOperationResult("拉取云端玩家进度");
					LogResult(cloudSaveOperationResult);
					result = new CloudSaveSnapshotPullResult(isSuccess: false, null, cloudSaveOperationResult.Summary);
				}
				else
				{
					if (TryBeginOperation("拉取云端玩家进度", out var blockedResult))
					{
						goto IL_0077;
					}
					LogResult(blockedResult);
					result = new CloudSaveSnapshotPullResult(isSuccess: false, null, blockedResult.Summary);
				}
				goto end_IL_0007;
				IL_0077:
				try
				{
					Awaiter<CloudSaveSnapshotPullResult> val;
					if (num != 0)
					{
						val = PullCloudSnapshotInternalAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveSnapshotPullResult>, _003CPullCloudSnapshotAsync_003Ed__41>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveSnapshotPullResult>);
						num = (_003C_003E1__state = -1);
					}
					CloudSaveSnapshotPullResult result2 = val.GetResult();
					LogSnapshotPullResult("拉取云端玩家进度", result2);
					result = result2;
				}
				finally
				{
					if (num < 0)
					{
						EndOperation();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPullCloudSnapshotInternalAsync_003Ed__43 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveSnapshotPullResult> _003C_003Et__builder;

		private Awaiter<StoragePullResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveSnapshotPullResult result;
			try
			{
				try
				{
					Awaiter<StoragePullResult> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<StoragePullResult>);
						num = (_003C_003E1__state = -1);
						goto IL_00bc;
					}
					CloudSaveOperationResult unavailableResult;
					CloudStorageComponent cloudStorage = GetCloudStorage(out unavailableResult);
					if (!((Object)(object)cloudStorage == (Object)null))
					{
						ConfigurePullRequestPolicy();
						CloudDataPullResolver.Reset();
						val = cloudStorage.PullAsync((IList<string>)CloudSaveSystemTypes, (ICloudStorageConflictResolver)(object)CloudDataPullResolver, default(CancellationToken)).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<StoragePullResult>, _003CPullCloudSnapshotInternalAsync_003Ed__43>(ref val, ref this);
							return;
						}
						goto IL_00bc;
					}
					result = new CloudSaveSnapshotPullResult(isSuccess: false, null, unavailableResult.Summary);
					goto end_IL_000a;
					IL_00bc:
					StoragePullResult result2 = val.GetResult();
					if (result2 == null)
					{
						result = new CloudSaveSnapshotPullResult(isSuccess: false, null, "框架未返回拉取结果", (CloudStorageErrorCode)9999);
					}
					else if (!result2.IsSuccess)
					{
						LoginManager.NotifyAccountNotFoundIfNeeded(result2.Code, "CloudSave:PullSnapshot");
						string failureReason = $"拉取失败 code={result2.Code} http={result2.HttpStatus} msg={result2.Msg}";
						result = new CloudSaveSnapshotPullResult(isSuccess: false, null, failureReason, result2.Code);
					}
					else
					{
						CloudSaveDataSnapshot snapshot = CloudDataPullResolver.Snapshot;
						result = new CloudSaveSnapshotPullResult(isSuccess: true, snapshot);
					}
					end_IL_000a:;
				}
				catch (Exception ex)
				{
					string failureReason2 = $"拉取云端玩家进度异常: {ex.Message}";
					Log.LogError("[CloudSaveFlow]", "拉取云端玩家进度异常: {0}", new object[1] { ex });
					result = new CloudSaveSnapshotPullResult(isSuccess: false, null, failureReason2, (CloudStorageErrorCode)9999);
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPullCloudSnapshotToLocalAsync_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveDataSnapshot> _003C_003Et__builder;

		private Awaiter<CloudSaveSnapshotPullResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveDataSnapshot result;
			try
			{
				if (num == 0)
				{
					goto IL_0049;
				}
				if (!IsCloudSaveEnabled())
				{
					LogResult(CreateDisabledOperationResult("拉取云端玩家进度并写入本地"));
					result = null;
				}
				else
				{
					if (TryBeginOperation("拉取云端玩家进度并写入本地", out var blockedResult))
					{
						goto IL_0049;
					}
					LogResult(blockedResult);
					result = null;
				}
				goto end_IL_0007;
				IL_0049:
				try
				{
					Awaiter<CloudSaveSnapshotPullResult> val;
					if (num != 0)
					{
						val = PullCloudSnapshotInternalAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveSnapshotPullResult>, _003CPullCloudSnapshotToLocalAsync_003Ed__42>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveSnapshotPullResult>);
						num = (_003C_003E1__state = -1);
					}
					CloudSaveSnapshotPullResult result2 = val.GetResult();
					CloudSaveDataSnapshot snapshot = result2.Snapshot;
					CloudSaveDataSnapshot cloudSaveDataSnapshot = CloudSaveDataHandler.CaptureSnapshot();
					if (!result2.IsSuccess || snapshot == null)
					{
						LogResult(new CloudSaveOperationResult("拉取云端玩家进度并写入本地", isSuccess: false, isInProgress: false, result2.FailureReason ?? "拉取成功但未取得有效云端数据"), cloudSaveDataSnapshot, snapshot);
						result = null;
					}
					else if (CloudSaveDataHandler.IsSchemaNewerThanClient(snapshot))
					{
						LogResult(new CloudSaveOperationResult("拉取云端玩家进度并写入本地", isSuccess: false, isInProgress: false, "云端数据结构版本高于当前客户端，已取消写入本地"), cloudSaveDataSnapshot, snapshot);
						result = null;
					}
					else
					{
						CloudSaveDataHandler.UpgradeSchemaFromLocal(snapshot, cloudSaveDataSnapshot);
						if (!CloudSaveDataHandler.TryApplySnapshot(snapshot, commitCloudSyncState: true, null))
						{
							LogResult(new CloudSaveOperationResult("拉取云端玩家进度并写入本地", isSuccess: false, isInProgress: false, "云端玩家进度写入本地失败"), cloudSaveDataSnapshot, snapshot);
							result = null;
						}
						else
						{
							LogResult(new CloudSaveOperationResult("拉取云端玩家进度并写入本地", isSuccess: true, isInProgress: false, "已拉取云端玩家进度并写入本地"), cloudSaveDataSnapshot, snapshot);
							result = snapshot;
						}
					}
				}
				finally
				{
					if (num < 0)
					{
						EndOperation();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPushLocalSnapshotToCloudAsync_003Ed__47 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				if (num == 0)
				{
					goto IL_0049;
				}
				if (!IsCloudSaveEnabled())
				{
					CloudSaveOperationResult cloudSaveOperationResult = CreateDisabledOperationResult("本地覆盖云端");
					LogResult(cloudSaveOperationResult);
					result = cloudSaveOperationResult;
				}
				else
				{
					if (TryBeginOperation("本地覆盖云端", out var blockedResult))
					{
						goto IL_0049;
					}
					LogResult(blockedResult);
					result = blockedResult;
				}
				goto end_IL_0007;
				IL_0049:
				try
				{
					Awaiter<CloudSaveOperationResult> val;
					if (num != 0)
					{
						val = ForceUploadLocalSnapshotAsync("本地覆盖云端").GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPushLocalSnapshotToCloudAsync_003Ed__47>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
						num = (_003C_003E1__state = -1);
					}
					CloudSaveOperationResult result2 = val.GetResult();
					LogResult(result2);
					result = result2;
				}
				finally
				{
					if (num < 0)
					{
						EndOperation();
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CResolveInterruptedBindSelectionAsync_003Ed__56 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public string operationName;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				Awaiter<LoginResult> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<LoginResult>);
					num = (_003C_003E1__state = -1);
					goto IL_00dc;
				}
				if (isBindSelectionUiActive)
				{
					result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: true, "绑定后选档进行中，跳过自动同步");
				}
				else
				{
					AccountComponent account = GameApp.Account;
					if (!((Object)(object)account == (Object)null) && account.IsInitialized)
					{
						if ((int)account.CurrentIdentity != 0)
						{
							val = LoginManager.LogoutAsync().GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CResolveInterruptedBindSelectionAsync_003Ed__56>(ref val, ref this);
								return;
							}
							goto IL_00dc;
						}
						goto IL_0144;
					}
					result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "绑定后选档未完成，但账号组件未就绪，无法解绑，跳过自动同步");
				}
				goto end_IL_0007;
				IL_0144:
				CompleteBindSelection("interruptedBindRolledBack");
				result = new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, "已将未完成选档的第三方账号解绑回游客");
				goto end_IL_0007;
				IL_00dc:
				LoginResult result2 = val.GetResult();
				if (result2 != null && result2.IsSuccess)
				{
					goto IL_0144;
				}
				result = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, $"绑定后选档未完成，解绑回游客失败，跳过自动同步 code={result2?.Code} msg={result2?.Msg}");
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CUploadDecisionSnapshotAsync_003Ed__74 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		public CloudSaveDataSnapshot snapshot;

		public long pulledCloudStorageVersion;

		public CloudStorageComponent cloud;

		public string operationName;

		private Awaiter<StorageUploadResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result2;
			try
			{
				Awaiter<StorageUploadResult> val;
				if (num != 0)
				{
					byte[] array = CloudSaveDataHandler.SerializeSnapshot(snapshot);
					long num2 = Math.Max(Math.Max(0L, pulledCloudStorageVersion), cloud.GetVersion("rotaterings_player_progress_v1")) + 1;
					cloud.SetWithVersion("rotaterings_player_progress_v1", array, num2);
					val = PushCloudSaveAsync(cloud).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<StorageUploadResult>, _003CUploadDecisionSnapshotAsync_003Ed__74>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<StorageUploadResult>);
					num = (_003C_003E1__state = -1);
				}
				StorageUploadResult result = val.GetResult();
				LoginManager.NotifyAccountNotFoundIfNeeded(result.Code, "CloudSave:Push");
				CloudSaveOperationResult cloudSaveOperationResult = CreateUploadResult(operationName, result);
				lastUploadResult = cloudSaveOperationResult.Summary;
				result2 = cloudSaveOperationResult;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result2);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const string Tag = "[CloudSaveFlow]";

	public const string MarkedLocalDataBehindSummary = "决策为使用云端，已标记本地数据落后云端";

	private const string CloudSavePullLoadingSource = "CloudSavePull";

	private const string CloudSaveSystemType = "rotaterings_player_progress_v1";

	private const string CloudStorageLocalFolder = "CloudStorage";

	private const int CloudSavePullTimeoutSeconds = 3;

	private const int CloudSavePullMaxRetries = 2;

	private static readonly string[] CloudSaveSystemTypes = new string[1] { "rotaterings_player_progress_v1" };

	private static readonly CloudSaveDataDecisionHandler CloudSaveResolver = new CloudSaveDataDecisionHandler();

	private static readonly CloudSaveDataPullResolver CloudDataPullResolver = new CloudSaveDataPullResolver();

	private static UniTaskCompletionSource<CloudSaveOperationResult> pullAndDecideCompletionSource;

	private static bool isOperationInProgress;

	private static bool hasPullAndDecideSucceededThisSession;

	private static bool isBindSelectionUiActive;

	private static bool isLocalDataBehindCloud;

	private static string lastPullResult = "未执行";

	private static string lastUploadResult = "未执行";

	private static CloudSaveSyncTrackData lastSyncTrackData;

	public static string CloudSaveSystemTypeName => "rotaterings_player_progress_v1";

	public static bool IsOperationInProgress => isOperationInProgress;

	public static bool HasEverPullAndDecideSucceededThisSession => hasPullAndDecideSucceededThisSession;

	public static bool IsLocalDataBehindCloud => isLocalDataBehindCloud;

	public static bool IsCloudApplyPending => isLocalDataBehindCloud;

	public static bool IsPullAndDecideInProgress => pullAndDecideCompletionSource != null;

	public static string LastPullResult => lastPullResult;

	public static string LastUploadResult => lastUploadResult;

	public static bool IsPendingForceCloudUpload => CloudSaveCache.Data.PendingForceCloudUpload;

	public static bool IsPendingBindSelection => CloudSaveCache.Data.PendingBindSelection;

	public static bool IsCloudSaveEnabled()
	{
		CloudSaveCacheData data = CloudSaveCache.Data;
		if (!ShopService.ShopToggle())
		{
			return data.HasEverPurchased;
		}
		return true;
	}

	public static void EnsureInitialized()
	{
		if (!IsCloudSaveEnabled())
		{
			return;
		}
		LoginManager.Init();
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account == (Object)null || !account.IsInitialized)
		{
			Log.LogError("[CloudSaveFlow]", "AccountComponent 初始化失败，无法创建 CloudStorageComponent");
			return;
		}
		CloudStorageComponent val = GameApp.CloudStorage;
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)account).gameObject.AddComponent<CloudStorageComponent>();
		}
		CloudSaveHeartbeat cloudSaveHeartbeat = ((Component)account).GetComponent<CloudSaveHeartbeat>();
		if ((Object)(object)cloudSaveHeartbeat == (Object)null)
		{
			cloudSaveHeartbeat = ((Component)account).gameObject.AddComponent<CloudSaveHeartbeat>();
		}
		cloudSaveHeartbeat.Initialize(account);
		val.AutoPushEnabled = false;
		val.SetPullSystemTypes((IList<string>)null, (ICloudStorageConflictResolver)null);
		LoginManager.Login();
	}

	public static UniTask<LoginResult> WaitForLoginAsync()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return LoginManager.WaitForLoginAsync();
	}

	[AsyncStateMachine(typeof(_003CPullCloudSnapshotAsync_003Ed__41))]
	public static UniTask<CloudSaveSnapshotPullResult> PullCloudSnapshotAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPullCloudSnapshotAsync_003Ed__41 _003CPullCloudSnapshotAsync_003Ed__ = default(_003CPullCloudSnapshotAsync_003Ed__41);
		_003CPullCloudSnapshotAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveSnapshotPullResult>.Create();
		_003CPullCloudSnapshotAsync_003Ed__._003C_003E1__state = -1;
		_003CPullCloudSnapshotAsync_003Ed__._003C_003Et__builder.Start<_003CPullCloudSnapshotAsync_003Ed__41>(ref _003CPullCloudSnapshotAsync_003Ed__);
		return _003CPullCloudSnapshotAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CPullCloudSnapshotToLocalAsync_003Ed__42))]
	public static UniTask<CloudSaveDataSnapshot> PullCloudSnapshotToLocalAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPullCloudSnapshotToLocalAsync_003Ed__42 _003CPullCloudSnapshotToLocalAsync_003Ed__ = default(_003CPullCloudSnapshotToLocalAsync_003Ed__42);
		_003CPullCloudSnapshotToLocalAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveDataSnapshot>.Create();
		_003CPullCloudSnapshotToLocalAsync_003Ed__._003C_003E1__state = -1;
		_003CPullCloudSnapshotToLocalAsync_003Ed__._003C_003Et__builder.Start<_003CPullCloudSnapshotToLocalAsync_003Ed__42>(ref _003CPullCloudSnapshotToLocalAsync_003Ed__);
		return _003CPullCloudSnapshotToLocalAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CPullCloudSnapshotInternalAsync_003Ed__43))]
	private static UniTask<CloudSaveSnapshotPullResult> PullCloudSnapshotInternalAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPullCloudSnapshotInternalAsync_003Ed__43 _003CPullCloudSnapshotInternalAsync_003Ed__ = default(_003CPullCloudSnapshotInternalAsync_003Ed__43);
		_003CPullCloudSnapshotInternalAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveSnapshotPullResult>.Create();
		_003CPullCloudSnapshotInternalAsync_003Ed__._003C_003E1__state = -1;
		_003CPullCloudSnapshotInternalAsync_003Ed__._003C_003Et__builder.Start<_003CPullCloudSnapshotInternalAsync_003Ed__43>(ref _003CPullCloudSnapshotInternalAsync_003Ed__);
		return _003CPullCloudSnapshotInternalAsync_003Ed__._003C_003Et__builder.Task;
	}

	public static CloudSaveOperationResult WriteProbeData()
	{
		if (!IsCloudSaveEnabled())
		{
			CloudSaveOperationResult result = CreateDisabledOperationResult("写入云存档数据");
			LogResult(result);
			return result;
		}
		if (!TryBeginOperation("写入云存档数据", out var blockedResult))
		{
			LogResult(blockedResult);
			return blockedResult;
		}
		try
		{
			CloudSaveOperationResult unavailableResult;
			CloudStorageComponent cloudStorage = GetCloudStorage(out unavailableResult);
			if ((Object)(object)cloudStorage == (Object)null)
			{
				LogResult(unavailableResult);
				return unavailableResult;
			}
			byte[] array = CloudSaveDataHandler.SerializeCurrentSnapshot();
			cloudStorage.Set("rotaterings_player_progress_v1", array);
			CloudSaveOperationResult result2 = new CloudSaveOperationResult("写入云存档数据", isSuccess: true, isInProgress: false, string.Format("已采集本地数据并写入缓存，版本={0}，dirty={1}，字节={2}", cloudStorage.GetVersion("rotaterings_player_progress_v1"), cloudStorage.IsDirty("rotaterings_player_progress_v1"), array.Length));
			LogResult(result2);
			return result2;
		}
		catch (Exception ex)
		{
			CloudSaveOperationResult result3 = new CloudSaveOperationResult("写入云存档数据", isSuccess: false, isInProgress: false, $"写入云存档数据异常: {ex.Message}");
			Log.LogError("[CloudSaveFlow]", "{0}", new object[1] { ex });
			LogResult(result3);
			return result3;
		}
		finally
		{
			EndOperation();
		}
	}

	[AsyncStateMachine(typeof(_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__45))]
	public static UniTask<CloudSaveOperationResult> DeleteCurrentThirdPartyCloudAccountAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__45 _003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__ = default(_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__45);
		_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__._003C_003E1__state = -1;
		_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__._003C_003Et__builder.Start<_003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__45>(ref _003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__);
		return _003CDeleteCurrentThirdPartyCloudAccountAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static bool DeleteCloudStorageLocalFile(ulong uid)
	{
		if (uid == 0L)
		{
			Log.LogError("[CloudSaveFlow]", "删除本地云存档文件失败：UID 无效");
			return false;
		}
		string text = string.Format("{0}/{1}.dat", "CloudStorage", uid);
		try
		{
			if (!ES3.FileExists(text))
			{
				return true;
			}
			ES3.DeleteFile(text);
			return true;
		}
		catch (Exception ex)
		{
			Log.LogError("[CloudSaveFlow]", "删除第三方账号的本地云存档文件失败 path={0} error={1}", new object[2] { text, ex });
			return false;
		}
	}

	[AsyncStateMachine(typeof(_003CPushLocalSnapshotToCloudAsync_003Ed__47))]
	public static UniTask<CloudSaveOperationResult> PushLocalSnapshotToCloudAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPushLocalSnapshotToCloudAsync_003Ed__47 _003CPushLocalSnapshotToCloudAsync_003Ed__ = default(_003CPushLocalSnapshotToCloudAsync_003Ed__47);
		_003CPushLocalSnapshotToCloudAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CPushLocalSnapshotToCloudAsync_003Ed__._003C_003E1__state = -1;
		_003CPushLocalSnapshotToCloudAsync_003Ed__._003C_003Et__builder.Start<_003CPushLocalSnapshotToCloudAsync_003Ed__47>(ref _003CPushLocalSnapshotToCloudAsync_003Ed__);
		return _003CPushLocalSnapshotToCloudAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CForceUploadLocalSnapshotAsync_003Ed__48))]
	internal static UniTask<CloudSaveOperationResult> ForceUploadLocalSnapshotAsync(string operationName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CForceUploadLocalSnapshotAsync_003Ed__48 _003CForceUploadLocalSnapshotAsync_003Ed__ = default(_003CForceUploadLocalSnapshotAsync_003Ed__48);
		_003CForceUploadLocalSnapshotAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CForceUploadLocalSnapshotAsync_003Ed__.operationName = operationName;
		_003CForceUploadLocalSnapshotAsync_003Ed__._003C_003E1__state = -1;
		_003CForceUploadLocalSnapshotAsync_003Ed__._003C_003Et__builder.Start<_003CForceUploadLocalSnapshotAsync_003Ed__48>(ref _003CForceUploadLocalSnapshotAsync_003Ed__);
		return _003CForceUploadLocalSnapshotAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static UniTask<StorageUploadResult> PushCloudSaveAsync(CloudStorageComponent cloud)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return cloud.PushAsync((IList<string>)CloudSaveSystemTypes, default(CancellationToken));
	}

	[AsyncStateMachine(typeof(_003CForcePushSnapshotAsync_003Ed__50))]
	private static UniTask<CloudSaveOperationResult> ForcePushSnapshotAsync(CloudStorageComponent cloud, CloudSaveDataSnapshot snapshot, string operationName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CForcePushSnapshotAsync_003Ed__50 _003CForcePushSnapshotAsync_003Ed__ = default(_003CForcePushSnapshotAsync_003Ed__50);
		_003CForcePushSnapshotAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CForcePushSnapshotAsync_003Ed__.cloud = cloud;
		_003CForcePushSnapshotAsync_003Ed__.snapshot = snapshot;
		_003CForcePushSnapshotAsync_003Ed__.operationName = operationName;
		_003CForcePushSnapshotAsync_003Ed__._003C_003E1__state = -1;
		_003CForcePushSnapshotAsync_003Ed__._003C_003Et__builder.Start<_003CForcePushSnapshotAsync_003Ed__50>(ref _003CForcePushSnapshotAsync_003Ed__);
		return _003CForcePushSnapshotAsync_003Ed__._003C_003Et__builder.Task;
	}

	internal static void SetPendingForceCloudUpload(bool value, string reason)
	{
		try
		{
			CloudSaveCache.SetPendingForceCloudUpload(value);
		}
		catch (Exception)
		{
		}
	}

	internal static void BeginBindSelection(string reason)
	{
		isBindSelectionUiActive = true;
		SetPendingBindSelectionFlag(value: true, reason);
	}

	internal static void SuspendBindSelection()
	{
		isBindSelectionUiActive = false;
	}

	internal static void CompleteBindSelection(string reason)
	{
		isBindSelectionUiActive = false;
		if (IsPendingBindSelection)
		{
			SetPendingBindSelectionFlag(value: false, reason);
		}
	}

	private static void SetPendingBindSelectionFlag(bool value, string reason)
	{
		try
		{
			CloudSaveCache.SetPendingBindSelection(value);
		}
		catch (Exception)
		{
		}
	}

	[AsyncStateMachine(typeof(_003CResolveInterruptedBindSelectionAsync_003Ed__56))]
	private static UniTask<CloudSaveOperationResult> ResolveInterruptedBindSelectionAsync(string operationName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CResolveInterruptedBindSelectionAsync_003Ed__56 _003CResolveInterruptedBindSelectionAsync_003Ed__ = default(_003CResolveInterruptedBindSelectionAsync_003Ed__56);
		_003CResolveInterruptedBindSelectionAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CResolveInterruptedBindSelectionAsync_003Ed__.operationName = operationName;
		_003CResolveInterruptedBindSelectionAsync_003Ed__._003C_003E1__state = -1;
		_003CResolveInterruptedBindSelectionAsync_003Ed__._003C_003Et__builder.Start<_003CResolveInterruptedBindSelectionAsync_003Ed__56>(ref _003CResolveInterruptedBindSelectionAsync_003Ed__);
		return _003CResolveInterruptedBindSelectionAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CPullAndDecideAsync_003Ed__57))]
	public static UniTask<CloudSaveOperationResult> PullAndDecideAsync(CloudSaveSyncMode mode = CloudSaveSyncMode.Normal)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPullAndDecideAsync_003Ed__57 _003CPullAndDecideAsync_003Ed__ = default(_003CPullAndDecideAsync_003Ed__57);
		_003CPullAndDecideAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CPullAndDecideAsync_003Ed__.mode = mode;
		_003CPullAndDecideAsync_003Ed__._003C_003E1__state = -1;
		_003CPullAndDecideAsync_003Ed__._003C_003Et__builder.Start<_003CPullAndDecideAsync_003Ed__57>(ref _003CPullAndDecideAsync_003Ed__);
		return _003CPullAndDecideAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static void ShowPullAndDecideResult(CloudSaveOperationResult operationResult, CloudSaveDataDecisionResult decision)
	{
		if (operationResult != null && !IsMarkedLocalDataBehindResult(operationResult) && operationResult.IsSuccess && decision?.LocalData != null && decision.CloudData != null)
		{
			CloudSaveMainLevelMergeResult mergeResult = decision.MergeResult;
			if (mergeResult == CloudSaveMainLevelMergeResult.UseCloudNoUpload || mergeResult == CloudSaveMainLevelMergeResult.UseCloudNeedUpload)
			{
				ToastService.Show(GameApp.Lang.GetText("Progress_Sync_Latest_Desc", Array.Empty<object>()));
			}
		}
	}

	private static int GetCurrentMainLevelId()
	{
		return GameApp.Proxy.Get<LevelModelProxy>()?.MainLevel ?? 0;
	}

	[AsyncStateMachine(typeof(_003CPullAndDecideInternalAsync_003Ed__60))]
	private static UniTask<CloudSaveOperationResult> PullAndDecideInternalAsync(CloudSaveSyncMode mode)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CPullAndDecideInternalAsync_003Ed__60 _003CPullAndDecideInternalAsync_003Ed__ = default(_003CPullAndDecideInternalAsync_003Ed__60);
		_003CPullAndDecideInternalAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CPullAndDecideInternalAsync_003Ed__.mode = mode;
		_003CPullAndDecideInternalAsync_003Ed__._003C_003E1__state = -1;
		_003CPullAndDecideInternalAsync_003Ed__._003C_003Et__builder.Start<_003CPullAndDecideInternalAsync_003Ed__60>(ref _003CPullAndDecideInternalAsync_003Ed__);
		return _003CPullAndDecideInternalAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static CloudSaveSyncTrackData CreateCloudSaveSyncTrackData(CloudSaveOperationResult result, long startedAt, CloudSaveDataSnapshot localSnapshot, CloudSaveDataSnapshot cloudSnapshot, CloudSaveDataSnapshot finalSnapshot, string selectedSaveSource, bool isConflict)
	{
		bool flag = IsMarkedLocalDataBehindResult(result);
		return new CloudSaveSyncTrackData(GetTrackResult(result), GetElapsedMilliseconds(startedAt), localSnapshot, cloudSnapshot, flag ? null : finalSnapshot, flag ? string.Empty : selectedSaveSource, (result != null && result.IsSuccess) ? null : GetTrackFailReason(result), isConflict);
	}

	private static string GetTrackResult(CloudSaveOperationResult result)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (result == null || result.IsInProgress)
		{
			return "skipped";
		}
		if (result.IsSuccess)
		{
			return "success";
		}
		if (result.ErrorCode != (CloudStorageErrorCode?)2002)
		{
			return "fail";
		}
		return "timeout";
	}

	private static string GetTrackFailReason(CloudSaveOperationResult result)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		object obj;
		if (result == null)
		{
			obj = null;
		}
		else
		{
			CloudStorageErrorCode? errorCode = result.ErrorCode;
			obj = (errorCode.HasValue ? ((object)errorCode.GetValueOrDefault()/*cast due to .constrained prefix*/).ToString() : null);
		}
		if (obj == null)
		{
			obj = "unknown";
		}
		return (string)obj;
	}

	private static int GetElapsedMilliseconds(long startedAt)
	{
		double val = (double)(Stopwatch.GetTimestamp() - startedAt) * 1000.0 / (double)Stopwatch.Frequency;
		return (int)Math.Max(0.0, Math.Min(2147483647.0, val));
	}

	private static string GetSelectedSaveSource(CloudSaveDataDecisionResult decision)
	{
		if (decision == null)
		{
			return string.Empty;
		}
		CloudSaveMainLevelMergeResult mergeResult = decision.MergeResult;
		if (mergeResult != CloudSaveMainLevelMergeResult.UseCloudNoUpload && mergeResult != CloudSaveMainLevelMergeResult.UseCloudNeedUpload)
		{
			return "local";
		}
		return "server";
	}

	private static void WriteCurrentSnapshotIfChanged(CloudStorageComponent cloud)
	{
		byte[] array = CloudSaveDataHandler.SerializeCurrentSnapshot();
		if (!CloudSaveDataHandler.AreRawDataEqual(cloud.Get("rotaterings_player_progress_v1"), array))
		{
			cloud.Set("rotaterings_player_progress_v1", array);
		}
	}

	[AsyncStateMachine(typeof(_003CProcessDecisionAsync_003Ed__67))]
	private static UniTask<CloudSaveOperationResult> ProcessDecisionAsync(CloudStorageComponent cloud, CloudSaveDataDecisionResult decision, string operationName, CloudSaveSyncMode mode)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CProcessDecisionAsync_003Ed__67 _003CProcessDecisionAsync_003Ed__ = default(_003CProcessDecisionAsync_003Ed__67);
		_003CProcessDecisionAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CProcessDecisionAsync_003Ed__.cloud = cloud;
		_003CProcessDecisionAsync_003Ed__.decision = decision;
		_003CProcessDecisionAsync_003Ed__.operationName = operationName;
		_003CProcessDecisionAsync_003Ed__.mode = mode;
		_003CProcessDecisionAsync_003Ed__._003C_003E1__state = -1;
		_003CProcessDecisionAsync_003Ed__._003C_003Et__builder.Start<_003CProcessDecisionAsync_003Ed__67>(ref _003CProcessDecisionAsync_003Ed__);
		return _003CProcessDecisionAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static bool IsGameplayInProgress()
	{
		IGameplayManager active = Gameplay.Active;
		if (active != null)
		{
			if (!((Object)(object)active.CurrentLevelObject != (Object)null))
			{
				return active.IsLoadingLevel;
			}
			return true;
		}
		return false;
	}

	private static void ReturnToHomeBeforeApplyingShopPurchaseSyncIfNeeded(bool shouldReturnToHome)
	{
		if (shouldReturnToHome)
		{
			Gameplay.Active?.ExitToHome();
			LevelBackHomeCommand.CloseUIGameThenOpenHome();
		}
	}

	public static bool IsHomeReadyForCloudSaveApply()
	{
		if (!GameApp.UI.IsOpen("UIHome") || GameApp.UI.IsOpen("UIGame") || EnterLevelCommand.IsEnteringLevel)
		{
			return false;
		}
		IGameplayManager active = Gameplay.Active;
		if (active != null)
		{
			if ((Object)(object)active.CurrentLevelObject == (Object)null)
			{
				return !active.IsLoadingLevel;
			}
			return false;
		}
		return true;
	}

	public static bool IsMarkedLocalDataBehindResult(CloudSaveOperationResult result)
	{
		if (result != null && result.IsSuccess)
		{
			return result.Summary == "决策为使用云端，已标记本地数据落后云端";
		}
		return false;
	}

	public static void ReportLastSyncTrackData(string triggerNode)
	{
		CloudSaveSyncTrackData cloudSaveSyncTrackData = lastSyncTrackData;
		lastSyncTrackData = null;
		cloudSaveSyncTrackData?.Report(triggerNode);
	}

	private static void ClearLocalDataBehindCloudAfterSuccessfulPullAndDecide()
	{
		if (isLocalDataBehindCloud)
		{
			isLocalDataBehindCloud = false;
		}
	}

	[AsyncStateMachine(typeof(_003CUploadDecisionSnapshotAsync_003Ed__74))]
	private static UniTask<CloudSaveOperationResult> UploadDecisionSnapshotAsync(CloudStorageComponent cloud, CloudSaveDataSnapshot snapshot, long pulledCloudStorageVersion, string operationName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003CUploadDecisionSnapshotAsync_003Ed__74 _003CUploadDecisionSnapshotAsync_003Ed__ = default(_003CUploadDecisionSnapshotAsync_003Ed__74);
		_003CUploadDecisionSnapshotAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CUploadDecisionSnapshotAsync_003Ed__.cloud = cloud;
		_003CUploadDecisionSnapshotAsync_003Ed__.snapshot = snapshot;
		_003CUploadDecisionSnapshotAsync_003Ed__.pulledCloudStorageVersion = pulledCloudStorageVersion;
		_003CUploadDecisionSnapshotAsync_003Ed__.operationName = operationName;
		_003CUploadDecisionSnapshotAsync_003Ed__._003C_003E1__state = -1;
		_003CUploadDecisionSnapshotAsync_003Ed__._003C_003Et__builder.Start<_003CUploadDecisionSnapshotAsync_003Ed__74>(ref _003CUploadDecisionSnapshotAsync_003Ed__);
		return _003CUploadDecisionSnapshotAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static bool TryCommitUploadedLocalData(CloudSaveDataSnapshot snapshot, List<CloudSavePurchaseCount> mergedPendingPurchaseCounts, out string error)
	{
		CloudSaveCache.CommitPurchaseSync(snapshot.SyncedPurchaseCounts, mergedPendingPurchaseCounts);
		CloudSaveCache.Data.CloudSaveDataVersion = Math.Max(0L, snapshot.Version);
		error = string.Empty;
		return true;
	}

	private static CloudSaveOperationResult CreateDisabledOperationResult(string operationName)
	{
		return new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "云存档功能未开启");
	}

	private static bool TryBeginOperation(string operationName, out CloudSaveOperationResult blockedResult)
	{
		if (isOperationInProgress)
		{
			blockedResult = new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: true, "已有云存档操作进行中");
			return false;
		}
		isOperationInProgress = true;
		blockedResult = null;
		return true;
	}

	private static void EndOperation()
	{
		isOperationInProgress = false;
	}

	private static void ConfigurePullRequestPolicy()
	{
		AccountComponent account = GameApp.Account;
		if (!((Object)(object)account == (Object)null))
		{
			account.TimeoutSec = 3;
			account.MaxRetries = 2;
		}
	}

	private static CloudStorageComponent GetCloudStorage(out CloudSaveOperationResult unavailableResult)
	{
		CloudStorageComponent cloudStorage = GameApp.CloudStorage;
		if ((Object)(object)cloudStorage != (Object)null)
		{
			unavailableResult = null;
			return cloudStorage;
		}
		unavailableResult = new CloudSaveOperationResult("云存档操作", isSuccess: false, isInProgress: false, "CloudStorageComponent 未初始化，请重新启动游戏");
		return null;
	}

	private static CloudSaveOperationResult CreatePullResult(string operationName, StoragePullResult pullResult)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (pullResult == null)
		{
			return new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "框架未返回拉取结果");
		}
		if (!pullResult.IsSuccess)
		{
			return new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, $"拉取失败 code={pullResult.Code} http={pullResult.HttpStatus} msg={pullResult.Msg}", pullResult.Code);
		}
		return new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, $"拉取成功 updated={pullResult.UpdatedCount} keptLocal={pullResult.KeptLocalCount} skipped={pullResult.SkippedCount} requested={pullResult.RequestedCount}");
	}

	private static CloudSaveOperationResult CreateUploadResult(string operationName, StorageUploadResult uploadResult)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (uploadResult == null)
		{
			return new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, "框架未返回上传结果");
		}
		if (!uploadResult.IsSuccess)
		{
			return new CloudSaveOperationResult(operationName, isSuccess: false, isInProgress: false, $"上传失败 code={uploadResult.Code} http={uploadResult.HttpStatus} affected={uploadResult.AffectedCount} msg={uploadResult.Msg}", uploadResult.Code);
		}
		return new CloudSaveOperationResult(operationName, isSuccess: true, isInProgress: false, $"上传成功 affected={uploadResult.AffectedCount} serverTime={uploadResult.ServerTime}");
	}

	private static void LogSnapshotPullResult(string operationName, CloudSaveSnapshotPullResult pullResult)
	{
		CloudSaveDataSnapshot localSnapshot = CloudSaveDataHandler.CaptureSnapshot();
		bool flag = pullResult != null && pullResult.Snapshot != null;
		LogResult(new CloudSaveOperationResult(operationName, (pullResult?.IsSuccess ?? false) && flag, isInProgress: false, flag ? "拉取成功，已取得有效云端数据" : (pullResult?.FailureReason ?? "拉取成功但云端没有有效数据")), localSnapshot, pullResult?.Snapshot);
	}

	private static void GetCurrentProgressSummary(out int mainLevelId, out string propCounts)
	{
		mainLevelId = GetCurrentMainLevelId();
		StringBuilder stringBuilder = new StringBuilder();
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		if (propModelProxy?.Data.PropDict == null)
		{
			propCounts = string.Empty;
			return;
		}
		foreach (KeyValuePair<EPropType, int> item in propModelProxy.Data.PropDict)
		{
			if (item.Value >= 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(item.Key).Append(':').Append(item.Value);
			}
		}
		propCounts = stringBuilder.ToString();
	}

	private static string GetSnapshotProgressSummary(CloudSaveDataSnapshot snapshot)
	{
		if (snapshot == null)
		{
			return "null";
		}
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append("MainLevelId=").Append(snapshot.MainLevelId).Append(" EndlessLevelId=")
			.Append(snapshot.EndlessLevelId)
			.Append(" PropCounts=[");
		int length = stringBuilder.Length;
		if (snapshot.PropCounts != null)
		{
			for (int i = 0; i < snapshot.PropCounts.Count; i++)
			{
				CloudSavePropCount cloudSavePropCount = snapshot.PropCounts[i];
				if (cloudSavePropCount != null)
				{
					if (stringBuilder.Length > length)
					{
						stringBuilder.Append(',');
					}
					stringBuilder.Append(cloudSavePropCount.PropId).Append(':').Append(cloudSavePropCount.Count);
				}
			}
		}
		stringBuilder.Append("] PurchaseCounts=[");
		AppendPurchaseCountsSummary(stringBuilder, snapshot.PurchaseCounts);
		stringBuilder.Append("] SyncedPurchaseCounts=[");
		AppendPurchaseCountsSummary(stringBuilder, snapshot.SyncedPurchaseCounts);
		stringBuilder.Append(']');
		return stringBuilder.ToString();
	}

	private static void AppendPurchaseCountsSummary(StringBuilder builder, List<CloudSavePurchaseCount> purchaseCounts)
	{
		if (purchaseCounts == null)
		{
			return;
		}
		int length = builder.Length;
		for (int i = 0; i < purchaseCounts.Count; i++)
		{
			CloudSavePurchaseCount cloudSavePurchaseCount = purchaseCounts[i];
			if (cloudSavePurchaseCount != null)
			{
				if (builder.Length > length)
				{
					builder.Append(',');
				}
				builder.Append(cloudSavePurchaseCount.PropId).Append(':').Append(cloudSavePurchaseCount.Count);
			}
		}
	}

	private static void LogResult(CloudSaveOperationResult result, CloudSaveDataSnapshot localSnapshot = null, CloudSaveDataSnapshot cloudSnapshot = null, CloudSaveDataSnapshot finalSnapshot = null)
	{
		GetCurrentProgressSummary(out var _, out var _);
		_ = localSnapshot?.Version;
		_ = cloudSnapshot?.Version;
		_ = finalSnapshot?.Version;
		_ = result.IsSuccess;
	}
}
