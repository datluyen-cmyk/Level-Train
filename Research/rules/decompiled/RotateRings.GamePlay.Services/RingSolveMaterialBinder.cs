using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingSolveMaterialBinder
{
	public struct Binding
	{
		internal int Id;

		public bool Applied;

		public Material MainMaterial;

		public Material ShadowMaterial;

		public Material MainTemplate;

		public Material ShadowTemplate;

		public ParticleSystemRenderer MainRenderer;

		public ParticleSystemRenderer ShadowRenderer;

		public ParticleSystem ShadowSystem;

		public bool ShadowEnabled;

		public bool ShadowRendererWasEnabled;

		public Material[] ExtraMainMaterials;

		public Material[] ExtraMainTemplates;

		public ParticleSystemRenderer[] ExtraMainRenderers;
	}

	private const int MaxMainRendererCount = 4;

	private readonly RingSolveMaterialSync materialSync;

	private readonly RingSolveMaterialPool materialPool;

	private readonly List<Binding> activeBindings = new List<Binding>(4);

	private readonly ParticleSystemRenderer[] mainRendererScratch = (ParticleSystemRenderer[])(object)new ParticleSystemRenderer[4];

	private readonly int[] mainRendererSortScoreScratch = new int[4];

	private int nextBindingId;

	public RingSolveMaterialBinder(RingSolveMaterialSync materialSync, RingSolveMaterialPool materialPool)
	{
		this.materialSync = materialSync ?? throw new ArgumentNullException("materialSync");
		this.materialPool = materialPool ?? throw new ArgumentNullException("materialPool");
	}

	public bool TryApply(GameObject effectInstance, GameObject partGo, IGameplayObject sourcePart, Texture fragmentMainTexture, float? fragmentColorIntensityOverride, float? fragmentBlendToWhiteOverride, out Binding binding, GameObject materialSourceGo = null)
	{
		binding = default(Binding);
		if (IsFixedPart(sourcePart))
		{
			return false;
		}
		Material sourceMaterial = (((Object)(object)materialSourceGo != (Object)null) ? TryGetSourceMaterialOnSelf(materialSourceGo) : TryGetSourceMaterial(partGo));
		if (!TryApply(effectInstance, sourceMaterial, fragmentMainTexture, fragmentColorIntensityOverride, fragmentBlendToWhiteOverride, out binding))
		{
			return false;
		}
		activeBindings.Add(binding);
		return true;
	}

	public void Release(Binding binding)
	{
		if (binding.Applied && binding.Id != 0 && RemoveActiveBinding(binding.Id))
		{
			if ((Object)(object)binding.MainRenderer != (Object)null)
			{
				((Renderer)binding.MainRenderer).sharedMaterial = binding.MainTemplate;
			}
			ReleaseExtraMainBindings(binding);
			if ((Object)(object)binding.ShadowRenderer != (Object)null)
			{
				((Renderer)binding.ShadowRenderer).sharedMaterial = binding.ShadowTemplate;
				((Renderer)binding.ShadowRenderer).enabled = binding.ShadowRendererWasEnabled;
			}
			materialPool.Release(binding.MainMaterial);
			materialPool.Release(binding.ShadowMaterial);
			ReleaseExtraMainMaterials(binding);
		}
	}

	private void ReleaseExtraMainBindings(Binding binding)
	{
		if (binding.ExtraMainRenderers == null || binding.ExtraMainTemplates == null)
		{
			return;
		}
		int num = Mathf.Min(binding.ExtraMainRenderers.Length, binding.ExtraMainTemplates.Length);
		for (int i = 0; i < num; i++)
		{
			ParticleSystemRenderer val = binding.ExtraMainRenderers[i];
			if ((Object)(object)val != (Object)null)
			{
				((Renderer)val).sharedMaterial = binding.ExtraMainTemplates[i];
			}
		}
	}

	private void ReleaseExtraMainMaterials(Binding binding)
	{
		if (binding.ExtraMainMaterials != null)
		{
			for (int i = 0; i < binding.ExtraMainMaterials.Length; i++)
			{
				materialPool.Release(binding.ExtraMainMaterials[i]);
			}
		}
	}

	private bool RemoveActiveBinding(int bindingId)
	{
		if (bindingId == 0)
		{
			return false;
		}
		for (int num = activeBindings.Count - 1; num >= 0; num--)
		{
			if (activeBindings[num].Id == bindingId)
			{
				activeBindings.RemoveAt(num);
				return true;
			}
		}
		return false;
	}

	public void ReleaseAllActive()
	{
		while (activeBindings.Count > 0)
		{
			Release(activeBindings[activeBindings.Count - 1]);
		}
	}

	private Material TryGetSourceMaterialOnSelf(GameObject partGo)
	{
		if ((Object)(object)partGo == (Object)null)
		{
			return null;
		}
		MeshRenderer component = partGo.GetComponent<MeshRenderer>();
		if ((Object)(object)component == (Object)null)
		{
			return null;
		}
		Material sharedMaterial = ((Renderer)component).sharedMaterial;
		if (!materialSync.IsSupportedSource(sharedMaterial))
		{
			return null;
		}
		return sharedMaterial;
	}

	private Material TryGetSourceMaterial(GameObject partGo)
	{
		if ((Object)(object)partGo == (Object)null)
		{
			return null;
		}
		MeshRenderer[] componentsInChildren = partGo.GetComponentsInChildren<MeshRenderer>(true);
		foreach (MeshRenderer val in componentsInChildren)
		{
			if (!((Object)(object)val == (Object)null))
			{
				Material sharedMaterial = ((Renderer)val).sharedMaterial;
				if (materialSync.IsSupportedSource(sharedMaterial))
				{
					return sharedMaterial;
				}
			}
		}
		return null;
	}

	private bool TryApply(GameObject effectInstance, Material sourceMaterial, Texture fragmentMainTexture, float? fragmentColorIntensityOverride, float? fragmentBlendToWhiteOverride, out Binding binding)
	{
		binding = default(Binding);
		if ((Object)(object)effectInstance == (Object)null || !materialSync.IsSupportedSource(sourceMaterial))
		{
			return false;
		}
		if (!TryFindParticleRenderers(effectInstance, out var mainRendererCount, out var shadowRenderer))
		{
			return false;
		}
		Material val = null;
		bool shadowRendererWasEnabled = false;
		Material mainMaterial = null;
		Material val2 = null;
		Material[] array = null;
		Material[] array2 = null;
		ParticleSystemRenderer[] array3 = null;
		ParticleSystemRenderer val3 = mainRendererScratch[0];
		Material sharedMaterial = ((Renderer)val3).sharedMaterial;
		int num = mainRendererCount - 1;
		if (num > 0)
		{
			array = (Material[])(object)new Material[num];
			array2 = (Material[])(object)new Material[num];
			array3 = (ParticleSystemRenderer[])(object)new ParticleSystemRenderer[num];
		}
		try
		{
			bool flag = false;
			if ((Object)(object)shadowRenderer != (Object)null)
			{
				val = ((Renderer)shadowRenderer).sharedMaterial;
				shadowRendererWasEnabled = ((Renderer)shadowRenderer).enabled;
				flag = materialSync.IsShadowEnabled(sourceMaterial);
			}
			for (int i = 0; i < mainRendererCount; i++)
			{
				ParticleSystemRenderer val4 = mainRendererScratch[i];
				Material sharedMaterial2 = ((Renderer)val4).sharedMaterial;
				Material val5 = materialPool.Get(sharedMaterial2);
				materialSync.Copy(sourceMaterial, val5, fragmentColorIntensityOverride, fragmentBlendToWhiteOverride);
				materialSync.ApplyFragmentMainTexture(val5, fragmentMainTexture);
				((Renderer)val4).sharedMaterial = val5;
				if (i == 0)
				{
					mainMaterial = val5;
					continue;
				}
				int num2 = i - 1;
				array[num2] = val5;
				array2[num2] = sharedMaterial2;
				array3[num2] = val4;
			}
			if ((Object)(object)shadowRenderer != (Object)null)
			{
				if (flag)
				{
					val2 = materialPool.Get(val);
					materialSync.Copy(sourceMaterial, val2);
					((Renderer)shadowRenderer).sharedMaterial = val2;
				}
				((Renderer)shadowRenderer).enabled = flag;
			}
			binding = new Binding
			{
				Id = ++nextBindingId,
				Applied = true,
				MainMaterial = mainMaterial,
				ShadowMaterial = val2,
				MainTemplate = sharedMaterial,
				ShadowTemplate = val,
				MainRenderer = val3,
				ShadowRenderer = shadowRenderer,
				ShadowSystem = (((Object)(object)shadowRenderer != (Object)null) ? ((Component)shadowRenderer).GetComponent<ParticleSystem>() : null),
				ShadowEnabled = flag,
				ShadowRendererWasEnabled = shadowRendererWasEnabled,
				ExtraMainMaterials = array,
				ExtraMainTemplates = array2,
				ExtraMainRenderers = array3
			};
			return true;
		}
		catch
		{
			RevertApply(val3, sharedMaterial, array3, array2, array, shadowRenderer, val, shadowRendererWasEnabled, mainMaterial, val2);
			binding = default(Binding);
			throw;
		}
	}

	private void RevertApply(ParticleSystemRenderer mainRenderer, Material mainTemplate, ParticleSystemRenderer[] extraMainRenderers, Material[] extraMainTemplates, Material[] extraMainMaterials, ParticleSystemRenderer shadowRenderer, Material shadowTemplate, bool shadowRendererWasEnabled, Material mainMaterial, Material shadowMaterial)
	{
		if ((Object)(object)mainRenderer != (Object)null)
		{
			((Renderer)mainRenderer).sharedMaterial = mainTemplate;
		}
		ReleaseExtraMainBindings(new Binding
		{
			ExtraMainRenderers = extraMainRenderers,
			ExtraMainTemplates = extraMainTemplates
		});
		if ((Object)(object)shadowRenderer != (Object)null)
		{
			((Renderer)shadowRenderer).sharedMaterial = shadowTemplate;
			((Renderer)shadowRenderer).enabled = shadowRendererWasEnabled;
		}
		materialPool.Release(mainMaterial);
		materialPool.Release(shadowMaterial);
		ReleaseExtraMainMaterials(new Binding
		{
			ExtraMainMaterials = extraMainMaterials
		});
	}

	private bool TryFindParticleRenderers(GameObject root, out int mainRendererCount, out ParticleSystemRenderer shadowRenderer)
	{
		mainRendererCount = 0;
		shadowRenderer = null;
		int num = 0;
		ParticleSystemRenderer[] componentsInChildren = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
		foreach (ParticleSystemRenderer val in componentsInChildren)
		{
			Material sharedMaterial = ((Renderer)val).sharedMaterial;
			if ((Object)(object)sharedMaterial == (Object)null || (Object)(object)sharedMaterial.shader == (Object)null)
			{
				continue;
			}
			string name = ((Object)sharedMaterial.shader).name;
			if (name == "HsApp/Particles/XYStyleFragment")
			{
				if (mainRendererCount >= 4)
				{
					return false;
				}
				mainRendererScratch[mainRendererCount++] = val;
			}
			else if (name == "Hidden/HsApp/Particles/StyleFragmentShadow")
			{
				shadowRenderer = val;
				num++;
			}
		}
		if (mainRendererCount == 0 || num > 1)
		{
			return false;
		}
		SortMainRenderersInPlace(mainRendererCount);
		return true;
	}

	private void SortMainRenderersInPlace(int count)
	{
		for (int i = 0; i < count; i++)
		{
			mainRendererSortScoreScratch[i] = GetMainRendererSortScore(mainRendererScratch[i]);
		}
		for (int j = 1; j < count; j++)
		{
			ParticleSystemRenderer val = mainRendererScratch[j];
			int num = mainRendererSortScoreScratch[j];
			int num2 = j - 1;
			while (num2 >= 0 && mainRendererSortScoreScratch[num2] < num)
			{
				mainRendererScratch[num2 + 1] = mainRendererScratch[num2];
				mainRendererSortScoreScratch[num2 + 1] = mainRendererSortScoreScratch[num2];
				num2--;
			}
			mainRendererScratch[num2 + 1] = val;
			mainRendererSortScoreScratch[num2 + 1] = num;
		}
	}

	private static int GetMainRendererSortScore(ParticleSystemRenderer renderer)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		if ((Object)(object)renderer == (Object)null)
		{
			return 0;
		}
		int num = (((Renderer)renderer).enabled ? 1 : 0);
		ParticleSystem component = ((Component)renderer).GetComponent<ParticleSystem>();
		if ((Object)(object)component != (Object)null)
		{
			ShapeModule shape = component.shape;
			if ((int)((ShapeModule)(ref shape)).shapeType == 6)
			{
				num += 2;
			}
		}
		return num;
	}

	private bool IsFixedPart(IGameplayObject sourcePart)
	{
		return sourcePart is FixedObject;
	}
}
