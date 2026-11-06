using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

public sealed class RotateRingOperateHighlightService : GameplayServiceBase<GameplayManager>
{
	private readonly Dictionary<int, List<RotateRingOperateHighlight>> instances = new Dictionary<int, List<RotateRingOperateHighlight>>();

	public RotateRingOperateHighlightService(GameplayManager host)
		: base(host)
	{
	}

	internal void ClearHighlightCache()
	{
		foreach (List<RotateRingOperateHighlight> value in instances.Values)
		{
			DisposeGroup(value);
		}
		instances.Clear();
	}

	public override void ClearLevel()
	{
		ClearHighlightCache();
		base.ClearLevel();
	}

	public override void Dispose()
	{
		if (!base.IsDisposed)
		{
			base.Dispose();
			ClearLevel();
		}
	}

	internal bool TrySetHighlight(GameObject partRoot, bool highlight)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return TrySetHighlight(partRoot, highlight, RotateRingOperateHighlight.OperateHighlightColor);
	}

	internal bool TrySetHighlight(GameObject partRoot, bool highlight, Color outlineColor)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
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

	private List<RotateRingOperateHighlight> CreateHighlightGroup(GameObject partRoot, Color outlineColor)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer[] componentsInChildren = partRoot.GetComponentsInChildren<MeshRenderer>();
		List<RotateRingOperateHighlight> list = new List<RotateRingOperateHighlight>(componentsInChildren.Length);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RotateRingOperateHighlight rotateRingOperateHighlight = RotateRingOperateHighlight.Create(componentsInChildren[i], outlineColor);
			if (rotateRingOperateHighlight != null)
			{
				list.Add(rotateRingOperateHighlight);
			}
		}
		return list;
	}

	private void DisposeGroup(List<RotateRingOperateHighlight> group)
	{
		for (int i = 0; i < group.Count; i++)
		{
			group[i].Dispose();
		}
	}
}
