using UnityEngine;

namespace RotateRings.GamePlay.Services;

public interface IGuideRenderTextureLifecycle
{
	RenderTexture Texture { get; }

	bool CreateRT(int width, int height, Camera renderCamera);

	void Release(bool scheduleDelayedDestroy, Camera renderCamera);

	void Flush(Camera renderCamera);

	void Dispose();
}
