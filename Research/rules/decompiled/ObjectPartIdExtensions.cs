using UnlockRing.Core.Domain;

public static class ObjectPartIdExtensions
{
	public static bool TryGetPartId(this IGameplayObject gameplayObject, out PartId partId)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		partId = default(PartId);
		if (gameplayObject == null)
		{
			return false;
		}
		if (!(gameplayObject is RingObject ringObject))
		{
			if (gameplayObject is PullObject pullObject)
			{
				return pullObject.TryGetPartId(out partId);
			}
			string physicPartId = gameplayObject.PartData.PhysicPartId;
			if (string.IsNullOrEmpty(physicPartId))
			{
				return false;
			}
			partId = new PartId(physicPartId);
			return true;
		}
		return ringObject.TryGetPartId(out partId);
	}
}
