using UnityEngine;

namespace UnlockRing.Presentation;

public sealed class ComboEncouragementResult : IPresentationResult
{
	public GameplayOperationEvaluation Evaluation { get; }

	public Vector3 WorldPosition { get; }

	public ComboEncouragementResult(GameplayOperationEvaluation evaluation, Vector3 worldPosition)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Evaluation = evaluation;
		WorldPosition = worldPosition;
	}
}
