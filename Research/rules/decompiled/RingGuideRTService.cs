using System;
using System.Collections.Generic;
using UniLog;
using UnityEngine;
using UnlockRing.Core.Domain;

public sealed class RingGuideRTService : IDisposable
{
	private const string Tag = "[UnlockRing][GuideRT] ";

	private const string GuideRenderLayerName = "GuideRT";

	private const string GuideCameraObjectName = "GuideRTCamera";

	private const float MinRenderTextureScale = 0.01f;

	private const float MaxRenderTextureScale = 1f;

	private const float RenderTextureScale = 0.6f;

	private readonly IRingGuideRTManager manager;

	private Camera renderCamera;

	private RenderTexture renderTexture;

	private int cachedRenderTextureWidth;

	private int cachedRenderTextureHeight;

	private bool disposed;

	public RingGuideRTService(IRingGuideRTManager manager)
	{
		this.manager = manager;
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			ReleaseGuideRenderTexture();
			DestroyRenderCamera();
		}
	}

	public RenderTexture CreateGuideRenderTexture(IReadOnlyList<Transform> targetTransforms)
	{
		Camera val = manager?.ResolveBoardCamera();
		if ((Object)(object)val == (Object)null)
		{
			return null;
		}
		if (targetTransforms == null || targetTransforms.Count == 0)
		{
			return null;
		}
		float num = ClampRenderTextureScale(0.6f);
		int num2 = Mathf.Max(1, Mathf.RoundToInt((float)val.pixelWidth * num));
		int num3 = Mathf.Max(1, Mathf.RoundToInt((float)val.pixelHeight * num));
		if (!EnsureRenderCamera(val))
		{
			return null;
		}
		SyncRenderCameraFromBoard(val);
		if (!AcquireRenderTexture(num2, num3))
		{
			Log.LogError("[UnlockRing][GuideRT] ", "Failed to acquire RenderTexture size={0}x{1}", new object[2] { num2, num3 });
			return null;
		}
		renderCamera.targetTexture = renderTexture;
		renderCamera.Render();
		return renderTexture;
	}

	public RenderTexture CreateAllBoardPartGuideRT()
	{
		if (manager == null)
		{
			return null;
		}
		IReadOnlyList<Transform> allGuideTargetTransforms = manager.GetAllGuideTargetTransforms();
		if (allGuideTargetTransforms.Count == 0)
		{
			return null;
		}
		return CreateGuideRenderTexture(allGuideTargetTransforms);
	}

	public RenderTexture CreatePartGuideRT(PartId partId)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (manager == null || !manager.TryGetPartGuideTarget(partId, out var target))
		{
			return null;
		}
		return CreateGuideRenderTexture((IReadOnlyList<Transform>)(object)new Transform[1] { target });
	}

	public void ReleaseGuideRenderTexture()
	{
		ReleaseRenderTexture();
		if ((Object)(object)renderCamera != (Object)null)
		{
			renderCamera.targetTexture = null;
		}
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
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		ResetCameraTransform(((Component)renderCamera).transform);
		CopyCameraSettings(boardCamera, renderCamera);
		renderCamera.clearFlags = (CameraClearFlags)2;
		renderCamera.backgroundColor = Color.clear;
		int num = manager?.GuideRenderLayer ?? 0;
		renderCamera.cullingMask = 1 << num;
		((Behaviour)renderCamera).enabled = false;
	}

	private void ResetCameraTransform(Transform cameraTransform)
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

	private float ClampRenderTextureScale(float scale)
	{
		if (scale <= 0f)
		{
			return 0.01f;
		}
		return Mathf.Min(scale, 1f);
	}

	private bool AcquireRenderTexture(int width, int height)
	{
		if ((Object)(object)renderTexture != (Object)null && cachedRenderTextureWidth == width && cachedRenderTextureHeight == height)
		{
			return true;
		}
		ReleaseRenderTexture();
		renderTexture = CreateRenderTexture(width, height);
		if ((Object)(object)renderTexture == (Object)null)
		{
			cachedRenderTextureWidth = 0;
			cachedRenderTextureHeight = 0;
			return false;
		}
		cachedRenderTextureWidth = width;
		cachedRenderTextureHeight = height;
		return true;
	}

	private void ReleaseRenderTexture()
	{
		if (!((Object)(object)renderTexture == (Object)null))
		{
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = null;
			cachedRenderTextureWidth = 0;
			cachedRenderTextureHeight = 0;
		}
	}

	private RenderTexture CreateRenderTexture(int width, int height)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 24, (RenderTextureFormat)0);
		if ((Object)(object)temporary == (Object)null)
		{
			return null;
		}
		((Object)temporary).name = "GuideRT";
		temporary.useMipMap = false;
		temporary.autoGenerateMips = false;
		temporary.antiAliasing = 1;
		return temporary;
	}
}
