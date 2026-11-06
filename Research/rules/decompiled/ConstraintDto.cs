using System;
using System.Linq;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class ConstraintDto
{
	public string kind;

	public string firstPart;

	public string firstFeature;

	public string secondPart;

	public string secondFeature;

	public string activityKind;

	public ExemptionDto[] collisionExemptions;

	public AuthoredConstraintDefinition ToDomain()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		ConstraintRelationKind result;
		AuthoredConstraintActivityKind result2;
		return new AuthoredConstraintDefinition((ConstraintRelationKind)(Enum.TryParse<ConstraintRelationKind>(kind, out result) ? ((int)result) : 0), new PartId(firstPart), new FeatureId(firstFeature), new PartId(secondPart), new FeatureId(secondFeature), (AuthoredConstraintActivityKind)(Enum.TryParse<AuthoredConstraintActivityKind>(activityKind, out result2) ? ((int)result2) : 0), (collisionExemptions ?? Array.Empty<ExemptionDto>()).Select((ExemptionDto e) => e.ToDomain()));
	}

	public static ConstraintDto FromDomain(AuthoredConstraintDefinition ac)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		ConstraintDto obj = new ConstraintDto
		{
			kind = ((object)ac.Kind/*cast due to .constrained prefix*/).ToString()
		};
		PartId val = ac.FirstPart;
		obj.firstPart = ((PartId)(ref val)).Value;
		FeatureId val2 = ac.FirstFeature;
		obj.firstFeature = ((FeatureId)(ref val2)).Value;
		val = ac.SecondPart;
		obj.secondPart = ((PartId)(ref val)).Value;
		val2 = ac.SecondFeature;
		obj.secondFeature = ((FeatureId)(ref val2)).Value;
		obj.activityKind = ((object)ac.ActivityKind/*cast due to .constrained prefix*/).ToString();
		obj.collisionExemptions = ac.CollisionExemptions.Select(ExemptionDto.FromDomain).ToArray();
		return obj;
	}
}
