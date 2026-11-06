using System.Collections.Generic;

namespace RotateRings.GamePlay.Save;

internal sealed class RestoreStep1_ValidateAndIndex : IInLevelSaveStep<RotateRingRestoreContext>
{
	private const string Tag = "[RestoreStep1_ValidateAndIndex]";

	public void Execute(RotateRingRestoreContext ctx)
	{
		ctx.IsValid = false;
		ctx.SaveByPartId = null;
		ctx.LiveByPartId = null;
		RotateRingInLevelSaveData save = ctx.Save;
		if (save?.Parts == null || save.Parts.Length == 0)
		{
			return;
		}
		IReadOnlyList<IGameplayObject> readOnlyList = ctx.GameplayManager?.GetGameplayObjects();
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			return;
		}
		Dictionary<string, RotateRingPartSave> dictionary = new Dictionary<string, RotateRingPartSave>(save.Parts.Length);
		for (int i = 0; i < save.Parts.Length; i++)
		{
			RotateRingPartSave rotateRingPartSave = save.Parts[i];
			if (rotateRingPartSave == null || string.IsNullOrEmpty(rotateRingPartSave.PartId) || !dictionary.TryAdd(rotateRingPartSave.PartId, rotateRingPartSave))
			{
				return;
			}
		}
		Dictionary<string, IGameplayObject> dictionary2 = new Dictionary<string, IGameplayObject>(readOnlyList.Count);
		int num = 0;
		for (int j = 0; j < readOnlyList.Count; j++)
		{
			IGameplayObject gameplayObject = readOnlyList[j];
			if (gameplayObject != null && !string.IsNullOrEmpty(gameplayObject.PartData.PhysicPartId) && (gameplayObject is RingObject || gameplayObject is PullObject || gameplayObject is FixedObject))
			{
				num++;
				if (!dictionary2.TryAdd(gameplayObject.PartData.PhysicPartId, gameplayObject))
				{
					return;
				}
			}
		}
		if (num != dictionary.Count)
		{
			return;
		}
		foreach (string key in dictionary.Keys)
		{
			if (!dictionary2.ContainsKey(key))
			{
				return;
			}
		}
		ctx.SaveByPartId = dictionary;
		ctx.LiveByPartId = dictionary2;
		ctx.IsValid = true;
	}
}
