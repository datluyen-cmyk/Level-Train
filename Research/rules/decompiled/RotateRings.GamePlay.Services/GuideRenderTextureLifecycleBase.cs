using UnityEngine;

namespace RotateRings.GamePlay.Services;

public abstract class GuideRenderTextureLifecycleBase : IGuideRenderTextureLifecycle
{
	public abstract RenderTexture Texture { get; }

	public abstract bool CreateRT(int width, int height, Camera renderCamera);

	public abstract void Release(bool scheduleDelayedDestroy, Camera renderCamera);

	public abstract void Flush(Camera renderCamera);

	public abstract void Dispose();

	protected static void DetachCameraTarget(Camera renderCamera, RenderTexture texture)
	{
		if ((Object)(object)renderCamera != (Object)null && (Object)(object)renderCamera.targetTexture == (Object)(object)texture)
		{
			renderCamera.targetTexture = null;
		}
	}
}
