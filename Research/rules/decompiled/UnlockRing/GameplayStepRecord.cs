using System;
using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class GameplayStepRecord
{
	public int StepIndex { get; }

	public PartId PartId { get; }

	public GameplayStepState State { get; }

	public int OperationCount { get; }

	public int ReleasedPartCount { get; }

	public IReadOnlyList<GameplayReleasedPart> ReleasedParts { get; }

	public IReadOnlyList<LegacyOperationSource> OperationSources { get; }

	public GameplayStepRecord(int stepIndex, PartId partId, GameplayStepState state, int operationCount, int releasedPartCount, IReadOnlyList<GameplayReleasedPart> releasedParts, IReadOnlyList<LegacyOperationSource> operationSources)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		StepIndex = stepIndex;
		PartId = partId;
		State = state;
		OperationCount = operationCount;
		ReleasedPartCount = releasedPartCount;
		ReleasedParts = releasedParts ?? throw new ArgumentNullException("releasedParts");
		OperationSources = operationSources ?? throw new ArgumentNullException("operationSources");
	}
}
