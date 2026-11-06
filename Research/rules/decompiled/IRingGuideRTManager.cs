using System.Collections.Generic;
using UnityEngine;
using UnlockRing.Core.Domain;

public interface IRingGuideRTManager
{
	int GuideRenderLayer { get; }

	Camera ResolveBoardCamera();

	IReadOnlyList<Transform> GetAllGuideTargetTransforms();

	bool TryGetPartGuideTarget(PartId partId, out Transform target);
}
