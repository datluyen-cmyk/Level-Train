using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using XingYun.Account.CloudStorage;

namespace RotateRings.Services;

internal sealed class CloudSaveDataPullResolver : ICloudStorageConflictResolver
{
	public CloudSaveDataSnapshot Snapshot { get; private set; }

	public void Reset()
	{
		Snapshot = null;
	}

	public UniTask<IReadOnlyDictionary<string, ConflictAction>> ResolveAsync(IReadOnlyList<CloudStorageMergeContext> contexts, CancellationToken cancellationToken)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		cancellationToken.ThrowIfCancellationRequested();
		Dictionary<string, ConflictAction> dictionary = new Dictionary<string, ConflictAction>(contexts?.Count ?? 0);
		if (contexts == null)
		{
			return UniTask.FromResult<IReadOnlyDictionary<string, ConflictAction>>((IReadOnlyDictionary<string, ConflictAction>)dictionary);
		}
		for (int i = 0; i < contexts.Count; i++)
		{
			CloudStorageMergeContext val = contexts[i];
			if (val == null || string.IsNullOrEmpty(val.SystemType))
			{
				continue;
			}
			dictionary[val.SystemType] = (ConflictAction)3;
			if (Snapshot == null && val.CloudRawData != null && val.CloudRawData.Length != 0 && CloudSaveDataHandler.TryDeserializeSnapshot(val.CloudRawData, out var snapshot))
			{
				if (CloudSaveDataHandler.TryDeserializeSnapshot(val.LocalRawData, out var snapshot2))
				{
					snapshot.HasEverPurchased |= snapshot2.HasEverPurchased;
				}
				Snapshot = snapshot;
			}
		}
		return UniTask.FromResult<IReadOnlyDictionary<string, ConflictAction>>((IReadOnlyDictionary<string, ConflictAction>)dictionary);
	}
}
