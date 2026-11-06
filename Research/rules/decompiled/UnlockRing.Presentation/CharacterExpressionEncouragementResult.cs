using System.Collections.Generic;
using RotateRings.GamePlay.Services.CharacterExpression;
using UnityEngine;

namespace UnlockRing.Presentation;

public sealed class CharacterExpressionEncouragementResult : IPresentationResult
{
	public IReadOnlyList<CharacterExpressionTriggerCondition> Conditions { get; }

	public Vector3 WorldPosition { get; }

	public int EffectiveFeedback { get; }

	public CharacterExpressionEncouragementResult(IReadOnlyList<CharacterExpressionTriggerCondition> conditions, Vector3 worldPosition, int effectiveFeedback)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Conditions = conditions;
		WorldPosition = worldPosition;
		EffectiveFeedback = effectiveFeedback;
	}
}
