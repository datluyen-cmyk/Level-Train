using System;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class ExemptionDto
{
	public string firstPart;

	public string firstFeature;

	public string secondPart;

	public string secondFeature;

	public AuthoredCollisionExemptionDefinition ToDomain()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		return new AuthoredCollisionExemptionDefinition(new PartId(firstPart), new FeatureId(firstFeature), new PartId(secondPart), new FeatureId(secondFeature));
	}

	public static ExemptionDto FromDomain(AuthoredCollisionExemptionDefinition ex)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		ExemptionDto exemptionDto = new ExemptionDto();
		PartId val = ex.FirstPart;
		exemptionDto.firstPart = ((PartId)(ref val)).Value;
		FeatureId val2 = ex.FirstFeature;
		exemptionDto.firstFeature = ((FeatureId)(ref val2)).Value;
		val = ex.SecondPart;
		exemptionDto.secondPart = ((PartId)(ref val)).Value;
		val2 = ex.SecondFeature;
		exemptionDto.secondFeature = ((FeatureId)(ref val2)).Value;
		return exemptionDto;
	}
}
