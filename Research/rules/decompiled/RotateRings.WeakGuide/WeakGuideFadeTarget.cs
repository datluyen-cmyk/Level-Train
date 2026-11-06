using UnityEngine;

namespace RotateRings.WeakGuide;

public readonly struct WeakGuideFadeTarget
{
	public Renderer Renderer { get; }

	public Material Material { get; }

	public string ColorProperty { get; }

	public Color OriginalColor { get; }

	public Renderer SourceRenderer { get; }

	public WeakGuideFadeTarget(Material material, string colorProperty, Color originalColor, Renderer sourceRenderer = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Renderer = null;
		Material = material;
		ColorProperty = colorProperty;
		OriginalColor = originalColor;
		SourceRenderer = sourceRenderer;
	}

	public WeakGuideFadeTarget(Renderer renderer, string colorProperty, Color originalColor, Renderer sourceRenderer = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Renderer = renderer;
		Material = null;
		ColorProperty = colorProperty;
		OriginalColor = originalColor;
		SourceRenderer = sourceRenderer;
	}
}
