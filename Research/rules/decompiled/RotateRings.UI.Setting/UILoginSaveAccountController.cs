using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using Game.UI;
using RotateRings.Services;
using RotateRings.Services.Account;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UILoginSaveAccount")]
public class UILoginSaveAccountController : GameBaseController<UILoginSaveAccountView>
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass10_0
	{
		public UILoginSaveAccountController _003C_003E4__this;

		public UILoginSaveAccountParam param;

		internal void _003CApplySelectedSnapshotAsync_003Eb__0()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			UniTaskVoid val = _003C_003E4__this.ApplySelectedSnapshotAsync();
			((UniTaskVoid)(ref val)).Forget();
		}

		internal void _003CApplySelectedSnapshotAsync_003Eb__1()
		{
			param.CompleteAction?.Invoke();
			GameApp.UI.CloseView("UILoginSelectAccount", (object)null, false);
			GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.LinkSuccess), (Action)null);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CApplySelectedSnapshotAsync_003Ed__10 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UILoginSaveAccountController _003C_003E4__this;

		private _003C_003Ec__DisplayClass10_0 _003C_003E8__1;

		private long _003CstartedAt_003E5__2;

		private Awaiter<CloudSaveDataSnapshot> _003C_003Eu__1;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILoginSaveAccountController uILoginSaveAccountController = _003C_003E4__this;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_007a;
				}
				_003C_003E8__1 = new _003C_003Ec__DisplayClass10_0();
				_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
				_003C_003E8__1.param = uILoginSaveAccountController.openParam;
				if (_003C_003E8__1.param != null)
				{
					uILoginSaveAccountController.isSaveOperationInProgress = true;
					uILoginSaveAccountController.SetButtonsInteractable(interactable: false);
					_003CstartedAt_003E5__2 = Stopwatch.GetTimestamp();
					goto IL_007a;
				}
				Log.LogError("[UI][UILoginSaveAccountController]", "存档确认参数为空，无法执行存档覆盖");
				goto end_IL_000e;
				IL_007a:
				try
				{
					Awaiter<CloudSaveDataSnapshot> val;
					bool flag;
					if (num != 0)
					{
						Awaiter<CloudSaveOperationResult> val2;
						if (num != 1)
						{
							if (_003C_003E8__1.param.Source == CloudSaveSelectionSource.Cloud)
							{
								val = LoginManager.PullCloudSnapshotToLocalForLoginAsync().GetAwaiter();
								if (!val.IsCompleted)
								{
									num = (_003C_003E1__state = 0);
									_003C_003Eu__1 = val;
									((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveDataSnapshot>, _003CApplySelectedSnapshotAsync_003Ed__10>(ref val, ref this);
									return;
								}
								goto IL_00ed;
							}
							val2 = LoginManager.PushLocalSnapshotToCloudForLoginAsync().GetAwaiter();
							if (!val2.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__2 = val2;
								((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CApplySelectedSnapshotAsync_003Ed__10>(ref val2, ref this);
								return;
							}
						}
						else
						{
							val2 = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<CloudSaveOperationResult>);
							num = (_003C_003E1__state = -1);
						}
						CloudSaveOperationResult result = val2.GetResult();
						flag = result?.IsSuccess ?? false;
						ReportLocalSelectionUploadResult(_003C_003E8__1.param, result, _003CstartedAt_003E5__2);
						goto IL_0187;
					}
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<CloudSaveDataSnapshot>);
					num = (_003C_003E1__state = -1);
					goto IL_00ed;
					IL_00ed:
					flag = val.GetResult() != null;
					goto IL_0187;
					IL_0187:
					if (!flag)
					{
						Log.LogError("[UI][UILoginSaveAccountController]", "存档同步失败 source={0}", new object[1] { _003C_003E8__1.param.Source });
						if (!uILoginSaveAccountController.isRemoved)
						{
							GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.LinkFailed, delegate
							{
								//IL_0006: Unknown result type (might be due to invalid IL or missing references)
								//IL_000b: Unknown result type (might be due to invalid IL or missing references)
								UniTaskVoid val3 = _003C_003E8__1._003C_003E4__this.ApplySelectedSnapshotAsync();
								((UniTaskVoid)(ref val3)).Forget();
							}), (Action)null);
						}
					}
					else
					{
						uILoginSaveAccountController.CloseViewWithAnimation(delegate
						{
							_003C_003E8__1.param.CompleteAction?.Invoke();
							GameApp.UI.CloseView("UILoginSelectAccount", (object)null, false);
							GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.LinkSuccess), (Action)null);
						});
					}
				}
				finally
				{
					if (num < 0)
					{
						uILoginSaveAccountController.isSaveOperationInProgress = false;
						if (!uILoginSaveAccountController.isRemoved && !uILoginSaveAccountController.isClosing)
						{
							uILoginSaveAccountController.SetButtonsInteractable(interactable: true);
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

	private const string LogTag = "[UI][UILoginSaveAccountController]";

	private UILoginSaveAccountParam openParam;

	private bool isSaveOperationInProgress;

	private bool isRemoved;

	private bool isClosing;

	public UILoginSaveAccountController(UILoginSaveAccountView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		openParam = param as UILoginSaveAccountParam;
		isSaveOperationInProgress = false;
		isRemoved = false;
		isClosing = false;
		if (openParam == null || openParam.PreviewSnapshot == null)
		{
			Log.LogError("[UI][UILoginSaveAccountController]", "存档确认参数无效，无法执行存档覆盖");
			((BaseController<UILoginSaveAccountView>)this).CloseView(false);
			return;
		}
		((BaseController<UILoginSaveAccountView>)this).view.SetAccountPreview(openParam.PreviewSnapshot);
		Button btn_Close = ((BaseController<UILoginSaveAccountView>)this).view.Btn_Close;
		if (btn_Close != null)
		{
			((UnityEvent)btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		Button btn_No = ((BaseController<UILoginSaveAccountView>)this).view.Btn_No;
		if (btn_No != null)
		{
			((UnityEvent)btn_No.onClick).AddListener(new UnityAction(OnNoClick));
		}
		Button btn_Yes = ((BaseController<UILoginSaveAccountView>)this).view.Btn_Yes;
		if (btn_Yes != null)
		{
			((UnityEvent)btn_Yes.onClick).AddListener(new UnityAction(OnYesClick));
		}
		((BaseController<UILoginSaveAccountView>)this).view.PlayOpenAnimation();
	}

	private void OnCloseClick()
	{
		if (!isSaveOperationInProgress)
		{
			CloseViewWithAnimation();
		}
	}

	private void OnNoClick()
	{
		if (!isSaveOperationInProgress)
		{
			CloseViewWithAnimation();
		}
	}

	private void OnYesClick()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!isSaveOperationInProgress)
		{
			UniTaskVoid val = ApplySelectedSnapshotAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CApplySelectedSnapshotAsync_003Ed__10))]
	private UniTaskVoid ApplySelectedSnapshotAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CApplySelectedSnapshotAsync_003Ed__10 _003CApplySelectedSnapshotAsync_003Ed__ = default(_003CApplySelectedSnapshotAsync_003Ed__10);
		_003CApplySelectedSnapshotAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CApplySelectedSnapshotAsync_003Ed__._003C_003E4__this = this;
		_003CApplySelectedSnapshotAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CApplySelectedSnapshotAsync_003Ed__._003C_003Et__builder)).Start<_003CApplySelectedSnapshotAsync_003Ed__10>(ref _003CApplySelectedSnapshotAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CApplySelectedSnapshotAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void ReportLocalSelectionUploadResult(UILoginSaveAccountParam param, CloudSaveOperationResult result, long startedAt)
	{
		bool flag = result?.IsSuccess ?? false;
		string failReason = (flag ? null : GetFailReason(result));
		RRDataTrack.Instance.ReportSavedataSyncRequestResult("log_in", flag ? "success" : "fail", GetElapsedMilliseconds(startedAt), param.PreviewSnapshot, param.CloudSnapshot, flag ? param.PreviewSnapshot : null, "local", failReason, isConflict: true);
	}

	private static string GetFailReason(CloudSaveOperationResult result)
	{
		if (result != null && result.Summary?.IndexOf("超时", StringComparison.Ordinal) >= 0)
		{
			return "timeout";
		}
		return "unknown";
	}

	private static int GetElapsedMilliseconds(long startedAt)
	{
		double val = (double)(Stopwatch.GetTimestamp() - startedAt) * 1000.0 / (double)Stopwatch.Frequency;
		return (int)Math.Max(0.0, Math.Min(2147483647.0, val));
	}

	private void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)((BaseController<UILoginSaveAccountView>)this).view.Btn_Close != (Object)null)
		{
			((Selectable)((BaseController<UILoginSaveAccountView>)this).view.Btn_Close).interactable = interactable;
		}
		if ((Object)(object)((BaseController<UILoginSaveAccountView>)this).view.Btn_No != (Object)null)
		{
			((Selectable)((BaseController<UILoginSaveAccountView>)this).view.Btn_No).interactable = interactable;
		}
		if ((Object)(object)((BaseController<UILoginSaveAccountView>)this).view.Btn_Yes != (Object)null)
		{
			((Selectable)((BaseController<UILoginSaveAccountView>)this).view.Btn_Yes).interactable = interactable;
		}
	}

	private void CloseViewWithAnimation(Action onClosed = null)
	{
		if (isClosing)
		{
			return;
		}
		if (isRemoved || ((BaseController<UILoginSaveAccountView>)this).view == null || (Object)(object)((UIBaseView)((BaseController<UILoginSaveAccountView>)this).view).Behaviour == (Object)null)
		{
			onClosed?.Invoke();
			return;
		}
		isClosing = true;
		((BaseController<UILoginSaveAccountView>)this).view.PlayCloseAnimation(delegate
		{
			if (!isRemoved)
			{
				((BaseController<UILoginSaveAccountView>)this).CloseView(false);
			}
			onClosed?.Invoke();
		});
	}

	public override void OnRemove()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		isRemoved = true;
		Button btn_Close = ((BaseController<UILoginSaveAccountView>)this).view.Btn_Close;
		if (btn_Close != null)
		{
			((UnityEvent)btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		Button btn_No = ((BaseController<UILoginSaveAccountView>)this).view.Btn_No;
		if (btn_No != null)
		{
			((UnityEvent)btn_No.onClick).RemoveListener(new UnityAction(OnNoClick));
		}
		Button btn_Yes = ((BaseController<UILoginSaveAccountView>)this).view.Btn_Yes;
		if (btn_Yes != null)
		{
			((UnityEvent)btn_Yes.onClick).RemoveListener(new UnityAction(OnYesClick));
		}
		openParam = null;
		base.OnRemove();
	}

	public override void OnEscape()
	{
		if (!isSaveOperationInProgress)
		{
			CloseViewWithAnimation();
		}
	}
}
