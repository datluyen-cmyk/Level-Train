using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using Game.UI;
using Game.UI.Language;
using RotateRings.Common;
using RotateRings.Services;
using RotateRings.Services.Account;
using RotateRings.Tool;
using RotateRings.UI.HowToPlay;
using RotateRings.UI.User;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Account;
using XingYun.Core.Singletons;
using XingYun.GM;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UISettingHomeV2")]
public class UISettingHomeV2Controller : GameBaseController<UISettingHomeV2View>
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass25_0
	{
		public UISettingHomeV2Controller _003C_003E4__this;

		public ThirdPartyType thirdPartyType;

		internal void _003COpenSaveSelectionAsync_003Eb__0()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.RetryCloudSync(thirdPartyType);
		}

		internal void _003COpenSaveSelectionAsync_003Eb__1()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.CancelPendingAccountBinding(thirdPartyType);
		}

		internal void _003COpenSaveSelectionAsync_003Eb__2()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.CancelPendingAccountBindingFromSelectAccountUi(thirdPartyType);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass26_0
	{
		public UISettingHomeV2Controller _003C_003E4__this;

		public ThirdPartyType thirdPartyType;

		internal void _003CSyncLocalSnapshotToNewCloudAsync_003Eb__0()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.RetryCloudSync(thirdPartyType);
		}

		internal void _003CSyncLocalSnapshotToNewCloudAsync_003Eb__1()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.CancelPendingAccountBinding(thirdPartyType);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass34_0
	{
		public UISettingHomeV2Controller _003C_003E4__this;

		public ThirdPartyType thirdPartyType;

		internal void _003CBindAccountAsync_003Eb__0()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.RetryAccountBinding(thirdPartyType);
		}

		internal void _003CBindAccountAsync_003Eb__1()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_003C_003E4__this.RetryAccountBinding(thirdPartyType);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBindAccountAsync_003Ed__34 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UISettingHomeV2Controller _003C_003E4__this;

		public ThirdPartyType thirdPartyType;

		private _003C_003Ec__DisplayClass34_0 _003C_003E8__1;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UISettingHomeV2Controller uISettingHomeV2Controller = _003C_003E4__this;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_007d;
				}
				_003C_003E8__1 = new _003C_003Ec__DisplayClass34_0();
				_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
				_003C_003E8__1.thirdPartyType = thirdPartyType;
				if (NetUtil.IsHaveNet)
				{
					LoginManager.Init();
					CloudSaveFlowService.EnsureInitialized();
					uISettingHomeV2Controller.isAccountActionInProgress = true;
					uISettingHomeV2Controller.SetAccountButtonsInteractable(interactable: false);
					goto IL_007d;
				}
				uISettingHomeV2Controller.OpenLinkResultWindow(isSuccess: false, delegate
				{
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					_003C_003E8__1._003C_003E4__this.RetryAccountBinding(_003C_003E8__1.thirdPartyType);
				});
				goto end_IL_000e;
				IL_007d:
				try
				{
					Awaiter val;
					Awaiter<LoginResult> val2;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter);
							num = (_003C_003E1__state = -1);
							goto IL_0181;
						}
						val2 = LoginManager.BindThirdPartyAsync(_003C_003E8__1.thirdPartyType).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CBindAccountAsync_003Ed__34>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
					}
					LoginResult result = val2.GetResult();
					bool flag = result != null && result.IsSuccess;
					if (!uISettingHomeV2Controller.isRemoved)
					{
						if (flag)
						{
							UniTask val3 = uISettingHomeV2Controller.OpenSaveSelectionAsync(_003C_003E8__1.thirdPartyType);
							val = ((UniTask)(ref val3)).GetAwaiter();
							if (!((Awaiter)(ref val)).IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__2 = val;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CBindAccountAsync_003Ed__34>(ref val, ref this);
								return;
							}
							goto IL_0181;
						}
						uISettingHomeV2Controller.OpenLinkResultWindow(isSuccess: false, delegate
						{
							//IL_0007: Unknown result type (might be due to invalid IL or missing references)
							_003C_003E8__1._003C_003E4__this.RetryAccountBinding(_003C_003E8__1.thirdPartyType);
						});
					}
					else if (flag)
					{
						CloudSaveFlowService.SuspendBindSelection();
					}
					goto end_IL_007d;
					IL_0181:
					((Awaiter)(ref val)).GetResult();
					end_IL_007d:;
				}
				finally
				{
					if (num < 0)
					{
						uISettingHomeV2Controller.isAccountActionInProgress = false;
						if (!uISettingHomeV2Controller.isRemoved)
						{
							uISettingHomeV2Controller.SetAccountButtonsInteractable(interactable: true);
							uISettingHomeV2Controller.RefreshAccountBindStatus();
						}
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
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
	private struct _003CCancelPendingAccountBindingAsync_003Ed__32 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public ThirdPartyType thirdPartyType;

		public UISettingHomeV2Controller _003C_003E4__this;

		private AccountComponent _003Caccount_003E5__2;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UISettingHomeV2Controller uISettingHomeV2Controller = _003C_003E4__this;
			try
			{
				Awaiter<LoginResult> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<LoginResult>);
					num = (_003C_003E1__state = -1);
					goto IL_00c8;
				}
				LoginManager.ClearPendingBindAssociationTrack();
				_003Caccount_003E5__2 = GameApp.Account;
				if (!((Object)(object)_003Caccount_003E5__2 == (Object)null) && _003Caccount_003E5__2.IsInitialized && _003Caccount_003E5__2.IsBound(thirdPartyType))
				{
					val = LoginManager.LogoutAsync().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CCancelPendingAccountBindingAsync_003Ed__32>(ref val, ref this);
						return;
					}
					goto IL_00c8;
				}
				CloudSaveFlowService.CompleteBindSelection("CancelBinding:alreadyUnbound");
				uISettingHomeV2Controller.RefreshAccountBindStatus();
				goto end_IL_000e;
				IL_00c8:
				LoginResult result = val.GetResult();
				if (result != null && result.IsSuccess && !_003Caccount_003E5__2.IsBound(thirdPartyType))
				{
					CloudSaveFlowService.CompleteBindSelection("CancelBinding:unbound");
				}
				else
				{
					CloudSaveFlowService.SuspendBindSelection();
				}
				uISettingHomeV2Controller.RefreshAccountBindStatus();
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Caccount_003E5__2 = null;
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Caccount_003E5__2 = null;
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
	private struct _003COpenSaveSelectionAsync_003Ed__25 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UISettingHomeV2Controller _003C_003E4__this;

		public ThirdPartyType thirdPartyType;

		private _003C_003Ec__DisplayClass25_0 _003C_003E8__1;

		private Awaiter<CloudSaveSnapshotPullResult> _003C_003Eu__1;

		private Awaiter _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UISettingHomeV2Controller uISettingHomeV2Controller = _003C_003E4__this;
			try
			{
				Awaiter val;
				Awaiter<CloudSaveSnapshotPullResult> val2;
				if (num != 0)
				{
					if (num == 1)
					{
						val = _003C_003Eu__2;
						_003C_003Eu__2 = default(Awaiter);
						num = (_003C_003E1__state = -1);
						goto IL_015f;
					}
					_003C_003E8__1 = new _003C_003Ec__DisplayClass25_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					_003C_003E8__1.thirdPartyType = thirdPartyType;
					val2 = LoginManager.PullCloudSnapshotForLoginAsync().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveSnapshotPullResult>, _003COpenSaveSelectionAsync_003Ed__25>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveSnapshotPullResult>);
					num = (_003C_003E1__state = -1);
				}
				CloudSaveSnapshotPullResult result = val2.GetResult();
				CloudSaveDataSnapshot snapshot = result.Snapshot;
				if (uISettingHomeV2Controller.isRemoved)
				{
					CloudSaveFlowService.SuspendBindSelection();
				}
				else if (!result.IsSuccess)
				{
					uISettingHomeV2Controller.OpenLinkResultWindow(isSuccess: false, delegate
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						_003C_003E8__1._003C_003E4__this.RetryCloudSync(_003C_003E8__1.thirdPartyType);
					}, delegate
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						_003C_003E8__1._003C_003E4__this.CancelPendingAccountBinding(_003C_003E8__1.thirdPartyType);
					});
				}
				else
				{
					if (snapshot == null)
					{
						UniTask val3 = uISettingHomeV2Controller.SyncLocalSnapshotToNewCloudAsync(_003C_003E8__1.thirdPartyType);
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__2 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003COpenSaveSelectionAsync_003Ed__25>(ref val, ref this);
							return;
						}
						goto IL_015f;
					}
					CloudSaveDataSnapshot localSnapshot = CloudSaveDataHandler.CaptureSnapshot();
					GameApp.UI.Open("UILoginSelectAccount", (UILayer)2, (object)new UILoginSelectAccountParam(snapshot, localSnapshot, uISettingHomeV2Controller.CompleteAccountBinding, delegate
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						_003C_003E8__1._003C_003E4__this.CancelPendingAccountBindingFromSelectAccountUi(_003C_003E8__1.thirdPartyType);
					}), (Action)null);
				}
				goto end_IL_000e;
				IL_015f:
				((Awaiter)(ref val)).GetResult();
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CSyncLocalSnapshotToNewCloudAsync_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UISettingHomeV2Controller _003C_003E4__this;

		public ThirdPartyType thirdPartyType;

		private _003C_003Ec__DisplayClass26_0 _003C_003E8__1;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UISettingHomeV2Controller uISettingHomeV2Controller = _003C_003E4__this;
			try
			{
				Awaiter<CloudSaveOperationResult> val;
				if (num != 0)
				{
					_003C_003E8__1 = new _003C_003Ec__DisplayClass26_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					_003C_003E8__1.thirdPartyType = thirdPartyType;
					val = LoginManager.PushLocalSnapshotToCloudForLoginAsync().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CSyncLocalSnapshotToNewCloudAsync_003Ed__26>(ref val, ref this);
						return;
					}
				}
				else
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveOperationResult>);
					num = (_003C_003E1__state = -1);
				}
				bool flag = val.GetResult()?.IsSuccess ?? false;
				if (flag)
				{
					CloudSaveFlowService.CompleteBindSelection("SyncLocalToNewCloud:success");
				}
				if (uISettingHomeV2Controller.isRemoved)
				{
					if (!flag)
					{
						CloudSaveFlowService.SuspendBindSelection();
					}
				}
				else if (flag)
				{
					uISettingHomeV2Controller.CompleteAccountBinding();
					uISettingHomeV2Controller.OpenLinkResultWindow(isSuccess: true);
				}
				else
				{
					uISettingHomeV2Controller.OpenLinkResultWindow(isSuccess: false, delegate
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						_003C_003E8__1._003C_003E4__this.RetryCloudSync(_003C_003E8__1.thirdPartyType);
					}, delegate
					{
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						_003C_003E8__1._003C_003E4__this.CancelPendingAccountBinding(_003C_003E8__1.thirdPartyType);
					});
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003E8__1 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003E8__1 = null;
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

	private const string LogTag = "[UI][UISettingHomeV2Controller]";

	private const float GmClickWindowSeconds = 5f;

	private const int GmClickTriggerCount = 20;

	private float gmFirstClickTime;

	private int gmClickCount;

	private string currentName;

	private UISetting_IconToggleItem musicToggleItem;

	private UISetting_IconToggleItem soundToggleItem;

	private UISetting_IconToggleItem vibrationToggleItem;

	private bool isClosing;

	private bool isRemoved;

	private bool isAccountActionInProgress;

	private PlayerNameEditPresenter namePresenter;

	private Button googlePlayAccountButton;

	private Button iosAccountButton;

	public UISettingHomeV2Controller(UISettingHomeV2View view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected O, but got Unknown
		gmFirstClickTime = 0f;
		gmClickCount = 0;
		isClosing = false;
		isRemoved = false;
		isAccountActionInProgress = false;
		googlePlayAccountButton = ((BaseController<UISettingHomeV2View>)this).view.gp_bind_account;
		iosAccountButton = ((BaseController<UISettingHomeV2View>)this).view.ios_bind_account;
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.BtnClose.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.BtnGM.onClick).AddListener(new UnityAction(OnGMClick));
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.btnHowtoPlay.onClick).AddListener(new UnityAction(OnHowtoplayClick));
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.btnAboutUs.onClick).AddListener(new UnityAction(OnAboutClick));
		Button btnLanguage = ((BaseController<UISettingHomeV2View>)this).view.btnLanguage;
		if (btnLanguage != null)
		{
			((UnityEvent)btnLanguage.onClick).AddListener(new UnityAction(OnLanguageClick));
		}
		Button btnRandom = ((BaseController<UISettingHomeV2View>)this).view.btnRandom;
		if (btnRandom != null)
		{
			((UnityEvent)btnRandom.onClick).AddListener(new UnityAction(OnSettingHomeRandomNameClick));
		}
		Button btnConvert = ((BaseController<UISettingHomeV2View>)this).view.btnConvert;
		if (btnConvert != null)
		{
			((UnityEvent)btnConvert.onClick).AddListener(new UnityAction(OnSettingHomeChangeAvatarClick));
		}
		Button obj = googlePlayAccountButton;
		if (obj != null)
		{
			((UnityEvent)obj.onClick).AddListener(new UnityAction(OnGooglePlayAccountClick));
		}
		Button obj2 = iosAccountButton;
		if (obj2 != null)
		{
			((UnityEvent)obj2.onClick).AddListener(new UnityAction(OnIosAccountClick));
		}
		((BaseController<UISettingHomeV2View>)this).ListenEvent<SettingHomeInit>((Action<SettingHomeInit>)OnSettingHomeInit);
		((BaseController<UISettingHomeV2View>)this).ListenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnUserSaveSuccess);
		((BaseController<UISettingHomeV2View>)this).ListenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		((BaseController<UISettingHomeV2View>)this).ListenEvent<UserAvatarSaveSuccess>((Action<UserAvatarSaveSuccess>)OnUserAvatarSaveSuccess);
		((BaseController<UISettingHomeV2View>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UISettingHomeV2View>)this).ListenEvent<EventLoginSucceed>((Action<EventLoginSucceed>)OnAccountLoginSucceed);
		namePresenter = new PlayerNameEditPresenter();
		namePresenter.Bind(((BaseController<UISettingHomeV2View>)this).view.InputField, ((BaseController<UISettingHomeV2View>)this).view.btnRandom, ((BaseController<UISettingHomeV2View>)this).view.btnEdit, ((BaseController<UISettingHomeV2View>)this).view.btnConvert, SaveNameFromPresenter, ReportSettingHomeEditNickname);
		currentName = PlayerService.GetName();
		((BaseController<UISettingHomeV2View>)this).ExecuteCommand<SettingHomeInitCommand>((object)null);
	}

	private void OnSettingHomeInit(SettingHomeInit arg)
	{
		currentName = arg.PlayerName;
		((BaseController<UISettingHomeV2View>)this).view.SetHeadInfoVariant(UserSettingService.GetHeadInfoType());
		((BaseController<UISettingHomeV2View>)this).view.Init(arg);
		RefreshAccountBindEntryVisibility();
		RefreshAccountBindStatus();
		if (musicToggleItem == null)
		{
			musicToggleItem = new UISetting_IconToggleItem(((BaseController<UISettingHomeV2View>)this).view.btn_Music, ((BaseController<UISettingHomeV2View>)this).view.SetMusicToggleVisual);
		}
		if (soundToggleItem == null)
		{
			soundToggleItem = new UISetting_IconToggleItem(((BaseController<UISettingHomeV2View>)this).view.btn_Sound, ((BaseController<UISettingHomeV2View>)this).view.SetSoundToggleVisual);
		}
		if (vibrationToggleItem == null)
		{
			vibrationToggleItem = new UISetting_IconToggleItem(((BaseController<UISettingHomeV2View>)this).view.btn_Vibration, ((BaseController<UISettingHomeV2View>)this).view.SetVibrationToggleVisual);
		}
		musicToggleItem.Initialize(arg.IsMusicOn, OnMusicToggle);
		soundToggleItem.Initialize(arg.IsSoundOn, OnSoundToggle);
		vibrationToggleItem.Initialize(arg.IsVibrateOn, OnVibrationToggle);
		DisableNameTabNameEditing();
		WireNameTabOpenUIUser();
		namePresenter?.Refresh(currentName);
		namePresenter?.TryAutoFocusOnFirstOpen();
		((BaseController<UISettingHomeV2View>)this).view.PlayOpenAnimation();
	}

	private void RefreshAccountBindEntryVisibility()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		RuntimePlatform platform = Application.platform;
		((BaseController<UISettingHomeV2View>)this).view.SetAccountBindEntriesVisible((int)platform == 11 || (int)platform == 7, (int)platform == 8 || (int)platform == 0);
	}

	private void RefreshAccountBindStatus()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		bool isBound = IsAccountBound(GetPlatformAccountType());
		((BaseController<UISettingHomeV2View>)this).view.SetAccountBindStatus(isGooglePlayEntry: true, isBound);
		((BaseController<UISettingHomeV2View>)this).view.SetAccountBindStatus(isGooglePlayEntry: false, isBound);
	}

	private static ThirdPartyType GetPlatformAccountType()
	{
		return (ThirdPartyType)3;
	}

	private static bool IsAccountBound(ThirdPartyType thirdPartyType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)thirdPartyType == 0)
		{
			return false;
		}
		AccountComponent account = GameApp.Account;
		if ((Object)(object)account != (Object)null && account.IsInitialized)
		{
			return account.IsBound(thirdPartyType);
		}
		return false;
	}

	private void OnGooglePlayAccountClick()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		HandleAccountEntryClick(GetPlatformAccountType());
	}

	private void OnIosAccountClick()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		HandleAccountEntryClick(GetPlatformAccountType());
	}

	private void HandleAccountEntryClick(ThirdPartyType thirdPartyType)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!isAccountActionInProgress && !LoginManager.IsThirdPartyLoginInFlight && (int)thirdPartyType != 0)
		{
			AccountComponent account = GameApp.Account;
			if ((Object)(object)account != (Object)null && account.IsInitialized && account.IsBound(thirdPartyType))
			{
				GameApp.UI.Open("UILoginSignOutWindow", (UILayer)2, (object)null, (Action)null);
				return;
			}
			UniTaskVoid val = BindAccountAsync(thirdPartyType);
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003COpenSaveSelectionAsync_003Ed__25))]
	private UniTask OpenSaveSelectionAsync(ThirdPartyType thirdPartyType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003COpenSaveSelectionAsync_003Ed__25 _003COpenSaveSelectionAsync_003Ed__ = default(_003COpenSaveSelectionAsync_003Ed__25);
		_003COpenSaveSelectionAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003COpenSaveSelectionAsync_003Ed__._003C_003E4__this = this;
		_003COpenSaveSelectionAsync_003Ed__.thirdPartyType = thirdPartyType;
		_003COpenSaveSelectionAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003COpenSaveSelectionAsync_003Ed__._003C_003Et__builder)).Start<_003COpenSaveSelectionAsync_003Ed__25>(ref _003COpenSaveSelectionAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003COpenSaveSelectionAsync_003Ed__._003C_003Et__builder)).Task;
	}

	[AsyncStateMachine(typeof(_003CSyncLocalSnapshotToNewCloudAsync_003Ed__26))]
	private UniTask SyncLocalSnapshotToNewCloudAsync(ThirdPartyType thirdPartyType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CSyncLocalSnapshotToNewCloudAsync_003Ed__26 _003CSyncLocalSnapshotToNewCloudAsync_003Ed__ = default(_003CSyncLocalSnapshotToNewCloudAsync_003Ed__26);
		_003CSyncLocalSnapshotToNewCloudAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CSyncLocalSnapshotToNewCloudAsync_003Ed__._003C_003E4__this = this;
		_003CSyncLocalSnapshotToNewCloudAsync_003Ed__.thirdPartyType = thirdPartyType;
		_003CSyncLocalSnapshotToNewCloudAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CSyncLocalSnapshotToNewCloudAsync_003Ed__._003C_003Et__builder)).Start<_003CSyncLocalSnapshotToNewCloudAsync_003Ed__26>(ref _003CSyncLocalSnapshotToNewCloudAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CSyncLocalSnapshotToNewCloudAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void CompleteAccountBinding()
	{
		CloudSaveFlowService.CompleteBindSelection("CompleteAccountBinding");
		LoginManager.ReportBindAssociationCompleted();
		RefreshAccountBindStatus();
	}

	private void RetryAccountBinding(ThirdPartyType thirdPartyType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = BindAccountAsync(thirdPartyType);
		((UniTaskVoid)(ref val)).Forget();
	}

	private void RetryCloudSync(ThirdPartyType thirdPartyType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		UniTaskExtensions.Forget(OpenSaveSelectionAsync(thirdPartyType));
	}

	private void CancelPendingAccountBindingFromSelectAccountUi(ThirdPartyType thirdPartyType)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		LoginManager.ReportBindAssociationCancelled();
		CancelPendingAccountBinding(thirdPartyType);
	}

	private void CancelPendingAccountBinding(ThirdPartyType thirdPartyType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = CancelPendingAccountBindingAsync(thirdPartyType);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CCancelPendingAccountBindingAsync_003Ed__32))]
	private UniTaskVoid CancelPendingAccountBindingAsync(ThirdPartyType thirdPartyType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CCancelPendingAccountBindingAsync_003Ed__32 _003CCancelPendingAccountBindingAsync_003Ed__ = default(_003CCancelPendingAccountBindingAsync_003Ed__32);
		_003CCancelPendingAccountBindingAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CCancelPendingAccountBindingAsync_003Ed__._003C_003E4__this = this;
		_003CCancelPendingAccountBindingAsync_003Ed__.thirdPartyType = thirdPartyType;
		_003CCancelPendingAccountBindingAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CCancelPendingAccountBindingAsync_003Ed__._003C_003Et__builder)).Start<_003CCancelPendingAccountBindingAsync_003Ed__32>(ref _003CCancelPendingAccountBindingAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CCancelPendingAccountBindingAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void OpenLinkResultWindow(bool isSuccess, Action retryAction = null, Action cancelAction = null)
	{
		UILoginSignInParam uILoginSignInParam = new UILoginSignInParam((!isSuccess) ? UILoginSignInResultType.LinkFailed : UILoginSignInResultType.LinkSuccess, retryAction, cancelAction);
		GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)uILoginSignInParam, (Action)null);
	}

	[AsyncStateMachine(typeof(_003CBindAccountAsync_003Ed__34))]
	private UniTaskVoid BindAccountAsync(ThirdPartyType thirdPartyType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CBindAccountAsync_003Ed__34 _003CBindAccountAsync_003Ed__ = default(_003CBindAccountAsync_003Ed__34);
		_003CBindAccountAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CBindAccountAsync_003Ed__._003C_003E4__this = this;
		_003CBindAccountAsync_003Ed__.thirdPartyType = thirdPartyType;
		_003CBindAccountAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CBindAccountAsync_003Ed__._003C_003Et__builder)).Start<_003CBindAccountAsync_003Ed__34>(ref _003CBindAccountAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CBindAccountAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void SetAccountButtonsInteractable(bool interactable)
	{
		if ((Object)(object)googlePlayAccountButton != (Object)null)
		{
			((Selectable)googlePlayAccountButton).interactable = interactable;
		}
		if ((Object)(object)iosAccountButton != (Object)null)
		{
			((Selectable)iosAccountButton).interactable = interactable;
		}
	}

	private void OnAccountLoginSucceed(EventLoginSucceed evt)
	{
		if (!isRemoved)
		{
			RefreshAccountBindStatus();
		}
	}

	private void OnMusicToggle(bool isOn)
	{
		SettingUtil.ToggleMusicState(isOn, userInitiated: true);
		RRDataTrack.Instance.ReportBtnClick("主页设置-背景音乐-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnSoundToggle(bool isOn)
	{
		SettingUtil.ToggleSoundState(isOn);
		RRDataTrack.Instance.ReportBtnClick("主页设置-音效-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnVibrationToggle(bool isOn)
	{
		SettingUtil.ToggleVibrationState(isOn);
		RRDataTrack.Instance.ReportBtnClick("主页设置-震动-[" + (isOn ? "开启" : "关闭") + "]");
	}

	private void OnCloseClick()
	{
		if (isClosing)
		{
			return;
		}
		isClosing = true;
		if (namePresenter != null && !namePresenter.TryCommit())
		{
			isClosing = false;
			return;
		}
		currentName = PlayerService.GetName();
		RRDataTrack.Instance.ReportBtnClick("主页设置-关闭");
		RRDataTrack.Instance.ReportUserSettingHomeCloseSnapshot(UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetMusicState()), UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetSoundState()), UserSettingTrackHelper.ToTrackFlag(SettingUtil.GetVibrationState()), UserSettingTrackHelper.ToAvatarTrackId(PlayerService.GetHeadId()));
		((BaseController<UISettingHomeV2View>)this).view.PlayCloseAnimation(delegate
		{
			((BaseController<UISettingHomeV2View>)this).CloseView(false);
		});
	}

	private void DisableNameTabNameEditing()
	{
		UIUser_NameTab uIUser_NameTab = ((BaseController<UISettingHomeV2View>)this).view.UIUser_NameTab;
		if (!((Object)(object)uIUser_NameTab == (Object)null))
		{
			if ((Object)(object)uIUser_NameTab.buttonEdit != (Object)null)
			{
				((Component)uIUser_NameTab.buttonEdit).gameObject.SetActive(false);
			}
			if ((Object)(object)uIUser_NameTab.buttonRandom != (Object)null)
			{
				((Component)uIUser_NameTab.buttonRandom).gameObject.SetActive(false);
			}
			if ((Object)(object)uIUser_NameTab.inputField != (Object)null)
			{
				((Component)uIUser_NameTab.inputField).gameObject.SetActive(false);
			}
		}
	}

	private void WireNameTabOpenUIUser()
	{
		UIUser_NameTab uIUser_NameTab = ((BaseController<UISettingHomeV2View>)this).view.UIUser_NameTab;
		if ((Object)(object)uIUser_NameTab == (Object)null)
		{
			return;
		}
		uIUser_NameTab.onBeforeOpenUIUser = delegate
		{
			if (namePresenter != null && !namePresenter.TryCommit())
			{
				return false;
			}
			currentName = PlayerService.GetName();
			((BaseController<UISettingHomeV2View>)this).view.RefreshName(currentName);
			ReportSettingHomeChangeAvatar();
			return true;
		};
	}

	private void ReportSettingHomeEditNickname()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-修改昵称");
	}

	private void OnSettingHomeRandomNameClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-随机昵称");
	}

	private void ReportSettingHomeChangeAvatar()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-修改头像");
	}

	private void OnSettingHomeChangeAvatarClick()
	{
		if (!GameApp.UI.IsOpen("UIUser"))
		{
			ReportSettingHomeChangeAvatar();
		}
	}

	private void SaveNameFromPresenter(string newName, bool skipValidation)
	{
		currentName = newName;
		string name = PlayerService.GetName();
		if (!string.Equals(currentName, name, StringComparison.Ordinal))
		{
			((BaseController<UISettingHomeV2View>)this).ExecuteCommand<UserSaveNameCommand>((object)new UserSaveNameRequest
			{
				PlayerName = currentName,
				SkipValidation = skipValidation
			});
		}
	}

	private void OnHowtoplayClick()
	{
		RRDataTrack.Instance.ReportBtnClick("主页设置-教程");
		GameApp.UI.Open("UIHowToPlayView", (UILayer)2, (object)HowToPlayType.Arrow, (Action)null);
	}

	private void OnAboutClick()
	{
		GameApp.UI.Open(UIPopupPathDefine.UIAboutUs, (UILayer)2, (object)null, (Action)null);
	}

	private void OnGMClick()
	{
		if (gmFirstClickTime <= 0f || Time.realtimeSinceStartup - gmFirstClickTime > 5f)
		{
			gmFirstClickTime = Time.realtimeSinceStartup;
			gmClickCount = 0;
		}
		gmClickCount++;
		if (!(Time.realtimeSinceStartup - gmFirstClickTime > 5f) && gmClickCount >= 20)
		{
			Singleton<VersionViewHelp>.Instance.OpenVersionUI();
			gmClickCount = 0;
			gmFirstClickTime = 0f;
		}
	}

	private void OnLanguageClick()
	{
		((BaseController<UISettingHomeV2View>)this).CloseView(false);
		GameApp.UI.Open("UILanguage", (UILayer)2, (object)null, (Action)null);
	}

	private void OnUserSaveSuccess(UserSaveSuccess evt)
	{
		currentName = evt.PlayerName;
		((BaseController<UISettingHomeV2View>)this).view.SetUserNameTab(evt.PlayerName, evt.HeadSprite, evt.HeadFrameSprite);
		namePresenter?.Refresh(evt.PlayerName);
	}

	private void OnUserNameSaveSuccess(UserNameSaveSuccess evt)
	{
		currentName = evt.PlayerName;
		((BaseController<UISettingHomeV2View>)this).view.RefreshName(evt.PlayerName);
		if (namePresenter != null && namePresenter.IsEditing)
		{
			namePresenter.EndEdit();
		}
		namePresenter?.Refresh(evt.PlayerName);
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		namePresenter?.OnLanguageChanged();
		namePresenter?.Refresh(PlayerService.GetName());
	}

	private void OnUserAvatarSaveSuccess(UserAvatarSaveSuccess evt)
	{
		((BaseController<UISettingHomeV2View>)this).view.RefreshNameHead(evt.HeadSprite);
		((BaseController<UISettingHomeV2View>)this).view.RefreshNameFrame(evt.HeadFrameSprite);
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UISettingHomeV2View>)this).OnActive(isActive);
		((BaseController<UISettingHomeV2View>)this).view.NotifyPopupMaskStackActive(isActive);
		if (isActive)
		{
			RRDataTrack.Instance.ReportBtnClick("主页设置-点击设置展现");
			namePresenter?.Refresh(PlayerService.GetName());
			RefreshAccountBindStatus();
		}
		else
		{
			namePresenter?.PersistDraftIfEditing();
			namePresenter?.SuppressAutoFocusForSession();
		}
	}

	public override void OnRemove()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		isRemoved = true;
		isAccountActionInProgress = false;
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.BtnClose.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.BtnGM.onClick).RemoveListener(new UnityAction(OnGMClick));
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.btnHowtoPlay.onClick).RemoveListener(new UnityAction(OnHowtoplayClick));
		((UnityEvent)((BaseController<UISettingHomeV2View>)this).view.btnAboutUs.onClick).RemoveListener(new UnityAction(OnAboutClick));
		Button btnLanguage = ((BaseController<UISettingHomeV2View>)this).view.btnLanguage;
		if (btnLanguage != null)
		{
			((UnityEvent)btnLanguage.onClick).RemoveListener(new UnityAction(OnLanguageClick));
		}
		Button btnRandom = ((BaseController<UISettingHomeV2View>)this).view.btnRandom;
		if (btnRandom != null)
		{
			((UnityEvent)btnRandom.onClick).RemoveListener(new UnityAction(OnSettingHomeRandomNameClick));
		}
		Button btnConvert = ((BaseController<UISettingHomeV2View>)this).view.btnConvert;
		if (btnConvert != null)
		{
			((UnityEvent)btnConvert.onClick).RemoveListener(new UnityAction(OnSettingHomeChangeAvatarClick));
		}
		Button obj = googlePlayAccountButton;
		if (obj != null)
		{
			((UnityEvent)obj.onClick).RemoveListener(new UnityAction(OnGooglePlayAccountClick));
		}
		Button obj2 = iosAccountButton;
		if (obj2 != null)
		{
			((UnityEvent)obj2.onClick).RemoveListener(new UnityAction(OnIosAccountClick));
		}
		googlePlayAccountButton = null;
		iosAccountButton = null;
		musicToggleItem?.Dispose();
		soundToggleItem?.Dispose();
		vibrationToggleItem?.Dispose();
		musicToggleItem = null;
		soundToggleItem = null;
		vibrationToggleItem = null;
		((BaseController<UISettingHomeV2View>)this).UnlistenEvent<SettingHomeInit>((Action<SettingHomeInit>)OnSettingHomeInit);
		((BaseController<UISettingHomeV2View>)this).UnlistenEvent<UserSaveSuccess>((Action<UserSaveSuccess>)OnUserSaveSuccess);
		((BaseController<UISettingHomeV2View>)this).UnlistenEvent<UserNameSaveSuccess>((Action<UserNameSaveSuccess>)OnUserNameSaveSuccess);
		((BaseController<UISettingHomeV2View>)this).UnlistenEvent<UserAvatarSaveSuccess>((Action<UserAvatarSaveSuccess>)OnUserAvatarSaveSuccess);
		((BaseController<UISettingHomeV2View>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UISettingHomeV2View>)this).UnlistenEvent<EventLoginSucceed>((Action<EventLoginSucceed>)OnAccountLoginSucceed);
		UIUser_NameTab uIUser_NameTab = ((BaseController<UISettingHomeV2View>)this).view.UIUser_NameTab;
		if ((Object)(object)uIUser_NameTab != (Object)null)
		{
			uIUser_NameTab.onBeforeOpenUIUser = null;
		}
		namePresenter?.Dispose();
		namePresenter = null;
		base.OnRemove();
	}

	public override void OnEscape()
	{
		if (namePresenter != null && namePresenter.IsEditing)
		{
			namePresenter.EndEdit();
		}
		else
		{
			OnCloseClick();
		}
	}
}
