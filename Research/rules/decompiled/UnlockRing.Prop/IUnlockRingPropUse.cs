using RotateRings.Prop;
using UnityEngine;
using UnlockRing.Core.Domain;

namespace UnlockRing.Prop;

public interface IUnlockRingPropUse
{
	EPropType PropType { get; }

	bool CanStart(IPropGameManager host);

	void Begin(IPropGameManager host, IUnlockRingPropUseCallback callback);

	void OnPropGuideOpened();

	void OnGuideReady();

	PropPointerResult OnPointer(PropPointerPhase phase, in PropPointerSnapshot snap);

	void OnGuideDismissed(bool cancelled);

	PartId? ResolvePointerHit(CoreBoardRenderer board, Vector2 world);

	void End();
}
