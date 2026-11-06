using UnlockRing.Core.Domain;

public interface IGameplaySolveReason
{
	GameplaySolveReason SolveReason { get; }

	PartId PassiveByPartId { get; }

	PartId StepBatchPartId { get; }

	void ClearSolveReason();

	void SetSolveReason(GameplaySolveReason reason, PartId passiveByPartId, PartId stepBatchPartId);
}
