using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Example.Cfg;
using Game.UI;
using Rotaterings.Tools;
using TMPro;
using UniLog;
using UnityEngine;
using UnityEngine.UI;
using XingYun.Extensions;
using XingYun.UI.Core;

namespace RotateRings.UI.RewardedLuckyDraw;

public class UIRewardedLuckyDrawWheelView : GameBaseView, IRewardedLuckyDrawView
{
	private const float CenterAlignmentTolerance = 0.01f;

	private const string OpenAnimationName = "anim_UIRewardedLuckyDrawWheel_open";

	private const string CloseAnimationName = "anim_UIRewardedLuckyDrawWheel_close";

	private const string PopupOpenAnimationName = "anim_popup_open";

	private Animation popupAnimation;

	private Animation panelAnimation;

	private Tween panelAnimationWait;

	private GameObject backgroundMask;

	private bool maskHandedOff;

	private TMP_Text adClaimText;

	private GameObject[] slots;

	private Tween spinTween;

	private float wheelAngle;

	public Button btn_Close;

	public Button btn_Ad;

	public RectTransform rect_Wheel;

	public GameObject go_WheelFrame;

	public GameObject go_Slot01;

	public GameObject go_Slot02;

	public GameObject go_Slot03;

	public GameObject go_Slot04;

	public GameObject go_Slot05;

	public GameObject go_Slot06;

	public GameObject go_Effect3;

	public GameObject go_Effect1;

	public GameObject go_Effect2;

	public GameObject go_Effect4;

	public GameObject go_Content;

	public Button CloseButton => btn_Close;

	public Button AdButton => btn_Ad;

	public void InitializePresentation(RewardedLuckyDrawProbability[] pool, string[] iconPaths)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		StopPresentation();
		maskHandedOff = false;
		Transform obj = ((Component)((UIBaseView)this).Behaviour).transform.Find("UIPanel/bgMask");
		backgroundMask = ((obj != null) ? ((Component)obj).gameObject : null);
		GameObject obj2 = backgroundMask;
		if (obj2 != null)
		{
			obj2.SetActive(true);
		}
		go_Content.SetActive(true);
		slots = (GameObject[])(object)new GameObject[6] { go_Slot01, go_Slot02, go_Slot03, go_Slot04, go_Slot05, go_Slot06 };
		wheelAngle = 0f;
		((Transform)rect_Wheel).localRotation = Quaternion.identity;
		go_WheelFrame.SetActive(false);
		go_Effect1.SetActive(true);
		((Component)btn_Ad).gameObject.SetActive(true);
		for (int i = 0; i < slots.Length; i++)
		{
			Transform obj3 = slots[i].transform.Find("img_Prop" + (i + 1).ToString("00"));
			Image val = ((obj3 != null) ? ((Component)obj3).GetComponent<Image>() : null);
			Transform obj4 = slots[i].transform.Find("txtm_Count" + (i + 1).ToString("00"));
			TMP_Text val2 = ((obj4 != null) ? ((Component)obj4).GetComponent<TMP_Text>() : null);
			if ((Object)(object)val != (Object)null)
			{
				val.sprite = LoadAsset<Sprite>(iconPaths[i]);
			}
			if ((Object)(object)val2 != (Object)null)
			{
				val2.text = "×" + pool[i].Count;
			}
		}
		ApplyCurrentLanguage();
		RewardedLuckyDrawAdButtonLayout.Refresh(adClaimText);
	}

	public void PlayOpenAnimation(Action onCompleted)
	{
		PlayPanelAnimation("anim_UIRewardedLuckyDrawWheel_open", "anim_popup_open", onCompleted);
	}

	public void PlayCloseAnimation(Action onCompleted)
	{
		PlayPanelAnimation("anim_UIRewardedLuckyDrawWheel_close", null, onCompleted);
	}

	private void PlayPanelAnimation(string clipName, string popupClipName, Action onCompleted)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		Tween obj = panelAnimationWait;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		panelAnimationWait = null;
		if ((Object)(object)popupAnimation == (Object)null)
		{
			popupAnimation = ((Component)((UIBaseView)this).Behaviour).GetComponent<Animation>();
		}
		if ((Object)(object)panelAnimation == (Object)null)
		{
			Transform obj2 = ((Component)((UIBaseView)this).Behaviour).transform.Find("UIPanel/Content");
			panelAnimation = ((obj2 != null) ? ((Component)obj2).GetComponent<Animation>() : null);
		}
		if (string.IsNullOrEmpty(popupClipName))
		{
			Animation obj3 = popupAnimation;
			if (obj3 != null)
			{
				obj3.Stop();
			}
		}
		float num = StartPanelAnimation(panelAnimation, clipName);
		float num2 = StartPanelAnimation(popupAnimation, popupClipName);
		float num3 = Mathf.Max(num, num2);
		if (num3 <= 0f)
		{
			onCompleted?.Invoke();
			return;
		}
		panelAnimationWait = TweenSettingsExtensions.SetLink<Tween>(DOVirtual.DelayedCall(num3, (TweenCallback)delegate
		{
			CompletePanelAnimation(clipName, popupClipName, onCompleted);
		}, false), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5);
	}

	private static float StartPanelAnimation(Animation animation, string clipName)
	{
		AnimationState val = (((Object)(object)animation != (Object)null && !string.IsNullOrEmpty(clipName)) ? animation[clipName] : null);
		if ((TrackedReference)(object)val == (TrackedReference)null)
		{
			return 0f;
		}
		animation.Stop();
		val.time = 0f;
		val.speed = 1f;
		val.wrapMode = (WrapMode)8;
		animation.Play(clipName);
		animation.Sample();
		return val.length;
	}

	private static bool IsPanelAnimationPending(Animation animation, string clipName)
	{
		AnimationState val = (((Object)(object)animation != (Object)null && !string.IsNullOrEmpty(clipName)) ? animation[clipName] : null);
		if ((TrackedReference)(object)val != (TrackedReference)null && animation.IsPlaying(clipName))
		{
			return val.normalizedTime < 1f;
		}
		return false;
	}

	private void CompletePanelAnimation(string clipName, string popupClipName, Action onCompleted)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		panelAnimationWait = null;
		if (IsPanelAnimationPending(popupAnimation, popupClipName) || IsPanelAnimationPending(panelAnimation, clipName))
		{
			panelAnimationWait = TweenSettingsExtensions.SetLink<Tween>(DOVirtual.DelayedCall(0.02f, (TweenCallback)delegate
			{
				CompletePanelAnimation(clipName, popupClipName, onCompleted);
			}, false), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5);
		}
		else
		{
			FinishPanelAnimation(popupAnimation, popupClipName);
			FinishPanelAnimation(panelAnimation, clipName);
			onCompleted?.Invoke();
		}
	}

	private static void FinishPanelAnimation(Animation animation, string clipName)
	{
		AnimationState val = (((Object)(object)animation != (Object)null && !string.IsNullOrEmpty(clipName)) ? animation[clipName] : null);
		if (!((TrackedReference)(object)val == (TrackedReference)null))
		{
			val.normalizedTime = 1f;
			animation.Sample();
			animation.Stop();
		}
	}

	private void ApplyCurrentLanguage()
	{
		TMP_Text[] componentsInChildren = ((Component)((UIBaseView)this).Behaviour).GetComponentsInChildren<TMP_Text>(true);
		foreach (TMP_Text val in componentsInChildren)
		{
			if (((Object)val).name == "txtm_Title")
			{
				TMPTextExtensions.SetLocalizedText(val, "Rewarded_LuckyDrawTitle", Array.Empty<object>());
			}
			else if (((Object)val).name == "txtm_Desc")
			{
				TMPTextExtensions.SetLocalizedText(val, "Rewarded_LuckyDrawContent", Array.Empty<object>());
			}
			else if (((Object)val).name == "txt_Claim")
			{
				adClaimText = val;
				TMPTextExtensions.SetLocalizedText(val, "Rewarded_LuckyDrawButton", Array.Empty<object>());
			}
		}
	}

	public void HideContentForClaim()
	{
		go_Content.SetActive(false);
		if (!GameApp.UI.IsOpen("UILevelAdPop"))
		{
			NotifyPopupMaskStackActive(isActive: true);
		}
	}

	public void SetButtonsInteractable(bool interactable)
	{
		((Selectable)btn_Close).interactable = interactable;
		((Selectable)btn_Ad).interactable = interactable;
		if (interactable && !((Component)btn_Ad).gameObject.activeSelf)
		{
			((Component)btn_Ad).gameObject.SetActive(true);
			RewardedLuckyDrawAdButtonLayout.Refresh(adClaimText);
		}
	}

	public void HideMaskForClaimClose()
	{
		maskHandedOff = true;
		GameObject obj = backgroundMask;
		if (obj != null)
		{
			obj.SetActive(false);
		}
	}

	public void NotifyPopupMaskStackActive(bool isActive)
	{
		if (!maskHandedOff)
		{
			UIBehaviour behaviour = ((UIBaseView)this).Behaviour;
			if (behaviour != null)
			{
				((Component)behaviour).GetComponent<UIPopupMaskFader>()?.OnUiStackActiveChanged(isActive);
			}
			if (isActive)
			{
				RewardedLuckyDrawAdButtonLayout.Refresh(adClaimText);
			}
		}
	}

	public void PlayResult(int slotId, RewardedLuckyDrawWheelAnimationSettings animation, Action onCompleted)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		if (slotId < 1 || slotId > slots.Length)
		{
			throw new ArgumentOutOfRangeException("slotId");
		}
		if (!animation.IsValid)
		{
			throw new ArgumentOutOfRangeException("animation");
		}
		animation = animation.FreezeCurves();
		StopPresentation();
		go_WheelFrame.SetActive(false);
		go_Effect2.SetActive(true);
		((Component)btn_Ad).gameObject.SetActive(false);
		float z = ((Transform)rect_Wheel).localEulerAngles.z;
		float num = Mathf.Repeat(0f - slots[slotId - 1].transform.localEulerAngles.z, 360f);
		float num2 = (animation.clockwise ? (-1f) : 1f);
		float num3 = num - z;
		if (animation.clockwise)
		{
			if (num3 > 0f)
			{
				num3 -= 360f;
			}
		}
		else if (num3 < 0f)
		{
			num3 += 360f;
		}
		int num4 = Mathf.RoundToInt(animation.TotalRevolutions);
		float num5 = num2 * animation.accelRevolutions * 360f;
		float num6 = num2 * animation.cruiseRevolutions * 360f;
		float num7 = num2 * (float)num4 * 360f + num3 - num5 - num6;
		float num8 = num2 * animation.settleAngle;
		if (animation.enableSettle)
		{
			num7 += num8;
		}
		Sequence val = (Sequence)(object)(spinTween = (Tween)(object)DOTween.Sequence());
		TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.Append(TweenSettingsExtensions.SetLink<Sequence>(TweenSettingsExtensions.SetTarget<Sequence>(TweenSettingsExtensions.SetEase<Sequence>(val, (Ease)1), (object)rect_Wheel), ((Component)((UIBaseView)this).Behaviour).gameObject, (LinkBehaviour)5), CreateSpinStep(num5, animation.accelDuration, animation.accelCurve)), CreateSpinStep(num6, animation.cruiseDuration, animation.cruiseCurve)), CreateSpinStep(num7, animation.decelDuration, animation.decelCurve));
		if (animation.enableSettle)
		{
			TweenSettingsExtensions.Append(val, CreateSpinStep(0f - num8, animation.settleDuration, animation.settleCurve));
		}
		TweenSettingsExtensions.OnComplete<Sequence>(TweenSettingsExtensions.AppendInterval(TweenSettingsExtensions.AppendCallback(val, (TweenCallback)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				wheelAngle = ((Transform)rect_Wheel).localEulerAngles.z;
				go_Effect2.SetActive(false);
				go_Effect1.SetActive(true);
				go_Effect3.SetActive(true);
				go_Effect4.SetActive(true);
				ShowLockedSlotHighlight(slotId);
			}
			catch (Exception ex)
			{
				Log.LogError("RewardedLuckyDraw", "转盘停止表现异常: {0}", new object[1] { ex });
			}
		}), animation.rewardPopupDelay), (TweenCallback)delegate
		{
			spinTween = null;
			try
			{
				onCompleted?.Invoke();
			}
			catch (Exception ex)
			{
				Log.LogError("RewardedLuckyDraw", "开奖表现回调异常: {0}", new object[1] { ex });
			}
		});
	}

	private Tween CreateSpinStep(float deltaAngle, float duration, AnimationCurve curve)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate((Transform)(object)rect_Wheel, new Vector3(0f, 0f, deltaAngle), duration, (RotateMode)3), curve);
	}

	private void ShowLockedSlotHighlight(int slotId)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		int num = FindTopSlot();
		if (num != slotId)
		{
			Log.LogError("RewardedLuckyDraw", "转盘视觉槽位与锁奖不一致 locked={0} top={1}", new object[2] { slotId, num });
			return;
		}
		RectTransform val = (RectTransform)go_WheelFrame.transform;
		((Transform)val).SetParent(slots[num - 1].transform, false);
		((Transform)val).SetSiblingIndex(0);
		val.anchoredPosition = Vector2.zero;
		((Transform)val).localRotation = Quaternion.identity;
		((Transform)val).localScale = Vector3.one;
		go_WheelFrame.SetActive(true);
	}

	private int FindTopSlot()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < slots.Length; i++)
		{
			if (Mathf.Abs(Mathf.Repeat(wheelAngle + slots[i].transform.localEulerAngles.z + 180f, 360f) - 180f) <= 0.01f)
			{
				return i + 1;
			}
		}
		return 0;
	}

	public void StopPresentation()
	{
		Tween obj = panelAnimationWait;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		panelAnimationWait = null;
		if ((Object)(object)panelAnimation != (Object)null)
		{
			panelAnimation.Stop();
		}
		if ((Object)(object)popupAnimation != (Object)null)
		{
			popupAnimation.Stop();
		}
		Tween obj2 = spinTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		spinTween = null;
		if ((Object)(object)go_WheelFrame != (Object)null)
		{
			go_WheelFrame.SetActive(false);
		}
		if ((Object)(object)go_Effect1 != (Object)null)
		{
			go_Effect1.SetActive(false);
		}
		if ((Object)(object)go_Effect2 != (Object)null)
		{
			go_Effect2.SetActive(false);
		}
		if ((Object)(object)go_Effect3 != (Object)null)
		{
			go_Effect3.SetActive(false);
		}
		if ((Object)(object)go_Effect4 != (Object)null)
		{
			go_Effect4.SetActive(false);
		}
	}

	public override void Dispose()
	{
		StopPresentation();
		((UIBaseView)this).Dispose();
	}

	public override void Inject(UIBehaviour behaviour)
	{
		((UIBaseView)this).Inject(behaviour);
		btn_Close = behaviour.Get<Button>("btn_Close");
		btn_Ad = behaviour.Get<Button>("btn_Ad");
		rect_Wheel = behaviour.Get<RectTransform>("rect_Wheel");
		go_WheelFrame = behaviour.Get<GameObject>("go_WheelFrame");
		go_Slot01 = behaviour.Get<GameObject>("go_Slot01");
		go_Slot02 = behaviour.Get<GameObject>("go_Slot02");
		go_Slot03 = behaviour.Get<GameObject>("go_Slot03");
		go_Slot04 = behaviour.Get<GameObject>("go_Slot04");
		go_Slot05 = behaviour.Get<GameObject>("go_Slot05");
		go_Slot06 = behaviour.Get<GameObject>("go_Slot06");
		go_Effect3 = behaviour.Get<GameObject>("go_Effect3");
		go_Effect1 = behaviour.Get<GameObject>("go_Effect1");
		go_Effect2 = behaviour.Get<GameObject>("go_Effect2");
		go_Effect4 = behaviour.Get<GameObject>("go_Effect4");
		go_Content = behaviour.Get<GameObject>("go_Content");
	}
}
