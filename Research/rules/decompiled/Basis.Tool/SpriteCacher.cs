using System.Collections.Generic;
using UnityEngine;

namespace Basis.Tool;

public class SpriteCacher
{
	public Dictionary<string, Sprite> cardSpriteDict = new Dictionary<string, Sprite>();

	public Sprite GetCachedSprite(string spriteName)
	{
		if (cardSpriteDict.TryGetValue(spriteName, out var value))
		{
			return value;
		}
		return null;
	}

	public bool TyeAddSprite(string spriteName, Sprite sprite)
	{
		if (!cardSpriteDict.TryAdd(spriteName, sprite))
		{
			return false;
		}
		return true;
	}

	public void ClearAllCachedSprite()
	{
		cardSpriteDict.Clear();
	}
}
