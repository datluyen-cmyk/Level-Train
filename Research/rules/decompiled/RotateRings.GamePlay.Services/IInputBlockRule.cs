using UnityEngine;

namespace RotateRings.GamePlay.Services;

public interface IInputBlockRule
{
	bool IsBlocking(Vector2 screenPosition);
}
