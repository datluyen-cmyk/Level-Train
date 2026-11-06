using Game.UI;
using RotateRings.UI.Common;
using RotateRings.UI.Level;
using RotateRings.UI.Moves;
using RotateRings.UI.Props;
using RotateRings.UI.Score;
using RotateRings.UI.Setting;
using RotateRings.UI.SubView;
using Rotaterings.UI.RankList;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace RotateRings.UI.Game;

public class UIGameView : GameBaseView
{
	private const string AnimUigameCombo1 = "anim_uigame_combo_1";

	private const string OpenAnimName = "anim_uigame_in";

	private const string CloseAnimName = "anim_uigame_out";

	private const string FpsWidgetLocation = "UIFPS";

	private const string ScoreWidgetLocation = "Score";

	private const string MovesWidgetLocation = "Moves";

	private const string LevelWidgetLocation = "Level";

	private const string ScoreByStepLimitWidgetLocation = "ScoreByStepLimitExperiment";

	private WidgetHandle settingHandle;

	private WidgetHandle hudWidgetHandle;

	private WidgetHandle leftWidgetHandle;

	private WidgetHandle propsHandle;

	private WidgetHandle rankStarCollectionHandle;

	private string rankStarCollectionLocation;

	private WidgetHandle fpsHandle;

	private int collectLayout;

	private RectTransformLayoutData collectAnchorPose = RankCollectPositionControlData.RankStarAnchor;

	private static readonly RectTransformLayoutData ZoomRestoreRightPose = new RectTransformLayoutData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(449f, -168f));

	private static readonly RectTransformLayoutData ZoomRestoreLeftPose = new RectTransformLayoutData(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-449f, -168f));

	private Transform chainLockOriginalParent;

	private int chainLockOriginalSiblingIndex;

	private bool externalInputBlocked;

	private bool gameplayDragInputBlocked;

	public GraphicRaycaster UIGameGraphicRaycaster;

	public RectTransform TopContent;

	public RectTransform CenterContent;

	public RectTransform PropContent;

	public UIBehaviour Setting;

	public UIBehaviour Props;

	public RectTransform UnlockPropAnchor;

	public RectTransform RankStarAnhor;

	public RectTransform GameRect;

	public RectTransform RingRect;

	public GameObject LoadingMask;

	public Animation Anim;

	public RectTransform CollectionPanel;

	public RectTransform ChainLockContent;

	public Button zoomRestoreBtn;

	public RectMask2D ContentRectMask;

	public RectTransform ThemeNode;

	public Image imgShop;

	public Button btnShop;

	public RectTransform btnShopEffectTrans;

	public RectTransform Middle;

	public RectTransform Left;

	public UISettingView SettingView => EnsureWidget<UISettingView>(ref settingHandle, Setting);

	public UIScoreView ScoreView => hudWidgetHandle?.GetView<UIScoreView>();

	public UIMovesView MovesView => hudWidgetHandle?.GetView<UIMovesView>();

	public UILevelView LevelView => leftWidgetHandle?.GetView<UILevelView>();

	public UIPropsView PropsView => EnsureWidget<UIPropsView>(ref propsHandle, Props);

	public UIRankStarCollectionView RankStarCollectionView
	{
		get
		{
			string collectionLocation = RankCollectThemeResourceResolver.GetCollectionLocation();
			if (rankStarCollectionHandle != null && !rankStarCollectionHandle.IsDisposed && rankStarCollectionLocation != collectionLocation)
			{
				DetachWidget(rankStarCollectionHandle);
				rankStarCollectionHandle = null;
			}
			UIRankStarCollectionView loadedRankStarCollectionView = LoadedRankStarCollectionView;
			if (loadedRankStarCollectionView != null)
			{
				return loadedRankStarCollectionView;
			}
			collectAnchorPose.TryApply(RankStarAnhor);
			UIRankStarCollectionView uIRankStarCollectionView = EnsureWidget<UIRankStarCollectionView>(ref rankStarCollectionHandle, collectionLocation, (Transform)(object)RankStarAnhor);
			if (uIRankStarCollectionView != null)
			{
				rankStarCollectionLocation = collectionLocation;
				uIRankStarCollectionView.ConfigureSide(collectLayout >= 2);
			}
			return uIRankStarCollectionView;
		}
	}

	public UIRankStarCollectionView LoadedRankStarCollectionView => rankStarCollectionHandle?.GetView<UIRankStarCollectionView>();

	public void EnsureHudWidgetAsScore()
	{
		EnsureHudWidget<UIScoreView>("Score");
	}

	public void EnsureHudWidgetAsMoves()
	{
		EnsureHudWidget<UIMovesView>("Moves");
		MovesView?.SetupLocalizedText();
		EnsureMovesController();
	}

	private void EnsureMovesController()
	{
		if (hudWidgetHandle != null && !hudWidgetHandle.IsDisposed && hudWidgetHandle.Controller == null)
		{
			UIMovesView view = hudWidgetHandle.GetView<UIMovesView>();
			if (view != null)
			{
				hudWidgetHandle.WithController<UIMovesController>(new UIMovesController(view));
			}
		}
	}

	private void EnsureHudWidget<TWidget>(string location) where TWidget : GameWidgetView, new()
	{
		if (hudWidgetHandle != null && !hudWidgetHandle.IsDisposed)
		{
			if (hudWidgetHandle.GetView<TWidget>() != null)
			{
				return;
			}
			DetachWidget(hudWidgetHandle);
			hudWidgetHandle = null;
		}
		object obj;
		if (!((Object)(object)Middle != (Object)null))
		{
			UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
			obj = ((behaviour != null) ? ((Component)behaviour).transform : null);
		}
		else
		{
			obj = Middle;
		}
		Transform parent = (Transform)obj;
		hudWidgetHandle = AttachWidget<TWidget>(location, parent);
	}

	public void EnsureLeftWidgetAsLevel()
	{
		EnsureLeftWidget<UILevelView>("Level");
	}

	public void EnsureLeftWidgetAsScoreByStepLimit()
	{
		EnsureLeftWidget<UIScoreByStepLimitExperimentView>("ScoreByStepLimitExperiment");
		EnsureScoreByStepLimitController();
	}

	private void EnsureScoreByStepLimitController()
	{
		if (leftWidgetHandle != null && !leftWidgetHandle.IsDisposed && leftWidgetHandle.Controller == null)
		{
			UIScoreByStepLimitExperimentView view = leftWidgetHandle.GetView<UIScoreByStepLimitExperimentView>();
			if (view != null)
			{
				leftWidgetHandle.WithController<UIScoreByStepLimitExperimentController>(new UIScoreByStepLimitExperimentController(view));
			}
		}
	}

	private void EnsureLeftWidget<TWidget>(string location) where TWidget : GameWidgetView, new()
	{
		if (leftWidgetHandle != null && !leftWidgetHandle.IsDisposed)
		{
			if (leftWidgetHandle.GetView<TWidget>() != null)
			{
				return;
			}
			DetachWidget(leftWidgetHandle);
			leftWidgetHandle = null;
		}
		object obj;
		if (!((Object)(object)Left != (Object)null))
		{
			UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
			obj = ((behaviour != null) ? ((Component)behaviour).transform : null);
		}
		else
		{
			obj = Left;
		}
		Transform parent = (Transform)obj;
		leftWidgetHandle = AttachWidget<TWidget>(location, parent);
	}

	public bool SetCollectPosition(int layout, RectTransformLayoutData pose)
	{
		if (layout < 0 || layout > 3 || !pose.TryApply(RankStarAnhor))
		{
			return false;
		}
		collectLayout = layout;
		collectAnchorPose = pose;
		LoadedRankStarCollectionView?.ConfigureSide(collectLayout >= 2);
		ApplyZoomRestoreButtonSide(collectLayout);
		return true;
	}

	private void ApplyZoomRestoreButtonSide(int layout)
	{
		if (!((Object)(object)zoomRestoreBtn == (Object)null))
		{
			Transform transform = ((Component)zoomRestoreBtn).transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (!((Object)(object)val == (Object)null))
			{
				((layout >= 2) ? ZoomRestoreLeftPose : ZoomRestoreRightPose).TryApply(val);
			}
		}
	}

	public void Init()
	{
		int layout = RankCollectPositionExperiment.ReadLayout();
		SetCollectPosition(layout, RankCollectPositionExperiment.ReadAnchor(layout));
		SetLoadingMaskVisible(visible: false);
		SetZoomRestoreButtonVisible(visible: false);
	}

	public void SetFpsVisible(bool visible)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (visible)
		{
			if (fpsHandle == null || fpsHandle.IsDisposed)
			{
				fpsHandle = null;
				object obj;
				if (!((Object)(object)TopContent != (Object)null))
				{
					UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
					obj = ((behaviour != null) ? ((Component)behaviour).transform : null);
				}
				else
				{
					obj = TopContent;
				}
				Transform parent = (Transform)obj;
				fpsHandle = AttachWidget<UIFPSView>("UIFPS", parent);
				if ((Object)(object)fpsHandle?.GameObject != (Object)null)
				{
					fpsHandle.GameObject.transform.localScale = Vector3.one;
				}
			}
		}
		else if (fpsHandle == null || fpsHandle.IsDisposed)
		{
			fpsHandle = null;
		}
		else
		{
			DetachWidget(fpsHandle);
			fpsHandle = null;
		}
	}

	public void SetVisible(bool visible)
	{
		if (!((Object)(object)((UIBaseView)this).Behaviour == (Object)null) && !((Object)(object)((Component)((UIBaseView)this).Behaviour).gameObject == (Object)null) && ((Component)((UIBaseView)this).Behaviour).gameObject.activeSelf != visible)
		{
			_ = ((Component)((UIBaseView)this).Behaviour).gameObject.activeSelf;
			((Component)((UIBaseView)this).Behaviour).gameObject.SetActive(visible);
		}
	}

	public void SetLoadingMaskVisible(bool visible)
	{
		if ((Object)(object)LoadingMask != (Object)null)
		{
			LoadingMask.SetActive(visible);
		}
	}

	public void SetZoomRestoreButtonVisible(bool visible)
	{
		if ((Object)(object)zoomRestoreBtn != (Object)null)
		{
			((Component)zoomRestoreBtn).gameObject.SetActive(visible);
		}
	}

	public bool IsZoomRestoreButtonVisible()
	{
		if ((Object)(object)zoomRestoreBtn != (Object)null)
		{
			return ((Component)zoomRestoreBtn).gameObject.activeSelf;
		}
		return false;
	}

	public float PlayEnterTransitionStart()
	{
		return 0f;
	}

	public void SetInputBlocked(bool blocked)
	{
		externalInputBlocked = blocked;
		ApplyInputBlockState();
	}

	public void SetGameplayDragInputBlocked(bool blocked)
	{
		gameplayDragInputBlocked = blocked;
		ApplyInputBlockState();
	}

	private void ApplyInputBlockState()
	{
		if (!((Object)(object)UIGameGraphicRaycaster == (Object)null))
		{
			((Behaviour)UIGameGraphicRaycaster).enabled = !externalInputBlocked && !gameplayDragInputBlocked;
		}
	}

	public void SetChainLockReparent(Transform guideRoot, RectTransform insertBefore, bool reparent)
	{
		if ((Object)(object)ChainLockContent == (Object)null)
		{
			return;
		}
		if (!reparent)
		{
			RestoreChainLockParent();
		}
		else if (!((Object)(object)guideRoot == (Object)null) && !((Object)(object)insertBefore == (Object)null))
		{
			if ((Object)(object)chainLockOriginalParent == (Object)null)
			{
				chainLockOriginalParent = ((Transform)ChainLockContent).parent;
				chainLockOriginalSiblingIndex = ((Transform)ChainLockContent).GetSiblingIndex();
			}
			((Transform)ChainLockContent).SetParent(guideRoot, true);
			((Transform)ChainLockContent).SetSiblingIndex(((Transform)insertBefore).GetSiblingIndex());
		}
	}

	private void RestoreChainLockParent()
	{
		if (!((Object)(object)chainLockOriginalParent == (Object)null))
		{
			Transform val = chainLockOriginalParent;
			int num = chainLockOriginalSiblingIndex;
			chainLockOriginalParent = null;
			if (!((Object)(object)val == (Object)null))
			{
				((Transform)ChainLockContent).SetParent(val, true);
				((Transform)ChainLockContent).SetSiblingIndex(Mathf.Clamp(num, 0, val.childCount - 1));
			}
		}
	}

	public bool SnapCloseAnimationToEnd()
	{
		StopAnimationIfPlaying("anim_uigame_in");
		return SnapAnimationToEnd("anim_uigame_out");
	}

	public bool SnapOpenAnimationToEnd()
	{
		StopAnimationIfPlaying("anim_uigame_out");
		return SnapAnimationToEnd("anim_uigame_in");
	}

	public bool IsCloseAnimationEndPoseActive()
	{
		if ((Object)(object)Anim == (Object)null || string.IsNullOrEmpty("anim_uigame_out"))
		{
			return false;
		}
		AnimationState val = Anim["anim_uigame_out"];
		if ((TrackedReference)(object)val == (TrackedReference)null || !val.enabled)
		{
			return false;
		}
		if (Mathf.Abs(val.speed) < 0.001f)
		{
			return val.time >= val.length - 0.001f;
		}
		return false;
	}

	public bool TryGetGuideAlignScreenCenter(RectTransform anchor, out Vector2 screenCenter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		screenCenter = default(Vector2);
		if ((Object)(object)anchor == (Object)null)
		{
			return false;
		}
		bool flag = IsCloseAnimationEndPoseActive();
		if (flag)
		{
			SnapOpenAnimationToEnd();
		}
		Canvas.ForceUpdateCanvases();
		bool result = TryGetRectScreenCenter(anchor, out screenCenter);
		if (flag)
		{
			SnapCloseAnimationToEnd();
			Canvas.ForceUpdateCanvases();
		}
		return result;
	}

	public float PlayOpenAnimation()
	{
		return PlayAnimationClip(Anim, "anim_uigame_in");
	}

	public float PlayCloseAnimation()
	{
		return PlayAnimationClip(Anim, "anim_uigame_out");
	}

	private bool SnapAnimationToEnd(string animName)
	{
		if ((Object)(object)Anim == (Object)null || string.IsNullOrEmpty(animName))
		{
			return false;
		}
		AnimationState val = Anim[animName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return false;
		}
		Anim.Stop();
		val.wrapMode = (WrapMode)8;
		val.enabled = true;
		val.speed = 0f;
		val.time = val.length;
		Anim.Play(animName);
		Anim.Sample();
		return true;
	}

	private static bool TryGetRectScreenCenter(RectTransform rect, out Vector2 screenCenter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		screenCenter = default(Vector2);
		if ((Object)(object)rect == (Object)null)
		{
			return false;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rect.GetWorldCorners(array);
		Vector3 val = (array[0] + array[2]) * 0.5f;
		screenCenter = RectTransformUtility.WorldToScreenPoint((Camera)null, val);
		return true;
	}

	private void StopAnimationIfPlaying(string animName)
	{
		if (!((Object)(object)Anim == (Object)null) && !string.IsNullOrEmpty(animName) && Anim.IsPlaying(animName))
		{
			Anim.Stop(animName);
		}
	}

	private static float PlayAnimationClip(Animation anim, string animName)
	{
		if ((Object)(object)anim == (Object)null || string.IsNullOrEmpty(animName))
		{
			return 0f;
		}
		AnimationState val = anim[animName];
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		if (anim.IsPlaying(animName))
		{
			anim.Stop(animName);
			val.time = 0f;
			anim.Sample();
		}
		val.speed = 1f;
		val.wrapMode = (WrapMode)8;
		anim.Play(animName);
		return val.length;
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		UIGameGraphicRaycaster = behaviour.Get<GraphicRaycaster>("UIGameGraphicRaycaster");
		TopContent = behaviour.Get<RectTransform>("TopContent");
		CenterContent = behaviour.Get<RectTransform>("CenterContent");
		PropContent = behaviour.Get<RectTransform>("PropContent");
		Setting = behaviour.Get<UIBehaviour>("Setting");
		Props = behaviour.Get<UIBehaviour>("Props");
		UnlockPropAnchor = behaviour.Get<RectTransform>("UnlockPropAnchor");
		RankStarAnhor = behaviour.Get<RectTransform>("RankStarAnhor");
		GameRect = behaviour.Get<RectTransform>("GameRect");
		RingRect = behaviour.Get<RectTransform>("RingRect");
		LoadingMask = behaviour.Get<GameObject>("LoadingMask");
		Anim = behaviour.Get<Animation>("Anim");
		CollectionPanel = behaviour.Get<RectTransform>("CollectionPanel");
		ChainLockContent = behaviour.Get<RectTransform>("ChainLockContent");
		zoomRestoreBtn = behaviour.Get<Button>("zoomRestoreBtn");
		ContentRectMask = behaviour.Get<RectMask2D>("ContentRectMask");
		ThemeNode = behaviour.Get<RectTransform>("ThemeNode");
		imgShop = behaviour.Get<Image>("imgShop");
		btnShop = behaviour.Get<Button>("btnShop");
		btnShopEffectTrans = behaviour.Get<RectTransform>("btnShopEffectTrans");
		Middle = behaviour.Get<RectTransform>("Middle");
		Left = behaviour.Get<RectTransform>("Left");
	}
}
