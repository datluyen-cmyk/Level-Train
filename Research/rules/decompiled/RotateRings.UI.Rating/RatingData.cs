using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.UI.Rating;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RatingData : ISaveDefaultInitializer, IMemoryPackable<RatingData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RatingDataFormatter : MemoryPackFormatter<RatingData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RatingData value)
		{
			RatingData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RatingData value)
		{
			RatingData.Deserialize(ref reader, ref value);
		}
	}

	public int PopupCount;

	public bool HasGivenHighStar;

	public int FirstPopupAction;

	public int CloseCount;

	public int SubmitLowStarCount;

	public int LastActionActiveDays;

	public float LastPopupPassTime;

	public int LastActionType;

	public void OnCreateDefault()
	{
		PopupCount = 0;
		HasGivenHighStar = false;
		FirstPopupAction = 0;
		CloseCount = 0;
		SubmitLowStarCount = 0;
		LastActionActiveDays = 0;
		LastPopupPassTime = 0f;
		LastActionType = 0;
	}

	static RatingData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RatingData>())
		{
			MemoryPackFormatterProvider.Register<RatingData>((MemoryPackFormatter<RatingData>)(object)new RatingDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RatingData[]>())
		{
			MemoryPackFormatterProvider.Register<RatingData[]>((MemoryPackFormatter<RatingData[]>)(object)new ArrayFormatter<RatingData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RatingData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
		}
		else
		{
			((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, bool, int, int, int, int, float, int>((byte)8, ref value.PopupCount, ref value.HasGivenHighStar, ref value.FirstPopupAction, ref value.CloseCount, ref value.SubmitLowStarCount, ref value.LastActionActiveDays, ref value.LastPopupPassTime, ref value.LastActionType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RatingData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int popupCount = default(int);
		bool hasGivenHighStar = default(bool);
		int firstPopupAction = default(int);
		int closeCount = default(int);
		int submitLowStarCount = default(int);
		int lastActionActiveDays = default(int);
		float lastPopupPassTime = default(float);
		int lastActionType = default(int);
		if (b == 8)
		{
			if (value != null)
			{
				popupCount = value.PopupCount;
				hasGivenHighStar = value.HasGivenHighStar;
				firstPopupAction = value.FirstPopupAction;
				closeCount = value.CloseCount;
				submitLowStarCount = value.SubmitLowStarCount;
				lastActionActiveDays = value.LastActionActiveDays;
				lastPopupPassTime = value.LastPopupPassTime;
				lastActionType = value.LastActionType;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref popupCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasGivenHighStar);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref firstPopupAction);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref closeCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref submitLowStarCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastActionActiveDays);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref lastPopupPassTime);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastActionType);
				goto IL_019f;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, bool, int, int, int, int, float, int>(ref popupCount, ref hasGivenHighStar, ref firstPopupAction, ref closeCount, ref submitLowStarCount, ref lastActionActiveDays, ref lastPopupPassTime, ref lastActionType);
		}
		else
		{
			if (b > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RatingData), (byte)8, b);
				return;
			}
			if (value == null)
			{
				popupCount = 0;
				hasGivenHighStar = false;
				firstPopupAction = 0;
				closeCount = 0;
				submitLowStarCount = 0;
				lastActionActiveDays = 0;
				lastPopupPassTime = 0f;
				lastActionType = 0;
			}
			else
			{
				popupCount = value.PopupCount;
				hasGivenHighStar = value.HasGivenHighStar;
				firstPopupAction = value.FirstPopupAction;
				closeCount = value.CloseCount;
				submitLowStarCount = value.SubmitLowStarCount;
				lastActionActiveDays = value.LastActionActiveDays;
				lastPopupPassTime = value.LastPopupPassTime;
				lastActionType = value.LastActionType;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref popupCount);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasGivenHighStar);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref firstPopupAction);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref closeCount);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref submitLowStarCount);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastActionActiveDays);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref lastPopupPassTime);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref lastActionType);
											_ = 8;
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_019f;
			}
		}
		value = new RatingData
		{
			PopupCount = popupCount,
			HasGivenHighStar = hasGivenHighStar,
			FirstPopupAction = firstPopupAction,
			CloseCount = closeCount,
			SubmitLowStarCount = submitLowStarCount,
			LastActionActiveDays = lastActionActiveDays,
			LastPopupPassTime = lastPopupPassTime,
			LastActionType = lastActionType
		};
		return;
		IL_019f:
		value.PopupCount = popupCount;
		value.HasGivenHighStar = hasGivenHighStar;
		value.FirstPopupAction = firstPopupAction;
		value.CloseCount = closeCount;
		value.SubmitLowStarCount = submitLowStarCount;
		value.LastActionActiveDays = lastActionActiveDays;
		value.LastPopupPassTime = lastPopupPassTime;
		value.LastActionType = lastActionType;
	}
}
