using System;
using System.Linq;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class PartDto
{
	public string id;

	public int authoredOrder;

	public PoseDto initialPose;

	public BehaviorDto[] behaviors;

	public FeatureDto[] features;

	public PartDefinition ToDomain()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		return new PartDefinition(new PartId(id), authoredOrder, initialPose?.ToDomain() ?? Pose2D.Identity, (features ?? Array.Empty<FeatureDto>()).Select((FeatureDto f) => f.ToDomain()), (behaviors ?? Array.Empty<BehaviorDto>()).Select((BehaviorDto b) => b.ToDomain()));
	}

	public static PartDto FromDomain(PartDefinition p)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		PartDto partDto = new PartDto();
		PartId val = p.Id;
		partDto.id = ((PartId)(ref val)).Value;
		partDto.authoredOrder = p.AuthoredOrder;
		partDto.initialPose = PoseDto.FromDomain(p.InitialPose);
		partDto.behaviors = p.Behaviors.Select(BehaviorDto.FromDomain).ToArray();
		partDto.features = p.Features.Select(FeatureDto.FromDomain).ToArray();
		return partDto;
	}
}
