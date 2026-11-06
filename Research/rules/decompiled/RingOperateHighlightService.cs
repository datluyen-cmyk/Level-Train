using System.Collections.Generic;
using UnityEngine;

public sealed class RingOperateHighlightService
{
	private readonly Dictionary<int, List<RingOperateHighlight>> instances = new Dictionary<int, List<RingOperateHighlight>>();

	internal bool TrySetHighlight(GameObject partRoot, bool highlight)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return TrySetHighlight(partRoot, highlight, RingOperateHighlight.OperateHighlightColor);
	}

	internal bool TrySetHighlight(GameObject partRoot, bool highlight, Color outlineColor)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (!highlight)
		{
			if ((Object)(object)partRoot == (Object)null)
			{
				return true;
			}
			int instanceID = ((Object)partRoot).GetInstanceID();
			if (instances.TryGetValue(instanceID, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					value[i].SetHighlight(highlight: false);
				}
			}
			return true;
		}
		if ((Object)(object)partRoot == (Object)null)
		{
			return false;
		}
		int instanceID2 = ((Object)partRoot).GetInstanceID();
		if (!instances.TryGetValue(instanceID2, out var value2))
		{
			value2 = CreateHighlightGroup(partRoot, outlineColor);
			if (value2.Count == 0)
			{
				return false;
			}
			instances[instanceID2] = value2;
		}
		for (int j = 0; j < value2.Count; j++)
		{
			value2[j].SetHighlight(highlight: true);
		}
		return true;
	}

	internal void Remove(GameObject partRoot)
	{
		if (!((Object)(object)partRoot == (Object)null))
		{
			int instanceID = ((Object)partRoot).GetInstanceID();
			if (instances.TryGetValue(instanceID, out var value))
			{
				DisposeGroup(value);
				instances.Remove(instanceID);
			}
		}
	}

	internal void DropCache(GameObject partRoot)
	{
		if (!((Object)(object)partRoot == (Object)null))
		{
			instances.Remove(((Object)partRoot).GetInstanceID());
		}
	}

	internal void ClearAll()
	{
		foreach (List<RingOperateHighlight> value in instances.Values)
		{
			DisposeGroup(value);
		}
		instances.Clear();
	}

	private static List<RingOperateHighlight> CreateHighlightGroup(GameObject partRoot, Color outlineColor)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer[] componentsInChildren = partRoot.GetComponentsInChildren<MeshRenderer>();
		List<RingOperateHighlight> list = new List<RingOperateHighlight>(componentsInChildren.Length);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RingOperateHighlight ringOperateHighlight = RingOperateHighlight.Create(componentsInChildren[i], outlineColor);
			if (ringOperateHighlight != null)
			{
				list.Add(ringOperateHighlight);
			}
		}
		return list;
	}

	private static void DisposeGroup(List<RingOperateHighlight> group)
	{
		for (int i = 0; i < group.Count; i++)
		{
			group[i].Dispose();
		}
	}
}
