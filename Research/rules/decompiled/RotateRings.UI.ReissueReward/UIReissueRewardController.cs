using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using RotateRings.Common;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.Home;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vibration;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.ReissueReward;

[UIPath("UIReissueReward")]
public class UIReissueRewardController : GameBaseController<UIReissueRewardView>
{
	public sealed class OpenParam
	{
		public List<ReissueRewardTransaction> Transactions;

		public Action OnClosed;
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CCloseAfterFlyAsync_003Ed__19 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UIReissueRewardController _003C_003E4__this;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIReissueRewardController uIReissueRewardController = _003C_003E4__this;
			try
			{
				try
				{
					Awaiter<bool> val2;
					if (num != 0)
					{
						UniTask val = UniTask.NextFrame(UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)((UIBaseView)((BaseController<UIReissueRewardView>)uIReissueRewardController).view).Behaviour));
						val2 = ((UniTask)(ref val)).SuppressCancellationThrow().GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CCloseAfterFlyAsync_003Ed__19>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<bool>);
						num = (_003C_003E1__state = -1);
					}
					if (!val2.GetResult())
					{
						((BaseController<UIReissueRewardView>)uIReissueRewardController).CloseView(false);
					}
				}
				catch (Exception ex)
				{
					Log.LogError("[UIReissueReward]", "奖励飞行关闭收尾异常: {0}", new object[1] { ex });
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

	private const string Tag = "[UIReissueReward]";

	private const string ReissueItemLocation = "ReissueRewardItem";

	private readonly List<WidgetHandle> _rewardItems = new List<WidgetHandle>();

	private OpenParam _openParam;

	private Action _onClosed;

	private bool _isClosing;

	private Button _backgroundButton;

	private bool _createdBackgroundButton;

	public UIReissueRewardController(UIReissueRewardView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		_openParam = param as OpenParam;
		_onClosed = _openParam?.OnClosed;
		if (_openParam?.Transactions == null || _openParam.Transactions.Count == 0)
		{
			((BaseController<UIReissueRewardView>)this).CloseView(false);
		}
		else if (!ReissueRewardService.GrantTransactions(_openParam.Transactions))
		{
			Log.LogError("[UIReissueReward]", "补发奖励入账失败，关闭 UIReissueReward");
			((BaseController<UIReissueRewardView>)this).CloseView(false);
		}
		else
		{
			RenderRewards(_openParam.Transactions);
			BindClickEvents();
			((BaseController<UIReissueRewardView>)this).view.PlayRootOpen();
		}
	}

	public override void OnRemove()
	{
		UnbindClickEvents();
		ClearRewardItems();
		Action onClosed = _onClosed;
		_onClosed = null;
		_openParam = null;
		base.OnRemove();
		onClosed?.Invoke();
	}

	public override void OnActive(bool isActive)
	{
		((BaseController<UIReissueRewardView>)this).OnActive(isActive);
	}

	public override void OnEscape()
	{
		CloseFromClick();
	}

	private void RenderRewards(IEnumerable<ReissueRewardTransaction> transactions)
	{
		ClearRewardItems();
		if ((Object)(object)((BaseController<UIReissueRewardView>)this).view.GridLayout == (Object)null)
		{
			return;
		}
		PropModelProxy propProxy = GameApp.Proxy.Get<PropModelProxy>();
		foreach (ReissueRewardTransaction transaction in transactions)
		{
			if (transaction?.RewardItems == null)
			{
				continue;
			}
			foreach (KeyValuePair<int, int> rewardItem in transaction.RewardItems)
			{
				AddRewardItem(rewardItem.Key, rewardItem.Value, propProxy);
			}
		}
		GameObject reissueRewardItem = ((BaseController<UIReissueRewardView>)this).view.ReissueRewardItem;
		if (reissueRewardItem != null)
		{
			reissueRewardItem.SetActive(false);
		}
	}

	private void AddRewardItem(int propId, int count, PropModelProxy propProxy)
	{
		if (propId <= 0 || count <= 0)
		{
			return;
		}
		EPropType ePropType = propProxy?.GetRewardPropType(propId) ?? EPropType.None;
		if (ePropType == EPropType.None)
		{
			return;
		}
		WidgetHandle widgetHandle = ((BaseController<UIReissueRewardView>)this).view.AttachWidget<UIReissueRewardItemView>("ReissueRewardItem", ((Component)((BaseController<UIReissueRewardView>)this).view.GridLayout).transform);
		if (widgetHandle == null)
		{
			Log.LogError("[UIReissueReward]", "AttachWidget failed: {0}", new object[1] { "ReissueRewardItem" });
			return;
		}
		UIReissueRewardItemView view = widgetHandle.GetView<UIReissueRewardItemView>();
		if (view == null)
		{
			Log.LogError("[UIReissueReward]", "ReissueRewardItem view type mismatch");
			widgetHandle.Dispose();
		}
		else
		{
			view.OnRender(PropIconService.GetIconNameLarge(ePropType), count);
			_rewardItems.Add(widgetHandle);
		}
	}

	private void BindClickEvents()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if ((Object)(object)((BaseController<UIReissueRewardView>)this).view.btnTapContinue != (Object)null)
		{
			((UnityEvent)((BaseController<UIReissueRewardView>)this).view.btnTapContinue.onClick).AddListener(new UnityAction(CloseFromClick));
		}
	}

	private void UnbindClickEvents()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		if (((BaseController<UIReissueRewardView>)this).view != null && (Object)(object)((BaseController<UIReissueRewardView>)this).view.btnTapContinue != (Object)null)
		{
			((UnityEvent)((BaseController<UIReissueRewardView>)this).view.btnTapContinue.onClick).RemoveListener(new UnityAction(CloseFromClick));
		}
		if ((Object)(object)_backgroundButton != (Object)null)
		{
			((UnityEvent)_backgroundButton.onClick).RemoveListener(new UnityAction(CloseFromClick));
			if (_createdBackgroundButton)
			{
				Object.Destroy((Object)(object)_backgroundButton);
			}
		}
		_backgroundButton = null;
		_createdBackgroundButton = false;
	}

	private void CloseFromClick()
	{
		if (_isClosing)
		{
			return;
		}
		_isClosing = true;
		if ((Object)(object)((BaseController<UIReissueRewardView>)this).view.btnTapContinue != (Object)null)
		{
			((Behaviour)((BaseController<UIReissueRewardView>)this).view.btnTapContinue).enabled = false;
		}
		RectTransform buttonLevelRT = UIHomeService.GetButtonLevelRT();
		List<RectTransform> list = new List<RectTransform>(_rewardItems.Count);
		foreach (WidgetHandle rewardItem in _rewardItems)
		{
			rewardItem?.GetView<UIReissueRewardItemView>()?.EnterFlySimulationSpace();
			object obj;
			if (rewardItem == null)
			{
				obj = null;
			}
			else
			{
				GameObject gameObject = rewardItem.GameObject;
				obj = ((gameObject != null) ? gameObject.transform : null);
			}
			RectTransform val = (RectTransform)((obj is RectTransform) ? obj : null);
			if ((Object)(object)val != (Object)null)
			{
				list.Add(val);
			}
		}
		bool flag = ((BaseController<UIReissueRewardView>)this).view.PlayCloseAndFlyItems(list, buttonLevelRT, OnFlyFirstArrived, delegate
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			CloseAfterFlyAsync();
		});
		float num = ((BaseController<UIReissueRewardView>)this).view.PlayRootClose();
		if (!flag && num > 0f)
		{
			GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UIReissueRewardView>)this).view).Behaviour, num, (Action)delegate
			{
				((BaseController<UIReissueRewardView>)this).CloseView(false);
			});
		}
		else if (!flag)
		{
			((BaseController<UIReissueRewardView>)this).CloseView(false);
		}
	}

	[AsyncStateMachine(typeof(_003CCloseAfterFlyAsync_003Ed__19))]
	private UniTaskVoid CloseAfterFlyAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CCloseAfterFlyAsync_003Ed__19 _003CCloseAfterFlyAsync_003Ed__ = default(_003CCloseAfterFlyAsync_003Ed__19);
		_003CCloseAfterFlyAsync_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CCloseAfterFlyAsync_003Ed__._003C_003E4__this = this;
		_003CCloseAfterFlyAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CCloseAfterFlyAsync_003Ed__._003C_003Et__builder)).Start<_003CCloseAfterFlyAsync_003Ed__19>(ref _003CCloseAfterFlyAsync_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CCloseAfterFlyAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void OnFlyFirstArrived()
	{
		AudioService.PlayUiSound(AudioKeys.ItemAdd.Value);
		VibrateUtility.VibrateRewardFlyIn(_rewardItems.Count, "UIReissueReward-查收未领取奖励飞抵按钮");
		UIHomeService.LoadLevelButtonEffect();
	}

	private void ClearRewardItems()
	{
		_backgroundButton = null;
		foreach (WidgetHandle rewardItem in _rewardItems)
		{
			rewardItem?.Dispose();
		}
		_rewardItems.Clear();
	}
}
