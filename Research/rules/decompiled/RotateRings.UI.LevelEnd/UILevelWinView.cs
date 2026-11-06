using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Basis.Common.UI;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Game.UI;
using Hot.Model;
using RotateRings.Common;
using RotateRings.GameWinIQ;
using RotateRings.Halloween;
using RotateRings.Model;
using RotateRings.Services;
using RotateRings.UI.Home;
using Spine;
using Spine.Unity;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Experiment;
using XingYun.Extensions;
using XingYun.Lang;
using XingYun.UI.Controller;
using XingYun.UI.Core;
using XingYun.Utility;

namespace RotateRings.UI.LevelEnd;

public class UILevelWinView : GameBaseView
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CBuildProgressNodesAsync_003Ed__114 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskMethodBuilder<List<PreStageRewardProgress>> _003C_003Et__builder;

		public UILevelWinView _003C_003E4__this;

		public int count;

		public string prefabLLocation;

		public string prefabRLocation;

		public string prefabMLocation;

		public CancellationToken cancellationToken;

		private List<PreStageRewardProgress> _003Cnodes_003E5__2;

		private int _003Ci_003E5__3;

		private Awaiter<WidgetHandle> _003C_003Eu__1;

		private Awaiter<bool> _003C_003Eu__2;

		private void MoveNext()
		{
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			UILevelWinView uILevelWinView = _003C_003E4__this;
			List<PreStageRewardProgress> result;
			try
			{
				Awaiter<WidgetHandle> val;
				if (num == 0)
				{
					val = _003C_003Eu__1;
					_003C_003Eu__1 = default(Awaiter<WidgetHandle>);
					num = (_003C_003E1__state = -1);
					goto IL_00f8;
				}
				Awaiter<bool> val2;
				if (num == 1)
				{
					val2 = _003C_003Eu__2;
					_003C_003Eu__2 = default(Awaiter<bool>);
					num = (_003C_003E1__state = -1);
					goto IL_0216;
				}
				uILevelWinView.ClearFinalProgressEffect();
				uILevelWinView.DetachProgressWidgets();
				_003Cnodes_003E5__2 = new List<PreStageRewardProgress>();
				if (!((Object)(object)uILevelWinView.Trans_Progress == (Object)null) && count > 0)
				{
					_003Ci_003E5__3 = 0;
					goto IL_0165;
				}
				result = _003Cnodes_003E5__2;
				goto end_IL_000e;
				IL_0216:
				if (val2.GetResult() || (Object)(object)((UIBaseView)uILevelWinView).Behaviour == (Object)null)
				{
					result = _003Cnodes_003E5__2;
				}
				else
				{
					Canvas.ForceUpdateCanvases();
					for (int i = 0; i < _003Cnodes_003E5__2.Count; i++)
					{
						_003Cnodes_003E5__2[i].RefreshWidthAfterLayout();
					}
					result = _003Cnodes_003E5__2;
				}
				goto end_IL_000e;
				IL_00f8:
				WidgetHandle result2 = val.GetResult();
				if (!cancellationToken.IsCancellationRequested && !((Object)(object)((UIBaseView)uILevelWinView).Behaviour == (Object)null))
				{
					if (result2 != null)
					{
						uILevelWinView.progressWidgetHandles.Add(result2);
						PreStageRewardProgress view = result2.GetView<PreStageRewardProgress>();
						if (view != null)
						{
							_003Cnodes_003E5__2.Add(view);
						}
					}
					_003Ci_003E5__3++;
					goto IL_0165;
				}
				result = _003Cnodes_003E5__2;
				goto end_IL_000e;
				IL_0165:
				if (_003Ci_003E5__3 < count)
				{
					string widgetLocation = ((_003Ci_003E5__3 == 0) ? prefabLLocation : ((_003Ci_003E5__3 == count - 1) ? prefabRLocation : prefabMLocation));
					val = uILevelWinView.AttachWidgetAsync<PreStageRewardProgress>(widgetLocation, ((Component)uILevelWinView.Trans_Progress).transform).GetAwaiter();
					if (!val.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = val;
						_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<WidgetHandle>, _003CBuildProgressNodesAsync_003Ed__114>(ref val, ref this);
						return;
					}
					goto IL_00f8;
				}
				LayoutRebuilder.ForceRebuildLayoutImmediate(uILevelWinView.Trans_Progress);
				for (int j = 0; j < _003Cnodes_003E5__2.Count; j++)
				{
					_003Cnodes_003E5__2[j].InitializeWidth();
				}
				UniTask val3 = UniTask.Yield(cancellationToken);
				val2 = ((UniTask)(ref val3)).SuppressCancellationThrow().GetAwaiter();
				if (!val2.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = val2;
					_003C_003Et__builder.AwaitUnsafeOnCompleted<Awaiter<bool>, _003CBuildProgressNodesAsync_003Ed__114>(ref val2, ref this);
					return;
				}
				goto IL_0216;
				end_IL_000e:;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003Cnodes_003E5__2 = null;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003Cnodes_003E5__2 = null;
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

	private const string SpineAnimOpen = "open";

	private const string SpineAnimOpen1 = "open1";

	private const string SpineAnimIdle1 = "idle1";

	private const string SpineAnimIdle2 = "idle2";

	private const string SpineAnimIdle3 = "idle3";

	private const string SquirrelIdle3AudioVariantId = "variant_21dcb42d";

	private const int BaseSettleActionCount = 2;

	private const int ExperimentSettleActionCount = 3;

	private const string DefaultSpineResourceName = "eff_spine_jieusuan_songshu_01";

	private const string HalloweenSpineResourceName = "eff_spine_fullscreen_songshu_gp4_5";

	private const string RootAppearClip = "anim_gamewin_ui_appear_gp1";

	private const string RootDisappearClip = "anim_gamewin_ui_disappear_gp1";

	private const string EncouragementClipStateName = "LevelWin_Encouragement_Config";

	private const string HalloweenBtnOKLocation = "HalloweenBtnOK";

	private const string HalloweenTitleLocation = "LevelWin_HalloweenTitle";

	private const string ScoreLabelKey = "LevelComplete_Score";

	private const string RoundTipTextKey = "Text_Endless";

	private TrackEntry idleLoopTrackEntry;

	private TrackEntryDelegate idleLoopCompleteHandler;

	private AudioKey currentIdleLoopAudioKey;

	private bool resumeVoCompleteSquirrelAfterOverlay;

	private bool resumeIdleLoopSoundAfterOverlay;

	private WidgetHandle scoreHandle;

	private WidgetHandle bestScoreHandle;

	private WidgetHandle btnOKHandle;

	private WidgetHandle halloweenTitleHandle;

	private WidgetHandle gameWinIqHandle;

	private GameObject halloweenBtnOK;

	private GameObject halloweenTitle;

	private int defaultBtnOKSiblingIndex = -1;

	private bool passTextOffsetApplied;

	private float passTextOriginalAnchoredY;

	private LevelWinAnimationConfig animationConfig;

	private ELevelDifficulty settlementDifficulty;

	private readonly List<WidgetHandle> progressWidgetHandles = new List<WidgetHandle>();

	private GameObject finalProgressEffect;

	public Image Img_FullBg;

	public RectTransform Trans_TopBar;

	public TMP_Text TxtM_Evaluation;

	public TMP_Text TxtM_Pass;

	public RectTransform Trans_CardContainer;

	public RectTransform Trans_Content;

	public RectTransform Trans_Bottom;

	public RectTransform Trans_StageReward;

	public TMP_Text TxtM_LevelLeft;

	public RectTransform Trans_Progress;

	public Button Btn_Gift;

	public GameObject Trans_Spine;

	public GameObject Trans_Effect;

	public Image Img_Ribbon;

	public Animation UIZoodokuGameWin;

	public GameObject Trans_Text;

	public GameObject Trans_StarScoreUI;

	public TMP_Text TxtM_ScoreNum;

	public TMP_Text TxtM_ScoreLabel;

	public Image Img_BannerPoints;

	public UISpineLoader Trans_Spine_Play;

	public GameObject eff_rank_btn_gp1;

	public GameObject Root;

	public GameObject round_tips_text;

	public TMP_Text roundText_Tip;

	public GameObject eff_results_fullscreen_gp2;

	public GameObject eff_results_fullscreen_gp3;

	public UIBehaviour BestScore;

	public UIBehaviour BtnOK;

	public GameObject Trans_IQ;

	private UILevelWinScoreView ScoreView => EnsureWidget<UILevelWinScoreView>(ref scoreHandle, ((UIBaseView)this).Behaviour);

	public UIBestScoreView BestScoreView => bestScoreHandle?.GetView<UIBestScoreView>();

	public UIHomeBtnOKView BtnOKView => btnOKHandle?.GetView<UIHomeBtnOKView>();

	public void InitBtnOK(bool playIdleAnimation)
	{
		ShutdownBtnOK();
		if ((Object)(object)BtnOK == (Object)null)
		{
			return;
		}
		if (HalloweenFeatureService.IsSettlementEnabled())
		{
			AttachHalloweenBtnOK();
		}
		if (btnOKHandle != null)
		{
			BtnOKView?.Initialize(playIdleAnimation);
			return;
		}
		btnOKHandle = AttachWidget<UIHomeBtnOKView>(BtnOK);
		if (btnOKHandle == null)
		{
			Log.LogError("UI", "[UILevelWinView] AttachWidget UIHomeBtnOKView failed");
		}
		else
		{
			BtnOKView?.Initialize(playIdleAnimation);
		}
	}

	public void ShutdownBtnOK()
	{
		if (btnOKHandle != null)
		{
			DetachWidget(btnOKHandle);
			btnOKHandle = null;
		}
		if ((Object)(object)halloweenBtnOK != (Object)null)
		{
			halloweenBtnOK.SetActive(false);
		}
		if (!((Object)(object)BtnOK == (Object)null))
		{
			((Component)BtnOK).gameObject.SetActive(true);
			if (defaultBtnOKSiblingIndex >= 0 && (Object)(object)((Component)BtnOK).transform.parent != (Object)null)
			{
				((Component)BtnOK).transform.SetSiblingIndex(Mathf.Min(defaultBtnOKSiblingIndex, ((Component)BtnOK).transform.parent.childCount - 1));
			}
		}
	}

	private void DestroyHalloweenBtnOK()
	{
		if ((Object)(object)halloweenBtnOK != (Object)null)
		{
			Object.Destroy((Object)(object)halloweenBtnOK);
			halloweenBtnOK = null;
		}
		defaultBtnOKSiblingIndex = -1;
	}

	private void InitializeHalloweenTitle()
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		ShutdownHalloweenTitle();
		if (!HalloweenFeatureService.IsSettlementEnabled() || (Object)(object)Trans_TopBar == (Object)null)
		{
			return;
		}
		if ((Object)(object)halloweenTitle == (Object)null)
		{
			GameObject val = LoadAsset<GameObject>("LevelWin_HalloweenTitle");
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			halloweenTitle = Object.Instantiate<GameObject>(val, (Transform)(object)Trans_TopBar, false);
		}
		halloweenTitle.SetActive(true);
		Transform transform = halloweenTitle.transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if (val2 != null)
		{
			val2.anchoredPosition = Vector2.zero;
		}
		UIBehaviour component = halloweenTitle.GetComponent<UIBehaviour>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("UI", "万圣节结算标题缺少 UIBehaviour");
			halloweenTitle.SetActive(false);
			return;
		}
		halloweenTitleHandle = AttachWidget<UILevelWinHalloweenTitleView>(component);
		UILevelWinHalloweenTitleView uILevelWinHalloweenTitleView = halloweenTitleHandle?.GetView<UILevelWinHalloweenTitleView>();
		if (uILevelWinHalloweenTitleView == null)
		{
			ShutdownHalloweenTitle();
			return;
		}
		if ((Object)(object)TxtM_Evaluation != (Object)null)
		{
			((Component)TxtM_Evaluation).gameObject.SetActive(false);
			uILevelWinHalloweenTitleView.SetTitle(TxtM_Evaluation.text);
		}
		if ((Object)(object)Img_Ribbon != (Object)null)
		{
			((Component)Img_Ribbon).gameObject.SetActive(false);
		}
	}

	public void ShutdownHalloweenTitle()
	{
		if ((Object)(object)halloweenTitle != (Object)null)
		{
			halloweenTitle.SetActive(false);
		}
		if (halloweenTitleHandle != null)
		{
			DetachWidget(halloweenTitleHandle);
			halloweenTitleHandle = null;
		}
		if ((Object)(object)TxtM_Evaluation != (Object)null)
		{
			((Component)TxtM_Evaluation).gameObject.SetActive(true);
		}
		if ((Object)(object)Img_Ribbon != (Object)null)
		{
			((Component)Img_Ribbon).gameObject.SetActive(true);
		}
	}

	private void DestroyHalloweenTitle()
	{
		if ((Object)(object)halloweenTitle != (Object)null)
		{
			Object.Destroy((Object)(object)halloweenTitle);
			halloweenTitle = null;
		}
	}

	private void AttachHalloweenBtnOK()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)BtnOK).transform;
		Transform parent = transform.parent;
		if ((Object)(object)parent == (Object)null)
		{
			return;
		}
		if ((Object)(object)halloweenBtnOK == (Object)null)
		{
			GameObject val = LoadAsset<GameObject>("HalloweenBtnOK");
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			halloweenBtnOK = Object.Instantiate<GameObject>(val, parent, false);
			Transform transform2 = halloweenBtnOK.transform;
			((RectTransform)((transform2 is RectTransform) ? transform2 : null)).anchoredPosition = ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
		}
		defaultBtnOKSiblingIndex = transform.GetSiblingIndex();
		((Component)transform).gameObject.SetActive(false);
		halloweenBtnOK.SetActive(true);
		halloweenBtnOK.transform.SetSiblingIndex(defaultBtnOKSiblingIndex);
		UIBehaviour component = halloweenBtnOK.GetComponent<UIBehaviour>();
		if ((Object)(object)component == (Object)null)
		{
			Log.LogError("UI", "[UILevelWinView] Halloween btnOK prefab missing UIBehaviour");
			halloweenBtnOK.SetActive(false);
			((Component)transform).gameObject.SetActive(true);
			defaultBtnOKSiblingIndex = -1;
			return;
		}
		btnOKHandle = AttachWidget<UIHomeBtnOKView>(component);
		if (btnOKHandle == null)
		{
			Log.LogError("UI", "[UILevelWinView] AttachWidget Halloween UIHomeBtnOKView failed");
			halloweenBtnOK.SetActive(false);
			((Component)transform).gameObject.SetActive(true);
			defaultBtnOKSiblingIndex = -1;
		}
	}

	public RectTransform GetBtnOKTransform()
	{
		UIHomeBtnOKView btnOKView = BtnOKView;
		object obj;
		if (btnOKView == null)
		{
			obj = null;
		}
		else
		{
			Button btnOK = btnOKView.btnOK;
			obj = ((btnOK != null) ? ((Component)btnOK).transform : null);
		}
		return (RectTransform)((obj is RectTransform) ? obj : null);
	}

	public void RefreshBtnOK(ELevelDifficulty difficulty, GameMode gameMode, int level)
	{
		BtnOKView?.Refresh(difficulty, hasUnfinishedLevel: false, gameMode, level);
	}

	public void SetBtnOKInteractable(bool interactable)
	{
		BtnOKView?.SetInteractable(interactable);
	}

	public void SetSettlementDifficulty(ELevelDifficulty difficulty)
	{
		settlementDifficulty = difficulty;
	}

	public void PlaySpineOpenIdle(Action onInstanceComplete, int levelWinAnimation, Action<float> onOpeningStarted = null, Action onOpeningComplete = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		StartSpineOpenThenIdle(onInstanceComplete, levelWinAnimation, onOpeningStarted, onOpeningComplete, cancellationToken);
	}

	private void StartSpineOpenThenIdle(Action onInstanceComplete, int levelWinAnimation, Action<float> onOpeningStarted, Action onOpeningComplete, CancellationToken cancellationToken)
	{
		StopIdle3SoundForRelease();
		UnsubscribeIdleLoopSound();
		currentIdleLoopAudioKey = null;
		ClearSpineSoundResumeSnapshot();
		UISpineLoader loader = Trans_Spine_Play;
		if (!IsSpinePlaybackAlive() || (Object)(object)loader == (Object)null)
		{
			onOpeningStarted?.Invoke(0f);
			onOpeningComplete?.Invoke();
			return;
		}
		ResumeSpineIdle();
		bool flag = levelWinAnimation == 1 || levelWinAnimation == 2;
		bool flag2 = HalloweenFeatureService.IsSettlementEnabled();
		int num = ((levelWinAnimation == 1 && !flag2) ? 3 : 2);
		int num2 = Random.Range(0, num);
		if (levelWinAnimation == 2 && !flag2 && (settlementDifficulty == ELevelDifficulty.Hard || settlementDifficulty == ELevelDifficulty.VeryHard))
		{
			num2 = 2;
		}
		if (flag && LevelEndRuntimeRules.ForceSquirrelIdle3ForAcceptance)
		{
			num2 = 2;
		}
		string openAnim = ((num2 == 2) ? "open1" : "open");
		string idleAnim = num2 switch
		{
			0 => "idle1", 
			1 => "idle2", 
			_ => "idle3", 
		};
		AudioKey audioKey = ResolveIdleLoopAudioKey(idleAnim);
		if (flag && idleAnim == "idle3")
		{
			AudioService.ActivateExperiment("variant_21dcb42d");
		}
		loader.Play(openAnim, isLoop: false, 0, delegate
		{
			if (!cancellationToken.IsCancellationRequested && IsSpinePlaybackAlive())
			{
				loader.Play(idleAnim, isLoop: true);
				currentIdleLoopAudioKey = audioKey;
				PlayIdleLoopSound(audioKey);
				SubscribeIdleLoopSound(loader, audioKey);
				onOpeningComplete?.Invoke();
			}
		}, delegate(GameObject spineInstance)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (!cancellationToken.IsCancellationRequested && !((Object)(object)spineInstance == (Object)null))
			{
				spineInstance.transform.localPosition = Vector3.zero;
				spineInstance.transform.localScale = Vector3.one;
				onInstanceComplete?.Invoke();
				SkeletonGraphic skeletonGraphic = loader.SkeletonGraphic;
				if (skeletonGraphic != null)
				{
					skeletonGraphic.Initialize(false);
				}
				float? obj;
				if (skeletonGraphic == null)
				{
					obj = null;
				}
				else
				{
					Skeleton skeleton = skeletonGraphic.Skeleton;
					if (skeleton == null)
					{
						obj = null;
					}
					else
					{
						SkeletonData data = skeleton.Data;
						if (data == null)
						{
							obj = null;
						}
						else
						{
							Animation obj2 = data.FindAnimation(openAnim);
							obj = ((obj2 != null) ? new float?(obj2.Duration) : ((float?)null));
						}
					}
				}
				float? num3 = obj;
				float valueOrDefault = num3.GetValueOrDefault();
				onOpeningStarted?.Invoke(valueOrDefault);
			}
		});
	}

	private static AudioKey ResolveIdleLoopAudioKey(string animationName)
	{
		return animationName switch
		{
			"idle1" => AudioKeys.LevelWinSpineAnimIdle2, 
			"idle2" => AudioKeys.LevelWinSpineAnimIdle, 
			"idle3" => AudioKeys.LevelWinSpineAnimIdle3, 
			_ => null, 
		};
	}

	private void PlayIdleLoopSound(AudioKey audioKey)
	{
		AudioService.PlayUiSound(audioKey.Value);
	}

	private void SubscribeIdleLoopSound(UISpineLoader loader, AudioKey audioKey)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		UnsubscribeIdleLoopSound();
		currentIdleLoopAudioKey = audioKey;
		object obj;
		if (loader == null)
		{
			obj = null;
		}
		else
		{
			SkeletonGraphic skeletonGraphic = loader.SkeletonGraphic;
			if (skeletonGraphic == null)
			{
				obj = null;
			}
			else
			{
				AnimationState animationState = skeletonGraphic.AnimationState;
				obj = ((animationState != null) ? animationState.GetCurrent(0) : null);
			}
		}
		TrackEntry val = (TrackEntry)obj;
		if (val == null)
		{
			return;
		}
		idleLoopTrackEntry = val;
		idleLoopCompleteHandler = (TrackEntryDelegate)delegate
		{
			if (IsSpinePlaybackAlive())
			{
				PlayIdleLoopSound(audioKey);
			}
		};
		idleLoopTrackEntry.Complete += idleLoopCompleteHandler;
	}

	private void UnsubscribeIdleLoopSound()
	{
		if (idleLoopTrackEntry != null && idleLoopCompleteHandler != null)
		{
			idleLoopTrackEntry.Complete -= idleLoopCompleteHandler;
		}
		idleLoopTrackEntry = null;
		idleLoopCompleteHandler = null;
	}

	private bool IsSpinePlaybackAlive()
	{
		if ((Object)(object)Trans_Spine_Play != (Object)null && (Object)(object)((UIBaseView)this).Behaviour != (Object)null)
		{
			return (Object)(object)((Component)((UIBaseView)this).Behaviour).gameObject != (Object)null;
		}
		return false;
	}

	public void PauseSpineIdle()
	{
		UISpineLoader trans_Spine_Play = Trans_Spine_Play;
		object obj;
		if (trans_Spine_Play == null)
		{
			obj = null;
		}
		else
		{
			SkeletonGraphic skeletonGraphic = trans_Spine_Play.SkeletonGraphic;
			obj = ((skeletonGraphic != null) ? skeletonGraphic.AnimationState : null);
		}
		AnimationState val = (AnimationState)obj;
		if (val != null)
		{
			val.TimeScale = 0f;
		}
		PauseSpineRelatedSounds();
	}

	public void ResumeSpineIdle()
	{
		UISpineLoader trans_Spine_Play = Trans_Spine_Play;
		object obj;
		if (trans_Spine_Play == null)
		{
			obj = null;
		}
		else
		{
			SkeletonGraphic skeletonGraphic = trans_Spine_Play.SkeletonGraphic;
			obj = ((skeletonGraphic != null) ? skeletonGraphic.AnimationState : null);
		}
		AnimationState val = (AnimationState)obj;
		if (val != null)
		{
			val.TimeScale = 1f;
		}
		ResumeSpineRelatedSounds();
	}

	public void ResetSpineIdleForRelease()
	{
		StopIdle3SoundForRelease();
		ClearSpineSoundResumeSnapshot();
		UISpineLoader trans_Spine_Play = Trans_Spine_Play;
		object obj;
		if (trans_Spine_Play == null)
		{
			obj = null;
		}
		else
		{
			SkeletonGraphic skeletonGraphic = trans_Spine_Play.SkeletonGraphic;
			obj = ((skeletonGraphic != null) ? skeletonGraphic.AnimationState : null);
		}
		AnimationState val = (AnimationState)obj;
		if (val != null)
		{
			val.TimeScale = 1f;
		}
	}

	private void StopIdle3SoundForRelease()
	{
		if (currentIdleLoopAudioKey == AudioKeys.LevelWinSpineAnimIdle3)
		{
			AudioService.StopSound(AudioKeys.LevelWinSpineAnimIdle3.Value);
			UnsubscribeIdleLoopSound();
			currentIdleLoopAudioKey = null;
		}
	}

	private void PauseSpineRelatedSounds()
	{
		UISpineLoader trans_Spine_Play = Trans_Spine_Play;
		object obj;
		if (trans_Spine_Play == null)
		{
			obj = null;
		}
		else
		{
			SkeletonGraphic skeletonGraphic = trans_Spine_Play.SkeletonGraphic;
			if (skeletonGraphic == null)
			{
				obj = null;
			}
			else
			{
				AnimationState animationState = skeletonGraphic.AnimationState;
				if (animationState == null)
				{
					obj = null;
				}
				else
				{
					TrackEntry current = animationState.GetCurrent(0);
					if (current == null)
					{
						obj = null;
					}
					else
					{
						Animation animation = current.Animation;
						obj = ((animation != null) ? animation.Name : null);
					}
				}
			}
		}
		if (obj == null)
		{
			obj = "null";
		}
		string text = (string)obj;
		bool flag = AudioService.IsEventPlaying(AudioKeys.LevelWinSpineAnimIdle.Value);
		bool flag2 = AudioService.IsEventPlaying(AudioKeys.LevelWinSpineAnimIdle2.Value);
		bool flag3 = AudioService.IsEventPlaying(AudioKeys.LevelWinSpineAnimIdle3.Value);
		bool flag4 = AudioService.IsEventPlaying(AudioKeys.VoCompleteSquirrel.Value);
		bool flag5 = text == "idle1" || text == "idle2" || text == "idle3";
		resumeVoCompleteSquirrelAfterOverlay = flag4;
		resumeIdleLoopSoundAfterOverlay = flag || flag2 || flag3 || flag5;
		AudioService.StopSound(AudioKeys.VoCompleteSquirrel.Value);
		AudioService.StopSound(AudioKeys.LevelWinSpineAnimIdle.Value);
		AudioService.StopSound(AudioKeys.LevelWinSpineAnimIdle2.Value);
		if (flag3 || text == "idle3")
		{
			AudioService.StopSound(AudioKeys.LevelWinSpineAnimIdle3.Value);
		}
	}

	private void ResumeSpineRelatedSounds()
	{
		if (resumeVoCompleteSquirrelAfterOverlay)
		{
			AudioService.PlayUiSound(AudioKeys.VoCompleteSquirrel.Value);
			resumeVoCompleteSquirrelAfterOverlay = false;
		}
		if (resumeIdleLoopSoundAfterOverlay)
		{
			AudioKey audioKey = currentIdleLoopAudioKey ?? ResolveIdleLoopAudioKeyFromTrack();
			if (audioKey != null)
			{
				AudioService.PlayUiSound(audioKey.Value);
				currentIdleLoopAudioKey = audioKey;
			}
			resumeIdleLoopSoundAfterOverlay = false;
		}
	}

	private AudioKey ResolveIdleLoopAudioKeyFromTrack()
	{
		UISpineLoader trans_Spine_Play = Trans_Spine_Play;
		object animationName;
		if (trans_Spine_Play == null)
		{
			animationName = null;
		}
		else
		{
			SkeletonGraphic skeletonGraphic = trans_Spine_Play.SkeletonGraphic;
			if (skeletonGraphic == null)
			{
				animationName = null;
			}
			else
			{
				AnimationState animationState = skeletonGraphic.AnimationState;
				if (animationState == null)
				{
					animationName = null;
				}
				else
				{
					TrackEntry current = animationState.GetCurrent(0);
					if (current == null)
					{
						animationName = null;
					}
					else
					{
						Animation animation = current.Animation;
						animationName = ((animation != null) ? animation.Name : null);
					}
				}
			}
		}
		return ResolveIdleLoopAudioKey((string)animationName);
	}

	private void ClearSpineSoundResumeSnapshot()
	{
		resumeVoCompleteSquirrelAfterOverlay = false;
		resumeIdleLoopSoundAfterOverlay = false;
	}

	public void PlaySettlementSounds()
	{
		AudioService.PlayUiSound(AudioKeys.LevelComplete.Value);
		AudioService.PlayUiSound(AudioKeys.VoCompleteSquirrel.Value);
	}

	private void ConfigureSpineResource()
	{
		if (!((Object)(object)Trans_Spine_Play == (Object)null))
		{
			Trans_Spine_Play.SetResource(LevelWinResourceKeys.GetSpinePrefab());
		}
	}

	public void PrepareEntrance()
	{
		ConfigureSpineResource();
		InitializeHalloweenTitle();
		ResetRootAnimationForReuse();
		EnsureRootCanvasGroupVisible();
		SetActive((Component)(object)Trans_TopBar, active: false);
		SetActive((Component)(object)(((Object)(object)TxtM_Pass != (Object)null) ? TxtM_Pass.transform.parent : null), active: false);
		if ((Object)(object)Trans_IQ != (Object)null)
		{
			Trans_IQ.SetActive(false);
		}
		SetActive((Component)(object)Trans_Bottom, active: false);
		SetActive((Component)(object)Trans_Content, active: false);
		if ((Object)(object)Trans_StarScoreUI != (Object)null)
		{
			Trans_StarScoreUI.SetActive(false);
		}
		BestScoreView?.PrepareHidden();
		SetBannerPointsVisible(visible: true);
		if ((Object)(object)eff_rank_btn_gp1 != (Object)null)
		{
			eff_rank_btn_gp1.SetActive(false);
		}
		ApplyAmbientEffect();
		SetBtnOKInteractable(interactable: false);
	}

	private void ApplyAmbientEffect()
	{
		int num = FunctionDataCenter.Get<LevelEndFunction>()?.LevelWinAmbient ?? 0;
		if ((Object)(object)eff_results_fullscreen_gp2 != (Object)null)
		{
			eff_results_fullscreen_gp2.SetActive(num == 0);
		}
		if ((Object)(object)eff_results_fullscreen_gp3 != (Object)null)
		{
			eff_results_fullscreen_gp3.SetActive(num == 1);
		}
	}

	private void EnsureRootCanvasGroupVisible()
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour == (Object)null))
		{
			CanvasGroup component = ((Component)((UIBaseView)this).Behaviour).GetComponent<CanvasGroup>();
			if ((Object)(object)component != (Object)null)
			{
				component.alpha = 1f;
			}
		}
	}

	private void ResetRootAnimationForReuse()
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour == (Object)null))
		{
			Animation component = ((Component)((UIBaseView)this).Behaviour).GetComponent<Animation>();
			if (!((Object)(object)component == (Object)null))
			{
				component.Stop();
				RewindAnimationClip(component, "anim_gamewin_ui_disappear_gp1");
				RewindAnimationClip(component, "anim_gamewin_ui_appear_gp1");
			}
		}
	}

	public float PlayRootAppear()
	{
		PlayClipWithRewind((Component)(object)((UIBaseView)this).Behaviour, "anim_gamewin_ui_appear_gp1");
		return GetRootAppearDuration();
	}

	public float GetRootAppearDuration()
	{
		if ((Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			return 0f;
		}
		Animation component = ((Component)((UIBaseView)this).Behaviour).GetComponent<Animation>();
		if ((Object)(object)component == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = component.GetClip("anim_gamewin_ui_appear_gp1");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	public void PlayBottonBoxGp1()
	{
		Animation val = (((Object)(object)Trans_Bottom != (Object)null) ? ((Component)Trans_Bottom).GetComponent<Animation>() : null);
		if (!((Object)(object)val == (Object)null))
		{
			AnimationState val2 = val["anim_gamewin_botton_box_gp1"];
			if (!((TrackedReference)(object)val2 == (TrackedReference)null))
			{
				val.Stop("anim_gamewin_botton_box_gp1");
				val2.time = 0f;
				val.Sample();
				val2.wrapMode = (WrapMode)8;
				val.Play("anim_gamewin_botton_box_gp1");
			}
		}
	}

	public float PlayCloseAnimation()
	{
		return PlayRootClipAndGetDuration("anim_gamewin_ui_disappear_gp1");
	}

	public float PlayTopBarAppear(bool originalPlayback = false)
	{
		float result = PlayAppear((Component)(object)Trans_TopBar, originalPlayback);
		WidgetHandle widgetHandle = halloweenTitleHandle;
		if (widgetHandle != null)
		{
			UILevelWinHalloweenTitleView view = widgetHandle.GetView<UILevelWinHalloweenTitleView>();
			if (view != null)
			{
				view.PlayOpenAnimation();
				return result;
			}
			return result;
		}
		return result;
	}

	public float PlayBottomAppear(bool originalPlayback = false)
	{
		return PlayAppear((Component)(object)Trans_Bottom, originalPlayback);
	}

	public void ShowScoreRegion()
	{
		GameObject trans_StarScoreUI = Trans_StarScoreUI;
		if (trans_StarScoreUI != null)
		{
			trans_StarScoreUI.SetActive(true);
		}
	}

	public float ShowEncouragement(bool animated)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Transform val = (((Object)(object)TxtM_Pass != (Object)null) ? TxtM_Pass.transform.parent : null);
		if (animated)
		{
			ShowScoreRegion();
			return PlayAppear((Component)(object)val, originalPlayback: true);
		}
		if ((Object)(object)val == (Object)null)
		{
			return 0f;
		}
		Animation component = ((Component)val).GetComponent<Animation>();
		if (component != null)
		{
			component.Stop();
		}
		val.localScale = Vector3.one;
		CanvasGroup component2 = ((Component)val).GetComponent<CanvasGroup>();
		if ((Object)(object)component2 != (Object)null)
		{
			component2.alpha = 1f;
		}
		((Component)val).gameObject.SetActive(true);
		return 0f;
	}

	public float PlayEncouragement(AnimationClip clip)
	{
		ShowEncouragement(animated: false);
		if ((Object)(object)clip == (Object)null || !clip.legacy)
		{
			return 0f;
		}
		Transform val = (((Object)(object)TxtM_Pass != (Object)null) ? TxtM_Pass.transform.parent : null);
		Animation val2 = (((Object)(object)val != (Object)null) ? ((Component)val).GetComponent<Animation>() : null);
		if ((Object)(object)val2 == (Object)null || !((Behaviour)val2).isActiveAndEnabled)
		{
			return 0f;
		}
		if ((Object)(object)val2.GetClip("LevelWin_Encouragement_Config") != (Object)(object)clip)
		{
			val2.AddClip(clip, "LevelWin_Encouragement_Config");
		}
		AnimationState val3 = val2["LevelWin_Encouragement_Config"];
		if ((TrackedReference)(object)val3 == (TrackedReference)null)
		{
			return 0f;
		}
		val3.time = 0f;
		val3.speed = 1f;
		val3.wrapMode = (WrapMode)8;
		if (!val2.Play("LevelWin_Encouragement_Config"))
		{
			return 0f;
		}
		val2.Sample();
		return val3.length;
	}

	public float PlayContentAppear(bool originalPlayback = false)
	{
		if ((Object)(object)Trans_Content == (Object)null)
		{
			return 0f;
		}
		if ((Object)(object)Trans_StarScoreUI != (Object)null)
		{
			Trans_StarScoreUI.SetActive(true);
		}
		((Component)Trans_Content).gameObject.SetActive(true);
		Animation component = ((Component)Trans_Content).GetComponent<Animation>();
		if ((Object)(object)component == (Object)null)
		{
			return 0f;
		}
		if (originalPlayback)
		{
			RestoreOriginalWrapMode(component, "anim_common_btn_appear");
			component.Play("anim_common_btn_appear");
			if (!((Object)(object)component.clip != (Object)null))
			{
				return 0f;
			}
			return component.clip.length;
		}
		PlayClipWithRewind((Component)(object)Trans_Content, "anim_common_btn_appear");
		AnimationClip clip = component.GetClip("anim_common_btn_appear");
		if (clip == null)
		{
			return 0f;
		}
		return clip.length;
	}

	public void EnableNextButton()
	{
		SetBtnOKInteractable(interactable: true);
	}

	public void DisableNextButton()
	{
		SetBtnOKInteractable(interactable: false);
	}

	public void SetScore(int score)
	{
		ScoreView?.SetScore(score);
	}

	public void PlayScoreScroll(int fromScore, int toScore, float duration, Action onComplete)
	{
		ScoreView.PlayScoreScroll(fromScore, toScore, duration, onComplete);
	}

	public float PlayScoreEmphasis(AnimationClip clip)
	{
		return ScoreView.PlayEmphasis(clip);
	}

	public LevelWinAnimationSettings CaptureAnimationSettings(bool useExperiment)
	{
		if ((Object)(object)animationConfig == (Object)null)
		{
			animationConfig = LoadAsset<LevelWinAnimationConfig>(Res.GetDefaultResPath("LevelWinAnimationConfig"));
		}
		if ((Object)(object)animationConfig == (Object)null)
		{
			throw new InvalidOperationException("缺少结算动画SO：LevelWinAnimationConfig");
		}
		return LevelEndRuntimeRules.Capture(animationConfig, useExperiment);
	}

	public void StopLevelWinPresentation()
	{
		scoreHandle?.GetView<UILevelWinScoreView>()?.StopPlayback();
		gameWinIqHandle?.GetView<UIZhiShangXiTongView>()?.StopPlayback();
		foreach (WidgetHandle progressWidgetHandle in progressWidgetHandles)
		{
			progressWidgetHandle?.GetView<PreStageRewardProgress>()?.StopPlayback();
		}
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		if (behaviour != null)
		{
			Animation component = ((Component)behaviour).GetComponent<Animation>();
			if (component != null)
			{
				component.Stop();
			}
		}
		RectTransform trans_TopBar = Trans_TopBar;
		if (trans_TopBar != null)
		{
			Animation component2 = ((Component)trans_TopBar).GetComponent<Animation>();
			if (component2 != null)
			{
				component2.Stop();
			}
		}
		RectTransform trans_Bottom = Trans_Bottom;
		if (trans_Bottom != null)
		{
			Animation component3 = ((Component)trans_Bottom).GetComponent<Animation>();
			if (component3 != null)
			{
				component3.Stop();
			}
		}
		RectTransform trans_Content = Trans_Content;
		if (trans_Content != null)
		{
			Animation component4 = ((Component)trans_Content).GetComponent<Animation>();
			if (component4 != null)
			{
				component4.Stop();
			}
		}
		TMP_Text txtM_Pass = TxtM_Pass;
		if (txtM_Pass == null)
		{
			return;
		}
		Transform parent = txtM_Pass.transform.parent;
		if (parent != null)
		{
			Animation component5 = ((Component)parent).GetComponent<Animation>();
			if (component5 != null)
			{
				component5.Stop();
			}
		}
	}

	public void RefreshScoreLabel()
	{
		if ((Object)(object)TxtM_ScoreLabel != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(TxtM_ScoreLabel, "LevelComplete_Score", Array.Empty<object>());
		}
	}

	public void SetBannerPointsVisible(bool visible)
	{
		if ((Object)(object)Img_BannerPoints != (Object)null)
		{
			((Component)Img_BannerPoints).gameObject.SetActive(visible);
		}
	}

	public void RefreshRoundTipText()
	{
		if ((Object)(object)roundText_Tip != (Object)null)
		{
			TMPTextExtensions.SetLocalizedText(roundText_Tip, "Text_Endless", Array.Empty<object>());
		}
	}

	public void SetLevel(int level)
	{
		GameMode currentGameMode = GameInfo.CurrentGameMode;
		RefreshBtnOK(LevelDifficultyUtil.GetDifficulty(currentGameMode, level), currentGameMode, level);
	}

	public void SetRoundTipsVisible(bool visible)
	{
		if ((Object)(object)round_tips_text != (Object)null)
		{
			round_tips_text.SetActive(visible);
		}
	}

	public void SetEvaluation(string text)
	{
		if ((Object)(object)TxtM_Evaluation != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtM_Evaluation, text);
		}
		halloweenTitleHandle?.GetView<UILevelWinHalloweenTitleView>()?.SetTitle(text);
	}

	public void SetPassText(string text)
	{
		if (!((Object)(object)TxtM_Pass == (Object)null))
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtM_Pass, text);
		}
	}

	public void SetLevelLeft(int levelsLeft)
	{
		if ((Object)(object)TxtM_LevelLeft != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(TxtM_LevelLeft, $"Only {levelsLeft} levels left!");
		}
	}

	public void SetLevelLeftText(string localizedText)
	{
		if (!((Object)(object)TxtM_LevelLeft == (Object)null))
		{
			LangBinder val = default(LangBinder);
			if (((Component)TxtM_LevelLeft).TryGetComponent<LangBinder>(ref val))
			{
				val.SetText(localizedText);
			}
			else
			{
				TMPTextExtensions.SetLocalizedTextValue(TxtM_LevelLeft, localizedText);
			}
		}
	}

	public void ShowGiftButton(bool show)
	{
		if ((Object)(object)Btn_Gift != (Object)null)
		{
			((Component)Btn_Gift).gameObject.SetActive(show);
		}
	}

	public void PlayRewardDisappear()
	{
	}

	public void PlayRewardAppear()
	{
	}

	public void InitBestScore()
	{
		ShutdownBestScore();
		if (!((Object)(object)BestScore == (Object)null))
		{
			bestScoreHandle = AttachWidget<UIBestScoreView>(BestScore);
			if (bestScoreHandle == null)
			{
				Log.LogError("UI", "[UILevelWinView] AttachWidget UIBestScoreView failed");
			}
			else
			{
				BestScoreView?.PrepareHidden();
			}
		}
	}

	public void ShutdownBestScore()
	{
		if (bestScoreHandle != null)
		{
			DetachWidget(bestScoreHandle);
			bestScoreHandle = null;
		}
	}

	public void ApplyGameWinIqPassLayout(bool enabled)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)TxtM_Pass == (Object)null)
		{
			return;
		}
		Transform parent = TxtM_Pass.transform.parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		if (!((Object)(object)val == (Object)null))
		{
			if (!passTextOffsetApplied)
			{
				passTextOriginalAnchoredY = val.anchoredPosition.y;
			}
			if (enabled)
			{
				val.anchoredPosition = new Vector2(val.anchoredPosition.x, passTextOriginalAnchoredY + 50f);
				passTextOffsetApplied = true;
			}
			else if (passTextOffsetApplied)
			{
				val.anchoredPosition = new Vector2(val.anchoredPosition.x, passTextOriginalAnchoredY);
				passTextOffsetApplied = false;
			}
		}
	}

	public void ShowGameWinIQ(GameWinIQSettlementSnapshot snapshot, bool playGrowthAutomatically = true)
	{
		if (!snapshot.ShouldShowUi || (Object)(object)Trans_IQ == (Object)null)
		{
			GameObject trans_IQ = Trans_IQ;
			if (trans_IQ != null)
			{
				trans_IQ.SetActive(false);
			}
			return;
		}
		Trans_IQ.SetActive(true);
		if (gameWinIqHandle == null)
		{
			gameWinIqHandle = AttachWidget<UIZhiShangXiTongView>("ZhiShangXiTong", Trans_IQ.transform, snapshot);
			UIZhiShangXiTongView uIZhiShangXiTongView = gameWinIqHandle?.GetView<UIZhiShangXiTongView>();
			if (uIZhiShangXiTongView == null)
			{
				Log.LogError("UI", "[UILevelWinView] AttachWidget ZhiShangXiTong failed");
				return;
			}
			uIZhiShangXiTongView.SetAutoPlayGrowth(playGrowthAutomatically);
			gameWinIqHandle.WithController<UIZhiShangXiTongController>(new UIZhiShangXiTongController(uIZhiShangXiTongView));
		}
		else
		{
			UIZhiShangXiTongController obj = gameWinIqHandle.Controller as UIZhiShangXiTongController;
			gameWinIqHandle.GetView<UIZhiShangXiTongView>()?.SetAutoPlayGrowth(playGrowthAutomatically);
			((BaseController<UIZhiShangXiTongView>)obj)?.OnRegister((object)snapshot);
		}
	}

	public float PlayGameWinIqAppear(AnimationClip clip)
	{
		return (gameWinIqHandle?.GetView<UIZhiShangXiTongView>()?.PlayAppearance(clip)).GetValueOrDefault();
	}

	public void PlayGameWinIqGrowth(float duration, Action onComplete)
	{
		UIZhiShangXiTongView uIZhiShangXiTongView = gameWinIqHandle?.GetView<UIZhiShangXiTongView>();
		if (uIZhiShangXiTongView == null)
		{
			onComplete?.Invoke();
		}
		else
		{
			uIZhiShangXiTongView.PlayPreparedGrowth(duration, onComplete);
		}
	}

	public void ShutdownGameWinIQ()
	{
		ApplyGameWinIqPassLayout(enabled: false);
		if (gameWinIqHandle != null)
		{
			DetachWidget(gameWinIqHandle);
			gameWinIqHandle = null;
		}
	}

	public void SetPassLocalizedKey(string langKey)
	{
		if (!((Object)(object)TxtM_Pass == (Object)null) && !string.IsNullOrEmpty(langKey))
		{
			TMPTextExtensions.SetLocalizedText(TxtM_Pass, langKey, Array.Empty<object>());
		}
	}

	public void EnsureContentVisible()
	{
		if ((Object)(object)Trans_StarScoreUI != (Object)null)
		{
			Trans_StarScoreUI.SetActive(true);
		}
		if ((Object)(object)Trans_Content != (Object)null)
		{
			((Component)Trans_Content).gameObject.SetActive(true);
		}
	}

	public void PlayNextBtnEffect()
	{
		if (!((Object)(object)Trans_Content == (Object)null))
		{
			((Component)Trans_Content).gameObject.SetActive(true);
			PlayClipWithRewind((Component)(object)Trans_Content, "anim_common_btn_effect");
			PlayNextBtnParticleEffect();
		}
	}

	public bool PlayFinalProgressEffect(GameObject effectPrefab)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)effectPrefab == (Object)null || (Object)(object)Trans_Spine == (Object)null || (Object)(object)Trans_Spine.transform.parent == (Object)null)
		{
			return false;
		}
		ClearFinalProgressEffect();
		Transform transform = ((Component)Btn_Gift).transform;
		finalProgressEffect = Object.Instantiate<GameObject>(effectPrefab, transform.parent, false);
		Transform transform2 = finalProgressEffect.transform;
		transform2.SetPositionAndRotation(transform.position, transform.rotation);
		transform2.localScale = Vector3.one;
		transform2.SetSiblingIndex(Mathf.Max(0, transform.GetSiblingIndex()));
		finalProgressEffect.SetActive(true);
		PlayUIParticleSystems(finalProgressEffect);
		return true;
	}

	[AsyncStateMachine(typeof(_003CBuildProgressNodesAsync_003Ed__114))]
	public UniTask<List<PreStageRewardProgress>> BuildProgressNodesAsync(string prefabLLocation, string prefabMLocation, string prefabRLocation, int count, CancellationToken cancellationToken = default(CancellationToken))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_003CBuildProgressNodesAsync_003Ed__114 _003CBuildProgressNodesAsync_003Ed__ = default(_003CBuildProgressNodesAsync_003Ed__114);
		_003CBuildProgressNodesAsync_003Ed__._003C_003Et__builder = AsyncUniTaskMethodBuilder<List<PreStageRewardProgress>>.Create();
		_003CBuildProgressNodesAsync_003Ed__._003C_003E4__this = this;
		_003CBuildProgressNodesAsync_003Ed__.prefabLLocation = prefabLLocation;
		_003CBuildProgressNodesAsync_003Ed__.prefabMLocation = prefabMLocation;
		_003CBuildProgressNodesAsync_003Ed__.prefabRLocation = prefabRLocation;
		_003CBuildProgressNodesAsync_003Ed__.count = count;
		_003CBuildProgressNodesAsync_003Ed__.cancellationToken = cancellationToken;
		_003CBuildProgressNodesAsync_003Ed__._003C_003E1__state = -1;
		_003CBuildProgressNodesAsync_003Ed__._003C_003Et__builder.Start<_003CBuildProgressNodesAsync_003Ed__114>(ref _003CBuildProgressNodesAsync_003Ed__);
		return _003CBuildProgressNodesAsync_003Ed__._003C_003Et__builder.Task;
	}

	private static void PlayUIParticleSystems(GameObject effect)
	{
		ParticleSystem[] componentsInChildren = effect.GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Simulate(0f, false, true);
			componentsInChildren[i].Play();
		}
	}

	private void ClearFinalProgressEffect()
	{
		if ((Object)(object)finalProgressEffect != (Object)null)
		{
			Object.Destroy((Object)(object)finalProgressEffect);
		}
		finalProgressEffect = null;
	}

	private void DetachProgressWidgets()
	{
		for (int num = progressWidgetHandles.Count - 1; num >= 0; num--)
		{
			WidgetHandle widgetHandle = progressWidgetHandles[num];
			if ((Object)(object)widgetHandle?.GameObject != (Object)null)
			{
				widgetHandle.GameObject.SetActive(false);
				widgetHandle.GameObject.transform.SetParent((Transform)null, false);
			}
			DetachWidget(widgetHandle);
		}
		progressWidgetHandles.Clear();
	}

	private static float PlayAppear(Component owner, bool originalPlayback = false)
	{
		if ((Object)(object)owner == (Object)null)
		{
			return 0f;
		}
		owner.gameObject.SetActive(true);
		Animation component = owner.GetComponent<Animation>();
		if (originalPlayback)
		{
			if ((Object)(object)component == (Object)null)
			{
				return 0f;
			}
			if ((Object)(object)component.clip != (Object)null)
			{
				RestoreOriginalWrapMode(component, ((Object)component.clip).name);
			}
			component.Play();
			if (!((Object)(object)component.clip != (Object)null))
			{
				return 0f;
			}
			return component.clip.length;
		}
		if ((Object)(object)component == (Object)null || (Object)(object)component.clip == (Object)null)
		{
			return 0f;
		}
		PlayClipWithRewind(owner, ((Object)component.clip).name);
		return component.clip.length;
	}

	private static void RestoreOriginalWrapMode(Animation animation, string clipName)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		AnimationClip clip = animation.GetClip(clipName);
		AnimationState val = animation[clipName];
		if ((Object)(object)clip != (Object)null && (TrackedReference)(object)val != (TrackedReference)null)
		{
			val.wrapMode = clip.wrapMode;
		}
	}

	private static void PlayClip(Component owner, string clipName)
	{
		PlayClipWithRewind(owner, clipName);
	}

	private static void PlayClipWithRewind(Component owner, string clipName)
	{
		if ((Object)(object)owner == (Object)null || string.IsNullOrEmpty(clipName))
		{
			return;
		}
		Animation component = owner.GetComponent<Animation>();
		if (!((Object)(object)component == (Object)null))
		{
			AnimationState val = component[clipName];
			if (!((TrackedReference)(object)val == (TrackedReference)null))
			{
				component.Stop(clipName);
				val.time = 0f;
				component.Sample();
				val.wrapMode = (WrapMode)8;
				component.Play(clipName);
			}
		}
	}

	private static void RewindAnimationClip(Animation animation, string clipName)
	{
		if (!((Object)(object)animation == (Object)null) && !string.IsNullOrEmpty(clipName))
		{
			AnimationState val = animation[clipName];
			if (!((TrackedReference)(object)val == (TrackedReference)null))
			{
				animation.Stop(clipName);
				val.time = 0f;
				val.enabled = true;
				animation.Sample();
			}
		}
	}

	private void PlayNextBtnParticleEffect()
	{
		eff_rank_btn_gp1.SetActive(true);
		PlayUIParticleSystems(eff_rank_btn_gp1);
	}

	public void PlayRootClip(string clipName)
	{
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		if (behaviour != null)
		{
			Animation component = ((Component)behaviour).GetComponent<Animation>();
			if (component != null)
			{
				component.Play(clipName);
			}
		}
	}

	private float PlayRootClipAndGetDuration(string clipName)
	{
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		Animation val = ((behaviour != null) ? ((Component)behaviour).GetComponent<Animation>() : null);
		if ((Object)(object)val == (Object)null || string.IsNullOrEmpty(clipName))
		{
			return 0f;
		}
		AnimationState val2 = val[clipName];
		if ((TrackedReference)(object)val2 == (TrackedReference)null)
		{
			return 0f;
		}
		if (val.IsPlaying(clipName))
		{
			val.Stop(clipName);
			val2.time = 0f;
			val.Sample();
		}
		val2.wrapMode = (WrapMode)8;
		val.Play(clipName);
		return val2.length;
	}

	private static void SetActive(Component owner, bool active)
	{
		if ((Object)(object)owner != (Object)null)
		{
			owner.gameObject.SetActive(active);
		}
	}

	public override void Dispose()
	{
		if (scoreHandle != null)
		{
			DetachWidget(scoreHandle);
			scoreHandle = null;
		}
		StopIdle3SoundForRelease();
		UnsubscribeIdleLoopSound();
		ClearFinalProgressEffect();
		DetachProgressWidgets();
		ShutdownBestScore();
		ShutdownBtnOK();
		ShutdownHalloweenTitle();
		DestroyHalloweenBtnOK();
		DestroyHalloweenTitle();
		ShutdownGameWinIQ();
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		Img_FullBg = behaviour.Get<Image>("Img_FullBg");
		Trans_TopBar = behaviour.Get<RectTransform>("Trans_TopBar");
		TxtM_Evaluation = behaviour.Get<TMP_Text>("TxtM_Evaluation");
		TxtM_Pass = behaviour.Get<TMP_Text>("TxtM_Pass");
		Trans_CardContainer = behaviour.Get<RectTransform>("Trans_CardContainer");
		Trans_Content = behaviour.Get<RectTransform>("Trans_Content");
		Trans_Bottom = behaviour.Get<RectTransform>("Trans_Bottom");
		Trans_StageReward = behaviour.Get<RectTransform>("Trans_StageReward");
		TxtM_LevelLeft = behaviour.Get<TMP_Text>("TxtM_LevelLeft");
		Trans_Progress = behaviour.Get<RectTransform>("Trans_Progress");
		Btn_Gift = behaviour.Get<Button>("Btn_Gift");
		Trans_Spine = behaviour.Get<GameObject>("Trans_Spine");
		Trans_Effect = behaviour.Get<GameObject>("Trans_Effect");
		Img_Ribbon = behaviour.Get<Image>("Img_Ribbon");
		UIZoodokuGameWin = behaviour.Get<Animation>("UIZoodokuGameWin");
		Trans_Text = behaviour.Get<GameObject>("Trans_Text");
		Trans_StarScoreUI = behaviour.Get<GameObject>("Trans_StarScoreUI");
		TxtM_ScoreNum = behaviour.Get<TMP_Text>("TxtM_ScoreNum");
		TxtM_ScoreLabel = behaviour.Get<TMP_Text>("TxtM_ScoreLabel");
		Img_BannerPoints = behaviour.Get<Image>("Img_BannerPoints");
		Trans_Spine_Play = behaviour.Get<UISpineLoader>("Trans_Spine_Play");
		eff_rank_btn_gp1 = behaviour.Get<GameObject>("eff_rank_btn_gp1");
		Root = behaviour.Get<GameObject>("Root");
		round_tips_text = behaviour.Get<GameObject>("round_tips_text");
		roundText_Tip = behaviour.Get<TMP_Text>("roundText_Tip");
		eff_results_fullscreen_gp2 = behaviour.Get<GameObject>("eff_results_fullscreen_gp2");
		eff_results_fullscreen_gp3 = behaviour.Get<GameObject>("eff_results_fullscreen_gp3");
		BestScore = behaviour.Get<UIBehaviour>("BestScore");
		BtnOK = behaviour.Get<UIBehaviour>("BtnOK");
		Trans_IQ = behaviour.Get<GameObject>("Trans_IQ");
	}
}
