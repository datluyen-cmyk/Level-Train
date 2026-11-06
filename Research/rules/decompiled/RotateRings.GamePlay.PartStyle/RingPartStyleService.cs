using System;
using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.PartStyle.ColorProvider;
using RotateRings.GamePlay.Services;
using RotateRings.Model;
using RotateRings.Services;
using UnityEngine;
using XingYun.Core.Singletons;
using XingYun.Event;
using XingYun.Experiment;

namespace RotateRings.GamePlay.PartStyle;

public sealed class RingPartStyleService : GameplayServiceBase<GameplayManager>
{
	private const string MultiMaterialAudioVariantId = "variant_7fba5b48";

	private const string WoodMarbleAudioVariantId = "variant_72ecdd0b";

	private const string GoldAudioVariantId = "variant_6a49cac9";

	private readonly Dictionary<PartStyleType, IPartStyleHandler> handlersByStyle = new Dictionary<PartStyleType, IPartStyleHandler>();

	private readonly HashSet<string> loadedMaterialPaths = new HashSet<string>();

	private readonly PartStyleJellyHandler jellyHandler = new PartStyleJellyHandler();

	private IPartStyleHandler fallbackHandler;

	private IPartStyleHandler currentHandler;

	private PartStyleType currentLevelStyle;

	private IColorProvider currentColorProvider;

	private Material cachedPaletteLockGapMaterial;

	public PartStyleType CurrentLevelStyle => currentLevelStyle;

	public RingPartStyleService(GameplayManager host)
		: base(host)
	{
		fallbackHandler = new PartStyleMatteFacetedHandler();
		RegisterHandler(fallbackHandler);
		RegisterHandler(jellyHandler);
		RegisterHandler(new PartStyleGlossyMetalHandler());
		RegisterHandler(new PartStyleMatteSmoothHandler());
		RegisterHandler(new PartStyleWoodHandler());
		RegisterHandler(new PartStyleMarbleHandler());
		RegisterHandler(new PartStyleGoldHandler());
		RegisterHandler(new PartStyleJadeHandler());
		currentHandler = fallbackHandler;
		Singleton<EventManager>.Instance.Listen<GameplayGestureEvent>((Action<GameplayGestureEvent>)OnGameplayGestureEvent);
		GameApp.Event.Listen<RingDragCollisionStopEvent>((Action<RingDragCollisionStopEvent>)OnDragCollisionStop);
	}

	public string GetTrackRingMaterialName()
	{
		return GetCurrentHandler()?.GetTrackRingMaterialName();
	}

	public Material GetPaletteLockGapMaterial()
	{
		if ((Object)(object)cachedPaletteLockGapMaterial != (Object)null)
		{
			return cachedPaletteLockGapMaterial;
		}
		if (GetCurrentHandler() is PartStyleHandlerBase partStyleHandlerBase)
		{
			Dictionary<string, Material> materialCache = new Dictionary<string, Material>(1);
			cachedPaletteLockGapMaterial = partStyleHandlerBase.LoadPaletteLockGapMaterial(materialCache, loadedMaterialPaths);
		}
		return cachedPaletteLockGapMaterial;
	}

	public void InitializeLevelStyle(in PartStyleResolveContext context)
	{
		if (context.GameMode == GameMode.Halloween)
		{
			ApplyHalloweenPrefabDefaultStyle();
		}
		else
		{
			InitializeLevelStyle(ResolveLevelPartStyleType(in context));
		}
	}

	private void ApplyHalloweenPrefabDefaultStyle()
	{
		SyncAudioExperiment(1);
		currentLevelStyle = PartStyleType.MatteFaceted;
		currentHandler = ResolveHandler(PartStyleType.MatteFaceted);
		currentColorProvider = null;
		if (currentHandler is PartStyleHandlerBase partStyleHandlerBase)
		{
			partStyleHandlerBase.SetColorProvider(null);
		}
		ReleaseLoadedMaterials();
		ReleaseLoadedSolveFragmentTextures();
		base.Host.OperateHighlightService?.ClearHighlightCache();
	}

	private void InitializeLevelStyle(PartStyleType style)
	{
		SyncAudioExperiment(GameBordPartStyleResolver.ReadGameBordStyleExp());
		currentLevelStyle = style;
		currentHandler = ResolveHandler(style);
		currentColorProvider = ResolveColorProvider();
		if (currentHandler is PartStyleHandlerBase partStyleHandlerBase)
		{
			partStyleHandlerBase.SetColorProvider(currentColorProvider);
		}
		ReleaseLoadedMaterials();
		ReleaseLoadedSolveFragmentTextures();
		IReadOnlyList<IGameplayObject> readOnlyList = base.Host.ObjectsService?.GameplayObjects;
		if (readOnlyList != null && readOnlyList.Count > 0)
		{
			currentHandler?.ApplyLevelMaterials(readOnlyList, loadedMaterialPaths);
		}
		base.Host.OperateHighlightService?.ClearHighlightCache();
	}

	public override void ClearLevel()
	{
		ReleaseLoadedMaterials();
		ReleaseLoadedSolveFragmentTextures();
		jellyHandler.ClearJellyEffects();
	}

	public override void Dispose()
	{
		Singleton<EventManager>.Instance.Unlisten<GameplayGestureEvent>((Action<GameplayGestureEvent>)OnGameplayGestureEvent);
		GameApp.Event.Unlisten<RingDragCollisionStopEvent>((Action<RingDragCollisionStopEvent>)OnDragCollisionStop);
		ReleaseLoadedMaterials();
		ReleaseLoadedSolveFragmentTextures();
		jellyHandler.ClearJellyEffects();
		handlersByStyle.Clear();
		fallbackHandler = null;
		currentHandler = null;
		base.Dispose();
	}

	private static PartStyleType ResolveLevelPartStyleType(in PartStyleResolveContext context)
	{
		return GameBordPartStyleResolver.Resolve(in context);
	}

	private static void SyncAudioExperiment(int styleExp)
	{
		if (styleExp > 1)
		{
			AudioService.ActivateExperiment("variant_7fba5b48");
			AudioService.ActivateExperiment("variant_72ecdd0b");
			AudioService.ActivateExperiment("variant_6a49cac9");
		}
		else
		{
			AudioService.DeactivateExperiment("variant_7fba5b48");
			AudioService.DeactivateExperiment("variant_72ecdd0b");
			AudioService.DeactivateExperiment("variant_6a49cac9");
		}
	}

	private IColorProvider ResolveColorProvider()
	{
		int num = 1000;
		GameBord gameBord = FunctionDataCenter.Get<GameBord>();
		if (gameBord != null && gameBord.RingColorSchema > 0)
		{
			num = gameBord.RingColorSchema;
		}
		IColorProvider colorProvider = RingColorProviderBuilder.Build(num, currentLevelStyle);
		if (colorProvider == null && num != 1000)
		{
			colorProvider = RingColorProviderBuilder.Build(1000, currentLevelStyle);
		}
		return colorProvider;
	}

	public void RegisterHandler(IPartStyleHandler handler)
	{
		if (handler != null)
		{
			handlersByStyle[handler.Style] = handler;
		}
	}

	public void UnregisterHandler(IPartStyleHandler handler)
	{
		if (handler != null && handlersByStyle.TryGetValue(handler.Style, out var value) && value == handler)
		{
			handlersByStyle.Remove(handler.Style);
		}
	}

	public T GetHandler<T>() where T : class, IPartStyleHandler
	{
		foreach (KeyValuePair<PartStyleType, IPartStyleHandler> item in handlersByStyle)
		{
			if (item.Value is T result)
			{
				return result;
			}
		}
		return null;
	}

	private IPartStyleHandler GetCurrentHandler()
	{
		return currentHandler ?? fallbackHandler;
	}

	private IPartStyleHandler ResolveHandler(PartStyleType style)
	{
		return handlersByStyle.GetValueOrDefault(style, fallbackHandler);
	}

	private void ReleaseLoadedMaterials()
	{
		cachedPaletteLockGapMaterial = null;
		if (loadedMaterialPaths.Count == 0)
		{
			return;
		}
		foreach (string loadedMaterialPath in loadedMaterialPaths)
		{
			GameApp.Res.UnloadAsset(loadedMaterialPath, "RingPartStyleMaterial", 1);
		}
		loadedMaterialPaths.Clear();
	}

	private void ReleaseLoadedSolveFragmentTextures()
	{
		foreach (IPartStyleHandler value in handlersByStyle.Values)
		{
			value.ReleaseSolveFragmentMainTexture();
		}
	}

	private void OnGameplayGestureEvent(GameplayGestureEvent evt)
	{
		IGameplayObject gameplayObject = evt?.GameplayObject;
		if (gameplayObject is RingObject || gameplayObject is PullObject)
		{
			GetCurrentHandler()?.OnPartDragBegin(gameplayObject);
		}
	}

	private void OnDragCollisionStop(RingDragCollisionStopEvent evt)
	{
		if (evt != null)
		{
			GetCurrentHandler()?.OnDragCollisionStop(evt);
		}
	}

	public void PlaySolveBreakSound()
	{
		GetCurrentHandler()?.PlaySolveBreakSound();
	}

	public Texture GetSolveFragmentMainTexture()
	{
		return GetCurrentHandler()?.GetSolveFragmentMainTexture();
	}

	public float? GetSolveFragmentColorIntensityOverride()
	{
		return GetCurrentHandler()?.GetSolveFragmentColorIntensityOverride();
	}

	public float? GetSolveFragmentBlendToWhiteOverride()
	{
		return GetCurrentHandler()?.GetSolveFragmentBlendToWhiteOverride();
	}

	public string GetSolveFragmentAddonEffectPath()
	{
		return GetCurrentHandler()?.GetSolveFragmentAddonEffectPath();
	}
}
