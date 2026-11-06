using UnityEngine;

namespace RotateRings.GamePlay.Events;

public sealed class GameCameraAdaptationEndEvent
{
	public Vector3 CameraPosition { get; set; }

	public GameCameraFramingSource Source { get; set; }
}
