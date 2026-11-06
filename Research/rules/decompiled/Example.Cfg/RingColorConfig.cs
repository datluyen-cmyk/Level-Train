using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using RotateRings.GamePlay.PartStyle;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class RingColorConfig : IMemoryPackable<RingColorConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RingColorConfigFormatter : MemoryPackFormatter<RingColorConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RingColorConfig value)
		{
			RingColorConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RingColorConfig value)
		{
			RingColorConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int SchemeId { get; private set; }

	[MemoryPackOrder(1)]
	public PartStyleType StyleId { get; private set; }

	[MemoryPackOrder(2)]
	public string Color1 { get; private set; }

	[MemoryPackOrder(3)]
	public string Color2 { get; private set; }

	[MemoryPackOrder(4)]
	public string Color3 { get; private set; }

	[MemoryPackOrder(5)]
	public string Color4 { get; private set; }

	[MemoryPackOrder(6)]
	public string Color5 { get; private set; }

	[MemoryPackOrder(7)]
	public string Color6 { get; private set; }

	[MemoryPackOrder(8)]
	public string Color7 { get; private set; }

	[MemoryPackOrder(9)]
	public float Saturation { get; private set; }

	static RingColorConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RingColorConfig>())
		{
			MemoryPackFormatterProvider.Register<RingColorConfig>((MemoryPackFormatter<RingColorConfig>)(object)new RingColorConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RingColorConfig[]>())
		{
			MemoryPackFormatterProvider.Register<RingColorConfig[]>((MemoryPackFormatter<RingColorConfig[]>)(object)new ArrayFormatter<RingColorConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartStyleType>())
		{
			MemoryPackFormatterProvider.Register<PartStyleType>((MemoryPackFormatter<PartStyleType>)(object)new UnmanagedFormatter<PartStyleType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RingColorConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int schemeId = value.SchemeId;
		PartStyleType styleId = value.StyleId;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, PartStyleType>((byte)10, ref schemeId, ref styleId);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color1);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color2);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color3);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color4);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color5);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color6);
		((MemoryPackWriter)(ref writer)).WriteString(value.Color7);
		float saturation = value.Saturation;
		((MemoryPackWriter)(ref writer)).WriteUnmanaged<float>(ref saturation);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RingColorConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int schemeId = default(int);
		PartStyleType styleId = default(PartStyleType);
		float saturation = default(float);
		string color;
		string color2;
		string color3;
		string color4;
		string color5;
		string color6;
		string color7;
		if (b == 10)
		{
			if (value != null)
			{
				schemeId = value.SchemeId;
				styleId = value.StyleId;
				color = value.Color1;
				color2 = value.Color2;
				color3 = value.Color3;
				color4 = value.Color4;
				color5 = value.Color5;
				color6 = value.Color6;
				color7 = value.Color7;
				saturation = value.Saturation;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref schemeId);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<PartStyleType>(ref styleId);
				color = ((MemoryPackReader)(ref reader)).ReadString();
				color2 = ((MemoryPackReader)(ref reader)).ReadString();
				color3 = ((MemoryPackReader)(ref reader)).ReadString();
				color4 = ((MemoryPackReader)(ref reader)).ReadString();
				color5 = ((MemoryPackReader)(ref reader)).ReadString();
				color6 = ((MemoryPackReader)(ref reader)).ReadString();
				color7 = ((MemoryPackReader)(ref reader)).ReadString();
				((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref saturation);
				goto IL_0227;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, PartStyleType>(ref schemeId, ref styleId);
			color = ((MemoryPackReader)(ref reader)).ReadString();
			color2 = ((MemoryPackReader)(ref reader)).ReadString();
			color3 = ((MemoryPackReader)(ref reader)).ReadString();
			color4 = ((MemoryPackReader)(ref reader)).ReadString();
			color5 = ((MemoryPackReader)(ref reader)).ReadString();
			color6 = ((MemoryPackReader)(ref reader)).ReadString();
			color7 = ((MemoryPackReader)(ref reader)).ReadString();
			((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref saturation);
		}
		else
		{
			if (b > 10)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RingColorConfig), (byte)10, b);
				return;
			}
			if (value == null)
			{
				schemeId = 0;
				styleId = PartStyleType.MatteFaceted;
				color = null;
				color2 = null;
				color3 = null;
				color4 = null;
				color5 = null;
				color6 = null;
				color7 = null;
				saturation = 0f;
			}
			else
			{
				schemeId = value.SchemeId;
				styleId = value.StyleId;
				color = value.Color1;
				color2 = value.Color2;
				color3 = value.Color3;
				color4 = value.Color4;
				color5 = value.Color5;
				color6 = value.Color6;
				color7 = value.Color7;
				saturation = value.Saturation;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref schemeId);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<PartStyleType>(ref styleId);
					if (b != 2)
					{
						color = ((MemoryPackReader)(ref reader)).ReadString();
						if (b != 3)
						{
							color2 = ((MemoryPackReader)(ref reader)).ReadString();
							if (b != 4)
							{
								color3 = ((MemoryPackReader)(ref reader)).ReadString();
								if (b != 5)
								{
									color4 = ((MemoryPackReader)(ref reader)).ReadString();
									if (b != 6)
									{
										color5 = ((MemoryPackReader)(ref reader)).ReadString();
										if (b != 7)
										{
											color6 = ((MemoryPackReader)(ref reader)).ReadString();
											if (b != 8)
											{
												color7 = ((MemoryPackReader)(ref reader)).ReadString();
												if (b != 9)
												{
													((MemoryPackReader)(ref reader)).ReadUnmanaged<float>(ref saturation);
													_ = 10;
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
				goto IL_0227;
			}
		}
		value = new RingColorConfig
		{
			SchemeId = schemeId,
			StyleId = styleId,
			Color1 = color,
			Color2 = color2,
			Color3 = color3,
			Color4 = color4,
			Color5 = color5,
			Color6 = color6,
			Color7 = color7,
			Saturation = saturation
		};
		return;
		IL_0227:
		value.SchemeId = schemeId;
		value.StyleId = styleId;
		value.Color1 = color;
		value.Color2 = color2;
		value.Color3 = color3;
		value.Color4 = color4;
		value.Color5 = color5;
		value.Color6 = color6;
		value.Color7 = color7;
		value.Saturation = saturation;
	}
}
