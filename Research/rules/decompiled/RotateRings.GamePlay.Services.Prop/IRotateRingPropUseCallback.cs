using UnityEngine;

namespace RotateRings.GamePlay.Services.Prop;

public interface IRotateRingPropUseCallback
{
	RectTransform PropGuidePanel { get; }

	bool HasPropConsumed { get; }

	void PrepareFreePropIfNeeded();

	void RequestFinish(RotateRingPropUseResult result, bool consumeOnSuccess);
}
