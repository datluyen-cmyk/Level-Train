using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal static class GameplayGuideRtLayerExclusion
{
	private static readonly int DefaultLayer = LayerMask.NameToLayer("Default");

	internal static void ExcludeFromGuideRender(Transform root)
	{
		if (!((Object)(object)root == (Object)null))
		{
			ExcludeRecursive(root);
		}
	}

	private static void ExcludeRecursive(Transform node)
	{
		GameObject gameObject = ((Component)node).gameObject;
		if (gameObject.layer == 7)
		{
			gameObject.layer = DefaultLayer;
		}
		for (int i = 0; i < node.childCount; i++)
		{
			ExcludeRecursive(node.GetChild(i));
		}
	}
}
