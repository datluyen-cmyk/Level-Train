using System.Collections.Generic;
using UnityEngine;

public class LayerAreaBlockedHelper
{
	protected List<LayerArea> layerAreasBlocked;

	protected List<ILayerMaskHelper> layerMaskHelpers;

	public bool SetBlockedByLayer(LayerArea layerArea, bool isBlocked)
	{
		if (isBlocked)
		{
			if (layerAreasBlocked == null)
			{
				return false;
			}
			if (layerAreasBlocked.Count <= 0 && layerMaskHelpers != null)
			{
				layerMaskHelpers.ForEach(delegate(ILayerMaskHelper x)
				{
					x.SetLayer(0);
				});
			}
			if (!layerAreasBlocked.Contains(layerArea))
			{
				layerAreasBlocked.Add(layerArea);
			}
			return true;
		}
		if (layerAreasBlocked == null)
		{
			return false;
		}
		layerAreasBlocked.Remove(layerArea);
		if (layerAreasBlocked.Count > 0)
		{
			if (layerMaskHelpers != null)
			{
				layerMaskHelpers.ForEach(delegate(ILayerMaskHelper x)
				{
					x.SetLayer(0);
				});
			}
			return true;
		}
		if (layerMaskHelpers != null)
		{
			layerMaskHelpers.ForEach(delegate(ILayerMaskHelper x)
			{
				x.ResetToOriginalLayer();
			});
		}
		return false;
	}

	public void AddLayerMaskHelper(ILayerMaskHelper layerMaskHelper)
	{
		if (layerMaskHelpers != null && layerMaskHelper != null)
		{
			layerMaskHelpers.Add(layerMaskHelper);
		}
	}

	public void ApplyAllLayerMaskHelpersIn(Transform transform, bool includeChildren = false)
	{
		if ((Object)(object)transform == (Object)null || layerMaskHelpers == null)
		{
			return;
		}
		ILayerMaskHelper component = ((Component)transform).GetComponent<ILayerMaskHelper>();
		if (component != null)
		{
			layerMaskHelpers.Add(component);
		}
		if (includeChildren)
		{
			ILayerMaskHelper[] componentsInChildren = ((Component)transform).GetComponentsInChildren<ILayerMaskHelper>();
			if (componentsInChildren != null && componentsInChildren.Length >= 1)
			{
				layerMaskHelpers.AddRange(componentsInChildren);
			}
		}
	}
}
