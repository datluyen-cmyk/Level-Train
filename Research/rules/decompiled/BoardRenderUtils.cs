using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Rules;

public static class BoardRenderUtils
{
	public static bool IsSliding(PartDefinition p)
	{
		return p.Behaviors.Any((BehaviorDefinition b) => b is AxisSlidableBehaviorDefinition);
	}

	public static bool IsPlayerPart(PartDefinition p)
	{
		return p.Behaviors.Any((BehaviorDefinition b) => b is RotatableBehaviorDefinition || b is AxisSlidableBehaviorDefinition);
	}

	public static bool IsFixedConstraintPart(PartDefinition p)
	{
		if (!IsPlayerPart(p) && p.Features.Count > 0)
		{
			return p.Features.All((FeatureDefinition f) => (int)f.Channel == 1);
		}
		return false;
	}

	public static bool IsRotationInvariant(IGeometryShape shape, Vector2 pivot)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val3;
		if (!(shape is AnnularArc val))
		{
			if (shape is Circle val2)
			{
				val3 = ((Circle)(ref val2)).Center - pivot;
				return ((Vector2)(ref val3)).Length <= 1E-09;
			}
			return false;
		}
		int result;
		if (((AnnularArc)(ref val)).IsFullCircle)
		{
			val3 = ((AnnularArc)(ref val)).Center - pivot;
			result = ((((Vector2)(ref val3)).Length <= 1E-09) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	public static List<CachedRenderFeature> CollectFeatures(PartDefinition part, PartState state, out Aabb? partBounds)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		List<CachedRenderFeature> list = new List<CachedRenderFeature>();
		Aabb? val = null;
		foreach (FeatureDefinition feature in part.Features)
		{
			FeatureId id = feature.Id;
			if (((FeatureId)(ref id)).Value.Equals("playable-lock", StringComparison.Ordinal))
			{
				continue;
			}
			ResolvedFeatureGeometry val2 = ShapeConverter.Convert(part, state, feature);
			if (val2.Conversion.IsExact)
			{
				IGeometryShape shape = val2.Conversion.Shape;
				list.Add(new CachedRenderFeature(feature, shape));
				Aabb value;
				if (!val.HasValue)
				{
					value = shape.Bounds;
				}
				else
				{
					Aabb value2 = val.Value;
					value = ((Aabb)(ref value2)).Union(shape.Bounds);
				}
				val = value;
			}
		}
		partBounds = val;
		return list;
	}

	public static Color FeatureColor(CachedRenderFeature cached, Color baseColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		FeatureId id = cached.Feature.Id;
		_003F val;
		if (!((FeatureId)(ref id)).Value.StartsWith("bomb-", StringComparison.Ordinal))
		{
			id = cached.Feature.Id;
			val = (((FeatureId)(ref id)).Value.StartsWith("catch-", StringComparison.Ordinal) ? RingTheme.Fixed : baseColor);
		}
		else
		{
			val = RingTheme.Bomb;
		}
		Color val2 = (Color)val;
		if (cached.Shape is AnnularArc && cached.Feature.Tags.Any((string t) => t == "source-elbow"))
		{
			((Color)(ref val2))._002Ector(val2.r, val2.g, val2.b, 0.64f);
		}
		return val2;
	}

	public static Aabb ComputeRequiredBounds(PartDefinition part, PartState state, IReadOnlyList<CachedRenderFeature> features, Aabb partBounds)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		Aabb result = partBounds;
		if (part.Behaviors.Any((BehaviorDefinition b) => b is RotatableBehaviorDefinition))
		{
			Pose2D pose = state.Pose;
			Vector2D position = ((Pose2D)(ref pose)).Position;
			double x = ((Vector2D)(ref position)).X;
			pose = state.Pose;
			position = ((Pose2D)(ref pose)).Position;
			Vector2 pivot = new Vector2(x, ((Vector2D)(ref position)).Y);
			Aabb? val = features.Where((CachedRenderFeature f) => !IsRotationInvariant(f.Shape, pivot)).Select((Func<CachedRenderFeature, Aabb?>)((CachedRenderFeature f) => f.Shape.Bounds)).Aggregate(null, delegate(Aabb? a, Aabb? b)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				if (!a.HasValue)
				{
					return b;
				}
				Aabb value2 = a.Value;
				return ((Aabb)(ref value2)).Union(b.Value);
			});
			if (val.HasValue)
			{
				Aabb value = val.Value;
				Vector2 min = ((Aabb)(ref value)).Min;
				value = val.Value;
				Vector2 max = ((Aabb)(ref value)).Max;
				double num = ((IEnumerable<Vector2>)(object)new Vector2[4]
				{
					new Vector2(((Vector2)(ref min)).X, ((Vector2)(ref min)).Y),
					new Vector2(((Vector2)(ref max)).X, ((Vector2)(ref min)).Y),
					new Vector2(((Vector2)(ref max)).X, ((Vector2)(ref max)).Y),
					new Vector2(((Vector2)(ref min)).X, ((Vector2)(ref max)).Y)
				}).Max(delegate(Vector2 c)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Unknown result type (might be due to invalid IL or missing references)
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					Vector2 val2 = c - pivot;
					return ((Vector2)(ref val2)).Length;
				});
				result = ((Aabb)(ref result)).Union(new Aabb(new Vector2(((Vector2)(ref pivot)).X - num, ((Vector2)(ref pivot)).Y - num), new Vector2(((Vector2)(ref pivot)).X + num, ((Vector2)(ref pivot)).Y + num)));
			}
		}
		return result;
	}

	public static Rect WorldRectFromAabb(Aabb value, float marginFactor, float coordScale)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((Aabb)(ref value)).Size;
		double num = ((Vector2)(ref val)).Length * (double)marginFactor;
		value = ((Aabb)(ref value)).Expand(num);
		val = ((Aabb)(ref value)).Min;
		float num2 = (float)((Vector2)(ref val)).X * coordScale;
		val = ((Aabb)(ref value)).Max;
		float num3 = (0f - (float)((Vector2)(ref val)).Y) * coordScale;
		val = ((Aabb)(ref value)).Size;
		float num4 = (float)((Vector2)(ref val)).X * coordScale;
		val = ((Aabb)(ref value)).Size;
		return new Rect(num2, num3, num4, (float)((Vector2)(ref val)).Y * coordScale);
	}

	public static float OrthoSizeFor(Rect bounds, float aspect)
	{
		return Mathf.Max(((Rect)(ref bounds)).height * 0.5f, ((Rect)(ref bounds)).width * 0.5f / aspect) * 1.08f;
	}

	public static void BakeToLocal(Mesh mesh, Vector2 posePos, float poseRotRad)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] vertices = mesh.vertices;
		float num = Mathf.Cos(0f - poseRotRad);
		float num2 = Mathf.Sin(0f - poseRotRad);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector2 val = new Vector2(vertices[i].x, vertices[i].y) - posePos;
			vertices[i] = new Vector3(val.x * num - val.y * num2, val.x * num2 + val.y * num, 0f);
		}
		for (int j = 0; j < vertices.Length; j++)
		{
			vertices[j].y = 0f - vertices[j].y;
		}
		mesh.vertices = vertices;
		int[] triangles = mesh.triangles;
		for (int k = 0; k < triangles.Length; k += 3)
		{
			int num3 = triangles[k + 1];
			triangles[k + 1] = triangles[k + 2];
			triangles[k + 2] = num3;
		}
		mesh.triangles = triangles;
		mesh.RecalculateBounds();
	}
}
