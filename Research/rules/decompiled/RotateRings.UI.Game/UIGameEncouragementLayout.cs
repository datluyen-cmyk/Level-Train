using System.Collections.Generic;
using RotateRings.Common;
using UnityEngine;

namespace RotateRings.UI.Game;

public static class UIGameEncouragementLayout
{
	private struct PlacedEncouragementEntry
	{
		public float RegisteredTime;

		public Vector3 WorldAnchor;

		public RectTransform MountParent;

		public Vector2 CenterLocal;

		public Vector2 ClampHalfSize;
	}

	public const float EncouragementDisplayYOffset = -80f;

	public const float EmojiOffsetAboveEncouragementY = 120f;

	public static readonly Vector2 DefaultEncouragementClampHalfSize = new Vector2(400f, 160f) * 0.5f;

	public const float EmojiOffsetFromGameplayAnchorY = 0f;

	public static readonly float DefaultEmojiUpperHalfHeight = new Vector2(100f, 100f).y * 0.85f * 0.5f;

	private const float PlacedWorldAnchorMatchDistance = 2f;

	private const float PlacedEntryMaxAgeSeconds = 3f;

	private const int MaxPlacedEntryCount = 6;

	private static readonly List<PlacedEncouragementEntry> placedEncouragements = new List<PlacedEncouragementEntry>(6);

	public static void RegisterPlacedEncouragement(RectTransform mountParent, Vector3 worldAnchor, Vector2 centerLocal, Vector2 clampHalfSize)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mountParent == (Object)null))
		{
			PrunePlacedEncouragements();
			placedEncouragements.Add(new PlacedEncouragementEntry
			{
				RegisteredTime = Time.realtimeSinceStartup,
				WorldAnchor = worldAnchor,
				MountParent = mountParent,
				CenterLocal = centerLocal,
				ClampHalfSize = clampHalfSize
			});
			while (placedEncouragements.Count > 6)
			{
				placedEncouragements.RemoveAt(0);
			}
		}
	}

	public static bool TryResolvePlacedEncouragement(Vector3 worldAnchor, RectTransform mountParent, out Vector2 centerLocal, out Vector2 clampHalfSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		centerLocal = default(Vector2);
		clampHalfSize = default(Vector2);
		if ((Object)(object)mountParent == (Object)null)
		{
			return false;
		}
		PrunePlacedEncouragements();
		float num = 4f;
		int num2 = -1;
		for (int num3 = placedEncouragements.Count - 1; num3 >= 0; num3--)
		{
			PlacedEncouragementEntry placedEncouragementEntry = placedEncouragements[num3];
			if (!((Object)(object)placedEncouragementEntry.MountParent != (Object)(object)mountParent))
			{
				Vector3 val = placedEncouragementEntry.WorldAnchor - worldAnchor;
				float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
				if (sqrMagnitude <= num)
				{
					num = sqrMagnitude;
					num2 = num3;
				}
			}
		}
		if (num2 < 0)
		{
			return false;
		}
		PlacedEncouragementEntry placedEncouragementEntry2 = placedEncouragements[num2];
		centerLocal = placedEncouragementEntry2.CenterLocal;
		clampHalfSize = placedEncouragementEntry2.ClampHalfSize;
		return true;
	}

	public static void ClearPlacedEncouragements()
	{
		placedEncouragements.Clear();
	}

	public static bool TryComputeEncouragementCenter(Vector3 worldPosition, RectTransform mountParent, Vector2 clampHalfSize, bool reserveUpperEmojiCompanionSpace, out Vector2 centerLocal)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		centerLocal = default(Vector2);
		if ((Object)(object)mountParent == (Object)null || !WorldToUIPostionUtil.WorldToLocalPoint(worldPosition, mountParent, out var localPoint))
		{
			return false;
		}
		centerLocal = localPoint + new Vector2(0f, -80f);
		if (reserveUpperEmojiCompanionSpace)
		{
			centerLocal = UIGameRingRectUiClamp.ClampEncouragementWithEmojiStack(centerLocal, mountParent, clampHalfSize, 120f, DefaultEmojiUpperHalfHeight);
		}
		else
		{
			centerLocal = UIGameRingRectUiClamp.ClampEncouragement(centerLocal, mountParent, null, clampHalfSize);
		}
		return true;
	}

	public static bool TryRegisterEncouragementLayoutFromWorldAnchor(RectTransform mountParent, Vector3 worldAnchor, Vector2 clampHalfSize, bool reserveUpperEmojiCompanionSpace)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!TryComputeEncouragementCenter(worldAnchor, mountParent, clampHalfSize, reserveUpperEmojiCompanionSpace, out var centerLocal))
		{
			return false;
		}
		RegisterPlacedEncouragement(mountParent, worldAnchor, centerLocal, clampHalfSize);
		return true;
	}

	public static bool TryApplyEmojiLayout(RectTransform emojiRoot, RectTransform mountParent, Vector3 worldPosition, Vector3 emojiPrefabScale, float emojiDisplayScale, Vector2 fallbackEncouragementClampHalfSize, float zOffset)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)emojiRoot == (Object)null || (Object)(object)mountParent == (Object)null)
		{
			return false;
		}
		if (TryResolveEncouragementCompanionForEmoji(worldPosition, mountParent, out var encouragementCenter))
		{
			return ApplyEmojiWithEncouragementCompanion(emojiRoot, mountParent, worldPosition, encouragementCenter, emojiPrefabScale, emojiDisplayScale, zOffset);
		}
		return ApplyEmojiWithoutEncouragement(emojiRoot, mountParent, worldPosition, emojiPrefabScale, emojiDisplayScale, zOffset);
	}

	public static bool TryApplyEmojiAlignedToEncouragementLeft(RectTransform emojiRoot, RectTransform mountParent, Vector3 worldPosition, Vector3 emojiPrefabScale, float emojiDisplayScale, Vector2 encouragementClampHalfSize, float zOffset)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return TryApplyEmojiLayout(emojiRoot, mountParent, worldPosition, emojiPrefabScale, emojiDisplayScale, encouragementClampHalfSize, zOffset);
	}

	private static bool TryResolveEncouragementCompanionForEmoji(Vector3 worldAnchor, RectTransform mountParent, out Vector2 encouragementCenter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		encouragementCenter = default(Vector2);
		if ((Object)(object)mountParent == (Object)null)
		{
			return false;
		}
		if (TryResolvePlacedEncouragement(worldAnchor, mountParent, out encouragementCenter, out var _))
		{
			return true;
		}
		return TryResolveLatestPlacedEncouragementOnMount(mountParent, out encouragementCenter);
	}

	private static bool TryResolveLatestPlacedEncouragementOnMount(RectTransform mountParent, out Vector2 encouragementCenter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		encouragementCenter = default(Vector2);
		PrunePlacedEncouragements();
		for (int num = placedEncouragements.Count - 1; num >= 0; num--)
		{
			PlacedEncouragementEntry placedEncouragementEntry = placedEncouragements[num];
			if (!((Object)(object)placedEncouragementEntry.MountParent != (Object)(object)mountParent))
			{
				encouragementCenter = placedEncouragementEntry.CenterLocal;
				return true;
			}
		}
		return false;
	}

	private static bool ApplyEmojiWithEncouragementCompanion(RectTransform emojiRoot, RectTransform mountParent, Vector3 worldPosition, Vector2 encouragementCenter, Vector3 emojiPrefabScale, float emojiDisplayScale, float zOffset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!TryComputeStandaloneEmojiPosition(worldPosition, mountParent, emojiRoot, emojiPrefabScale, emojiDisplayScale, out var emojiPosition))
		{
			return false;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(emojiPosition.x, encouragementCenter.y + 120f);
		((Transform)emojiRoot).localRotation = Quaternion.identity;
		((Transform)emojiRoot).localPosition = new Vector3(val.x, val.y, zOffset);
		return true;
	}

	private static bool ApplyEmojiWithoutEncouragement(RectTransform emojiRoot, RectTransform mountParent, Vector3 worldPosition, Vector3 emojiPrefabScale, float emojiDisplayScale, float zOffset)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!TryComputeStandaloneEmojiPosition(worldPosition, mountParent, emojiRoot, emojiPrefabScale, emojiDisplayScale, out var emojiPosition))
		{
			return false;
		}
		((Transform)emojiRoot).localRotation = Quaternion.identity;
		((Transform)emojiRoot).localPosition = new Vector3(emojiPosition.x, emojiPosition.y, zOffset);
		return true;
	}

	private static bool TryComputeStandaloneEmojiPosition(Vector3 worldPosition, RectTransform mountParent, RectTransform emojiRoot, Vector3 emojiPrefabScale, float emojiDisplayScale, out Vector2 emojiPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		emojiPosition = default(Vector2);
		if ((Object)(object)emojiRoot == (Object)null || (Object)(object)mountParent == (Object)null || !WorldToUIPostionUtil.WorldToLocalPoint(worldPosition, mountParent, out var localPoint))
		{
			return false;
		}
		emojiPosition = localPoint + new Vector2(0f, 0f);
		((Transform)emojiRoot).localScale = emojiPrefabScale * emojiDisplayScale;
		((Transform)emojiRoot).localPosition = new Vector3(emojiPosition.x, emojiPosition.y, 0f);
		Canvas.ForceUpdateCanvases();
		Bounds val = RectTransformUtility.CalculateRelativeRectTransformBounds((Transform)(object)mountParent, (Transform)(object)emojiRoot);
		Vector2 effectHalfSize = default(Vector2);
		((Vector2)(ref effectHalfSize))._002Ector(((Bounds)(ref val)).extents.x, ((Bounds)(ref val)).extents.y);
		emojiPosition = UIGameRingRectUiClamp.ClampEncouragement(emojiPosition, mountParent, null, effectHalfSize);
		return true;
	}

	private static void PrunePlacedEncouragements()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		for (int num = placedEncouragements.Count - 1; num >= 0; num--)
		{
			if (realtimeSinceStartup - placedEncouragements[num].RegisteredTime > 3f)
			{
				placedEncouragements.RemoveAt(num);
			}
		}
	}
}
