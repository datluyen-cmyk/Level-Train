using UnityEngine;

namespace RotateRings.GamePlay.PartStyle.ColorProvider;

public interface IColorProvider
{
	Color[] GetColors();

	float GetSaturation();
}
