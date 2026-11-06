using UnityEngine;
using XingYun.Experiment;
using XingYun.Rendering;

namespace RotateRings.GamePlay.Services;

public sealed class RotateRingOperateHighlight
{
	internal static readonly Color OperateHighlightColor = new Color(0.5215686f, 1f, 0.9411765f, 1f);

	private const string KwOutlineOn = "_OUTLINE_ON";

	private readonly MeshRenderer _renderer;

	private readonly Material _sourceSharedMaterial;

	private readonly Color _outlineColor;

	private Material _runtimeMaterial;

	private bool _highlighted;

	private StyleLineUnderlayProxy underlayProxy;

	private RotateRingOperateHighlight(MeshRenderer renderer, Material sourceSharedMaterial, Color outlineColor)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		_renderer = renderer;
		underlayProxy = ((Component)renderer).GetComponent<StyleLineUnderlayProxy>();
		_sourceSharedMaterial = sourceSharedMaterial;
		_outlineColor = outlineColor;
	}

	internal static RotateRingOperateHighlight Create(MeshRenderer renderer, Color outlineColor)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)renderer == (Object)null || (Object)(object)((Renderer)renderer).sharedMaterial == (Object)null)
		{
			return null;
		}
		return new RotateRingOperateHighlight(renderer, ((Renderer)renderer).sharedMaterial, outlineColor);
	}

	internal void SetHighlight(bool highlight)
	{
		if ((Object)(object)_renderer == (Object)null || _highlighted == highlight)
		{
			return;
		}
		_highlighted = highlight;
		if (highlight)
		{
			EnsureRuntimeMaterial();
			ApplyOperateOutline(_runtimeMaterial);
			((Renderer)_renderer).sharedMaterial = _runtimeMaterial;
			StyleLineUnderlayProxy obj = underlayProxy;
			if (obj != null)
			{
				obj.Sync();
			}
		}
		else
		{
			((Renderer)_renderer).sharedMaterial = _sourceSharedMaterial;
			StyleLineUnderlayProxy obj2 = underlayProxy;
			if (obj2 != null)
			{
				obj2.Sync();
			}
		}
	}

	internal void Dispose()
	{
		if (_highlighted && (Object)(object)_renderer != (Object)null)
		{
			((Renderer)_renderer).sharedMaterial = _sourceSharedMaterial;
			_highlighted = false;
		}
		if (!((Object)(object)_runtimeMaterial == (Object)null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_runtimeMaterial);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_runtimeMaterial);
			}
			_runtimeMaterial = null;
		}
	}

	private void EnsureRuntimeMaterial()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (!((Object)(object)_runtimeMaterial != (Object)null))
		{
			_runtimeMaterial = new Material(_sourceSharedMaterial)
			{
				name = ((Object)_sourceSharedMaterial).name + " (OperateHighlight)"
			};
		}
	}

	private void ApplyOperateOutline(Material mat)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		mat.EnableKeyword("_OUTLINE_ON");
		mat.SetFloat("_OutlineEnable", 1f);
		mat.SetColor("_OutlineColor", _outlineColor);
		float num = 0.1f;
		float num2 = 0.01f;
		GameBord gameBord = FunctionDataCenter.Get<GameBord>();
		if (gameBord != null && gameBord.GameplayObjectHighlightOutline == 1)
		{
			num = 0.2f;
			num2 = 0.005f;
		}
		mat.SetFloat("_OutlineWidth", num);
		mat.SetFloat("_OutlineSoftness", num2);
		mat.SetFloat("_OutlineMode", 2f);
		if (mat.HasProperty("_OutlineUseVertColor"))
		{
			mat.SetFloat("_OutlineUseVertColor", 0f);
		}
		if (mat.HasProperty("_ShadowTintColor"))
		{
			Color color = mat.GetColor("_ShadowTintColor");
			color.a = 0f;
			mat.SetColor("_ShadowTintColor", color);
		}
	}
}
