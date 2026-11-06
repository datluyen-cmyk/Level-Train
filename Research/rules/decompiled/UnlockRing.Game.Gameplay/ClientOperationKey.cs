using System;
using System.Collections.Generic;
using UnlockRing.Core.Domain;
using UnlockRing.Core.Queries;

namespace UnlockRing.Game.Gameplay;

public readonly struct ClientOperationKey : IEquatable<ClientOperationKey>
{
	public PartId PartId { get; }

	public AvailableOperationKind Kind { get; }

	public ClientOperationKey(PartId PartId, AvailableOperationKind Kind)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		this.PartId = PartId;
		this.Kind = Kind;
	}

	public void Deconstruct(out PartId PartId, out AvailableOperationKind Kind)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected I4, but got Unknown
		PartId = this.PartId;
		Kind = (AvailableOperationKind)(int)this.Kind;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return $"ClientOperationKey {{ PartId = {PartId}, Kind = {Kind} }}";
	}

	public bool Equals(ClientOperationKey other)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (EqualityComparer<PartId>.Default.Equals(PartId, other.PartId))
		{
			return EqualityComparer<AvailableOperationKind>.Default.Equals(Kind, other.Kind);
		}
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ClientOperationKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return EqualityComparer<PartId>.Default.GetHashCode(PartId) * -1521134295 + EqualityComparer<AvailableOperationKind>.Default.GetHashCode(Kind);
	}

	public static bool operator ==(ClientOperationKey left, ClientOperationKey right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ClientOperationKey left, ClientOperationKey right)
	{
		return !left.Equals(right);
	}
}
