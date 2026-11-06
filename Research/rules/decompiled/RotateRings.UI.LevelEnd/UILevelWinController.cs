using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Example.Cfg;
using Game.Common.Services;
using Game.DataTrack;
using Game.UI;
using Game.UI.Language;
using Hot.Model;
using RotateRings.Ad;
using RotateRings.Chest;
using RotateRings.Common;
using RotateRings.DailyFirstWin;
using RotateRings.DailyScore;
using RotateRings.GameWinIQ;
using RotateRings.Model;
using RotateRings.Prop;
using RotateRings.Services;
using RotateRings.UI.ClaimReward;
using RotateRings.UI.Home;
using RotateRings.UI.RewardedLuckyDraw;
using RotateRings.UI.Tips;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Core.Singletons;
using XingYun.Lang;
using XingYun.UI.Controller;
using XingYun.UI.Core;

namespace RotateRings.UI.LevelEnd;

[UIPath("UILevelWin")]
public class UILevelWinController : GameBaseController<UILevelWinView>, ILevelWinAnimatorHost
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBuildProgressBarAsync_003Ed__44 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder _003C_003Et__builder;

		public UILevelWinController _003C_003E4__this;

		public bool animateCurrentLevel;

		private CancellationToken _003CbuildCt_003E5__2;

		private int _003Ctotal_003E5__3;

		private int _003CpreLit_003E5__4;

		private CancellationToken _003CcancellationToken_003E5__5;

		private CancellationTokenSource _003ClinkedCts_003E5__6;

		private GameObject _003CtrailPrefab_003E5__7;

		private Awaiter<GameObject> _003C_003Eu__1;

		private Awaiter<List<PreStageRewardProgress>> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_0137: Unknown result type (might be due to invalid IL or missing references)
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILevelWinController uILevelWinController = _003C_003E4__this;
			try
			{
				if ((uint)num <= 1u)
				{
					goto IL_00e3;
				}
				uILevelWinController._progressBuildCts?.Cancel();
				uILevelWinController._progressBuildCts?.Dispose();
				uILevelWinController._progressBuildCts = new CancellationTokenSource();
				_003CbuildCt_003E5__2 = uILevelWinController._progressBuildCts.Token;
				uILevelWinController._progressNodes.Clear();
				UILevelWinView view = ((BaseController<UILevelWinView>)uILevelWinController).view;
				if (!((Object)(object)((view != null) ? ((UIBaseView)view).Behaviour : null) == (Object)null))
				{
					ChestProxy chestProxy = GameApp.Proxy.Get<ChestProxy>();
					_003Ctotal_003E5__3 = chestProxy.GetProgressTotalCount();
					int progressLitCount = chestProxy.GetProgressLitCount();
					_003CpreLit_003E5__4 = (animateCurrentLevel ? Mathf.Max(0, progressLitCount - 1) : progressLitCount);
					_003CcancellationToken_003E5__5 = UniTaskCancellationExtensions.GetCancellationTokenOnDestroy((Component)(object)((UIBaseView)((BaseController<UILevelWinView>)uILevelWinController).view).Behaviour);
					_003ClinkedCts_003E5__6 = CancellationTokenSource.CreateLinkedTokenSource(_003CbuildCt_003E5__2, _003CcancellationToken_003E5__5);
					goto IL_00e3;
				}
				goto end_IL_000e;
				IL_00e3:
				try
				{
					Awaiter<List<PreStageRewardProgress>> val;
					Awaiter<GameObject> val2;
					if (num != 0)
					{
						if (num == 1)
						{
							val = _003C_003Eu__2;
							_003C_003Eu__2 = default(Awaiter<List<PreStageRewardProgress>>);
							num = (_003C_003E1__state = -1);
							goto IL_0218;
						}
						val2 = uILevelWinController.LoadProgressTrailPrefabAsync(_003ClinkedCts_003E5__6.Token).GetAwaiter();
						if (!val2.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val2;
							((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CBuildProgressBarAsync_003Ed__44>(ref val2, ref this);
							return;
						}
					}
					else
					{
						val2 = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					GameObject result = val2.GetResult();
					_003CtrailPrefab_003E5__7 = result;
					if (!_003CbuildCt_003E5__2.IsCancellationRequested && !_003CcancellationToken_003E5__5.IsCancellationRequested)
					{
						UILevelWinView view2 = ((BaseController<UILevelWinView>)uILevelWinController).view;
						if (!((Object)(object)((view2 != null) ? ((UIBaseView)view2).Behaviour : null) == (Object)null))
						{
							val = ((BaseController<UILevelWinView>)uILevelWinController).view.BuildProgressNodesAsync("PreStageRewardProgressL", "PreStageRewardProgressM", "PreStageRewardProgressR", _003Ctotal_003E5__3, _003CcancellationToken_003E5__5).GetAwaiter();
							if (!val.IsCompleted)
							{
								num = (_003C_003E1__state = 1);
								_003C_003Eu__2 = val;
								((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<List<PreStageRewardProgress>>, _003CBuildProgressBarAsync_003Ed__44>(ref val, ref this);
								return;
							}
							goto IL_0218;
						}
					}
					goto end_IL_00e3;
					IL_0218:
					List<PreStageRewardProgress> result2 = val.GetResult();
					if (!_003CbuildCt_003E5__2.IsCancellationRequested && !_003CcancellationToken_003E5__5.IsCancellationRequested)
					{
						UILevelWinView view3 = ((BaseController<UILevelWinView>)uILevelWinController).view;
						if (!((Object)(object)((view3 != null) ? ((UIBaseView)view3).Behaviour : null) == (Object)null))
						{
							uILevelWinController._progressPreLitCount = _003CpreLit_003E5__4;
							uILevelWinController._progressNodes.AddRange(result2);
							for (int i = 0; i < uILevelWinController._progressNodes.Count; i++)
							{
								uILevelWinController._progressNodes[i].SetTrailPrefab(_003CtrailPrefab_003E5__7);
							}
							if (((BaseController<UILevelWinView>)uILevelWinController).view.Root.activeInHierarchy)
							{
								uILevelWinController.ApplyProgressBarLitState();
							}
						}
					}
					end_IL_00e3:;
				}
				finally
				{
					if (num < 0 && _003ClinkedCts_003E5__6 != null)
					{
						((IDisposable)_003ClinkedCts_003E5__6).Dispose();
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CbuildCt_003E5__2 = default(CancellationToken);
				_003CcancellationToken_003E5__5 = default(CancellationToken);
				_003ClinkedCts_003E5__6 = null;
				_003CtrailPrefab_003E5__7 = null;
				((AsyncUniTaskMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CbuildCt_003E5__2 = default(CancellationToken);
			_003CcancellationToken_003E5__5 = default(CancellationToken);
			_003ClinkedCts_003E5__6 = null;
			_003CtrailPrefab_003E5__7 = null;
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
	private struct _003CLoadProgressTrailPrefabAsync_003Ed__46 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<GameObject> _003C_003Et__builder;

		public CancellationToken cancellationToken;

		private Awaiter<GameObject> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			GameObject result;
			try
			{
				try
				{
					Awaiter<GameObject> val;
					if (num != 0)
					{
						val = UniTaskExtensions.AttachExternalCancellation<GameObject>(GameApp.Res.LoadAssetAsync<GameObject>("eff_uiwin_trail_01", "UILevelWin_Effect"), cancellationToken).GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = val;
							_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<GameObject>, _003CLoadProgressTrailPrefabAsync_003Ed__46>(ref val, ref this);
							return;
						}
					}
					else
					{
						val = _003C_003Eu__1;
						_003C_003Eu__1 = default(Awaiter<GameObject>);
						num = (_003C_003E1__state = -1);
					}
					result = val.GetResult();
				}
				catch (OperationCanceledException)
				{
					result = null;
				}
				catch (Exception)
				{
					result = null;
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

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CTryStartEntranceWhenReady_003Ed__50 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public UILevelWinController _003C_003E4__this;

		private CancellationToken _003CcancellationToken_003E5__2;

		private Awaiter<bool> _003C_003Eu__1;

		private void MoveNext()
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILevelWinController uILevelWinController = _003C_003E4__this;
			try
			{
				Awaiter<bool> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_00b0;
				}
				if (num == 1)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_0150;
				}
				UILevelWinView view = ((BaseController<UILevelWinView>)uILevelWinController).view;
				UniTask val2;
				if (!((Object)(object)((view != null) ? ((UIBaseView)view).Behaviour : null) == (Object)null))
				{
					_003CcancellationToken_003E5__2 = uILevelWinController._levelWinAnimator.CancellationToken;
					val2 = LevelWinPreloadService.WaitUntilReady(_003CcancellationToken_003E5__2);
					val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CTryStartEntranceWhenReady_003Ed__50>(ref val, ref this);
						return;
					}
					goto IL_00b0;
				}
				goto end_IL_000e;
				IL_00b0:
				if (!val.GetResult() && !_003CcancellationToken_003E5__2.IsCancellationRequested)
				{
					UILevelWinView view2 = ((BaseController<UILevelWinView>)uILevelWinController).view;
					if (!((Object)(object)((view2 != null) ? ((UIBaseView)view2).Behaviour : null) == (Object)null))
					{
						val2 = UniTaskExtensions.AttachExternalCancellation(uILevelWinController._progressBarBuildTask, _003CcancellationToken_003E5__2);
						val = ((UniTask)(ref val2)).SuppressCancellationThrow().GetAwaiter();
						if (!val.IsCompleted)
						{
							num = (_003C_003E1__state = 1);
							_003C_003Eu__1 = val;
							((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).AwaitUnsafeOnCompleted<Awaiter<bool>, _003CTryStartEntranceWhenReady_003Ed__50>(ref val, ref this);
							return;
						}
						goto IL_0150;
					}
				}
				goto end_IL_000e;
				IL_0150:
				if (!val.GetResult() && !_003CcancellationToken_003E5__2.IsCancellationRequested)
				{
					UILevelWinView view3 = ((BaseController<UILevelWinView>)uILevelWinController).view;
					if (!((Object)(object)((view3 != null) ? ((UIBaseView)view3).Behaviour : null) == (Object)null))
					{
						((BaseController<UILevelWinView>)uILevelWinController).view.Root.SetActive(true);
						uILevelWinController.ApplyProgressBarLitState();
						((BaseController<UILevelWinView>)uILevelWinController).view.PrepareEntrance();
						uILevelWinController._levelWinAnimator.Play(uILevelWinController.levelWinAnimation, uILevelWinController._animationSettings);
					}
				}
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003CcancellationToken_003E5__2 = default(CancellationToken);
				((AsyncUniTaskVoidMethodBuilder)(ref _003C_003Et__builder)).SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003CcancellationToken_003E5__2 = default(CancellationToken);
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

	private const string TAG = "[UILevelWinController]";

	private const string BoxPrefabLocation = "eff_gamewin_box_01_gp1";

	private const string FinalProgressEffectPrefabLocation = "eff_uiwin_boom_01";

	private const float FinalProgressEffectDuration = 0.5f;

	private const string TitleKeyPrefix = "LevelComplete_Title";

	private const int TitleCount = 6;

	private const string CheerUpKeyPrefix = "LevelComplete_CheerUp_";

	private const int CheerUpCount = 10;

	private const string K_LevelLeft = "chest_progress";

	private const string K_ChestTitle = "chest_title";

	private const string K_CanClaim = "chest_unlocked";

	private const string FallbackEvaluation = "Well Done!";

	private const string FallbackCheerUpText = "Great job!";

	private const string GeniusPassTextKey = "Text_Genius";

	private const string FallbackLevelLeftFmt = "Only {0} levels left!";

	private const string FallbackChestTitle = "Level Rewards";

	private UILevelWinParam _param;

	private LevelWinAnimator _levelWinAnimator;

	private LevelWinAnimationSettings _animationSettings;

	private int levelWinAnimation;

	private int _finalProgressEffectTimerId = -1;

	private readonly List<PreStageRewardProgress> _progressNodes = new List<PreStageRewardProgress>();

	private bool _claimOpened;

	private bool _contentShown;

	private bool _postSettlementStarted;

	private bool _postSettlementPending;

	private bool _isClosing;

	private bool _pendingNextBtnEffect;

	private bool _pendingEntrance;

	private bool _nextButtonLocked;

	private bool rewardedLuckyDrawProgressDone;

	private int _closeTimerId = -1;

	private UniTask _progressBarBuildTask = UniTask.CompletedTask;

	private CancellationTokenSource _progressBuildCts;

	private int _progressPreLitCount;

	private DailyScoreSettlementSnapshot _dailyScoreSnapshot = DailyScoreSettlementSnapshot.Empty;

	private GameWinIQSettlementSnapshot _gameWinIqSnapshot = GameWinIQSettlementSnapshot.Empty;

	private static readonly string[] SettlementMaskPopupPaths = new string[7] { "UIClaimReward", "UIDailyFirstWin", "UIRating", "UIRatingFeedback", "UILevelAdPop", "UIRewardedLuckyDrawWheel", "UIRewardedLuckyDrawStatic" };

	private bool UseScoreScrollAnim
	{
		get
		{
			if (levelWinAnimation != 1)
			{
				return levelWinAnimation == 2;
			}
			return true;
		}
	}

	int ILevelWinAnimatorHost.SettlementScore => _param.Score;

	GameWinIQSettlementSnapshot ILevelWinAnimatorHost.IqSnapshot => _gameWinIqSnapshot;

	internal GameObject InstanceRoot
	{
		get
		{
			UILevelWinView view = ((BaseController<UILevelWinView>)this).view;
			if (!((Object)(object)((view != null) ? ((UIBaseView)view).Behaviour : null) != (Object)null))
			{
				return null;
			}
			return ((Component)((UIBaseView)((BaseController<UILevelWinView>)this).view).Behaviour).gameObject;
		}
	}

	public UILevelWinController(UILevelWinView view)
		: base(view)
	{
	}//IL_001a: Unknown result type (might be due to invalid IL or missing references)
	//IL_001f: Unknown result type (might be due to invalid IL or missing references)


	public override void OnRegister(object param = null)
	{
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		levelWinAnimation = FeatureConfig.LevelEnd?.LevelWinAnimation ?? 0;
		_levelWinAnimator?.Dispose();
		ResetViewState();
		if ((Object)(object)((BaseController<UILevelWinView>)this).view.Trans_Bottom != (Object)null)
		{
			((Component)((BaseController<UILevelWinView>)this).view.Trans_Bottom).gameObject.SetActive(true);
		}
		((BaseController<UILevelWinView>)this).view.Root.SetActive(false);
		LevelModelProxy levelModelProxy = GameApp.Proxy.Get<LevelModelProxy>();
		ChestProxy chestProxy = GameApp.Proxy.Get<ChestProxy>();
		ChestServices.Refresh(chestProxy);
		_param = (param as UILevelWinParam) ?? new UILevelWinParam();
		NormalizeLevelSnapshot(_param, levelModelProxy);
		GameMode gameMode = ((!_param.IsMainToEndlessTransition) ? GameInfo.CurrentGameMode : GameMode.Main);
		((BaseController<UILevelWinView>)this).view.SetSettlementDifficulty(LevelDifficultyUtil.GetDifficulty(gameMode, _param.CompletedLevel));
		_param.Score = ((_param.Score > 0) ? _param.Score : (levelModelProxy?.GetCurrentScore() ?? 0));
		UILevelWinParam param2 = _param;
		if (param2.OnNext == null)
		{
			param2.OnNext = delegate
			{
				Gameplay.Active?.GoToNextLevel();
			};
		}
		((BaseController<UILevelWinView>)this).ListenEvent<ChestClaimed>((Action<ChestClaimed>)OnChestClaimed);
		((BaseController<UILevelWinView>)this).ListenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		Button btn_Gift = ((BaseController<UILevelWinView>)this).view.Btn_Gift;
		if (btn_Gift != null)
		{
			((UnityEvent)btn_Gift.onClick).AddListener(new UnityAction(OnGiftClick));
		}
		((BaseController<UILevelWinView>)this).view.InitBtnOK(playIdleAnimation: false);
		UIHomeBtnOKView btnOKView = ((BaseController<UILevelWinView>)this).view.BtnOKView;
		if (btnOKView != null)
		{
			Button btnOK = btnOKView.btnOK;
			if (btnOK != null)
			{
				((UnityEvent)btnOK.onClick).AddListener(new UnityAction(OnNextClick));
			}
		}
		((BaseController<UILevelWinView>)this).view.ShowGiftButton(show: true);
		((BaseController<UILevelWinView>)this).view.InitBestScore();
		_dailyScoreSnapshot = (_param.IsVictorySnapshot ? DailyScoreService.Instance.GetLastSettlementSnapshot() : DailyScoreSettlementSnapshot.Empty);
		_gameWinIqSnapshot = ((!_param.IsVictorySnapshot) ? GameWinIQSettlementSnapshot.Empty : (_param.GameWinIqSnapshot.ShouldShowUi ? _param.GameWinIqSnapshot : GameWinIQService.Instance.GetLastSettlementSnapshot()));
		((BaseController<UILevelWinView>)this).view.ApplyGameWinIqPassLayout(_gameWinIqSnapshot.ShouldShowUi);
		((BaseController<UILevelWinView>)this).view.RefreshScoreLabel();
		((BaseController<UILevelWinView>)this).view.RefreshRoundTipText();
		((BaseController<UILevelWinView>)this).view.SetScore((!UseScoreScrollAnim) ? _param.Score : 0);
		((BaseController<UILevelWinView>)this).view.SetEvaluation(GetRandomWellDoneText());
		ApplyPassCheerUpText();
		RefreshLevelDisplay();
		SetLevelLeftText(chestProxy.GetLevelsLeft());
		_progressBarBuildTask = BuildProgressBarAsync();
		_animationSettings = ((BaseController<UILevelWinView>)this).view.CaptureAnimationSettings(UseScoreScrollAnim);
		_levelWinAnimator = new LevelWinAnimator(this);
		_pendingEntrance = true;
	}

	private void ResetViewState()
	{
		_isClosing = false;
		_claimOpened = false;
		_contentShown = false;
		_postSettlementStarted = false;
		_postSettlementPending = false;
		_pendingNextBtnEffect = false;
		_pendingEntrance = false;
		_nextButtonLocked = false;
		rewardedLuckyDrawProgressDone = false;
		_progressNodes.Clear();
		_progressPreLitCount = 0;
		_dailyScoreSnapshot = DailyScoreSettlementSnapshot.Empty;
		_gameWinIqSnapshot = GameWinIQSettlementSnapshot.Empty;
	}

	private void NormalizeLevelSnapshot(UILevelWinParam param, LevelModelProxy levelProxy)
	{
		if (param.IsVictorySnapshot)
		{
			param.CompletedLevel = Mathf.Max(0, param.CompletedLevel);
		}
		else
		{
			param.CompletedLevel = levelProxy?.Level ?? 1;
		}
	}

	private void RefreshLevelDisplay()
	{
		int level = GameApp.Proxy.Get<LevelModelProxy>()?.Level ?? 1;
		((BaseController<UILevelWinView>)this).view.SetLevel(level);
		((BaseController<UILevelWinView>)this).view.SetRoundTipsVisible(_param.IsMainToEndlessTransition);
	}

	[AsyncStateMachine(typeof(_003CBuildProgressBarAsync_003Ed__44))]
	private UniTask BuildProgressBarAsync(bool animateCurrentLevel = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		_003CBuildProgressBarAsync_003Ed__44 _003CBuildProgressBarAsync_003Ed__ = default(_003CBuildProgressBarAsync_003Ed__44);
		_003CBuildProgressBarAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder.Create();
		_003CBuildProgressBarAsync_003Ed__._003C_003E4__this = this;
		_003CBuildProgressBarAsync_003Ed__.animateCurrentLevel = animateCurrentLevel;
		_003CBuildProgressBarAsync_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref _003CBuildProgressBarAsync_003Ed__._003C_003Et__builder)).Start<_003CBuildProgressBarAsync_003Ed__44>(ref _003CBuildProgressBarAsync_003Ed__);
		return ((AsyncUniTaskMethodBuilder)(ref _003CBuildProgressBarAsync_003Ed__._003C_003Et__builder)).Task;
	}

	private void ApplyProgressBarLitState()
	{
		UILevelWinView view = ((BaseController<UILevelWinView>)this).view;
		if (!((Object)(object)((view != null) ? ((UIBaseView)view).Behaviour : null) == (Object)null) && _progressNodes.Count != 0)
		{
			if ((Object)(object)((BaseController<UILevelWinView>)this).view.Trans_Progress != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(((BaseController<UILevelWinView>)this).view.Trans_Progress);
				Canvas.ForceUpdateCanvases();
			}
			for (int i = 0; i < _progressNodes.Count; i++)
			{
				_progressNodes[i].RefreshWidthAfterLayout();
				_progressNodes[i].SetLit(i < _progressPreLitCount);
			}
		}
	}

	[AsyncStateMachine(typeof(_003CLoadProgressTrailPrefabAsync_003Ed__46))]
	private UniTask<GameObject> LoadProgressTrailPrefabAsync(CancellationToken cancellationToken)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CLoadProgressTrailPrefabAsync_003Ed__46 _003CLoadProgressTrailPrefabAsync_003Ed__ = default(_003CLoadProgressTrailPrefabAsync_003Ed__46);
		_003CLoadProgressTrailPrefabAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<GameObject>.Create();
		_003CLoadProgressTrailPrefabAsync_003Ed__.cancellationToken = cancellationToken;
		_003CLoadProgressTrailPrefabAsync_003Ed__._003C_003E1__state = -1;
		_003CLoadProgressTrailPrefabAsync_003Ed__._003C_003Et__builder.Start<_003CLoadProgressTrailPrefabAsync_003Ed__46>(ref _003CLoadProgressTrailPrefabAsync_003Ed__);
		return _003CLoadProgressTrailPrefabAsync_003Ed__._003C_003Et__builder.Task;
	}

	private GameObject LoadFinalProgressEffectPrefab()
	{
		try
		{
			return GameApp.Res.LoadAsset<GameObject>("eff_uiwin_boom_01", "UILevelWin_Effect");
		}
		catch (Exception)
		{
			return null;
		}
	}

	private void RefreshChestDisplay()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		int levelsLeft = GameApp.Proxy.Get<ChestProxy>().GetLevelsLeft();
		SetLevelLeftText(levelsLeft);
		_progressBarBuildTask = BuildProgressBarAsync(animateCurrentLevel: false);
	}

	private void SetLevelLeftText(int levelsLeft)
	{
		if (levelsLeft <= 0)
		{
			((BaseController<UILevelWinView>)this).view.SetLevelLeftText(Localize("chest_unlocked", "Level Rewards"));
			return;
		}
		((BaseController<UILevelWinView>)this).view.SetLevelLeftText(Localize("chest_progress", "Only {0} levels left!", levelsLeft));
	}

	[AsyncStateMachine(typeof(_003CTryStartEntranceWhenReady_003Ed__50))]
	private UniTaskVoid TryStartEntranceWhenReady()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		_003CTryStartEntranceWhenReady_003Ed__50 _003CTryStartEntranceWhenReady_003Ed__ = default(_003CTryStartEntranceWhenReady_003Ed__50);
		_003CTryStartEntranceWhenReady_003Ed__._003C_003Et__builder = AsyncUniTaskVoidMethodBuilder.Create();
		_003CTryStartEntranceWhenReady_003Ed__._003C_003E4__this = this;
		_003CTryStartEntranceWhenReady_003Ed__._003C_003E1__state = -1;
		((AsyncUniTaskVoidMethodBuilder)(ref _003CTryStartEntranceWhenReady_003Ed__._003C_003Et__builder)).Start<_003CTryStartEntranceWhenReady_003Ed__50>(ref _003CTryStartEntranceWhenReady_003Ed__);
		return ((AsyncUniTaskVoidMethodBuilder)(ref _003CTryStartEntranceWhenReady_003Ed__._003C_003Et__builder)).Task;
	}

	void ILevelWinAnimatorHost.OnScorePhaseStarted()
	{
		TryPlayDailyBestScoreUi();
	}

	void ILevelWinAnimatorHost.PlayChestProgress(float duration, Action onComplete)
	{
		StartProgressGrow(duration, onComplete);
	}

	bool ILevelWinAnimatorHost.OnContentAppearStarted()
	{
		if (_contentShown)
		{
			return false;
		}
		_contentShown = true;
		RefreshLevelDisplay();
		return true;
	}

	void ILevelWinAnimatorHost.OnContentAppearCompleted()
	{
		OnContentAppearDone();
	}

	private void ApplyPassCheerUpText()
	{
		if (_gameWinIqSnapshot.ShouldShowUi && _gameWinIqSnapshot.IsPeakGenius)
		{
			((BaseController<UILevelWinView>)this).view.SetPassLocalizedKey("Text_Genius");
		}
		else
		{
			((BaseController<UILevelWinView>)this).view.SetPassText(GetRandomCheerUpText());
		}
	}

	private void TryPlayDailyBestScoreUi()
	{
		if (_param.IsVictorySnapshot)
		{
			bool flag = _dailyScoreSnapshot.ShouldShowBestUi && _dailyScoreSnapshot.ShowMode == 2;
			((BaseController<UILevelWinView>)this).view.SetBannerPointsVisible(!flag);
			if (_dailyScoreSnapshot.ShouldShowBestUi)
			{
				((BaseController<UILevelWinView>)this).view.BestScoreView?.PlayOnScorePhase(_dailyScoreSnapshot.ShowMode);
			}
		}
	}

	private void StartProgressGrow(float duration, Action onComplete)
	{
		ChestProxy chestProxy = GameApp.Proxy.Get<ChestProxy>();
		int index = chestProxy.GetAnimatingIndex();
		if (index < 0 || index >= _progressNodes.Count)
		{
			OnLuckyDrawProgressCompleted();
			onComplete?.Invoke();
			return;
		}
		if (chestProxy.IsClaimable())
		{
			LockNextButton();
		}
		_progressNodes[index].PlayAnimate(delegate
		{
			bool flag = index == _progressNodes.Count - 1 && ((BaseController<UILevelWinView>)this).view.PlayFinalProgressEffect(LoadFinalProgressEffectPrefab());
			if (chestProxy.IsClaimable())
			{
				AudioService.PlayUiSound(AudioKeys.ProgressChestUnlock.Value);
				((BaseController<UILevelWinView>)this).view.PlayBottonBoxGp1();
				if (flag)
				{
					DelayClaimUntilFinalProgressEffectComplete();
				}
				else
				{
					((BaseController<UILevelWinView>)this).ExecuteCommand<ChestClaimCommand>((object)null);
				}
			}
			else
			{
				OnLuckyDrawProgressCompleted();
			}
			onComplete?.Invoke();
		}, duration);
	}

	private void LockNextButton()
	{
		_nextButtonLocked = true;
		((BaseController<UILevelWinView>)this).view?.DisableNextButton();
	}

	private void UnlockNextButton()
	{
		_nextButtonLocked = false;
		((BaseController<UILevelWinView>)this).view?.EnableNextButton();
	}

	private void DelayClaimUntilFinalProgressEffectComplete()
	{
		if (_finalProgressEffectTimerId >= 0)
		{
			GameApp.Timer.Kill(_finalProgressEffectTimerId);
		}
		_finalProgressEffectTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UILevelWinView>)this).view).Behaviour, 0.5f, (Action)delegate
		{
			_finalProgressEffectTimerId = -1;
			((BaseController<UILevelWinView>)this).ExecuteCommand<ChestClaimCommand>((object)null);
		});
	}

	private void ShowContent()
	{
		_levelWinAnimator?.ShowContent();
	}

	private void ShowContentNoAnim()
	{
		if (!_contentShown)
		{
			_contentShown = true;
			OnContentAppearDone();
		}
	}

	private void OnContentAppearDone()
	{
		int num;
		if (!GameApp.Proxy.Get<ChestProxy>().IsClaimable())
		{
			if (_claimOpened)
			{
				num = (GameApp.UI.IsOpen("UIClaimReward") ? 1 : 0);
				if (num != 0)
				{
					goto IL_0031;
				}
			}
			else
			{
				num = 0;
			}
			((BaseController<UILevelWinView>)this).view.EnableNextButton();
			_nextButtonLocked = false;
			goto IL_004b;
		}
		num = 1;
		goto IL_0031;
		IL_0031:
		LockNextButton();
		goto IL_004b;
		IL_004b:
		RefreshLevelDisplay();
		if (num != 0)
		{
			_postSettlementPending = true;
		}
		else
		{
			StartPostSettlementPipeline();
		}
	}

	private void StartPostSettlementPipeline()
	{
		if (_postSettlementStarted)
		{
			return;
		}
		bool hasPreparedPopup = RewardedLuckyDrawService.HasPreparedPopup;
		if (hasPreparedPopup)
		{
			RewardedLuckyDrawService.BindHostCompletion(OnLuckyDrawFlowCompleted);
			LockNextButton();
		}
		if (hasPreparedPopup && !rewardedLuckyDrawProgressDone)
		{
			_postSettlementPending = true;
			return;
		}
		_postSettlementStarted = true;
		_postSettlementPending = false;
		if (_param.IsVictorySnapshot)
		{
			TryAutoClaimDailyFirstWinPendingReward();
		}
		PostSettlementPipeline.BuildAndExecute();
	}

	private void OnLuckyDrawProgressCompleted()
	{
		rewardedLuckyDrawProgressDone = true;
		if (_postSettlementPending && !_isClosing)
		{
			StartPostSettlementPipeline();
		}
	}

	private void OnLuckyDrawFlowCompleted()
	{
		if (!_isClosing && !GameApp.Proxy.Get<ChestProxy>().IsClaimable())
		{
			UnlockNextButton();
		}
	}

	private void OnChestClaimed(ChestClaimed evt)
	{
		if (!_claimOpened)
		{
			_claimOpened = true;
			for (int i = 0; i < _progressNodes.Count; i++)
			{
				_progressNodes[i].SetLit(lit: false);
			}
			if (evt.IsAutoClaim)
			{
				ShowContent();
				StartPostSettlementPipeline();
			}
			else
			{
				OpenClaimRewardUI(evt.RewardConfigId, isAutoClaim: false, evt.CycleIndex);
			}
		}
	}

	private void OpenClaimRewardUI(int rewardId, bool isAutoClaim, int chestCycleIndex)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		string transactionId = ReissueRewardService.GetChestTransactionId(chestCycleIndex);
		UIClaimRewardParam uIClaimRewardParam = new UIClaimRewardParam
		{
			RewardId = rewardId,
			BoxStartWorldPosition = GetWorldPosition((Component)(object)((BaseController<UILevelWinView>)this).view.Btn_Gift),
			PropFlyTargetPos = GetScreenPosition((Component)(object)((BaseController<UILevelWinView>)this).view.GetBtnOKTransform()),
			BoxPrefabLocation = "eff_gamewin_box_01_gp1",
			BoxAnimName = "01",
			RewardReasonId = (isAutoClaim ? "关卡宝箱自动补发" : "主关卡宝箱奖励"),
			RewardReasonDetail = (isAutoClaim ? $"第{_param.CompletedLevel}关前未领取，自动补发" : $"到达第{_param.CompletedLevel}关"),
			RewardItems = BuildRewardItems(rewardId),
			OnFlyStart = OnRewardFlyStart,
			OnFirstPropArrive = OnRewardFirstPropArrive,
			OnBoxArrive = null,
			OnBeforeClose = null,
			OnClosed = OnClaimRewardClosed,
			ChestCycleIndex = chestCycleIndex,
			PendingRewardTransactionId = transactionId,
			ClaimHandler = () => ReissueRewardService.GrantTransactionById(transactionId, 1),
			ClaimMultiplierHandler = (int multiplier, bool isFreeDoubleReward) => ReissueRewardService.GrantTransactionById(transactionId, multiplier, isFreeDoubleReward),
			EnableClaimX2 = true,
			AdPopPosition = AdPopPosition.ChestMultiReward,
			OnClaimX2ClickTrack = delegate
			{
				RRDataTrack.Instance.ReportBtnClick("关卡宝箱-多倍领奖");
			}
		};
		((BaseController<UILevelWinView>)this).view.PlayRewardDisappear();
		GameApp.UI.Open("UIClaimReward", (UILayer)2, (object)uIClaimRewardParam, (Action)null);
	}

	private void OnClaimRewardClosed()
	{
		if (_postSettlementPending)
		{
			StartPostSettlementPipeline();
		}
	}

	private static Dictionary<int, int> BuildRewardItems(int rewardId)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (rewardId > 0)
		{
			RewardConfig rewardConfig = GameApp.Config.GetTable<ExampleTables>()?.TbRewardConfig?.Get(rewardId);
			if (rewardConfig?.get != null)
			{
				foreach (KeyValuePair<int, int> item in rewardConfig.get)
				{
					dictionary[item.Key] = item.Value;
				}
			}
		}
		if (dictionary.Count == 0)
		{
			dictionary[201] = 1;
		}
		return dictionary;
	}

	public void ConfigureLuckyDrawClaim(UIClaimRewardParam claimParam)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		claimParam.PropFlyTargetPos = GetScreenPosition((Component)(object)((BaseController<UILevelWinView>)this).view.GetBtnOKTransform());
		claimParam.OnFlyStart = (Action)Delegate.Combine(claimParam.OnFlyStart, new Action(OnRewardFlyStart));
		claimParam.OnFirstPropArrive = (Action)Delegate.Combine(claimParam.OnFirstPropArrive, new Action(OnRewardFirstPropArrive));
	}

	private void OnRewardFlyStart()
	{
		((BaseController<UILevelWinView>)this).view.PlayRewardAppear();
		((BaseController<UILevelWinView>)this).view.EnsureContentVisible();
	}

	private void OnRewardFirstPropArrive()
	{
		_pendingNextBtnEffect = true;
	}

	private void TryPlayPendingNextBtnEffect()
	{
		if (_pendingNextBtnEffect)
		{
			_pendingNextBtnEffect = false;
			((BaseController<UILevelWinView>)this).view.PlayNextBtnEffect();
		}
	}

	private void OnGiftClick()
	{
		AudioService.PlayUiSound(AudioKeys.ProgressChestPop.Value);
		ChestProxy chestProxy = GameApp.Proxy.Get<ChestProxy>();
		if (chestProxy.IsClaimable())
		{
			((BaseController<UILevelWinView>)this).ExecuteCommand<ChestClaimCommand>((object)null);
		}
		else
		{
			ShowChestRewardPreview(chestProxy.GetCurrentRewardConfigId());
		}
	}

	private void ShowChestRewardPreview(int rewardId)
	{
		if ((Object)(object)((BaseController<UILevelWinView>)this).view.Btn_Gift == (Object)null)
		{
			return;
		}
		Dictionary<int, int> dictionary = BuildRewardItems(rewardId);
		if (dictionary.Count == 0)
		{
			return;
		}
		PropModelProxy propModelProxy = GameApp.Proxy.Get<PropModelProxy>();
		List<ShowRewardItem> list = new List<ShowRewardItem>(dictionary.Count);
		foreach (KeyValuePair<int, int> item in dictionary)
		{
			list.Add(new ShowRewardItem
			{
				PropType = (propModelProxy?.GetRewardPropType(item.Key) ?? EPropType.None),
				Count = item.Value
			});
		}
		Transform transform = ((Component)((BaseController<UILevelWinView>)this).view.Btn_Gift).transform;
		TipsService.ShowChestRewards((RectTransform)(object)((transform is RectTransform) ? transform : null), list, Localize("chest_title", "Level Rewards"));
	}

	private void OnNextClick()
	{
		if (!_isClosing && !_nextButtonLocked)
		{
			((BaseController<UILevelWinView>)this).ExecuteCommand<ChestAutoClaimCommand>((object)null);
			RewardedLuckyDrawService.AbortSettlement();
			PostSettlementPipeline.Abort();
			CloseWithAnimation(delegate
			{
				_param?.OnNext?.Invoke();
			});
			int num = _param?.CompletedLevel ?? 1;
			RRDataTrack.Instance.ReportBtnClick("结算-第" + (num + 1) + "关");
		}
	}

	private void CloseWithAnimation(Action onComplete = null)
	{
		if (_isClosing)
		{
			return;
		}
		_isClosing = true;
		((BaseController<UILevelWinView>)this).view.SetBtnOKInteractable(interactable: false);
		float num = 0f;
		if (num <= 0f)
		{
			((BaseController<UILevelWinView>)this).CloseView(true);
			onComplete?.Invoke();
			return;
		}
		_closeTimerId = GameApp.Timer.Delay((Object)(object)((UIBaseView)((BaseController<UILevelWinView>)this).view).Behaviour, num, (Action)delegate
		{
			_closeTimerId = -1;
			((Component)((UIBaseView)((BaseController<UILevelWinView>)this).view).Behaviour).gameObject.SetActive(false);
			((BaseController<UILevelWinView>)this).CloseView(true);
			onComplete?.Invoke();
		});
	}

	private void TryAutoClaimDailyFirstWinPendingReward()
	{
		DailyFirstWinService.Instance.TryAutoClaimOnLevelWinSettlement(out var _);
	}

	private static Vector2 GetWorldPosition(Component component)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)component == (Object)null))
		{
			return new Vector2(component.transform.position.x, component.transform.position.y);
		}
		return Vector2.zero;
	}

	private static Vector2 GetScreenPosition(Component component)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)component == (Object)null))
		{
			return RectTransformUtility.WorldToScreenPoint((Camera)null, component.transform.position);
		}
		return Vector2.zero;
	}

	private static string GetRandomWellDoneText()
	{
		int num = Random.Range(0, 6);
		return Localize((num == 0) ? "LevelComplete_Title" : string.Format("{0}_{1}", "LevelComplete_Title", num), "Well Done!");
	}

	private static string GetRandomCheerUpText()
	{
		int num = Random.Range(0, 10);
		return Localize(string.Format("{0}{1}", "LevelComplete_CheerUp_", num), "Great job!");
	}

	private static string Localize(string key, string fallback, params object[] args)
	{
		string text = Singleton<LangManager>.Instance.GetText(key, args);
		if (string.IsNullOrEmpty(text))
		{
			if (args == null || args.Length == 0)
			{
				return fallback;
			}
			return string.Format(fallback, args);
		}
		return text;
	}

	private void OnLanguageChangeSuccess(LanguageChangeSuccess evt)
	{
		RefreshUI();
	}

	private void RefreshUI()
	{
		((BaseController<UILevelWinView>)this).view.RefreshScoreLabel();
		((BaseController<UILevelWinView>)this).view.RefreshRoundTipText();
		((BaseController<UILevelWinView>)this).view.SetScore(_param.Score);
		((BaseController<UILevelWinView>)this).view.SetEvaluation(GetRandomWellDoneText());
		ApplyPassCheerUpText();
		RefreshLevelDisplay();
		SetLevelLeftText(GameApp.Proxy.Get<ChestProxy>().GetLevelsLeft());
		((BaseController<UILevelWinView>)this).view.BestScoreView?.RefreshLocalizedText();
	}

	public override void OnRemove()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		RewardedLuckyDrawService.AbortSettlement();
		LevelEndBgmVolumeService.RestoreAfterSettlement();
		_levelWinAnimator?.Dispose();
		if (_finalProgressEffectTimerId >= 0)
		{
			GameApp.Timer.Kill(_finalProgressEffectTimerId);
			_finalProgressEffectTimerId = -1;
		}
		if (_closeTimerId >= 0)
		{
			GameApp.Timer.Kill(_closeTimerId);
			_closeTimerId = -1;
		}
		((BaseController<UILevelWinView>)this).UnlistenEvent<ChestClaimed>((Action<ChestClaimed>)OnChestClaimed);
		((BaseController<UILevelWinView>)this).UnlistenEvent<LanguageChangeSuccess>((Action<LanguageChangeSuccess>)OnLanguageChangeSuccess);
		((BaseController<UILevelWinView>)this).view.ResetSpineIdleForRelease();
		UIHomeBtnOKView btnOKView = ((BaseController<UILevelWinView>)this).view.BtnOKView;
		if (btnOKView != null)
		{
			Button btnOK = btnOKView.btnOK;
			if (btnOK != null)
			{
				((UnityEvent)btnOK.onClick).RemoveListener(new UnityAction(OnNextClick));
			}
		}
		((BaseController<UILevelWinView>)this).view.ShutdownBtnOK();
		((BaseController<UILevelWinView>)this).view.ShutdownBestScore();
		((BaseController<UILevelWinView>)this).view.ShutdownHalloweenTitle();
		((BaseController<UILevelWinView>)this).view.ShutdownGameWinIQ();
		Button btn_Gift = ((BaseController<UILevelWinView>)this).view.Btn_Gift;
		if (btn_Gift != null)
		{
			((UnityEvent)btn_Gift.onClick).RemoveListener(new UnityAction(OnGiftClick));
		}
		_progressBuildCts?.Cancel();
		_progressBuildCts?.Dispose();
		_progressBuildCts = null;
		base.OnRemove();
	}

	public override void OnUnloadAsset()
	{
		LevelWinPreloadService.NotifyResourcesReleased();
		GameApp.Res.ReleaseGroup("UILevelWin_Animal");
		GameApp.Res.ReleaseGroup("UILevelWin_Effect");
		base.OnUnloadAsset();
	}

	private static bool IsSettlementMaskPopupOpen()
	{
		for (int i = 0; i < SettlementMaskPopupPaths.Length; i++)
		{
			if (GameApp.UI.IsOpen(SettlementMaskPopupPaths[i]))
			{
				return true;
			}
		}
		return false;
	}

	public override void OnActive(bool isActive)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		((BaseController<UILevelWinView>)this).OnActive(isActive);
		if (!isActive)
		{
			if (IsSettlementMaskPopupOpen())
			{
				((BaseController<UILevelWinView>)this).view.PauseSpineIdle();
			}
			return;
		}
		((BaseController<UILevelWinView>)this).view.ResumeSpineIdle();
		RefreshLevelDisplay();
		if (_pendingEntrance)
		{
			_pendingEntrance = false;
			UniTaskVoid val = TryStartEntranceWhenReady();
			((UniTaskVoid)(ref val)).Forget();
			return;
		}
		if (_claimOpened)
		{
			RefreshChestDisplay();
			ShowContentNoAnim();
			UnlockNextButton();
		}
		TryPlayPendingNextBtnEffect();
	}

	public override void OnEscape()
	{
	}
}
