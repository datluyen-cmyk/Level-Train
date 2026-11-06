using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.Tool;
using XingYun.Core.Singletons;
using XingYun.IAP.Core;
using XingYun.UI.Core;

namespace RotateRings.UI.Shop;

public static class PurchaseService
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public int shopItemId;

		public EIapEnterPath enterPath;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Awaiter<CloudSaveOperationResult> val;
				if (num != 0)
				{
					if (!CloudSaveFlowService.IsCloudSaveEnabled() || (CloudSaveFlowService.HasEverPullAndDecideSucceededThisSession && !CloudSaveFlowService.IsPendingForceCloudUpload && !CloudSaveFlowService.IsPendingBindSelection))
					{
						goto IL_00ca;
					}
					val = CloudSaveFlowService.PullAndDecideAsync(CloudSaveSyncMode.ShopPurchase).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__10>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
					num = (_003C_003E1__state = -1);
				}
				CloudSaveOperationResult result = val.GetResult();
				CloudSaveFlowService.ReportLastSyncTrackData("purchase_before");
				if (!CloudSaveFlowService.IsPendingForceCloudUpload)
				{
					_ = result.IsSuccess;
					goto IL_00ca;
				}
				Hide("ShopPurchase");
				GameApp.UI.Open(UIPopupPathDefine.UIConnectionLost, (UILayer)2, (object)(XingYunIAPErrorCode)2001, (Action)null);
				goto end_IL_0007;
				IL_00ca:
				GameApp.Command.Execute<ShopBuyCommand>((object)new ShopBuyParam
				{
					ShopItemId = shopItemId,
					EnterPath = enterPath
				});
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
	private struct _003CSyncCloudSaveAfterPurchaseAsync_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private int _003Cattempt_003E5__2;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num != 0)
				{
					isPostPurchaseCloudSaveSyncRunning = true;
				}
				try
				{
					if (num != 0)
					{
						_003Cattempt_003E5__2 = 1;
						goto IL_00a1;
					}
					Awaiter<CloudSaveOperationResult> val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
					num = (_003C_003E1__state = -1);
					goto IL_0077;
					IL_0077:
					if (!val.GetResult().IsSuccess)
					{
						_003Cattempt_003E5__2++;
						goto IL_00a1;
					}
					CloudSaveFlowService.ReportLastSyncTrackData("purchase_after");
					goto end_IL_0011;
					IL_00a1:
					if (_003Cattempt_003E5__2 <= 3)
					{
						val = CloudSaveFlowService.PullAndDecideAsync(CloudSaveSyncMode.ShopPurchase).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CSyncCloudSaveAfterPurchaseAsync_003Ed__12>(ref val, ref this);
							return;
						}
						goto IL_0077;
					}
					CloudSaveFlowService.ReportLastSyncTrackData("purchase_after");
					end_IL_0011:;
				}
				finally
				{
					if (num < 0)
					{
						isPostPurchaseCloudSaveSyncRunning = false;
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

	public const string ShopPurchaseSource = "ShopPurchase";

	public const string RestorePurchaseSource = "RestorePurchase";

	private const string LogTag = "PurchaseService";

	private const int PostPurchaseCloudSaveSyncMaxAttempts = 3;

	private static readonly Dictionary<string, int> SourceCounts = new Dictionary<string, int>();

	private static bool isPostPurchaseCloudSaveSyncRunning;

	private static int referenceCount;

	public static bool IsShowing => referenceCount > 0;

	public static void RequestPurchase(int shopItemId, EIapEnterPath enterPath = EIapEnterPath.HomeEntry)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!isPostPurchaseCloudSaveSyncRunning)
		{
			if (!NetUtil.IsHaveNet)
			{
				GameApp.UI.Open(UIPopupPathDefine.UIConnectionLost, (UILayer)2, (object)(XingYunIAPErrorCode)2001, (Action)null);
			}
			else if (Singleton<IAPManager>.Instance.CanStartNewPurchase())
			{
				Show("ShopPurchase");
				UniTaskVoid val = RequestPurchaseAfterCloudSaveSyncAsync(shopItemId, enterPath);
				((UniTaskVoid)(ref val)).Forget();
			}
		}
	}

	[AsyncStateMachine(typeof(_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__10))]
	private static UniTaskVoid RequestPurchaseAfterCloudSaveSyncAsync(int shopItemId, EIapEnterPath enterPath)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__10 _003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__ = default(_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__10);
		_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__.shopItemId = shopItemId;
		_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__.enterPath = enterPath;
		_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__._003C_003Et__builder)).Start<_003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__10>(ref _003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CRequestPurchaseAfterCloudSaveSyncAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public static void RequestCloudSaveSyncAfterPurchase()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (CloudSaveFlowService.IsCloudSaveEnabled() && !isPostPurchaseCloudSaveSyncRunning)
		{
			UniTaskVoid val = SyncCloudSaveAfterPurchaseAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CSyncCloudSaveAfterPurchaseAsync_003Ed__12))]
	private static UniTaskVoid SyncCloudSaveAfterPurchaseAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CSyncCloudSaveAfterPurchaseAsync_003Ed__12 _003CSyncCloudSaveAfterPurchaseAsync_003Ed__ = default(_003CSyncCloudSaveAfterPurchaseAsync_003Ed__12);
		_003CSyncCloudSaveAfterPurchaseAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CSyncCloudSaveAfterPurchaseAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CSyncCloudSaveAfterPurchaseAsync_003Ed__._003C_003Et__builder)).Start<_003CSyncCloudSaveAfterPurchaseAsync_003Ed__12>(ref _003CSyncCloudSaveAfterPurchaseAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CSyncCloudSaveAfterPurchaseAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public static void Show(string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			source = "Unknown";
		}
		referenceCount++;
		SourceCounts.TryGetValue(source, out var value);
		SourceCounts[source] = value + 1;
		Notify(isShowing: true);
	}

	public static void Hide(string source)
	{
		if (!string.IsNullOrEmpty(source) && SourceCounts.TryGetValue(source, out var value))
		{
			referenceCount = Math.Max(0, referenceCount - 1);
			if (value <= 1)
			{
				SourceCounts.Remove(source);
			}
			else
			{
				SourceCounts[source] = value - 1;
			}
			if (!IsShowing)
			{
				Notify(isShowing: false);
			}
		}
	}

	public static void ForceClear()
	{
		if (IsShowing)
		{
			referenceCount = 0;
			SourceCounts.Clear();
			Notify(isShowing: false);
		}
	}

	private static void Notify(bool isShowing)
	{
		GameApp.Event.Send<PurchaseLoadingEvent>(new PurchaseLoadingEvent
		{
			IsShowing = isShowing
		});
	}
}
