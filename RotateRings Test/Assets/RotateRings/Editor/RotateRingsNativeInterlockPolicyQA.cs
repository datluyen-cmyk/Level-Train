#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class RotateRingsNativeInterlockPolicyQA
{
    [MenuItem("RotateRings/QA/Run Native Interlock Policy QA")]
    public static void Run()
    {
        var configuredSelf = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.MainBody,
            RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeBody,
            exactBinding: true,
            anyPartnerBinding: true,
            angleDelta: 31f,
            whitelist: true,
            otherHasRelativeMarker: true);
        Require(Ignores(configuredSelf), "Exact configured self collider should be ignored regardless of partner angle.");

        var unconfiguredPinSamePartner = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.MainBody,
            RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeBody,
            exactBinding: false,
            anyPartnerBinding: true,
            angleDelta: 0f,
            whitelist: true,
            otherHasRelativeMarker: true);
        Require(!Ignores(unconfiguredPinSamePartner), "An unconfigured pin on a configured partner must still block.");

        var wallAtFiveDegrees = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.MainBody,
            RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeWall,
            exactBinding: false,
            anyPartnerBinding: true,
            angleDelta: 5f,
            whitelist: true,
            otherHasRelativeMarker: true);
        Require(Ignores(wallAtFiveDegrees), "A compatible partner wall at the 5 degree boundary should be ignored.");

        var wallOutsideFiveDegrees = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.MainBody,
            RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeWall,
            exactBinding: false,
            anyPartnerBinding: true,
            angleDelta: 5.01f,
            whitelist: true,
            otherHasRelativeMarker: true);
        Require(!Ignores(wallOutsideFiveDegrees), "A partner wall outside the 5 degree window should block.");

        var incompatibleRelativeSource = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.RingRelative,
            RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeBody,
            exactBinding: true,
            anyPartnerBinding: true,
            angleDelta: 0f,
            whitelist: false,
            otherHasRelativeMarker: true);
        Require(!Ignores(incompatibleRelativeSource), "A relative query must not use a binding for a non-whitelisted collider.");

        var compatibleRelativeSource = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.RingRelative,
            RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeBody,
            exactBinding: true,
            anyPartnerBinding: true,
            angleDelta: 0f,
            whitelist: true,
            otherHasRelativeMarker: true);
        Require(Ignores(compatibleRelativeSource), "A relative query may use an exact binding for a whitelisted collider.");

        var unconfiguredHardPin = Facts(
            RotateRingsNativeInterlockPolicy.QuerySource.MainBody,
            RotateRingsNativeInterlockPolicy.OtherRole.MainBody,
            exactBinding: false,
            anyPartnerBinding: false,
            angleDelta: 0f,
            whitelist: true,
            otherHasRelativeMarker: false);
        Require(!Ignores(unconfiguredHardPin), "A hard-block pin collider must remain a blocker when unbound.");

        var rejectedUnsupportedBinding = false;
        try
        {
            var unsupported = Facts(
                RotateRingsNativeInterlockPolicy.QuerySource.RingRelative,
                RotateRingsNativeInterlockPolicy.OtherRole.RingRelativeBody,
                exactBinding: true,
                anyPartnerBinding: true,
                angleDelta: 0f,
                whitelist: true,
                otherHasRelativeMarker: true,
                bindingRingRelativeId: 1);
            Ignores(unsupported);
        }
        catch (NotSupportedException)
        {
            rejectedUnsupportedBinding = true;
        }
        Require(rejectedUnsupportedBinding, "Unsupported nonzero RingRelativeId bindings must fail clearly.");

        Debug.Log("RotateRings Native Interlock Policy QA passed.");
    }

    static RotateRingsNativeInterlockPolicy.Facts Facts(
        RotateRingsNativeInterlockPolicy.QuerySource source,
        RotateRingsNativeInterlockPolicy.OtherRole role,
        bool exactBinding,
        bool anyPartnerBinding,
        float angleDelta,
        bool whitelist,
        bool otherHasRelativeMarker,
        int bindingRingRelativeId = 0)
    {
        return new RotateRingsNativeInterlockPolicy.Facts(
            source: source,
            ownerHasRingRelatives: true,
            otherIsDifferentRing: true,
            otherHasRingRelativeMarker: otherHasRelativeMarker,
            role: role,
            otherIsNativeWhitelistCollider: whitelist,
            hasStaticBindings: true,
            exactStaticBinding: exactBinding,
            anyStaticBindingToPartner: anyPartnerBinding,
            partnerExistsAndUnsolved: true,
            partnerAngleDeltaDegrees: angleDelta,
            bindingRingRelativeId: bindingRingRelativeId);
    }

    static bool Ignores(RotateRingsNativeInterlockPolicy.Facts facts)
    {
        return RotateRingsNativeInterlockPolicy.ShouldIgnoreBlock(facts);
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
#endif
