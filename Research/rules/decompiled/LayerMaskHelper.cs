using UnityEngine;

public class LayerMaskHelper : MonoBehaviour, ILayerMaskHelper
{
	[SerializeField]
	private int originalLayer;

	public void SetLayer(int layer)
	{
		if (originalLayer == -1)
		{
			originalLayer = ((Component)this).gameObject.layer;
		}
		((Component)this).gameObject.layer = layer;
	}

	public void ResetToOriginalLayer()
	{
		if (originalLayer != -1)
		{
			((Component)this).gameObject.layer = originalLayer;
		}
	}

	private void Awake()
	{
		originalLayer = ((Component)this).gameObject.layer;
	}
}
