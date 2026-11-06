using System.Collections.Generic;
using RotateRings.Common;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Objects;
using RotateRings.GamePlay.PartStyle.ColorProvider;
using RotateRings.Services;
using UnityEngine;
using Vibration;
using XingYun.Render;
using XingYun.RingGame;

namespace RotateRings.GamePlay.PartStyle;

public abstract class PartStyleHandlerBase : IPartStyleHandler
{
	private const string TAG = "[PartStyle][PartStyleHandlerBase] ";

	public const string MaterialResGroup = "RingPartStyleMaterial";

	private const string DefaultPaletteLockGapMaterialPath = "RingExtra_Palette";

	private static readonly int IdSaturation = Shader.PropertyToID("_Saturation");

	private string loadedSolveFragmentMainTexturePath;

	private string solveFragmentMainTextureLoadFailedPath;

	private Texture solveFragmentMainTexture;

	private IColorProvider colorProvider;

	public abstract PartStyleType Style { get; }

	public virtual string GetTrackRingMaterialName()
	{
		return Style.ToString();
	}

	public void SetColorProvider(IColorProvider provider)
	{
		colorProvider = provider;
	}

	protected abstract string GetRingMaterialPath();

	protected abstract string GetPullMaterialPath();

	protected abstract string GetFixedMaterialPath();

	protected virtual string GetPaletteLockGapMaterialPath()
	{
		string ringMaterialPath = GetRingMaterialPath();
		if (string.IsNullOrEmpty(ringMaterialPath))
		{
			return "RingExtra_Palette";
		}
		return ringMaterialPath + "_Palette";
	}

	internal Material LoadPaletteLockGapMaterial(Dictionary<string, Material> materialCache, ISet<string> loadedMaterialPaths)
	{
		return LoadMaterialOnce(GetPaletteLockGapMaterialPath(), materialCache, loadedMaterialPaths);
	}

	protected virtual string GetSolveFragmentMainTexturePath()
	{
		return null;
	}

	public virtual void OnPartDragBegin(IGameplayObject gameplayObject)
	{
	}

	public virtual void OnDragCollisionStop(RingDragCollisionStopEvent evt)
	{
		AudioService.PlaySound(AudioKeys.Blocked.Value);
	}

	public virtual void PlaySolveBreakSound()
	{
		AudioService.PlayUiSound(AudioKeys.CeramicRingBreak.Value);
		AudioService.PlayUiSound(AudioKeys.CeramicRingRelease.Value);
		VibrateUtility.Vibrate(1102);
	}

	public virtual Texture GetSolveFragmentMainTexture()
	{
		string solveFragmentMainTexturePath = GetSolveFragmentMainTexturePath();
		if (string.IsNullOrEmpty(solveFragmentMainTexturePath))
		{
			return null;
		}
		if ((Object)(object)solveFragmentMainTexture != (Object)null && loadedSolveFragmentMainTexturePath == solveFragmentMainTexturePath)
		{
			return solveFragmentMainTexture;
		}
		if (solveFragmentMainTextureLoadFailedPath == solveFragmentMainTexturePath)
		{
			return null;
		}
		ReleaseSolveFragmentMainTexture();
		solveFragmentMainTexture = GameApp.Res.LoadAsset<Texture>(solveFragmentMainTexturePath, "RingPartStyleMaterial");
		if ((Object)(object)solveFragmentMainTexture != (Object)null)
		{
			loadedSolveFragmentMainTexturePath = solveFragmentMainTexturePath;
			solveFragmentMainTextureLoadFailedPath = null;
			return solveFragmentMainTexture;
		}
		solveFragmentMainTextureLoadFailedPath = solveFragmentMainTexturePath;
		return null;
	}

	public virtual void ReleaseSolveFragmentMainTexture()
	{
		if ((Object)(object)solveFragmentMainTexture == (Object)null || string.IsNullOrEmpty(loadedSolveFragmentMainTexturePath))
		{
			solveFragmentMainTexture = null;
			loadedSolveFragmentMainTexturePath = null;
			return;
		}
		GameApp.Res.UnloadAsset(loadedSolveFragmentMainTexturePath, "RingPartStyleMaterial", 1);
		solveFragmentMainTexture = null;
		loadedSolveFragmentMainTexturePath = null;
		solveFragmentMainTextureLoadFailedPath = null;
	}

	public virtual float? GetSolveFragmentColorIntensityOverride()
	{
		return null;
	}

	public virtual float? GetSolveFragmentBlendToWhiteOverride()
	{
		return null;
	}

	public virtual string GetSolveFragmentAddonEffectPath()
	{
		return null;
	}

	public virtual void ApplyLevelMaterials(IReadOnlyList<IGameplayObject> objects, ISet<string> loadedMaterialPaths)
	{
		if (objects == null || objects.Count == 0)
		{
			return;
		}
		Dictionary<string, Material> materialCache = new Dictionary<string, Material>(4);
		Material val = LoadMaterialOnce(GetRingMaterialPath(), materialCache, loadedMaterialPaths);
		Material val2 = LoadMaterialOnce(GetPullMaterialPath(), materialCache, loadedMaterialPaths);
		Material val3 = LoadMaterialOnce(GetFixedMaterialPath(), materialCache, loadedMaterialPaths);
		Material val4 = LoadPaletteLockGapMaterial(materialCache, loadedMaterialPaths);
		float saturation = colorProvider?.GetSaturation() ?? 1f;
		TrySetSaturation(val, saturation);
		TrySetSaturation(val2, saturation);
		TrySetSaturation(val3, saturation);
		for (int i = 0; i < objects.Count; i++)
		{
			IGameplayObject gameplayObject = objects[i];
			if (gameplayObject == null)
			{
				continue;
			}
			IGameplayObject gameplayObject2 = gameplayObject;
			if (!(gameplayObject2 is RingObject ring))
			{
				if (!(gameplayObject2 is PullObject pull))
				{
					if (gameplayObject2 is FixedButterflyObject)
					{
						continue;
					}
					if (!(gameplayObject2 is FixedObject fixedObject))
					{
						if (gameplayObject2 is RingPaletteLockGapObject gapObject && (Object)(object)val4 != (Object)null)
						{
							ApplyPaletteLockGapMaterial(gapObject, val4);
						}
					}
					else if ((Object)(object)val3 != (Object)null)
					{
						ApplyFixedMaterial(fixedObject, val3);
					}
				}
				else if ((Object)(object)val2 != (Object)null)
				{
					ApplyPullMaterial(pull, val2);
				}
			}
			else if ((Object)(object)val != (Object)null)
			{
				ApplyRingMaterial(ring, val);
			}
		}
		Color[] array = colorProvider?.GetColors();
		if (array != null && array.Length != 0)
		{
			ApplyPaletteVertexColors(objects, array);
		}
	}

	protected void ApplyPaletteVertexColors(IReadOnlyList<IGameplayObject> objects, Color[] ringColors)
	{
		if (objects != null && objects.Count != 0 && ringColors != null && ringColors.Length != 0)
		{
			for (int i = 0; i < objects.Count; i++)
			{
				ApplyPaletteVertexColor(objects[i], ringColors);
			}
		}
	}

	protected void ApplyPaletteVertexColor(IGameplayObject gameplayObject, Color[] ringColors)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (gameplayObject == null || ringColors == null || ringColors.Length == 0)
		{
			return;
		}
		PartPaletteColor paletteColor = gameplayObject.PartData.PaletteColor;
		if (paletteColor == PartPaletteColor.None)
		{
			return;
		}
		int num = PartPalette.ToPaletteIndex(paletteColor);
		if (num < 0 || num >= ringColors.Length)
		{
			return;
		}
		Color color = ringColors[num];
		if (!(gameplayObject is RingObject ringObject))
		{
			if (gameplayObject is PullObject pullObject)
			{
				((Component)pullObject).GetComponentInChildren<ArrowRibbonController>(true)?.ApplyVertexColor(color);
			}
		}
		else
		{
			((Component)ringObject).GetComponentInChildren<RingSegmentController>(true)?.ApplyVertexColor(color);
		}
	}

	private static Material LoadMaterialOnce(string materialPath, Dictionary<string, Material> materialCache, ISet<string> loadedMaterialPaths)
	{
		if (string.IsNullOrEmpty(materialPath))
		{
			return null;
		}
		if (materialCache.TryGetValue(materialPath, out var value))
		{
			return value;
		}
		Material val = GameApp.Res.LoadAsset<Material>(materialPath, "RingPartStyleMaterial");
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		materialCache[materialPath] = val;
		loadedMaterialPaths?.Add(materialPath);
		return val;
	}

	private static void TrySetSaturation(Material material, float saturation)
	{
		if (!((Object)(object)material == (Object)null) && material.HasProperty(IdSaturation))
		{
			material.SetFloat(IdSaturation, saturation);
		}
	}

	private static void ApplyRingMaterial(RingObject ring, Material material)
	{
		RingSegmentController componentInChildren = ((Component)ring).GetComponentInChildren<RingSegmentController>(true);
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.BindSharedMaterial(material);
		}
	}

	private static void ApplyPullMaterial(PullObject pull, Material material)
	{
		ArrowRibbonController componentInChildren = ((Component)pull).GetComponentInChildren<ArrowRibbonController>(true);
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.BindSharedMaterial(material);
		}
	}

	private static void ApplyPaletteLockGapMaterial(RingPaletteLockGapObject gapObject, Material material)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		RingSegmentController component = ((Component)gapObject).GetComponent<RingSegmentController>();
		if (!((Object)(object)component == (Object)null))
		{
			component.BindSharedMaterial(material);
			component.ApplyVertexColor(RingPaletteLockGapObject.GapFillerVertexColor);
		}
	}

	private static void ApplyFixedMaterial(FixedObject fixedObject, Material material)
	{
		if (!(fixedObject is FixedButterflyObject))
		{
			QuadPlaneController componentInChildren = ((Component)fixedObject).GetComponentInChildren<QuadPlaneController>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				componentInChildren.BindSharedMaterial(material);
			}
		}
	}
}
