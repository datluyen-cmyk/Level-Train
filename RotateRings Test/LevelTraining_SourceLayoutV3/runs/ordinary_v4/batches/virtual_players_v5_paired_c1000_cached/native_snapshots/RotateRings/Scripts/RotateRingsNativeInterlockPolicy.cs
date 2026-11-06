using System;

/// <summary>
/// Pure policy for the native RingPassiveBlockCheck.ShouldIgnoreBlock decision.
/// This models the source500 relation shape, where static bindings use
/// RingRelativeId == 0. Collider lookup and Unity overlap collection stay with
/// the caller.
/// </summary>
public static class RotateRingsNativeInterlockPolicy
{
    public enum QuerySource
    {
        MainBody,
        RingRelative
    }

    public enum OtherRole
    {
        MainBody,
        RingRelativeBody,
        RingRelativeWall,
        OtherRelative
    }

    public readonly struct Facts
    {
        public readonly QuerySource Source;
        public readonly bool OwnerHasRingRelatives;
        public readonly bool OtherIsDifferentRing;
        public readonly bool OtherHasRingRelativeMarker;
        public readonly OtherRole Role;
        public readonly bool OtherIsNativeWhitelistCollider;
        public readonly bool HasStaticBindings;
        public readonly bool ExactStaticBinding;
        public readonly bool AnyStaticBindingToPartner;
        public readonly bool PartnerExistsAndUnsolved;
        public readonly float PartnerAngleDeltaDegrees;
        public readonly int BindingRingRelativeId;

        public Facts(
            QuerySource source,
            bool ownerHasRingRelatives,
            bool otherIsDifferentRing,
            bool otherHasRingRelativeMarker,
            OtherRole role,
            bool otherIsNativeWhitelistCollider,
            bool hasStaticBindings,
            bool exactStaticBinding,
            bool anyStaticBindingToPartner,
            bool partnerExistsAndUnsolved,
            float partnerAngleDeltaDegrees,
            int bindingRingRelativeId = 0)
        {
            Source = source;
            OwnerHasRingRelatives = ownerHasRingRelatives;
            OtherIsDifferentRing = otherIsDifferentRing;
            OtherHasRingRelativeMarker = otherHasRingRelativeMarker;
            Role = role;
            OtherIsNativeWhitelistCollider = otherIsNativeWhitelistCollider;
            HasStaticBindings = hasStaticBindings;
            ExactStaticBinding = exactStaticBinding;
            AnyStaticBindingToPartner = anyStaticBindingToPartner;
            PartnerExistsAndUnsolved = partnerExistsAndUnsolved;
            PartnerAngleDeltaDegrees = partnerAngleDeltaDegrees;
            BindingRingRelativeId = bindingRingRelativeId;
        }
    }

    /// <summary>
    /// Returns the native ShouldIgnoreBlock result for a candidate collider.
    /// A false result means this policy considers the candidate a blocker.
    /// </summary>
    public static bool ShouldIgnoreBlock(Facts facts)
    {
        Validate(facts);

        if (CanSkipBlockForSource(facts))
            return true;

        if (IsInBinds(facts))
            return true;

        return CanIgnorePartnerWallInAngle(facts);
    }

    static bool CanSkipBlockForSource(Facts facts)
    {
        if (facts.Source != QuerySource.MainBody ||
            !facts.OtherIsDifferentRing ||
            !facts.OtherHasRingRelativeMarker ||
            !facts.OwnerHasRingRelatives)
            return false;

        // Native code first checks every owner RingRelative source for a binding.
        // In this source500 policy, registered RingRelativeId is always zero;
        // a relative probe is compatible only for native whitelist colliders.
        if (facts.HasStaticBindings && facts.ExactStaticBinding &&
            IsSourceCompatible(facts, QuerySource.RingRelative) &&
            IsBoundColliderWithinAngle(facts))
            return true;

        // Native body-source logic deliberately keeps physical relative body and
        // wall hits. Other marker roles are skipped by its source-specific rule.
        return facts.Role != OtherRole.RingRelativeBody &&
               facts.Role != OtherRole.RingRelativeWall;
    }

    static bool IsInBinds(Facts facts)
    {
        return facts.HasStaticBindings &&
               facts.ExactStaticBinding &&
               facts.PartnerExistsAndUnsolved &&
               IsSourceCompatible(facts, facts.Source) &&
               IsBoundColliderWithinAngle(facts);
    }

    static bool IsBoundColliderWithinAngle(Facts facts)
    {
        if (!facts.PartnerExistsAndUnsolved)
            return false;
        if (facts.Role != OtherRole.RingRelativeWall)
            return true;
        return Math.Abs(facts.PartnerAngleDeltaDegrees) <= 5f;
    }

    static bool CanIgnorePartnerWallInAngle(Facts facts)
    {
        return facts.Role == OtherRole.RingRelativeWall &&
               facts.HasStaticBindings &&
               facts.AnyStaticBindingToPartner &&
               facts.PartnerExistsAndUnsolved &&
               IsSourceCompatible(facts, facts.Source) &&
               Math.Abs(facts.PartnerAngleDeltaDegrees) <= 5f;
    }

    static bool IsSourceCompatible(Facts facts, QuerySource source)
    {
        // Every audited source500 static relation binding stores RingRelativeId=0.
        // The owner main-body source has ID zero. Relative sources use the native
        // whitelist-collider fallback and require an owner with relative probes.
        if (source == QuerySource.MainBody)
            return true;
        return facts.OwnerHasRingRelatives && facts.OtherIsNativeWhitelistCollider;
    }

    static void Validate(Facts facts)
    {
        if (facts.BindingRingRelativeId != 0)
            throw new NotSupportedException(
                "RotateRingsNativeInterlockPolicy supports only static bindings with RingRelativeId == 0.");
        if (facts.Source != QuerySource.MainBody && facts.Source != QuerySource.RingRelative)
            throw new NotSupportedException("Unsupported RingPassiveBlockCheck query source.");
        if (facts.Role != OtherRole.MainBody && facts.Role != OtherRole.RingRelativeBody &&
            facts.Role != OtherRole.RingRelativeWall && facts.Role != OtherRole.OtherRelative)
            throw new NotSupportedException("Unsupported native collider role.");
    }
}
