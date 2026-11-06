using UnityEngine;

namespace UnlockRing.Prop;

public interface IUnlockRingPropUseCallback
{
	RectTransform PropGuidePanel { get; }

	bool HasPropConsumed { get; }

	void PrepareFreePropIfNeeded();

	void RequestFinish(UnlockRingPropUseResult result, bool consumeOnSuccess);
}
