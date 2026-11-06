using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnlockRing.Core.Domain;

namespace UnlockRing.Integration.Sample;

public static class LevelBinaryCodec
{
	private const string Magic = "URLV";

	private const int CurrentVersion = 1;

	private const byte ShapeCircle = 0;

	private const byte ShapeSegment = 1;

	private const byte ShapeCapsule = 2;

	private const byte ShapeObb = 3;

	private const byte ShapePolygon = 4;

	private const byte ShapeArc = 5;

	private const byte ShapeAnnularArc = 6;

	private const byte ShapeCompound = 7;

	private const byte BehaviorRotatable = 0;

	private const byte BehaviorAxisSlidable = 1;

	private const byte BehaviorLinearGuide = 2;

	private const byte BehaviorThreadable = 3;

	private const byte BehaviorCatch = 4;

	private const byte BehaviorHook = 5;

	private const byte BehaviorBomb = 6;

	private const byte BehaviorForceRemovable = 7;

	private const byte BehaviorReleasable = 8;

	public static byte[] Serialize(LevelDefinition level)
	{
		if (level == null)
		{
			throw new ArgumentNullException("level");
		}
		using MemoryStream memoryStream = new MemoryStream(256);
		using BinaryWriter w = new BinaryWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: false);
		WriteLevel(w, level);
		return memoryStream.ToArray();
	}

	private static void WriteLevel(BinaryWriter w, LevelDefinition level)
	{
		byte[] bytes = Encoding.ASCII.GetBytes("URLV");
		w.Write(bytes);
		w.Write(1);
		WriteString(w, level.LevelId);
		w.Write(level.SchemaVersion);
		WriteString(w, level.RulesVersion);
		w.Write(level.Parts.Count);
		foreach (PartDefinition part in level.Parts)
		{
			WritePart(w, part);
		}
		w.Write(level.Rules.ForceRemovalEnabled);
		w.Write(level.Rules.ContactTolerance);
		w.Write(level.AuthoredConstraints.Count);
		foreach (AuthoredConstraintDefinition authoredConstraint in level.AuthoredConstraints)
		{
			WriteConstraint(w, authoredConstraint);
		}
	}

	private static void WritePart(BinaryWriter w, PartDefinition part)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		PartId id = part.Id;
		WriteString(w, ((PartId)(ref id)).Value);
		w.Write(part.AuthoredOrder);
		WritePose(w, part.InitialPose);
		w.Write(part.Features.Count);
		foreach (FeatureDefinition feature in part.Features)
		{
			WriteFeature(w, feature);
		}
		w.Write(part.Behaviors.Count);
		foreach (BehaviorDefinition behavior in part.Behaviors)
		{
			WriteBehavior(w, behavior);
		}
	}

	private static void WriteFeature(BinaryWriter w, FeatureDefinition feature)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		FeatureId id = feature.Id;
		WriteString(w, ((FeatureId)(ref id)).Value);
		w.Write((byte)feature.Channel);
		w.Write((byte)feature.AnchorSpace);
		WriteShape(w, feature.Shape);
		w.Write(feature.Tags.Count);
		foreach (string tag in feature.Tags)
		{
			WriteString(w, tag);
		}
	}

	private static void WriteBehavior(BinaryWriter w, BehaviorDefinition behavior)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (!(behavior is RotatableBehaviorDefinition))
		{
			AxisSlidableBehaviorDefinition val = (AxisSlidableBehaviorDefinition)(object)((behavior is AxisSlidableBehaviorDefinition) ? behavior : null);
			if (val == null)
			{
				LinearGuideBehaviorDefinition val2 = (LinearGuideBehaviorDefinition)(object)((behavior is LinearGuideBehaviorDefinition) ? behavior : null);
				if (val2 == null)
				{
					ThreadableBehaviorDefinition val3 = (ThreadableBehaviorDefinition)(object)((behavior is ThreadableBehaviorDefinition) ? behavior : null);
					if (val3 == null)
					{
						CatchBehaviorDefinition val4 = (CatchBehaviorDefinition)(object)((behavior is CatchBehaviorDefinition) ? behavior : null);
						if (val4 == null)
						{
							HookBehaviorDefinition val5 = (HookBehaviorDefinition)(object)((behavior is HookBehaviorDefinition) ? behavior : null);
							if (val5 == null)
							{
								if (!(behavior is BombBehaviorDefinition))
								{
									if (!(behavior is ForceRemovableBehaviorDefinition))
									{
										if (!(behavior is ReleasableBehaviorDefinition))
										{
											throw new ArgumentException("Unsupported behavior kind: " + behavior.Kind, "behavior");
										}
										w.Write((byte)8);
									}
									else
									{
										w.Write((byte)7);
									}
								}
								else
								{
									w.Write((byte)6);
								}
							}
							else
							{
								w.Write((byte)5);
								WriteString(w, val5.CompatibilityTag);
							}
						}
						else
						{
							w.Write((byte)4);
							WriteString(w, val4.CompatibilityTag);
						}
					}
					else
					{
						w.Write((byte)3);
						WriteString(w, val3.CompatibilityTag);
					}
				}
				else
				{
					w.Write((byte)2);
					WriteVector(w, val2.LocalAxis);
					w.Write(val2.Clearance);
				}
			}
			else
			{
				w.Write((byte)1);
				WriteVector(w, val.LocalAxis);
			}
		}
		else
		{
			w.Write((byte)0);
		}
	}

	private static void WriteShape(BinaryWriter w, ShapeDefinition shape)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		CircleShapeDefinition val = (CircleShapeDefinition)(object)((shape is CircleShapeDefinition) ? shape : null);
		if (val == null)
		{
			SegmentShapeDefinition val2 = (SegmentShapeDefinition)(object)((shape is SegmentShapeDefinition) ? shape : null);
			if (val2 == null)
			{
				CapsuleShapeDefinition val3 = (CapsuleShapeDefinition)(object)((shape is CapsuleShapeDefinition) ? shape : null);
				if (val3 == null)
				{
					ObbShapeDefinition val4 = (ObbShapeDefinition)(object)((shape is ObbShapeDefinition) ? shape : null);
					if (val4 == null)
					{
						ConvexPolygonShapeDefinition val5 = (ConvexPolygonShapeDefinition)(object)((shape is ConvexPolygonShapeDefinition) ? shape : null);
						if (val5 != null)
						{
							w.Write((byte)4);
							w.Write(val5.Vertices.Count);
							{
								foreach (Vector2D vertex in val5.Vertices)
								{
									WriteVector(w, vertex);
								}
								return;
							}
						}
						ArcShapeDefinition val6 = (ArcShapeDefinition)(object)((shape is ArcShapeDefinition) ? shape : null);
						if (val6 == null)
						{
							AnnularArcShapeDefinition val7 = (AnnularArcShapeDefinition)(object)((shape is AnnularArcShapeDefinition) ? shape : null);
							if (val7 == null)
							{
								CompoundShapeDefinition val8 = (CompoundShapeDefinition)(object)((shape is CompoundShapeDefinition) ? shape : null);
								if (val8 != null)
								{
									w.Write((byte)7);
									w.Write(val8.Children.Count);
									{
										foreach (ShapeDefinition child in val8.Children)
										{
											WriteShape(w, child);
										}
										return;
									}
								}
								throw new ArgumentException("Unsupported shape kind: " + shape.Kind, "shape");
							}
							w.Write((byte)6);
							WriteVector(w, val7.Center);
							w.Write(val7.InnerRadius);
							w.Write(val7.OuterRadius);
							w.Write(val7.StartRadians);
							w.Write(val7.SweepRadians);
						}
						else
						{
							w.Write((byte)5);
							WriteVector(w, val6.Center);
							w.Write(val6.Radius);
							w.Write(val6.StartRadians);
							w.Write(val6.SweepRadians);
						}
					}
					else
					{
						w.Write((byte)3);
						WriteVector(w, val4.Center);
						WriteVector(w, val4.HalfExtents);
						w.Write(val4.RotationRadians);
					}
				}
				else
				{
					w.Write((byte)2);
					WriteVector(w, val3.Start);
					WriteVector(w, val3.End);
					w.Write(val3.Radius);
				}
			}
			else
			{
				w.Write((byte)1);
				WriteVector(w, val2.Start);
				WriteVector(w, val2.End);
			}
		}
		else
		{
			w.Write((byte)0);
			WriteVector(w, val.Center);
			w.Write(val.Radius);
		}
	}

	private static void WriteConstraint(BinaryWriter w, AuthoredConstraintDefinition constraint)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		w.Write((byte)constraint.Kind);
		PartId val = constraint.FirstPart;
		WriteString(w, ((PartId)(ref val)).Value);
		FeatureId val2 = constraint.FirstFeature;
		WriteString(w, ((FeatureId)(ref val2)).Value);
		val = constraint.SecondPart;
		WriteString(w, ((PartId)(ref val)).Value);
		val2 = constraint.SecondFeature;
		WriteString(w, ((FeatureId)(ref val2)).Value);
		w.Write((byte)constraint.ActivityKind);
		w.Write(constraint.CollisionExemptions.Count);
		foreach (AuthoredCollisionExemptionDefinition collisionExemption in constraint.CollisionExemptions)
		{
			val = collisionExemption.FirstPart;
			WriteString(w, ((PartId)(ref val)).Value);
			val2 = collisionExemption.FirstFeature;
			WriteString(w, ((FeatureId)(ref val2)).Value);
			val = collisionExemption.SecondPart;
			WriteString(w, ((PartId)(ref val)).Value);
			val2 = collisionExemption.SecondFeature;
			WriteString(w, ((FeatureId)(ref val2)).Value);
		}
	}

	private static void WritePose(BinaryWriter w, Pose2D pose)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		WriteVector(w, ((Pose2D)(ref pose)).Position);
		w.Write(((Pose2D)(ref pose)).RotationRadians);
	}

	private static void WriteVector(BinaryWriter w, Vector2D v)
	{
		w.Write(((Vector2D)(ref v)).X);
		w.Write(((Vector2D)(ref v)).Y);
	}

	private static void WriteString(BinaryWriter w, string value)
	{
		if (value == null)
		{
			w.Write(-1);
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		w.Write(bytes.Length);
		w.Write(bytes);
	}

	public static LevelDefinition Deserialize(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		using MemoryStream input = new MemoryStream(data, writable: false);
		using BinaryReader r = new BinaryReader(input, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: false);
		return ReadLevel(r);
	}

	private static LevelDefinition ReadLevel(BinaryReader r)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		byte[] bytes = r.ReadBytes(4);
		string text = Encoding.ASCII.GetString(bytes);
		if (text != "URLV")
		{
			throw new InvalidDataException("Invalid level binary magic: " + text);
		}
		int num = r.ReadInt32();
		if (num != 1)
		{
			throw new InvalidDataException("Unsupported level binary version: " + num);
		}
		string text2 = ReadString(r);
		int num2 = r.ReadInt32();
		string text3 = ReadString(r);
		int num3 = r.ReadInt32();
		List<PartDefinition> list = new List<PartDefinition>(num3);
		for (int i = 0; i < num3; i++)
		{
			list.Add(ReadPart(r));
		}
		bool num4 = r.ReadBoolean();
		double num5 = r.ReadDouble();
		LevelRuleDefinition val = new LevelRuleDefinition(num4, num5);
		int num6 = r.ReadInt32();
		List<AuthoredConstraintDefinition> list2 = new List<AuthoredConstraintDefinition>(num6);
		for (int j = 0; j < num6; j++)
		{
			list2.Add(ReadConstraint(r));
		}
		return new LevelDefinition(text2, num2, text3, (CoordinateConvention)0, (IEnumerable<PartDefinition>)list, val, (IEnumerable<AuthoredConstraintDefinition>)list2);
	}

	private static PartDefinition ReadPart(BinaryReader r)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		PartId val = default(PartId);
		((PartId)(ref val))._002Ector(ReadString(r));
		int num = r.ReadInt32();
		Pose2D val2 = ReadPose(r);
		int num2 = r.ReadInt32();
		List<FeatureDefinition> list = new List<FeatureDefinition>(num2);
		for (int i = 0; i < num2; i++)
		{
			list.Add(ReadFeature(r));
		}
		int num3 = r.ReadInt32();
		List<BehaviorDefinition> list2 = new List<BehaviorDefinition>(num3);
		for (int j = 0; j < num3; j++)
		{
			list2.Add(ReadBehavior(r));
		}
		return new PartDefinition(val, num, val2, (IEnumerable<FeatureDefinition>)list, (IEnumerable<BehaviorDefinition>)list2);
	}

	private static FeatureDefinition ReadFeature(BinaryReader r)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		FeatureId val = default(FeatureId);
		((FeatureId)(ref val))._002Ector(ReadString(r));
		FeatureChannel val2 = (FeatureChannel)r.ReadByte();
		FeatureAnchorSpace val3 = (FeatureAnchorSpace)r.ReadByte();
		ShapeDefinition val4 = ReadShape(r);
		int num = r.ReadInt32();
		List<string> list = new List<string>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(ReadString(r));
		}
		return new FeatureDefinition(val, val2, val3, val4, (IEnumerable<string>)list);
	}

	private static BehaviorDefinition ReadBehavior(BinaryReader r)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		byte b = r.ReadByte();
		switch (b)
		{
		case 0:
			return (BehaviorDefinition)new RotatableBehaviorDefinition(0);
		case 1:
			return (BehaviorDefinition)new AxisSlidableBehaviorDefinition(ReadVector(r));
		case 2:
		{
			Vector2D val = ReadVector(r);
			double num = r.ReadDouble();
			return (BehaviorDefinition)new LinearGuideBehaviorDefinition(val, num);
		}
		case 3:
			return (BehaviorDefinition)new ThreadableBehaviorDefinition(ReadString(r));
		case 4:
			return (BehaviorDefinition)new CatchBehaviorDefinition(ReadString(r));
		case 5:
			return (BehaviorDefinition)new HookBehaviorDefinition(ReadString(r));
		case 6:
			return (BehaviorDefinition)new BombBehaviorDefinition();
		case 7:
			return (BehaviorDefinition)new ForceRemovableBehaviorDefinition();
		case 8:
			return (BehaviorDefinition)new ReleasableBehaviorDefinition();
		default:
			throw new InvalidDataException("Unknown behavior tag: " + b);
		}
	}

	private static ShapeDefinition ReadShape(BinaryReader r)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		byte b = r.ReadByte();
		switch (b)
		{
		case 0:
			return (ShapeDefinition)new CircleShapeDefinition(ReadVector(r), r.ReadDouble());
		case 1:
		{
			Vector2D val7 = ReadVector(r);
			Vector2D val8 = ReadVector(r);
			return (ShapeDefinition)new SegmentShapeDefinition(val7, val8);
		}
		case 2:
		{
			Vector2D val3 = ReadVector(r);
			Vector2D val4 = ReadVector(r);
			double num3 = r.ReadDouble();
			return (ShapeDefinition)new CapsuleShapeDefinition(val3, val4, num3);
		}
		case 3:
		{
			Vector2D val = ReadVector(r);
			Vector2D val2 = ReadVector(r);
			double num2 = r.ReadDouble();
			return (ShapeDefinition)new ObbShapeDefinition(val, val2, num2);
		}
		case 4:
		{
			int num11 = r.ReadInt32();
			List<Vector2D> list2 = new List<Vector2D>(num11);
			for (int j = 0; j < num11; j++)
			{
				list2.Add(ReadVector(r));
			}
			return (ShapeDefinition)new ConvexPolygonShapeDefinition((IEnumerable<Vector2D>)list2);
		}
		case 5:
		{
			Vector2D val6 = ReadVector(r);
			double num8 = r.ReadDouble();
			double num9 = r.ReadDouble();
			double num10 = r.ReadDouble();
			return (ShapeDefinition)new ArcShapeDefinition(val6, num8, num9, num10);
		}
		case 6:
		{
			Vector2D val5 = ReadVector(r);
			double num4 = r.ReadDouble();
			double num5 = r.ReadDouble();
			double num6 = r.ReadDouble();
			double num7 = r.ReadDouble();
			return (ShapeDefinition)new AnnularArcShapeDefinition(val5, num4, num5, num6, num7);
		}
		case 7:
		{
			int num = r.ReadInt32();
			List<ShapeDefinition> list = new List<ShapeDefinition>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(ReadShape(r));
			}
			return (ShapeDefinition)new CompoundShapeDefinition((IEnumerable<ShapeDefinition>)list);
		}
		default:
			throw new InvalidDataException("Unknown shape tag: " + b);
		}
	}

	private static AuthoredConstraintDefinition ReadConstraint(BinaryReader r)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		ConstraintRelationKind val = (ConstraintRelationKind)r.ReadByte();
		PartId val2 = default(PartId);
		((PartId)(ref val2))._002Ector(ReadString(r));
		FeatureId val3 = default(FeatureId);
		((FeatureId)(ref val3))._002Ector(ReadString(r));
		PartId val4 = default(PartId);
		((PartId)(ref val4))._002Ector(ReadString(r));
		FeatureId val5 = default(FeatureId);
		((FeatureId)(ref val5))._002Ector(ReadString(r));
		AuthoredConstraintActivityKind val6 = (AuthoredConstraintActivityKind)r.ReadByte();
		int num = r.ReadInt32();
		List<AuthoredCollisionExemptionDefinition> list = new List<AuthoredCollisionExemptionDefinition>(num);
		PartId val7 = default(PartId);
		FeatureId val8 = default(FeatureId);
		PartId val9 = default(PartId);
		FeatureId val10 = default(FeatureId);
		for (int i = 0; i < num; i++)
		{
			((PartId)(ref val7))._002Ector(ReadString(r));
			((FeatureId)(ref val8))._002Ector(ReadString(r));
			((PartId)(ref val9))._002Ector(ReadString(r));
			((FeatureId)(ref val10))._002Ector(ReadString(r));
			list.Add(new AuthoredCollisionExemptionDefinition(val7, val8, val9, val10));
		}
		return new AuthoredConstraintDefinition(val, val2, val3, val4, val5, val6, (IEnumerable<AuthoredCollisionExemptionDefinition>)list);
	}

	private static Pose2D ReadPose(BinaryReader r)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Vector2D val = ReadVector(r);
		double num = r.ReadDouble();
		return new Pose2D(val, num);
	}

	private static Vector2D ReadVector(BinaryReader r)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		double num = r.ReadDouble();
		double num2 = r.ReadDouble();
		return new Vector2D(num, num2);
	}

	private static string ReadString(BinaryReader r)
	{
		int num = r.ReadInt32();
		if (num < 0)
		{
			return null;
		}
		byte[] bytes = r.ReadBytes(num);
		return Encoding.UTF8.GetString(bytes);
	}
}
