using System.Collections.Generic;
using UnityEngine;

namespace UnlockRing.Presentation;

public static class PresentationBuilder
{
	private const string TAG = "[Presentation][Builder] ";

	public static PresentationNodeBase Build(List<GameplayCauseEventRecord> records)
	{
		if (records == null || records.Count == 0)
		{
			return null;
		}
		Dictionary<GameObject, PresentationNodeBase> dictionary = new Dictionary<GameObject, PresentationNodeBase>(records.Count);
		List<PresentationNodeBase> list = new List<PresentationNodeBase>(records.Count);
		for (int i = 0; i < records.Count; i++)
		{
			GameplayCauseEventRecord gameplayCauseEventRecord = records[i];
			if (!((Object)(object)gameplayCauseEventRecord?.self == (Object)null) && !dictionary.ContainsKey(gameplayCauseEventRecord.self))
			{
				PresentationNodeBase presentationNodeBase = CreateNode(gameplayCauseEventRecord);
				if (presentationNodeBase != null)
				{
					dictionary.Add(gameplayCauseEventRecord.self, presentationNodeBase);
				}
			}
		}
		for (int j = 0; j < records.Count; j++)
		{
			GameplayCauseEventRecord gameplayCauseEventRecord2 = records[j];
			if (!((Object)(object)gameplayCauseEventRecord2?.self == (Object)null) && dictionary.TryGetValue(gameplayCauseEventRecord2.self, out var value))
			{
				GameObject srouceObj = gameplayCauseEventRecord2.srouceObj;
				PresentationNodeBase value2;
				if ((Object)(object)srouceObj == (Object)null)
				{
					list.Add(value);
				}
				else if (dictionary.TryGetValue(srouceObj, out value2))
				{
					value2.AddNode(value);
				}
				else
				{
					list.Add(value);
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		AggregateRootNode aggregateRootNode = new AggregateRootNode();
		List<IPresentationNode> children = aggregateRootNode.Children;
		children.Capacity = list.Count;
		for (int k = 0; k < list.Count; k++)
		{
			children.Add(list[k]);
		}
		return aggregateRootNode;
	}

	private static PresentationNodeBase CreateNode(GameplayCauseEventRecord record)
	{
		return record.currentCauseType switch
		{
			CauseEventType.Pointup => new PointupPresentationNode(record.self), 
			CauseEventType.Bomb => new BombPresentationNode(null, null, record.self), 
			CauseEventType.RingBreak => new RingBreakPresentationNode(null, null, record.self, record.srouceObj), 
			_ => null, 
		};
	}

	public static PresentationNodeBase Build(SolveCauseForest forest, PresentationContext context)
	{
		if (forest == null || forest.Roots.Count == 0)
		{
			return null;
		}
		if (forest.Roots.Count == 1)
		{
			return BuildNode(forest.Roots[0], context);
		}
		AggregateRootNode aggregateRootNode = new AggregateRootNode();
		List<IPresentationNode> children = aggregateRootNode.Children;
		children.Capacity = forest.Roots.Count;
		for (int i = 0; i < forest.Roots.Count; i++)
		{
			PresentationNodeBase presentationNodeBase = BuildNode(forest.Roots[i], context);
			if (presentationNodeBase != null)
			{
				children.Add(presentationNodeBase);
			}
		}
		if (children.Count <= 0)
		{
			return null;
		}
		return aggregateRootNode;
	}

	private static PresentationNodeBase BuildNode(CauseNode causeNode, PresentationContext context)
	{
		if (causeNode == null)
		{
			return null;
		}
		PresentationNodeBase presentationNodeBase = CreateNode(causeNode, context);
		if (presentationNodeBase == null)
		{
			return null;
		}
		if (causeNode.Kind == CauseNodeKind.Solve && causeNode.SolveResult?.GameplayObject != null && context?.ChainService != null && context.ChainService.HasPendingChainBreak(causeNode.SolveResult.GameplayObject))
		{
			ChainBreakPresentationNode chainBreakPresentationNode = new ChainBreakPresentationNode(causeNode.SolveResult.GameplayObject, context);
			chainBreakPresentationNode.AddNode(presentationNodeBase);
			presentationNodeBase = chainBreakPresentationNode;
		}
		List<CauseNode> children = causeNode.Children;
		if (children.Count > 0)
		{
			List<IPresentationNode> children2 = presentationNodeBase.Children;
			children2.Capacity = children.Count;
			for (int i = 0; i < children.Count; i++)
			{
				PresentationNodeBase presentationNodeBase2 = BuildNode(children[i], context);
				if (presentationNodeBase2 != null)
				{
					children2.Add(presentationNodeBase2);
				}
			}
		}
		return presentationNodeBase;
	}

	private static PresentationNodeBase CreateNode(CauseNode causeNode, PresentationContext context)
	{
		return causeNode.Kind switch
		{
			CauseNodeKind.BombRoot => new BombPresentationNode(causeNode.BombResult, context), 
			CauseNodeKind.Solve => new RingBreakPresentationNode(causeNode.SolveResult, context), 
			CauseNodeKind.PropRoot => new PropRootPresentationNode(), 
			_ => null, 
		};
	}
}
