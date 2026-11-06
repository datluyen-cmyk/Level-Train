using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.UI.SubView;
using UniLog;
using UnityEngine;
using XingYun.UI.Core;

namespace RotateRings.UI.Toast;

public class UIToastView : GameBaseView
{
	private delegate(WidgetHandle handle, IToastWidgetView view) ToastAttachHandler();

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayToastAsync_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public IToastWidgetView toastWidget;

		public UIToastOpenParam param;

		public int generation;

		public UIToastView _003C_003E4__this;

		public Action onAllClosed;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIToastView uIToastView = _003C_003E4__this;
			try
			{
				Awaiter<bool> val2;
				if (num != 0)
				{
					UniTask val = toastWidget.Show(param.Content, param.Duration);
					val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
					if (!val2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CPlayToastAsync_003Ed__26>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
				}
				if (!val2.GetResult() && generation == uIToastView.playGeneration)
				{
					Action onClosed = param.OnClosed;
					onAllClosed?.Invoke();
					onClosed?.Invoke();
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

	private const string Tag = "[UIToastView] ";

	private const string SingleToastLocation = "SingleToast";

	private Dictionary<EToastType, ToastAttachHandler> toastAttachers;

	private WidgetHandle currentHandle;

	private EToastType? currentToastType;

	private int playGeneration;

	private bool anchorOverridden;

	private Vector2 originalAnchorMin;

	private Vector2 originalAnchorMax;

	private Vector2 originalPivot;

	private Vector2 originalAnchoredPosition;

	public RectTransform Panel;

	public RectTransform Content;

	public RectTransform SingleToastAnchor;

	private Dictionary<EToastType, ToastAttachHandler> ToastAttachers => toastAttachers ?? (toastAttachers = BuildToastProviders());

	public void ShowToast(UIToastOpenParam param, Action onAllClosed)
	{
		if (param != null)
		{
			if (CanReuseCurrentToast(param.ToastType))
			{
				RefreshCurrentToast(param, onAllClosed);
				return;
			}
			ForceCleanup();
			AttachAndShowToast(param, onAllClosed);
		}
	}

	public void ForceCleanup()
	{
		playGeneration++;
		StopCurrentToastTimer();
		if (currentHandle != null)
		{
			DetachWidget(currentHandle);
			currentHandle = null;
		}
		RestoreAnchorOverride();
		currentToastType = null;
	}

	public override void Dispose()
	{
		ForceCleanup();
		((UIBaseView)this).Dispose();
	}

	public void RefreshVisibleText()
	{
		if (GetCurrentToastWidget() is UISingleToastView uISingleToastView)
		{
			uISingleToastView.RefreshTextMesh();
		}
	}

	private ToastAttachHandler GetToastAttacher(EToastType toastType)
	{
		if (ToastAttachers.TryGetValue(toastType, out var value))
		{
			return value;
		}
		return ToastAttachers[EToastType.Single];
	}

	private Dictionary<EToastType, ToastAttachHandler> BuildToastProviders()
	{
		return new Dictionary<EToastType, ToastAttachHandler> { 
		{
			EToastType.Single,
			AttachSingleToast
		} };
	}

	private (WidgetHandle handle, IToastWidgetView view) AttachSingleToast()
	{
		return AttachToastWidget<UISingleToastView>(SingleToastAnchor, "SingleToast");
	}

	private (WidgetHandle handle, IToastWidgetView view) AttachToastWidget<TWidget>(RectTransform anchor, string widgetLocation) where TWidget : GameWidgetView, IToastWidgetView, new()
	{
		if ((Object)(object)anchor == (Object)null)
		{
			return (handle: null, view: null);
		}
		WidgetHandle widgetHandle = AttachWidget<TWidget>(widgetLocation, (Transform)(object)anchor);
		return (handle: widgetHandle, view: (widgetHandle != null) ? widgetHandle.GetView<TWidget>() : null);
	}

	private void AttachAndShowToast(UIToastOpenParam param, Action onAllClosed)
	{
		(WidgetHandle handle, IToastWidgetView view) tuple = GetToastAttacher(param.ToastType)();
		WidgetHandle item = tuple.handle;
		IToastWidgetView item2 = tuple.view;
		currentHandle = item;
		if (item2 == null)
		{
			Log.LogError("[UIToastView] ", "AttachWidget failed, type={0}", new object[1] { param.ToastType });
			onAllClosed?.Invoke();
		}
		else
		{
			currentToastType = param.ToastType;
			ApplyAnchorOverride(param);
			PlayToast(item2, param, onAllClosed);
		}
	}

	private bool CanReuseCurrentToast(EToastType toastType)
	{
		if (currentHandle != null)
		{
			return currentToastType == toastType;
		}
		return false;
	}

	private void RefreshCurrentToast(UIToastOpenParam param, Action onAllClosed)
	{
		IToastWidgetView currentToastWidget = GetCurrentToastWidget();
		if (currentToastWidget == null)
		{
			ForceCleanup();
			AttachAndShowToast(param, onAllClosed);
		}
		else
		{
			ApplyAnchorOverride(param);
			PlayToast(currentToastWidget, param, onAllClosed);
		}
	}

	private void PlayToast(IToastWidgetView toastWidget, UIToastOpenParam param, Action onAllClosed)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		UniTaskVoid val = PlayToastAsync(toastWidget, param, onAllClosed, ++playGeneration);
		((UniTaskVoid)(ref val)).Forget();
	}

	[AsyncStateMachine(typeof(_003CPlayToastAsync_003Ed__26))]
	private UniTaskVoid PlayToastAsync(IToastWidgetView toastWidget, UIToastOpenParam param, Action onAllClosed, int generation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayToastAsync_003Ed__26 _003CPlayToastAsync_003Ed__ = default(_003CPlayToastAsync_003Ed__26);
		_003CPlayToastAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CPlayToastAsync_003Ed__._003C_003E4__this = this;
		_003CPlayToastAsync_003Ed__.toastWidget = toastWidget;
		_003CPlayToastAsync_003Ed__.param = param;
		_003CPlayToastAsync_003Ed__.onAllClosed = onAllClosed;
		_003CPlayToastAsync_003Ed__.generation = generation;
		_003CPlayToastAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayToastAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayToastAsync_003Ed__26>(ref _003CPlayToastAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CPlayToastAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private IToastWidgetView GetCurrentToastWidget()
	{
		return currentHandle?.View as IToastWidgetView;
	}

	private void StopCurrentToastTimer()
	{
		GetCurrentToastWidget()?.CancelShow();
	}

	private void ApplyAnchorOverride(UIToastOpenParam param)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SingleToastAnchor == (Object)null || param.VerticalAlign == ToastVerticalAlign.Default)
		{
			RestoreAnchorOverride();
			return;
		}
		if (!anchorOverridden)
		{
			originalAnchorMin = SingleToastAnchor.anchorMin;
			originalAnchorMax = SingleToastAnchor.anchorMax;
			originalPivot = SingleToastAnchor.pivot;
			originalAnchoredPosition = SingleToastAnchor.anchoredPosition;
		}
		anchorOverridden = true;
		float x = SingleToastAnchor.pivot.x;
		float x2 = SingleToastAnchor.anchoredPosition.x;
		switch (param.VerticalAlign)
		{
		case ToastVerticalAlign.Top:
			SingleToastAnchor.anchorMin = new Vector2(0.5f, 1f);
			SingleToastAnchor.anchorMax = new Vector2(0.5f, 1f);
			SingleToastAnchor.pivot = new Vector2(x, 1f);
			SingleToastAnchor.anchoredPosition = new Vector2(x2, 0f - param.Offset);
			break;
		case ToastVerticalAlign.Bottom:
			SingleToastAnchor.anchorMin = new Vector2(0.5f, 0f);
			SingleToastAnchor.anchorMax = new Vector2(0.5f, 0f);
			SingleToastAnchor.pivot = new Vector2(x, 0f);
			SingleToastAnchor.anchoredPosition = new Vector2(x2, param.Offset);
			break;
		case ToastVerticalAlign.Center:
			SingleToastAnchor.anchorMin = new Vector2(0.5f, 0.5f);
			SingleToastAnchor.anchorMax = new Vector2(0.5f, 0.5f);
			SingleToastAnchor.pivot = new Vector2(x, 0.5f);
			SingleToastAnchor.anchoredPosition = new Vector2(x2, param.Offset);
			break;
		}
	}

	private void RestoreAnchorOverride()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (anchorOverridden && !((Object)(object)SingleToastAnchor == (Object)null))
		{
			SingleToastAnchor.anchorMin = originalAnchorMin;
			SingleToastAnchor.anchorMax = originalAnchorMax;
			SingleToastAnchor.pivot = originalPivot;
			SingleToastAnchor.anchoredPosition = originalAnchoredPosition;
			anchorOverridden = false;
		}
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Panel = behaviour.Get<RectTransform>("Panel");
		Content = behaviour.Get<RectTransform>("Content");
		SingleToastAnchor = behaviour.Get<RectTransform>("SingleToastAnchor");
	}
}
