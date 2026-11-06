using System;
using RotateRings.GamePlay.Events;
using RotateRings.GamePlay.Services;
using RotateRings.Theme;
using UnityEngine;

namespace RotateRings.Halloween;

public sealed class HalloweenGameBackgroundService : GameplayServiceBase<GameplayManager>
{
	private const string Tag = "[HalloweenGameBackground] ";

	private const string BackgroundLocation = "HalloweenGameBG";

	private const string BottomSpriteName = "SpriteBottom";

	private const string BrightBottomSpriteLocation = "home_bg_hallowmas_bright";

	private const string DarkBottomSpriteLocation = "home_bg_hallowmas_dark";

	private const float ScreenDesignHeight = 2160f;

	private const float BottomSpriteDesignWidth = 1620f;

	private const float BottomSpriteDesignHeight = 354f;

	private const float BottomSpriteBottomOffsetPixels = 116f;

	private GameObject backgroundInstance;

	private SpriteRenderer bottomSpriteRenderer;

	private Sprite brightBottomSprite;

	private Sprite darkBottomSprite;

	public HalloweenGameBackgroundService(GameplayManager host)
		: base(host)
	{
		GameApp.Event.Listen<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
		GameApp.Event.Listen<LevelLoadingStartedEvent>((Action<LevelLoadingStartedEvent>)OnLevelLoadingStarted);
		GameApp.Event.Listen<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			GameApp.Event.Unlisten<GameCameraAdaptationEndEvent>((Action<GameCameraAdaptationEndEvent>)OnCameraAdaptationEnd);
			GameApp.Event.Unlisten<LevelLoadingStartedEvent>((Action<LevelLoadingStartedEvent>)OnLevelLoadingStarted);
			GameApp.Event.Unlisten<ThemeChangedEvent>((Action<ThemeChangedEvent>)OnThemeChanged);
			DestroyBackground();
			base.Dispose();
		}
	}

	public void SyncBackgroundForLevelEntry()
	{
		if (!base.IsDisposed)
		{
			if (!HalloweenFeatureService.IsGameEnabled())
			{
				DestroyBackground();
				return;
			}
			CreateBackgroundIfEnabled();
			SyncBottomSpriteToCamera();
		}
	}

	private void OnLevelLoadingStarted(LevelLoadingStartedEvent evt)
	{
		SyncBackgroundForLevelEntry();
	}

	private void CreateBackgroundIfEnabled()
	{
		if (!HalloweenFeatureService.IsGameEnabled() || (Object)(object)backgroundInstance != (Object)null)
		{
			return;
		}
		Transform levelBackground = base.Host.LevelBackground;
		if ((Object)(object)levelBackground == (Object)null || (Object)(object)levelBackground.parent == (Object)null)
		{
			return;
		}
		GameObject val = GameApp.Res.LoadAsset<GameObject>("HalloweenGameBG", (string)null);
		if (!((Object)(object)val == (Object)null))
		{
			backgroundInstance = Object.Instantiate<GameObject>(val, levelBackground.parent, false);
			backgroundInstance.transform.SetSiblingIndex(levelBackground.GetSiblingIndex() + 1);
			bottomSpriteRenderer = FindBottomSpriteRenderer(backgroundInstance.transform);
			if ((Object)(object)bottomSpriteRenderer == (Object)null)
			{
				DestroyBackground();
				return;
			}
			ApplyBottomSpriteTheme(ThemeService.HasInitialized ? ThemeService.CurrentThemeId : ThemeId.Day);
			SyncBottomSpriteToCamera();
		}
	}

	private void ApplyBottomSpriteTheme(ThemeId themeId)
	{
		if (!((Object)(object)bottomSpriteRenderer == (Object)null))
		{
			Sprite val = ((themeId == ThemeId.Night) ? GetBottomSprite(isNight: true) : GetBottomSprite(isNight: false));
			if (!((Object)(object)val == (Object)null))
			{
				bottomSpriteRenderer.sprite = val;
				SyncBottomSpriteToCamera();
			}
		}
	}

	private Sprite GetBottomSprite(bool isNight)
	{
		Sprite val = (isNight ? darkBottomSprite : brightBottomSprite);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		string text = (isNight ? "home_bg_hallowmas_dark" : "home_bg_hallowmas_bright");
		val = GameApp.Res.LoadAsset<Sprite>(text, (string)null);
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		if (isNight)
		{
			darkBottomSprite = val;
		}
		else
		{
			brightBottomSprite = val;
		}
		return val;
	}

	private static SpriteRenderer FindBottomSpriteRenderer(Transform backgroundRoot)
	{
		if ((Object)(object)backgroundRoot == (Object)null)
		{
			return null;
		}
		Transform obj = backgroundRoot.Find("SpriteBottom");
		if (obj == null)
		{
			return null;
		}
		return ((Component)obj).GetComponent<SpriteRenderer>();
	}

	private void SyncBottomSpriteToCamera()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bottomSpriteRenderer == (Object)null)
		{
			return;
		}
		Camera val = base.Host.GameplayCam ?? GameplayCameraManager.GameplayCamera;
		if ((Object)(object)val == (Object)null || !val.orthographic || val.orthographicSize <= 0f || val.aspect <= 0f)
		{
			return;
		}
		Sprite sprite = bottomSpriteRenderer.sprite;
		if ((Object)(object)sprite == (Object)null)
		{
			return;
		}
		Rect rect = sprite.rect;
		if (!(((Rect)(ref rect)).width <= 0f))
		{
			rect = sprite.rect;
			if (!(((Rect)(ref rect)).height <= 0f))
			{
				float num = val.orthographicSize * 2f * val.aspect;
				float num2 = 2160f * val.aspect;
				float num3 = Mathf.Max(1620f, num2);
				float num4 = num / num2;
				float num5 = num3 * num4;
				float num6 = num5 * 354f / 1620f;
				float num7 = 116f * num4;
				rect = sprite.rect;
				float num8 = ((Rect)(ref rect)).width / sprite.pixelsPerUnit;
				rect = sprite.rect;
				float num9 = ((Rect)(ref rect)).height / sprite.pixelsPerUnit;
				Transform transform = ((Component)bottomSpriteRenderer).transform;
				Vector3 localScale = transform.localScale;
				localScale.x = num5 / num8;
				localScale.y = num6 / num9;
				transform.localScale = localScale;
				Vector3 position = transform.position;
				float num10 = ((Component)val).transform.position.x - num5 * 0.5f;
				float num11 = num5 * sprite.pivot.x;
				rect = sprite.rect;
				position.x = num10 + num11 / ((Rect)(ref rect)).width;
				float num12 = ((Component)val).transform.position.y - val.orthographicSize - num7;
				float num13 = num6 * sprite.pivot.y;
				rect = sprite.rect;
				position.y = num12 + num13 / ((Rect)(ref rect)).height;
				transform.position = position;
			}
		}
	}

	private void OnThemeChanged(ThemeChangedEvent evt)
	{
		if (!base.IsDisposed && evt != null)
		{
			ApplyBottomSpriteTheme(evt.CurrentThemeId);
		}
	}

	private void OnCameraAdaptationEnd(GameCameraAdaptationEndEvent evt)
	{
		if (!base.IsDisposed)
		{
			SyncBottomSpriteToCamera();
		}
	}

	private void DestroyBackground()
	{
		if ((Object)(object)backgroundInstance != (Object)null)
		{
			Object.Destroy((Object)(object)backgroundInstance);
			backgroundInstance = null;
		}
		bottomSpriteRenderer = null;
	}
}
