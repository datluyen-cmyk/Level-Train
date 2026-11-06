using System.Collections.Generic;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Example.Cfg;

[MemoryPackable(/*Could not decode attribute arguments.*/)]
public class OperationEvaluationConfig : IMemoryPackable<OperationEvaluationConfig>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class OperationEvaluationConfigFormatter : MemoryPackFormatter<OperationEvaluationConfig>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref OperationEvaluationConfig value)
		{
			OperationEvaluationConfig.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref OperationEvaluationConfig value)
		{
			OperationEvaluationConfig.Deserialize(ref reader, ref value);
		}
	}

	[MemoryPackOrder(0)]
	public int Id { get; private set; }

	[MemoryPackOrder(1)]
	public int GroupID { get; private set; }

	[MemoryPackOrder(2)]
	public List<int> Evaluation1 { get; private set; }

	[MemoryPackOrder(3)]
	public List<int> UnlockNumScore { get; private set; }

	[MemoryPackOrder(4)]
	public List<int> Evaluation2 { get; private set; }

	[MemoryPackOrder(5)]
	public List<int> Evaluation3 { get; private set; }

	[MemoryPackOrder(6)]
	public List<int> Evaluation4 { get; private set; }

	[MemoryPackOrder(7)]
	public List<int> Evaluation5 { get; private set; }

	static OperationEvaluationConfig()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<OperationEvaluationConfig>())
		{
			MemoryPackFormatterProvider.Register<OperationEvaluationConfig>((MemoryPackFormatter<OperationEvaluationConfig>)(object)new OperationEvaluationConfigFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<OperationEvaluationConfig[]>())
		{
			MemoryPackFormatterProvider.Register<OperationEvaluationConfig[]>((MemoryPackFormatter<OperationEvaluationConfig[]>)(object)new ArrayFormatter<OperationEvaluationConfig>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<int>>())
		{
			MemoryPackFormatterProvider.Register<List<int>>((MemoryPackFormatter<List<int>>)(object)new ListFormatter<int>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref OperationEvaluationConfig? value)
	{
		if (value == null)
		{
			((MemoryPackWriter)(ref writer)).WriteNullObjectHeader();
			return;
		}
		int id = value.Id;
		int groupID = value.GroupID;
		((MemoryPackWriter)(ref writer)).WriteUnmanagedWithObjectHeader<int, int>((byte)8, ref id, ref groupID);
		List<int> evaluation = value.Evaluation1;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref evaluation);
		evaluation = value.UnlockNumScore;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref evaluation);
		evaluation = value.Evaluation2;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref evaluation);
		evaluation = value.Evaluation3;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref evaluation);
		evaluation = value.Evaluation4;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref evaluation);
		evaluation = value.Evaluation5;
		((MemoryPackWriter)(ref writer)).WriteValue<List<int>>(ref evaluation);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref OperationEvaluationConfig? value)
	{
		byte b = default(byte);
		if (!((MemoryPackReader)(ref reader)).TryReadObjectHeader(ref b))
		{
			value = null;
			return;
		}
		int id = default(int);
		int groupID = default(int);
		List<int> evaluation;
		List<int> unlockNumScore;
		List<int> evaluation2;
		List<int> evaluation3;
		List<int> evaluation4;
		List<int> evaluation5;
		if (b == 8)
		{
			if (value != null)
			{
				id = value.Id;
				groupID = value.GroupID;
				evaluation = value.Evaluation1;
				unlockNumScore = value.UnlockNumScore;
				evaluation2 = value.Evaluation2;
				evaluation3 = value.Evaluation3;
				evaluation4 = value.Evaluation4;
				evaluation5 = value.Evaluation5;
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref groupID);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockNumScore);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation2);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation3);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation4);
				((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation5);
				goto IL_01be;
			}
			((MemoryPackReader)(ref reader)).ReadUnmanaged<int, int>(ref id, ref groupID);
			evaluation = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			unlockNumScore = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			evaluation2 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			evaluation3 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			evaluation4 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
			evaluation5 = ((MemoryPackReader)(ref reader)).ReadValue<List<int>>();
		}
		else
		{
			if (b > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(OperationEvaluationConfig), (byte)8, b);
				return;
			}
			if (value == null)
			{
				id = 0;
				groupID = 0;
				evaluation = null;
				unlockNumScore = null;
				evaluation2 = null;
				evaluation3 = null;
				evaluation4 = null;
				evaluation5 = null;
			}
			else
			{
				id = value.Id;
				groupID = value.GroupID;
				evaluation = value.Evaluation1;
				unlockNumScore = value.UnlockNumScore;
				evaluation2 = value.Evaluation2;
				evaluation3 = value.Evaluation3;
				evaluation4 = value.Evaluation4;
				evaluation5 = value.Evaluation5;
			}
			if (b != 0)
			{
				((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref id);
				if (b != 1)
				{
					((MemoryPackReader)(ref reader)).ReadUnmanaged<int>(ref groupID);
					if (b != 2)
					{
						((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation);
						if (b != 3)
						{
							((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref unlockNumScore);
							if (b != 4)
							{
								((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation2);
								if (b != 5)
								{
									((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation3);
									if (b != 6)
									{
										((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation4);
										if (b != 7)
										{
											((MemoryPackReader)(ref reader)).ReadValue<List<int>>(ref evaluation5);
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
				goto IL_01be;
			}
		}
		value = new OperationEvaluationConfig
		{
			Id = id,
			GroupID = groupID,
			Evaluation1 = evaluation,
			UnlockNumScore = unlockNumScore,
			Evaluation2 = evaluation2,
			Evaluation3 = evaluation3,
			Evaluation4 = evaluation4,
			Evaluation5 = evaluation5
		};
		return;
		IL_01be:
		value.Id = id;
		value.GroupID = groupID;
		value.Evaluation1 = evaluation;
		value.UnlockNumScore = unlockNumScore;
		value.Evaluation2 = evaluation2;
		value.Evaluation3 = evaluation3;
		value.Evaluation4 = evaluation4;
		value.Evaluation5 = evaluation5;
	}
}
