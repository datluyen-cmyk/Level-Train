using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class GuideRenderTextureTemporaryPoolLifecycle : GuideRenderTextureLifecycleBase
{
	private const string GuideRenderTextureName = "GuideRT";

	private RenderTexture texture;

	private int cachedWidth;

	private int cachedHeight;

	public override RenderTexture Texture => texture;

	public override bool CreateRT(int width, int height, Camera renderCamera)
	{
		if ((Object)(object)texture != (Object)null && cachedWidth == width && cachedHeight == height)
		{
			return true;
		}
		if ((Object)(object)texture != (Object)null)
		{
			ReleaseTemporary(renderCamera, texture);
			texture = null;
			cachedWidth = 0;
			cachedHeight = 0;
		}
		texture = CreateTemporary(width, height);
		if ((Object)(object)texture == (Object)null)
		{
			return false;
		}
		cachedWidth = width;
		cachedHeight = height;
		return true;
	}

	public override void Release(bool scheduleDelayedDestroy, Camera renderCamera)
	{
		if (!((Object)(object)texture == (Object)null))
		{
			ReleaseTemporary(renderCamera, texture);
			texture = null;
			cachedWidth = 0;
			cachedHeight = 0;
		}
	}

	public override void Flush(Camera renderCamera)
	{
		Release(scheduleDelayedDestroy: false, renderCamera);
	}

	public override void Dispose()
	{
	}

	private static RenderTexture CreateTemporary(int width, int height)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 24, (RenderTextureFormat)0);
		if ((Object)(object)temporary == (Object)null)
		{
			return null;
		}
		ApplyCommonSettings(temporary);
		return temporary;
	}

	private static void ReleaseTemporary(Camera renderCamera, RenderTexture rt)
	{
		if (!((Object)(object)rt == (Object)null))
		{
			GuideRenderTextureLifecycleBase.DetachCameraTarget(renderCamera, rt);
			RenderTexture.ReleaseTemporary(rt);
		}
	}

	private static void ApplyCommonSettings(RenderTexture rt)
	{
		((Object)rt).name = "GuideRT";
		rt.useMipMap = false;
		rt.autoGenerateMips = false;
		rt.antiAliasing = 1;
	}
}
