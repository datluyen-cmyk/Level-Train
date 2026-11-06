using System;
using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Save;

internal sealed class CaptureStep2_Parts : IInLevelSaveStep<RotateRingCaptureContext>
{
	private const string Tag = "[CaptureStep2_Parts]";

	public void Execute(RotateRingCaptureContext ctx)
	{
		RotateRingInLevelSaveData save = ctx.Save;
		GameplayManager gameplayManager = ctx.GameplayManager;
		IReadOnlyList<RotateRingPartSave> readOnlyList = gameplayManager?.RingSave?.PartManifest;
		if (readOnlyList == null || readOnlyList.Count == 0)
		{
			save.HasData = false;
			save.Parts = Array.Empty<RotateRingPartSave>();
			return;
		}
		Dictionary<string, IGameplayObject> dictionary = BuildLivePartMap(gameplayManager?.GetGameplayObjects());
		List<RotateRingPartSave> list = new List<RotateRingPartSave>(readOnlyList.Count);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			RotateRingPartSave rotateRingPartSave = readOnlyList[i];
			if (rotateRingPartSave == null || string.IsNullOrEmpty(rotateRingPartSave.PartId))
			{
				continue;
			}
			if (dictionary.TryGetValue(rotateRingPartSave.PartId, out var value))
			{
				if (TryCaptureLivePart(value, rotateRingPartSave.PartKind, out var partSave))
				{
					list.Add(partSave);
				}
			}
			else
			{
				list.Add(new RotateRingPartSave
				{
					PartId = rotateRingPartSave.PartId,
					PartKind = rotateRingPartSave.PartKind,
					IsSolved = true
				});
			}
		}
		if (list.Count == 0)
		{
			save.HasData = false;
			save.Parts = Array.Empty<RotateRingPartSave>();
		}
		else
		{
			save.Parts = list.ToArray();
		}
	}

	private static Dictionary<string, IGameplayObject> BuildLivePartMap(IReadOnlyList<IGameplayObject> objects)
	{
		Dictionary<string, IGameplayObject> dictionary = new Dictionary<string, IGameplayObject>();
		if (objects == null)
		{
			return dictionary;
		}
		for (int i = 0; i < objects.Count; i++)
		{
			IGameplayObject gameplayObject = objects[i];
			MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
			if (val != null && !((Object)(object)val == (Object)null) && !string.IsNullOrEmpty(gameplayObject.PartData.PhysicPartId) && (gameplayObject is RingObject || gameplayObject is PullObject || gameplayObject is FixedObject))
			{
				dictionary[gameplayObject.PartData.PhysicPartId] = gameplayObject;
			}
		}
		return dictionary;
	}

	private static bool TryCaptureLivePart(IGameplayObject obj, RotateRingPartSave.PartKindType expectedPartKind, out RotateRingPartSave partSave)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		partSave = null;
		MonoBehaviour val = (MonoBehaviour)((obj is MonoBehaviour) ? obj : null);
		if (val == null || (Object)(object)val == (Object)null || string.IsNullOrEmpty(obj.PartData.PhysicPartId))
		{
			return false;
		}
		partSave = new RotateRingPartSave
		{
			PartId = obj.PartData.PhysicPartId,
			IsSolved = obj.IsSolved,
			PartKind = expectedPartKind,
			LockCount = obj.LockCount
		};
		if (!(obj is RingObject ringObject))
		{
			if (!(obj is PullObject pullObject))
			{
				if (obj is FixedObject && expectedPartKind == RotateRingPartSave.PartKindType.Fixed)
				{
					return true;
				}
			}
			else if (expectedPartKind == RotateRingPartSave.PartKindType.Pull)
			{
				Vector3 position = ((Component)pullObject).transform.position;
				partSave.PosX = position.x;
				partSave.PosY = position.y;
				return true;
			}
		}
		else if (expectedPartKind == RotateRingPartSave.PartKindType.Ring)
		{
			partSave.RotationZ = ((Component)ringObject).transform.localEulerAngles.z;
			return true;
		}
		partSave = null;
		return false;
	}
}
