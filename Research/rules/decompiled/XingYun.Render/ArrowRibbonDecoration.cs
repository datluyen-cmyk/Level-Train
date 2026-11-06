using UnityEngine;

namespace XingYun.Render;

public sealed class ArrowRibbonDecoration : MonoBehaviour
{
	[Header("Placement")]
	[Tooltip("Fixed arrow-local distance along the ribbon (0 = tail). Guide distance = WindowStart + this.")]
	public float LocalRibbonDistance;

	[Tooltip("Sideways offset from the centerline, in the ribbon plane.")]
	public float LateralOffset;

	[Tooltip("Extra rotation (degrees) around the plane normal, on top of the path tangent.")]
	public float RotationOffset;

	[Tooltip("Uniform local scale applied while visible.")]
	public float Scale = 1f;

	[Header("Visual (optional)")]
	[Tooltip("Sprite to display. If a SpriteRenderer is present it will be assigned.")]
	public Sprite Sprite;

	[Tooltip("Renderer toggled for show/hide. Auto-found in children if left empty.")]
	public Renderer TargetRenderer;

	private ArrowGuidePath _guide;

	private SpriteRenderer _spriteRenderer;

	private bool _visible = true;

	private void Awake()
	{
		CacheRenderer();
	}

	private void CacheRenderer()
	{
		if ((Object)(object)TargetRenderer == (Object)null)
		{
			TargetRenderer = ((Component)this).GetComponentInChildren<Renderer>(true);
		}
		ref SpriteRenderer spriteRenderer = ref _spriteRenderer;
		Renderer targetRenderer = TargetRenderer;
		spriteRenderer = (SpriteRenderer)(object)((targetRenderer is SpriteRenderer) ? targetRenderer : null);
		if ((Object)(object)_spriteRenderer == (Object)null)
		{
			_spriteRenderer = ((Component)this).GetComponentInChildren<SpriteRenderer>(true);
		}
		if ((Object)(object)_spriteRenderer != (Object)null && (Object)(object)Sprite != (Object)null)
		{
			_spriteRenderer.sprite = Sprite;
		}
	}

	public void Init(ArrowGuidePath guide)
	{
		_guide = guide;
		if ((Object)(object)TargetRenderer == (Object)null && (Object)(object)_spriteRenderer == (Object)null)
		{
			CacheRenderer();
		}
	}

	public void UpdateOnPath(float windowStart)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (_guide != null)
		{
			float dist = windowStart + LocalRibbonDistance;
			if (!_guide.Sample(dist, out var position, out var tangent))
			{
				SetVisible(visible: false);
				return;
			}
			SetVisible(visible: true);
			Vector3 planeNormal = _guide.PlaneNormal;
			Vector3 val = Vector3.Cross(planeNormal, tangent);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			((Component)this).transform.localPosition = position + normalized * LateralOffset;
			Quaternion val2 = Quaternion.LookRotation(planeNormal, tangent);
			((Component)this).transform.localRotation = val2 * Quaternion.AngleAxis(RotationOffset, Vector3.forward);
			((Component)this).transform.localScale = new Vector3(Scale, Scale, Scale);
		}
	}

	private void SetVisible(bool visible)
	{
		if (visible != _visible)
		{
			_visible = visible;
			if ((Object)(object)TargetRenderer != (Object)null)
			{
				TargetRenderer.enabled = visible;
			}
			else
			{
				((Component)this).gameObject.SetActive(visible);
			}
		}
	}
}
