using System;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingSolveMaterialSync
{
	public const string SourceShaderName = "HsApp/Effect/StyleLine";

	public const string LegacySourceShaderName = "HsApp/Effect/XYStyleLine";

	public const string MainShaderName = "HsApp/Particles/XYStyleFragment";

	public const string ShadowShaderName = "Hidden/HsApp/Particles/StyleFragmentShadow";

	private const string MainTextureProperty = "_MainTex";

	private static readonly int ColorIntensityId = Shader.PropertyToID("_ColorIntensity");

	private static readonly int BlendId = Shader.PropertyToID("_Blend");

	private readonly string[] mainColors = new string[5] { "_Color", "_MatCapColor", "_LightColor", "_ShadowColor", "_UnderlayColor" };

	private readonly string[] mainVectors = new string[1] { "_LightDir" };

	private readonly string[] rectMaskVectors = new string[2] { "_StyleLineRectMaskRect", "_StyleLineRectMaskSoftness" };

	private readonly string[] mainTextures = new string[1] { "_MatCapTex" };

	private readonly string[] mainFloats = new string[28]
	{
		"_ColorIntensity", "_Blend", "_RecolorStrength", "_UseVertexColor", "_Contrast", "_Saturation", "_ColorfulBlend", "_AlphaClipEnable", "_AlphaClip", "_NormalMapEnable",
		"_NormalScale", "_MatCapEnable", "_MatCapStrength", "_MatCapLOD", "_MatCapRotation", "_MatCapBlendMode", "_SimpleLightEnable", "_LightIntensity", "_LightStrength", "_LightWrap",
		"_Underlay", "_UnderlayDilate", "_UnderlaySoftness", "_SrcBlend", "_DstBlend", "_Cull", "_ZWrite", "_ZTest"
	};

	private readonly string[] mainKeywords = new string[8] { "_ALPHACLIP_ON", "_NORMALMAP_ON", "_MATCAP_ON", "_MATCAP_LOD_ON", "_SIMPLELIGHT_ON", "_UNDERLAY_NONE", "_UNDERLAY_NORMAL", "_UNDERLAY_INNER" };

	private readonly string[] shadowColors = new string[1] { "_ShadowTintColor" };

	private readonly string[] shadowFloats = new string[7] { "_ShadowOffsetX", "_ShadowOffsetY", "_ShadowDilate", "_ShadowSoftness", "_ShadowZOffset", "_ShadowZWrite", "_ZTest" };

	public bool IsSupportedSource(Material source)
	{
		if ((Object)(object)source != (Object)null && (Object)(object)source.shader != (Object)null)
		{
			return IsSupportedSourceShader(((Object)source.shader).name);
		}
		return false;
	}

	public void Copy(Material sourceStyleLine, Material target, float? colorIntensityOverride = null, float? blendToWhiteOverride = null)
	{
		ValidateSource(sourceStyleLine);
		if ((Object)(object)target == (Object)null || (Object)(object)target.shader == (Object)null)
		{
			throw new ArgumentNullException("target");
		}
		string name = ((Object)target.shader).name;
		if (!(name == "HsApp/Particles/XYStyleFragment"))
		{
			if (!(name == "Hidden/HsApp/Particles/StyleFragmentShadow"))
			{
				throw new ArgumentException("Unsupported StyleFragment target shader: '" + ((Object)target.shader).name + "'.", "target");
			}
			CopyColors(sourceStyleLine, target, shadowColors);
			CopyFloats(sourceStyleLine, target, shadowFloats);
			CopyVectors(sourceStyleLine, target, rectMaskVectors);
		}
		else
		{
			CopyColors(sourceStyleLine, target, mainColors);
			CopyVectors(sourceStyleLine, target, mainVectors);
			CopyVectors(sourceStyleLine, target, rectMaskVectors);
			CopyTextures(sourceStyleLine, target, mainTextures);
			CopyFloats(sourceStyleLine, target, mainFloats);
			CopyKeywords(sourceStyleLine, target, mainKeywords);
			SyncFragmentKeywordsFromProperties(target);
			ApplyColorIntensityOverride(target, colorIntensityOverride);
			ApplyBlendToWhiteOverride(target, blendToWhiteOverride);
		}
	}

	public bool IsShadowEnabled(Material sourceStyleLine)
	{
		ValidateSource(sourceStyleLine);
		if (sourceStyleLine.HasProperty("_ShadowEnable"))
		{
			return sourceStyleLine.GetFloat("_ShadowEnable") > 0.5f;
		}
		return false;
	}

	public void ApplyFragmentMainTexture(Material pooledMainMaterial, Texture texture)
	{
		if (!((Object)(object)pooledMainMaterial == (Object)null) && !((Object)(object)texture == (Object)null) && !((Object)(object)pooledMainMaterial.shader == (Object)null) && !(((Object)pooledMainMaterial.shader).name != "HsApp/Particles/XYStyleFragment") && pooledMainMaterial.HasProperty("_MainTex"))
		{
			pooledMainMaterial.SetTexture("_MainTex", texture);
		}
	}

	private static void ApplyColorIntensityOverride(Material target, float? colorIntensityOverride)
	{
		if (!((Object)(object)target == (Object)null) && colorIntensityOverride.HasValue && target.HasProperty(ColorIntensityId))
		{
			target.SetFloat(ColorIntensityId, colorIntensityOverride.Value);
		}
	}

	private static void ApplyBlendToWhiteOverride(Material target, float? blendToWhiteOverride)
	{
		if (!((Object)(object)target == (Object)null) && blendToWhiteOverride.HasValue && target.HasProperty(BlendId))
		{
			target.SetFloat(BlendId, blendToWhiteOverride.Value);
		}
	}

	private bool IsSupportedSourceShader(string shaderName)
	{
		if (!(shaderName == "HsApp/Effect/StyleLine"))
		{
			return shaderName == "HsApp/Effect/XYStyleLine";
		}
		return true;
	}

	private void ValidateSource(Material source)
	{
		if ((Object)(object)source == (Object)null || (Object)(object)source.shader == (Object)null)
		{
			throw new ArgumentNullException("source");
		}
		if (!IsSupportedSourceShader(((Object)source.shader).name))
		{
			throw new ArgumentException("Expected source shader 'HsApp/Effect/StyleLine' or 'HsApp/Effect/XYStyleLine', but got '" + ((Object)source.shader).name + "'.", "source");
		}
	}

	private void CopyFloats(Material source, Material target, string[] names)
	{
		foreach (string text in names)
		{
			if (source.HasProperty(text) && target.HasProperty(text))
			{
				target.SetFloat(text, source.GetFloat(text));
			}
		}
	}

	private void CopyColors(Material source, Material target, string[] names)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (string text in names)
		{
			if (source.HasProperty(text) && target.HasProperty(text))
			{
				target.SetColor(text, source.GetColor(text));
			}
		}
	}

	private void CopyVectors(Material source, Material target, string[] names)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		foreach (string text in names)
		{
			if (source.HasProperty(text) && target.HasProperty(text))
			{
				target.SetVector(text, source.GetVector(text));
			}
		}
	}

	private void CopyTextures(Material source, Material target, string[] names)
	{
		foreach (string text in names)
		{
			if (source.HasProperty(text) && target.HasProperty(text))
			{
				target.SetTexture(text, source.GetTexture(text));
			}
		}
	}

	private void CopyKeywords(Material source, Material target, string[] names)
	{
		foreach (string text in names)
		{
			if (source.IsKeywordEnabled(text))
			{
				target.EnableKeyword(text);
			}
			else
			{
				target.DisableKeyword(text);
			}
		}
	}

	private static void SyncFragmentKeywordsFromProperties(Material material)
	{
		SetKeyword(material, "_NORMALMAP_ON", material.GetFloat("_NormalMapEnable") > 0.5f);
		SetKeyword(material, "_MATCAP_ON", material.GetFloat("_MatCapEnable") > 0.5f);
		SetKeyword(material, "_SIMPLELIGHT_ON", material.GetFloat("_SimpleLightEnable") > 0.5f);
		SetKeyword(material, "_MATCAP_LOD_ON", material.GetFloat("_MatCapLOD") > 0.001f);
		SetKeyword(material, "_ALPHACLIP_ON", material.GetFloat("_AlphaClipEnable") > 0.5f);
		int num = Mathf.RoundToInt(material.GetFloat("_Underlay"));
		SetKeyword(material, "_UNDERLAY_NONE", num == 0);
		SetKeyword(material, "_UNDERLAY_NORMAL", num == 1);
		SetKeyword(material, "_UNDERLAY_INNER", num == 2);
	}

	private static void SetKeyword(Material material, string keyword, bool enabled)
	{
		if (enabled)
		{
			material.EnableKeyword(keyword);
		}
		else
		{
			material.DisableKeyword(keyword);
		}
	}
}
