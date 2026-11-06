using UnityEngine;
using UnlockRing.Core.Domain;

namespace UnlockRing;

public interface IBoardGestureOverride
{
	bool SuppressBlockedFeedback { get; }

	bool TryOverrideBegin(CoreBoardRenderer board, Vector2 screen, out BoardGestureOverrideResult result);

	bool TryOverrideUpdate(CoreBoardRenderer board, Vector2 screen, Vector2 world, PartId? activePart, out BoardGestureOverrideResult result);

	bool TryOverrideEnd(CoreBoardRenderer board, Vector2 screen, Vector2 world, PartId? activePart, out BoardGestureOverrideResult result);
}
