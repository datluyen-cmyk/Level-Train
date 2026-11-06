using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class ShopConfig : IMemoryPackable<ShopConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ShopConfigFormatter : MemoryPackFormatter<ShopConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ShopConfig value)
		{
			ShopConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ShopConfig value)
		{
			ShopConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int ID { get; private set; }

	[MemoryPackOrder(1)]
	public int giftType { get; private set; }

	[MemoryPackOrder(2)]
	public int tag { get; private set; }

	[MemoryPackOrder(3)]
	public string price { get; private set; }

	[MemoryPackOrder(4)]
	public bool NonConsumption { get; private set; }

	[MemoryPackOrder(5)]
	public int discount { get; private set; }

	[MemoryPackOrder(6)]
	public int objectType { get; private set; }

	[MemoryPackOrder(7)]
	public bool showItem { get; private set; }

	[MemoryPackOrder(8)]
	public string giftIconName { get; private set; }

	static ShopConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ShopConfig>())
		{
			MemoryPackFormatterProvider.Register<ShopConfig>((MemoryPackFormatter<ShopConfig>)(object)new ShopConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ShopConfig[]>())
		{
			MemoryPackFormatterProvider.Register<ShopConfig[]>((MemoryPackFormatter<ShopConfig[]>)(object)new ArrayFormatter<ShopConfig>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ShopConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int iD = value.ID;
		int num = value.giftType;
		int num2 = value.tag;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int, int>((byte)9, ref iD, ref num, ref num2);
		((MemoryPackWriter)(ref writer)).WriteString(value.price);
		bool nonConsumption = value.NonConsumption;
		iD = value.discount;
		num = value.objectType;
		bool flag = value.showItem;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<bool, int, int, bool>(ref nonConsumption, ref iD, ref num, ref flag);
		((MemoryPackWriter)(ref writer)).WriteString(value.giftIconName);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ShopConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int iD = default(int);
		int num = default(int);
		int num2 = default(int);
		bool nonConsumption = default(bool);
		int num3 = default(int);
		int num4 = default(int);
		bool flag = default(bool);
		string text;
		string text2;
		if (b == 9)
		{
			if (value != null)
			{
				iD = value.ID;
				num = value.giftType;
				num2 = value.tag;
				text = value.price;
				nonConsumption = value.NonConsumption;
				num3 = value.discount;
				num4 = value.objectType;
				flag = value.showItem;
				text2 = value.giftIconName;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num2);
				text = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref nonConsumption);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num3);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num4);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref flag);
				text2 = ((MemoryPackReader)(ref reader)).ReadString();
				goto IL_01dc;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int, int>(ref iD, ref num, ref num2);
			text = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<bool, int, int, bool>(ref nonConsumption, ref num3, ref num4, ref flag);
			text2 = ((MemoryPackReader)(ref reader)).ReadString();
		}
		else
		{
			if (b > 9)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ShopConfig), (byte)9, b);
				return;
			}
			if (value == null)
			{
				iD = 0;
				num = 0;
				num2 = 0;
				text = null;
				nonConsumption = false;
				num3 = 0;
				num4 = 0;
				flag = false;
				text2 = null;
			}
			else
			{
				iD = value.ID;
				num = value.giftType;
				num2 = value.tag;
				text = value.price;
				nonConsumption = value.NonConsumption;
				num3 = value.discount;
				num4 = value.objectType;
				flag = value.showItem;
				text2 = value.giftIconName;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref iD);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num2);
						if (b != 3)
						{
							text = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref nonConsumption);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num3);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref num4);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<bool>(ref flag);
											if (b != 8)
											{
												text2 = ((MemoryPackReader)(ref reader)).ReadString();
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
				goto IL_01dc;
			}
		}
		value = new ShopConfig
		{
			ID = iD,
			giftType = num,
			tag = num2,
			price = text,
			NonConsumption = nonConsumption,
			discount = num3,
			objectType = num4,
			showItem = flag,
			giftIconName = text2
		};
		return;
		IL_01dc:
		value.ID = iD;
		value.giftType = num;
		value.tag = num2;
		value.price = text;
		value.NonConsumption = nonConsumption;
		value.discount = num3;
		value.objectType = num4;
		value.showItem = flag;
		value.giftIconName = text2;
	}
}
