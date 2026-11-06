using System.Collections.Generic;
using UnlockRing.Core.Domain;

namespace UnlockRing.Presentation;

public sealed class SolveCauseForest
{
	public List<CauseNode> Roots { get; } = new List<CauseNode>(4);

	public Dictionary<PartId, CauseNode> SolveNodesByPartId { get; } = new Dictionary<PartId, CauseNode>(8);
}
