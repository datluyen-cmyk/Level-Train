using RotateRings.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vibration;

namespace Game.UI.Common;

[AddComponentMenu("UI/XY Button", 32)]
[DisallowMultipleComponent]
public class XYButton : Button
{
	[SerializeField]
	[Tooltip("两次有效点击之间的最短间隔（秒），使用 unscaled 时间")]
	private float clickCooldown = 0.3f;

	[SerializeField]
	[Tooltip("点击时播放的动画片段；赋值后自动挂载 Animation 并绑定，不自动播放")]
	private AnimationClip clickAnimationClip;

	[SerializeField]
	[Tooltip("点击音效资源名，留空则不播放")]
	private string clickAudio = "rr_sfx_click";

	[SerializeField]
	[Tooltip("是否启用震动反馈")]
	private bool useVibrate = true;

	private float lastAcceptedClickUnscaledTime = float.NegativeInfinity;

	private Animation clickAnimation;

	public float ClickCooldown
	{
		get
		{
			return clickCooldown;
		}
		set
		{
			clickCooldown = Mathf.Max(0f, value);
		}
	}

	public AnimationClip ClickAnimationClip
	{
		get
		{
			return clickAnimationClip;
		}
		set
		{
			clickAnimationClip = value;
			SyncClickAnimation();
		}
	}

	public string ClickAudio
	{
		get
		{
			return clickAudio;
		}
		set
		{
			clickAudio = value ?? string.Empty;
		}
	}

	protected override void Awake()
	{
		((Selectable)this).Awake();
		SyncClickAnimation();
	}

	public void ResetCooldown()
	{
		lastAcceptedClickUnscaledTime = float.NegativeInfinity;
	}

	public void PlayClickFeedback()
	{
		PlayClickAnimation();
	}

	protected override void OnDisable()
	{
		((Selectable)this).OnDisable();
		ResetCooldown();
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 0 && ((UIBehaviour)this).IsActive() && ((Selectable)this).IsInteractable() && TryConsumeClick())
		{
			PlayClickAnimation();
			PlayClickAudio();
			if (useVibrate)
			{
				VibrateUtility.ButtonVibrate();
			}
			((Button)this).OnPointerClick(eventData);
		}
	}

	public override void OnSubmit(BaseEventData eventData)
	{
		if (((UIBehaviour)this).IsActive() && ((Selectable)this).IsInteractable() && TryConsumeClick())
		{
			PlayClickAnimation();
			PlayClickAudio();
			((Button)this).OnSubmit(eventData);
		}
	}

	private bool TryConsumeClick()
	{
		float unscaledTime = Time.unscaledTime;
		if (unscaledTime - lastAcceptedClickUnscaledTime < clickCooldown)
		{
			return false;
		}
		lastAcceptedClickUnscaledTime = unscaledTime;
		return true;
	}

	private void SyncClickAnimation()
	{
		if ((Object)(object)clickAnimationClip == (Object)null)
		{
			clickAnimation = null;
			return;
		}
		clickAnimation = ((Component)this).GetComponent<Animation>();
		if ((Object)(object)clickAnimation == (Object)null)
		{
			clickAnimation = ((Component)this).gameObject.AddComponent<Animation>();
		}
		clickAnimation.playAutomatically = false;
		string name = ((Object)clickAnimationClip).name;
		if ((Object)(object)clickAnimation.GetClip(name) != (Object)(object)clickAnimationClip)
		{
			if ((Object)(object)clickAnimation.GetClip(name) != (Object)null)
			{
				clickAnimation.RemoveClip(name);
			}
			clickAnimation.AddClip(clickAnimationClip, name);
		}
	}

	private void PlayClickAnimation()
	{
		if (!((Object)(object)clickAnimationClip == (Object)null))
		{
			SyncClickAnimation();
			if (!((Object)(object)clickAnimation == (Object)null))
			{
				string name = ((Object)clickAnimationClip).name;
				clickAnimation.Rewind(name);
				clickAnimation.Play(name);
			}
		}
	}

	private void PlayClickAudio()
	{
		if (!string.IsNullOrEmpty(clickAudio))
		{
			AudioService.PlayUiSound(clickAudio);
		}
	}
}
