using System;
using System.Linq;
using UnlockRing.Core.Domain;

[Serializable]
public sealed class LevelDto
{
	public string levelId;

	public int schemaVersion;

	public string rulesVersion;

	public string coordinates;

	public RuleDto rules;

	public PartDto[] parts;

	public ConstraintDto[] authoredConstraints;

	public LevelDefinition ToDomain()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		CoordinateConvention result;
		return new LevelDefinition(levelId, schemaVersion, rulesVersion, (CoordinateConvention)(Enum.TryParse<CoordinateConvention>(coordinates, out result) ? ((int)result) : 0), (parts ?? Array.Empty<PartDto>()).Select((PartDto p) => p.ToDomain()), rules?.ToDomain(), (authoredConstraints ?? Array.Empty<ConstraintDto>()).Select((ConstraintDto c) => c.ToDomain()));
	}

	public static LevelDto FromDomain(LevelDefinition lv)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new LevelDto
		{
			levelId = lv.LevelId,
			schemaVersion = lv.SchemaVersion,
			rulesVersion = lv.RulesVersion,
			coordinates = ((object)lv.Coordinates/*cast due to .constrained prefix*/).ToString(),
			rules = ((lv.Rules == null) ? null : RuleDto.FromDomain(lv.Rules)),
			parts = lv.Parts.Select(PartDto.FromDomain).ToArray(),
			authoredConstraints = lv.AuthoredConstraints.Select(ConstraintDto.FromDomain).ToArray()
		};
	}
}
