using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace RotateRings.GamePlay.Services;

public sealed class RingLockFeatureService : GameplayServiceBase<GameplayManager>
{
	private readonly List<IPartLockFeature> features = new List<IPartLockFeature>();

	private bool collected;

	public RingLockFeatureService(GameplayManager host)
		: base(host)
	{
	}

	public bool ShouldPreserveBindingOnDragExit(PartId partId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		EnsureCollected();
		for (int i = 0; i < features.Count; i++)
		{
			if (features[i].ShouldPreserveBindingOnDragExit(partId))
			{
				return true;
			}
		}
		return false;
	}

	public bool ArePartsConnected(PartId lockedPartId, PartId lockerPartId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		EnsureCollected();
		for (int i = 0; i < features.Count; i++)
		{
			if (features[i].ArePartsConnected(lockedPartId, lockerPartId))
			{
				return true;
			}
		}
		return false;
	}

	public void NotifyObjectSolved(RingObject solvedRing)
	{
		EnsureCollected();
		for (int i = 0; i < features.Count; i++)
		{
			features[i].OnObjectSolved(solvedRing);
		}
	}

	private void EnsureCollected()
	{
		if (!collected)
		{
			collected = true;
			base.Host.CollectServices(features);
		}
	}
}
