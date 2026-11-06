using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnlockRing.Core.Authoring;

public static class RingColorEditor
{
	private sealed class AdjacencyGraph
	{
		private readonly Dictionary<string, HashSet<string>> neighbors = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

		public static AdjacencyGraph BuildFromRingLinks(IReadOnlyList<RingLinkEntry> links, IReadOnlyCollection<string> stickPartIds = null)
		{
			AdjacencyGraph adjacencyGraph = new AdjacencyGraph();
			if (links == null || links.Count == 0)
			{
				return adjacencyGraph;
			}
			for (int i = 0; i < links.Count; i++)
			{
				RingLinkEntry link = links[i];
				string ringPartId;
				string stickPartId;
				if (IsRingRingThreadingLink(link))
				{
					adjacencyGraph.AddEdge(link.sourceRingId, link.targetPartId);
				}
				else if (stickPartIds != null && TryGetStickLockRingPartId(link, stickPartIds, out ringPartId, out stickPartId))
				{
					adjacencyGraph.AddEdge(ringPartId, stickPartId);
				}
			}
			return adjacencyGraph;
		}

		public IReadOnlyCollection<string> GetNeighbors(string ringPartId)
		{
			if (string.IsNullOrEmpty(ringPartId) || !neighbors.TryGetValue(ringPartId, out var value))
			{
				return (IReadOnlyCollection<string>)(object)Array.Empty<string>();
			}
			return value;
		}

		public int GetNeighborCount(string ringPartId)
		{
			return GetNeighbors(ringPartId).Count;
		}

		public IEnumerable<string> GetAllPartIds()
		{
			return neighbors.Keys;
		}

		private static bool IsRingRingThreadingLink(RingLinkEntry link)
		{
			if (link.relationKind == 0 && link.activityKind == 0 && !string.IsNullOrEmpty(link.sourceRingId) && IsRingPartId(link.sourceRingId))
			{
				return IsRingPartId(link.targetPartId);
			}
			return false;
		}

		private void AddEdge(string ringPartIdA, string ringPartIdB)
		{
			if (!string.Equals(ringPartIdA, ringPartIdB, StringComparison.Ordinal))
			{
				AddDirectedNeighbor(ringPartIdA, ringPartIdB);
				AddDirectedNeighbor(ringPartIdB, ringPartIdA);
			}
		}

		private void AddDirectedNeighbor(string fromRingPartId, string toRingPartId)
		{
			if (!neighbors.TryGetValue(fromRingPartId, out var value))
			{
				value = new HashSet<string>(StringComparer.Ordinal);
				neighbors[fromRingPartId] = value;
			}
			value.Add(toRingPartId);
		}
	}

	public const int PaletteCount = 7;

	private const string LegacyRingPrefix = "ring_";

	private const string CoreRingId = "rotating";

	private const string CoreRingPrefix = "rotating-";

	public static RingLinkEntry[] FromDraftConstraints(IList<EditableAuthoredConstraintDraft> constraints)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected I4, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected I4, but got Unknown
		if (constraints == null || constraints.Count == 0)
		{
			return Array.Empty<RingLinkEntry>();
		}
		List<RingLinkEntry> list = new List<RingLinkEntry>(constraints.Count);
		for (int i = 0; i < constraints.Count; i++)
		{
			EditableAuthoredConstraintDraft val = constraints[i];
			if ((int)val.Kind != 1)
			{
				list.Add(new RingLinkEntry
				{
					sourceRingId = val.FirstPartId,
					pinFeatureId = val.FirstFeatureId,
					targetPartId = val.SecondPartId,
					targetFeatureId = val.SecondFeatureId,
					relationKind = (int)val.Kind,
					activityKind = (int)val.ActivityKind
				});
			}
		}
		return list.ToArray();
	}

	public static GuideLinkEntry[] ExtractGuideLinks(IList<EditableAuthoredConstraintDraft> constraints)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		if (constraints == null || constraints.Count == 0)
		{
			return Array.Empty<GuideLinkEntry>();
		}
		List<GuideLinkEntry> list = new List<GuideLinkEntry>(constraints.Count);
		for (int i = 0; i < constraints.Count; i++)
		{
			EditableAuthoredConstraintDraft val = constraints[i];
			if ((int)val.Kind == 1)
			{
				list.Add(new GuideLinkEntry
				{
					guidePartId = val.FirstPartId,
					lockedPartId = val.SecondPartId
				});
			}
		}
		return list.ToArray();
	}

	public static bool LevelHasSlidingStick(IList<EditablePartDraft> parts)
	{
		if (parts == null || parts.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			if (parts[i] is SlidingStickDraft)
			{
				return true;
			}
		}
		return false;
	}

	public static Color GetSequentialPaletteColor(int partIndex)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return PartPalette.For(ModPaletteIndex(partIndex, 7));
	}

	public static HashSet<string> CollectSlidingStickPartIds(IList<EditablePartDraft> parts)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		if (parts == null || parts.Count == 0)
		{
			return hashSet;
		}
		for (int i = 0; i < parts.Count; i++)
		{
			EditablePartDraft obj = parts[i];
			SlidingStickDraft val = (SlidingStickDraft)(object)((obj is SlidingStickDraft) ? obj : null);
			if (val != null)
			{
				hashSet.Add(((EditablePartDraft)val).Id);
			}
		}
		return hashSet;
	}

	public static Dictionary<string, string> BuildStickToLinkedRingMap(IReadOnlyList<RingLinkEntry> links, IReadOnlyCollection<string> stickPartIds)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		if (links == null || links.Count == 0 || stickPartIds == null || stickPartIds.Count == 0)
		{
			return dictionary;
		}
		for (int i = 0; i < links.Count; i++)
		{
			if (TryGetStickLockRingPartId(links[i], stickPartIds, out var ringPartId, out var stickPartId))
			{
				dictionary[stickPartId] = ringPartId;
			}
		}
		return dictionary;
	}

	public static Color ResolveStickPaletteColor(string stickPartId, int partIndex, IReadOnlyDictionary<string, string> stickToLinkedRingId, IReadOnlyDictionary<string, int> paletteIndices, IReadOnlyDictionary<string, int> preferredIndices = null)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(stickPartId) && stickToLinkedRingId != null && stickToLinkedRingId.ContainsKey(stickPartId))
		{
			if (paletteIndices != null && paletteIndices.TryGetValue(stickPartId, out var value))
			{
				return PartPalette.For(value);
			}
			return PartPalette.For(GetPreferredIndex(stickPartId, preferredIndices, 7));
		}
		return GetSequentialPaletteColor(partIndex);
	}

	public static int ResolveStickPaletteIndex(string stickPartId, int partIndex, IReadOnlyDictionary<string, string> stickToLinkedRingId, IReadOnlyDictionary<string, int> paletteIndices, IReadOnlyDictionary<string, int> preferredIndices = null)
	{
		if (!string.IsNullOrEmpty(stickPartId) && stickToLinkedRingId != null && stickToLinkedRingId.ContainsKey(stickPartId))
		{
			if (paletteIndices != null && paletteIndices.TryGetValue(stickPartId, out var value))
			{
				return value;
			}
			return GetPreferredIndex(stickPartId, preferredIndices, 7);
		}
		return ModPaletteIndex(partIndex, 7);
	}

	public static Dictionary<string, int> SolvePaletteIndices(IReadOnlyList<RingLinkEntry> links, IReadOnlyDictionary<string, int> preferredIndices = null, IReadOnlyCollection<string> stickPartIds = null)
	{
		if (links == null || links.Count == 0)
		{
			return new Dictionary<string, int>();
		}
		return Solve(AdjacencyGraph.BuildFromRingLinks(links, stickPartIds), 0, preferredIndices);
	}

	private static Dictionary<string, int> Solve(AdjacencyGraph graph, int colorCount, IReadOnlyDictionary<string, int> preferredIndices)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		if (graph == null)
		{
			return dictionary;
		}
		if (colorCount <= 0)
		{
			colorCount = 7;
		}
		List<string> list = CollectGraphPartIds(graph);
		if (list.Count == 0)
		{
			return dictionary;
		}
		list.Sort(delegate(string a, string b)
		{
			int num2 = graph.GetNeighborCount(b).CompareTo(graph.GetNeighborCount(a));
			return (num2 != 0) ? num2 : GetPreferredIndex(a, preferredIndices, colorCount).CompareTo(GetPreferredIndex(b, preferredIndices, colorCount));
		});
		if (!TryAssignColors(list, 0, graph, colorCount, dictionary, preferredIndices))
		{
			dictionary.Clear();
			for (int num = 0; num < list.Count; num++)
			{
				string text = list[num];
				dictionary[text] = GetPreferredIndex(text, preferredIndices, colorCount);
			}
		}
		ResolveSameColorAdjacentConflicts(graph, dictionary, colorCount, preferredIndices);
		return dictionary;
	}

	private static int GetPreferredIndex(string ringPartId, IReadOnlyDictionary<string, int> preferredIndices, int colorCount)
	{
		if (preferredIndices != null && preferredIndices.TryGetValue(ringPartId, out var value))
		{
			return ModPaletteIndex(value, colorCount);
		}
		return 0;
	}

	private static int ModPaletteIndex(int index, int colorCount)
	{
		if (colorCount <= 0)
		{
			return 0;
		}
		int num = index % colorCount;
		if (num >= 0)
		{
			return num;
		}
		return num + colorCount;
	}

	private static IEnumerable<int> ColorCandidates(int preferred, int colorCount)
	{
		int normalizedPreferred = ModPaletteIndex(preferred, colorCount);
		for (int k = 0; k < colorCount; k++)
		{
			yield return (normalizedPreferred + k) % colorCount;
		}
	}

	private static List<string> CollectGraphPartIds(AdjacencyGraph graph)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		foreach (string allPartId in graph.GetAllPartIds())
		{
			hashSet.Add(allPartId);
			foreach (string neighbor in graph.GetNeighbors(allPartId))
			{
				hashSet.Add(neighbor);
			}
		}
		return hashSet.ToList();
	}

	private static bool TryAssignColors(IReadOnlyList<string> orderedRingPartIds, int index, AdjacencyGraph graph, int colorCount, Dictionary<string, int> assignments, IReadOnlyDictionary<string, int> preferredIndices)
	{
		if (index >= orderedRingPartIds.Count)
		{
			return true;
		}
		string text = orderedRingPartIds[index];
		if (assignments.ContainsKey(text))
		{
			return TryAssignColors(orderedRingPartIds, index + 1, graph, colorCount, assignments, preferredIndices);
		}
		HashSet<int> hashSet = new HashSet<int>();
		foreach (string neighbor in graph.GetNeighbors(text))
		{
			if (assignments.TryGetValue(neighbor, out var value))
			{
				hashSet.Add(value);
			}
		}
		foreach (int item in ColorCandidates(GetPreferredIndex(text, preferredIndices, colorCount), colorCount))
		{
			if (!hashSet.Contains(item))
			{
				assignments[text] = item;
				if (TryAssignColors(orderedRingPartIds, index + 1, graph, colorCount, assignments, preferredIndices))
				{
					return true;
				}
				assignments.Remove(text);
			}
		}
		return false;
	}

	private static void ResolveSameColorAdjacentConflicts(AdjacencyGraph graph, Dictionary<string, int> assignments, int colorCount, IReadOnlyDictionary<string, int> preferredIndices)
	{
		bool flag = true;
		int num = assignments.Count * colorCount;
		while (flag && num-- > 0)
		{
			flag = false;
			foreach (KeyValuePair<string, int> item in assignments.ToList())
			{
				string key = item.Key;
				int value = item.Value;
				foreach (string neighbor in graph.GetNeighbors(key))
				{
					if (assignments.TryGetValue(neighbor, out var value2) && value2 == value)
					{
						string ringPartId = ((graph.GetNeighborCount(key) <= graph.GetNeighborCount(neighbor)) ? key : neighbor);
						if (TryRecolor(graph, assignments, ringPartId, colorCount, preferredIndices))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
	}

	private static bool TryRecolor(AdjacencyGraph graph, Dictionary<string, int> assignments, string ringPartId, int colorCount, IReadOnlyDictionary<string, int> preferredIndices)
	{
		if (!assignments.ContainsKey(ringPartId))
		{
			return false;
		}
		int item = assignments[ringPartId];
		HashSet<int> hashSet = new HashSet<int> { item };
		foreach (string neighbor in graph.GetNeighbors(ringPartId))
		{
			if (assignments.TryGetValue(neighbor, out var value))
			{
				hashSet.Add(value);
			}
		}
		foreach (int item2 in ColorCandidates(GetPreferredIndex(ringPartId, preferredIndices, colorCount), colorCount))
		{
			if (!hashSet.Contains(item2))
			{
				assignments[ringPartId] = item2;
				return true;
			}
		}
		return false;
	}

	private static bool IsThreadingCrossingLink(RingLinkEntry link)
	{
		if (link.relationKind == 0 && link.activityKind == 0 && !string.IsNullOrEmpty(link.sourceRingId))
		{
			return !string.IsNullOrEmpty(link.targetPartId);
		}
		return false;
	}

	private static bool TryGetStickLockRingPartId(RingLinkEntry link, IReadOnlyCollection<string> stickPartIds, out string ringPartId, out string stickPartId)
	{
		ringPartId = null;
		stickPartId = null;
		if (!IsThreadingCrossingLink(link) || stickPartIds == null)
		{
			return false;
		}
		if (stickPartIds.Contains(link.targetPartId) && IsRingPartId(link.sourceRingId))
		{
			ringPartId = link.sourceRingId;
			stickPartId = link.targetPartId;
			return true;
		}
		if (stickPartIds.Contains(link.sourceRingId) && IsRingPartId(link.targetPartId))
		{
			ringPartId = link.targetPartId;
			stickPartId = link.sourceRingId;
			return true;
		}
		return false;
	}

	private static bool IsRingPartId(string partId)
	{
		if (!string.IsNullOrEmpty(partId))
		{
			if (!partId.StartsWith("ring_", StringComparison.Ordinal) && !partId.Equals("rotating", StringComparison.Ordinal))
			{
				return partId.StartsWith("rotating-", StringComparison.Ordinal);
			}
			return true;
		}
		return false;
	}
}
