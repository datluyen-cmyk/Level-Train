using System.Collections.Generic;
using UniLog;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RingGuideRTService : GameplayServiceBase<GameplayManager>
{
	private const string Tag = "[GamePlay][GuideRT] ";

	private const string GuideCameraObjectName = "GuideRTCamera";

	private const float MinRenderTextureScale = 0.01f;

	private const float MaxRenderTextureScale = 1f;

	private const float RenderTextureScale = 1f;

	private static readonly int GuideRenderLayer = 7;

	private Camera renderCamera;

	private IGuideRenderTextureLifecycle renderTextureLifecycle;

	private int activeLifecycleMode = -1;

	private readonly List<Transform> guideRTTargetCache = new List<Transform>();

	public static int LifecycleMode => 0;

	public RingGuideRTService(GameplayManager host)
		: base(host)
	{
	}

	public override void ClearLevel()
	{
		FlushRenderTextureLifecycle();
		DestroyRenderCamera();
		base.ClearLevel();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			if ((Object)(object)renderCamera != (Object)null)
			{
				renderCamera.targetTexture = null;
			}
			FlushRenderTextureLifecycle();
			DestroyRenderCamera();
			base.Dispose();
		}
	}

	public RenderTexture CreateGuideRenderTexture(IReadOnlyList<Transform> targetTransforms)
	{
		Camera gameplayCam = base.Host.GameplayCam;
		if ((Object)(object)gameplayCam == (Object)null)
		{
			return null;
		}
		if (targetTransforms == null || targetTransforms.Count == 0)
		{
			return null;
		}
		float num = ClampRenderTextureScale(1f);
		int num2 = Mathf.Max(1, Mathf.RoundToInt((float)gameplayCam.pixelWidth * num));
		int num3 = Mathf.Max(1, Mathf.RoundToInt((float)gameplayCam.pixelHeight * num));
		if (!EnsureRenderCamera(gameplayCam))
		{
			return null;
		}
		SyncRenderCameraFromBoard(gameplayCam);
		IGuideRenderTextureLifecycle guideRenderTextureLifecycle = GetRenderTextureLifecycle();
		if (!guideRenderTextureLifecycle.CreateRT(num2, num3, renderCamera))
		{
			Log.LogError("[GamePlay][GuideRT] ", "Failed to acquire RenderTexture size={0}x{1}", new object[2] { num2, num3 });
			return null;
		}
		RenderTexture texture = guideRenderTextureLifecycle.Texture;
		renderCamera.targetTexture = texture;
		renderCamera.Render();
		return texture;
	}

	public RenderTexture CreateAllBoardPartGuideRT()
	{
		IReadOnlyList<Transform> allGuideTargetTransforms = GetAllGuideTargetTransforms();
		if (allGuideTargetTransforms.Count == 0)
		{
			return null;
		}
		return CreateGuideRenderTexture(allGuideTargetTransforms);
	}

	public void ReleaseGuideRenderTexture()
	{
		GetRenderTextureLifecycle().Release(scheduleDelayedDestroy: true, renderCamera);
		if ((Object)(object)renderCamera != (Object)null)
		{
			renderCamera.targetTexture = null;
		}
	}

	private IReadOnlyList<Transform> GetAllGuideTargetTransforms()
	{
		guideRTTargetCache.Clear();
		IReadOnlyList<IGameplayObject> gameplayObjects = base.Host.GetGameplayObjects();
		if (gameplayObjects == null)
		{
			return guideRTTargetCache;
		}
		for (int i = 0; i < gameplayObjects.Count; i++)
		{
			IGameplayObject gameplayObject = gameplayObjects[i];
			if (gameplayObject == null || gameplayObject.IsSolved)
			{
				continue;
			}
			Component val = (Component)((gameplayObject is Component) ? gameplayObject : null);
			if (val != null)
			{
				Transform transform = val.transform;
				if ((Object)(object)transform != (Object)null)
				{
					guideRTTargetCache.Add(transform);
				}
			}
		}
		return guideRTTargetCache;
	}

	private bool EnsureRenderCamera(Camera boardCamera)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if ((Object)(object)renderCamera == (Object)null)
		{
			GameObject val = new GameObject("GuideRTCamera");
			val.transform.SetParent(((Component)boardCamera).transform, false);
			renderCamera = val.AddComponent<Camera>();
		}
		else if ((Object)(object)((Component)renderCamera).transform.parent != (Object)(object)((Component)boardCamera).transform)
		{
			((Component)renderCamera).transform.SetParent(((Component)boardCamera).transform, false);
		}
		return true;
	}

	private void SyncRenderCameraFromBoard(Camera boardCamera)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		ResetCameraTransform(((Component)renderCamera).transform);
		CopyCameraSettings(boardCamera, renderCamera);
		renderCamera.clearFlags = (CameraClearFlags)2;
		renderCamera.backgroundColor = Color.clear;
		renderCamera.cullingMask = 1 << GuideRenderLayer;
		((Behaviour)renderCamera).enabled = false;
	}

	private static void ResetCameraTransform(Transform cameraTransform)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		cameraTransform.localPosition = Vector3.zero;
		cameraTransform.localRotation = Quaternion.identity;
		cameraTransform.localScale = Vector3.one;
	}

	private void DestroyRenderCamera()
	{
		if (!((Object)(object)renderCamera == (Object)null))
		{
			renderCamera.targetTexture = null;
			Object.Destroy((Object)(object)((Component)renderCamera).gameObject);
			renderCamera = null;
		}
	}

	private static void CopyCameraSettings(Camera source, Camera target)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		target.orthographic = source.orthographic;
		target.orthographicSize = source.orthographicSize;
		target.fieldOfView = source.fieldOfView;
		target.nearClipPlane = source.nearClipPlane;
		target.farClipPlane = source.farClipPlane;
		target.rect = source.rect;
		target.allowHDR = source.allowHDR;
		target.allowMSAA = source.allowMSAA;
		target.depth = source.depth;
	}

	private IGuideRenderTextureLifecycle GetRenderTextureLifecycle()
	{
		int lifecycleMode = LifecycleMode;
		if (renderTextureLifecycle != null && activeLifecycleMode == lifecycleMode)
		{
			return renderTextureLifecycle;
		}
		renderTextureLifecycle?.Flush(renderCamera);
		renderTextureLifecycle?.Dispose();
		renderTextureLifecycle = CreateRenderTextureLifecycle(lifecycleMode);
		activeLifecycleMode = lifecycleMode;
		return renderTextureLifecycle;
	}

	private IGuideRenderTextureLifecycle CreateRenderTextureLifecycle(int mode)
	{
		if (mode != 0)
		{
			return new GuideRenderTextureOwnedDelayedLifecycle(() => base.IsDisposed);
		}
		return new GuideRenderTextureTemporaryPoolLifecycle();
	}

	private void FlushRenderTextureLifecycle()
	{
		renderTextureLifecycle?.Flush(renderCamera);
		renderTextureLifecycle?.Dispose();
		renderTextureLifecycle = null;
		activeLifecycleMode = -1;
	}

	private float ClampRenderTextureScale(float scale)
	{
		if (scale <= 0f)
		{
			return 0.01f;
		}
		return Mathf.Min(scale, 1f);
	}
}
