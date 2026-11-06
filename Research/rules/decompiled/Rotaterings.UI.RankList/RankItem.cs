using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rotaterings.Rank;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using XingYun.Extensions;

namespace Rotaterings.UI.RankList;

public class RankItem : MonoBehaviour
{
	private struct MainVisualStyleSnapshot
	{
		public Sprite ImgBgSprite;

		public Color TxtRankNumColor;

		public Color TxtNameColor;

		public Sprite BgCollectionSprite;

		public Color TxtCollectNumColor;
	}

	public enum ERanItemAnimType
	{
		ItemUp,
		ItemDown,
		ItemOpen
	}

	[SerializeField]
	private Animation rootAni;

	[SerializeField]
	private RectTransform nodeRect;

	[SerializeField]
	private CanvasGroup nodeCanvasGroup;

	[SerializeField]
	private Animation nodeAnimation;

	[SerializeField]
	private Animation startAndEndAni;

	[SerializeField]
	private GameObject rewardBoxShadow;

	[SerializeField]
	private GameObject rewardBoxShadow_own;

	[SerializeField]
	private Button btnRewardBox;

	[SerializeField]
	private Button btnArrowRight;

	[SerializeField]
	private Animation headRootAni;

	[SerializeField]
	private Image imgHead;

	[SerializeField]
	private Image imgHeadFrame;

	[SerializeField]
	public Image imgNoRank;

	[SerializeField]
	private TMP_Text txtName;

	[SerializeField]
	private TMP_Text txtRankNum;

	[SerializeField]
	private TMP_Text txtCollectNum;

	[SerializeField]
	private Transform imgBg;

	[SerializeField]
	private Transform imgCollect;

	[SerializeField]
	private Transform imgOutline;

	[SerializeField]
	private GameObject rankUpEffect;

	[SerializeField]
	private GameObject rankDownEffect;

	[SerializeField]
	private GameObject boxOutline;

	[SerializeField]
	private GameObject boxLight;

	private const string RankItemUp = "anim_RankItemSelf_up_gp1";

	private const string RankItemDown = "anim_RankItemSelf_down_gp1";

	private const string RankItemOpen = "anim_RankItemSelf_open_gp1";

	private const string GlowIn = "anim_rank_scoreboard_glow_in_gp1";

	private const string GlowClose = "anim_rank_scoreboard_glow_close_gp1";

	private const string UptoTop3AnimName = "anim_rankitem_uptotop3_1_gp1";

	private const string Top3UpAnimName = "anim_rankitem_top3up_gp1";

	private const string Top3DownAnimName = "anim_rankitem_top3down_gp1";

	private const string LeaderboardHeadFrameSpriteName = "leaderboard_avatar_frame";

	private const string SelfPlayerNameLanguageKey = "LeaderBoard_PlayerName_You";

	private const float RewardExperimentNameWidth = 285f;

	public Action<int, Transform> OnRewardBoxClicked;

	private Vector2 nodeOriginPos;

	private float currentVisualOffsetY;

	private Vector3 nodeOriginScale = Vector3.one;

	private bool isOriginCaptured;

	private Transform headRootOriginParent;

	private Transform headRootVisual;

	private Vector3 headRootOriginLocalPos;

	private Vector3 headRootOriginLocalScale = Vector3.one;

	private Vector3 headRootVisualOriginLocalScale = Vector3.one;

	private bool headRootOriginCaptured;

	private GameObject transferredDownEffect;

	private ERankContent rankType;

	private RankItemData entry;

	private int rankIndex;

	private bool isSettleTopThree;

	private int headLoadGeneration;

	private bool collectPositionConfigured;

	private bool collectPositionTopHeader;

	private int formalCollectIconSide;

	private float defaultNameWidth;

	private float defaultNameFontSize;

	private float defaultNameFontSizeMin;

	private float defaultNameFontSizeMax;

	private bool defaultNameAutoSizing;

	private TextOverflowModes defaultNameOverflowMode;

	private CanvasGroup rewardBoxCanvasGroup;

	private CanvasGroup rewardBoxShadowCanvasGroup;

	private CanvasGroup rewardBoxOwnShadowCanvasGroup;

	private bool defaultImgBgActive;

	private bool defaultImgCollectActive;

	private bool defaultArrowRightActive;

	private static MainVisualStyleSnapshot selfMainStyle;

	private static MainVisualStyleSnapshot botMainStyle;

	private static bool mainVisualStyleReady;

	public Transform ImgCollect => imgCollect;

	public int RankIndex => rankIndex;

	public Vector3 HeadWorldPosition
	{
		get
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)imgHead != (Object)null))
			{
				return ((Component)this).transform.position;
			}
			return ((Component)imgHead).transform.position;
		}
	}

	public Transform HeadRootTransform
	{
		get
		{
			if (!((Object)(object)headRootAni != (Object)null))
			{
				return null;
			}
			return ((Component)headRootAni).transform;
		}
	}

	public RectTransform RewardBoxRectTransform
	{
		get
		{
			if (!((Object)(object)btnRewardBox != (Object)null))
			{
				return null;
			}
			Transform transform = ((Component)btnRewardBox).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private void Awake()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		CacheRewardBoxCanvasGroups();
		CacheDefaultNameStyle();
		CacheDefaultVisualState();
		if ((Object)(object)btnRewardBox != (Object)null)
		{
			((UnityEvent)btnRewardBox.onClick).AddListener(new UnityAction(OnRewardBoxClick));
		}
		if ((Object)(object)btnArrowRight != (Object)null)
		{
			((UnityEvent)btnArrowRight.onClick).AddListener(new UnityAction(OnArrowRightClick));
		}
		HideRewardBoxExperimentVisuals();
	}

	private void OnEnable()
	{
		if (collectPositionConfigured)
		{
			ApplyCollectPosition();
		}
	}

	private void CacheRewardBoxCanvasGroups()
	{
		rewardBoxCanvasGroup = GetOrAddCanvasGroup(((Object)(object)btnRewardBox != (Object)null) ? ((Component)btnRewardBox).gameObject : null);
		rewardBoxShadowCanvasGroup = GetOrAddCanvasGroup(rewardBoxShadow);
		rewardBoxOwnShadowCanvasGroup = GetOrAddCanvasGroup(rewardBoxShadow_own);
	}

	private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return null;
		}
		CanvasGroup component = target.GetComponent<CanvasGroup>();
		if (!((Object)(object)component != (Object)null))
		{
			return target.AddComponent<CanvasGroup>();
		}
		return component;
	}

	private void CacheDefaultNameStyle()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)txtName == (Object)null))
		{
			Rect rect = txtName.rectTransform.rect;
			defaultNameWidth = ((Rect)(ref rect)).width;
			defaultNameFontSize = txtName.fontSize;
			defaultNameFontSizeMin = txtName.fontSizeMin;
			defaultNameFontSizeMax = txtName.fontSizeMax;
			defaultNameAutoSizing = txtName.enableAutoSizing;
			defaultNameOverflowMode = txtName.overflowMode;
		}
	}

	private void CacheDefaultVisualState()
	{
		defaultImgBgActive = (Object)(object)imgBg != (Object)null && ((Component)imgBg).gameObject.activeSelf;
		defaultImgCollectActive = (Object)(object)imgCollect != (Object)null && ((Component)imgCollect).gameObject.activeSelf;
		defaultArrowRightActive = (Object)(object)btnArrowRight != (Object)null && ((Component)btnArrowRight).gameObject.activeSelf;
	}

	public void InitializeCollectPosition(bool topHeader, int iconSide)
	{
		collectPositionConfigured = true;
		collectPositionTopHeader = topHeader;
		formalCollectIconSide = ((iconSide == 1) ? 1 : 0);
	}

	public void CopyCollectPositionFrom(RankItem source)
	{
		if (!((Object)(object)source == (Object)null))
		{
			InitializeCollectPosition(source.collectPositionTopHeader, source.formalCollectIconSide);
		}
	}

	private void ApplyCollectPosition()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (collectPositionConfigured)
		{
			RankCollectPairPosition rankCollectPairPosition = RankCollectPositionExperiment.ResolveRank(collectPositionTopHeader, formalCollectIconSide);
			Transform obj = imgCollect;
			RectTransform val = (RectTransform)(object)((obj is RectTransform) ? obj : null);
			if (val != null && val.anchoredPosition != rankCollectPairPosition.ImgCollectPosition)
			{
				val.anchoredPosition = rankCollectPairPosition.ImgCollectPosition;
			}
			if ((Object)(object)txtCollectNum != (Object)null && txtCollectNum.rectTransform.anchoredPosition != rankCollectPairPosition.TxtCollectNumPosition)
			{
				txtCollectNum.rectTransform.anchoredPosition = rankCollectPairPosition.TxtCollectNumPosition;
			}
		}
	}

	public void SetSettleTopThree(bool value)
	{
		isSettleTopThree = value;
	}

	public Vector2 GetRewardBoxWorldPosition()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)btnRewardBox != (Object)null))
		{
			return Vector2.zero;
		}
		return new Vector2(((Component)btnRewardBox).transform.position.x, ((Component)btnRewardBox).transform.position.y);
	}

	public void SetRankType(ERankContent type)
	{
		rankType = type;
	}

	public void Render(RankItemData entry, bool hideVisual = false)
	{
		if (!collectPositionConfigured)
		{
			InitializeCollectPosition(topHeader: false, RankCollectPositionExperiment.ReadIconSide());
		}
		CaptureOrigin();
		CaptureHeadRootOrigin();
		ResetReusableVisualState();
		ResetNodeAnimation();
		OnRelease();
		this.entry = entry;
		rankIndex = entry?.RankIndex ?? (-1);
		SetBoxActive(rankIndex > 0 && rankIndex <= RankConst.CurrentRewardRankLimit);
		SetVisualOffsetY(0f);
		SetVisible(!hideVisual);
		SetRankNumber(rankIndex);
		headLoadGeneration++;
		SetHeadIcon();
		SetRankCollectionNum();
		SetRankName();
		ApplyRewardExperimentNameStyle();
		SetContentState();
		SetTopThreeOutline();
		ApplyMainVisualStyle();
		SetNodeActive(active: true);
		ApplyCollectPosition();
		ApplyRewardBoxExperimentVisuals(revealReady: false);
	}

	public void RevealRewardBoxExperimentVisuals()
	{
		ApplyRewardBoxExperimentVisuals(revealReady: true);
	}

	private void ApplyRewardBoxExperimentVisuals(bool revealReady)
	{
		if (!HasRewardBoxVisible())
		{
			HideRewardBoxExperimentVisuals();
			return;
		}
		bool active = RankFunctionResolver.ShouldShowBoxOutline();
		bool active2 = RankFunctionResolver.ShouldShowBoxLight();
		if (!revealReady)
		{
			HideRewardBoxExperimentVisuals();
			return;
		}
		SetRewardBoxEffectActive(boxOutline, active);
		SetRewardBoxEffectActive(boxLight, active2);
	}

	private void HideRewardBoxExperimentVisuals()
	{
		SetRewardBoxEffectActive(boxOutline, active: false);
		SetRewardBoxEffectActive(boxLight, active: false);
	}

	private bool HasRewardBoxVisible()
	{
		if ((Object)(object)btnRewardBox != (Object)null && ((Component)btnRewardBox).gameObject.activeSelf && rankIndex > 0)
		{
			return rankIndex <= RankConst.CurrentRewardRankLimit;
		}
		return false;
	}

	private void SetRewardBoxEffectActive(GameObject target, bool active)
	{
		if ((Object)(object)target != (Object)null)
		{
			target.SetActive(active);
		}
	}

	public static void InitializeMainVisualStyle(RankItem selfSource, RankItem botSource)
	{
		if ((Object)(object)selfSource == (Object)null || (Object)(object)botSource == (Object)null)
		{
			mainVisualStyleReady = false;
			return;
		}
		selfMainStyle = selfSource.BuildMainVisualStyleSnapshot();
		botMainStyle = botSource.BuildMainVisualStyleSnapshot();
		mainVisualStyleReady = true;
	}

	public void SetCollectScore(int num)
	{
		if ((Object)(object)txtCollectNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtCollectNum, num.ToString());
		}
	}

	public void SetRankNumber(int rankIndex)
	{
		if ((Object)(object)txtRankNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtRankNum, rankIndex.ToString());
			((Component)txtRankNum).gameObject.SetActive(rankIndex > 0 && rankIndex <= RankConst.MaxRankBot);
		}
		if ((Object)(object)imgNoRank != (Object)null)
		{
			((Component)imgNoRank).gameObject.SetActive(rankIndex < 0 || rankIndex > RankConst.MaxRankBot);
		}
	}

	public void SetVisible(bool visible)
	{
		if (!((Object)(object)nodeCanvasGroup == (Object)null))
		{
			nodeCanvasGroup.alpha = (visible ? 1f : 0f);
			nodeCanvasGroup.blocksRaycasts = visible;
			nodeCanvasGroup.interactable = visible;
		}
	}

	public void SetVisualOffsetY(float offsetY)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)nodeRect == (Object)null) && !Mathf.Approximately(currentVisualOffsetY, offsetY))
		{
			currentVisualOffsetY = offsetY;
			nodeRect.anchoredPosition = nodeOriginPos + new Vector2(0f, offsetY);
		}
	}

	public void SetNodeActive(bool active)
	{
		if ((Object)(object)nodeRect != (Object)null)
		{
			((Component)nodeRect).gameObject.SetActive(active);
		}
	}

	public void FadeOutRewardBox(float duration)
	{
		FadeOutRewardBoxNode(((Object)(object)btnRewardBox != (Object)null) ? ((Component)btnRewardBox).gameObject : null, rewardBoxCanvasGroup, duration);
		FadeOutRewardBoxNode(rewardBoxShadow, rewardBoxShadowCanvasGroup, duration);
		FadeOutRewardBoxNode(rewardBoxShadow_own, rewardBoxOwnShadowCanvasGroup, duration);
	}

	public void FadeInRewardBoxAndOwnShadow(int targetRank, float duration)
	{
		if (targetRank > 0 && targetRank <= RankConst.CurrentRewardRankLimit)
		{
			FadeInRewardBoxNode(((Object)(object)btnRewardBox != (Object)null) ? ((Component)btnRewardBox).gameObject : null, rewardBoxCanvasGroup, duration);
			if (!isSettleTopThree && targetRank > 3)
			{
				FadeInRewardBoxNode(rewardBoxShadow_own, rewardBoxOwnShadowCanvasGroup, duration);
			}
		}
	}

	private static void FadeOutRewardBoxNode(GameObject target, CanvasGroup canvasGroup, float duration)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		if (!((Object)(object)target == (Object)null) && target.activeSelf && !((Object)(object)canvasGroup == (Object)null))
		{
			ShortcutExtensions.DOKill((Component)(object)canvasGroup, false);
			canvasGroup.alpha = 1f;
			TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, duration), (TweenCallback)delegate
			{
				target.SetActive(false);
			});
		}
	}

	private static void FadeInRewardBoxNode(GameObject target, CanvasGroup canvasGroup, float duration)
	{
		if (!((Object)(object)target == (Object)null) && !((Object)(object)canvasGroup == (Object)null))
		{
			target.SetActive(true);
			ShortcutExtensions.DOKill((Component)(object)canvasGroup, false);
			canvasGroup.alpha = 0f;
			DOTweenModuleUI.DOFade(canvasGroup, 1f, duration);
		}
	}

	public void SetBoxActive(bool active)
	{
		SetRewardBoxNodeActive(((Object)(object)btnRewardBox != (Object)null) ? ((Component)btnRewardBox).gameObject : null, rewardBoxCanvasGroup, active);
		bool flag = active && !isSettleTopThree && rankIndex > 3;
		GameObject target = rewardBoxShadow;
		CanvasGroup canvasGroup = rewardBoxShadowCanvasGroup;
		int active2;
		if (flag)
		{
			RankItemData rankItemData = entry;
			active2 = ((rankItemData == null || !rankItemData.IsSelf) ? 1 : 0);
		}
		else
		{
			active2 = 0;
		}
		SetRewardBoxNodeActive(target, canvasGroup, (byte)active2 != 0);
		SetRewardBoxNodeActive(rewardBoxShadow_own, rewardBoxOwnShadowCanvasGroup, flag && (entry?.IsSelf ?? false));
	}

	private static void SetRewardBoxNodeActive(GameObject target, CanvasGroup canvasGroup, bool active)
	{
		if (!((Object)(object)target == (Object)null))
		{
			if ((Object)(object)canvasGroup != (Object)null)
			{
				ShortcutExtensions.DOKill((Component)(object)canvasGroup, false);
				canvasGroup.alpha = 1f;
			}
			target.SetActive(active);
		}
	}

	public void PlayRankItemAnimation(ERanItemAnimType type, bool skipGlow = false)
	{
		SetNodeActive(active: true);
		if (!((Object)(object)nodeAnimation == (Object)null))
		{
			((Behaviour)nodeAnimation).enabled = true;
			string text = type switch
			{
				ERanItemAnimType.ItemUp => "anim_RankItemSelf_up_gp1", 
				ERanItemAnimType.ItemDown => "anim_RankItemSelf_down_gp1", 
				ERanItemAnimType.ItemOpen => "anim_RankItemSelf_open_gp1", 
				_ => string.Empty, 
			};
			if (!string.IsNullOrEmpty(text))
			{
				nodeAnimation.Play(text);
			}
			if (!skipGlow)
			{
				PlayStartEndAnimation(type);
			}
		}
	}

	private void PlayStartEndAnimation(ERanItemAnimType type)
	{
		if (!((Object)(object)startAndEndAni == (Object)null))
		{
			string text = type switch
			{
				ERanItemAnimType.ItemUp => "anim_rank_scoreboard_glow_in_gp1", 
				ERanItemAnimType.ItemDown => "anim_rank_scoreboard_glow_close_gp1", 
				_ => null, 
			};
			if (!string.IsNullOrEmpty(text))
			{
				startAndEndAni.Rewind(text);
				startAndEndAni.Play(text);
			}
		}
	}

	public void PlayRankUpEffect(bool active)
	{
		if ((Object)(object)rankUpEffect != (Object)null)
		{
			rankUpEffect.SetActive(active);
		}
	}

	public GameObject TransferRankDownEffect(RankItem target)
	{
		if ((Object)(object)rankDownEffect == (Object)null || (Object)(object)target == (Object)null || (Object)(object)target.nodeRect == (Object)null)
		{
			return null;
		}
		rankDownEffect.transform.SetParent((Transform)(object)target.nodeRect, true);
		GameObject result = rankDownEffect;
		rankDownEffect = null;
		target.transferredDownEffect = result;
		return result;
	}

	private void OnDisable()
	{
		if ((Object)(object)transferredDownEffect != (Object)null)
		{
			Object.Destroy((Object)(object)transferredDownEffect);
			transferredDownEffect = null;
		}
	}

	public float PlayUptoTop3Animation()
	{
		if ((Object)(object)rootAni == (Object)null)
		{
			return 0.5f;
		}
		((Behaviour)rootAni).enabled = true;
		rootAni.Rewind("anim_rankitem_uptotop3_1_gp1");
		rootAni.Play("anim_rankitem_uptotop3_1_gp1");
		AnimationClip clip = rootAni.GetClip("anim_rankitem_uptotop3_1_gp1");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0.5f;
		}
		return clip.length;
	}

	public void PlayGlowClose()
	{
		if (!((Object)(object)startAndEndAni == (Object)null))
		{
			startAndEndAni.Rewind("anim_rank_scoreboard_glow_close_gp1");
			startAndEndAni.Play("anim_rank_scoreboard_glow_close_gp1");
		}
	}

	public float PlayTop3UpAnimation()
	{
		if ((Object)(object)headRootAni == (Object)null)
		{
			return 0.5f;
		}
		((Behaviour)headRootAni).enabled = true;
		headRootAni.Rewind("anim_rankitem_top3up_gp1");
		headRootAni.Play("anim_rankitem_top3up_gp1");
		AnimationClip clip = headRootAni.GetClip("anim_rankitem_top3up_gp1");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0.5f;
		}
		return clip.length;
	}

	public float PlayTop3DownAnimation()
	{
		if ((Object)(object)headRootAni == (Object)null)
		{
			return 0.5f;
		}
		((Behaviour)headRootAni).enabled = true;
		headRootAni.Rewind("anim_rankitem_top3down_gp1");
		headRootAni.Play("anim_rankitem_top3down_gp1");
		AnimationClip clip = headRootAni.GetClip("anim_rankitem_top3down_gp1");
		if (!((Object)(object)clip != (Object)null))
		{
			return 0.5f;
		}
		return clip.length;
	}

	public void MoveHeadRootTo(Vector3 worldPos, float duration)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)headRootAni == (Object)null))
		{
			((Component)headRootAni).transform.SetAsLastSibling();
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)headRootAni).transform, worldPos, duration, false), (Ease)9);
		}
	}

	public void ReparentHeadRoot(Transform parent)
	{
		if (!((Object)(object)headRootAni == (Object)null) && !((Object)(object)parent == (Object)null))
		{
			((Component)headRootAni).transform.SetParent(parent, true);
			((Component)headRootAni).transform.SetAsLastSibling();
		}
	}

	public void ReturnHeadRoot(Transform parent)
	{
		if (!((Object)(object)headRootAni == (Object)null) && !((Object)(object)parent == (Object)null))
		{
			((Component)headRootAni).transform.SetParent(parent, true);
		}
	}

	private void CaptureOrigin()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!isOriginCaptured && !((Object)(object)nodeRect == (Object)null))
		{
			nodeOriginPos = nodeRect.anchoredPosition;
			nodeOriginScale = ((Transform)nodeRect).localScale;
			isOriginCaptured = true;
		}
	}

	private void CaptureHeadRootOrigin()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (!headRootOriginCaptured && !((Object)(object)headRootAni == (Object)null))
		{
			headRootOriginParent = ((Component)headRootAni).transform.parent;
			headRootVisual = ((Component)headRootAni).transform.Find("Root");
			headRootOriginLocalPos = ((Component)headRootAni).transform.localPosition;
			headRootOriginLocalScale = ((Component)headRootAni).transform.localScale;
			if ((Object)(object)headRootVisual != (Object)null)
			{
				headRootVisualOriginLocalScale = headRootVisual.localScale;
			}
			headRootOriginCaptured = true;
		}
	}

	public void ResetHeadRoot()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)headRootAni == (Object)null))
		{
			CaptureHeadRootOrigin();
			headRootAni.Stop();
			Transform transform = ((Component)headRootAni).transform;
			ShortcutExtensions.DOKill((Component)(object)transform, false);
			if ((Object)(object)headRootOriginParent != (Object)null)
			{
				transform.SetParent(headRootOriginParent, false);
			}
			transform.localPosition = headRootOriginLocalPos;
			transform.localScale = headRootOriginLocalScale;
			if ((Object)(object)headRootVisual != (Object)null)
			{
				headRootVisual.localScale = headRootVisualOriginLocalScale;
			}
			Transform val = ((Component)headRootAni).transform.Find("Root/eff_imghead_yuan_gp1");
			if ((Object)(object)val != (Object)null)
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
	}

	private void SetRankCollectionNum()
	{
		if ((Object)(object)txtCollectNum != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtCollectNum, (entry?.Collect ?? 0).ToString());
		}
	}

	private void SetRankName()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		int rankPlayerName = RankFunctionResolver.GetRankPlayerName();
		RankItemData rankItemData = entry;
		bool flag = rankItemData != null && rankItemData.IsSelf && (rankPlayerName == 2 || rankPlayerName == 3);
		if ((Object)(object)txtName == (Object)null)
		{
			return;
		}
		if (flag)
		{
			TMPTextExtensions.SetLocalizedText(txtName, "LeaderBoard_PlayerName_You", Array.Empty<object>());
		}
		else
		{
			TMPTextExtensions.SetLocalizedTextValue(txtName, entry?.Name ?? string.Empty);
		}
		if (rankType == ERankContent.Settle && entry != null && !entry.IsSelf && !isSettleTopThree)
		{
			Color botNameColor = RankConst.BotNameColor;
			if ((Object)(object)txtName != (Object)null)
			{
				((Graphic)txtName).color = botNameColor;
			}
			if ((Object)(object)txtCollectNum != (Object)null)
			{
				((Graphic)txtCollectNum).color = botNameColor;
			}
			if ((Object)(object)imgBg != (Object)null)
			{
				((Component)imgBg).gameObject.SetActive(true);
			}
		}
		else if (isSettleTopThree && (Object)(object)imgCollect != (Object)null)
		{
			((Component)imgCollect).gameObject.SetActive(true);
		}
	}

	private void ApplyRewardExperimentNameStyle()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)txtName == (Object)null))
		{
			bool flag = !collectPositionTopHeader && RankConst.CurrentRewardConfig > 0;
			txtName.rectTransform.SetSizeWithCurrentAnchors((Axis)0, flag ? 285f : defaultNameWidth);
			txtName.fontSize = defaultNameFontSize;
			txtName.fontSizeMin = defaultNameFontSizeMin;
			txtName.fontSizeMax = defaultNameFontSizeMax;
			txtName.enableAutoSizing = !flag && defaultNameAutoSizing;
			txtName.overflowMode = (TextOverflowModes)(flag ? 1 : ((int)defaultNameOverflowMode));
		}
	}

	private void SetTopThreeOutline()
	{
		if (!((Object)(object)imgOutline == (Object)null))
		{
			bool active = (rankType == ERankContent.Main || isSettleTopThree) && entry != null && entry.IsSelf && rankIndex >= 1 && rankIndex <= 3;
			((Component)imgOutline).gameObject.SetActive(active);
		}
	}

	private void ApplyMainVisualStyle()
	{
		if (entry != null && mainVisualStyleReady && ((rankType != ERankContent.Main && !isSettleTopThree) || rankIndex <= 0 || rankIndex > 3))
		{
			ApplyMainVisualStyle(entry.IsSelf ? selfMainStyle : botMainStyle);
		}
	}

	private void ApplyMainVisualStyle(MainVisualStyleSnapshot style)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Image transformImage = GetTransformImage(imgBg);
		if ((Object)(object)transformImage != (Object)null && (Object)(object)style.ImgBgSprite != (Object)null)
		{
			transformImage.sprite = style.ImgBgSprite;
		}
		if ((Object)(object)txtRankNum != (Object)null)
		{
			((Graphic)txtRankNum).color = style.TxtRankNumColor;
		}
		if ((Object)(object)txtName != (Object)null)
		{
			((Graphic)txtName).color = style.TxtNameColor;
		}
		if ((Object)(object)txtCollectNum != (Object)null)
		{
			((Graphic)txtCollectNum).color = style.TxtCollectNumColor;
		}
		if ((Object)(object)imgNoRank != (Object)null && ((Component)imgNoRank).gameObject.activeSelf)
		{
			((Graphic)imgNoRank).color = style.TxtRankNumColor;
		}
		Image bgCollectionImage = GetBgCollectionImage();
		if ((Object)(object)bgCollectionImage != (Object)null && (Object)(object)style.BgCollectionSprite != (Object)null)
		{
			bgCollectionImage.sprite = style.BgCollectionSprite;
		}
	}

	private MainVisualStyleSnapshot BuildMainVisualStyleSnapshot()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		Image bgCollectionImage = GetBgCollectionImage();
		MainVisualStyleSnapshot result = default(MainVisualStyleSnapshot);
		Image transformImage = GetTransformImage(imgBg);
		result.ImgBgSprite = ((transformImage != null) ? transformImage.sprite : null);
		result.TxtRankNumColor = (((Object)(object)txtRankNum != (Object)null) ? ((Graphic)txtRankNum).color : Color.white);
		result.TxtNameColor = (((Object)(object)txtName != (Object)null) ? ((Graphic)txtName).color : Color.white);
		result.BgCollectionSprite = (((Object)(object)bgCollectionImage != (Object)null) ? bgCollectionImage.sprite : null);
		result.TxtCollectNumColor = (((Object)(object)txtCollectNum != (Object)null) ? ((Graphic)txtCollectNum).color : Color.white);
		return result;
	}

	private Image GetBgCollectionImage()
	{
		if ((Object)(object)imgCollect != (Object)null && (Object)(object)imgCollect.parent != (Object)null)
		{
			Image component = ((Component)imgCollect.parent).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				return component;
			}
		}
		Transform val = ((Component)this).transform.Find("bgCollection");
		if (!((Object)(object)val != (Object)null))
		{
			return null;
		}
		return ((Component)val).GetComponent<Image>();
	}

	private static Image GetTransformImage(Transform target)
	{
		if (!((Object)(object)target != (Object)null))
		{
			return null;
		}
		return ((Component)target).GetComponent<Image>();
	}

	private void SetContentState()
	{
		if (rankType != ERankContent.Settle)
		{
			return;
		}
		if (isSettleTopThree)
		{
			SetBoxActive(rankIndex > 0 && rankIndex <= RankConst.CurrentRewardRankLimit);
			if ((Object)(object)btnArrowRight != (Object)null)
			{
				((Component)btnArrowRight).gameObject.SetActive(false);
			}
			if ((Object)(object)txtRankNum != (Object)null)
			{
				((Component)txtRankNum).gameObject.SetActive(rankIndex > 0 && rankIndex <= RankConst.MaxRankBot);
			}
			return;
		}
		SetBoxActive(rankIndex > 0 && rankIndex <= RankConst.CurrentRewardRankLimit);
		if ((Object)(object)btnArrowRight != (Object)null)
		{
			((Component)btnArrowRight).gameObject.SetActive(false);
		}
		if ((Object)(object)txtRankNum != (Object)null)
		{
			((Component)txtRankNum).gameObject.SetActive(rankIndex > 3 && rankIndex <= RankConst.MaxRankBot);
		}
		if ((Object)(object)imgNoRank != (Object)null)
		{
			((Component)imgNoRank).gameObject.SetActive(rankIndex < 0 || rankIndex > RankConst.MaxRankBot);
		}
	}

	private void SetHeadIcon()
	{
		if (entry == null)
		{
			return;
		}
		if ((Object)(object)imgHead != (Object)null)
		{
			imgHead.sprite = null;
		}
		if ((Object)(object)imgHeadFrame != (Object)null)
		{
			imgHeadFrame.sprite = null;
		}
		int generation = headLoadGeneration;
		bool num = entry.IsSelf || (rankIndex >= 1 && rankIndex <= 3);
		RankPlayerService.GetHeadSpriteAsync((!entry.IsSelf) ? entry.HeadId : 0, delegate(Sprite sprite)
		{
			if (generation == headLoadGeneration && !((Object)(object)imgHead == (Object)null) && (Object)(object)sprite != (Object)null)
			{
				imgHead.sprite = sprite;
			}
		});
		if (num)
		{
			RankPlayerService.GetFrameSpriteByNameAsync("leaderboard_avatar_frame", delegate(Sprite sprite)
			{
				if (generation == headLoadGeneration && !((Object)(object)imgHeadFrame == (Object)null) && (Object)(object)sprite != (Object)null)
				{
					imgHeadFrame.sprite = sprite;
				}
			});
			return;
		}
		RankPlayerService.GetHeadFrameSpriteAsync(entry.HeadFrameId, delegate(Sprite sprite)
		{
			if (generation == headLoadGeneration && !((Object)(object)imgHeadFrame == (Object)null) && (Object)(object)sprite != (Object)null)
			{
				imgHeadFrame.sprite = sprite;
			}
		});
	}

	private void ResetReusableVisualState()
	{
		ResetLegacyAnimation(rootAni);
		ResetLegacyAnimation(startAndEndAni);
		ResetLegacyAnimation(headRootAni);
		ResetHeadRoot();
		if ((Object)(object)transferredDownEffect != (Object)null)
		{
			transferredDownEffect.SetActive(false);
			Object.Destroy((Object)(object)transferredDownEffect);
			transferredDownEffect = null;
		}
		PlayRankUpEffect(active: false);
		if ((Object)(object)rankDownEffect != (Object)null)
		{
			rankDownEffect.SetActive(false);
		}
		if ((Object)(object)imgBg != (Object)null)
		{
			((Component)imgBg).gameObject.SetActive(defaultImgBgActive);
		}
		if ((Object)(object)imgCollect != (Object)null)
		{
			((Component)imgCollect).gameObject.SetActive(defaultImgCollectActive);
		}
		if ((Object)(object)btnArrowRight != (Object)null)
		{
			((Component)btnArrowRight).gameObject.SetActive(defaultArrowRightActive);
		}
		if ((Object)(object)txtRankNum != (Object)null)
		{
			((Component)txtRankNum).gameObject.SetActive(false);
		}
		if ((Object)(object)imgNoRank != (Object)null)
		{
			((Component)imgNoRank).gameObject.SetActive(false);
		}
	}

	private static void ResetLegacyAnimation(Animation animation)
	{
		if (!((Object)(object)animation == (Object)null))
		{
			animation.Stop();
			animation.Rewind();
			animation.Sample();
		}
	}

	private void ResetNodeAnimation()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)nodeAnimation != (Object)null)
		{
			nodeAnimation.Stop();
			nodeAnimation.Sample();
		}
		if ((Object)(object)nodeRect != (Object)null && isOriginCaptured)
		{
			nodeRect.anchoredPosition = nodeOriginPos;
			((Transform)nodeRect).localScale = nodeOriginScale;
		}
	}

	private void OnRelease()
	{
		OnRewardBoxClicked = null;
	}

	private void OnRewardBoxClick()
	{
		if (!((Object)(object)btnRewardBox == (Object)null))
		{
			OnRewardBoxClicked?.Invoke(rankIndex, ((Component)btnRewardBox).transform);
		}
	}

	private void OnArrowRightClick()
	{
	}
}
