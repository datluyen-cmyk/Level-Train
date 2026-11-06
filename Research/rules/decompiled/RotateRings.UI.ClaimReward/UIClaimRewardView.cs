using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.UI;
using Game.UI.ClaimReward;
using RotateRings.Common;
using RotateRings.Services;
using Rotaterings.Tools;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XY.Common.ParticleImage;
using XingYun.Extensions;
using XingYun.Lang;
using XingYun.UI.Core;

namespace RotateRings.UI.ClaimReward;

public class UIClaimRewardView : GameBaseView
{
	private const string MaskOpenClipName = "UIClaimReward_mask_open";

	private const string MaskCloseClipName = "UIClaimReward_mask_close";

	private const string PanelOpenClipName = "UIClaimReward_open";

	private const string PanelAnimalOpenClipName = "UIClaimReward_animal_open";

	private const string PanelNoBoxOpenClipName = "UIClaimRewardnobox_open";

	private const string PanelCloseClipName = "UIClaimReward_close";

	private const string PanelNoBoxCloseClipName = "UIClaimRewardnobox_close";

	private const string X2ParticlePrefabLocation = "eff_claimrewarddouble_fly_01";

	private const float X2ParticleControlXBlend = 1.2f;

	private const float X2ParticleControlYBlend = 0.4f;

	private const float OpenAnimationCheckInterval = 0.05f;

	private const float AnimationCompletionEpsilon = 0.001f;

	private static readonly Vector2 X2ParticleSingleLeftControl = new Vector2(-222f, 461f);

	private static readonly Vector2 X2ParticleSingleRightControl = new Vector2(222f, 461f);

	private const string HiddenTextColorStart = "<color=#FFFFFF00>";

	private const string HiddenTextColorEnd = "</color>";

	private PropFlyPathAsset _flyPath;

	private PropFlyPathAsset _flyBackPath;

	private readonly List<GameObject> _x2ParticleClones = new List<GameObject>();

	private int _claimX2LayoutRefreshTimerId = -1;

	private Tween _openAutoStopTween;

	public Button btnOk;

	public Animation PanelAnimation;

	public CanvasGroup UIPanel;

	public TMP_Text txtDesc;

	public GameObject GoPropList;

	public Button btnClaimX2;

	public TMP_Text txtClaimX2;

	public Button btnClaim;

	public RectTransform HLayout;

	public GameObject PropItemTemplate;

	public Button btnContinue;

	public RectTransform StartPos;

	public TMP_Text txtCongratulations;

	public TMP_Text txtClaim;

	public TMP_Text txtContinue;

	public GameObject box_node;

	public GameObject objBoxEndPos;

	public GameObject objRewardStartPos;

	public SkeletonGraphic SkeletonGraphic;

	public GameObject Prop1;

	public GameObject Prop2;

	public GameObject Prop3;

	public Animation bgMask;

	public HorizontalLayoutGroup Layout;

	public GameObject RankType1;

	public TMP_Text RankType1Con;

	public TMP_Text RankType1Txt;

	public GameObject RankType2;

	public TMP_Text RankType2Con;

	public TMP_Text RankType2Txt;

	public GameObject RankType3;

	public TMP_Text RankType3Con;

	public TMP_Text RankType3Con_1;

	public TMP_Text RankType3Con_2;

	public TMP_Text RankType3Con_3;

	public TMP_Text RankType3Con_4;

	public TMP_Text RankType3Txt;

	public GameObject RankType4;

	public TMP_Text RankType4Con;

	public TMP_Text RankType4Txt;

	public TMP_Text RankType4Txt_1;

	public TMP_Text RankType4Txt_2;

	public TMP_Text RankType4Txt_3;

	public TMP_Text RankType4Txt_4;

	public void InjectRefs(UIClaimRewardViewRefs refs)
	{
		_flyPath = (((Object)(object)refs != (Object)null) ? refs.PropFlyPath : null);
		_flyBackPath = (((Object)(object)refs != (Object)null) ? refs.PropFlyBackPath : null);
	}

	public void SetClaimX2Text(string text)
	{
		if ((Object)(object)txtClaimX2 != (Object)null)
		{
			TMPTextExtensions.SetLocalizedTextValue(txtClaimX2, text);
		}
	}

	public void SetCongratulationsLocalized(string langKey)
	{
		TMP_Text obj = txtCongratulations;
		if (obj != null)
		{
			TMPTextExtensions.SetLocalizedText(obj, langKey, Array.Empty<object>());
		}
	}

	public void SetCongratulationsText(string text)
	{
		if ((Object)(object)txtCongratulations != (Object)null)
		{
			txtCongratulations.text = text;
		}
	}

	public void SetRankType1Visible(bool visible)
	{
		if ((Object)(object)RankType1 != (Object)null)
		{
			RankType1.SetActive(visible);
		}
	}

	public void SetRankType1LocalizedText(string contentKey, string descriptionKey, int rank)
	{
		string text = FormatRankText(rank);
		TMP_Text rankType1Con = RankType1Con;
		if (rankType1Con != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType1Con, contentKey, new object[1] { text });
		}
		TMP_Text rankType1Txt = RankType1Txt;
		if (rankType1Txt != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType1Txt, descriptionKey, new object[1] { text });
		}
	}

	public void SetRankType2Visible(bool visible)
	{
		if ((Object)(object)RankType2 != (Object)null)
		{
			RankType2.SetActive(visible);
		}
	}

	public void SetRankType2LocalizedText(string contentKey, string descriptionKey, int rank)
	{
		TMP_Text rankType2Con = RankType2Con;
		if (rankType2Con != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType2Con, contentKey, Array.Empty<object>());
		}
		TMP_Text rankType2Txt = RankType2Txt;
		if (rankType2Txt != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType2Txt, descriptionKey, new object[1] { FormatRankText(rank) });
		}
	}

	public void SetRankType3Visible(bool visible)
	{
		if ((Object)(object)RankType3 != (Object)null)
		{
			RankType3.SetActive(visible);
		}
	}

	public void SetRankType3LocalizedText(string contentKey, string descriptionKey, int rank)
	{
		string text = FormatRankText(rank);
		bool flag = rank > 3;
		TMP_Text rankType3Con = RankType3Con;
		if (rankType3Con != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType3Con, contentKey, new object[1] { text });
		}
		SetRankType3ContentVisible(RankType3Con_1, rank == 1);
		SetRankType3ContentVisible(RankType3Con_2, rank == 2);
		SetRankType3ContentVisible(RankType3Con_3, rank == 3);
		SetRankType3ContentVisible(RankType3Con_4, flag);
		string rankOnlyLocalizedText = GetRankOnlyLocalizedText(contentKey, rank);
		switch (rank)
		{
		case 1:
			SetRankType3RankOnlyText(RankType3Con_1, rankOnlyLocalizedText);
			break;
		case 2:
			SetRankType3RankOnlyText(RankType3Con_2, rankOnlyLocalizedText);
			break;
		case 3:
			SetRankType3RankOnlyText(RankType3Con_3, rankOnlyLocalizedText);
			break;
		default:
			if (flag)
			{
				SetRankType3RankOnlyText(RankType3Con_4, rankOnlyLocalizedText);
			}
			break;
		}
		TMP_Text rankType3Txt = RankType3Txt;
		if (rankType3Txt != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType3Txt, descriptionKey, new object[1] { text });
		}
	}

	private static string GetRankOnlyLocalizedText(string contentKey, int rank)
	{
		string text = GameApp.Lang.GetText(contentKey, Array.Empty<object>());
		int num = text.IndexOf("{0}", StringComparison.Ordinal);
		if (num < 0)
		{
			return GameApp.Lang.GetText(contentKey, new object[1] { FormatRankText(rank) });
		}
		int num2 = num + "{0}".Length;
		string[] obj = new string[7]
		{
			"<color=#FFFFFF00>",
			text.Substring(0, num),
			"</color>",
			FormatRankText(rank),
			"<color=#FFFFFF00>",
			null,
			null
		};
		string text2 = text;
		int num3 = num2;
		obj[5] = text2.Substring(num3, text2.Length - num3);
		obj[6] = "</color>";
		return string.Concat(obj);
	}

	private static string FormatRankText(int rank)
	{
		return $"#{rank}";
	}

	private static void SetRankType3RankOnlyText(TMP_Text content, string text)
	{
		if (!((Object)(object)content == (Object)null))
		{
			LangBinder langBinder = TMPTextExtensions.GetLangBinder(content);
			if ((Object)(object)langBinder != (Object)null)
			{
				((Behaviour)langBinder).enabled = false;
			}
			content.richText = true;
			content.text = text;
		}
	}

	private static void SetRankType3ContentVisible(TMP_Text content, bool visible)
	{
		if ((Object)(object)content != (Object)null)
		{
			((Component)content).gameObject.SetActive(visible);
		}
	}

	public void SetRankType4Visible(bool visible)
	{
		if ((Object)(object)RankType4 != (Object)null)
		{
			RankType4.SetActive(visible);
		}
	}

	public void SetRankType4LocalizedText(string contentKey, string descriptionKey, int rank)
	{
		string text = FormatRankText(rank);
		bool flag = rank > 3;
		TMP_Text rankType4Con = RankType4Con;
		if (rankType4Con != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType4Con, contentKey, Array.Empty<object>());
		}
		TMP_Text rankType4Txt = RankType4Txt;
		if (rankType4Txt != null)
		{
			TMPTextExtensions.SetLocalizedText(rankType4Txt, descriptionKey, new object[1] { text });
		}
		SetRankType4ContentVisible(RankType4Txt_1, rank == 1);
		SetRankType4ContentVisible(RankType4Txt_2, rank == 2);
		SetRankType4ContentVisible(RankType4Txt_3, rank == 3);
		SetRankType4ContentVisible(RankType4Txt_4, flag);
		string rankOnlyLocalizedText = GetRankOnlyLocalizedText(descriptionKey, rank);
		switch (rank)
		{
		case 1:
			SetRankType3RankOnlyText(RankType4Txt_1, rankOnlyLocalizedText);
			return;
		case 2:
			SetRankType3RankOnlyText(RankType4Txt_2, rankOnlyLocalizedText);
			return;
		case 3:
			SetRankType3RankOnlyText(RankType4Txt_3, rankOnlyLocalizedText);
			return;
		}
		if (flag)
		{
			SetRankType3RankOnlyText(RankType4Txt_4, rankOnlyLocalizedText);
		}
	}

	private static void SetRankType4ContentVisible(TMP_Text content, bool visible)
	{
		if ((Object)(object)content != (Object)null)
		{
			((Component)content).gameObject.SetActive(visible);
		}
	}

	public void RefreshClaimX2Layout()
	{
		if (!((Object)(object)Layout == (Object)null) && !((Object)(object)((UIBaseView)this).Behaviour == (Object)null))
		{
			if (_claimX2LayoutRefreshTimerId >= 0)
			{
				GameApp.Timer.Kill(_claimX2LayoutRefreshTimerId);
				_claimX2LayoutRefreshTimerId = -1;
			}
			_claimX2LayoutRefreshTimerId = GameApp.Timer.NextFrame((Object)(object)((Component)((UIBaseView)this).Behaviour).gameObject, (Action)delegate
			{
				_claimX2LayoutRefreshTimerId = -1;
				RebuildClaimX2LayoutImmediate();
			});
		}
	}

	private void RebuildClaimX2LayoutImmediate()
	{
		if (!((Object)(object)Layout == (Object)null) && (!((Object)(object)btnClaimX2 != (Object)null) || ((Component)btnClaimX2).gameObject.activeInHierarchy))
		{
			TMP_Text obj = txtClaimX2;
			if (obj != null)
			{
				TMPTextExtensions.RefreshText(obj);
			}
			TMP_Text obj2 = txtClaimX2;
			if (obj2 != null)
			{
				obj2.ForceMeshUpdate(false, false);
			}
			Canvas.ForceUpdateCanvases();
			Transform transform = ((Component)Layout).transform;
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((transform is RectTransform) ? transform : null));
		}
	}

	public void PlayX2Particles(IReadOnlyList<RectTransform> targets, Action onComplete)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		ClearX2Particles();
		if (targets == null || targets.Count == 0 || (Object)(object)txtClaimX2 == (Object)null || (Object)(object)((UIBaseView)this).Behaviour == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		GameObject val = GameApp.Res.LoadAsset<GameObject>("eff_claimrewarddouble_fly_01", (string)null);
		if ((Object)(object)val == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		Transform transform = ((Component)UIPanel).transform;
		Vector3 position = ((Component)btnClaimX2).transform.position;
		int remainingCount = 0;
		Action notifyCompleted = delegate
		{
			remainingCount--;
			if (remainingCount <= 0)
			{
				onComplete?.Invoke();
			}
		};
		if (targets.Count == 1 && (Object)(object)targets[0] != (Object)null)
		{
			RectTransform target = targets[0];
			SpawnX2Particle(val, transform, target, position, X2ParticleSingleLeftControl, notifyCompleted, ref remainingCount);
			SpawnX2Particle(val, transform, target, position, X2ParticleSingleRightControl, notifyCompleted, ref remainingCount);
		}
		else
		{
			Vector2 customBezierControlOffset = default(Vector2);
			for (int num = 0; num < targets.Count; num++)
			{
				RectTransform val2 = targets[num];
				if ((Object)(object)val2 == (Object)null)
				{
					continue;
				}
				GameObject particleObject = Object.Instantiate<GameObject>(val, transform, false);
				XYParticleImage componentInChildren = particleObject.GetComponentInChildren<XYParticleImage>(true);
				if ((Object)(object)componentInChildren == (Object)null)
				{
					Object.Destroy((Object)(object)particleObject);
					continue;
				}
				particleObject.SetActive(true);
				particleObject.transform.SetAsLastSibling();
				particleObject.transform.position = position;
				_x2ParticleClones.Add(particleObject);
				int num2 = remainingCount;
				remainingCount = num2 + 1;
				Vector2 transformLocalPosition = componentInChildren.GetTransformLocalPosition((Transform)(object)val2);
				((Vector2)(ref customBezierControlOffset))._002Ector(transformLocalPosition.x * 1.2f, transformLocalPosition.y * 0.4f);
				componentInChildren.bezierControlPointScheme = BezierControlPointScheme.Custom;
				componentInChildren.customBezierControlOffset = customBezierControlOffset;
				componentInChildren.Play((Transform)(object)val2, null, null, delegate
				{
					DestroyX2Particle(particleObject);
					notifyCompleted();
				});
			}
		}
		if (remainingCount == 0)
		{
			onComplete?.Invoke();
		}
	}

	public void ClearX2Particles()
	{
		for (int i = 0; i < _x2ParticleClones.Count; i++)
		{
			GameObject val = _x2ParticleClones[i];
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
			}
		}
		_x2ParticleClones.Clear();
	}

	private void DestroyX2Particle(GameObject particleObject)
	{
		_x2ParticleClones.Remove(particleObject);
		if ((Object)(object)particleObject != (Object)null)
		{
			Object.Destroy((Object)(object)particleObject);
		}
	}

	private void SpawnX2Particle(GameObject prefab, Transform parent, RectTransform target, Vector3 startWorldPosition, Vector2 control, Action notifyCompleted, ref int remainingCount)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		GameObject particleObject = Object.Instantiate<GameObject>(prefab, parent, false);
		XYParticleImage componentInChildren = particleObject.GetComponentInChildren<XYParticleImage>(true);
		if ((Object)(object)componentInChildren == (Object)null)
		{
			Object.Destroy((Object)(object)particleObject);
			return;
		}
		particleObject.SetActive(true);
		particleObject.transform.SetAsLastSibling();
		particleObject.transform.position = startWorldPosition;
		_x2ParticleClones.Add(particleObject);
		remainingCount++;
		componentInChildren.bezierControlPointScheme = BezierControlPointScheme.Custom;
		componentInChildren.customBezierControlOffset = control;
		componentInChildren.Play((Transform)(object)target, null, null, delegate
		{
			DestroyX2Particle(particleObject);
			notifyCompleted();
		});
	}

	public void NotifyPopupMaskStackActive(bool isActive)
	{
		UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
		if (behaviour != null)
		{
			((Component)behaviour).GetComponent<UIPopupMaskFader>()?.OnUiStackActiveChanged(isActive);
		}
	}

	public void HideBgMaskVisual()
	{
		if (!((Object)(object)bgMask == (Object)null))
		{
			bgMask.Stop();
			CanvasGroup component = ((Component)bgMask).GetComponent<CanvasGroup>();
			if ((Object)(object)component != (Object)null)
			{
				component.alpha = 0f;
			}
		}
	}

	public void PlayMaskOpen(bool skipFadeIn = false)
	{
		if (!skipFadeIn)
		{
			PlayMaskAnimation("UIClaimReward_mask_open", 0f);
		}
		else if (!((Object)(object)bgMask == (Object)null) && !((Object)(object)bgMask.GetClip("UIClaimReward_mask_open") == (Object)null))
		{
			bgMask.Stop();
			AnimationState obj = bgMask["UIClaimReward_mask_open"];
			obj.wrapMode = (WrapMode)8;
			bgMask.Play("UIClaimReward_mask_open");
			obj.normalizedTime = 1f;
			bgMask.Sample();
			bgMask.Stop();
		}
	}

	private void PlayMaskClose()
	{
		PlayMaskAnimation("UIClaimReward_mask_close", 1f);
	}

	private void PlayMaskAnimation(string clipName, float prepareAlpha)
	{
		if (!((Object)(object)bgMask == (Object)null) && !((Object)(object)bgMask.GetClip(clipName) == (Object)null))
		{
			CanvasGroup component = ((Component)bgMask).GetComponent<CanvasGroup>();
			if ((Object)(object)component != (Object)null)
			{
				component.alpha = prepareAlpha;
			}
			bgMask.Stop();
			bgMask.Rewind(clipName);
			AnimationState val = bgMask[clipName];
			if ((TrackedReference)(object)val != (TrackedReference)null)
			{
				val.normalizedTime = 0f;
				val.wrapMode = (WrapMode)8;
			}
			bgMask.Sample();
			bgMask.Play(clipName);
		}
	}

	public void PlayPanelOpen(bool playMask = true, bool hasAnimals = false, bool showClaimX2 = true, bool useContinueButton = false)
	{
		if (playMask)
		{
			PlayMaskOpen();
		}
		string clipName = (hasAnimals ? "UIClaimReward_animal_open" : "UIClaimReward_open");
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Component)((Component)btnClaim).transform.GetChild(0)).GetComponent<Image>();
		}
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Component)btnClaim).GetComponent<CanvasGroup>();
		}
		PlayPanelAnimation(PanelAnimation, clipName);
		ShowActionButtons(showClaimX2, useContinueButton);
		Tween openAutoStopTween = _openAutoStopTween;
		if (openAutoStopTween != null)
		{
			TweenExtensions.Kill(openAutoStopTween, false);
		}
		float clipDuration = GetClipDuration(PanelAnimation, clipName);
		if (clipDuration > 0f)
		{
			StopPanelOpenWhenAnimationComplete(clipName, clipDuration);
		}
		else if ((Object)(object)PanelAnimation != (Object)null)
		{
			PanelAnimation.Stop();
		}
	}

	public void PrepareRewardPresentation()
	{
		Tween openAutoStopTween = _openAutoStopTween;
		if (openAutoStopTween != null)
		{
			TweenExtensions.Kill(openAutoStopTween, false);
		}
		_openAutoStopTween = null;
		PlayPanelAnimation(PanelAnimation, "UIClaimRewardnobox_open");
		AnimationState val = (((Object)(object)PanelAnimation != (Object)null) ? PanelAnimation["UIClaimRewardnobox_open"] : null);
		if ((TrackedReference)(object)val != (TrackedReference)null)
		{
			val.normalizedTime = 0f;
			val.speed = 0f;
			PanelAnimation.Sample();
		}
	}

	public void ShowRewardPresentation(bool hasAnimals = false, bool showClaimX2 = true, bool useContinueButton = false)
	{
		Tween openAutoStopTween = _openAutoStopTween;
		if (openAutoStopTween != null)
		{
			TweenExtensions.Kill(openAutoStopTween, false);
		}
		_openAutoStopTween = null;
		PlayPanelAnimation(PanelAnimation, "UIClaimRewardnobox_open");
		AnimationState val = (((Object)(object)PanelAnimation != (Object)null) ? PanelAnimation["UIClaimRewardnobox_open"] : null);
		if ((TrackedReference)(object)val != (TrackedReference)null)
		{
			val.normalizedTime = 0f;
			val.speed = 1f;
			PanelAnimation.Sample();
		}
		ShowActionButtons(showClaimX2, useContinueButton);
		float clipDuration = GetClipDuration(PanelAnimation, "UIClaimRewardnobox_open");
		if (clipDuration > 0f)
		{
			StopPanelOpenWhenAnimationComplete("UIClaimRewardnobox_open", clipDuration);
		}
		else if ((Object)(object)PanelAnimation != (Object)null)
		{
			PanelAnimation.Stop();
		}
	}

	private void StopPanelOpenWhenAnimationComplete(string clipName, float clipDuration)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		AnimationState val = (((Object)(object)PanelAnimation != (Object)null) ? PanelAnimation[clipName] : null);
		if ((TrackedReference)(object)val == (TrackedReference)null || val.normalizedTime >= 0.999f)
		{
			LogPanelOpenStopState("BEFORE autoStop", clipName, clipDuration);
			if ((Object)(object)PanelAnimation != (Object)null)
			{
				PanelAnimation.Stop();
			}
			LogPanelOpenStopState("AFTER autoStop", clipName, clipDuration);
			_openAutoStopTween = null;
		}
		else
		{
			_openAutoStopTween = DOVirtual.DelayedCall(0.05f, (TweenCallback)delegate
			{
				StopPanelOpenWhenAnimationComplete(clipName, clipDuration);
			}, true);
		}
	}

	private void LogPanelOpenStopState(string stage, string clipName, float expectedDuration)
	{
		if ((Object)(object)PanelAnimation != (Object)null)
		{
			_ = PanelAnimation[clipName];
		}
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Component)btnClaim).GetComponent<CanvasGroup>();
		}
		if ((Object)(object)btnClaimX2 != (Object)null)
		{
			((Component)btnClaimX2).GetComponent<CanvasGroup>();
		}
	}

	public Tween PlayPanelClose(Action onComplete, bool playMask = true, bool skipBoxAnimation = false)
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		Tween openAutoStopTween = _openAutoStopTween;
		if (openAutoStopTween != null)
		{
			TweenExtensions.Kill(openAutoStopTween, false);
		}
		_openAutoStopTween = null;
		string clipName = (skipBoxAnimation ? "UIClaimRewardnobox_close" : "UIClaimReward_close");
		if (playMask)
		{
			PlayMaskClose();
		}
		PlayPanelAnimation(PanelAnimation, clipName);
		float num = Mathf.Max(GetClipDuration(PanelAnimation, clipName), playMask ? GetClipDuration(bgMask, "UIClaimReward_mask_close") : 0f);
		if (num > 0f)
		{
			return DOVirtual.DelayedCall(num, (TweenCallback)delegate
			{
				onComplete?.Invoke();
			}, true);
		}
		onComplete?.Invoke();
		return null;
	}

	public void ShowActionButtons(bool showClaimX2 = true, bool useContinueButton = false)
	{
		if ((Object)(object)btnContinue != (Object)null)
		{
			((Component)btnContinue).gameObject.SetActive(useContinueButton);
		}
		if (useContinueButton)
		{
			TMP_Text obj = txtContinue;
			if (obj != null)
			{
				TMPTextExtensions.SetLocalizedText(obj, "chest_claim", Array.Empty<object>());
			}
		}
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Component)btnClaim).gameObject.SetActive(!useContinueButton);
		}
		if ((Object)(object)btnClaimX2 != (Object)null)
		{
			((Component)btnClaimX2).gameObject.SetActive(showClaimX2);
			if (showClaimX2)
			{
				RefreshClaimX2Layout();
			}
		}
		ForceSelfButtonImageAlpha(btnClaim);
		ForceSelfButtonImageAlpha(btnContinue);
		ForceSelfButtonImageAlpha(btnClaimX2);
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Component)((Component)btnClaim).transform.GetChild(0)).GetComponent<Image>();
		}
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Component)btnClaim).GetComponent<CanvasGroup>();
		}
		if ((Object)(object)btnClaimX2 != (Object)null)
		{
			((Component)((Component)btnClaimX2).transform.GetChild(0)).GetComponent<Image>();
		}
		if ((Object)(object)btnClaimX2 != (Object)null)
		{
			((Component)btnClaimX2).GetComponent<CanvasGroup>();
		}
	}

	private static void ForceSelfButtonImageAlpha(Button btn)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)btn == (Object)null)
		{
			return;
		}
		Image component = ((Component)btn).GetComponent<Image>();
		if (!((Object)(object)component == (Object)null) && !((Object)(object)component.sprite == (Object)null))
		{
			Color color = ((Graphic)component).color;
			if (!Mathf.Approximately(color.a, 1f))
			{
				color.a = 1f;
				((Graphic)component).color = color;
			}
		}
	}

	public void ReplaceBoxNodeChild(GameObject prefab)
	{
		if (!((Object)(object)box_node == (Object)null) && !((Object)(object)prefab == (Object)null))
		{
			for (int num = box_node.transform.childCount - 1; num >= 0; num--)
			{
				Object.DestroyImmediate((Object)(object)((Component)box_node.transform.GetChild(num)).gameObject);
			}
			Object.Instantiate<GameObject>(prefab, box_node.transform).SetActive(true);
		}
	}

	public void SetBoxNodeStartWorldPosition(Vector2 worldPosition)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)box_node == (Object)null) && box_node.transform.childCount != 0)
		{
			Transform child = box_node.transform.GetChild(0);
			RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
			if ((Object)(object)val != (Object)null)
			{
				((Transform)val).position = new Vector3(worldPosition.x, worldPosition.y, ((Transform)val).position.z);
			}
		}
	}

	public Tween PlayBoxFlyToEnd(float duration = 0.5f, bool enableFadeIn = true, Action onArrive = null)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		if ((Object)(object)box_node == (Object)null || (Object)(object)objBoxEndPos == (Object)null || box_node.transform.childCount == 0)
		{
			return null;
		}
		Transform child = box_node.transform.GetChild(0);
		RectTransform val = (RectTransform)(object)((child is RectTransform) ? child : null);
		Transform transform = objBoxEndPos.transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val == (Object)null || (Object)(object)val2 == (Object)null)
		{
			return null;
		}
		CanvasGroup val3 = ((Component)val).GetComponent<CanvasGroup>();
		if (enableFadeIn)
		{
			if ((Object)(object)val3 == (Object)null)
			{
				val3 = ((Component)val).gameObject.AddComponent<CanvasGroup>();
			}
			val3.alpha = 0f;
			DOTweenModuleUI.DOFade(val3, 1f, duration);
		}
		else if ((Object)(object)val3 != (Object)null)
		{
			val3.alpha = 1f;
		}
		TweenerCore<Vector2, Vector2, VectorOptions> val4 = TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(val, val2.anchoredPosition, duration, false), (Ease)1);
		if (onArrive != null)
		{
			TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(val4, (TweenCallback)delegate
			{
				onArrive();
			});
		}
		return (Tween)(object)val4;
	}

	public void PlayBoxSpine(string animName, Action onComplete)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		if (string.IsNullOrEmpty(animName))
		{
			onComplete?.Invoke();
			return;
		}
		SkeletonGraphic sg = (((Object)(object)box_node != (Object)null) ? box_node.GetComponentInChildren<SkeletonGraphic>(true) : null);
		if ((Object)(object)sg == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		AudioService.PlayUiSound(AudioKeys.RewardOpen.Value);
		sg.AnimationState.SetAnimation(0, animName, false);
		if (onComplete != null)
		{
			TrackEntryDelegate handler = null;
			handler = (TrackEntryDelegate)delegate
			{
				sg.AnimationState.Complete -= handler;
				onComplete();
			};
			sg.AnimationState.Complete += handler;
		}
	}

	public void ReplaceBoxSkeletonData(SkeletonDataAsset skeletonDataAsset, string skinName = null)
	{
		if ((Object)(object)box_node == (Object)null || (Object)(object)skeletonDataAsset == (Object)null)
		{
			return;
		}
		SkeletonGraphic componentInChildren = box_node.GetComponentInChildren<SkeletonGraphic>(true);
		if ((Object)(object)componentInChildren == (Object)null)
		{
			return;
		}
		SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
		if (skeletonData == null)
		{
			return;
		}
		componentInChildren.skeletonDataAsset = skeletonDataAsset;
		if (!string.IsNullOrEmpty(skinName))
		{
			Skin val = skeletonData.FindSkin(skinName);
			if (val != null)
			{
				componentInChildren.Skeleton.SetSkin(val);
				componentInChildren.Skeleton.SetSlotsToSetupPose();
			}
		}
		componentInChildren.Initialize(false);
		componentInChildren.UpdateMesh();
	}

	public Sequence PlayPropFlyAnimation(List<RectTransform> items, bool hasNonConsumable, Action onComplete = null)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Min(items.Count, 3);
		if (num == 0)
		{
			onComplete?.Invoke();
			return null;
		}
		Vector2 startLocal = Vector2.op_Implicit(((Transform)HLayout).InverseTransformPoint(objRewardStartPos.transform.position));
		Vector2[] array = CalcPropTargets(num, hasNonConsumable);
		foreach (RectTransform item in items)
		{
			((Component)item).gameObject.SetActive(false);
		}
		Sequence val = DOTween.Sequence();
		for (int i = 0; i < num; i++)
		{
			int idx = i;
			float t = 0f;
			Sequence val2 = TweenSettingsExtensions.AppendCallback(DOTween.Sequence(), (TweenCallback)delegate
			{
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				items[idx].anchoredPosition = startLocal;
				((Component)items[idx]).gameObject.SetActive(true);
				t = 0f;
			});
			if ((Object)(object)_flyPath != (Object)null)
			{
				Vector2 start = startLocal;
				Vector2 end = array[idx];
				TweenSettingsExtensions.Append(val2, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => t), (DOSetter<float>)delegate(float v)
				{
					//IL_004d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0053: Unknown result type (might be due to invalid IL or missing references)
					//IL_0058: Unknown result type (might be due to invalid IL or missing references)
					t = v;
					items[idx].anchoredPosition = _flyPath.Evaluate(t, start, end);
				}, 1f, 0.5f), (Ease)1));
			}
			else
			{
				TweenSettingsExtensions.Append(val2, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(items[idx], array[idx], 0.5f, false), (Ease)9));
			}
			TweenSettingsExtensions.Insert(val, (float)idx * 0.05f, (Tween)(object)val2);
		}
		TweenSettingsExtensions.InsertCallback(val, (float)(num - 1) * 0.05f + 0.5f, (TweenCallback)delegate
		{
			onComplete?.Invoke();
		});
		return val;
	}

	public Vector2[] CalcPropTargets(int count, bool hasNonConsumable)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (((Object)(object)Prop1 != (Object)null) ? Vector2.op_Implicit(((Transform)HLayout).InverseTransformPoint(Prop1.transform.position)) : Vector2.zero);
		Vector2 val2 = (((Object)(object)Prop2 != (Object)null) ? Vector2.op_Implicit(((Transform)HLayout).InverseTransformPoint(Prop2.transform.position)) : val);
		float y = val.y;
		return count switch
		{
			1 => (Vector2[])(object)new Vector2[1]
			{
				new Vector2(0f, y)
			}, 
			2 => (Vector2[])(object)new Vector2[2] { val, val2 }, 
			3 => (Vector2[])(object)((!hasNonConsumable) ? new Vector2[3]
			{
				new Vector2(-250f, y),
				new Vector2(0f, y),
				new Vector2(250f, y)
			} : new Vector2[3]
			{
				new Vector2(-169f, -159f),
				new Vector2(0f, 72f),
				new Vector2(185f, -154f)
			}), 
			_ => (Vector2[])(object)new Vector2[1]
			{
				new Vector2(0f, y)
			}, 
		};
	}

	public Sequence PlayPropsFlyToTarget(List<RectTransform> rects, List<Animation> anims, Vector2 screenTargetPos, Action onComplete, Action onFirstArrive = null)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		if (rects == null || rects.Count == 0)
		{
			onComplete?.Invoke();
			return null;
		}
		Vector3 worldTarget = new Vector3(screenTargetPos.x, screenTargetPos.y, 0f);
		Sequence val = DOTween.Sequence();
		for (int i = 0; i < rects.Count; i++)
		{
			RectTransform rect = rects[i];
			if ((Object)(object)rect == (Object)null)
			{
				continue;
			}
			int idx = i;
			float t = 0f;
			Vector2 startPos = default(Vector2);
			Sequence val2 = TweenSettingsExtensions.AppendCallback(DOTween.Sequence(), (TweenCallback)delegate
			{
				//IL_006e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0073: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				Animation val3 = ((anims != null && idx < anims.Count) ? anims[idx] : null);
				if ((Object)(object)val3 != (Object)null && (Object)(object)val3.GetClip("anim_item_disappear_gp1") != (Object)null)
				{
					val3.Play("anim_item_disappear_gp1");
				}
				startPos = Vector2.op_Implicit(((Transform)rect).position);
				t = 0f;
			});
			if ((Object)(object)_flyBackPath != (Object)null)
			{
				TweenSettingsExtensions.Append(val2, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => t), (DOSetter<float>)delegate(float v)
				{
					//IL_0024: Unknown result type (might be due to invalid IL or missing references)
					//IL_002f: Unknown result type (might be due to invalid IL or missing references)
					//IL_0034: Unknown result type (might be due to invalid IL or missing references)
					//IL_0039: Unknown result type (might be due to invalid IL or missing references)
					//IL_003e: Unknown result type (might be due to invalid IL or missing references)
					t = v;
					((Transform)rect).position = Vector2.op_Implicit(_flyBackPath.Evaluate(t, startPos, Vector2.op_Implicit(worldTarget)));
				}, 1f, 0.5f), (Ease)1));
			}
			else
			{
				TweenSettingsExtensions.Append(val2, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove((Transform)(object)rect, worldTarget, 0.5f, false), (Ease)8));
			}
			TweenSettingsExtensions.AppendCallback(val2, (TweenCallback)delegate
			{
				((Component)rect).gameObject.SetActive(false);
			});
			TweenSettingsExtensions.Insert(val, (float)idx * 0.05f, (Tween)(object)val2);
		}
		TweenSettingsExtensions.InsertCallback(val, 0.5f, (TweenCallback)delegate
		{
			onFirstArrive?.Invoke();
		});
		float num = (float)Mathf.Max(0, rects.Count - 1) * 0.05f + 0.5f;
		TweenSettingsExtensions.InsertCallback(val, num, (TweenCallback)delegate
		{
			onComplete?.Invoke();
		});
		return val;
	}

	public void SetClaimButtonsInteractable(bool interactable)
	{
		if ((Object)(object)btnClaim != (Object)null)
		{
			((Selectable)btnClaim).interactable = interactable;
		}
		if ((Object)(object)btnContinue != (Object)null)
		{
			((Selectable)btnContinue).interactable = interactable;
		}
		if ((Object)(object)btnClaimX2 != (Object)null)
		{
			((Selectable)btnClaimX2).interactable = interactable;
		}
	}

	private static void PlayPanelAnimation(Animation anim, string clipName)
	{
		if (!((Object)(object)anim == (Object)null) && !((Object)(object)anim.GetClip(clipName) == (Object)null))
		{
			anim.Stop();
			anim.Rewind(clipName);
			AnimationState val = anim[clipName];
			if ((TrackedReference)(object)val != (TrackedReference)null)
			{
				val.normalizedTime = 0f;
				val.wrapMode = (WrapMode)8;
			}
			anim.Sample();
			anim.Play(clipName);
		}
	}

	private static float GetClipDuration(Animation anim, string clipName)
	{
		if ((Object)(object)anim == (Object)null)
		{
			return 0f;
		}
		AnimationClip clip = anim.GetClip(clipName);
		if (!((Object)(object)clip != (Object)null))
		{
			return 0f;
		}
		return clip.length;
	}

	public override void Dispose()
	{
		if (_claimX2LayoutRefreshTimerId >= 0)
		{
			GameApp.Timer.Kill(_claimX2LayoutRefreshTimerId);
			_claimX2LayoutRefreshTimerId = -1;
		}
		Tween openAutoStopTween = _openAutoStopTween;
		if (openAutoStopTween != null)
		{
			TweenExtensions.Kill(openAutoStopTween, false);
		}
		_openAutoStopTween = null;
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btnOk = behaviour.Get<Button>("btnOk");
		PanelAnimation = behaviour.Get<Animation>("PanelAnimation");
		UIPanel = behaviour.Get<CanvasGroup>("UIPanel");
		txtDesc = behaviour.Get<TMP_Text>("txtDesc");
		GoPropList = behaviour.Get<GameObject>("GoPropList");
		btnClaimX2 = behaviour.Get<Button>("btnClaimX2");
		txtClaimX2 = behaviour.Get<TMP_Text>("txtClaimX2");
		btnClaim = behaviour.Get<Button>("btnClaim");
		HLayout = behaviour.Get<RectTransform>("HLayout");
		PropItemTemplate = behaviour.Get<GameObject>("PropItemTemplate");
		btnContinue = behaviour.Get<Button>("btnContinue");
		StartPos = behaviour.Get<RectTransform>("StartPos");
		txtCongratulations = behaviour.Get<TMP_Text>("txtCongratulations");
		txtClaim = behaviour.Get<TMP_Text>("txtClaim");
		txtContinue = behaviour.Get<TMP_Text>("txtContinue");
		box_node = behaviour.Get<GameObject>("box_node");
		objBoxEndPos = behaviour.Get<GameObject>("objBoxEndPos");
		objRewardStartPos = behaviour.Get<GameObject>("objRewardStartPos");
		SkeletonGraphic = behaviour.Get<SkeletonGraphic>("SkeletonGraphic");
		Prop1 = behaviour.Get<GameObject>("Prop1");
		Prop2 = behaviour.Get<GameObject>("Prop2");
		Prop3 = behaviour.Get<GameObject>("Prop3");
		bgMask = behaviour.Get<Animation>("bgMask");
		Layout = behaviour.Get<HorizontalLayoutGroup>("Layout");
		RankType1 = behaviour.Get<GameObject>("RankType1");
		RankType1Con = behaviour.Get<TMP_Text>("RankType1Con");
		RankType1Txt = behaviour.Get<TMP_Text>("RankType1Txt");
		RankType2 = behaviour.Get<GameObject>("RankType2");
		RankType2Con = behaviour.Get<TMP_Text>("RankType2Con");
		RankType2Txt = behaviour.Get<TMP_Text>("RankType2Txt");
		RankType3 = behaviour.Get<GameObject>("RankType3");
		RankType3Con = behaviour.Get<TMP_Text>("RankType3Con");
		RankType3Con_1 = behaviour.Get<TMP_Text>("RankType3Con_1");
		RankType3Con_2 = behaviour.Get<TMP_Text>("RankType3Con_2");
		RankType3Con_3 = behaviour.Get<TMP_Text>("RankType3Con_3");
		RankType3Con_4 = behaviour.Get<TMP_Text>("RankType3Con_4");
		RankType3Txt = behaviour.Get<TMP_Text>("RankType3Txt");
		RankType4 = behaviour.Get<GameObject>("RankType4");
		RankType4Con = behaviour.Get<TMP_Text>("RankType4Con");
		RankType4Txt = behaviour.Get<TMP_Text>("RankType4Txt");
		RankType4Txt_1 = behaviour.Get<TMP_Text>("RankType4Txt_1");
		RankType4Txt_2 = behaviour.Get<TMP_Text>("RankType4Txt_2");
		RankType4Txt_3 = behaviour.Get<TMP_Text>("RankType4Txt_3");
		RankType4Txt_4 = behaviour.Get<TMP_Text>("RankType4Txt_4");
	}
}
