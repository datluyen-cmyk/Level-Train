using UnityEngine;

internal readonly struct ChainInstanceData
{
	public readonly GameObject Instance;

	public readonly ChainDirection Direction;

	public ChainInstanceData(GameObject instance, ChainDirection direction)
	{
		Instance = instance;
		Direction = direction;
	}
}
