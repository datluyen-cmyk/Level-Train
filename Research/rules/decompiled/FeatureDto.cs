using System;
using System.Collections.Generic;
using System.Linq;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class FeatureDto
{
	public string id;

	public string channel;

	public string anchorSpace;

	public ShapeDto shape;

	public string[] tags;

	public FeatureDefinition ToDomain()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		FeatureChannel result;
		FeatureAnchorSpace result2;
		return new FeatureDefinition(new FeatureId(id), (FeatureChannel)(Enum.TryParse<FeatureChannel>(channel, out result) ? ((int)result) : 0), (FeatureAnchorSpace)(Enum.TryParse<FeatureAnchorSpace>(anchorSpace, out result2) ? ((int)result2) : 0), shape?.ToDomain() ?? throw new InvalidOperationException("Feature shape is null"), (IEnumerable<string>)tags);
	}

	public static FeatureDto FromDomain(FeatureDefinition f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		FeatureDto featureDto = new FeatureDto();
		FeatureId val = f.Id;
		featureDto.id = ((FeatureId)(ref val)).Value;
		featureDto.channel = ((object)f.Channel/*cast due to .constrained prefix*/).ToString();
		featureDto.anchorSpace = ((object)f.AnchorSpace/*cast due to .constrained prefix*/).ToString();
		featureDto.shape = ShapeDto.FromDomain(f.Shape);
		featureDto.tags = f.Tags.ToArray();
		return featureDto;
	}
}
