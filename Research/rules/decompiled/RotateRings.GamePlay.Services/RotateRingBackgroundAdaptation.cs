using System;
using RotateRings.GamePlay.Events;
using RotateRings.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace RotateRings.GamePlay.Services;

public sealed class RotateRingBackgroundAdaptation : GameplayServiceBase<GameplayManager>
{
	private const string TAG = "[GamePlay][BackgroundAdaptation] ";

	private const float DesignWidth = 1080f;

	private const float DesignHeight = 2160f;

	private const float DesignAspect = 0.5f;

	private const float BackgroundCoverOverscanScale = 1.05f;

	private const string NightBgMaterialPath = "BGNightMat";

	private const string NightBgEffectPath = "eff_background_loop_star";

	private const float NightBgEffectWorldZOffset = -1f;

	private Transform background;

	private Camera camera;

	private Vector2 lastCameraXY;

	private float lastOrthographicSize = -1f;

	private float lastCameraAspect = -1f;

	private bool hasApplied;

	private Vector2? backgroundUnitSize;

	private float framingOrthographicSize;

	private Renderer backgroundRenderer;

	private Material backgroundOriginalMaterial;

	private Material nightBgMaterial;

	private GameObject nightBgEffectInstance;

	private ThemeId currentAppliedTheme;

	public RotateRingBackgroundAdaptation(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
		GameApp.Event.Listen<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
	}

	public void InitLevel(Transform background, Camera camera, Material maskMaterial)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		RestoreBackgroundMaterial();
		this.background = background;
		this.camera = camera;
		lastCameraXY = default(Vector2);
		lastOrthographicSize = -1f;
		lastCameraAspect = -1f;
		hasApplied = false;
		backgroundUnitSize = null;
		framingOrthographicSize = 0f;
		backgroundRenderer = (((Object)(object)background != (Object)null) ? ((Component)background).GetComponent<Renderer>() : null);
		backgroundOriginalMaterial = (((Object)(object)backgroundRenderer != (Object)null) ? backgroundRenderer.sharedMaterial : null);
		if ((Object)(object)this.background != (Object)null)
		{
			Vector3 localPosition = this.background.localPosition;
			localPosition.z = 2f;
			this.background.localPosition = localPosition;
			CacheBackgroundUnitSize();
		}
		ApplyThemeMaterials(ThemeService.HasInitialized ? ThemeService.CurrentThemeId : ThemeId.Day, force: true);
		TrySync();
	}

	public void RefreshThemeMaterial()
	{
		if (!base.IsDisposed)
		{
			if ((Object)(object)backgroundRenderer == (Object)null)
			{
				background = base.Host.LevelBackground;
				backgroundRenderer = (((Object)(object)background != (Object)null) ? ((Component)background).GetComponent<Renderer>() : null);
				backgroundOriginalMaterial = (((Object)(object)backgroundRenderer != (Object)null) ? backgroundRenderer.sharedMaterial : null);
			}
			ApplyThemeMaterials(ThemeService.CurrentThemeId, force: true);
		}
	}

	public override void ClearLevel()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		framingOrthographicSize = 0f;
		lastCameraXY = default(Vector2);
		lastOrthographicSize = -1f;
		lastCameraAspect = -1f;
		hasApplied = false;
		base.ClearLevel();
	}

	public void BindUiRectMask(RectMask2D mask, bool flipY = false)
	{
	}

	public void ClearUiRectMask()
	{
	}

	public void RefreshBackgroundMaskRectShaderParams()
	{
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			ClearUiRectMask();
			RestoreBackgroundMaterial();
			GameApp.Event.Unlisten<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
			GameApp.Event.Unlisten<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
			base.Dispose();
		}
	}

	private void OnThemeChanged(ThemeChangedEvent evt)
	{
		if (!base.IsDisposed)
		{
			ApplyThemeMaterials(evt.CurrentThemeId, force: false);
		}
	}

	private void ApplyThemeMaterials(ThemeId themeId, bool force)
	{
		if (!force && currentAppliedTheme == themeId)
		{
			return;
		}
		currentAppliedTheme = themeId;
		if (themeId == ThemeId.Night)
		{
			EnsureNightMaterialsLoaded();
		}
		if ((Object)(object)backgroundRenderer != (Object)null)
		{
			Material val = ((themeId == ThemeId.Night && (Object)(object)nightBgMaterial != (Object)null) ? nightBgMaterial : backgroundOriginalMaterial);
			if ((Object)(object)val != (Object)null)
			{
				backgroundRenderer.sharedMaterial = val;
			}
		}
		if (themeId == ThemeId.Night)
		{
			EnsureNightBgEffect();
		}
		else
		{
			ClearNightBgEffect();
		}
	}

	private void EnsureNightMaterialsLoaded()
	{
		if ((Object)(object)nightBgMaterial == (Object)null)
		{
			nightBgMaterial = GameApp.Res.LoadAsset<Material>("BGNightMat", (string)null);
			_ = (Object)(object)nightBgMaterial == (Object)null;
		}
	}

	private void RestoreBackgroundMaterial()
	{
		ClearNightBgEffect();
		if ((Object)(object)backgroundRenderer != (Object)null && (Object)(object)backgroundOriginalMaterial != (Object)null)
		{
			backgroundRenderer.sharedMaterial = backgroundOriginalMaterial;
		}
		backgroundRenderer = null;
		backgroundOriginalMaterial = null;
		currentAppliedTheme = ThemeId.Day;
	}

	private void EnsureNightBgEffect()
	{
		if ((Object)(object)nightBgEffectInstance != (Object)null || (Object)(object)background == (Object)null)
		{
			return;
		}
		GameObject val = GameApp.Res.LoadAsset<GameObject>("eff_background_loop_star", (string)null);
		if (!((Object)(object)val == (Object)null))
		{
			Transform parent = background.parent;
			if (!((Object)(object)parent == (Object)null))
			{
				nightBgEffectInstance = Object.Instantiate<GameObject>(val, parent, false);
				SyncNightBgEffectTransform();
			}
		}
	}

	private void SyncNightBgEffectTransform()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)nightBgEffectInstance == (Object)null) && !((Object)(object)background == (Object)null))
		{
			Vector3 position = background.position;
			nightBgEffectInstance.transform.position = new Vector3(position.x, position.y, position.z + -1f);
		}
	}

	private void ClearNightBgEffect()
	{
		if (!((Object)(object)nightBgEffectInstance == (Object)null))
		{
			Object.Destroy((Object)(object)nightBgEffectInstance);
			nightBgEffectInstance = null;
		}
	}

	private void RefreshScreenFixed()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)background == (Object)null) && !((Object)(object)camera == (Object)null))
		{
			ApplyBackgroundScaleScreenFixed();
			Vector3 position = ((Component)camera).transform.position;
			ApplyBackgroundXY(position);
			lastCameraXY = new Vector2(position.x, position.y);
			lastOrthographicSize = camera.orthographicSize;
			lastCameraAspect = camera.aspect;
			hasApplied = true;
		}
	}

	private void CaptureFramingBaselineFromLayout()
	{
		RotateCameraAdaptation gameService = base.Host.GetGameService<RotateCameraAdaptation>();
		if (gameService != null && gameService.CachedOrthographicSize > 0f)
		{
			framingOrthographicSize = gameService.CachedOrthographicSize;
		}
		else if ((Object)(object)camera != (Object)null && camera.orthographicSize > 0f)
		{
			framingOrthographicSize = camera.orthographicSize;
		}
	}

	private static bool TryComputeCoverWorldSize(float orthographicSize, float aspect, out float bgWidth, out float bgHeight)
	{
		bgWidth = 0f;
		bgHeight = 0f;
		if (orthographicSize <= 0f || aspect <= 0f)
		{
			return false;
		}
		float num = 2f * orthographicSize;
		float num2 = num * aspect;
		if (aspect > 0.5f)
		{
			bgWidth = num2;
			bgHeight = bgWidth / 0.5f;
		}
		else
		{
			bgHeight = num;
			bgWidth = bgHeight * 0.5f;
		}
		return true;
	}

	private void ApplyBackgroundScaleScreenFixed()
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)background == (Object)null || (Object)(object)camera == (Object)null)
		{
			return;
		}
		float orthographicSize = camera.orthographicSize;
		if (orthographicSize <= 0f)
		{
			return;
		}
		float num = ((framingOrthographicSize > 0f) ? framingOrthographicSize : orthographicSize);
		float aspect = camera.aspect;
		if (TryComputeCoverWorldSize(num, aspect, out var bgWidth, out var bgHeight))
		{
			if (num > 0.0001f)
			{
				float num2 = orthographicSize / num;
				bgWidth *= num2;
				bgHeight *= num2;
			}
			bgWidth *= 1.05f;
			bgHeight *= 1.05f;
			if (!backgroundUnitSize.HasValue)
			{
				CacheBackgroundUnitSize();
			}
			Vector2 unitSize = (Vector2)(((_003F?)backgroundUnitSize) ?? Vector2.one);
			ApplyWorldSizeToTransform(background, unitSize, bgWidth, bgHeight);
		}
	}

	private void CacheBackgroundUnitSize()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		backgroundUnitSize = null;
		if (!((Object)(object)background == (Object)null) && TryResolveUnitSize(background, out var unitSize))
		{
			backgroundUnitSize = unitSize;
		}
	}

	private bool TryResolveUnitSize(Transform target, out Vector2 unitSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		unitSize = Vector2.one;
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		MeshFilter component = ((Component)target).GetComponent<MeshFilter>();
		if ((Object)(object)component != (Object)null && (Object)(object)component.sharedMesh != (Object)null)
		{
			Bounds bounds = component.sharedMesh.bounds;
			Vector3 size = ((Bounds)(ref bounds)).size;
			unitSize = new Vector2(size.x, size.y);
			if (unitSize.x > 0.0001f)
			{
				return unitSize.y > 0.0001f;
			}
			return false;
		}
		SpriteRenderer component2 = ((Component)target).GetComponent<SpriteRenderer>();
		if ((Object)(object)component2 == (Object)null)
		{
			return false;
		}
		if ((int)component2.drawMode != 0)
		{
			unitSize = component2.size;
			if (unitSize.x > 0.0001f)
			{
				return unitSize.y > 0.0001f;
			}
			return false;
		}
		Vector2 val = Vector2.op_Implicit(target.lossyScale);
		Bounds bounds2 = ((Renderer)component2).bounds;
		float num = ((Mathf.Abs(val.x) > 0.0001f) ? (((Bounds)(ref bounds2)).size.x / Mathf.Abs(val.x)) : ((Bounds)(ref bounds2)).size.x);
		float num2 = ((Mathf.Abs(val.y) > 0.0001f) ? (((Bounds)(ref bounds2)).size.y / Mathf.Abs(val.y)) : ((Bounds)(ref bounds2)).size.y);
		unitSize = new Vector2(num, num2);
		if (unitSize.x > 0.0001f)
		{
			return unitSize.y > 0.0001f;
		}
		return false;
	}

	private void ApplyWorldSizeToTransform(Transform target, Vector2 unitSize, float worldWidth, float worldHeight)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target == (Object)null)
		{
			return;
		}
		SpriteRenderer component = ((Component)target).GetComponent<SpriteRenderer>();
		if ((Object)(object)component != (Object)null && (int)component.drawMode != 0)
		{
			component.size = new Vector2(worldWidth, worldHeight);
			return;
		}
		Vector3 val = (((Object)(object)target.parent != (Object)null) ? target.parent.lossyScale : Vector3.one);
		float num = Mathf.Abs(val.x);
		float num2 = Mathf.Abs(val.y);
		if (num < 0.0001f)
		{
			num = 1f;
		}
		if (num2 < 0.0001f)
		{
			num2 = 1f;
		}
		Vector3 localScale = target.localScale;
		localScale.x = worldWidth / (unitSize.x * num);
		localScale.y = worldHeight / (unitSize.y * num2);
		target.localScale = localScale;
	}

	private void ApplyBackgroundXY(Vector3 cameraPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		background.position = new Vector3(cameraPosition.x, cameraPosition.y, background.position.z);
		SyncNightBgEffectTransform();
	}

	private void TrySync()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)background == (Object)null) && !((Object)(object)camera == (Object)null))
		{
			Vector3 position = ((Component)camera).transform.position;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(position.x, position.y);
			if (!hasApplied || val != lastCameraXY || !Mathf.Approximately(camera.orthographicSize, lastOrthographicSize) || !Mathf.Approximately(camera.aspect, lastCameraAspect))
			{
				RefreshScreenFixed();
			}
		}
	}

	private void OnCameraAdaptationEnd(GameCameraAdaptationEndEvent evt)
	{
		if (!base.IsDisposed)
		{
			if (evt != null && evt.Source == GameCameraFramingSource.Layout)
			{
				CaptureFramingBaselineFromLayout();
			}
			TrySync();
		}
	}
}
