using Game.UI;
using SuperScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XingYun.UI.Core;

namespace Rotaterings.UI.RankList;

public class UIRankListView : GameBaseView
{
	private const float BgCountDownYMain = -85f;

	private const float BgCountDownYSettle = -195f;

	private const string MainPanelOpenAnim = "anim_rank_main_start_gp1";

	private const string SettlePanelOpenAnim = "anim_uirankresult_top_in_gp1";

	private const string BottomAnimName = "anim_common_btn_appear";

	private const string BottomCommonEffectAnimName = "anim_common_btn_effect";

	private const string BottomCommonEffectNodeName = "eff_rank_btn_gp1";

	private const string TopUIAnim1 = "anim_UIRankListTop1_open";

	private const string TopUIAnim2 = "anim_UIRankListTop2_open";

	private const string TopUIAnim3 = "anim_UIRankListTop3_open";

	private ParticleSystem[] caidaiParticleSystems;

	public LoopListView2 RankLoopList;

	public LoopListView2 SettlementLoopList;

	public RankItem RankItem;

	public Button btnBack;

	public Button btnTip;

	public RankItem ThirdRankItem;

	public RankItem SecondRankItem;

	public RankItem FirstRankItem;

	public TMP_Text txtCountDown;

	public GameObject GoRank;

	public GameObject GoSettlement;

	public RectTransform bg1;

	public RectTransform bg2;

	public RectTransform bg3;

	public RankItem ListRankItemSelf;

	public Button btnContinue;

	public TMP_Text txtTitle;

	public RectTransform moveArrow;

	public TMP_Text txtX1;

	public TMP_Text txtX2;

	public TMP_Text txtX3;

	public RectTransform imgCollection;

	public RectTransform AddScore;

	public TMP_Text Score;

	public GameObject GoCollection;

	public GameObject MainBg;

	public GameObject SettleMask;

	public Button btnLevel;

	public GameObject bgCountDown;

	public TMP_Text txtTitle_Settle;

	public RectTransform bg4;

	public RectTransform bg5;

	public TMP_Text txtX4;

	public TMP_Text txtX5;

	public RectTransform greenbg1;

	public RectTransform greenbg2;

	public RectTransform greenbg3;

	public RectTransform greenbg4;

	public RectTransform greenbg5;

	public Animation PanelAnimation;

	public Animation BottomAnimation;

	public GameObject caidaiObj;

	public TMP_Text txtContinue;

	public GameObject bgCountDown_Main;

	public GameObject bgCountDown_Settle;

	public CanvasGroup RootCanvasGroup;

	public TMP_Text txtContinue_1;

	public CanvasGroup BottomCanvasGroup;

	public Animation ani_TopUI;

	public RectTransform rect_eff_Top;

	public void Init(ERankContent content)
	{
		ResetPanelAnimationForReuse();
		HideCaidaiObj();
		HideTopUI();
		SetRankContentActive(content);
	}

	public bool PlayTopUI(int rank)
	{
		if (!RankFunctionResolver.ShouldPlayTopRankAnimation())
		{
			return false;
		}
		if ((Object)(object)ani_TopUI == (Object)null || rank < 1 || rank > 3)
		{
			return false;
		}
		string text = rank switch
		{
			1 => "anim_UIRankListTop1_open", 
			2 => "anim_UIRankListTop2_open", 
			3 => "anim_UIRankListTop3_open", 
			_ => null, 
		};
		if (string.IsNullOrEmpty(text) || (Object)(object)ani_TopUI.GetClip(text) == (Object)null)
		{
			return false;
		}
		ResetTopUIVisuals();
		((Component)ani_TopUI).gameObject.SetActive(true);
		PlayAnimationFromStart(ani_TopUI, text);
		return true;
	}

	public void HideTopUI()
	{
		if (!((Object)(object)ani_TopUI == (Object)null))
		{
			ResetTopUIVisuals();
			((Component)ani_TopUI).gameObject.SetActive(false);
		}
	}

	private void ResetTopUIVisuals()
	{
		if ((Object)(object)ani_TopUI == (Object)null)
		{
			return;
		}
		ani_TopUI.Stop();
		Transform transform = ((Component)ani_TopUI).transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if ((Object)(object)child == (Object)null)
			{
				continue;
			}
			if ((Object)(object)rect_eff_Top != (Object)null && (Object)(object)child == (Object)(object)((Component)rect_eff_Top).transform)
			{
				for (int j = 0; j < child.childCount; j++)
				{
					Transform child2 = child.GetChild(j);
					if ((Object)(object)child2 != (Object)null)
					{
						((Component)child2).gameObject.SetActive(false);
					}
				}
			}
			else
			{
				((Component)child).gameObject.SetActive(false);
			}
		}
	}

	private void ResetPanelAnimationForReuse()
	{
		ResetBottomAnimation();
		if (!((Object)(object)PanelAnimation == (Object)null))
		{
			RewindClip(PanelAnimation, "anim_rank_main_start_gp1");
			RewindClip(PanelAnimation, "anim_uirankresult_top_in_gp1");
			if ((Object)(object)RootCanvasGroup != (Object)null)
			{
				RootCanvasGroup.alpha = 1f;
			}
		}
	}

	public void ResetBottomAnimation()
	{
		RewindClip(BottomAnimation, "anim_common_btn_appear");
		ResetBottomCommonEffect();
		if ((Object)(object)BottomCanvasGroup != (Object)null)
		{
			BottomCanvasGroup.alpha = 0f;
		}
		SetBottomButtonsInteractable(interactable: false);
	}

	public void ResetBottomCommonEffect()
	{
		RewindClip(BottomAnimation, "anim_common_btn_effect");
		Animation bottomAnimation = BottomAnimation;
		Transform val = ((bottomAnimation != null) ? ((Component)bottomAnimation).transform.Find("eff_rank_btn_gp1") : null);
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(false);
		}
	}

	private static void RewindClip(Animation animation, string clipName)
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
				val.enabled = false;
			}
		}
	}

	public void PlayMainPanelOpen()
	{
		if ((Object)(object)PanelAnimation != (Object)null)
		{
			PanelAnimation.Play("anim_rank_main_start_gp1");
		}
	}

	public void PlaySettlePanelOpen()
	{
		if ((Object)(object)PanelAnimation != (Object)null)
		{
			PanelAnimation.Play("anim_uirankresult_top_in_gp1");
		}
	}

	public void PlayBottomAnimation()
	{
		SetBottomButtonsInteractable(interactable: true);
		PlayAnimationFromStart(BottomAnimation, "anim_common_btn_appear");
	}

	private void SetBottomButtonsInteractable(bool interactable)
	{
		if ((Object)(object)BottomCanvasGroup != (Object)null)
		{
			BottomCanvasGroup.interactable = interactable;
			BottomCanvasGroup.blocksRaycasts = interactable;
		}
	}

	public void PlayBottomCommonEffect()
	{
		PlayAnimationFromStart(BottomAnimation, "anim_common_btn_effect");
	}

	private static void PlayAnimationFromStart(Animation animation, string clipName)
	{
		if (!((Object)(object)animation == (Object)null) && !string.IsNullOrEmpty(clipName) && !((Object)(object)animation.GetClip(clipName) == (Object)null))
		{
			animation.Stop(clipName);
			animation.Rewind(clipName);
			animation.Play(clipName);
		}
	}

	public void ShowCaidaiObj()
	{
		if ((Object)(object)caidaiObj != (Object)null)
		{
			caidaiObj.SetActive(true);
		}
	}

	public void HideCaidaiObj()
	{
		if ((Object)(object)caidaiObj == (Object)null)
		{
			return;
		}
		if (caidaiParticleSystems == null)
		{
			caidaiParticleSystems = caidaiObj.GetComponentsInChildren<ParticleSystem>(true);
		}
		for (int i = 0; i < caidaiParticleSystems.Length; i++)
		{
			ParticleSystem val = caidaiParticleSystems[i];
			if ((Object)(object)val != (Object)null)
			{
				val.Stop(true, (ParticleSystemStopBehavior)0);
			}
		}
		caidaiObj.SetActive(false);
	}

	private void SetRankContentActive(ERankContent content)
	{
		bool active = content == ERankContent.Main;
		if ((Object)(object)btnBack != (Object)null)
		{
			((Component)btnBack).gameObject.SetActive(active);
		}
		if ((Object)(object)btnTip != (Object)null)
		{
			((Component)btnTip).gameObject.SetActive(active);
		}
		if ((Object)(object)txtTitle != (Object)null)
		{
			((Component)txtTitle).gameObject.SetActive(active);
		}
		if ((Object)(object)txtTitle_Settle != (Object)null)
		{
			((Component)txtTitle_Settle).gameObject.SetActive(content == ERankContent.Settle);
		}
		SetBgCountDownActive(content);
		SetBgCountDownPosition(content);
		if ((Object)(object)btnLevel != (Object)null)
		{
			((Component)btnLevel).gameObject.SetActive(active);
		}
		if ((Object)(object)btnContinue != (Object)null)
		{
			((Component)btnContinue).gameObject.SetActive(content == ERankContent.Settle);
		}
		if ((Object)(object)GoRank != (Object)null)
		{
			GoRank.SetActive(active);
		}
		if ((Object)(object)GoSettlement != (Object)null)
		{
			GoSettlement.SetActive(content == ERankContent.Settle);
		}
		if ((Object)(object)ListRankItemSelf != (Object)null)
		{
			((Component)ListRankItemSelf).gameObject.SetActive(false);
		}
		if ((Object)(object)SettleMask != (Object)null)
		{
			SettleMask.SetActive(content == ERankContent.Settle);
		}
	}

	private void SetBgCountDownActive(ERankContent content)
	{
		bool flag = content == ERankContent.Main;
		if ((Object)(object)bgCountDown_Main != (Object)null)
		{
			bgCountDown_Main.SetActive(flag);
		}
		if ((Object)(object)bgCountDown_Settle != (Object)null)
		{
			bgCountDown_Settle.SetActive(!flag);
		}
	}

	private void SetBgCountDownPosition(ERankContent content)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)bgCountDown == (Object)null))
		{
			Transform transform = bgCountDown.transform;
			RectTransform val = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			if (!((Object)(object)val == (Object)null))
			{
				Vector2 anchoredPosition = val.anchoredPosition;
				anchoredPosition.y = ((content == ERankContent.Main) ? (-85f) : (-195f));
				val.anchoredPosition = anchoredPosition;
			}
		}
	}

	public override void Dispose()
	{
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		RankLoopList = behaviour.Get<LoopListView2>("RankLoopList");
		SettlementLoopList = behaviour.Get<LoopListView2>("SettlementLoopList");
		RankItem = behaviour.Get<RankItem>("RankItem");
		btnBack = behaviour.Get<Button>("btnBack");
		btnTip = behaviour.Get<Button>("btnTip");
		ThirdRankItem = behaviour.Get<RankItem>("ThirdRankItem");
		SecondRankItem = behaviour.Get<RankItem>("SecondRankItem");
		FirstRankItem = behaviour.Get<RankItem>("FirstRankItem");
		txtCountDown = behaviour.Get<TMP_Text>("txtCountDown");
		GoRank = behaviour.Get<GameObject>("GoRank");
		GoSettlement = behaviour.Get<GameObject>("GoSettlement");
		bg1 = behaviour.Get<RectTransform>("bg1");
		bg2 = behaviour.Get<RectTransform>("bg2");
		bg3 = behaviour.Get<RectTransform>("bg3");
		ListRankItemSelf = behaviour.Get<RankItem>("ListRankItemSelf");
		btnContinue = behaviour.Get<Button>("btnContinue");
		txtTitle = behaviour.Get<TMP_Text>("txtTitle");
		moveArrow = behaviour.Get<RectTransform>("moveArrow");
		txtX1 = behaviour.Get<TMP_Text>("txtX1");
		txtX2 = behaviour.Get<TMP_Text>("txtX2");
		txtX3 = behaviour.Get<TMP_Text>("txtX3");
		imgCollection = behaviour.Get<RectTransform>("imgCollection");
		AddScore = behaviour.Get<RectTransform>("AddScore");
		Score = behaviour.Get<TMP_Text>("Score");
		GoCollection = behaviour.Get<GameObject>("GoCollection");
		MainBg = behaviour.Get<GameObject>("MainBg");
		SettleMask = behaviour.Get<GameObject>("SettleMask");
		btnLevel = behaviour.Get<Button>("btnLevel");
		bgCountDown = behaviour.Get<GameObject>("bgCountDown");
		txtTitle_Settle = behaviour.Get<TMP_Text>("txtTitle_Settle");
		bg4 = behaviour.Get<RectTransform>("bg4");
		bg5 = behaviour.Get<RectTransform>("bg5");
		txtX4 = behaviour.Get<TMP_Text>("txtX4");
		txtX5 = behaviour.Get<TMP_Text>("txtX5");
		greenbg1 = behaviour.Get<RectTransform>("greenbg1");
		greenbg2 = behaviour.Get<RectTransform>("greenbg2");
		greenbg3 = behaviour.Get<RectTransform>("greenbg3");
		greenbg4 = behaviour.Get<RectTransform>("greenbg4");
		greenbg5 = behaviour.Get<RectTransform>("greenbg5");
		PanelAnimation = behaviour.Get<Animation>("PanelAnimation");
		BottomAnimation = behaviour.Get<Animation>("BottomAnimation");
		caidaiObj = behaviour.Get<GameObject>("caidaiObj");
		txtContinue = behaviour.Get<TMP_Text>("txtContinue");
		bgCountDown_Main = behaviour.Get<GameObject>("bgCountDown_Main");
		bgCountDown_Settle = behaviour.Get<GameObject>("bgCountDown_Settle");
		RootCanvasGroup = behaviour.Get<CanvasGroup>("RootCanvasGroup");
		txtContinue_1 = behaviour.Get<TMP_Text>("txtContinue_1");
		BottomCanvasGroup = behaviour.Get<CanvasGroup>("BottomCanvasGroup");
		ani_TopUI = behaviour.Get<Animation>("ani_TopUI");
		rect_eff_Top = behaviour.Get<RectTransform>("rect_eff_Top");
	}
}
