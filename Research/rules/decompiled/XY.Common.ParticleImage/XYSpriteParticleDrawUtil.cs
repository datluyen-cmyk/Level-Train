using UnityEngine;
using UnityEngine.Sprites;

namespace XY.Common.ParticleImage;

internal static class XYSpriteParticleDrawUtil
{
	public static float GetAspectRatio(Sprite sprite)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sprite == (Object)null)
		{
			return 1f;
		}
		if (TryGetSpritePixelSize(sprite, out var pixelWidth, out var pixelHeight))
		{
			return pixelWidth / pixelHeight;
		}
		Rect textureRect = sprite.textureRect;
		if (((Rect)(ref textureRect)).width > 0f && ((Rect)(ref textureRect)).height > 0f)
		{
			return ((Rect)(ref textureRect)).width / ((Rect)(ref textureRect)).height;
		}
		Rect rect = sprite.rect;
		if (((Rect)(ref rect)).width > 0f && ((Rect)(ref rect)).height > 0f)
		{
			return ((Rect)(ref rect)).width / ((Rect)(ref rect)).height;
		}
		return 1f;
	}

	public static Vector3 FitSizeToAspect(Vector3 size, float aspect)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!IsValidAspect(aspect))
		{
			return size;
		}
		float num = Mathf.Max(size.x, size.y);
		if (num <= 0f)
		{
			return size;
		}
		if (aspect >= 1f)
		{
			return new Vector3(num, num / aspect, size.z);
		}
		return new Vector3(num * aspect, num, size.z);
	}

	public static SpriteSheet BuildSheet(Sprite sprite)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sprite == (Object)null)
		{
			return new SpriteSheet(new Vector2(1f, 1f), Vector2.zero);
		}
		Vector4 outerUV = DataUtility.GetOuterUV(sprite);
		if (IsValidOuterUv(outerUV))
		{
			return new SpriteSheet(new Vector2(((Vector4)(ref outerUV))[2], ((Vector4)(ref outerUV))[3]), new Vector2(((Vector4)(ref outerUV))[0], ((Vector4)(ref outerUV))[1]));
		}
		return BuildSheetFromTextureRect(sprite);
	}

	private static bool IsValidOuterUv(Vector4 outerUv)
	{
		if (((Vector4)(ref outerUv))[2] > ((Vector4)(ref outerUv))[0])
		{
			return ((Vector4)(ref outerUv))[3] > ((Vector4)(ref outerUv))[1];
		}
		return false;
	}

	private static SpriteSheet BuildSheetFromTextureRect(Sprite sprite)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Texture2D texture = sprite.texture;
		Rect textureRect = sprite.textureRect;
		if ((Object)(object)texture == (Object)null || ((Rect)(ref textureRect)).width <= 0f || ((Rect)(ref textureRect)).height <= 0f)
		{
			return new SpriteSheet(new Vector2(1f, 1f), Vector2.zero);
		}
		float num = 1f / (float)((Texture)texture).width;
		float num2 = 1f / (float)((Texture)texture).height;
		return new SpriteSheet(new Vector2((((Rect)(ref textureRect)).x + ((Rect)(ref textureRect)).width) * num, (((Rect)(ref textureRect)).y + ((Rect)(ref textureRect)).height) * num2), new Vector2(((Rect)(ref textureRect)).x * num, ((Rect)(ref textureRect)).y * num2));
	}

	private static bool TryGetSpritePixelSize(Sprite sprite, out float pixelWidth, out float pixelHeight)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Texture2D texture = sprite.texture;
		if ((Object)(object)texture == (Object)null)
		{
			Rect textureRect = sprite.textureRect;
			if (((Rect)(ref textureRect)).width <= 0f || ((Rect)(ref textureRect)).height <= 0f)
			{
				pixelWidth = 0f;
				pixelHeight = 0f;
				return false;
			}
			pixelWidth = ((Rect)(ref textureRect)).width;
			pixelHeight = ((Rect)(ref textureRect)).height;
			return true;
		}
		Vector4 outerUV = DataUtility.GetOuterUV(sprite);
		pixelWidth = (((Vector4)(ref outerUV))[2] - ((Vector4)(ref outerUV))[0]) * (float)((Texture)texture).width;
		pixelHeight = (((Vector4)(ref outerUV))[3] - ((Vector4)(ref outerUV))[1]) * (float)((Texture)texture).height;
		if (pixelWidth > 0f)
		{
			return pixelHeight > 0f;
		}
		return false;
	}

	private static bool IsValidAspect(float aspect)
	{
		if (aspect > 0f && !float.IsNaN(aspect))
		{
			return !float.IsInfinity(aspect);
		}
		return false;
	}
}
