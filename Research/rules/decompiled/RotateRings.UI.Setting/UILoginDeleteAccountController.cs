using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.Services;
using RotateRings.Services.Account;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UILoginDeleteAccount")]
public class UILoginDeleteAccountController : GameBaseController<UILoginDeleteAccountView>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDeleteAccountAsync_003Ed__12 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UILoginDeleteAccountController _003C_003E4__this;

		private Awaiter<CloudSaveOperationResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILoginDeleteAccountController CS_0024_003C_003E8__locals8 = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					CS_0024_003C_003E8__locals8.isDeleting = true;
					CS_0024_003C_003E8__locals8.SetButtonsInteractable(interactable: false);
				}
				try
				{
					Awaiter<CloudSaveOperationResult> val;
					if (num != 0)
					{
						val = CloudSaveFlowService.DeleteCurrentThirdPartyCloudAccountAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveOperationResult>, _003CDeleteAccountAsync_003Ed__12>(ref val, ref this);
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
					if (result != null && result.IsSuccess)
					{
						CS_0024_003C_003E8__locals8.CloseViewWithAnimation(delegate
						{
							GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.DeleteSuccess), (Action)null);
						});
					}
					else
					{
						GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.LinkFailed, delegate
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0006: Unknown result type (might be due to invalid IL or missing references)
							UniTaskVoid val2 = CS_0024_003C_003E8__locals8.DeleteAccountAsync();
							((UniTaskVoid)(ref val2)).Forget();
						}), (Action)null);
					}
				}
				finally
				{
					if (num < 0)
					{
						CS_0024_003C_003E8__locals8.isDeleting = false;
						if (!CS_0024_003C_003E8__locals8.isRemoved && !CS_0024_003C_003E8__locals8.isClosing)
						{
							CS_0024_003C_003E8__locals8.SetButtonsInteractable(interactable: true);
						}
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadCloudAccountPreviewAsync_003Ed__7 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UILoginDeleteAccountController _003C_003E4__this;

		private Awaiter<CloudSaveSnapshotPullResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILoginDeleteAccountController uILoginDeleteAccountController = _003C_003E4__this;
			try
			{
				if (num != 0)
				{
					uILoginDeleteAccountController.isLoadingAccountPreview = true;
					uILoginDeleteAccountController.SetDeleteButtonInteractable(interactable: false);
				}
				try
				{
					Awaiter<CloudSaveSnapshotPullResult> val;
					if (num != 0)
					{
						val = LoginManager.PullCloudSnapshotForLoginAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<CloudSaveSnapshotPullResult>, _003CLoadCloudAccountPreviewAsync_003Ed__7>(ref val, ref this);
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
					if (!uILoginDeleteAccountController.isRemoved && result.IsSuccess && result.Snapshot != null)
					{
						((BaseController<UILoginDeleteAccountView>)uILoginDeleteAccountController).view.SetAccountPreview(result.Snapshot);
					}
				}
				finally
				{
					if (num < 0)
					{
						uILoginDeleteAccountController.isLoadingAccountPreview = false;
						if (!uILoginDeleteAccountController.isRemoved && !uILoginDeleteAccountController.isDeleting && !uILoginDeleteAccountController.isClosing)
						{
							uILoginDeleteAccountController.SetDeleteButtonInteractable(interactable: true);
						}
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

	private const string LogTag = "[UI][UILoginDeleteAccountController]";

	private bool isDeleting;

	private bool isLoadingAccountPreview;

	private bool isRemoved;

	private bool isClosing;

	public UILoginDeleteAccountController(UILoginDeleteAccountView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		isDeleting = false;
		isLoadingAccountPreview = false;
		isRemoved = false;
		isClosing = false;
		((BaseController<UILoginDeleteAccountView>)this).view.SetAccountPreview(null);
		Button btn_Close = ((BaseController<UILoginDeleteAccountView>)this).view.Btn_Close;
		if (btn_Close != null)
		{
			((UnityEvent)btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		}
		Button cancelBtn = ((BaseController<UILoginDeleteAccountView>)this).view.cancelBtn;
		if (cancelBtn != null)
		{
			((UnityEvent)cancelBtn.onClick).AddListener(new UnityAction(OnCancelClick));
		}
		Button deleteBtn = ((BaseController<UILoginDeleteAccountView>)this).view.deleteBtn;
		if (deleteBtn != null)
		{
			((UnityEvent)deleteBtn.onClick).AddListener(new UnityAction(OnDeleteClick));
		}
		((BaseController<UILoginDeleteAccountView>)this).view.PlayOpenAnimation();
		UniTaskVoid val = LoadCloudAccountPreviewAsync();
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CLoadCloudAccountPreviewAsync_003Ed__7))]
	private UniTaskVoid LoadCloudAccountPreviewAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadCloudAccountPreviewAsync_003Ed__7 _003CLoadCloudAccountPreviewAsync_003Ed__ = default(_003CLoadCloudAccountPreviewAsync_003Ed__7);
		_003CLoadCloudAccountPreviewAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CLoadCloudAccountPreviewAsync_003Ed__._003C_003E4__this = this;
		_003CLoadCloudAccountPreviewAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CLoadCloudAccountPreviewAsync_003Ed__._003C_003Et__builder)).Start<_003CLoadCloudAccountPreviewAsync_003Ed__7>(ref _003CLoadCloudAccountPreviewAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CLoadCloudAccountPreviewAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void SetDeleteButtonInteractable(bool interactable)
	{
		if ((Object)(object)((BaseController<UILoginDeleteAccountView>)this).view.deleteBtn != (Object)null)
		{
			((Selectable)((BaseController<UILoginDeleteAccountView>)this).view.deleteBtn).interactable = interactable;
		}
	}

	private void OnCloseClick()
	{
		CloseViewWithAnimation();
	}

	private void OnCancelClick()
	{
		CloseViewWithAnimation();
	}

	private void OnDeleteClick()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!isDeleting && !isLoadingAccountPreview)
		{
			UniTaskVoid val = DeleteAccountAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	[AsyncStateMachine(typeof(_003CDeleteAccountAsync_003Ed__12))]
	private UniTaskVoid DeleteAccountAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CDeleteAccountAsync_003Ed__12 _003CDeleteAccountAsync_003Ed__ = default(_003CDeleteAccountAsync_003Ed__12);
		_003CDeleteAccountAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CDeleteAccountAsync_003Ed__._003C_003E4__this = this;
		_003CDeleteAccountAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CDeleteAccountAsync_003Ed__._003C_003Et__builder)).Start<_003CDeleteAccountAsync_003Ed__12>(ref _003CDeleteAccountAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CDeleteAccountAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)((BaseController<UILoginDeleteAccountView>)this).view.Btn_Close != (Object)null)
		{
			((Selectable)((BaseController<UILoginDeleteAccountView>)this).view.Btn_Close).interactable = interactable;
		}
		if ((Object)(object)((BaseController<UILoginDeleteAccountView>)this).view.cancelBtn != (Object)null)
		{
			((Selectable)((BaseController<UILoginDeleteAccountView>)this).view.cancelBtn).interactable = interactable;
		}
		if ((Object)(object)((BaseController<UILoginDeleteAccountView>)this).view.deleteBtn != (Object)null)
		{
			((Selectable)((BaseController<UILoginDeleteAccountView>)this).view.deleteBtn).interactable = interactable;
		}
	}

	private void CloseViewWithAnimation(Action onClosed = null)
	{
		if (isClosing)
		{
			return;
		}
		if (isRemoved || ((BaseController<UILoginDeleteAccountView>)this).view == null || (Object)(object)((UIBaseView)((BaseController<UILoginDeleteAccountView>)this).view).Behaviour == (Object)null)
		{
			onClosed?.Invoke();
			return;
		}
		isClosing = true;
		((BaseController<UILoginDeleteAccountView>)this).view.PlayCloseAnimation(delegate
		{
			if (!isRemoved)
			{
				((BaseController<UILoginDeleteAccountView>)this).CloseView(false);
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
		Button btn_Close = ((BaseController<UILoginDeleteAccountView>)this).view.Btn_Close;
		if (btn_Close != null)
		{
			((UnityEvent)btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		}
		Button cancelBtn = ((BaseController<UILoginDeleteAccountView>)this).view.cancelBtn;
		if (cancelBtn != null)
		{
			((UnityEvent)cancelBtn.onClick).RemoveListener(new UnityAction(OnCancelClick));
		}
		Button deleteBtn = ((BaseController<UILoginDeleteAccountView>)this).view.deleteBtn;
		if (deleteBtn != null)
		{
			((UnityEvent)deleteBtn.onClick).RemoveListener(new UnityAction(OnDeleteClick));
		}
		base.OnRemove();
	}

	public override void OnEscape()
	{
		if (!isDeleting)
		{
			CloseViewWithAnimation();
		}
	}
}
