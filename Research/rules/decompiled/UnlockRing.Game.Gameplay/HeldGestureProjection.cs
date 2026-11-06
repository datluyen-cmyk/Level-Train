using System.Collections.Generic;
using System.Linq;
using UnlockRing.Core.Domain;

namespace UnlockRing.Game.Gameplay;

public static class HeldGestureProjection
{
	public static (LevelDefinition Level, BoardState State) Create(LevelDefinition level, BoardState state)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00f7: Expected O, but got Unknown
		HashSet<string> activeKeys = state.Constraints.Select(StateRelationKey).ToHashSet();
		PartDefinition[] array = level.Parts.Select((PartDefinition part) => (PartDefinition)(part.Behaviors.OfType<ReleasableBehaviorDefinition>().Any() ? ((object)new PartDefinition(part.Id, part.AuthoredOrder, part.InitialPose, (IEnumerable<FeatureDefinition>)part.Features, part.Behaviors.Where((BehaviorDefinition behavior) => !(behavior is ReleasableBehaviorDefinition)))) : ((object)part))).ToArray();
		AuthoredConstraintDefinition[] array2 = level.AuthoredConstraints.Select((AuthoredConstraintDefinition definition) => (AuthoredConstraintDefinition)(((int)definition.Kind == 0 && activeKeys.Contains(DefinitionRelationKey(definition))) ? ((object)new AuthoredConstraintDefinition((ConstraintRelationKind)3, definition.FirstPart, definition.FirstFeature, definition.SecondPart, definition.SecondFeature, (AuthoredConstraintActivityKind)1, (IEnumerable<AuthoredCollisionExemptionDefinition>)definition.CollisionExemptions)) : ((object)definition))).ToArray();
		ConstraintRelation[] array3 = state.Constraints.Select((ConstraintRelation relation) => (ConstraintRelation)(((int)relation.Kind == 0) ? ((object)new ConstraintRelation((ConstraintRelationKind)3, relation.FirstPart, relation.FirstFeature, relation.SecondPart, relation.SecondFeature)) : ((object)relation))).ToArray();
		LevelDefinition val = new LevelDefinition(level.LevelId, level.SchemaVersion, level.RulesVersion, level.Coordinates, (IEnumerable<PartDefinition>)array, level.Rules, (IEnumerable<AuthoredConstraintDefinition>)array2);
		BoardState item = new BoardState(state.LevelId, state.SchemaVersion, state.RulesVersion, state.Revision, (IEnumerable<KeyValuePair<PartId, PartState>>)state.Parts, (IEnumerable<ConstraintRelation>)array3);
		return (Level: val, State: item);
	}

	private static string StateRelationKey(ConstraintRelation relation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return ComposeRelationKey(relation.Kind, relation.FirstPart, relation.FirstFeature, relation.SecondPart, relation.SecondFeature);
	}

	private static string DefinitionRelationKey(AuthoredConstraintDefinition definition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return ComposeRelationKey(definition.Kind, definition.FirstPart, definition.FirstFeature, definition.SecondPart, definition.SecondFeature);
	}

	private static string ComposeRelationKey(ConstraintRelationKind kind, PartId firstPart, FeatureId firstFeature, PartId secondPart, FeatureId secondFeature)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		string text = ((PartId)(ref firstPart)).Value + "/" + ((FeatureId)(ref firstFeature)).Value;
		string text2 = ((PartId)(ref secondPart)).Value + "/" + ((FeatureId)(ref secondFeature)).Value;
		if (string.CompareOrdinal(text, text2) > 0)
		{
			return $"{kind}:{text2}|{text}";
		}
		return $"{kind}:{text}|{text2}";
	}
}
