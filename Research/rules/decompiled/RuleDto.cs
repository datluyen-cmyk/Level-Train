using System;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class RuleDto
{
	public bool forceRemovalEnabled;

	public double contactTolerance;

	public LevelRuleDefinition ToDomain()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		return new LevelRuleDefinition(forceRemovalEnabled, contactTolerance);
	}

	public static RuleDto FromDomain(LevelRuleDefinition r)
	{
		return new RuleDto
		{
			forceRemovalEnabled = r.ForceRemovalEnabled,
			contactTolerance = r.ContactTolerance
		};
	}
}
