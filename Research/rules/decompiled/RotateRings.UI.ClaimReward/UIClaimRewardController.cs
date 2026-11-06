using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using Example.Cfg;
using Game.DataTrack;
using Game.UI;
using Game.UI.ClaimReward;
using RotateRings.Ad;
using RotateRings.Common;
using RotateRings.Prop;
using RotateRings.Services;
using Rotaterings.UI.RankList;
using Spine.Unity;
using UniLog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vibration;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.ClaimReward;

[UIPath("UIClaimReward")]
public class UIClaimRewardController : GameBaseController<UIClaimRewardView>
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CPlayPreparedNoBoxOpenAsync_003Ed__26 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public int version;

		public UIClaimRewardController _003C_003E4__this;

		public UIClaimRewardParam openParam;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UIClaimRewardController uIClaimRewardController = _003C_003E4__this;
			try
			{
				Awaiter val2;
				if (num != 0)
				{
					UniTask val = UniTask.NextFrame((PlayerLoopTiming)13);
					val2 = ((UniTask)(ref val)).GetAwaiter();
					if (!((Awaiter)(ref val2)).IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val2;
						((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003CPlayPreparedNoBoxOpenAsync_003Ed__26>(ref val2, ref this);
						return;
					}
				}
				else
				{
					val2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter);
					num = (_003C_003E1__state = -1);
				}
				((Awaiter)(ref val2)).GetResult();
				if (version == uIClaimRewardController._noBoxOpenVersion && uIClaimRewardController._openParam == openParam && !((Object)(object)((UIBaseView)((BaseController<UIClaimRewardView>)uIClaimRewardController).view).Behaviour == (Object)null) && ((Component)((UIBaseView)((BaseController<UIClaimRewardView>)uIClaimRewardController).view).Behaviour).gameObject.activeInHierarchy)
				{
					((BaseController<UIClaimRewardView>)uIClaimRewardController).view.ShowRewardPresentation(uIClaimRewardController.HasAnimalReward(), uIClaimRewardController.HasRewardToClaim() && (openParam?.EnableClaimX2 ?? false), uIClaimRewardController.UseContinueClaimButton);
					openParam?.OnNoBoxOpenStarted?.Invoke();
					if (version == uIClaimRewardController._noBoxOpenVersion && uIClaimRewardController.HasRewardToClaim())
					{
						((BaseController<UIClaimRewardView>)uIClaimRewardController).view.SetClaimButtonsInteractable(interactable: true);
					}
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
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

	private const string DoubleRewardItemClipName = "anim_propitemtemplate_gp1";

	private const float DoubleRewardCountUpdateDelay = 0.1f;

	private const string DefaultTitleTextKey = "LeaderBoard_Reward_1";

	private const string RankType1ContentTextKey = "Leaderboard_Reward_Congrats_1";

	private const string RankType1DescriptionTextKey = "Leaderboard_Reward_Congrats_2";

	private const string RankType2ContentTextKey = "Treasure_Title";

	private const string RankType2DescriptionTextKey = "Leaderboard_Reward_Toast";

	private const string RankType4ContentTextKey = "Treasure_Title";

	private const string RankType4DescriptionTextKey = "Leaderboard_Reward_Toast";

	private const float BoxOpenLeadDelay = 1.4f;

	private UIClaimRewardParam _openParam;

	private Tween _boxFlyTween;

	private Tween _closeTween;

	private Tween _boxOpenLeadTween;

	private Sequence _propFlySequence;

	private Sequence _x2RewardSequence;

	private readonly List<UIPropItemTemplateController> _propItems = new List<UIPropItemTemplateController>();

	private List<(int propId, int count)> _orderedRewardItems;

	private bool _hasNonConsumableProp;

	private bool _hasPropType2;

	private Action _pendingGrantContinuation;

	private int _preInstantiateRewardCoroutineId = -1;

	private int _openBoxSoundTimerId = -1;

	private int _noBoxOpenVersion;

	private bool UseContinueClaimButton
	{
		get
		{
			if (HasRewardToClaim())
			{
				UIClaimRewardParam openParam = _openParam;
				if (openParam == null || !openParam.EnableClaimX2)
				{
					UIClaimRewardParam openParam2 = _openParam;
					if (openParam2 == null || !openParam2.UseContinueButton)
					{
						return _openParam?.SkipBoxAnimation ?? false;
					}
					return true;
				}
			}
			return false;
		}
	}

	public UIClaimRewardController(UIClaimRewardView view)
		: base(view)
	{
	}

	public override void OnRegister(object param = null)
	{
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Expected O, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Expected O, but got Unknown
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Expected O, but got Unknown
		_noBoxOpenVersion++;
		if ((Object)(object)((UIBaseView)((BaseController<UIClaimRewardView>)this).view).Behaviour != (Object)null)
		{
			UIClaimRewardViewRefs component = ((Component)((UIBaseView)((BaseController<UIClaimRewardView>)this).view).Behaviour).GetComponent<UIClaimRewardViewRefs>();
			((BaseController<UIClaimRewardView>)this).view.InjectRefs(component);
		}
		_openParam = param as UIClaimRewardParam;
		bool flag = _openParam?.SkipBoxAnimation ?? false;
		GameObject box_node = ((BaseController<UIClaimRewardView>)this).view.box_node;
		if (box_node != null)
		{
			box_node.SetActive(!flag);
		}
		UIClaimRewardParam openParam = _openParam;
		if (openParam != null && openParam.HideBgMaskOnOpen)
		{
			((BaseController<UIClaimRewardView>)this).view.HideBgMaskVisual();
		}
		else
		{
			((BaseController<UIClaimRewardView>)this).view.PlayMaskOpen(_openParam?.SkipMaskFadeIn ?? false);
		}
		if (_openParam != null && !flag)
		{
			if (!string.IsNullOrEmpty(_openParam.BoxPrefabLocation))
			{
				GameObject val = GameApp.Res.LoadAsset<GameObject>(_openParam.BoxPrefabLocation, (string)null);
				if ((Object)(object)val != (Object)null)
				{
					((BaseController<UIClaimRewardView>)this).view.ReplaceBoxNodeChild(val);
				}
			}
			((BaseController<UIClaimRewardView>)this).view.SetBoxNodeStartWorldPosition(_openParam.BoxStartWorldPosition);
			_boxFlyTween = ((BaseController<UIClaimRewardView>)this).view.PlayBoxFlyToEnd(0.5f, _openParam.EnableBoxFlyFadeIn, _openParam.OnBoxArrive);
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaimX2 != (Object)null)
		{
			((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnClaimX2.onClick).AddListener(new UnityAction(OnClaimX2Click));
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaim != (Object)null)
		{
			((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnClaim.onClick).AddListener(new UnityAction(OnClaimClick));
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnContinue != (Object)null)
		{
			((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnContinue.onClick).AddListener(new UnityAction(OnClaimClick));
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnOk != (Object)null)
		{
			((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnOk.onClick).AddListener(new UnityAction(OnBtnOkClick));
		}
		RemoveButtonDisabledTint(((BaseController<UIClaimRewardView>)this).view.btnClaimX2);
		RemoveButtonDisabledTint(((BaseController<UIClaimRewardView>)this).view.btnClaim);
		RemoveButtonDisabledTint(((BaseController<UIClaimRewardView>)this).view.btnContinue);
		string defaultTitleKey = (string.IsNullOrEmpty(_openParam?.TitleTextKey) ? "LeaderBoard_Reward_1" : _openParam.TitleTextKey);
		ConfigureRankRewardTitle(defaultTitleKey);
		((BaseController<UIClaimRewardView>)this).ListenEvent<ClaimRewardGrantSuccess>((Action<ClaimRewardGrantSuccess>)OnGrantSuccess);
		((BaseController<UIClaimRewardView>)this).ListenEvent<ClaimRewardGrantFailed>((Action<ClaimRewardGrantFailed>)OnGrantFailed);
		SetupButtonVisibility();
		TryReportAdRewardEnterShow();
		_openParam?.OnOpen?.Invoke();
		bool flag2 = !string.IsNullOrEmpty(_openParam?.PendingRewardTransactionId);
		if (IsChestReward() && HasRewardToClaim() && !flag2)
		{
			ClaimRewardService.Grant(_openParam);
			RRDataTrack.Instance.ReportChestClaimPopupShow();
		}
		if (flag)
		{
			((BaseController<UIClaimRewardView>)this).view.PrepareRewardPresentation();
		}
		else
		{
			_openBoxSoundTimerId = GameApp.Timer.Delay(0.5f, (Action)delegate
			{
				AudioService.PlayUiSound(AudioKeys.Unlock.Value);
				_openBoxSoundTimerId = -1;
			});
			((BaseController<UIClaimRewardView>)this).view.PlayBoxSpine(_openParam?.BoxAnimName, null);
			((BaseController<UIClaimRewardView>)this).view.PlayPanelOpen(playMask: false, HasAnimalReward(), HasRewardToClaim() && (_openParam?.EnableClaimX2 ?? false), UseContinueClaimButton);
		}
		if (HasRewardToClaim())
		{
			((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: false);
		}
		if (!flag && !string.IsNullOrEmpty(_openParam?.BoxSkeletonDataAssetPath))
		{
			ReplaceBoxSpineData(_openParam.BoxSkeletonDataAssetPath, _openParam.BoxSkinName);
		}
		if (flag && HasRewardToClaim())
		{
			InstantiateReward();
			SetHLayoutState(enable: false);
			SnapPropItemsToTarget();
			AudioService.PlayUiSound(AudioKeys.ItemHammerGet.Value);
		}
		else if (HasRewardToClaim())
		{
			GameObject val2 = (((Object)(object)((UIBaseView)((BaseController<UIClaimRewardView>)this).view).NodeRect != (Object)null) ? ((Component)((UIBaseView)((BaseController<UIClaimRewardView>)this).view).NodeRect).gameObject : null);
			if ((Object)(object)val2 != (Object)null)
			{
				Tween boxOpenLeadTween = _boxOpenLeadTween;
				if (boxOpenLeadTween != null)
				{
					TweenExtensions.Kill(boxOpenLeadTween, false);
				}
				_boxOpenLeadTween = (Tween)(object)TweenSettingsExtensions.SetLink<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(TweenSettingsExtensions.AppendCallback(TweenSettingsExtensions.AppendInterval(DOTween.Sequence(), 1.4f), new TweenCallback(HandleHasReward)), (object)val2), val2, (LinkBehaviour)5);
			}
		}
		if (flag)
		{
			UniTaskExtensions.Forget(PlayPreparedNoBoxOpenAsync(_openParam, _noBoxOpenVersion));
		}
	}

	[AsyncStateMachine(typeof(_003CPlayPreparedNoBoxOpenAsync_003Ed__26))]
	private UniTask PlayPreparedNoBoxOpenAsync(UIClaimRewardParam openParam, int version)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_003CPlayPreparedNoBoxOpenAsync_003Ed__26 _003CPlayPreparedNoBoxOpenAsync_003Ed__ = default(_003CPlayPreparedNoBoxOpenAsync_003Ed__26);
		_003CPlayPreparedNoBoxOpenAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CPlayPreparedNoBoxOpenAsync_003Ed__._003C_003E4__this = this;
		_003CPlayPreparedNoBoxOpenAsync_003Ed__.openParam = openParam;
		_003CPlayPreparedNoBoxOpenAsync_003Ed__.version = version;
		_003CPlayPreparedNoBoxOpenAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CPlayPreparedNoBoxOpenAsync_003Ed__._003C_003Et__builder)).Start<_003CPlayPreparedNoBoxOpenAsync_003Ed__26>(ref _003CPlayPreparedNoBoxOpenAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CPlayPreparedNoBoxOpenAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public override void OnRemove()
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		_noBoxOpenVersion++;
		if (_openBoxSoundTimerId >= 0)
		{
			GameApp.Timer.Kill(_openBoxSoundTimerId);
			_openBoxSoundTimerId = -1;
		}
		if (_preInstantiateRewardCoroutineId > 0)
		{
			GameApp.Coroutine.Kill(_preInstantiateRewardCoroutineId);
			_preInstantiateRewardCoroutineId = -1;
		}
		KillCloseTween();
		Tween boxFlyTween = _boxFlyTween;
		if (boxFlyTween != null)
		{
			TweenExtensions.Kill(boxFlyTween, false);
		}
		_boxFlyTween = null;
		Tween boxOpenLeadTween = _boxOpenLeadTween;
		if (boxOpenLeadTween != null)
		{
			TweenExtensions.Kill(boxOpenLeadTween, false);
		}
		_boxOpenLeadTween = null;
		Sequence propFlySequence = _propFlySequence;
		if (propFlySequence != null)
		{
			TweenExtensions.Kill((Tween)(object)propFlySequence, false);
		}
		_propFlySequence = null;
		Sequence x2RewardSequence = _x2RewardSequence;
		if (x2RewardSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)x2RewardSequence, false);
		}
		_x2RewardSequence = null;
		((BaseController<UIClaimRewardView>)this).view.ClearX2Particles();
		if (((BaseController<UIClaimRewardView>)this).view != null)
		{
			if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnOk != (Object)null)
			{
				((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnOk.onClick).RemoveListener(new UnityAction(OnBtnOkClick));
			}
			if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaimX2 != (Object)null)
			{
				((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnClaimX2.onClick).RemoveListener(new UnityAction(OnClaimX2Click));
			}
			if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaim != (Object)null)
			{
				((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnClaim.onClick).RemoveListener(new UnityAction(OnClaimClick));
			}
			if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnContinue != (Object)null)
			{
				((UnityEvent)((BaseController<UIClaimRewardView>)this).view.btnContinue.onClick).RemoveListener(new UnityAction(OnClaimClick));
			}
		}
		((BaseController<UIClaimRewardView>)this).UnlistenEvent<ClaimRewardGrantSuccess>((Action<ClaimRewardGrantSuccess>)OnGrantSuccess);
		((BaseController<UIClaimRewardView>)this).UnlistenEvent<ClaimRewardGrantFailed>((Action<ClaimRewardGrantFailed>)OnGrantFailed);
		_pendingGrantContinuation = null;
		ClearPropItems();
		base.OnRemove();
	}

	private void OnBtnOkClick()
	{
		if (!HasRewardToClaim())
		{
			((Selectable)((BaseController<UIClaimRewardView>)this).view.btnOk).interactable = false;
			KillCloseTween();
			_closeTween = PlayPanelClose(delegate
			{
				((BaseController<UIClaimRewardView>)this).CloseView(false);
			});
		}
	}

	private void OnClaimClick()
	{
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaim != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnClaim).GetComponent<Image>();
		}
		_openParam?.OnClaimClickTrack?.Invoke();
		if (IsChestReward())
		{
			RRDataTrack.Instance.ReportChestClaimNormal();
		}
		((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: false);
		RequestGrant(1, PlayCloseAndFly);
	}

	private void OnClaimX2Click()
	{
		_openParam?.OnClaimX2ClickTrack?.Invoke();
		((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: false);
		Sequence propFlySequence = _propFlySequence;
		if (propFlySequence != null)
		{
			TweenExtensions.Kill((Tween)(object)propFlySequence, false);
		}
		_propFlySequence = null;
		SnapPropItemsToTarget();
		AdPopPosition adPopPosition = _openParam?.AdPopPosition ?? AdPopPosition.None;
		if (adPopPosition == AdPopPosition.None)
		{
			((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
			RequestGrant(1, PlayCloseAndFly);
			return;
		}
		if (FreeDoubleRewardPolicy.CanClaimWithoutAd())
		{
			_openParam.IsFreeDoubleReward = true;
			RequestGrant(2, delegate
			{
				FreeDoubleRewardPolicy.MarkClaimedFree();
				PlayX2RewardPresentation();
			});
			return;
		}
		_openParam.IsFreeDoubleReward = false;
		EPropType ePropType = ResolveAdPropType();
		if (ePropType == EPropType.None)
		{
			((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
			RequestGrant(1, PlayCloseAndFly);
			return;
		}
		if (_openParam?.OnAdEnterTrack != null)
		{
			_openParam.OnAdEnterTrack(ePropType);
		}
		else
		{
			RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Click, adPopPosition);
		}
		bool rewardAdSucceeded = false;
		AdService.ShowRewardAd(new RewardAdRequest
		{
			PopPosition = adPopPosition,
			Callback = delegate(RewardAdResult result)
			{
				if (result.IsSuccess)
				{
					rewardAdSucceeded = true;
					RequestGrant(2, PlayX2RewardPresentation);
					if (IsChestReward())
					{
						RRDataTrack.Instance.ReportChestClaimByAd();
					}
				}
				else
				{
					((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
				}
			}
		}, delegate
		{
			if (!rewardAdSucceeded)
			{
				((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
			}
		});
	}

	private void OnGrantSuccess(ClaimRewardGrantSuccess evt)
	{
		if (evt?.Param == _openParam)
		{
			Action pendingGrantContinuation = _pendingGrantContinuation;
			_pendingGrantContinuation = null;
			pendingGrantContinuation?.Invoke();
		}
	}

	private bool IsChestReward()
	{
		if (_openParam != null)
		{
			return _openParam.ChestCycleIndex >= 0;
		}
		return false;
	}

	private void OnGrantFailed(ClaimRewardGrantFailed evt)
	{
		if (evt?.Param == null || evt.Param == _openParam)
		{
			_pendingGrantContinuation = null;
			KillCloseTween();
			Action onComplete = delegate
			{
				_openParam?.OnBeforeClose?.Invoke();
				((BaseController<UIClaimRewardView>)this).CloseView(false);
			};
			UIClaimRewardParam openParam = _openParam;
			int playMask;
			if (openParam == null || !openParam.SkipBoxAnimation)
			{
				UIClaimRewardParam openParam2 = _openParam;
				playMask = ((openParam2 == null || !openParam2.HideBgMaskOnOpen) ? 1 : 0);
			}
			else
			{
				playMask = 1;
			}
			_closeTween = PlayPanelClose(onComplete, (byte)playMask != 0);
		}
	}

	private void RequestGrant(int targetMultiplier, Action onSuccess)
	{
		if (_openParam == null)
		{
			_pendingGrantContinuation = null;
			((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
			return;
		}
		bool num;
		if (_openParam.ClaimHandler != null || _openParam.ClaimMultiplierHandler != null)
		{
			_pendingGrantContinuation = onSuccess;
			if (targetMultiplier <= 1)
			{
				Func<bool> claimHandler = _openParam.ClaimHandler;
				if (claimHandler != null)
				{
					num = claimHandler();
					goto IL_0080;
				}
			}
			else
			{
				Func<int, bool, bool> claimMultiplierHandler = _openParam.ClaimMultiplierHandler;
				if (claimMultiplierHandler != null)
				{
					num = claimMultiplierHandler(targetMultiplier, _openParam.IsFreeDoubleReward);
					goto IL_0080;
				}
			}
			goto IL_00b0;
		}
		_pendingGrantContinuation = onSuccess;
		((BaseController<UIClaimRewardView>)this).ExecuteCommand<ClaimRewardGrantCommand>((object)new ClaimRewardGrantRequest
		{
			Param = _openParam,
			TargetMultiplier = targetMultiplier
		});
		return;
		IL_0080:
		if (num)
		{
			_openParam.OnClaimSucceeded?.Invoke();
			Action pendingGrantContinuation = _pendingGrantContinuation;
			_pendingGrantContinuation = null;
			pendingGrantContinuation?.Invoke();
			return;
		}
		goto IL_00b0;
		IL_00b0:
		_pendingGrantContinuation = null;
		((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
	}

	private void HandleHasReward()
	{
		try
		{
			if (HasRewardToClaim())
			{
				_preInstantiateRewardCoroutineId = GameApp.Coroutine.Run(PreInstantiateRewardCoroutine());
			}
		}
		catch (Exception ex)
		{
			Log.LogError("[UIClaimRewardController]", "宝箱 lead delay 回调启动道具飞行动画失败: {0}", new object[1] { ex });
		}
	}

	private IEnumerator PreInstantiateRewardCoroutine()
	{
		yield return (object)new WaitForSeconds(0.1f);
		InstantiateReward();
		SetHLayoutState(enable: false);
		List<RectTransform> list = new List<RectTransform>();
		foreach (UIPropItemTemplateController propItem in _propItems)
		{
			if ((Object)(object)propItem.RootRect != (Object)null)
			{
				list.Add(propItem.RootRect);
			}
		}
		_propFlySequence = ((BaseController<UIClaimRewardView>)this).view.PlayPropFlyAnimation(list, _hasNonConsumableProp);
		_preInstantiateRewardCoroutineId = -1;
		((BaseController<UIClaimRewardView>)this).view.SetClaimButtonsInteractable(interactable: true);
	}

	private void PlayX2RewardPresentation()
	{
		AudioService.PlayUiSound(AudioKeys.RewardDouble.Value);
		List<RectTransform> list = new List<RectTransform>(_propItems.Count);
		foreach (UIPropItemTemplateController propItem in _propItems)
		{
			if ((Object)(object)propItem?.PropCountRect != (Object)null)
			{
				list.Add(propItem.PropCountRect);
			}
		}
		((BaseController<UIClaimRewardView>)this).view.PlayX2Particles(list, PlayX2RewardItemPresentation);
	}

	private void PlayX2RewardItemPresentation()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		Sequence x2RewardSequence = _x2RewardSequence;
		if (x2RewardSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)x2RewardSequence, false);
		}
		float num = 0f;
		foreach (UIPropItemTemplateController propItem in _propItems)
		{
			object obj;
			if (propItem == null)
			{
				obj = null;
			}
			else
			{
				Animation itemAnim = propItem.ItemAnim;
				obj = ((itemAnim != null) ? itemAnim.GetClip("anim_propitemtemplate_gp1") : null);
			}
			AnimationClip val = (AnimationClip)obj;
			if (!((Object)(object)val == (Object)null))
			{
				propItem.ItemAnim.Play("anim_propitemtemplate_gp1");
				num = Mathf.Max(num, val.length);
			}
		}
		_x2RewardSequence = DOTween.Sequence();
		TweenSettingsExtensions.AppendInterval(_x2RewardSequence, 0.1f);
		TweenSettingsExtensions.AppendCallback(_x2RewardSequence, (TweenCallback)delegate
		{
			RefreshRewardPropCounts(2);
		});
		TweenSettingsExtensions.AppendInterval(_x2RewardSequence, Mathf.Max(0f, num - 0.1f));
		TweenSettingsExtensions.OnComplete<Sequence>(_x2RewardSequence, (TweenCallback)delegate
		{
			PlayCloseAndFly();
		});
	}

	private void PlayCloseAndFly()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		Sequence propFlySequence = _propFlySequence;
		if (propFlySequence != null)
		{
			TweenExtensions.Kill((Tween)(object)propFlySequence, false);
		}
		_propFlySequence = null;
		SnapPropItemsToTarget();
		bool num = _openParam != null && _openParam.PropFlyTargetPos != Vector2.zero && _propItems.Count > 0;
		KillCloseTween();
		if (num)
		{
			List<RectTransform> rects = new List<RectTransform>();
			List<Animation> list = new List<Animation>();
			foreach (UIPropItemTemplateController propItem in _propItems)
			{
				if (propItem != null && !((Object)(object)propItem.RootRect == (Object)null))
				{
					rects.Add(propItem.RootRect);
					list.Add(propItem.ItemAnim);
				}
			}
			UIClaimRewardParam openParam = _openParam;
			int playMask;
			if (openParam == null || !openParam.SkipBoxAnimation)
			{
				UIClaimRewardParam openParam2 = _openParam;
				playMask = ((openParam2 == null || !openParam2.HideBgMaskOnOpen) ? 1 : 0);
			}
			else
			{
				playMask = 1;
			}
			_closeTween = PlayPanelClose(null, (byte)playMask != 0);
			_openParam?.OnFlyStart?.Invoke();
			_propFlySequence = ((BaseController<UIClaimRewardView>)this).view.PlayPropsFlyToTarget(rects, list, _openParam.PropFlyTargetPos, delegate
			{
				_openParam?.OnBeforeClose?.Invoke();
				InvokeClosedAndCloseView();
			}, delegate
			{
				AudioService.PlayUiSound(AudioKeys.ItemAdd.Value);
				VibrateUtility.VibrateRewardFlyIn(rects.Count, "UIClaimReward-" + (_openParam?.RewardReasonId ?? "未知来源") + "道具抵达按钮");
				_openParam?.OnFirstPropArrive?.Invoke();
			});
		}
		else
		{
			Action onComplete = delegate
			{
				_openParam?.OnBeforeClose?.Invoke();
				InvokeClosedAndCloseView();
			};
			UIClaimRewardParam openParam3 = _openParam;
			int playMask2;
			if (openParam3 == null || !openParam3.SkipBoxAnimation)
			{
				UIClaimRewardParam openParam4 = _openParam;
				playMask2 = ((openParam4 == null || !openParam4.HideBgMaskOnOpen) ? 1 : 0);
			}
			else
			{
				playMask2 = 1;
			}
			_closeTween = PlayPanelClose(onComplete, (byte)playMask2 != 0);
		}
	}

	private Tween PlayPanelClose(Action onComplete, bool playMask = true)
	{
		_openParam?.OnCloseStarted?.Invoke();
		return ((BaseController<UIClaimRewardView>)this).view.PlayPanelClose(onComplete, playMask, _openParam?.SkipBoxAnimation ?? false);
	}

	private void InvokeClosedAndCloseView()
	{
		_openParam?.OnClosed?.Invoke();
		((BaseController<UIClaimRewardView>)this).CloseView(false);
	}

	private void InstantiateReward(int multiplier = 1)
	{
		ClearPropItems();
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.HLayout == (Object)null)
		{
			return;
		}
		((BaseController<UIClaimRewardView>)this).view.GoPropList.gameObject.SetActive(true);
		_hasNonConsumableProp = false;
		_hasPropType2 = false;
		Dictionary<int, int> unifiedRewardSource = GetUnifiedRewardSource(multiplier);
		if (unifiedRewardSource == null || unifiedRewardSource.Count == 0)
		{
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		List<(int, int)> list = new List<(int, int)>();
		List<(int, int)> list2 = new List<(int, int)>();
		foreach (KeyValuePair<int, int> item in unifiedRewardSource)
		{
			PropConfig propConfig = propModelProxy?.GetPropConfig(item.Key);
			if (propConfig != null && IsNonConsumable(propConfig))
			{
				list2.Add((item.Key, item.Value));
			}
			else
			{
				list.Add((item.Key, item.Value));
			}
		}
		int num = list.Count + list2.Count;
		int num2 = Mathf.Min(Mathf.Max(0, (num - list2.Count) / 2), list.Count);
		_orderedRewardItems = new List<(int, int)>();
		for (int i = 0; i < num2; i++)
		{
			_orderedRewardItems.Add(list[i]);
		}
		_orderedRewardItems.AddRange(list2);
		for (int j = num2; j < list.Count; j++)
		{
			_orderedRewardItems.Add(list[j]);
		}
		foreach (var orderedRewardItem in _orderedRewardItems)
		{
			TryAddPropItem(orderedRewardItem.propId, orderedRewardItem.count, propModelProxy);
		}
		((BaseController<UIClaimRewardView>)this).view.PropItemTemplate.SetActive(false);
	}

	private Dictionary<int, int> GetUnifiedRewardSource(int multiplier)
	{
		if (_openParam?.RewardItems != null && _openParam.RewardItems.Count > 0)
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>(_openParam.RewardItems.Count);
			{
				foreach (KeyValuePair<int, int> rewardItem in _openParam.RewardItems)
				{
					dictionary[rewardItem.Key] = rewardItem.Value * multiplier;
				}
				return dictionary;
			}
		}
		Dictionary<int, int> rewardConfigItems = GetRewardConfigItems();
		bool flag = _openParam?.ExtraRewardItems != null && _openParam.ExtraRewardItems.Count > 0;
		if (rewardConfigItems == null && !flag)
		{
			return null;
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		if (rewardConfigItems != null)
		{
			foreach (KeyValuePair<int, int> item in rewardConfigItems)
			{
				dictionary2[item.Key] = item.Value * multiplier;
			}
		}
		if (flag)
		{
			foreach (KeyValuePair<int, int> extraRewardItem in _openParam.ExtraRewardItems)
			{
				if (rewardConfigItems == null || !rewardConfigItems.ContainsKey(extraRewardItem.Key))
				{
					dictionary2[extraRewardItem.Key] = extraRewardItem.Value * multiplier;
				}
			}
		}
		return dictionary2;
	}

	private void TryAddPropItem(int propId, int count, PropModelProxy propProxy)
	{
		PropConfig propConfig = propProxy?.GetPropConfig(propId);
		if (propConfig != null && IsNonConsumable(propConfig))
		{
			_hasNonConsumableProp = true;
		}
		WidgetHandle widgetHandle = ((BaseController<UIClaimRewardView>)this).view.AttachWidget<UIPropItemTemplateView>(((BaseController<UIClaimRewardView>)this).view.PropItemTemplate, ((Component)((BaseController<UIClaimRewardView>)this).view.HLayout).transform);
		if (widgetHandle != null)
		{
			UIPropItemTemplateController uIPropItemTemplateController = new UIPropItemTemplateController(widgetHandle.GetView<UIPropItemTemplateView>());
			uIPropItemTemplateController.BindHandle(widgetHandle);
			widgetHandle.WithController<UIPropItemTemplateController>(uIPropItemTemplateController);
			uIPropItemTemplateController.Render(propId, count);
			uIPropItemTemplateController.SetActive(active: true);
			_propItems.Add(uIPropItemTemplateController);
		}
	}

	private void RefreshRewardPropCounts(int multiplier)
	{
		if (_propItems.Count != 0 && _orderedRewardItems != null && _orderedRewardItems.Count != 0)
		{
			int num = Mathf.Min(_orderedRewardItems.Count, _propItems.Count);
			for (int i = 0; i < num; i++)
			{
				int propCount = _orderedRewardItems[i].count * multiplier;
				_propItems[i].SetPropCount(propCount);
			}
		}
	}

	private void SnapPropItemsToTarget()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (_propItems.Count == 0)
		{
			return;
		}
		int num = Mathf.Min(_propItems.Count, 3);
		if (num == 0)
		{
			return;
		}
		Vector2[] array = ((BaseController<UIClaimRewardView>)this).view.CalcPropTargets(num, _hasNonConsumableProp);
		if (array == null)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			RectTransform val = _propItems[i]?.RootRect;
			if (!((Object)(object)val == (Object)null))
			{
				((Component)val).gameObject.SetActive(true);
				val.anchoredPosition = array[i];
			}
		}
	}

	private bool HasRewardToClaim()
	{
		UIClaimRewardParam openParam = _openParam;
		if (openParam == null || openParam.RewardId <= 0)
		{
			if (_openParam?.RewardItems != null)
			{
				return _openParam.RewardItems.Count > 0;
			}
			return false;
		}
		return true;
	}

	private bool HasAnimalReward()
	{
		return false;
	}

	private void SetupButtonVisibility()
	{
		bool flag = HasRewardToClaim();
		bool useContinueClaimButton = UseContinueClaimButton;
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnContinue != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnContinue).gameObject.SetActive(useContinueClaimButton);
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnOk != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnOk).gameObject.SetActive(!flag);
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaim != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnClaim).gameObject.SetActive(flag && !useContinueClaimButton);
		}
		bool flag2 = flag && (_openParam?.EnableClaimX2 ?? false);
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaimX2 != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnClaimX2).gameObject.SetActive(flag2);
		}
		if (flag2)
		{
			((BaseController<UIClaimRewardView>)this).view.RefreshClaimX2Layout();
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaim != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnClaim).GetComponent<Image>();
		}
		if ((Object)(object)((BaseController<UIClaimRewardView>)this).view.btnClaimX2 != (Object)null)
		{
			((Component)((BaseController<UIClaimRewardView>)this).view.btnClaimX2).GetComponent<Image>();
		}
	}

	private void TryReportAdRewardEnterShow()
	{
		if (_openParam != null && _openParam.EnableClaimX2 && HasRewardToClaim() && _openParam.AdPopPosition != AdPopPosition.None && !FreeDoubleRewardPolicy.CanClaimWithoutAd() && ResolveAdPropType() != EPropType.None)
		{
			RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Show, _openParam.AdPopPosition);
		}
	}

	private void ClearPropItems()
	{
		foreach (UIPropItemTemplateController propItem in _propItems)
		{
			propItem?.Dispose();
		}
		_propItems.Clear();
	}

	private void SetHLayoutState(bool enable)
	{
		if (!((Object)(object)((BaseController<UIClaimRewardView>)this).view.HLayout == (Object)null))
		{
			LayoutGroup component = ((Component)((BaseController<UIClaimRewardView>)this).view.HLayout).GetComponent<LayoutGroup>();
			if ((Object)(object)component != (Object)null)
			{
				((Behaviour)component).enabled = enable;
			}
			ContentSizeFitter component2 = ((Component)((BaseController<UIClaimRewardView>)this).view.HLayout).GetComponent<ContentSizeFitter>();
			if ((Object)(object)component2 != (Object)null)
			{
				((Behaviour)component2).enabled = enable;
			}
		}
	}

	private void ConfigureRankRewardTitle(string defaultTitleKey)
	{
		UIClaimRewardParam openParam = _openParam;
		int num = ((openParam != null && openParam.IsRankReward) ? RankFunctionResolver.GetRankRewardClaim() : 0);
		((BaseController<UIClaimRewardView>)this).view.SetRankType1Visible(num == 1);
		((BaseController<UIClaimRewardView>)this).view.SetRankType2Visible(num == 2);
		((BaseController<UIClaimRewardView>)this).view.SetRankType3Visible(num == 3);
		((BaseController<UIClaimRewardView>)this).view.SetRankType4Visible(num == 4);
		switch (num)
		{
		case 1:
			((BaseController<UIClaimRewardView>)this).view.SetCongratulationsText(string.Empty);
			((BaseController<UIClaimRewardView>)this).view.SetRankType1LocalizedText("Leaderboard_Reward_Congrats_1", "Leaderboard_Reward_Congrats_2", _openParam.RankRewardRank);
			break;
		case 2:
			((BaseController<UIClaimRewardView>)this).view.SetCongratulationsText(string.Empty);
			((BaseController<UIClaimRewardView>)this).view.SetRankType2LocalizedText("Treasure_Title", "Leaderboard_Reward_Toast", _openParam.RankRewardRank);
			break;
		case 3:
			((BaseController<UIClaimRewardView>)this).view.SetCongratulationsText(string.Empty);
			((BaseController<UIClaimRewardView>)this).view.SetRankType3LocalizedText("Leaderboard_Reward_Congrats_1", "Leaderboard_Reward_Congrats_2", _openParam.RankRewardRank);
			break;
		case 4:
			((BaseController<UIClaimRewardView>)this).view.SetCongratulationsText(string.Empty);
			((BaseController<UIClaimRewardView>)this).view.SetRankType4LocalizedText("Treasure_Title", "Leaderboard_Reward_Toast", _openParam.RankRewardRank);
			break;
		default:
			((BaseController<UIClaimRewardView>)this).view.SetCongratulationsText(string.Empty);
			break;
		case 0:
			((BaseController<UIClaimRewardView>)this).view.SetCongratulationsLocalized(defaultTitleKey);
			break;
		}
	}

	private void KillCloseTween()
	{
		Tween closeTween = _closeTween;
		if (closeTween != null)
		{
			TweenExtensions.Kill(closeTween, false);
		}
		_closeTween = null;
	}

	private static void RemoveButtonDisabledTint(Button btn)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)btn == (Object)null))
		{
			ColorBlock colors = ((Selectable)btn).colors;
			((ColorBlock)(ref colors)).disabledColor = ((ColorBlock)(ref colors)).normalColor;
			((Selectable)btn).colors = colors;
		}
	}

	private void ReplaceBoxSpineData(string skeletonDataAssetPath, string skinName = null)
	{
		if (!string.IsNullOrEmpty(skeletonDataAssetPath))
		{
			SkeletonDataAsset val = GameApp.Res.LoadAsset<SkeletonDataAsset>(skeletonDataAssetPath, (string)null);
			if (!((Object)(object)val == (Object)null))
			{
				((BaseController<UIClaimRewardView>)this).view.ReplaceBoxSkeletonData(val, skinName);
			}
		}
	}

	private EPropType ResolveAdPropType()
	{
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		UIClaimRewardParam openParam = _openParam;
		if (openParam != null && openParam.RewardId > 0)
		{
			EPropType ePropType = propModelProxy?.GetRewardPropType(_openParam.RewardId) ?? EPropType.None;
			if (ePropType != EPropType.None)
			{
				return ePropType;
			}
		}
		Dictionary<int, int> dictionary = _openParam?.RewardItems ?? GetRewardConfigItems();
		if (dictionary != null)
		{
			foreach (KeyValuePair<int, int> item in dictionary)
			{
				EPropType ePropType2 = propModelProxy?.GetRewardPropType(item.Key) ?? EPropType.None;
				if (ePropType2 != EPropType.None)
				{
					return ePropType2;
				}
			}
		}
		return EPropType.Coin;
	}

	private Dictionary<int, int> GetRewardConfigItems()
	{
		UIClaimRewardParam openParam = _openParam;
		if (openParam != null && openParam.RewardId <= 0)
		{
			return null;
		}
		return (GameApp.Config.GetTable<ExampleTables>()?.TbRewardConfig?.Get(_openParam.RewardId))?.get;
	}

	private static bool IsNonConsumable(PropConfig propCfg)
	{
		return false;
	}

	public override void OnEscape()
	{
		HasRewardToClaim();
	}
}
