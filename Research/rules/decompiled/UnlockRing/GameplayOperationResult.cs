using System;
using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing;

public sealed class GameplayOperationResult
{
	public PartId PartId { get; }

	public LegacyOperationSource OperationSource { get; }

	public GameplayOperationCompletion Completion { get; }

	public int ReleasedPartCount { get; }

	public IReadOnlyList<GameplayReleasedPart> ReleasedParts { get; }

	public bool HasSolved { get; }

	public bool HasRelease => ReleasedPartCount > 0;

	public GameplayOperationResult(PartId partId, LegacyOperationSource operationSource, GameplayOperationCompletion completion, int releasedPartCount = 0, bool hasSolved = false)
		: this(partId, operationSource, completion, releasedPartCount, Array.Empty<GameplayReleasedPart>(), hasSolved)
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)


	public GameplayOperationResult(PartId partId, LegacyOperationSource operationSource, GameplayOperationCompletion completion, int releasedPartCount, IReadOnlyList<GameplayReleasedPart> releasedParts, bool hasSolved = false)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (releasedPartCount < 0)
		{
			throw new ArgumentOutOfRangeException("releasedPartCount");
		}
		if (releasedParts == null)
		{
			throw new ArgumentNullException("releasedParts");
		}
		if (completion == GameplayOperationCompletion.Blocked && (releasedPartCount != 0 || releasedParts.Count != 0))
		{
			throw new ArgumentException("Blocked operation cannot release parts.", "releasedPartCount");
		}
		PartId = partId;
		OperationSource = operationSource;
		Completion = completion;
		ReleasedParts = releasedParts;
		ReleasedPartCount = releasedPartCount;
		HasSolved = hasSolved;
	}
}
