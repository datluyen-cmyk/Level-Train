using UnityEngine;

namespace UnlockRing;

public interface IGameCameraSource
{
	Camera Cam { get; }

	string CameraLevelKey { get; }

	Rect WorldBounds { get; }
}
