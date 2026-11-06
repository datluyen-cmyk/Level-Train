using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class OperationScoreConfig : IMemoryPackable<OperationScoreConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class OperationScoreConfigFormatter : MemoryPackFormatter<OperationScoreConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref OperationScoreConfig value)
		{
			OperationScoreConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref OperationScoreConfig value)
		{
			OperationScoreConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int GroupID { get; private set; }

	[MemoryPackOrder(2)]
	public List<int> UnlockNum { get; private set; }

	[MemoryPackOrder(3)]
	public List<int> UnlockNumScore { get; private set; }

	[MemoryPackOrder(4)]
	public List<int> StepNum { get; private set; }

	[MemoryPackOrder(5)]
	public List<int> StepNumScore { get; private set; }

	[MemoryPackOrder(6)]
	public List<int> ContinuousTopScore { get; private set; }

	static OperationScoreConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<OperationScoreConfig>())
		{
			MemoryPackFormatterProvider.Register<OperationScoreConfig>((MemoryPackFormatter<OperationScoreConfig>)(object)new OperationScoreConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<OperationScoreConfig[]>())
		{
			MemoryPackFormatterProvider.Register<OperationScoreConfig[]>((MemoryPackFormatter<OperationScoreConfig[]>)(object)new ArrayFormatter<OperationScoreConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref OperationScoreConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int groupID = value.GroupID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)7, ref id, ref groupID);
		List<int> unlockNum = value.UnlockNum;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref unlockNum);
		unlockNum = value.UnlockNumScore;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref unlockNum);
		unlockNum = value.StepNum;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref unlockNum);
		unlockNum = value.StepNumScore;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref unlockNum);
		unlockNum = value.ContinuousTopScore;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref unlockNum);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref OperationScoreConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int groupID = default(int);
		List<int> unlockNum;
		List<int> unlockNumScore;
		List<int> stepNum;
		List<int> stepNumScore;
		List<int> continuousTopScore;
		if (b == 7)
		{
			if (value != null)
			{
				id = value.Id;
				groupID = value.GroupID;
				unlockNum = value.UnlockNum;
				unlockNumScore = value.UnlockNumScore;
				stepNum = value.StepNum;
				stepNumScore = value.StepNumScore;
				continuousTopScore = value.ContinuousTopScore;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref groupID);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockNum);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockNumScore);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref stepNum);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref stepNumScore);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref continuousTopScore);
				goto IL_018d;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref id, ref groupID);
			unlockNum = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			unlockNumScore = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			stepNum = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			stepNumScore = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			continuousTopScore = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
		}
		else
		{
			if (b > 7)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(OperationScoreConfig), (byte)7, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				groupID = 0;
				unlockNum = null;
				unlockNumScore = null;
				stepNum = null;
				stepNumScore = null;
				continuousTopScore = null;
			}
			else
			{
				id = value.Id;
				groupID = value.GroupID;
				unlockNum = value.UnlockNum;
				unlockNumScore = value.UnlockNumScore;
				stepNum = value.StepNum;
				stepNumScore = value.StepNumScore;
				continuousTopScore = value.ContinuousTopScore;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref groupID);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockNum);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockNumScore);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref stepNum);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref stepNumScore);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref continuousTopScore);
										_ = 7;
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_018d;
			}
		}
		value = new OperationScoreConfig
		{
			Id = id,
			GroupID = groupID,
			UnlockNum = unlockNum,
			UnlockNumScore = unlockNumScore,
			StepNum = stepNum,
			StepNumScore = stepNumScore,
			ContinuousTopScore = continuousTopScore
		};
		return;
		IL_018d:
		value.Id = id;
		value.GroupID = groupID;
		value.UnlockNum = unlockNum;
		value.UnlockNumScore = unlockNumScore;
		value.StepNum = stepNum;
		value.StepNumScore = stepNumScore;
		value.ContinuousTopScore = continuousTopScore;
	}
}
