using Example.Cfg;
using RotateRings.UI.Game;
using UnityEngine;
using UnlockRing;
using XingYun.UI.Controller;

public sealed class GameCameraAdaptation
{
	private const float FallbackAspect = 2f;

	private const float DefaultCameraZ = -10f;

	private const float NearClipPlane = -100f;

	private const float FarClipPlane = 100f;

	private readonly IGameCameraSource cameraSource;

	private RectTransform gameRect;

	private int cachedGameRectWidth = -1;

	private int cachedGameRectHeight = -1;

	private int lastScreenWidth = -1;

	private int lastScreenHeight = -1;

	private bool hasBoundFromUI;

	public static float InitialSize = 15f;

	public GameCameraAdaptation(IGameCameraSource cameraSource)
	{
		this.cameraSource = cameraSource;
	}

	public void SetGameRect(RectTransform rect)
	{
		gameRect = rect;
		cachedGameRectWidth = -1;
		cachedGameRectHeight = -1;
	}

	public void Refresh()
	{
		ApplyFraming();
	}

	public void ApplyFixedSize(float orthographicSize)
	{
	}

	public void BindFromUIGame()
	{
		if (TryResolveUIGameView(out var gameView))
		{
			SetGameRect(gameView.GameRect);
			Refresh();
		}
	}

	public void UpdateIfNeeded()
	{
		if (!TryResolveUIGameView(out var _))
		{
			if ((Object)(object)gameRect != (Object)null)
			{
				SetGameRect(null);
			}
			hasBoundFromUI = false;
		}
		else if (!hasBoundFromUI)
		{
			BindFromUIGame();
			hasBoundFromUI = true;
		}
		int width = Screen.width;
		int height = Screen.height;
		if (width != lastScreenWidth || height != lastScreenHeight || TryConsumeGameRectSizeChange())
		{
			lastScreenWidth = width;
			lastScreenHeight = height;
			ApplyFraming();
		}
	}

	private static bool TryResolveUIGameView(out UIGameView gameView)
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
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Camera val = cameraSource?.Cam;
		if (!((Object)(object)val == (Object)null))
		{
			ConfigureCamera(val);
			float z = ((Component)val).transform.position.z;
			if (!(InitialSize > 0f) || !TryApplyInitialFraming(val, cameraSource.WorldBounds, z))
			{
				CalculateFraming(cameraSource.WorldBounds, val, cameraSource.CameraLevelKey, out var orthographicSize, out var position);
				val.orthographicSize = orthographicSize;
				((Component)val).transform.position = position;
			}
			GameApp.Event.Send<GameCameraAdaptationEndEvent>(new GameCameraAdaptationEndEvent
			{
				CameraPosition = ((Component)val).transform.position
			});
		}
	}

	private bool TryApplyInitialFraming(Camera camera, Rect worldBounds, float cameraZ)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetScreenRect(out var screenRect))
		{
			return false;
		}
		camera.orthographicSize = InitialSize;
		((Component)camera).transform.position = new Vector3(((Rect)(ref worldBounds)).center.x, ((Rect)(ref worldBounds)).center.y, cameraZ);
		return !IsWorldBoundsOverflow(camera, worldBounds, screenRect);
	}

	private static bool IsWorldBoundsOverflow(Camera camera, Rect worldBounds, Rect gameScreenRect)
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

	private static void ConfigureCamera(Camera camera)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		camera.orthographic = true;
		camera.clearFlags = (CameraClearFlags)2;
		camera.backgroundColor = RingTheme.Surface;
		camera.nearClipPlane = -100f;
		camera.farClipPlane = 100f;
	}

	private bool TryConsumeGameRectSizeChange()
	{
		if (!TryGetScreenRect(out var screenRect))
		{
			return false;
		}
		int num = Mathf.RoundToInt(((Rect)(ref screenRect)).width);
		int num2 = Mathf.RoundToInt(((Rect)(ref screenRect)).height);
		if (num == cachedGameRectWidth && num2 == cachedGameRectHeight)
		{
			return false;
		}
		cachedGameRectWidth = num;
		cachedGameRectHeight = num2;
		return true;
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

	private void CalculateFraming(Rect worldBounds, Camera camera, string levelKey, out float orthographicSize, out Vector3 position)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		float cameraScaleValue = GetCameraScaleValue(levelKey);
		float cameraZ = (((Object)(object)camera != (Object)null) ? ((Component)camera).transform.position.z : (-10f));
		if (TryGetScreenRect(out var screenRect) && Screen.height > 0 && ((Rect)(ref screenRect)).width > 0f && ((Rect)(ref screenRect)).height > 0f && ((Rect)(ref worldBounds)).width > 0f && ((Rect)(ref worldBounds)).height > 0f)
		{
			BuildGameRectFraming(worldBounds, screenRect, cameraScaleValue, cameraZ, out orthographicSize, out position);
		}
		else
		{
			BuildFullscreenFraming(worldBounds, cameraScaleValue, cameraZ, out orthographicSize, out position);
		}
	}

	private static void BuildGameRectFraming(Rect worldBounds, Rect gameScreenRect, float scaleValue, float cameraZ, out float orthographicSize, out Vector3 position)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Rect)(ref worldBounds)).width * (float)Screen.height / (2f * ((Rect)(ref gameScreenRect)).width);
		float num2 = ((Rect)(ref worldBounds)).height * (float)Screen.height / (2f * ((Rect)(ref gameScreenRect)).height);
		orthographicSize = Mathf.Max(num, num2) * scaleValue;
		float num3 = (float)Screen.height * 0.5f;
		float num4 = (((Rect)(ref gameScreenRect)).yMin + ((Rect)(ref gameScreenRect)).height * 0.5f - num3) * (2f * orthographicSize) / (float)Screen.height;
		position = new Vector3(((Rect)(ref worldBounds)).center.x, ((Rect)(ref worldBounds)).center.y - num4, cameraZ);
	}

	private static void BuildFullscreenFraming(Rect worldBounds, float scaleValue, float cameraZ, out float orthographicSize, out Vector3 position)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Screen.width > 0 && Screen.height > 0) ? ((float)Screen.width / (float)Screen.height) : 2f);
		float num2 = ((Rect)(ref worldBounds)).width * 0.5f / num * scaleValue;
		float num3 = ((Rect)(ref worldBounds)).height * 0.5f * scaleValue;
		orthographicSize = Mathf.Max(num2, num3);
		position = new Vector3(((Rect)(ref worldBounds)).center.x, ((Rect)(ref worldBounds)).center.y, cameraZ);
	}

	private static float GetCameraScaleValue(string levelKey)
	{
		if (string.IsNullOrEmpty(levelKey))
		{
			return 1f;
		}
		return GameApp.Config.GetTable<ExampleTables>().TbCameraConfig.Get(levelKey)?.ScaleValue ?? 1f;
	}
}
