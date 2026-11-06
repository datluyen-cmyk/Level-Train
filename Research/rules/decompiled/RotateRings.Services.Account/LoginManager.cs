using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using RotateRings.Tool;
using RotateRings.UI.Shop;
using UniLog;
using UnityEngine;
using XingYun.Account;
using XingYun.Account.CloudStorage;
using XingYun.Aot;

namespace RotateRings.Services.Account;

public static class LoginManager
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBindThirdPartyAsync_003Ed__40 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<LoginResult> _003C_003Et__builder;

		public ThirdPartyType type;

		public CancellationToken cancellationToken;

		private AccountComponent _003Caccount_003E5__2;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0283: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0242: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0264: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LoginResult result;
			try
			{
				switch (num)
				{
				default:
					Init();
					if (!IsFeatureEnabled)
					{
						result = FailBindAndTrack(type, (AccountErrorCode)1003, "账号功能未开启");
					}
					else if (!NetUtil.IsHaveNet)
					{
						result = FailBindAndTrack(type, (AccountErrorCode)2001, "无网络");
					}
					else if (IsThirdPartyLoginInFlight)
					{
						result = FailBindAndTrack(type, (AccountErrorCode)1003, "第三方登录进行中");
					}
					else
					{
						_003Caccount_003E5__2 = GameApp.Account;
						if (!((Object)(object)_003Caccount_003E5__2 == (Object)null) && _003Caccount_003E5__2.IsInitialized)
						{
							accountStatusBeforeOp = RRDataTrack.GetAccountStatus();
							if (silentLoginInFlight && loginCompletionSource != null)
							{
								bindGuestLoginPrepInFlight = true;
								goto case 0;
							}
							if (!_003Caccount_003E5__2.IsLoggedIn)
							{
								bindGuestLoginPrepInFlight = true;
								goto case 1;
							}
							goto IL_020b;
						}
						result = FailBindAndTrack(type, (AccountErrorCode)1001, "账号组件未初始化");
					}
					goto end_IL_0007;
				case 0:
					try
					{
						Awaiter<LoginResult> val;
						if (num != 0)
						{
							val = loginCompletionSource.Task.GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 0);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CBindThirdPartyAsync_003Ed__40>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<LoginResult>);
							num = (_003C_003E1__state = -1);
						}
						LoginResult result2 = val.GetResult();
						if (result2 != null)
						{
							_ = result2.IsSuccess;
						}
					}
					finally
					{
						if (num < 0)
						{
							bindGuestLoginPrepInFlight = false;
						}
					}
					goto IL_020b;
				case 1:
					try
					{
						Awaiter<LoginResult> val;
						if (num != 1)
						{
							val = _003Caccount_003E5__2.LoginAsync(cancellationToken).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__1 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CBindThirdPartyAsync_003Ed__40>(ref val, ref this);
								return;
							}
						}
						else
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter<LoginResult>);
							num = (_003C_003E1__state = -1);
						}
						LoginResult result3 = val.GetResult();
						if (result3 != null)
						{
							_ = result3.IsSuccess;
						}
					}
					finally
					{
						if (num < 0)
						{
							bindGuestLoginPrepInFlight = false;
						}
					}
					goto IL_020b;
				case 2:
					break;
					IL_020b:
					accountStatusBeforeOp = RRDataTrack.GetAccountStatus();
					thirdPartyLoginInFlight = true;
					userInitiatedThirdPartyType = type;
					break;
				}
				try
				{
					Awaiter<LoginResult> val;
					if (num != 2)
					{
						val = _003Caccount_003E5__2.BindThirdPartyAsync(type, cancellationToken).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 2);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CBindThirdPartyAsync_003Ed__40>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
					}
					LoginResult result4 = val.GetResult();
					if (result4 != null && result4.IsSuccess)
					{
						CloudSaveFlowService.BeginBindSelection("BindThirdParty:success");
					}
					result = result4;
				}
				finally
				{
					if (num < 0)
					{
						thirdPartyLoginInFlight = false;
						userInitiatedThirdPartyType = null;
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Caccount_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Caccount_003E5__2 = null;
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
	private struct _003CDoForceLogoutToGuestAsync_003Ed__50 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private CloudStorageComponent _003Ccloud_003E5__2;

		private bool _003CautoPushWasEnabled_003E5__3;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num != 0)
				{
					_003Ccloud_003E5__2 = GameApp.CloudStorage;
					_003CautoPushWasEnabled_003E5__3 = (Object)(object)_003Ccloud_003E5__2 != (Object)null && _003Ccloud_003E5__2.AutoPushEnabled;
					if (_003CautoPushWasEnabled_003E5__3)
					{
						_003Ccloud_003E5__2.AutoPushEnabled = false;
					}
				}
				try
				{
					Awaiter<LoginResult> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
						goto IL_00c0;
					}
					if ((Object)(object)GameApp.Account != (Object)null)
					{
						val = GameApp.Account.UnbindThirdPartyAsync(default(CancellationToken)).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CDoForceLogoutToGuestAsync_003Ed__50>(ref val, ref this);
							return;
						}
						goto IL_00c0;
					}
					goto end_IL_004c;
					IL_00c0:
					val.GetResult();
					end_IL_004c:;
				}
				catch (Exception)
				{
				}
				finally
				{
					if (num < 0)
					{
						if (_003CautoPushWasEnabled_003E5__3 && (Object)(object)_003Ccloud_003E5__2 != (Object)null)
						{
							_003Ccloud_003E5__2.AutoPushEnabled = true;
						}
						accountNotFoundHandling = false;
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Ccloud_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Ccloud_003E5__2 = null;
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
	private struct _003CDoSilentLoginAsync_003Ed__49 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				LoginResult val2;
				try
				{
					Awaiter<LoginResult> val;
					if (num == 0)
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
						goto IL_0090;
					}
					AccountComponent account = GameApp.Account;
					if (!((Object)(object)account == (Object)null))
					{
						val = account.LoginAsync(default(CancellationToken)).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CDoSilentLoginAsync_003Ed__49>(ref val, ref this);
							return;
						}
						goto IL_0090;
					}
					val2 = Fail((AccountErrorCode)1001, "Account 为空");
					goto end_IL_000a;
					IL_0090:
					val2 = val.GetResult();
					end_IL_000a:;
				}
				catch (Exception ex)
				{
					Log.LogError("[LoginManager]", "静默登录异常: {0}", new object[1] { ex.Message });
					val2 = Fail((AccountErrorCode)9999, ex.Message);
				}
				finally
				{
					if (num < 0)
					{
						silentLoginInFlight = false;
					}
				}
				loginCompletionSource?.TrySetResult(val2);
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
	private struct _003CLogoutAsync_003Ed__45 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<LoginResult> _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LoginResult result;
			try
			{
				if (num == 0)
				{
					goto IL_00ae;
				}
				Init();
				AccountComponent account = default(AccountComponent);
				if (!NetUtil.IsHaveNet)
				{
					result = Fail((AccountErrorCode)2001, "无网络");
				}
				else if (logoutInFlight)
				{
					result = Fail((AccountErrorCode)1003, "登出进行中");
				}
				else
				{
					account = GameApp.Account;
					if ((Object)(object)account == (Object)null || !account.IsInitialized)
					{
						result = Fail((AccountErrorCode)1001, "账号组件未初始化");
					}
					else
					{
						if ((int)account.CurrentIdentity != 0)
						{
							logoutInFlight = true;
							accountStatusBeforeOp = RRDataTrack.GetAccountStatus();
							pendingLogoutProvider = RRDataTrack.GetCurrentProvider();
							goto IL_00ae;
						}
						result = Fail((AccountErrorCode)1003, "Guest 不可登出");
					}
				}
				goto end_IL_0007;
				IL_00ae:
				try
				{
					Awaiter<LoginResult> val;
					if (num != 0)
					{
						val = account.UnbindThirdPartyAsync(cancellationToken).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CLogoutAsync_003Ed__45>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
					}
					LoginResult result2 = val.GetResult();
					if (result2 != null && result2.IsSuccess)
					{
						CloudSaveFlowService.SetPendingForceCloudUpload(value: true, "Logout:backToGuest");
					}
					else
					{
						MapLoginResult(result2, out var trackResult, out var failReason);
						RRDataTrack.ReportSettingAccountRequestResult("logout", trackResult, accountStatusBeforeOp, RRDataTrack.GetAccountStatus(), pendingLogoutProvider, failReason);
					}
					result = result2;
				}
				finally
				{
					if (num < 0)
					{
						logoutInFlight = false;
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
	private struct _003CPullCloudSnapshotForLoginAsync_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveSnapshotPullResult> _003C_003Et__builder;

		private Awaiter<CloudSaveSnapshotPullResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveSnapshotPullResult result;
			try
			{
				Awaiter<CloudSaveSnapshotPullResult> val;
				if (num != 0)
				{
					val = WithLoginCloudSaveLoadingAsync(CloudSaveFlowService.PullCloudSnapshotAsync).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveSnapshotPullResult>, _003CPullCloudSnapshotForLoginAsync_003Ed__41>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveSnapshotPullResult>);
					num = (_003C_003E1__state = -1);
				}
				result = val.GetResult();
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
	private struct _003CPullCloudSnapshotToLocalForLoginAsync_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveDataSnapshot> _003C_003Et__builder;

		private Awaiter<CloudSaveDataSnapshot> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveDataSnapshot result;
			try
			{
				Awaiter<CloudSaveDataSnapshot> val;
				if (num != 0)
				{
					val = WithLoginCloudSaveLoadingAsync(CloudSaveFlowService.PullCloudSnapshotToLocalAsync).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveDataSnapshot>, _003CPullCloudSnapshotToLocalForLoginAsync_003Ed__42>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveDataSnapshot>);
					num = (_003C_003E1__state = -1);
				}
				result = val.GetResult();
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
	private struct _003CPushLocalSnapshotToCloudForLoginAsync_003Ed__43 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<CloudSaveOperationResult> _003C_003Et__builder;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			CloudSaveOperationResult result;
			try
			{
				Awaiter<CloudSaveOperationResult> val;
				if (num != 0)
				{
					val = WithLoginCloudSaveLoadingAsync(CloudSaveFlowService.PushLocalSnapshotToCloudAsync).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CPushLocalSnapshotToCloudForLoginAsync_003Ed__43>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
					num = (_003C_003E1__state = -1);
				}
				result = val.GetResult();
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
	private struct _003CUnregisterThirdPartyAsync_003Ed__46 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<UnregisterResult> _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private AccountComponent _003Caccount_003E5__2;

		private UnregisterResult _003Cresult_003E5__3;

		private Awaiter<UnregisterResult> _003C_003Eu__1;

		private Awaiter<LoginResult> _003C_003Eu__2;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__3;

		private void MoveNext()
		{
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UnregisterResult result;
			try
			{
				if ((uint)num <= 2u)
				{
					goto IL_00d1;
				}
				Init();
				if (!IsFeatureEnabled)
				{
					result = FailUnregister((AccountErrorCode)1003, "账号功能未开启");
				}
				else if (!NetUtil.IsHaveNet)
				{
					result = FailUnregister((AccountErrorCode)2001, "无网络");
				}
				else if (unregisterInFlight)
				{
					result = FailUnregister((AccountErrorCode)1003, "删号进行中");
				}
				else
				{
					_003Caccount_003E5__2 = GameApp.Account;
					if (!((Object)(object)_003Caccount_003E5__2 == (Object)null) && _003Caccount_003E5__2.IsInitialized)
					{
						unregisterInFlight = true;
						accountStatusBeforeOp = RRDataTrack.GetAccountStatus();
						pendingUnregisterProvider = RRDataTrack.GetCurrentProvider();
						pendingDeleteType = "server_data";
						pendingLocalDataAction = "keep";
						goto IL_00d1;
					}
					result = FailUnregister((AccountErrorCode)1001, "账号组件未初始化");
				}
				goto end_IL_0007;
				IL_00d1:
				try
				{
					Awaiter<UnregisterResult> val3;
					Awaiter<LoginResult> val2;
					Awaiter<CloudSaveOperationResult> val;
					UnregisterResult result2;
					CloudSaveOperationResult result3;
					switch (num)
					{
					default:
						val3 = _003Caccount_003E5__2.UnregisterThirdPartyAsync(cancellationToken).GetAwaiter();
						if (!val3.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val3;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<UnregisterResult>, _003CUnregisterThirdPartyAsync_003Ed__46>(ref val3, ref this);
							return;
						}
						goto IL_0146;
					case 0:
						val3 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<UnregisterResult>);
						num = (_003C_003E1__state = -1);
						goto IL_0146;
					case 1:
						val2 = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
						goto IL_01f6;
					case 2:
						{
							val = _003C_003Eu__3;
							_003C_003Eu__3 = default(Awaiter<CloudSaveOperationResult>);
							num = (_003C_003E1__state = -1);
							goto IL_026a;
						}
						IL_01fe:
						if (_003Caccount_003E5__2.IsLoggedIn)
						{
							val = CloudSaveFlowService.ForceUploadLocalSnapshotAsync("删号后强制上传本地").GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 2);
								_003C_003Eu__3 = val;
								_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CUnregisterThirdPartyAsync_003Ed__46>(ref val, ref this);
								return;
							}
							goto IL_026a;
						}
						CloudSaveFlowService.SetPendingForceCloudUpload(value: true, "Unregister:guestNotLoggedIn");
						break;
						IL_0146:
						result2 = val3.GetResult();
						_003Cresult_003E5__3 = result2;
						if (_003Cresult_003E5__3 != null && _003Cresult_003E5__3.IsSuccess)
						{
							if ((int)_003Caccount_003E5__2.CurrentIdentity == 0 && _003Caccount_003E5__2.Guid == 0L)
							{
								val2 = _003Caccount_003E5__2.LoginAsync(cancellationToken).GetAwaiter();
								if (!val2.IsCompleted)
								{
									num = (_003C_003E1__state = 1);
									_003C_003Eu__2 = val2;
									_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CUnregisterThirdPartyAsync_003Ed__46>(ref val2, ref this);
									return;
								}
								goto IL_01f6;
							}
							goto IL_01fe;
						}
						result = _003Cresult_003E5__3;
						goto end_IL_00d1;
						IL_026a:
						result3 = val.GetResult();
						if (result3 != null && result3.IsSuccess)
						{
						}
						break;
						IL_01f6:
						val2.GetResult();
						goto IL_01fe;
					}
					result = _003Cresult_003E5__3;
					end_IL_00d1:;
				}
				finally
				{
					if (num < 0)
					{
						unregisterInFlight = false;
					}
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Caccount_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Caccount_003E5__2 = null;
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
	private struct _003CWithLoginCloudSaveLoadingAsync_003Ed__44<T> : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<T> _003C_003Et__builder;

		public Func<UniTask<T>> action;

		private Awaiter<T> _003C_003Eu__1;

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
			T result;
			try
			{
				if (num != 0)
				{
					PurchaseService.Show("LoginCloudSave");
				}
				try
				{
					Awaiter<T> val;
					if (num != 0)
					{
						val = action().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<T>, _003CWithLoginCloudSaveLoadingAsync_003Ed__44<T>>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<T>);
						num = (_003C_003E1__state = -1);
					}
					result = val.GetResult();
				}
				finally
				{
					if (num < 0)
					{
						PurchaseService.Hide("LoginCloudSave");
					}
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

	private const string LogTag = "[LoginManager]";

	private const int LoginTimeoutSec = 3;

	private const int LoginMaxRetries = 2;

	private const string LoginCloudSaveLoadingSource = "LoginCloudSave";

	private static bool accountEventsHooked;

	private static bool silentLoginInFlight;

	private static bool thirdPartyLoginInFlight;

	private static bool logoutInFlight;

	private static bool unregisterInFlight;

	private static bool accountNotFoundHandling;

	private static bool loginStarted;

	private static bool bindGuestLoginPrepInFlight;

	private static ThirdPartyType? userInitiatedThirdPartyType;

	private static string accountStatusBeforeOp = "guest_offline";

	private static string pendingDeleteType;

	private static string pendingLocalDataAction;

	private static string pendingUnregisterProvider;

	private static string pendingLogoutProvider;

	private static bool loginOutcomeTrackReported;

	private static string pendingBindTrackStatusBefore;

	private static string pendingBindTrackProvider;

	private static ThirdPartyType? pendingBindTrackThirdPartyType;

	private static UniTaskCompletionSource<LoginResult> loginCompletionSource;

	public static bool IsFeatureEnabled => CloudSaveFlowService.IsCloudSaveEnabled();

	public static bool IsThirdPartyLoginInFlight => userInitiatedThirdPartyType.HasValue;

	public static bool IsLoginInFlight => silentLoginInFlight;

	public static bool IsLogoutInFlight => logoutInFlight;

	public static bool IsUnregisterInFlight => unregisterInFlight;

	public static void ReportBindAssociationCompleted()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (pendingBindTrackThirdPartyType.HasValue)
		{
			ThirdPartyType value = pendingBindTrackThirdPartyType.Value;
			string statusBefore = pendingBindTrackStatusBefore ?? RRDataTrack.GetAccountStatus();
			string provider = pendingBindTrackProvider ?? RRDataTrack.GetProviderFromThirdPartyType(value);
			ClearPendingBindAssociationTrack();
			bool isFirstBind = AccountFirstBindStore.ConsumeFirstBindFlag();
			RRDataTrack.ReportSettingAccountRequestResult("bind", "success", statusBefore, RRDataTrack.GetAccountStatus(), provider, null, isFirstBind);
		}
	}

	public static void ReportBindAssociationCancelled()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (pendingBindTrackThirdPartyType.HasValue)
		{
			ThirdPartyType value = pendingBindTrackThirdPartyType.Value;
			string statusBefore = pendingBindTrackStatusBefore ?? RRDataTrack.GetAccountStatus();
			string provider = pendingBindTrackProvider ?? RRDataTrack.GetProviderFromThirdPartyType(value);
			ClearPendingBindAssociationTrack();
			RRDataTrack.ReportSettingAccountRequestResult("bind", "cancel", statusBefore, RRDataTrack.GetAccountStatus(), provider, "sdk_cancel");
		}
	}

	public static void ClearPendingBindAssociationTrack()
	{
		pendingBindTrackThirdPartyType = null;
		pendingBindTrackStatusBefore = null;
		pendingBindTrackProvider = null;
	}

	public static void Init()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!IsFeatureEnabled)
		{
			return;
		}
		EnsureAccountComponent();
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account == (Object)null)
		{
			Log.LogError("[LoginManager]", "Init: GameApp.Account 为空");
			return;
		}
		if (!account.IsInitialized)
		{
			account.Initialize(MapBuildEnvironment());
			account.TimeoutSec = 3;
			account.MaxRetries = 2;
		}
		HookAccountEvents();
		RRDataTrack.InitAccountUserPropertySync();
	}

	public static void EnsureAccountComponent()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (!((Object)(object)AccountComponent.Instance != (Object)null))
		{
			GameObject val = new GameObject("[XingYun.Account]");
			Object.DontDestroyOnLoad((Object)val);
			val.AddComponent<AccountComponent>();
		}
	}

	public static void Login()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		Init();
		if (!IsFeatureEnabled || silentLoginInFlight || IsThirdPartyLoginInFlight)
		{
			return;
		}
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account != (Object)null && account.IsLoggedIn)
		{
			if (!loginStarted)
			{
				loginStarted = true;
				loginCompletionSource = new UniTaskCompletionSource<LoginResult>();
			}
			if (loginCompletionSource != null && !UniTaskStatusExtensions.IsCompleted(loginCompletionSource.Task.Status))
			{
				loginCompletionSource.TrySetResult(new LoginResult
				{
					Code = (AccountErrorCode)0,
					Guid = account.Guid,
					Auid = account.Auid,
					Resumed = true,
					Msg = "本地已登录，跳过 SilentLogin"
				});
			}
			TryReportLoginOutcomeOnce(RRDataTrack.GetAccountStatus(), RRDataTrack.GetAccountStatus());
		}
		else
		{
			if (!loginStarted)
			{
				loginStarted = true;
				loginCompletionSource = new UniTaskCompletionSource<LoginResult>();
			}
			silentLoginInFlight = true;
			accountStatusBeforeOp = RRDataTrack.GetAccountStatus();
			UniTaskVoid val = DoSilentLoginAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	public static UniTask<LoginResult> WaitForLoginAsync()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!IsFeatureEnabled)
		{
			return UniTask.FromResult<LoginResult>(new LoginResult
			{
				Code = (AccountErrorCode)1001,
				Msg = "账号功能未开启"
			});
		}
		if (loginCompletionSource != null)
		{
			return loginCompletionSource.Task;
		}
		return UniTask.FromResult<LoginResult>(new LoginResult
		{
			Code = (AccountErrorCode)1001,
			Msg = "账号尚未开始登录"
		});
	}

	[AsyncStateMachine(typeof(_003CBindThirdPartyAsync_003Ed__40))]
	public static UniTask<LoginResult> BindThirdPartyAsync(ThirdPartyType type, CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CBindThirdPartyAsync_003Ed__40 _003CBindThirdPartyAsync_003Ed__ = default(_003CBindThirdPartyAsync_003Ed__40);
		_003CBindThirdPartyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<LoginResult>.Create();
		_003CBindThirdPartyAsync_003Ed__.type = type;
		_003CBindThirdPartyAsync_003Ed__.cancellationToken = cancellationToken;
		_003CBindThirdPartyAsync_003Ed__._003C_003E1__state = -1;
		_003CBindThirdPartyAsync_003Ed__._003C_003Et__builder.Start<_003CBindThirdPartyAsync_003Ed__40>(ref _003CBindThirdPartyAsync_003Ed__);
		return _003CBindThirdPartyAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CPullCloudSnapshotForLoginAsync_003Ed__41))]
	public static UniTask<CloudSaveSnapshotPullResult> PullCloudSnapshotForLoginAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPullCloudSnapshotForLoginAsync_003Ed__41 _003CPullCloudSnapshotForLoginAsync_003Ed__ = default(_003CPullCloudSnapshotForLoginAsync_003Ed__41);
		_003CPullCloudSnapshotForLoginAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveSnapshotPullResult>.Create();
		_003CPullCloudSnapshotForLoginAsync_003Ed__._003C_003E1__state = -1;
		_003CPullCloudSnapshotForLoginAsync_003Ed__._003C_003Et__builder.Start<_003CPullCloudSnapshotForLoginAsync_003Ed__41>(ref _003CPullCloudSnapshotForLoginAsync_003Ed__);
		return _003CPullCloudSnapshotForLoginAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__42))]
	public static UniTask<CloudSaveDataSnapshot> PullCloudSnapshotToLocalForLoginAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__42 _003CPullCloudSnapshotToLocalForLoginAsync_003Ed__ = default(_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__42);
		_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveDataSnapshot>.Create();
		_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__._003C_003E1__state = -1;
		_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__._003C_003Et__builder.Start<_003CPullCloudSnapshotToLocalForLoginAsync_003Ed__42>(ref _003CPullCloudSnapshotToLocalForLoginAsync_003Ed__);
		return _003CPullCloudSnapshotToLocalForLoginAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__43))]
	public static UniTask<CloudSaveOperationResult> PushLocalSnapshotToCloudForLoginAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__43 _003CPushLocalSnapshotToCloudForLoginAsync_003Ed__ = default(_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__43);
		_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<CloudSaveOperationResult>.Create();
		_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__._003C_003E1__state = -1;
		_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__._003C_003Et__builder.Start<_003CPushLocalSnapshotToCloudForLoginAsync_003Ed__43>(ref _003CPushLocalSnapshotToCloudForLoginAsync_003Ed__);
		return _003CPushLocalSnapshotToCloudForLoginAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CWithLoginCloudSaveLoadingAsync_003Ed__44<>))]
	private unsafe static UniTask<T> WithLoginCloudSaveLoadingAsync<T>(Func<UniTask<T>> action)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CWithLoginCloudSaveLoadingAsync_003Ed__44<T> _003CWithLoginCloudSaveLoadingAsync_003Ed__ = default(_003CWithLoginCloudSaveLoadingAsync_003Ed__44<T>);
		_003CWithLoginCloudSaveLoadingAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<T>.Create();
		_003CWithLoginCloudSaveLoadingAsync_003Ed__.action = action;
		_003CWithLoginCloudSaveLoadingAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder<_003CWithLoginCloudSaveLoadingAsync_003Ed__44<T>>*)(&_003CWithLoginCloudSaveLoadingAsync_003Ed__._003C_003Et__builder))->Start<_003CWithLoginCloudSaveLoadingAsync_003Ed__44<T>>(ref _003CWithLoginCloudSaveLoadingAsync_003Ed__);
		return _003CWithLoginCloudSaveLoadingAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CLogoutAsync_003Ed__45))]
	public static UniTask<LoginResult> LogoutAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLogoutAsync_003Ed__45 _003CLogoutAsync_003Ed__ = default(_003CLogoutAsync_003Ed__45);
		_003CLogoutAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<LoginResult>.Create();
		_003CLogoutAsync_003Ed__.cancellationToken = cancellationToken;
		_003CLogoutAsync_003Ed__._003C_003E1__state = -1;
		_003CLogoutAsync_003Ed__._003C_003Et__builder.Start<_003CLogoutAsync_003Ed__45>(ref _003CLogoutAsync_003Ed__);
		return _003CLogoutAsync_003Ed__._003C_003Et__builder.Task;
	}

	[AsyncStateMachine(typeof(_003CUnregisterThirdPartyAsync_003Ed__46))]
	public static UniTask<UnregisterResult> UnregisterThirdPartyAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CUnregisterThirdPartyAsync_003Ed__46 _003CUnregisterThirdPartyAsync_003Ed__ = default(_003CUnregisterThirdPartyAsync_003Ed__46);
		_003CUnregisterThirdPartyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<UnregisterResult>.Create();
		_003CUnregisterThirdPartyAsync_003Ed__.cancellationToken = cancellationToken;
		_003CUnregisterThirdPartyAsync_003Ed__._003C_003E1__state = -1;
		_003CUnregisterThirdPartyAsync_003Ed__._003C_003Et__builder.Start<_003CUnregisterThirdPartyAsync_003Ed__46>(ref _003CUnregisterThirdPartyAsync_003Ed__);
		return _003CUnregisterThirdPartyAsync_003Ed__._003C_003Et__builder.Task;
	}

	public static void NotifyAccountNotFound(string reason)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)GameApp.Account == (Object)null) && (int)GameApp.Account.CurrentIdentity != 0 && !accountNotFoundHandling)
		{
			accountNotFoundHandling = true;
			UniTaskVoid val = DoForceLogoutToGuestAsync(reason);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	public static void NotifyAccountNotFoundIfNeeded(CloudStorageErrorCode code, string reason)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)code == 4001)
		{
			NotifyAccountNotFound(reason);
		}
	}

	[AsyncStateMachine(typeof(_003CDoSilentLoginAsync_003Ed__49))]
	private static UniTaskVoid DoSilentLoginAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CDoSilentLoginAsync_003Ed__49 _003CDoSilentLoginAsync_003Ed__ = default(_003CDoSilentLoginAsync_003Ed__49);
		_003CDoSilentLoginAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDoSilentLoginAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDoSilentLoginAsync_003Ed__._003C_003Et__builder)).Start<_003CDoSilentLoginAsync_003Ed__49>(ref _003CDoSilentLoginAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDoSilentLoginAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CDoForceLogoutToGuestAsync_003Ed__50))]
	private static UniTaskVoid DoForceLogoutToGuestAsync(string reason)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_003CDoForceLogoutToGuestAsync_003Ed__50 _003CDoForceLogoutToGuestAsync_003Ed__ = default(_003CDoForceLogoutToGuestAsync_003Ed__50);
		_003CDoForceLogoutToGuestAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDoForceLogoutToGuestAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDoForceLogoutToGuestAsync_003Ed__._003C_003Et__builder)).Start<_003CDoForceLogoutToGuestAsync_003Ed__50>(ref _003CDoForceLogoutToGuestAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDoForceLogoutToGuestAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void HookAccountEvents()
	{
		if (!accountEventsHooked)
		{
			AccountComponent account = GameApp.Account;
			if (!((Object)(object)account == (Object)null))
			{
				accountEventsHooked = true;
				account.OnLoginSuccess += OnLoginSuccess;
				account.OnLoginFailed += OnLoginFailed;
				account.OnSessionKicked += OnSessionKicked;
				account.OnUnregisterSuccess += OnUnregisterSuccess;
				account.OnUnregisterFailed += OnUnregisterFailed;
			}
		}
	}

	private static void OnLoginSuccess(LoginResult result)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (result != null && result.Resumed)
		{
			string accountStatus = RRDataTrack.GetAccountStatus();
			TryReportLoginOutcomeOnce(accountStatus, accountStatus);
			return;
		}
		bool hasValue = userInitiatedThirdPartyType.HasValue;
		bool flag = logoutInFlight;
		if (hasValue && userInitiatedThirdPartyType.HasValue)
		{
			pendingBindTrackStatusBefore = accountStatusBeforeOp;
			pendingBindTrackThirdPartyType = userInitiatedThirdPartyType.Value;
			pendingBindTrackProvider = RRDataTrack.GetProviderFromThirdPartyType(userInitiatedThirdPartyType.Value);
		}
		else
		{
			bool isFirstBind = false;
			string text = (flag ? "logout" : ((!unregisterInFlight) ? "login" : "unbind"));
			RRDataTrack.ReportSettingAccountRequestResult(provider: flag ? (pendingLogoutProvider ?? RRDataTrack.GetCurrentProvider()) : ((!unregisterInFlight) ? RRDataTrack.GetCurrentProvider() : (pendingUnregisterProvider ?? RRDataTrack.GetCurrentProvider())), action: text, result: "success", statusBefore: accountStatusBeforeOp, statusAfter: RRDataTrack.GetAccountStatus(), failReason: null, isFirstBind: isFirstBind);
			if (text == "login")
			{
				loginOutcomeTrackReported = true;
			}
			if (text == "logout")
			{
				loginOutcomeTrackReported = false;
			}
		}
		if (!bindGuestLoginPrepInFlight || hasValue)
		{
			EventLoginSucceed.Send(hasValue, flag);
		}
	}

	private static void OnLoginFailed(LoginResult result)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (result != null && (int)result.Code == 1003 && !string.IsNullOrEmpty(result.Msg) && (result.Msg.Contains("请勿并发调用") || result.Msg.Contains("请勿并发 UnbindThirdParty")) && !result.Msg.Contains("BindThirdParty"))
		{
			return;
		}
		bool hasValue = userInitiatedThirdPartyType.HasValue;
		if (!logoutInFlight)
		{
			string action;
			if (!hasValue)
			{
				action = ((!unregisterInFlight) ? "login" : "unbind");
			}
			else
			{
				action = "bind";
				ClearPendingBindAssociationTrack();
			}
			MapLoginResult(result, out var trackResult, out var failReason);
			RRDataTrack.ReportSettingAccountRequestResult(provider: unregisterInFlight ? pendingUnregisterProvider : ((!hasValue || !userInitiatedThirdPartyType.HasValue) ? null : RRDataTrack.GetProviderFromThirdPartyType(userInitiatedThirdPartyType.Value)), action: action, result: trackResult, statusBefore: accountStatusBeforeOp, statusAfter: RRDataTrack.GetAccountStatus(), failReason: failReason);
		}
	}

	private static void OnSessionKicked(SessionKickedContext ctx)
	{
	}

	private static void OnUnregisterSuccess(ulong oldGuid)
	{
		RRDataTrack.ReportSettingAccountRequestResult("delete_account", "success", accountStatusBeforeOp, RRDataTrack.GetAccountStatus(), pendingUnregisterProvider, null, isFirstBind: false, pendingDeleteType, pendingLocalDataAction);
	}

	private static void OnUnregisterFailed(UnregisterResult result)
	{
		MapUnregisterResult(result, out var trackResult, out var failReason);
		RRDataTrack.ReportSettingAccountRequestResult("delete_account", trackResult, accountStatusBeforeOp, RRDataTrack.GetAccountStatus(), pendingUnregisterProvider, failReason, isFirstBind: false, pendingDeleteType, pendingLocalDataAction);
	}

	private static BuildEnvironment MapBuildEnvironment()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Define.BuildEnvironment == 0)
		{
			return (BuildEnvironment)0;
		}
		return (BuildEnvironment)1;
	}

	private static void TryReportLoginOutcomeOnce(string statusBefore, string statusAfter)
	{
		if (!loginOutcomeTrackReported)
		{
			loginOutcomeTrackReported = true;
			RRDataTrack.ReportSettingAccountRequestResult("login", "success", statusBefore, statusAfter, RRDataTrack.GetCurrentProvider());
		}
	}

	private static LoginResult FailBindAndTrack(ThirdPartyType type, AccountErrorCode code, string msg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		LoginResult result = Fail(code, msg);
		MapLoginResult(result, out var trackResult, out var failReason);
		string statusBefore = (string.IsNullOrEmpty(accountStatusBeforeOp) ? RRDataTrack.GetAccountStatus() : accountStatusBeforeOp);
		ClearPendingBindAssociationTrack();
		RRDataTrack.ReportSettingAccountRequestResult("bind", trackResult, statusBefore, RRDataTrack.GetAccountStatus(), RRDataTrack.GetProviderFromThirdPartyType(type), failReason);
		return result;
	}

	private static LoginResult Fail(AccountErrorCode code, string msg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		return new LoginResult
		{
			Code = code,
			Msg = msg
		};
	}

	private static UnregisterResult FailUnregister(AccountErrorCode code, string msg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		return new UnregisterResult
		{
			Code = code,
			Msg = msg
		};
	}

	private static void MapLoginResult(LoginResult result, out string trackResult, out string failReason)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (result != null && result.IsSuccess)
		{
			trackResult = "success";
			failReason = null;
		}
		else
		{
			MapAccountError((AccountErrorCode)((result == null) ? 9999 : ((int)result.Code)), result?.HttpStatus ?? 0, result?.Msg, out trackResult, out failReason);
		}
	}

	private static void MapUnregisterResult(UnregisterResult result, out string trackResult, out string failReason)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (result != null && result.IsSuccess)
		{
			trackResult = "success";
			failReason = null;
		}
		else
		{
			MapAccountError((AccountErrorCode)((result == null) ? 9999 : ((int)result.Code)), result?.HttpStatus ?? 0, result?.Msg, out trackResult, out failReason);
		}
	}

	private static void MapAccountError(AccountErrorCode code, long httpStatus, string msg, out string result, out string failReason)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected I4, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Invalid comparison between Unknown and I4
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		if (httpStatus == 401)
		{
			result = "forbidden";
			failReason = "server_error";
			return;
		}
		if ((int)code == 1003 && !string.IsNullOrEmpty(msg) && (msg.Contains("苹果登录失败 code=") || msg.Contains("Google 登录失败 code=")))
		{
			result = "forbidden";
			failReason = "sdk_password_error";
			return;
		}
		switch (code - 2001)
		{
		default:
			if ((int)code != 4999)
			{
				if ((int)code == 9001)
				{
					result = "cancel";
					failReason = "sdk_cancel";
				}
				else
				{
					result = "forbidden";
					failReason = "unknown";
				}
				break;
			}
			goto case 2;
		case 0:
			result = "no_network";
			failReason = "network_fail";
			break;
		case 1:
			result = "timeout";
			failReason = "network_fail";
			break;
		case 2:
			result = "forbidden";
			failReason = "server_error";
			break;
		}
	}
}
