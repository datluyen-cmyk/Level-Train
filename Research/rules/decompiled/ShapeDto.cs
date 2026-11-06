using System;
using System.Linq;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class ShapeDto
{
	public string kind;

	public VecDto center;

	public VecDto start;

	public VecDto end;

	public double radius;

	public double innerRadius;

	public double outerRadius;

	public double startRadians;

	public double sweepRadians;

	public VecDto halfExtents;

	public double rotationRadians;

	public VecDto[] vertices;

	public ShapeDto[] children;

	public ShapeDefinition ToDomain()
	{
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Expected O, but got Unknown
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Expected O, but got Unknown
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		return (ShapeDefinition)(kind switch
		{
			"circle" => (object)new CircleShapeDefinition(center.ToDomain(), radius), 
			"segment" => (object)new SegmentShapeDefinition(start.ToDomain(), end.ToDomain()), 
			"capsule" => (object)new CapsuleShapeDefinition(start.ToDomain(), end.ToDomain(), radius), 
			"obb" => (object)new ObbShapeDefinition(center.ToDomain(), halfExtents.ToDomain(), rotationRadians), 
			"convexPolygon" => (object)new ConvexPolygonShapeDefinition((vertices ?? Array.Empty<VecDto>()).Select((VecDto v) => v.ToDomain())), 
			"arc" => (object)new ArcShapeDefinition(center.ToDomain(), radius, startRadians, sweepRadians), 
			"annularArc" => (object)new AnnularArcShapeDefinition(center.ToDomain(), innerRadius, outerRadius, startRadians, sweepRadians), 
			"compound" => (object)new CompoundShapeDefinition((children ?? Array.Empty<ShapeDto>()).Select((ShapeDto c) => c.ToDomain())), 
			_ => throw new InvalidOperationException("Unknown shape kind: " + kind), 
		});
	}

	public static ShapeDto FromDomain(ShapeDefinition s)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		ShapeDto shapeDto = new ShapeDto
		{
			kind = s.Kind
		};
		CircleShapeDefinition val = (CircleShapeDefinition)(object)((s is CircleShapeDefinition) ? s : null);
		if (val == null)
		{
			SegmentShapeDefinition val2 = (SegmentShapeDefinition)(object)((s is SegmentShapeDefinition) ? s : null);
			if (val2 == null)
			{
				CapsuleShapeDefinition val3 = (CapsuleShapeDefinition)(object)((s is CapsuleShapeDefinition) ? s : null);
				if (val3 == null)
				{
					ObbShapeDefinition val4 = (ObbShapeDefinition)(object)((s is ObbShapeDefinition) ? s : null);
					if (val4 == null)
					{
						ConvexPolygonShapeDefinition val5 = (ConvexPolygonShapeDefinition)(object)((s is ConvexPolygonShapeDefinition) ? s : null);
						if (val5 == null)
						{
							ArcShapeDefinition val6 = (ArcShapeDefinition)(object)((s is ArcShapeDefinition) ? s : null);
							if (val6 == null)
							{
								AnnularArcShapeDefinition val7 = (AnnularArcShapeDefinition)(object)((s is AnnularArcShapeDefinition) ? s : null);
								if (val7 == null)
								{
									CompoundShapeDefinition val8 = (CompoundShapeDefinition)(object)((s is CompoundShapeDefinition) ? s : null);
									if (val8 != null)
									{
										shapeDto.children = val8.Children.Select(FromDomain).ToArray();
									}
								}
								else
								{
									shapeDto.center = VecDto.FromDomain(val7.Center);
									shapeDto.innerRadius = val7.InnerRadius;
									shapeDto.outerRadius = val7.OuterRadius;
									shapeDto.startRadians = val7.StartRadians;
									shapeDto.sweepRadians = val7.SweepRadians;
								}
							}
							else
							{
								shapeDto.center = VecDto.FromDomain(val6.Center);
								shapeDto.radius = val6.Radius;
								shapeDto.startRadians = val6.StartRadians;
								shapeDto.sweepRadians = val6.SweepRadians;
							}
						}
						else
						{
							shapeDto.vertices = val5.Vertices.Select(VecDto.FromDomain).ToArray();
						}
					}
					else
					{
						shapeDto.center = VecDto.FromDomain(val4.Center);
						shapeDto.halfExtents = VecDto.FromDomain(val4.HalfExtents);
						shapeDto.rotationRadians = val4.RotationRadians;
					}
				}
				else
				{
					shapeDto.start = VecDto.FromDomain(val3.Start);
					shapeDto.end = VecDto.FromDomain(val3.End);
					shapeDto.radius = val3.Radius;
				}
			}
			else
			{
				shapeDto.start = VecDto.FromDomain(val2.Start);
				shapeDto.end = VecDto.FromDomain(val2.End);
			}
		}
		else
		{
			shapeDto.center = VecDto.FromDomain(val.Center);
			shapeDto.radius = val.Radius;
		}
		return shapeDto;
	}
}
