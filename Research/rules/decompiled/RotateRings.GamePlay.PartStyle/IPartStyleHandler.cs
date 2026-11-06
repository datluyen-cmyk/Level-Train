using System.Collections.Generic;
using RotateRings.GamePlay.Events;
using UnityEngine;

namespace RotateRings.GamePlay.PartStyle;

public interface IPartStyleHandler
{
	PartStyleType Style { get; }

	string GetTrackRingMaterialName();

	void PlaySolveBreakSound();

	Texture GetSolveFragmentMainTexture();

	void ReleaseSolveFragmentMainTexture();

	float? GetSolveFragmentColorIntensityOverride();

	float? GetSolveFragmentBlendToWhiteOverride();

	string GetSolveFragmentAddonEffectPath();

	void ApplyLevelMaterials(IReadOnlyList<IGameplayObject> objects, ISet<string> loadedMaterialPaths);

	void OnPartDragBegin(IGameplayObject gameplayObject);

	void OnDragCollisionStop(RingDragCollisionStopEvent evt);
}
