using System.Collections.Generic;
using UnlockRing.Core.Domain;

public static class RingObjectsSaveDataServices
{
	private static readonly Dictionary<PartId, GameplayPartData> allObjectPartData = new Dictionary<PartId, GameplayPartData>();

	public static void Register(IGameplayObject gameplayObject)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (gameplayObject != null)
		{
			GameplayPartData partData = gameplayObject.PartData;
			if (!string.IsNullOrEmpty(partData.PhysicPartId) && partData.Id > 0 && gameplayObject.TryGetPartId(out var partId))
			{
				allObjectPartData.TryAdd(partId, partData);
			}
		}
	}

	public static bool TryGet(PartId partId, out GameplayPartData partData)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return allObjectPartData.TryGetValue(partId, out partData);
	}

	public static void Clear()
	{
		allObjectPartData.Clear();
	}
}
