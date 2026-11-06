using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.DataTrack;
using RotateRings.Ad;
using RotateRings.Chest;
using RotateRings.DailyFirstWin;
using RotateRings.Model;
using RotateRings.RewardedLuckyDraw;
using RotateRings.Services;
using RotateRings.Tool;
using RotateRings.UI.ClaimReward;
using RotateRings.UI.LevelEnd;
using RotateRings.UI.Rating;
using Rotaterings.Rank;
using UniLog;
using UnityEngine;
using XingYun.UI.Controller;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.RewardedLuckyDraw;

public static class RewardedLuckyDrawService
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass21_0
	{
		public string id;

		public RewardedLuckyDrawRequest request;

		internal bool _003COpenClaimRewardAsync_003Eb__2()
		{
			return ReissueRewardService.GrantTransactionById(id, 1);
		}

		internal void _003COpenClaimRewardAsync_003Eb__3()
		{
			if (!ReissueRewardService.GrantTransactionById(id, 1))
			{
				Log.LogError("[RewardedLuckyDraw]", "抽奖奖励自动入账失败，保留事务供重试或补发 id={0}", new object[1] { id });
			}
		}

		internal void _003COpenClaimRewardAsync_003Eb__4()
		{
			IController val = default(IController);
			if (IsCurrent(request) && GameApp.UI.IsOpen(request.UIPath, ref val) && val == request.Controller && val is UIRewardedLuckyDrawWheelController uIRewardedLuckyDrawWheelController)
			{
				uIRewardedLuckyDrawWheelController.HideMaskForClaimClose();
			}
		}

		internal void _003COpenClaimRewardAsync_003Eb__5()
		{
			FinishClaim(request);
		}

		internal bool _003COpenClaimRewardAsync_003Eb__0()
		{
			if (IsCurrent(request))
			{
				return !GameApp.UI.IsOpen("UILevelAdPop");
			}
			return true;
		}

		internal void _003COpenClaimRewardAsync_003Eb__1()
		{
			FinishClaim(request);
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003COpenClaimRewardAsync_003Ed__21 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public RewardedLuckyDrawRequest request;

		public Vector2 propFlyTargetPos;

		public Action onNoBoxOpenStarted;

		public Action hideContent;

		private _003C_003Ec__DisplayClass21_0 _003C_003E8__1;

		private UIClaimRewardParam _003CclaimParam_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0275: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_030f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0314: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02da: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				Dictionary<int, int> rewardItems = default(Dictionary<int, int>);
				if ((uint)num > 1u)
				{
					_003C_003E8__1 = new _003C_003Ec__DisplayClass21_0();
					_003C_003E8__1.request = request;
					_003C_003E8__1.id = Proxy.Data.PendingTransactionId;
					rewardItems = new Dictionary<int, int> { [Proxy.Data.PendingPropType] = Proxy.Data.PendingCount };
				}
				try
				{
					Awaiter val;
					UniTask val3;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__1;
							_003C_003Eu__1 = default(Awaiter);
							num = (_003C_003E1__state = -1);
							goto IL_032b;
						}
						_003CclaimParam_003E5__2 = new UIClaimRewardParam
						{
							RewardItems = rewardItems,
							PendingRewardTransactionId = _003C_003E8__1.id,
							BoxStartWorldPosition = propFlyTargetPos,
							PropFlyTargetPos = propFlyTargetPos,
							BoxPrefabLocation = "eff_gamewin_box_01_gp1",
							BoxAnimName = "01",
							SkipBoxAnimation = (_003C_003E8__1.request.PresentationType == 0),
							SkipMaskFadeIn = (_003C_003E8__1.request.PresentationType == 0),
							HideBgMaskOnOpen = true,
							EnableClaimX2 = false,
							UseContinueButton = true,
							OnNoBoxOpenStarted = ((_003C_003E8__1.request.PresentationType == 0) ? onNoBoxOpenStarted : null),
							RewardReasonId = "结算随机道具奖励",
							RewardReasonDetail = _003C_003E8__1.id,
							ClaimHandler = () => ReissueRewardService.GrantTransactionById(_003C_003E8__1.id, 1),
							OnOpen = delegate
							{
								if (!ReissueRewardService.GrantTransactionById(_003C_003E8__1.id, 1))
								{
									Log.LogError("[RewardedLuckyDraw]", "抽奖奖励自动入账失败，保留事务供重试或补发 id={0}", new object[1] { _003C_003E8__1.id });
								}
							},
							OnCloseStarted = ((_003C_003E8__1.request.PresentationType == 0) ? ((Action)delegate
							{
								IController val4 = default(IController);
								if (IsCurrent(_003C_003E8__1.request) && GameApp.UI.IsOpen(_003C_003E8__1.request.UIPath, ref val4) && val4 == _003C_003E8__1.request.Controller && val4 is UIRewardedLuckyDrawWheelController uIRewardedLuckyDrawWheelController)
								{
									uIRewardedLuckyDrawWheelController.HideMaskForClaimClose();
								}
							}) : null),
							OnClosed = delegate
							{
								FinishClaim(_003C_003E8__1.request);
							}
						};
						IController val2 = default(IController);
						if (GameApp.UI.IsOpen("UILevelWin", ref val2) && val2 is UILevelWinController uILevelWinController)
						{
							uILevelWinController.ConfigureLuckyDrawClaim(_003CclaimParam_003E5__2);
						}
						if (_003C_003E8__1.request.PresentationType != 0)
						{
							hideContent();
						}
						if (!GameApp.UI.IsOpen("UILevelAdPop"))
						{
							goto IL_0298;
						}
						val3 = UniTask.WaitUntil((Func<bool>)(() => !IsCurrent(_003C_003E8__1.request) || !GameApp.UI.IsOpen("UILevelAdPop")), (PlayerLoopTiming)8, default(CancellationToken));
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003COpenClaimRewardAsync_003Ed__21>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter);
						num = (_003C_003E1__state = -1);
					}
					((Awaiter)(ref val)).GetResult();
					goto IL_0298;
					IL_0298:
					if (IsCurrent(_003C_003E8__1.request))
					{
						val3 = GameApp.UI.OpenAsync("UIClaimReward", (UILayer)2, (object)_003CclaimParam_003E5__2, (Action)delegate
						{
							FinishClaim(_003C_003E8__1.request);
						});
						val = ((UniTask)(ref val3)).GetAwaiter();
						if (!((Awaiter)(ref val)).IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003COpenClaimRewardAsync_003Ed__21>(ref val, ref this);
							return;
						}
						goto IL_032b;
					}
					goto end_IL_006e;
					IL_032b:
					((Awaiter)(ref val)).GetResult();
					if (!GameApp.UI.IsOpen("UIClaimReward"))
					{
						FinishClaim(_003C_003E8__1.request);
					}
					_003CclaimParam_003E5__2 = null;
					end_IL_006e:;
				}
				catch (Exception ex)
				{
					Log.LogError("[RewardedLuckyDraw]", "打开领奖页失败，已预留奖励: {0}", new object[1] { ex });
					FinishClaim(_003C_003E8__1.request);
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003COpenPreparedPopupAsync_003Ed__17 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public Action onCompleted;

		private RewardedLuckyDrawRequest _003Crequest_003E5__2;

		private Awaiter _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			try
			{
				if (num == 0)
				{
					goto IL_00a2;
				}
				_003Crequest_003E5__2 = preparedRequest;
				if (_003Crequest_003E5__2 == null || !_003Crequest_003E5__2.Valid)
				{
					onCompleted?.Invoke();
				}
				else
				{
					_003Crequest_003E5__2.OnCompleted = onCompleted;
					if (false)
					{
						goto IL_00a2;
					}
					if (Proxy.HasShownToday)
					{
						Complete(_003Crequest_003E5__2);
					}
					else if (!NetUtil.IsHaveNet)
					{
						Complete(_003Crequest_003E5__2);
					}
					else
					{
						if (AdService.IsRewardReady)
						{
							goto IL_00a2;
						}
						Complete(_003Crequest_003E5__2);
					}
				}
				goto end_IL_0007;
				IL_00a2:
				try
				{
					Awaiter val2;
					if (num != 0)
					{
						UniTask val = GameApp.UI.OpenAsync(_003Crequest_003E5__2.UIPath, (UILayer)2, (object)_003Crequest_003E5__2, (Action)null);
						val2 = ((UniTask)(ref val)).GetAwaiter();
						if (!((Awaiter)(ref val2)).IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter, _003COpenPreparedPopupAsync_003Ed__17>(ref val2, ref this);
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
					if (IsCurrent(_003Crequest_003E5__2))
					{
						IController val3 = default(IController);
						if (!_003Crequest_003E5__2.Registered)
						{
							Complete(_003Crequest_003E5__2);
						}
						else if (!GameApp.UI.IsOpen(_003Crequest_003E5__2.UIPath, ref val3) || val3 != _003Crequest_003E5__2.Controller)
						{
							Complete(_003Crequest_003E5__2);
						}
						else if (!_003Crequest_003E5__2.Shown)
						{
							_003Crequest_003E5__2.Shown = true;
							Proxy.MarkShown(Session);
							RRDataTrack.Instance.ReportAdRewardEnter(RRDataTrack.AdAction.Show, AdPopPosition.RewardedLuckyDraw);
						}
					}
				}
				catch (Exception ex)
				{
					Log.LogError("[RewardedLuckyDraw]", "打开抽奖弹窗失败: {0}", new object[1] { ex });
					IController val4 = default(IController);
					if (GameApp.UI.IsOpen(_003Crequest_003E5__2.UIPath, ref val4) && val4 == _003Crequest_003E5__2.Controller)
					{
						GameApp.UI.CloseView(_003Crequest_003E5__2.UIPath, (object)null, false);
					}
					Complete(_003Crequest_003E5__2);
				}
				end_IL_0007:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Crequest_003E5__2 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Crequest_003E5__2 = null;
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

	private const string Tag = "[RewardedLuckyDraw]";

	private static RewardedLuckyDrawRequest preparedRequest;

	private static RewardedLuckyDrawWheelAnimationConfig wheelAnimationConfig;

	private static RewardedLuckyDrawProxy Proxy => GameApp.Proxy.Get<RewardedLuckyDrawProxy>();

	private static int Session => GameApp.Proxy.Get<PlayerModelProxy>().Data.ColdLaunchCount;

	private static bool HasActiveRequest
	{
		get
		{
			if (preparedRequest != null && preparedRequest.Valid)
			{
				return !preparedRequest.Completed;
			}
			return false;
		}
	}

	public static RewardedLuckyDrawWheelAnimationSettings WheelAnimationSettings
	{
		get
		{
			if (!((Object)(object)wheelAnimationConfig != (Object)null))
			{
				return RewardedLuckyDrawWheelAnimationSettings.Default;
			}
			return wheelAnimationConfig.Settings;
		}
	}

	public static bool HasPreparedPopup
	{
		get
		{
			if (!HasActiveRequest)
			{
				return false;
			}
			return true;
		}
	}

	public static void BindHostCompletion(Action onCompleted)
	{
		if (HasActiveRequest)
		{
			preparedRequest.OnHostCompleted = onCompleted;
		}
	}

	public static void Initialize()
	{
		AbortSettlement();
		wheelAnimationConfig = GameApp.Res.LoadAsset<RewardedLuckyDrawWheelAnimationConfig>(Res.GetDefaultResPath("RewardedLuckyDrawWheelAnimationConfig"), (string)null);
		if ((Object)(object)wheelAnimationConfig == (Object)null)
		{
			throw new InvalidOperationException("缺少正式转盘动画SO资源: RewardedLuckyDrawWheelAnimationConfig");
		}
		_ = wheelAnimationConfig.Settings;
		Proxy.RestoreOnColdStart();
	}

	public static void PrepareSettlement(int completedPropUses)
	{
		AbortSettlement();
		if (!Proxy.TryGetEligibleConfiguration(completedPropUses, out var config) || GameApp.Proxy.Get<ChestProxy>().IsClaimable() || DailyFirstWinService.Instance.HasFirstWinThisSession())
		{
			return;
		}
		RankModelData rankModelData = GameApp.Proxy.Get<RankModelProxy>()?.Data;
		if ((rankModelData == null || !rankModelData.IsPeriodSettlePending || RewardService.GetRewardId(rankModelData) <= 0) && !GameApp.Proxy.Get<RatingProxy>().ShouldShowRating())
		{
			if (!RewardedLuckyDrawRewardResolver.TryGetPool(config.ProbabilityGroupId, out var slots))
			{
				Log.LogError("[RewardedLuckyDraw]", "概率表缺失或六档配置非法 group={0}", new object[1] { config.ProbabilityGroupId });
				return;
			}
			preparedRequest = new RewardedLuckyDrawRequest
			{
				PresentationType = config.PresentationType,
				Slots = slots,
				Victory = Proxy.Data.VictoryCount,
				UIPath = ((config.PresentationType == 0) ? "UIRewardedLuckyDrawWheel" : "UIRewardedLuckyDrawStatic")
			};
		}
	}

	public static void OpenPreparedPopup(Action onCompleted)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		UniTaskExtensions.Forget(OpenPreparedPopupAsync(onCompleted));
	}

	[AsyncStateMachine(typeof(_003COpenPreparedPopupAsync_003Ed__17))]
	private static UniTask OpenPreparedPopupAsync(Action onCompleted)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003COpenPreparedPopupAsync_003Ed__17 _003COpenPreparedPopupAsync_003Ed__ = default(_003COpenPreparedPopupAsync_003Ed__17);
		_003COpenPreparedPopupAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003COpenPreparedPopupAsync_003Ed__.onCompleted = onCompleted;
		_003COpenPreparedPopupAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003COpenPreparedPopupAsync_003Ed__._003C_003Et__builder)).Start<_003COpenPreparedPopupAsync_003Ed__17>(ref _003COpenPreparedPopupAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003COpenPreparedPopupAsync_003Ed__._003C_003Et__builder)).Task;
	}

	public static bool RegisterPopup(RewardedLuckyDrawRequest request)
	{
		if (!IsCurrent(request) || request.Registered)
		{
			return false;
		}
		request.Registered = true;
		return true;
	}

	public static void WatchAd(RewardedLuckyDrawRequest request, Action<bool, int> onResult)
	{
		string transactionId;
		bool callbackHandled;
		if (IsCurrent(request) && !request.AdInFlight && !request.OpeningClaim)
		{
			transactionId = Proxy.LockDraw(request.Slots);
			request.AdInFlight = true;
			callbackHandled = false;
			AdService.ShowRewardAd(new RewardAdRequest
			{
				PopPosition = AdPopPosition.RewardedLuckyDraw,
				Callback = HandleAdResult
			}, delegate
			{
				HandleAdResult(RewardAdResult.Fail());
			});
		}
		void HandleAdResult(RewardAdResult result)
		{
			if (!callbackHandled)
			{
				callbackHandled = true;
				request.AdInFlight = false;
				if (!(Proxy.Data.PendingTransactionId != transactionId) && !Proxy.Data.AdCompleted)
				{
					if (!result.IsSuccess)
					{
						if (IsCurrent(request))
						{
							onResult?.Invoke(arg1: false, 0);
						}
					}
					else
					{
						int pendingSlotId = Proxy.Data.PendingSlotId;
						Proxy.CompleteAd(transactionId);
						if (IsCurrent(request))
						{
							if (Proxy.Data.PendingTransactionId != transactionId || !Proxy.Data.AdCompleted || pendingSlotId < 1 || pendingSlotId > request.Slots.Length)
							{
								onResult?.Invoke(arg1: false, 0);
							}
							else
							{
								onResult?.Invoke(arg1: true, pendingSlotId);
							}
						}
					}
				}
			}
		}
	}

	public static void OpenClaimReward(RewardedLuckyDrawRequest request, Vector2 propFlyTargetPos, Action hideContent, Action onNoBoxOpenStarted)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (IsCurrent(request) && !request.OpeningClaim && Proxy.Data.AdCompleted)
		{
			request.OpeningClaim = true;
			UniTaskExtensions.Forget(OpenClaimRewardAsync(request, propFlyTargetPos, hideContent, onNoBoxOpenStarted));
		}
	}

	[AsyncStateMachine(typeof(_003COpenClaimRewardAsync_003Ed__21))]
	private static UniTask OpenClaimRewardAsync(RewardedLuckyDrawRequest request, Vector2 propFlyTargetPos, Action hideContent, Action onNoBoxOpenStarted)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_003COpenClaimRewardAsync_003Ed__21 _003COpenClaimRewardAsync_003Ed__ = default(_003COpenClaimRewardAsync_003Ed__21);
		_003COpenClaimRewardAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003COpenClaimRewardAsync_003Ed__.request = request;
		_003COpenClaimRewardAsync_003Ed__.propFlyTargetPos = propFlyTargetPos;
		_003COpenClaimRewardAsync_003Ed__.hideContent = hideContent;
		_003COpenClaimRewardAsync_003Ed__.onNoBoxOpenStarted = onNoBoxOpenStarted;
		_003COpenClaimRewardAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003COpenClaimRewardAsync_003Ed__._003C_003Et__builder)).Start<_003COpenClaimRewardAsync_003Ed__21>(ref _003COpenClaimRewardAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003COpenClaimRewardAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private static void FinishClaim(RewardedLuckyDrawRequest request)
	{
		if (request != null && !request.Completed)
		{
			ClosePopup(request, delegate
			{
				Complete(request);
			}, playCloseAnimation: false);
		}
	}

	private static void ClosePopup(RewardedLuckyDrawRequest request, Action onClosed = null, bool playCloseAnimation = true)
	{
		IController controller = default(IController);
		if (!GameApp.UI.IsOpen(request.UIPath, ref controller) || controller != request.Controller)
		{
			onClosed?.Invoke();
		}
		else if (playCloseAnimation && controller is UIRewardedLuckyDrawWheelController uIRewardedLuckyDrawWheelController)
		{
			uIRewardedLuckyDrawWheelController.CloseWithAnimation(CloseCurrent);
		}
		else
		{
			CloseCurrent();
		}
		void CloseCurrent()
		{
			IController val = default(IController);
			if (GameApp.UI.IsOpen(request.UIPath, ref val) && val == controller)
			{
				GameApp.UI.CloseView(request.UIPath, (object)null, false);
			}
			onClosed?.Invoke();
		}
	}

	public static void DismissPopup(RewardedLuckyDrawRequest request)
	{
		if (IsCurrent(request) && !request.AdInFlight && !Proxy.Data.AdCompleted)
		{
			Proxy.Dismiss(Session);
			ClosePopup(request, delegate
			{
				Complete(request);
			});
		}
	}

	public static void OnPopupRemoved(RewardedLuckyDrawRequest request)
	{
		if (IsCurrent(request) && !request.OpeningClaim)
		{
			if (request.Registered && !request.AdInFlight)
			{
				Proxy.Dismiss(Session);
			}
			Complete(request);
		}
	}

	public static void AbortSettlement()
	{
		if (preparedRequest != null)
		{
			if (preparedRequest.Valid && !preparedRequest.Completed)
			{
				_ = preparedRequest.Shown;
			}
			if (preparedRequest.OpeningClaim)
			{
				ClosePopup(preparedRequest, null, playCloseAnimation: false);
			}
			preparedRequest.Valid = false;
			preparedRequest.OnCompleted = null;
			preparedRequest.OnHostCompleted = null;
			preparedRequest = null;
		}
	}

	private static bool IsCurrent(RewardedLuckyDrawRequest request)
	{
		if (request != null && request.Valid && !request.Completed)
		{
			return preparedRequest == request;
		}
		return false;
	}

	private static void Complete(RewardedLuckyDrawRequest request)
	{
		if (request == null || request.Completed)
		{
			return;
		}
		request.Completed = true;
		Action onCompleted = request.OnCompleted;
		Action onHostCompleted = request.OnHostCompleted;
		request.OnCompleted = null;
		request.OnHostCompleted = null;
		if (!request.Valid)
		{
			return;
		}
		try
		{
			onHostCompleted?.Invoke();
		}
		finally
		{
			onCompleted?.Invoke();
		}
	}
}
