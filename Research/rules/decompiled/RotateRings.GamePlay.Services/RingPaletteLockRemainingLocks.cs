using System.Collections.Generic;
using UnityEngine;

namespace RotateRings.GamePlay.Services;

internal sealed class RingPaletteLockRemainingLocks
{
	private readonly Dictionary<RingObject, int> countByRing = new Dictionary<RingObject, int>();

	public void Clear()
	{
		countByRing.Clear();
	}

	public void Add(RingObject ring)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			countByRing.TryGetValue(ring, out var value);
			countByRing[ring] = value + 1;
		}
	}

	public void Set(RingObject ring, int count)
	{
		if (!((Object)(object)ring == (Object)null))
		{
			if (count <= 0)
			{
				countByRing.Remove(ring);
			}
			else
			{
				countByRing[ring] = count;
			}
		}
	}

	public int Get(RingObject ring)
	{
		if ((Object)(object)ring == (Object)null)
		{
			return 0;
		}
		if (!countByRing.TryGetValue(ring, out var value))
		{
			return 0;
		}
		return value;
	}

	internal int Decrement(RingObject ring)
	{
		if ((Object)(object)ring == (Object)null || !countByRing.TryGetValue(ring, out var value))
		{
			return 0;
		}
		value--;
		if (value <= 0)
		{
			countByRing.Remove(ring);
			return 0;
		}
		countByRing[ring] = value;
		return value;
	}

	internal void ClearForRing(RingObject ring)
	{
		if ((Object)(object)ring != (Object)null)
		{
			countByRing.Remove(ring);
		}
	}
}
