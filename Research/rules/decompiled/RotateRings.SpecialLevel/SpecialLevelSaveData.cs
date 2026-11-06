using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.SpecialLevel;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class SpecialLevelSaveData : ISaveDefaultInitializer, IMemoryPackable<SpecialLevelSaveData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SpecialLevelSaveDataFormatter : MemoryPackFormatter<SpecialLevelSaveData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SpecialLevelSaveData value)
		{
			SpecialLevelSaveData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SpecialLevelSaveData value)
		{
			SpecialLevelSaveData.Deserialize(ref reader, ref value);
		}
	}

	public string LastInsertDateKey;

	public string SessionDateKey;

	public int ActivityDayIndex;

	public int BaseGameMode;

	public bool IsLevelCompleted;

	public void OnCreateDefault()
	{
		LastInsertDateKey = string.Empty;
		SessionDateKey = string.Empty;
		ActivityDayIndex = 0;
		BaseGameMode = 0;
		IsLevelCompleted = false;
	}

	static SpecialLevelSaveData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SpecialLevelSaveData>())
		{
			MemoryPackFormatterProvider.Register<SpecialLevelSaveData>((MemoryPackFormatter<SpecialLevelSaveData>)(object)new SpecialLevelSaveDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SpecialLevelSaveData[]>())
		{
			MemoryPackFormatterProvider.Register<SpecialLevelSaveData[]>((MemoryPackFormatter<SpecialLevelSaveData[]>)(object)new ArrayFormatter<SpecialLevelSaveData>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SpecialLevelSaveData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)5);
		((MemoryPackWriter)(ref writer)).WriteString(value.LastInsertDateKey);
		((MemoryPackWriter)(ref writer)).WriteString(value.SessionDateKey);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int, bool>(ref value.ActivityDayIndex, ref value.BaseGameMode, ref value.IsLevelCompleted);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SpecialLevelSaveData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int activityDayIndex = default(int);
		int baseGameMode = default(int);
		bool isLevelCompleted = default(bool);
		string lastInsertDateKey;
		string sessionDateKey;
		if (b == 5)
		{
			if (value != null)
			{
				lastInsertDateKey = value.LastInsertDateKey;
				sessionDateKey = value.SessionDateKey;
				activityDayIndex = value.ActivityDayIndex;
				baseGameMode = value.BaseGameMode;
				isLevelCompleted = value.IsLevelCompleted;
				lastInsertDateKey = ((MemoryPackReader)(ref reader)).ReadString();
				sessionDateKey = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activityDayIndex);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref baseGameMode);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isLevelCompleted);
				goto IL_011d;
			}
			lastInsertDateKey = ((MemoryPackReader)(ref reader)).ReadString();
			sessionDateKey = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, bool>(ref activityDayIndex, ref baseGameMode, ref isLevelCompleted);
		}
		else
		{
			if (b > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SpecialLevelSaveData), (byte)5, b);
				return;
			}
			if (value == null)
			{
				lastInsertDateKey = null;
				sessionDateKey = null;
				activityDayIndex = 0;
				baseGameMode = 0;
				isLevelCompleted = false;
			}
			else
			{
				lastInsertDateKey = value.LastInsertDateKey;
				sessionDateKey = value.SessionDateKey;
				activityDayIndex = value.ActivityDayIndex;
				baseGameMode = value.BaseGameMode;
				isLevelCompleted = value.IsLevelCompleted;
			}
			if (b != 0)
			{
				lastInsertDateKey = ((MemoryPackReader)(ref reader)).ReadString();
				if (b != 1)
				{
					sessionDateKey = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref activityDayIndex);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref baseGameMode);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref isLevelCompleted);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_011d;
			}
		}
		value = new SpecialLevelSaveData
		{
			LastInsertDateKey = lastInsertDateKey,
			SessionDateKey = sessionDateKey,
			ActivityDayIndex = activityDayIndex,
			BaseGameMode = baseGameMode,
			IsLevelCompleted = isLevelCompleted
		};
		return;
		IL_011d:
		value.LastInsertDateKey = lastInsertDateKey;
		value.SessionDateKey = sessionDateKey;
		value.ActivityDayIndex = activityDayIndex;
		value.BaseGameMode = baseGameMode;
		value.IsLevelCompleted = isLevelCompleted;
	}
}
