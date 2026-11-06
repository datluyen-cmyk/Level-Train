using Example.Cfg;
using RotateRings.GamePlay.Events;
using RotateRings.UI.Game;
using UnityEngine;
using XingYun.UI.Controller;

namespace RotateRings.GamePlay.Services;

public sealed class RotateCameraAdaptation : GameplayServiceBase<GameplayManager>
{
	private const float FallbackAspect = 2f;

	private const float NearClipPlane = -100f;

	private const float FarClipPlane = 100f;

	private RectTransform gameRect;

	public static float InitialSize = 15f;

	public float CachedOrthographicSize { get; private set; }

	public Vector3 CachedCameraPosition { get; private set; }

	public RotateCameraAdaptation(GameplayManager host)
		: base(host)
	{
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
		}
	}

	public void ApplyFixedSize(float orthographicSize)
	{
	}

	public void BindFromUIGame()
	{
		if (TryResolveUIGameView(out var gameView))
		{
			gameRect = gameView.GameRect;
			ApplyFraming();
		}
	}

	public bool RestoreInitialFraming()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Camera gameplayCam = base.Host.GameplayCam;
		if ((Object)(object)gameplayCam == (Object)null || CachedOrthographicSize <= 0f)
		{
			return false;
		}
		gameplayCam.orthographicSize = CachedOrthographicSize;
		((Component)gameplayCam).transform.position = CachedCameraPosition;
		GameApp.Event.Send<GameCameraAdaptationEndEvent>(new GameCameraAdaptationEndEvent
		{
			CameraPosition = ((Component)gameplayCam).transform.position,
			Source = GameCameraFramingSource.Layout
		});
		return true;
	}

	private bool TryResolveUIGameView(out UIGameView gameView)
	{
		gameView = null;
		IController val = default(IController);
		if (!GameApp.UI.IsOpen("UIGame", ref val) || !(val is UIGameController uIGameController))
		{
			return false;
		}
		gameView = uIGameController.View;
		if (gameView != null)
		{
			return (Object)(object)gameView.GameRect != (Object)null;
		}
		return false;
	}

	private void ApplyFraming()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		Camera gameplayCam = base.Host.GameplayCam;
		if ((Object)(object)gameplayCam == (Object)null)
		{
			return;
		}
		ConfigureCamera(gameplayCam);
		float z = ((Component)gameplayCam).transform.position.z;
		float cameraScaleValue = GetCameraScaleValue(base.Host.LevelCameraKey);
		if (TryGetScreenRect(out var screenRect) && ((Rect)(ref screenRect)).width > 0f && ((Rect)(ref screenRect)).height > 0f && Screen.height > 0)
		{
			if (TryBaseSize(gameplayCam, 1f))
			{
				if (!TryBaseSize(gameplayCam, cameraScaleValue))
				{
					TryAutoAdapt(gameplayCam, 1f);
				}
			}
			else if (!TryAutoAdapt(gameplayCam, cameraScaleValue))
			{
				TryAutoAdapt(gameplayCam, 1f);
			}
		}
		else
		{
			BuildFullscreenFraming(base.Host.WorldBounds, cameraScaleValue, z, out var orthographicSize, out var position);
			gameplayCam.orthographicSize = orthographicSize;
			((Component)gameplayCam).transform.position = position;
		}
		CachedOrthographicSize = gameplayCam.orthographicSize;
		CachedCameraPosition = ((Component)gameplayCam).transform.position;
		GameApp.Event.Send<GameCameraAdaptationEndEvent>(new GameCameraAdaptationEndEvent
		{
			CameraPosition = ((Component)gameplayCam).transform.position,
			Source = GameCameraFramingSource.Layout
		});
	}

	private bool TryBaseSize(Camera camera, float levelScale)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetScreenRect(out var screenRect) || Screen.height <= 0)
		{
			return false;
		}
		float z = ((Component)camera).transform.position.z;
		float num = ComputeGameRectCenterOffsetY(orthographicSize: camera.orthographicSize = InitialSize / levelScale, gameScreenRect: screenRect);
		Transform transform = ((Component)camera).transform;
		Rect worldBounds = base.Host.WorldBounds;
		float x = ((Rect)(ref worldBounds)).center.x;
		worldBounds = base.Host.WorldBounds;
		transform.position = new Vector3(x, ((Rect)(ref worldBounds)).center.y - num, z);
		return !IsWorldBoundsOverflow(camera, base.Host.WorldBounds, screenRect);
	}

	private bool TryAutoAdapt(Camera camera, float scale)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetScreenRect(out var screenRect) || Screen.height <= 0)
		{
			return false;
		}
		float z = ((Component)camera).transform.position.z;
		Rect worldBounds = base.Host.WorldBounds;
		float num = ((Rect)(ref worldBounds)).width * (float)Screen.height / (2f * ((Rect)(ref screenRect)).width);
		worldBounds = base.Host.WorldBounds;
		float num2 = ((Rect)(ref worldBounds)).height * (float)Screen.height / (2f * ((Rect)(ref screenRect)).height);
		float num3 = ComputeGameRectCenterOffsetY(orthographicSize: camera.orthographicSize = Mathf.Max(num, num2) / scale, gameScreenRect: screenRect);
		Transform transform = ((Component)camera).transform;
		worldBounds = base.Host.WorldBounds;
		float x = ((Rect)(ref worldBounds)).center.x;
		worldBounds = base.Host.WorldBounds;
		transform.position = new Vector3(x, ((Rect)(ref worldBounds)).center.y - num3, z);
		return !IsWorldBoundsOverflow(camera, base.Host.WorldBounds, screenRect);
	}

	private float ComputeGameRectCenterOffsetY(Rect gameScreenRect, float orthographicSize)
	{
		float num = (float)Screen.height * 0.5f;
		return (((Rect)(ref gameScreenRect)).yMin + ((Rect)(ref gameScreenRect)).height * 0.5f - num) * (2f * orthographicSize) / (float)Screen.height;
	}

	private bool IsWorldBoundsOverflow(Camera camera, Rect worldBounds, Rect gameScreenRect)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(camera.WorldToScreenPoint(new Vector3(((Rect)(ref worldBounds)).xMin, ((Rect)(ref worldBounds)).yMin, 0f)));
		Vector2 val2 = Vector2.op_Implicit(camera.WorldToScreenPoint(new Vector3(((Rect)(ref worldBounds)).xMax, ((Rect)(ref worldBounds)).yMax, 0f)));
		if (!(val.x < ((Rect)(ref gameScreenRect)).xMin) && !(val2.x > ((Rect)(ref gameScreenRect)).xMax) && !(val.y < ((Rect)(ref gameScreenRect)).yMin))
		{
			return val2.y > ((Rect)(ref gameScreenRect)).yMax;
		}
		return true;
	}

	private void ConfigureCamera(Camera camera)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		camera.orthographic = true;
		camera.clearFlags = (CameraClearFlags)2;
		camera.backgroundColor = RingTheme.Surface;
		camera.nearClipPlane = -100f;
		camera.farClipPlane = 100f;
	}

	private bool TryGetScreenRect(out Rect screenRect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		screenRect = default(Rect);
		if ((Object)(object)gameRect == (Object)null)
		{
			return false;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		gameRect.GetWorldCorners(array);
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		Canvas componentInParent = ((Component)gameRect).GetComponentInParent<Canvas>();
		Camera val = (((Object)(object)componentInParent != (Object)null && (int)componentInParent.renderMode != 0) ? componentInParent.rootCanvas.worldCamera : null);
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 val2 = RectTransformUtility.WorldToScreenPoint(val, array[i]);
			num = Mathf.Min(num, val2.x);
			num2 = Mathf.Min(num2, val2.y);
			num3 = Mathf.Max(num3, val2.x);
			num4 = Mathf.Max(num4, val2.y);
		}
		if (num3 <= num || num4 <= num2)
		{
			return false;
		}
		screenRect = Rect.MinMaxRect(num, num2, num3, num4);
		return true;
	}

	private void BuildFullscreenFraming(Rect worldBounds, float scaleValue, float cameraZ, out float orthographicSize, out Vector3 position)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Screen.width > 0 && Screen.height > 0) ? ((float)Screen.width / (float)Screen.height) : 2f);
		float num2 = ((Rect)(ref worldBounds)).width * 0.5f / num / scaleValue;
		float num3 = ((Rect)(ref worldBounds)).height * 0.5f / scaleValue;
		orthographicSize = Mathf.Max(num2, num3);
		position = new Vector3(((Rect)(ref worldBounds)).center.x, ((Rect)(ref worldBounds)).center.y, cameraZ);
	}

	private float GetCameraScaleValue(string levelKey)
	{
		if (string.IsNullOrEmpty(levelKey))
		{
			return 1f;
		}
		return GameApp.Config.GetTable<ExampleTables>().TbCameraConfig.Get(levelKey)?.ScaleValue ?? 1f;
	}
}
