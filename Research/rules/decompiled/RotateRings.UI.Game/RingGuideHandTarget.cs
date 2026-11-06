using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RotateRings.Common;
using UnityEngine;

namespace RotateRings.UI.Game;

public class RingGuideHandTarget : IGuideHandTarget
{
	private const float RotateDuration = 0.6f;

	private const float RotateAngle = 100f;

	private const float SpineHandYOffset = 100f;

	private readonly UIGameGuideHandService service;

	private RingObject ring;

	public RingGuideHandTarget(UIGameGuideHandService service, RingObject ring)
	{
		this.service = service;
		this.ring = ring;
	}

	public void SetupRotation()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		service.ActiveInstanceTransform.localRotation = Quaternion.identity;
	}

	public void SetupInstance()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = service.ResolveLocalPosition(ring.GetPosition());
		service.ActiveInstanceTransform.localPosition = Vector2.op_Implicit(val);
		float guideRadius = GetGuideRadius();
		float num = WorldToUIPostionUtil.WorldRadiusToLocal(ring.GetPosition(), guideRadius, service.CurrentParent);
		Transform activeSpineHandTransform = service.ActiveSpineHandTransform;
		if ((Object)(object)activeSpineHandTransform != (Object)null)
		{
			activeSpineHandTransform.localPosition = new Vector3(0f, 0f - num - 100f, 0f);
		}
	}

	public Tween CreateGestureTween()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DOLocalRotate(service.ActiveInstanceTransform, new Vector3(0f, 0f, 100f), 0.6f, (RotateMode)3), (Ease)4), -1, (LoopType)1);
	}

	internal void Reset(RingObject newRing)
	{
		ring = newRing;
	}

	private float GetGuideRadius()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Collider2D component = ((Component)ring).GetComponent<Collider2D>();
		if ((Object)(object)component != (Object)null)
		{
			Bounds bounds = component.bounds;
			return ((Bounds)(ref bounds)).extents.x;
		}
		return 50f;
	}
}
