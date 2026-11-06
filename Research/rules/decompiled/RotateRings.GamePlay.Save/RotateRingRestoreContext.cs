using System.Collections.Generic;

namespace RotateRings.GamePlay.Save;

internal sealed class RotateRingRestoreContext
{
	public RotateRingInLevelSaveData Save;

	public GameplayManager GameplayManager;

	public bool IsValid = true;

	public Dictionary<string, RotateRingPartSave> SaveByPartId;

	public Dictionary<string, IGameplayObject> LiveByPartId;
}
