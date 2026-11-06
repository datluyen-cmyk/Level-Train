using System;
using System.Linq;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Rules;

namespace UnlockRing.Game.Gameplay;

public static class CoreHitTester
{
	public static PartId? HitTest(LevelDefinition level, BoardState state, Vector2D worldPoint, double hitSlop = 8.0)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (level == null)
		{
			throw new ArgumentNullException("level");
		}
		if (state == null)
		{
			throw new ArgumentNullException("state");
		}
		if (!double.IsFinite(hitSlop) || hitSlop < 0.0)
		{
			throw new ArgumentOutOfRangeException("hitSlop");
		}
		Vector2 point = default(Vector2);
		((Vector2)(ref point))._002Ector(((Vector2D)(ref worldPoint)).X, ((Vector2D)(ref worldPoint)).Y);
		foreach (PartDefinition item in level.Parts.OrderByDescending((PartDefinition candidate) => candidate.AuthoredOrder))
		{
			if (state.Parts.TryGetValue(item.Id, out var value) && !value.Released && !value.Removed && IsPlayerMovablePart(item) && TryMeasurePartHitDistance(item, value, point, hitSlop, out var _))
			{
				return item.Id;
			}
		}
		return null;
	}

	internal static bool TryMeasurePartHitDistance(PartDefinition part, PartState partState, Vector2 point, double hitSlop, out double distance)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		distance = double.MaxValue;
		bool result = false;
		foreach (FeatureDefinition feature in part.Features)
		{
			FeatureId id = feature.Id;
			if (((FeatureId)(ref id)).Value.Equals("playable-lock", StringComparison.Ordinal))
			{
				continue;
			}
			ResolvedFeatureGeometry val = ShapeConverter.Convert(part, partState, feature);
			if (!val.Conversion.IsExact)
			{
				continue;
			}
			IGeometryShape shape = val.Conversion.Shape;
			if (shape.Contains(point, 0.0))
			{
				distance = 0.0;
				return true;
			}
			double num = Vector2.Distance(shape.ClosestPoint(point), point);
			if (num <= hitSlop)
			{
				result = true;
				if (num < distance)
				{
					distance = num;
				}
			}
		}
		return result;
	}

	internal static bool IsPlayerMovablePart(PartDefinition part)
	{
		return part.Behaviors.Any((BehaviorDefinition behavior) => behavior is RotatableBehaviorDefinition || behavior is AxisSlidableBehaviorDefinition);
	}
}
