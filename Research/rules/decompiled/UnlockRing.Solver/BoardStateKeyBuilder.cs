using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnlockRing.Core.Domain;

namespace UnlockRing.Solver;

public sealed class BoardStateKeyBuilder
{
	private readonly StateIdentityQuantization? _quantization;

	public BoardStateKeyBuilder(StateIdentityQuantization? quantization = null)
	{
		_quantization = quantization;
	}

	public string Build(BoardState state)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Expected I4, but got Unknown
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		Compatibility.ThrowIfNull(state, "state");
		StringBuilder stringBuilder = new StringBuilder(256);
		AppendText(stringBuilder, state.LevelId);
		stringBuilder.Append(state.SchemaVersion).Append(';');
		AppendText(stringBuilder, state.RulesVersion);
		PartId val;
		foreach (KeyValuePair<PartId, PartState> item in state.Parts.OrderBy((KeyValuePair<PartId, PartState> x) => x.Key))
		{
			val = item.Key;
			AppendText(stringBuilder, ((PartId)(ref val)).Value);
			PartState value = item.Value;
			Pose2D pose = value.Pose;
			Vector2D position = ((Pose2D)(ref pose)).Position;
			AppendNumber(stringBuilder, ((Vector2D)(ref position)).X, _quantization?.Position);
			pose = value.Pose;
			position = ((Pose2D)(ref pose)).Position;
			AppendNumber(stringBuilder, ((Vector2D)(ref position)).Y, _quantization?.Position);
			pose = value.Pose;
			AppendNumber(stringBuilder, ((Pose2D)(ref pose)).RotationRadians, _quantization?.RotationRadians);
			AppendNumber(stringBuilder, value.AccumulatedRotationRadians, _quantization?.RotationRadians);
			AppendNumber(stringBuilder, value.SlideOffset, _quantization?.SlideOffset);
			stringBuilder.Append(value.Released ? '1' : '0').Append(value.Removed ? '1' : '0').Append(value.BombTriggered ? '1' : '0')
				.Append(';');
		}
		foreach (ConstraintRelation item2 in state.Constraints.OrderBy<ConstraintRelation, string>(ConstraintKey, StringComparer.Ordinal))
		{
			stringBuilder.Append((int)item2.Kind).Append(':');
			val = item2.FirstPart;
			AppendText(stringBuilder, ((PartId)(ref val)).Value);
			FeatureId val2 = item2.FirstFeature;
			AppendText(stringBuilder, ((FeatureId)(ref val2)).Value);
			val = item2.SecondPart;
			AppendText(stringBuilder, ((PartId)(ref val)).Value);
			val2 = item2.SecondFeature;
			AppendText(stringBuilder, ((FeatureId)(ref val2)).Value);
		}
		return stringBuilder.ToString();
	}

	private static string ConstraintKey(ConstraintRelation relation)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected I4, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		string[] obj = new string[5]
		{
			((int)relation.Kind).ToString(CultureInfo.InvariantCulture),
			null,
			null,
			null,
			null
		};
		PartId val = relation.FirstPart;
		obj[1] = ((PartId)(ref val)).Value;
		FeatureId val2 = relation.FirstFeature;
		obj[2] = ((FeatureId)(ref val2)).Value;
		val = relation.SecondPart;
		obj[3] = ((PartId)(ref val)).Value;
		val2 = relation.SecondFeature;
		obj[4] = ((FeatureId)(ref val2)).Value;
		return string.Join("\0", obj);
	}

	private static void AppendText(StringBuilder builder, string value)
	{
		builder.Append(value.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(value)
			.Append(';');
	}

	private static void AppendNumber(StringBuilder builder, double value, double? quantum)
	{
		if (!Compatibility.IsFinite(value))
		{
			throw new ArgumentException("State identity cannot contain non-finite numbers.");
		}
		if (!quantum.HasValue)
		{
			builder.Append(((value == 0.0) ? 0 : BitConverter.DoubleToInt64Bits(value)).ToString("x16", CultureInfo.InvariantCulture)).Append(';');
			return;
		}
		double num = value / quantum.Value;
		if (num > 9.223372036854776E+18 || num < -9.223372036854776E+18)
		{
			throw new ArgumentOutOfRangeException("value", "Quantized state value exceeds Int64 range.");
		}
		builder.Append(Math.Round(num, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture)).Append(';');
	}
}
