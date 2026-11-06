using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.Services.Account;
using RotateRings.Tool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Account;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.Setting;

[UIPath("UILoginSignOutWindow")]
public class UILoginSignOutController : GameBaseController<UILoginSignOutView>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CSignOutAsync_003Ed__11 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UILoginSignOutController _003C_003E4__this;

		private Awaiter<LoginResult> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILoginSignOutController CS_0024_003C_003E8__locals9 = _003C_003E4__this;
			try
			{
				if (num == 0)
				{
					goto IL_0050;
				}
				if (NetUtil.IsHaveNet)
				{
					CS_0024_003C_003E8__locals9.isSigningOut = true;
					CS_0024_003C_003E8__locals9.SetButtonsInteractable(interactable: false);
					goto IL_0050;
				}
				GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.LinkFailed, delegate
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					UniTaskVoid val2 = CS_0024_003C_003E8__locals9.SignOutAsync();
					((UniTaskVoid)(ref val2)).Forget();
				}), (Action)null);
				goto end_IL_000e;
				IL_0050:
				try
				{
					Awaiter<LoginResult> val;
					if (num != 0)
					{
						val = LoginManager.LogoutAsync().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<LoginResult>, _003CSignOutAsync_003Ed__11>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<LoginResult>);
						num = (_003C_003E1__state = -1);
					}
					LoginResult result = val.GetResult();
					if (result != null && result.IsSuccess)
					{
						CS_0024_003C_003E8__locals9.CloseViewWithAnimation(delegate
						{
							GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.SignOutSuccess), (Action)null);
						});
					}
					else
					{
						GameApp.UI.Open("UILoginSignInWindow", (UILayer)2, (object)new UILoginSignInParam(UILoginSignInResultType.LinkFailed, delegate
						{
							//IL_0001: Unknown result type (might be due to invalid IL or missing references)
							//IL_0006: Unknown result type (might be due to invalid IL or missing references)
							UniTaskVoid val2 = CS_0024_003C_003E8__locals9.SignOutAsync();
							((UniTaskVoid)(ref val2)).Forget();
						}), (Action)null);
					}
				}
				finally
				{
					if (num < 0)
					{
						CS_0024_003C_003E8__locals9.isSigningOut = false;
						if (!CS_0024_003C_003E8__locals9.isRemoved && !CS_0024_003C_003E8__locals9.isClosing)
						{
							CS_0024_003C_003E8__locals9.SetButtonsInteractable(interactable: true);
						}
					}
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

	private const string LogTag = "[UI][UILoginSignOutController]";

	private const string ConfirmTitleLangKey = "Account_SignOut_Confirm_Title";

	private const string ConfirmDescriptionLangKey = "Account_SignOut_Confirm_Desc";

	private bool isSigningOut;

	private bool isClosing;

	private bool isRemoved;

	public UILoginSignOutController(UILoginSignOutView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		isSigningOut = false;
		isClosing = false;
		isRemoved = false;
		((BaseController<UILoginSignOutView>)this).view.SetLocalizedContent("Account_SignOut_Confirm_Title", "Account_SignOut_Confirm_Desc");
		((UnityEvent)((BaseController<UILoginSignOutView>)this).view.Btn_Close.onClick).AddListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UILoginSignOutView>)this).view.Btn_SignOut.onClick).AddListener(new UnityAction(OnSignOutClick));
		((UnityEvent)((BaseController<UILoginSignOutView>)this).view.Btn_Cancel.onClick).AddListener(new UnityAction(OnCancelClick));
		((BaseController<UILoginSignOutView>)this).view.PlayOpenAnimation();
	}

	private void OnCloseClick()
	{
		CloseViewWithAnimation();
	}

	private void OnSignOutClick()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!isSigningOut)
		{
			UniTaskVoid val = SignOutAsync();
			((UniTaskVoid)(ref val)).Forget();
		}
	}

	private void OnCancelClick()
	{
		CloseViewWithAnimation();
	}

	[AsyncStateMachine(typeof(_003CSignOutAsync_003Ed__11))]
	private UniTaskVoid SignOutAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CSignOutAsync_003Ed__11 _003CSignOutAsync_003Ed__ = default(_003CSignOutAsync_003Ed__11);
		_003CSignOutAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CSignOutAsync_003Ed__._003C_003E4__this = this;
		_003CSignOutAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CSignOutAsync_003Ed__._003C_003Et__builder)).Start<_003CSignOutAsync_003Ed__11>(ref _003CSignOutAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CSignOutAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void SetButtonsInteractable(bool interactable)
	{
		if ((Object)(object)((BaseController<UILoginSignOutView>)this).view.Btn_Close != (Object)null)
		{
			((Selectable)((BaseController<UILoginSignOutView>)this).view.Btn_Close).interactable = interactable;
		}
		if ((Object)(object)((BaseController<UILoginSignOutView>)this).view.Btn_SignOut != (Object)null)
		{
			((Selectable)((BaseController<UILoginSignOutView>)this).view.Btn_SignOut).interactable = interactable;
		}
		if ((Object)(object)((BaseController<UILoginSignOutView>)this).view.Btn_Cancel != (Object)null)
		{
			((Selectable)((BaseController<UILoginSignOutView>)this).view.Btn_Cancel).interactable = interactable;
		}
	}

	private void CloseViewWithAnimation(Action onClosed = null)
	{
		if (isClosing)
		{
			return;
		}
		if (isRemoved || ((BaseController<UILoginSignOutView>)this).view == null || (Object)(object)((UIBaseView)((BaseController<UILoginSignOutView>)this).view).Behaviour == (Object)null)
		{
			onClosed?.Invoke();
			return;
		}
		isClosing = true;
		((BaseController<UILoginSignOutView>)this).view.PlayCloseAnimation(delegate
		{
			if (!isRemoved)
			{
				((BaseController<UILoginSignOutView>)this).CloseView(false);
			}
			onClosed?.Invoke();
		});
	}

	public override void OnRemove()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		isRemoved = true;
		((UnityEvent)((BaseController<UILoginSignOutView>)this).view.Btn_Close.onClick).RemoveListener(new UnityAction(OnCloseClick));
		((UnityEvent)((BaseController<UILoginSignOutView>)this).view.Btn_SignOut.onClick).RemoveListener(new UnityAction(OnSignOutClick));
		((UnityEvent)((BaseController<UILoginSignOutView>)this).view.Btn_Cancel.onClick).RemoveListener(new UnityAction(OnCancelClick));
		base.OnRemove();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UILoginSignOutView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		CloseViewWithAnimation();
	}
}
