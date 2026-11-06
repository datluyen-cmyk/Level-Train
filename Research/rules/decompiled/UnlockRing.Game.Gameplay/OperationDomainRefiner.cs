using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;
using UnlockRing.Core.Queries;
using UnlockRing.Core.Rules;
using UnlockRing.Core.Runtime;

namespace UnlockRing.Game.Gameplay;

public static class OperationDomainRefiner
{
	private readonly struct ElbowStopGeometry : IEquatable<ElbowStopGeometry>
	{
		public IGeometryShape Shape { get; }

		public FeatureId? PairedGuideFeature { get; }

		public ElbowStopGeometry(IGeometryShape shape, FeatureId? pairedGuideFeature)
		{
			Shape = shape;
			PairedGuideFeature = pairedGuideFeature;
		}

		public bool Equals(ElbowStopGeometry other)
		{
			if (EqualityComparer<IGeometryShape>.Default.Equals(Shape, other.Shape))
			{
				return EqualityComparer<FeatureId?>.Default.Equals(PairedGuideFeature, other.PairedGuideFeature);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is ElbowStopGeometry other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			HashCode hashCode = default(HashCode);
			hashCode.Add<IGeometryShape>(Shape);
			hashCode.Add(PairedGuideFeature);
			return hashCode.ToHashCode();
		}

		public static bool operator ==(ElbowStopGeometry left, ElbowStopGeometry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ElbowStopGeometry left, ElbowStopGeometry right)
		{
			return !left.Equals(right);
		}
	}

	private readonly struct GuideStopGeometry : IEquatable<GuideStopGeometry>
	{
		public PartId PartId { get; }

		public FeatureId FeatureId { get; }

		public IGeometryShape Shape { get; }

		public GuideStopGeometry(PartId partId, FeatureId featureId, IGeometryShape shape)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			PartId = partId;
			FeatureId = featureId;
			Shape = shape;
		}

		public bool Equals(GuideStopGeometry other)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			if (EqualityComparer<PartId>.Default.Equals(PartId, other.PartId) && EqualityComparer<FeatureId>.Default.Equals(FeatureId, other.FeatureId))
			{
				return EqualityComparer<IGeometryShape>.Default.Equals(Shape, other.Shape);
			}
			return false;
		}

		public override bool Equals(object? obj)
		{
			if (obj is GuideStopGeometry other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			HashCode hashCode = default(HashCode);
			hashCode.Add<PartId>(PartId);
			hashCode.Add<FeatureId>(FeatureId);
			hashCode.Add<IGeometryShape>(Shape);
			return hashCode.ToHashCode();
		}

		public static bool operator ==(GuideStopGeometry left, GuideStopGeometry right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GuideStopGeometry left, GuideStopGeometry right)
		{
			return !left.Equals(right);
		}
	}

	private const string ElbowGuideStopReason = "弯头到达导轨限位";

	public static bool HasReleasedBound(GestureMotionDomain? domain)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		if (domain != null)
		{
			if ((int)domain.NegativeBound.Kind != 1)
			{
				return (int)domain.PositiveBound.Kind == 1;
			}
			return true;
		}
		return false;
	}

	public static double ClampLiveGestureAmount(GestureMotionDomain domain, double amount)
	{
		if (domain == null)
		{
			throw new ArgumentNullException("domain");
		}
		if (amount > 0.0 && IsElbowGuideStop(domain.PositiveBound))
		{
			return Math.Min(amount, domain.PositiveBound.SafeAmount);
		}
		if (amount < 0.0 && IsElbowGuideStop(domain.NegativeBound))
		{
			return Math.Max(amount, 0.0 - domain.NegativeBound.SafeAmount);
		}
		return amount;
		static bool IsElbowGuideStop(GestureMotionBound bound)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if ((int)bound.Kind == 0)
			{
				return bound.Reason.Contains("弯头到达导轨限位", StringComparison.Ordinal);
			}
			return false;
		}
	}

	public static HeldGestureRange? QueryHeldGestureRange(LevelDefinition level, BoardState state, PartId partId, AvailableOperationKind kind, GestureDomainOptions? translationOptions, GestureDomainOptions? rotationOptions, CancellationToken cancellationToken)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Invalid comparison between Unknown and I4
		LevelDefinition item = HeldGestureProjection.Create(level, state).Level;
		AvailableOperationResult val = CoreQueries.QueryAvailableOperation(item, state, partId, kind, translationOptions, rotationOptions, cancellationToken, true);
		if ((int)kind == 0)
		{
			val = ApplyElbowGuideStops(item, state, val, cancellationToken);
		}
		else if ((int)kind == 1 && rotationOptions == null)
		{
			val = ExtendCertifiedRotationRange(item, state, val, cancellationToken);
		}
		AvailableOperation operation = val.Operation;
		GestureMotionDomain val2 = ((operation != null) ? operation.MotionDomain : null);
		AvailableOperation operation2 = val.Operation;
		if (operation2 == null || (int)operation2.Availability > 0 || val2 == null || (int)val2.Completeness == 2)
		{
			return null;
		}
		return new HeldGestureRange(val2.MinimumSignedAmount, val2.MaximumSignedAmount);
	}

	public static AvailableOperationResult ExtendCertifiedRotationRange(LevelDefinition level, BoardState state, AvailableOperationResult result, CancellationToken cancellationToken)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		AvailableOperation operation = result.Operation;
		GestureMotionDomain domain = ((operation != null) ? operation.MotionDomain : null);
		if (operation == null || domain == null || (int)domain.MotionKind != 0)
		{
			return result;
		}
		GestureMotionBound val = Extend(domain.NegativeBound);
		GestureMotionBound val2 = Extend(domain.PositiveBound);
		if (val == domain.NegativeBound && val2 == domain.PositiveBound)
		{
			return result;
		}
		GestureMotionDomain val3 = new GestureMotionDomain(domain.BaseStateHash, domain.BaseStateRevision, domain.PartId, domain.MotionKind, domain.TranslationWorldAxis, domain.RotationPivot, val, val2, (GestureMotionDomainCompleteness)1, domain.Eligibility, domain.Reason);
		return new AvailableOperationResult(result.BaseStateHash, result.BaseStateRevision, result.Key, new AvailableOperation(operation.PartId, operation.Kind, operation.Availability, val3, operation.Blocker, operation.Reason), result.BaseStateAccepted, result.Reason);
		GestureMotionBound Extend(GestureMotionBound bound)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Expected O, but got Unknown
			if ((int)bound.Kind != 2 || bound.SafeAmount <= 1E-09 || bound.SafeAmount >= 359.999999999)
			{
				return bound;
			}
			double num = bound.SafeAmount;
			double num2 = Math.Min(360.0, num * 2.0);
			MotionPreview val4 = null;
			MotionPreview preview;
			while (IsFullySafe(num2, out preview))
			{
				num = num2;
				val4 = preview;
				if (num2 >= 359.999999999)
				{
					break;
				}
				num2 = Math.Min(360.0, num2 * 2.0);
			}
			if (num < num2)
			{
				for (int i = 0; i < 8; i++)
				{
					cancellationToken.ThrowIfCancellationRequested();
					double num3 = (num + num2) * 0.5;
					if (IsFullySafe(num3, out var preview2))
					{
						num = num3;
						val4 = preview2;
					}
					else
					{
						num2 = num3;
					}
				}
			}
			if (val4 != null && !(num <= bound.SafeAmount + 1E-06))
			{
				return new GestureMotionBound(bound.DirectionSign, (GestureMotionBoundKind)2, val4.GeometryStatus, num, num, (double?)null, val4.ReachablePose, (CollisionWitness)null, "客户端已扩展到 Core 能连续证明安全的旋转边界。", (double?)null);
			}
			return bound;
			bool IsFullySafe(double amount, out MotionPreview reference)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				reference = CoreRuntime.Preview(level, state, MotionRequest.Rotate(domain.PartId, (double)bound.DirectionSign * amount), cancellationToken);
				if (reference.CanApply && !reference.Result.Truncated)
				{
					return Math.Abs(reference.Result.ActualAmount) >= amount - 1E-07;
				}
				return false;
			}
		}
	}

	public static AvailableOperationResult RecoverCertifiedRotationRelease(LevelDefinition level, BoardState state, AvailableOperationResult result, CancellationToken cancellationToken)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		AvailableOperation operation = result.Operation;
		GestureMotionDomain domain = ((operation != null) ? operation.MotionDomain : null);
		if (operation == null || domain == null || (int)domain.MotionKind != 0)
		{
			return result;
		}
		GestureMotionBound val = Recover(domain.NegativeBound);
		GestureMotionBound val2 = Recover(domain.PositiveBound);
		if (val == domain.NegativeBound && val2 == domain.PositiveBound)
		{
			return result;
		}
		GestureMotionDomain val3 = new GestureMotionDomain(domain.BaseStateHash, domain.BaseStateRevision, domain.PartId, domain.MotionKind, domain.TranslationWorldAxis, domain.RotationPivot, val, val2, (GestureMotionDomainCompleteness)1, domain.Eligibility, domain.Reason);
		return new AvailableOperationResult(result.BaseStateHash, result.BaseStateRevision, result.Key, new AvailableOperation(operation.PartId, operation.Kind, operation.Availability, val3, operation.Blocker, operation.Reason), result.BaseStateAccepted, result.Reason);
		bool IsReleased(MotionPreview preview)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (preview.CanApply && preview.Result.State.Parts.TryGetValue(domain.PartId, out var value))
			{
				if (!value.Released)
				{
					return value.Removed;
				}
				return true;
			}
			return false;
		}
		GestureMotionBound Recover(GestureMotionBound bound)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Expected O, but got Unknown
			if ((int)bound.Kind != 2 || bound.SafeAmount >= 359.999999999)
			{
				return bound;
			}
			double num = bound.SafeAmount;
			MotionPreview val4 = null;
			double num2 = num;
			double[] array = new double[8] { 20.0, 16.0, 24.0, 12.0, 28.0, 8.0, 32.0, 4.0 };
			foreach (double num3 in array)
			{
				cancellationToken.ThrowIfCancellationRequested();
				num2 = Math.Min(360.0, num + num3);
				MotionPreview val5 = Preview(num2);
				if (IsReleased(val5))
				{
					val4 = val5;
					break;
				}
				if (num2 >= 359.999999999)
				{
					break;
				}
			}
			if (val4 != null)
			{
				for (int j = 0; j < 5; j++)
				{
					double num4 = (num + num2) * 0.5;
					MotionPreview val6 = Preview(num4);
					if (IsReleased(val6))
					{
						num2 = num4;
						val4 = val6;
					}
					else
					{
						num = num4;
					}
				}
				return new GestureMotionBound(bound.DirectionSign, (GestureMotionBoundKind)1, val4.GeometryStatus, num2, bound.SafeAmount, (double?)num2, val4.ReachablePose, val4.Result.Collision, "客户端已用 Core 接受的完整脱离姿态桥接旋转释放区间。", (double?)null);
			}
			return bound;
			MotionPreview Preview(double amount)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				return CoreRuntime.Preview(level, state, MotionRequest.Rotate(domain.PartId, (double)bound.DirectionSign * amount), cancellationToken);
			}
		}
	}

	public static AvailableOperationResult ApplyElbowGuideStops(LevelDefinition level, BoardState state, AvailableOperationResult result, CancellationToken cancellationToken)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Expected O, but got Unknown
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Expected O, but got Unknown
		AvailableOperation operation = result.Operation;
		GestureMotionDomain domain = ((operation != null) ? operation.MotionDomain : null);
		if (operation == null || domain == null || (int)domain.MotionKind != 1 || !domain.TranslationWorldAxis.HasValue)
		{
			return result;
		}
		PartDefinition movingPart = level.Parts.Single(delegate(PartDefinition part)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			PartId id = part.Id;
			return ((PartId)(ref id)).Equals(domain.PartId);
		});
		PartState movingState = state.Parts[domain.PartId];
		ElbowStopGeometry[] elbows = (from feature in movingPart.Features
			where feature.Tags.Contains<string>("elbow", StringComparer.Ordinal) || feature.Tags.Contains<string>("source-elbow", StringComparer.Ordinal)
			select (Feature: feature, Geometry: ShapeConverter.Convert(movingPart, movingState, feature)) into value
			where value.Geometry.Conversion.IsExact
			select new ElbowStopGeometry(value.Geometry.Conversion.Shape, PairedGuideFeature(value.Feature.Id))).ToArray();
		if (elbows.Length == 0)
		{
			return result;
		}
		GuideStopGeometry[] guides = (from value in level.Parts.Where(delegate(PartDefinition part)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				PartId id = part.Id;
				PartState value;
				return !((PartId)(ref id)).Equals(domain.PartId) && part.Behaviors.OfType<LinearGuideBehaviorDefinition>().Any() && state.Parts.TryGetValue(part.Id, out value) && !value.Released && !value.Removed;
			}).SelectMany((PartDefinition part) => from feature in part.Features
				where (int)feature.Channel == 1
				select (PartId: part.Id, FeatureId: feature.Id, Geometry: ShapeConverter.Convert(part, state.Parts[part.Id], feature)))
			where value.Geometry.Conversion.IsExact
			select new GuideStopGeometry(value.PartId, value.FeatureId, value.Geometry.Conversion.Shape)).ToArray();
		if (guides.Length == 0)
		{
			return result;
		}
		GestureMotionBound val = Stop(domain.NegativeBound);
		GestureMotionBound val2 = Stop(domain.PositiveBound);
		if (val == domain.NegativeBound && val2 == domain.PositiveBound)
		{
			return result;
		}
		GestureMotionDomain val3 = new GestureMotionDomain(domain.BaseStateHash, domain.BaseStateRevision, domain.PartId, domain.MotionKind, domain.TranslationWorldAxis, domain.RotationPivot, val, val2, domain.Completeness, domain.Eligibility, domain.Reason);
		return new AvailableOperationResult(result.BaseStateHash, result.BaseStateRevision, result.Key, new AvailableOperation(operation.PartId, operation.Kind, operation.Availability, val3, operation.Blocker, operation.Reason), result.BaseStateAccepted, result.Reason);
		bool IsActiveGuidePair(FeatureId elbowGuideFeature, GuideStopGeometry guide)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			return state.Constraints.Any(delegate(ConstraintRelation relation)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Invalid comparison between Unknown and I4
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0079: Unknown result type (might be due to invalid IL or missing references)
				//IL_007e: Unknown result type (might be due to invalid IL or missing references)
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				//IL_009e: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				//IL_00af: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_006c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
				if ((int)relation.Kind == 2)
				{
					PartId val4 = relation.FirstPart;
					FeatureId val5;
					if (((PartId)(ref val4)).Equals(movingPart.Id))
					{
						val5 = relation.FirstFeature;
						if (((FeatureId)(ref val5)).Equals(elbowGuideFeature))
						{
							val4 = relation.SecondPart;
							if (((PartId)(ref val4)).Equals(guide.PartId))
							{
								val5 = relation.SecondFeature;
								if (((FeatureId)(ref val5)).Equals(guide.FeatureId))
								{
									return true;
								}
							}
						}
					}
					val4 = relation.SecondPart;
					if (((PartId)(ref val4)).Equals(movingPart.Id))
					{
						val5 = relation.SecondFeature;
						if (((FeatureId)(ref val5)).Equals(elbowGuideFeature))
						{
							val4 = relation.FirstPart;
							if (((PartId)(ref val4)).Equals(guide.PartId))
							{
								val5 = relation.FirstFeature;
								return ((FeatureId)(ref val5)).Equals(guide.FeatureId);
							}
						}
					}
					return false;
				}
				return false;
			});
		}
		FeatureId? PairedGuideFeature(FeatureId elbowFeature)
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			string value = ((FeatureId)(ref elbowFeature)).Value;
			if (!value.EndsWith(".solid", StringComparison.Ordinal))
			{
				return null;
			}
			string text = value;
			int length = ".solid".Length;
			FeatureId candidate = new FeatureId(text.Substring(0, text.Length - length) + ".guide");
			if (!movingPart.Features.Any(delegate(FeatureDefinition feature)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Invalid comparison between Unknown and I4
				FeatureId id = feature.Id;
				return ((FeatureId)(ref id)).Equals(candidate) && (int)feature.Channel == 1;
			}))
			{
				return null;
			}
			return candidate;
		}
		GestureMotionBound Stop(GestureMotionBound bound)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			double commitAmount = bound.CommitAmount;
			if (commitAmount <= 1E-09)
			{
				return bound;
			}
			Vector2D axis = domain.TranslationWorldAxis.Value * (double)bound.DirectionSign;
			double num = 0.0;
			for (int i = 1; i <= 256; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				double num2 = commitAmount * (double)i / 256.0;
				if (Touches(num2))
				{
					double num3 = num;
					double num4 = num2;
					for (int j = 0; j < 16; j++)
					{
						double num5 = (num3 + num4) / 2.0;
						if (Touches(num5))
						{
							num4 = num5;
						}
						else
						{
							num3 = num5;
						}
					}
					return new GestureMotionBound(bound.DirectionSign, (GestureMotionBoundKind)0, (GeometryQueryStatus)0, bound.HorizonAmount, num3, (double?)num4, bound.ReachablePose, (CollisionWitness)null, "弯头到达导轨限位。", (double?)null);
				}
				num = num2;
			}
			return bound;
		}
		bool Touches(double amount)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			Vector2 delta = new Vector2(((Vector2D)(ref P_1.axis)).X * amount, ((Vector2D)(ref P_1.axis)).Y * amount);
			return elbows.Any((ElbowStopGeometry elbow) => guides.Any(delegate(GuideStopGeometry guide)
			{
				//IL_004e: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_0067: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				//IL_0075: Unknown result type (might be due to invalid IL or missing references)
				//IL_0076: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Invalid comparison between Unknown and I4
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				//IL_007c: Invalid comparison between Unknown and I4
				if (elbow.PairedGuideFeature.HasValue && IsActiveGuidePair(elbow.PairedGuideFeature.Value, guide))
				{
					return false;
				}
				CollisionWitness val4 = GeometryQueries.Query(Translate(elbow.Shape, delta), guide.Shape);
				if ((int)((CollisionWitness)(ref val4)).Status == 0)
				{
					ContactKind kind = ((CollisionWitness)(ref val4)).Kind;
					return (int)kind == 1 || (int)kind == 2;
				}
				return false;
			}));
		}
	}

	public static AvailableOperationResult RefineFirstTranslationRelease(LevelDefinition level, BoardState state, AvailableOperationResult result, CancellationToken cancellationToken)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		AvailableOperation operation = result.Operation;
		GestureMotionDomain domain = ((operation != null) ? operation.MotionDomain : null);
		if (operation == null || domain == null || (int)domain.MotionKind != 1 || !domain.TranslationWorldAxis.HasValue)
		{
			return result;
		}
		GestureMotionBound val = Refine(domain.NegativeBound);
		GestureMotionBound val2 = Refine(domain.PositiveBound);
		if (val == domain.NegativeBound && val2 == domain.PositiveBound)
		{
			return result;
		}
		GestureMotionDomain val3 = new GestureMotionDomain(domain.BaseStateHash, domain.BaseStateRevision, domain.PartId, domain.MotionKind, domain.TranslationWorldAxis, domain.RotationPivot, val, val2, domain.Completeness, domain.Eligibility, domain.Reason);
		return new AvailableOperationResult(result.BaseStateHash, result.BaseStateRevision, result.Key, new AvailableOperation(operation.PartId, operation.Kind, operation.Availability, val3, operation.Blocker, operation.Reason), result.BaseStateAccepted, result.Reason);
		GestureMotionBound Refine(GestureMotionBound bound)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Expected O, but got Unknown
			if ((int)bound.Kind != 1 || bound.ImpactAmount.HasValue || bound.CommitAmount <= 1E-09)
			{
				return bound;
			}
			double num = 0.0;
			double num2 = bound.CommitAmount;
			MotionPreview val4 = null;
			for (int i = 0; i < 14; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				double num3 = (num + num2) / 2.0;
				Vector2D val5 = domain.TranslationWorldAxis.Value * (double)bound.DirectionSign;
				MotionPreview val6 = CoreRuntime.Preview(level, state, MotionRequest.Translate(domain.PartId, num3, val5), cancellationToken);
				if (val6.CanApply && val6.Result.State.Parts.TryGetValue(domain.PartId, out var value) && (value.Released || value.Removed))
				{
					num2 = num3;
					val4 = val6;
				}
				else
				{
					num = num3;
				}
			}
			if (val4 != null)
			{
				return new GestureMotionBound(bound.DirectionSign, (GestureMotionBoundKind)1, bound.GeometryStatus, bound.HorizonAmount, num, (double?)num2, val4.ReachablePose, val4.Result.Collision, "客户端已细化到首次释放边界。", (double?)null);
			}
			return bound;
		}
	}

	public static AvailableOperationResult RecoverCertifiedTranslationRelease(LevelDefinition level, BoardState state, AvailableOperationResult result, CancellationToken cancellationToken)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Expected O, but got Unknown
		AvailableOperation operation = result.Operation;
		GestureMotionDomain domain = ((operation != null) ? operation.MotionDomain : null);
		if (operation == null || domain == null || (int)domain.MotionKind != 1 || !domain.TranslationWorldAxis.HasValue || !level.Parts.Single(delegate(PartDefinition part)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			PartId id = part.Id;
			return ((PartId)(ref id)).Equals(domain.PartId);
		}).Behaviors.OfType<ReleasableBehaviorDefinition>().Any())
		{
			return result;
		}
		GestureMotionBound val = Recover(domain.NegativeBound);
		GestureMotionBound val2 = Recover(domain.PositiveBound);
		if (val == domain.NegativeBound && val2 == domain.PositiveBound)
		{
			return result;
		}
		GestureMotionDomain val3 = new GestureMotionDomain(domain.BaseStateHash, domain.BaseStateRevision, domain.PartId, domain.MotionKind, domain.TranslationWorldAxis, domain.RotationPivot, val, val2, (GestureMotionDomainCompleteness)1, domain.Eligibility, domain.Reason);
		return new AvailableOperationResult(result.BaseStateHash, result.BaseStateRevision, result.Key, new AvailableOperation(operation.PartId, operation.Kind, operation.Availability, val3, operation.Blocker, operation.Reason), result.BaseStateAccepted, result.Reason);
		GestureMotionBound Recover(GestureMotionBound bound)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Expected O, but got Unknown
			if ((int)bound.Kind != 0 || bound.HorizonAmount <= bound.SafeAmount + 1E-06)
			{
				return bound;
			}
			double num = bound.HorizonAmount - bound.SafeAmount;
			for (int i = 1; i <= 32; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				double num2 = bound.SafeAmount + num * (double)i / 32.0;
				Vector2D val4 = domain.TranslationWorldAxis.Value * (double)bound.DirectionSign;
				MotionPreview val5 = CoreRuntime.Preview(level, state, MotionRequest.Translate(domain.PartId, num2, val4), cancellationToken);
				if (val5.CanApply && val5.Result.State.Parts.TryGetValue(domain.PartId, out var value) && (value.Released || value.Removed))
				{
					return new GestureMotionBound(bound.DirectionSign, (GestureMotionBoundKind)1, val5.GeometryStatus, bound.HorizonAmount, bound.SafeAmount, (double?)num2, val5.ReachablePose, val5.Result.Collision, "客户端已证明穿过 Threading 退出缝隙后的完整平移释放终点。", (double?)null);
				}
			}
			return bound;
		}
	}

	private static IGeometryShape Translate(IGeometryShape shape, Vector2 delta)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		if (!(shape is Circle val))
		{
			if (!(shape is Segment val2))
			{
				if (!(shape is Capsule val3))
				{
					if (!(shape is Obb val4))
					{
						if (!(shape is Arc val5))
						{
							if (!(shape is AnnularArc val6))
							{
								ConvexPolygon val7 = (ConvexPolygon)(object)((shape is ConvexPolygon) ? shape : null);
								if (val7 != null)
								{
									return (IGeometryShape)new ConvexPolygon((IReadOnlyList<Vector2>)val7.Vertices.Select((Vector2 point) => point + delta).ToArray());
								}
								return shape;
							}
							return (IGeometryShape)(object)new AnnularArc(((AnnularArc)(ref val6)).Center + delta, ((AnnularArc)(ref val6)).InnerRadius, ((AnnularArc)(ref val6)).OuterRadius, ((AnnularArc)(ref val6)).StartClockwiseRadians, ((AnnularArc)(ref val6)).ClockwiseSweepRadians);
						}
						return (IGeometryShape)(object)new Arc(((Arc)(ref val5)).Center + delta, ((Arc)(ref val5)).Radius, ((Arc)(ref val5)).StartClockwiseRadians, ((Arc)(ref val5)).ClockwiseSweepRadians);
					}
					return (IGeometryShape)(object)new Obb(((Obb)(ref val4)).Center + delta, ((Obb)(ref val4)).HalfExtents, ((Obb)(ref val4)).ClockwiseRadians);
				}
				Segment spine = ((Capsule)(ref val3)).Spine;
				Vector2 val8 = ((Segment)(ref spine)).Start + delta;
				spine = ((Capsule)(ref val3)).Spine;
				return (IGeometryShape)(object)new Capsule(new Segment(val8, ((Segment)(ref spine)).End + delta), ((Capsule)(ref val3)).Radius);
			}
			return (IGeometryShape)(object)new Segment(((Segment)(ref val2)).Start + delta, ((Segment)(ref val2)).End + delta);
		}
		return (IGeometryShape)(object)new Circle(((Circle)(ref val)).Center + delta, ((Circle)(ref val)).Radius);
	}
}
