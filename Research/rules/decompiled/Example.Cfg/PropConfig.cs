using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class PropConfig : IMemoryPackable<PropConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PropConfigFormatter : MemoryPackFormatter<PropConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PropConfig value)
		{
			PropConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PropConfig value)
		{
			PropConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public string ScriptField { get; private set; }

	[MemoryPackOrder(2)]
	public string Note { get; private set; }

	[MemoryPackOrder(3)]
	public int CoinValue { get; private set; }

	[MemoryPackOrder(4)]
	public string propName { get; private set; }

	[MemoryPackOrder(5)]
	public string Icon { get; private set; }

	[MemoryPackOrder(6)]
	public string IconLarge { get; private set; }

	[MemoryPackOrder(7)]
	public int UnlockInitNum { get; private set; }

	[MemoryPackOrder(8)]
	public int UnlockLv { get; private set; }

	[MemoryPackOrder(9)]
	public string GuideKey { get; private set; }

	[MemoryPackOrder(10)]
	public List<string> GuideDescKeys { get; private set; }

	static PropConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PropConfig>())
		{
			MemoryPackFormatterProvider.Register<PropConfig>((MemoryPackFormatter<PropConfig>)(object)new PropConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PropConfig[]>())
		{
			MemoryPackFormatterProvider.Register<PropConfig[]>((MemoryPackFormatter<PropConfig[]>)(object)new ArrayFormatter<PropConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<string>>())
		{
			MemoryPackFormatterProvider.Register<List<string>>((MemoryPackFormatter<List<string>>)(object)new ListFormatter<string>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PropConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int>((byte)11, ref id);
		((MemoryPackWriter)(ref writer)).WriteString(value.ScriptField);
		((MemoryPackWriter)(ref writer)).WriteString(value.Note);
		id = value.CoinValue;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int>(ref id);
		((MemoryPackWriter)(ref writer)).WriteString(value.propName);
		((MemoryPackWriter)(ref writer)).WriteString(value.Icon);
		((MemoryPackWriter)(ref writer)).WriteString(value.IconLarge);
		id = value.UnlockInitNum;
		int unlockLv = value.UnlockLv;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<int, int>(ref id, ref unlockLv);
		((MemoryPackWriter)(ref writer)).WriteString(value.GuideKey);
		List<string> guideDescKeys = value.GuideDescKeys;
		((MemoryPackWriter)(ref writer)).WriteValue<List<string>>(ref guideDescKeys);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PropConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int coinValue = default(int);
		int unlockInitNum = default(int);
		int unlockLv = default(int);
		List<string> guideDescKeys;
		string scriptField;
		string note;
		string text;
		string icon;
		string iconLarge;
		string guideKey;
		if (b == 11)
		{
			if (value != null)
			{
				id = value.Id;
				scriptField = value.ScriptField;
				note = value.Note;
				coinValue = value.CoinValue;
				text = value.propName;
				icon = value.Icon;
				iconLarge = value.IconLarge;
				unlockInitNum = value.UnlockInitNum;
				unlockLv = value.UnlockLv;
				guideKey = value.GuideKey;
				guideDescKeys = value.GuideDescKeys;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				scriptField = ((MemoryPackReader)(ref reader)).ReadString();
				note = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref coinValue);
				text = ((MemoryPackReader)(ref reader)).ReadString();
				icon = ((MemoryPackReader)(ref reader)).ReadString();
				iconLarge = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockInitNum);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockLv);
				guideKey = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref guideDescKeys);
				goto IL_0255;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
			scriptField = ((MemoryPackReader)(ref reader)).ReadString();
			note = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref coinValue);
			text = ((MemoryPackReader)(ref reader)).ReadString();
			icon = ((MemoryPackReader)(ref reader)).ReadString();
			iconLarge = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref unlockInitNum, ref unlockLv);
			guideKey = ((MemoryPackReader)(ref reader)).ReadString();
			guideDescKeys = ((MemoryPackReader)(ref reader)).ReadValue<List<string>>();
		}
		else
		{
			if (b > 11)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PropConfig), (byte)11, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				scriptField = null;
				note = null;
				coinValue = 0;
				text = null;
				icon = null;
				iconLarge = null;
				unlockInitNum = 0;
				unlockLv = 0;
				guideKey = null;
				guideDescKeys = null;
			}
			else
			{
				id = value.Id;
				scriptField = value.ScriptField;
				note = value.Note;
				coinValue = value.CoinValue;
				text = value.propName;
				icon = value.Icon;
				iconLarge = value.IconLarge;
				unlockInitNum = value.UnlockInitNum;
				unlockLv = value.UnlockLv;
				guideKey = value.GuideKey;
				guideDescKeys = value.GuideDescKeys;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					scriptField = ((MemoryPackReader)(ref reader)).ReadString();
					if (b != 2)
					{
						note = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref coinValue);
							if (b != 4)
							{
								text = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									icon = ((MemoryPackReader)(ref reader)).ReadString();
									if (b != 6)
									{
										iconLarge = ((MemoryPackReader)(ref reader)).ReadString();
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockInitNum);
											if (b != 8)
											{
												((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref unlockLv);
												if (b != 9)
												{
													guideKey = ((MemoryPackReader)(ref reader)).ReadString();
													if (b != 10)
													{
														((MemoryPackReader)(ref reader)).ReadValue<List<string>>(ref guideDescKeys);
														_ = 11;
													}
												}
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
				goto IL_0255;
			}
		}
		value = new PropConfig
		{
			Id = id,
			ScriptField = scriptField,
			Note = note,
			CoinValue = coinValue,
			propName = text,
			Icon = icon,
			IconLarge = iconLarge,
			UnlockInitNum = unlockInitNum,
			UnlockLv = unlockLv,
			GuideKey = guideKey,
			GuideDescKeys = guideDescKeys
		};
		return;
		IL_0255:
		value.Id = id;
		value.ScriptField = scriptField;
		value.Note = note;
		value.CoinValue = coinValue;
		value.propName = text;
		value.Icon = icon;
		value.IconLarge = iconLarge;
		value.UnlockInitNum = unlockInitNum;
		value.UnlockLv = unlockLv;
		value.GuideKey = guideKey;
		value.GuideDescKeys = guideDescKeys;
	}
}
