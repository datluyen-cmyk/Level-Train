using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing.Presentation;

public sealed class CauseNode
{
	public CauseNodeKind Kind { get; }

	public BombBoomResult BombResult { get; }

	public RingBreakResult SolveResult { get; }

	public PartId PartId { get; }

	public LegacyOperationSource? PropSource { get; }

	public List<CauseNode> Children { get; } = new List<CauseNode>(2);

	private CauseNode(CauseNodeKind kind, BombBoomResult bombResult, RingBreakResult solveResult, PartId partId, LegacyOperationSource? propSource)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Kind = kind;
		BombResult = bombResult;
		SolveResult = solveResult;
		PartId = partId;
		PropSource = propSource;
	}

	public static CauseNode ForBomb(BombBoomResult bombResult)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return new CauseNode(CauseNodeKind.BombRoot, bombResult, null, default(PartId), null);
	}

	public static CauseNode ForSolve(RingBreakResult solveResult, PartId partId)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return new CauseNode(CauseNodeKind.Solve, null, solveResult, partId, null);
	}

	public static CauseNode ForProp(LegacyOperationSource propSource)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return new CauseNode(CauseNodeKind.PropRoot, null, null, default(PartId), propSource);
	}
}
