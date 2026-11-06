using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using Script.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.UI.DailyFirstWin;

public static class NewPieceEffectTexturesUtil
{
	private static readonly Dictionary<int, NewPieceEffectTexturesConfig> ConfigCache = new Dictionary<int, NewPieceEffectTexturesConfig>();

	private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

	private static readonly int RemapTexId = Shader.PropertyToID("_Remap_Tex");

	private static readonly int MaskTexId = Shader.PropertyToID("_Mask_Tex");

	public static bool TryLoadConfig(int pieceDay, string logOwner, out NewPieceEffectTexturesConfig config)
	{
		config = null;
		if (pieceDay < 1 || pieceDay > 7)
		{
			return false;
		}
		if (ConfigCache.TryGetValue(pieceDay, out config) && (Object)(object)config != (Object)null)
		{
			return true;
		}
		string text = string.Format("{0}{1:D2}", "NewPieceEffectTextures_Day", pieceDay);
		GameObject val = GameApp.Res.LoadAsset<GameObject>(text, (string)null);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		config = val.GetComponent<NewPieceEffectTexturesConfig>();
		if ((Object)(object)config == (Object)null)
		{
			return false;
		}
		ConfigCache[pieceDay] = config;
		return true;
	}

	public static void ApplyImageEffectTexture(Image image, Texture2D texture, ref Material runtimeMaterial)
	{
		ApplyClonedEffectMaterial(ResolveImageMaterialTemplate(image), MainTexId, (Texture)(object)texture, ref runtimeMaterial, delegate(Material mat)
		{
			AssignImageEffectMaterial(image, mat);
		});
	}

	public static void ApplyParticleMaskTexture(ParticleSystem particle, Texture2D maskTexture, ref Material runtimeMaterial, UIParticle materialSearchRoot = null)
	{
		ApplyClonedEffectMaterial(ResolveParticleMaterialTemplate(particle, runtimeMaterial, materialSearchRoot), MaskTexId, (Texture)(object)maskTexture, ref runtimeMaterial, delegate(Material mat)
		{
			AssignParticleMaterial(particle, mat);
		});
	}

	public static void ApplyParticleFrameTexture(ParticleSystem particle, Texture2D frameTexture, ref Material runtimeMaterial, UIParticle materialSearchRoot = null)
	{
		ApplyClonedEffectMaterial(ResolveParticleMaterialTemplate(particle, runtimeMaterial, materialSearchRoot), MainTexId, (Texture)(object)frameTexture, ref runtimeMaterial, delegate(Material mat)
		{
			AssignParticleMaterial(particle, mat);
		});
	}

	public static void ApplyLandParticleTextures(int pieceDay, string logOwner, ParticleSystem sweep, UIParticle sweepRoot, ref Material sweepRuntimeMaterial, ParticleSystem fameGlow, UIParticle fameGlowRoot, ref Material fameGlowRuntimeMaterial, Action refreshUIParticles)
	{
		if (TryLoadConfig(pieceDay, logOwner, out var config))
		{
			Texture2D frameTexture = config.FrameTexture;
			Texture2D maskTexture = config.MaskTexture;
			if (!((Object)(object)frameTexture == (Object)null))
			{
				_ = (Object)(object)maskTexture == (Object)null;
			}
			if ((Object)(object)maskTexture != (Object)null)
			{
				ApplyParticleMaskTexture(sweep, maskTexture, ref sweepRuntimeMaterial, sweepRoot);
				RestartParticleAfterMaterialChange(sweep);
			}
			if ((Object)(object)frameTexture != (Object)null)
			{
				ApplyParticleFrameTexture(fameGlow, frameTexture, ref fameGlowRuntimeMaterial, fameGlowRoot);
				RestartParticleAfterMaterialChange(fameGlow);
			}
			refreshUIParticles?.Invoke();
		}
	}

	public static void RestartParticleAfterMaterialChange(ParticleSystem particle)
	{
		if (!((Object)(object)particle == (Object)null))
		{
			particle.Stop(true, (ParticleSystemStopBehavior)0);
			particle.Clear(true);
		}
	}

	public static void ReleaseRuntimeMaterial(ref Material runtimeMaterial, Action clearAssignment)
	{
		if (!((Object)(object)runtimeMaterial == (Object)null))
		{
			clearAssignment?.Invoke();
			Object.Destroy((Object)(object)runtimeMaterial);
			runtimeMaterial = null;
		}
	}

	public static void ClearImageEffectMaterial(Image image)
	{
		AssignImageEffectMaterial(image, null);
	}

	public static void ClearParticleMaterial(ParticleSystem particle)
	{
		AssignParticleMaterial(particle, null);
	}

	public static bool TryApplyPieceSprite(Image image, string spriteName, bool setNativeSize = false, bool activateOnSuccess = false)
	{
		if ((Object)(object)image == (Object)null || string.IsNullOrEmpty(spriteName))
		{
			return false;
		}
		Sprite val = SpriteManager.LoadSprite(spriteName);
		if ((Object)(object)val == (Object)null)
		{
			return false;
		}
		image.sprite = val;
		if (setNativeSize)
		{
			((Graphic)image).SetNativeSize();
		}
		if (activateOnSuccess)
		{
			((Component)image).gameObject.SetActive(true);
		}
		return true;
	}

	private static void ApplyClonedEffectMaterial(Material template, int propertyId, Texture texture, ref Material runtimeMaterial, Action<Material> assignMaterial)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (!((Object)(object)template == (Object)null) && !((Object)(object)texture == (Object)null) && assignMaterial != null)
		{
			ReleaseRuntimeMaterial(ref runtimeMaterial, null);
			Material val = new Material(template);
			val.SetTexture(propertyId, texture);
			if (propertyId == MainTexId)
			{
				val.SetTexture(RemapTexId, texture);
				val.mainTexture = texture;
			}
			runtimeMaterial = val;
			assignMaterial(val);
		}
	}

	private static void AssignImageEffectMaterial(Image image, Material material)
	{
		if (!((Object)(object)image == (Object)null))
		{
			image.sprite = null;
			((Graphic)image).material = material;
			((Graphic)image).SetAllDirty();
		}
	}

	private static void AssignParticleMaterial(ParticleSystem particle, Material material)
	{
		ParticleSystemRenderer val = (((Object)(object)particle != (Object)null) ? ((Component)particle).GetComponent<ParticleSystemRenderer>() : null);
		if ((Object)(object)val != (Object)null)
		{
			((Renderer)val).sharedMaterial = material;
		}
	}

	private static Material ResolveImageMaterialTemplate(Image image)
	{
		if (!((Object)(object)image != (Object)null))
		{
			return null;
		}
		return ((Graphic)image).material;
	}

	private static Material ResolveParticleMaterialTemplate(ParticleSystem particle, Material currentRuntime, UIParticle particleRoot)
	{
		Material val = ResolveParticleRendererMaterialTemplate(particle);
		if ((Object)(object)val != (Object)null && (Object)(object)val != (Object)(object)currentRuntime)
		{
			return val;
		}
		if ((Object)(object)particleRoot == (Object)null)
		{
			return val;
		}
		return ResolveParticleMaterialTemplateFromUIParticleRoot(particleRoot, currentRuntime) ?? val;
	}

	private static Material ResolveParticleRendererMaterialTemplate(ParticleSystem particle)
	{
		ParticleSystemRenderer val = (((Object)(object)particle != (Object)null) ? ((Component)particle).GetComponent<ParticleSystemRenderer>() : null);
		if (!((Object)(object)val != (Object)null))
		{
			return null;
		}
		return ((Renderer)val).sharedMaterial;
	}

	private static Material ResolveParticleMaterialTemplateFromUIParticleRoot(UIParticle particleRoot, Material currentRuntime)
	{
		ParticleSystemRenderer[] componentsInChildren = ((Component)particleRoot).GetComponentsInChildren<ParticleSystemRenderer>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material sharedMaterial = ((Renderer)componentsInChildren[i]).sharedMaterial;
			if ((Object)(object)sharedMaterial != (Object)null && (Object)(object)sharedMaterial != (Object)(object)currentRuntime)
			{
				return sharedMaterial;
			}
		}
		return null;
	}
}
