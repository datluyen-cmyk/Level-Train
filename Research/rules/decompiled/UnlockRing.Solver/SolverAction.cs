using System;
using UnlockRing.Core.Domain;

namespace UnlockRing.Solver;

public sealed record SolverAction
{
	public string StableKey { get; }

	public SolverActionKind Kind { get; }

	public string PartId { get; }

	public double MovementDistance { get; }

	public int IrreversibleRisk { get; }

	public string Reason { get; }

	public DecisionLiteral? Decision { get; }

	public MotionRequest? Motion { get; }

	public PartId? ForceRemovePart { get; }

	public string? BombSourcePartId { get; }

	public SolverAction(string stableKey, SolverActionKind kind, string partId, double movementDistance, int irreversibleRisk = 0, string reason = "", DecisionLiteral? decision = null, MotionRequest? motion = null, PartId? forceRemovePart = null, string? bombSourcePartId = null)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		Compatibility.ThrowIfNullOrWhiteSpace(stableKey, "stableKey");
		Compatibility.ThrowIfNullOrWhiteSpace(partId, "partId");
		if (!Compatibility.IsFinite(movementDistance) || movementDistance < 0.0)
		{
			throw new ArgumentOutOfRangeException("movementDistance");
		}
		if (irreversibleRisk < 0)
		{
			throw new ArgumentOutOfRangeException("irreversibleRisk");
		}
		if (kind == SolverActionKind.BombDecision && (object)decision == null)
		{
			throw new ArgumentException("Bomb decisions require a decision literal.", "decision");
		}
		if (kind == SolverActionKind.BombDecision && irreversibleRisk == 0)
		{
			throw new ArgumentException("Bomb decisions must carry non-zero irreversible risk.", "irreversibleRisk");
		}
		if (kind != SolverActionKind.BombDecision && (object)decision != null)
		{
			throw new ArgumentException("Only bomb decisions may carry decision literals in rules version 1.", "decision");
		}
		if (motion != null && forceRemovePart.HasValue)
		{
			throw new ArgumentException("An action can carry either a motion or a force-remove payload, never both.");
		}
		PartId val;
		if (motion != null)
		{
			val = motion.PartId;
			if (!string.Equals(((PartId)(ref val)).Value, partId, StringComparison.Ordinal))
			{
				throw new ArgumentException("Motion payload part must match the action part.", "motion");
			}
		}
		if (forceRemovePart.HasValue)
		{
			val = forceRemovePart.Value;
			if (!string.Equals(((PartId)(ref val)).Value, partId, StringComparison.Ordinal))
			{
				throw new ArgumentException("Force-remove payload part must match the action part.", "forceRemovePart");
			}
		}
		if (kind != SolverActionKind.BombDecision && bombSourcePartId != null)
		{
			throw new ArgumentException("Only bomb decisions may identify a bomb source.", "bombSourcePartId");
		}
		if (bombSourcePartId != null)
		{
			Compatibility.ThrowIfNullOrWhiteSpace(bombSourcePartId, "bombSourcePartId");
		}
		StableKey = stableKey;
		Kind = kind;
		PartId = partId;
		MovementDistance = movementDistance;
		IrreversibleRisk = irreversibleRisk;
		Reason = reason ?? string.Empty;
		Decision = decision;
		Motion = motion;
		ForceRemovePart = forceRemovePart;
		BombSourcePartId = bombSourcePartId;
	}
}
