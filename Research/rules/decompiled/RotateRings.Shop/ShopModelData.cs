using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using XingYun.Save;

namespace RotateRings.Shop;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class ShopModelData : ISaveDefaultInitializer, IMemoryPackable<ShopModelData>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ShopModelDataFormatter : MemoryPackFormatter<ShopModelData>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ShopModelData value)
		{
			ShopModelData.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ShopModelData value)
		{
			ShopModelData.Deserialize(ref reader, ref value);
		}
	}

	public Dictionary<int, int> GoodsBuyCount;

	public bool HasShopEntryLevelState;

	public int ShopEntryMode;

	public int ShopEntryLevel;

	public int ShopEntryConfigValue;

	public bool ShopEntryConditionMet;

	public int ShopEntryRewardAdCompletedCount;

	public long ShopEntryLevelStartedUtcTicks;

	public long ShopEntryLastOperationUtcTicks;

	public void OnCreateDefault()
	{
		GoodsBuyCount = new Dictionary<int, int>();
		HasShopEntryLevelState = false;
		ShopEntryMode = 0;
		ShopEntryLevel = 0;
		ShopEntryConfigValue = 0;
		ShopEntryConditionMet = false;
		ShopEntryRewardAdCompletedCount = 0;
		ShopEntryLevelStartedUtcTicks = 0L;
		ShopEntryLastOperationUtcTicks = 0L;
	}

	static ShopModelData()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ShopModelData>())
		{
			MemoryPackFormatterProvider.Register<ShopModelData>((MemoryPackFormatter<ShopModelData>)(object)new ShopModelDataFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ShopModelData[]>())
		{
			MemoryPackFormatterProvider.Register<ShopModelData[]>((MemoryPackFormatter<ShopModelData[]>)(object)new ArrayFormatter<ShopModelData>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<int, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<int, int>>((MemoryPackFormatter<Dictionary<int, int>>)(object)new DictionaryFormatter<int, int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ShopModelData? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		((MemoryPackWriter)(ref writer)).WriteObjectHeader((byte)9);
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<int, int>>(ref value.GoodsBuyCount);
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<bool, int, int, int, bool, int, long, long>(ref value.HasShopEntryLevelState, ref value.ShopEntryMode, ref value.ShopEntryLevel, ref value.ShopEntryConfigValue, ref value.ShopEntryConditionMet, ref value.ShopEntryRewardAdCompletedCount, ref value.ShopEntryLevelStartedUtcTicks, ref value.ShopEntryLastOperationUtcTicks);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ShopModelData? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		Dictionary<int, int> goodsBuyCount;
		bool hasShopEntryLevelState = default(bool);
		int shopEntryMode = default(int);
		int shopEntryLevel = default(int);
		int shopEntryConfigValue = default(int);
		bool shopEntryConditionMet = default(bool);
		int shopEntryRewardAdCompletedCount = default(int);
		long shopEntryLevelStartedUtcTicks = default(long);
		long shopEntryLastOperationUtcTicks = default(long);
		if (b == 9)
		{
			if (value != null)
			{
				goodsBuyCount = value.GoodsBuyCount;
				hasShopEntryLevelState = value.HasShopEntryLevelState;
				shopEntryMode = value.ShopEntryMode;
				shopEntryLevel = value.ShopEntryLevel;
				shopEntryConfigValue = value.ShopEntryConfigValue;
				shopEntryConditionMet = value.ShopEntryConditionMet;
				shopEntryRewardAdCompletedCount = value.ShopEntryRewardAdCompletedCount;
				shopEntryLevelStartedUtcTicks = value.ShopEntryLevelStartedUtcTicks;
				shopEntryLastOperationUtcTicks = value.ShopEntryLastOperationUtcTicks;
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref goodsBuyCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasShopEntryLevelState);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryMode);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryLevel);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryConfigValue);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref shopEntryConditionMet);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryRewardAdCompletedCount);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref shopEntryLevelStartedUtcTicks);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref shopEntryLastOperationUtcTicks);
				goto IL_01d1;
			}
			goodsBuyCount = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, int, int, int, bool, int, long, long>(ref hasShopEntryLevelState, ref shopEntryMode, ref shopEntryLevel, ref shopEntryConfigValue, ref shopEntryConditionMet, ref shopEntryRewardAdCompletedCount, ref shopEntryLevelStartedUtcTicks, ref shopEntryLastOperationUtcTicks);
		}
		else
		{
			if (b > 9)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ShopModelData), (byte)9, b);
				return;
			}
			if (value == null)
			{
				goodsBuyCount = null;
				hasShopEntryLevelState = false;
				shopEntryMode = 0;
				shopEntryLevel = 0;
				shopEntryConfigValue = 0;
				shopEntryConditionMet = false;
				shopEntryRewardAdCompletedCount = 0;
				shopEntryLevelStartedUtcTicks = 0L;
				shopEntryLastOperationUtcTicks = 0L;
			}
			else
			{
				goodsBuyCount = value.GoodsBuyCount;
				hasShopEntryLevelState = value.HasShopEntryLevelState;
				shopEntryMode = value.ShopEntryMode;
				shopEntryLevel = value.ShopEntryLevel;
				shopEntryConfigValue = value.ShopEntryConfigValue;
				shopEntryConditionMet = value.ShopEntryConditionMet;
				shopEntryRewardAdCompletedCount = value.ShopEntryRewardAdCompletedCount;
				shopEntryLevelStartedUtcTicks = value.ShopEntryLevelStartedUtcTicks;
				shopEntryLastOperationUtcTicks = value.ShopEntryLastOperationUtcTicks;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref goodsBuyCount);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref hasShopEntryLevelState);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryMode);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryLevel);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryConfigValue);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref shopEntryConditionMet);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref shopEntryRewardAdCompletedCount);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref shopEntryLevelStartedUtcTicks);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<long>(ref shopEntryLastOperationUtcTicks);
												_ = 9;
											}
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
				goto IL_01d1;
			}
		}
		value = new ShopModelData
		{
			GoodsBuyCount = goodsBuyCount,
			HasShopEntryLevelState = hasShopEntryLevelState,
			ShopEntryMode = shopEntryMode,
			ShopEntryLevel = shopEntryLevel,
			ShopEntryConfigValue = shopEntryConfigValue,
			ShopEntryConditionMet = shopEntryConditionMet,
			ShopEntryRewardAdCompletedCount = shopEntryRewardAdCompletedCount,
			ShopEntryLevelStartedUtcTicks = shopEntryLevelStartedUtcTicks,
			ShopEntryLastOperationUtcTicks = shopEntryLastOperationUtcTicks
		};
		return;
		IL_01d1:
		value.GoodsBuyCount = goodsBuyCount;
		value.HasShopEntryLevelState = hasShopEntryLevelState;
		value.ShopEntryMode = shopEntryMode;
		value.ShopEntryLevel = shopEntryLevel;
		value.ShopEntryConfigValue = shopEntryConfigValue;
		value.ShopEntryConditionMet = shopEntryConditionMet;
		value.ShopEntryRewardAdCompletedCount = shopEntryRewardAdCompletedCount;
		value.ShopEntryLevelStartedUtcTicks = shopEntryLevelStartedUtcTicks;
		value.ShopEntryLastOperationUtcTicks = shopEntryLastOperationUtcTicks;
	}
}
