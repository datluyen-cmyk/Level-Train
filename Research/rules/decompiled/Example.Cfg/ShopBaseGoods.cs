using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class ShopBaseGoods : IMemoryPackable<ShopBaseGoods>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ShopBaseGoodsFormatter : MemoryPackFormatter<ShopBaseGoods>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ShopBaseGoods value)
		{
			ShopBaseGoods.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ShopBaseGoods value)
		{
			ShopBaseGoods.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public int ExclusionId { get; private set; }

	[MemoryPackOrder(2)]
	public string GPStoreId { get; private set; }

	[MemoryPackOrder(3)]
	public string IOSStoreId { get; private set; }

	[MemoryPackOrder(4)]
	public string TestGPStoreId { get; private set; }

	[MemoryPackOrder(5)]
	public string TestIOSStoreId { get; private set; }

	[MemoryPackOrder(6)]
	public Dictionary<int, int> BuyItems { get; private set; }

	[MemoryPackOrder(7)]
	public int LimitedCount { get; private set; }

	static ShopBaseGoods()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ShopBaseGoods>())
		{
			MemoryPackFormatterProvider.Register<ShopBaseGoods>((MemoryPackFormatter<ShopBaseGoods>)(object)new ShopBaseGoodsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ShopBaseGoods[]>())
		{
			MemoryPackFormatterProvider.Register<ShopBaseGoods[]>((MemoryPackFormatter<ShopBaseGoods[]>)(object)new ArrayFormatter<ShopBaseGoods>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Dictionary<int, int>>())
		{
			MemoryPackFormatterProvider.Register<Dictionary<int, int>>((MemoryPackFormatter<Dictionary<int, int>>)(object)new DictionaryFormatter<int, int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ShopBaseGoods? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		int exclusionId = value.ExclusionId;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)8, ref iD, ref exclusionId);
		((MemoryPackWriter)(ref writer)).WriteString(value.GPStoreId);
		((MemoryPackWriter)(ref writer)).WriteString(value.IOSStoreId);
		((MemoryPackWriter)(ref writer)).WriteString(value.TestGPStoreId);
		((MemoryPackWriter)(ref writer)).WriteString(value.TestIOSStoreId);
		Dictionary<int, int> buyItems = value.BuyItems;
		((MemoryPackWriter)(ref writer)).WriteValue<Dictionary<int, int>>(ref buyItems);
		iD = value.LimitedCount;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref iD);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ShopBaseGoods? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		int exclusionId = default(int);
		Dictionary<int, int> buyItems;
		int limitedCount = default(int);
		string gPStoreId;
		string iOSStoreId;
		string testGPStoreId;
		string testIOSStoreId;
		if (b == 8)
		{
			if (value != null)
			{
				iD = value.ID;
				exclusionId = value.ExclusionId;
				gPStoreId = value.GPStoreId;
				iOSStoreId = value.IOSStoreId;
				testGPStoreId = value.TestGPStoreId;
				testIOSStoreId = value.TestIOSStoreId;
				buyItems = value.BuyItems;
				limitedCount = value.LimitedCount;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exclusionId);
				gPStoreId = ((MemoryPackReader)(ref reader)).ReadString();
				iOSStoreId = ((MemoryPackReader)(ref reader)).ReadString();
				testGPStoreId = ((MemoryPackReader)(ref reader)).ReadString();
				testIOSStoreId = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref buyItems);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref limitedCount);
				goto IL_01bc;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref iD, ref exclusionId);
			gPStoreId = ((MemoryPackReader)(ref reader)).ReadString();
			iOSStoreId = ((MemoryPackReader)(ref reader)).ReadString();
			testGPStoreId = ((MemoryPackReader)(ref reader)).ReadString();
			testIOSStoreId = ((MemoryPackReader)(ref reader)).ReadString();
			buyItems = ((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref limitedCount);
		}
		else
		{
			if (b > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ShopBaseGoods), (byte)8, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				exclusionId = 0;
				gPStoreId = null;
				iOSStoreId = null;
				testGPStoreId = null;
				testIOSStoreId = null;
				buyItems = null;
				limitedCount = 0;
			}
			else
			{
				iD = value.ID;
				exclusionId = value.ExclusionId;
				gPStoreId = value.GPStoreId;
				iOSStoreId = value.IOSStoreId;
				testGPStoreId = value.TestGPStoreId;
				testIOSStoreId = value.TestIOSStoreId;
				buyItems = value.BuyItems;
				limitedCount = value.LimitedCount;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref exclusionId);
					if (b != 2)
					{
						gPStoreId = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							iOSStoreId = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								testGPStoreId = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									testIOSStoreId = ((MemoryPackReader)(ref reader)).ReadString();
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<Dictionary<int, int>>(ref buyItems);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref limitedCount);
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
				goto IL_01bc;
			}
		}
		value = new ShopBaseGoods
		{
			ID = iD,
			ExclusionId = exclusionId,
			GPStoreId = gPStoreId,
			IOSStoreId = iOSStoreId,
			TestGPStoreId = testGPStoreId,
			TestIOSStoreId = testIOSStoreId,
			BuyItems = buyItems,
			LimitedCount = limitedCount
		};
		return;
		IL_01bc:
		value.ID = iD;
		value.ExclusionId = exclusionId;
		value.GPStoreId = gPStoreId;
		value.IOSStoreId = iOSStoreId;
		value.TestGPStoreId = testGPStoreId;
		value.TestIOSStoreId = testIOSStoreId;
		value.BuyItems = buyItems;
		value.LimitedCount = limitedCount;
	}
}
