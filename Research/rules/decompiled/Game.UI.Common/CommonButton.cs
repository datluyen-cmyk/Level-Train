using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI.Common;

public class CommonButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
	[Header("按钮类型")]
	[SerializeField]
	private CommonButtonType type = CommonButtonType.Scale;

	[Header("按压目标 (Press / PressWithBack)")]
	[SerializeField]
	private RectTransform nodeMove;

	[Tooltip("仅 PressWithBack 使用：背板，按下时 sizeDelta.y += pressOffsetY 制造立体压陷。")]
	[SerializeField]
	private RectTransform nodeBack;

	[Header("按压偏移 (Press / PressWithBack)")]
	[SerializeField]
	private float pressOffsetX;

	[SerializeField]
	private float pressOffsetY;

	[Header("缩放参数 (Scale)")]
	[SerializeField]
	private Transform scaleTarget;

	[SerializeField]
	private Vector3 pressedScale = new Vector3(1.1f, 1.1f, 1f);

	[SerializeField]
	private float scaleDuration = 0.1f;

	[SerializeField]
	private Ease scaleEase = (Ease)6;

	[Header("点击回调(可选)")]
	[SerializeField]
	private UnityEvent clickEvent = new UnityEvent();

	[Header("按钮埋点(可选)")]
	[Tooltip("固定 g_btn_click.click_type。为空时不走 CommonButton 统一埋点，由业务 Controller 自行上报动态内容。")]
	[SerializeField]
	private string trackClickType = string.Empty;

	[Header("音效控制")]
	[Tooltip("勾选后 CommonButton 不再播放 UIClick / UIClickDown / UIClickOff 音效。适用于 toggle 类按钮（业务自行播音效）。")]
	[SerializeField]
	private bool disableClickSound;

	private Vector2 normalAnchoredPosition;

	private Vector2 normalBackSize;

	private Vector3 normalScale;

	private bool isPressed;

	private bool hasCachedState;

	private Selectable selectable;

	private bool clickArmed;

	public CommonButtonType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public RectTransform NodeMove
	{
		get
		{
			return nodeMove;
		}
		set
		{
			nodeMove = value;
			CacheNormalState();
		}
	}

	public RectTransform NodeBack
	{
		get
		{
			return nodeBack;
		}
		set
		{
			nodeBack = value;
			CacheNormalState();
		}
	}

	public Transform ScaleTarget
	{
		get
		{
			return scaleTarget;
		}
		set
		{
			scaleTarget = value;
			CacheNormalState();
		}
	}

	public float PressOffsetX
	{
		get
		{
			return pressOffsetX;
		}
		set
		{
			pressOffsetX = value;
		}
	}

	public float PressOffsetY
	{
		get
		{
			return pressOffsetY;
		}
		set
		{
			pressOffsetY = value;
		}
	}

	public Vector3 PressedScale
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return pressedScale;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			pressedScale = value;
		}
	}

	public float ScaleDuration
	{
		get
		{
			return scaleDuration;
		}
		set
		{
			scaleDuration = Mathf.Max(0f, value);
		}
	}

	public UnityEvent ClickEvent => clickEvent;

	public string TrackClickType
	{
		get
		{
			return trackClickType;
		}
		set
		{
			trackClickType = value ?? string.Empty;
		}
	}

	public bool DisableClickSound
	{
		get
		{
			return disableClickSound;
		}
		set
		{
			disableClickSound = value;
		}
	}

	protected virtual void Awake()
	{
		if (type == CommonButtonType.Scale)
		{
			if ((Object)(object)scaleTarget == (Object)null)
			{
				scaleTarget = ((Component)this).transform;
			}
		}
		else if ((Object)(object)nodeMove == (Object)null)
		{
			ref RectTransform reference = ref nodeMove;
			Transform transform = ((Component)this).transform;
			reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
		selectable = ((Component)this).GetComponent<Selectable>();
		CacheNormalState();
	}

	protected virtual void OnEnable()
	{
		CancelPressState();
	}

	protected virtual void OnDisable()
	{
		CancelPressState();
	}

	protected virtual void OnApplicationPause(bool pauseStatus)
	{
		if (pauseStatus)
		{
			CancelPressState();
		}
	}

	protected virtual void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			CancelPressState();
		}
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!CanClick() || isPressed || !HasPressTarget())
		{
			return;
		}
		clickArmed = true;
		CacheNormalState();
		isPressed = true;
		switch (type)
		{
		case CommonButtonType.Press:
			nodeMove.anchoredPosition = normalAnchoredPosition + new Vector2(pressOffsetX, pressOffsetY);
			break;
		case CommonButtonType.PressWithBack:
			if ((Object)(object)nodeBack != (Object)null)
			{
				nodeBack.sizeDelta = new Vector2(normalBackSize.x, normalBackSize.y + pressOffsetY);
			}
			nodeMove.anchoredPosition = normalAnchoredPosition + new Vector2(pressOffsetX, pressOffsetY);
			break;
		case CommonButtonType.Scale:
			TweenScale(pressedScale);
			break;
		}
		PlayPressSound();
	}

	public virtual void OnPointerUp(PointerEventData eventData)
	{
		if (!isPressed)
		{
			return;
		}
		RestoreNormalState(immediate: false);
		if (eventData != null && eventData.dragging)
		{
			clickArmed = false;
			PlayClickOffSound();
		}
		else if (clickArmed)
		{
			clickArmed = false;
			TrackFixedButtonClick();
			UnityEvent obj = clickEvent;
			if (obj != null)
			{
				obj.Invoke();
			}
			PlayClickSound();
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		if (isPressed)
		{
			clickArmed = false;
			PlayClickOffSound();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (eventData != null && !((Object)(object)eventData.pointerPress != (Object)(object)((Component)this).gameObject) && isPressed && CanClick())
		{
			clickArmed = true;
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
	}

	private bool HasPressTarget()
	{
		if (type != CommonButtonType.Scale)
		{
			return (Object)(object)nodeMove != (Object)null;
		}
		return (Object)(object)scaleTarget != (Object)null;
	}

	private void CacheNormalState()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		switch (type)
		{
		case CommonButtonType.Press:
			if ((Object)(object)nodeMove != (Object)null)
			{
				normalAnchoredPosition = nodeMove.anchoredPosition;
				hasCachedState = true;
			}
			break;
		case CommonButtonType.PressWithBack:
			if ((Object)(object)nodeMove != (Object)null)
			{
				normalAnchoredPosition = nodeMove.anchoredPosition;
			}
			if ((Object)(object)nodeBack != (Object)null)
			{
				normalBackSize = nodeBack.sizeDelta;
			}
			hasCachedState = (Object)(object)nodeMove != (Object)null || (Object)(object)nodeBack != (Object)null;
			break;
		case CommonButtonType.Scale:
			if ((Object)(object)scaleTarget != (Object)null && !DOTween.IsTweening((object)scaleTarget, false))
			{
				normalScale = scaleTarget.localScale;
				hasCachedState = true;
			}
			break;
		}
	}

	private void CancelPressState()
	{
		if (isPressed || clickArmed)
		{
			clickArmed = false;
			RestoreNormalState(immediate: true);
		}
	}

	protected virtual void RestoreNormalState(bool immediate)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (hasCachedState)
		{
			switch (type)
			{
			case CommonButtonType.Press:
				if ((Object)(object)nodeMove != (Object)null)
				{
					nodeMove.anchoredPosition = normalAnchoredPosition;
				}
				break;
			case CommonButtonType.PressWithBack:
				if ((Object)(object)nodeMove != (Object)null)
				{
					nodeMove.anchoredPosition = normalAnchoredPosition;
				}
				if ((Object)(object)nodeBack != (Object)null)
				{
					nodeBack.sizeDelta = normalBackSize;
				}
				break;
			case CommonButtonType.Scale:
				if ((Object)(object)scaleTarget != (Object)null)
				{
					if (immediate)
					{
						ShortcutExtensions.DOKill((Component)(object)scaleTarget, false);
						scaleTarget.localScale = normalScale;
					}
					else
					{
						TweenScale(normalScale);
					}
				}
				break;
			}
		}
		isPressed = false;
	}

	private void TweenScale(Vector3 targetScale)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)scaleTarget == (Object)null))
		{
			ShortcutExtensions.DOKill((Component)(object)scaleTarget, false);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(scaleTarget, targetScale, scaleDuration), scaleEase);
		}
	}

	private bool CanClick()
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			if (!((Object)(object)selectable == (Object)null))
			{
				return selectable.IsInteractable();
			}
			return true;
		}
		return false;
	}

	private void PlayClickSound()
	{
		_ = disableClickSound;
	}

	private void PlayPressSound()
	{
		_ = disableClickSound;
	}

	private void PlayClickOffSound()
	{
		_ = disableClickSound;
	}

	private void TrackFixedButtonClick()
	{
		string.IsNullOrWhiteSpace(trackClickType);
	}
}
