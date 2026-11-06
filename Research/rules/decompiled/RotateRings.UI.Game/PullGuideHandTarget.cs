using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace RotateRings.UI.Game;

public class PullGuideHandTarget : IGuideHandTarget
{
	private const float MoveDuration = 0.6f;

	private const float MoveDistance = 150f;

	private readonly UIGameGuideHandService service;

	private PullObject pull;

	public PullGuideHandTarget(UIGameGuideHandService service, PullObject pull)
	{
		this.service = service;
		this.pull = pull;
	}

	public void SetupRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Vector2 dragDirection = GetDragDirection();
		Vector2 val = ((Vector2)(ref dragDirection)).normalized;
		if (val == Vector2.zero)
		{
			val = Vector2.up;
		}
		float num = Mathf.Atan2(val.y, val.x) * 57.29578f - 90f;
		service.ActiveInstanceTransform.localRotation = Quaternion.Euler(0f, 0f, num);
	}

	public void SetupInstance()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		RectTransform activeInstanceRect = service.ActiveInstanceRect;
		if (!((Object)(object)activeInstanceRect == (Object)null))
		{
			Vector2 anchoredPosition = service.ResolveLocalPosition(pull.GetPosition());
			activeInstanceRect.anchoredPosition = anchoredPosition;
		}
	}

	public Tween CreateGestureTween()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		RectTransform activeInstanceRect = service.ActiveInstanceRect;
		if ((Object)(object)activeInstanceRect == (Object)null)
		{
			return null;
		}
		Vector2 dragDirection = GetDragDirection();
		Vector2 val = ((Vector2)(ref dragDirection)).normalized;
		if (val == Vector2.zero)
		{
			val = Vector2.up;
		}
		Vector2 val2 = activeInstanceRect.anchoredPosition + val * 150f;
		return (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPos(activeInstanceRect, val2, 0.6f, false), (Ease)4), -1, (LoopType)1);
	}

	internal void Reset(PullObject newPull)
	{
		pull = newPull;
	}

	private Vector2 GetDragDirection()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return pull.GetLockedDragDirection();
	}
}
