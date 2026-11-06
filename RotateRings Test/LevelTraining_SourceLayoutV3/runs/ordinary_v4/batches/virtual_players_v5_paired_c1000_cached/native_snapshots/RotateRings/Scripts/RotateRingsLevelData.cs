using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class RotateRingsLevelPack { public int schemaVersion;public bool complete,sourceOriginalOnly; public List<RotateRingsLevel> levels = new List<RotateRingsLevel>(); }
[Serializable] public sealed class RotateRingsLevel {
    public int id; public string title; public string referenceId; public int parMoves; public float difficulty;
    public bool sourceReference,sourceOnlyParityFixture;
    public int position; public string sourceId;
    public RotateRingsGuideLink[] guideLinks;
    public RotateRingsBomb[] bombs;
    public List<RotateRingsPart> parts = new List<RotateRingsPart>();
    public List<RotateRingsLink> links = new List<RotateRingsLink>();
    public List<RotateRingsGesture> solution = new List<RotateRingsGesture>();
}
[Serializable] public sealed class RotateRingsPart {
    public string templateId;
    public int id; public string kind; public float x; public float y; public float z; public float radius; public float width; public float gap;
    public string sourceId; public bool scoreEligible; public int chainLockCount;
    public float angle; public int color; public bool collectible; public int key; public int lockKey; public bool bomb;
    public float axis; public float length;
    public RotateRingsSourceColliderBinding[] sourceColliderBindings;
    public RotateRingsRigidbody sourceRigidbody;
    public RotateRingsPhysicalShape[] inputShapes;
}
[Serializable] public sealed class RotateRingsRigidbody {
    public bool present,simulated,useFullKinematicContacts,useAutoMass;
    public long sourceComponentId;public int bodyType,includeLayers,excludeLayers,interpolation,sleepMode,collisionDetection,constraints;
    public float mass,linearDamping,angularDamping,gravityScale;
    public RotateRingsOriginalMaterialRef physicsMaterial;
}
[Serializable] public sealed class RotateRingsShapePath { public Vector2[] points; }
[Serializable] public sealed class RotateRingsBomb {
    public string sourceName;public int ownerPartId=-1;public long sourceColliderId;
    public Vector2 worldPosition;public RotateRingsPhysicalShape physicalShape;
    public RotateRingsCatchVisual catchVisual;
}
[Serializable] public sealed class RotateRingsPhysicalShape {
    public long sourceColliderId; public string sourceRole; public float localZ;
    public string role; public int attachmentId=-1; public bool trigger;
    public string shapeType,sourceColliderType; public Vector2 center; public float radius;
    public int sourceLayer;public bool sourceEnabled=true;
    public float edgeRadius;public RotateRingsOriginalMaterialRef physicsMaterial;
    public Vector2[] points; public RotateRingsShapePath[] paths;
}
[Serializable] public sealed class RotateRingsVisualAttachment {
    public int id; public int type; public string kind; public float angleDeg;
    public float stemLength,stemWidth,stemInsertOffset,crossLength,crossWidth,crossCapLength;
    public bool isInnerMount; public int styleIndex;
}
[Serializable] public sealed class RotateRingsOriginalTemplate {
    public string id; public string kind; public float radius,width,gap,length;
    public float visualRadius,visualWidth,visualGap,gapDirectionDeg,capRatio;
    public bool extraEnabled;
    public float tailLength,headLength,bodyTileLength;
    public int styleIndex,arcSubdivisions;
    public RotateRingsPhysicalShape[] physicalShapes;
    public RotateRingsVisualAttachment[] attachments;
    public RotateRingsPullVisual pullVisual;
    public RotateRingsOriginalMovement movement;
    public RotateRingsCatchVisual catchVisual;
    public RotateRingsOriginalMaterialRef sourceMaterial;
    public Color sourceVertexColor=Color.white;
    public float ringZ,stemZ,crossBarZ=-.2f;
}
[Serializable] public sealed class RotateRingsSourceColliderBinding {
    public long sourceColliderId;public int shapeIndex;public string sourceFeatureId,sourceRole;
}
[Serializable] public sealed class RotateRingsGuideLink {
    public int guideOwnerId,lockedOwnerId;public string hostPartId,guidePartId,lockedPartId;
    public int[] guideShapeIndices;
}
[Serializable] public sealed class RotateRingsCatchVisual { public RotateRingsCatchQuad[] quads; }
[Serializable] public sealed class RotateRingsCatchQuad {
    public int role=-1,orientation,bridgeFacingLocalXSign;
    public float width,height,zOffset,capStartLength,capEndLength;
    public Vector3 localCenter,localPosition,localEuler,localScale=Vector3.one;
    public Color vertexColor=Color.white;
    public string sourceClass;
    public RotateRingsOriginalMaterialRef sourceMaterial;
}
[Serializable] public sealed class RotateRingsOriginalMaterialRef { public int fileId;public long pathId; }
[Serializable] public sealed class RotateRingsPullBuildSettings {
    public float width,cornerRadius,exitExtraLength;
    public int cornerSegments,widthSubdivisions,styleIndex;
}
[Serializable] public sealed class RotateRingsPullDecoration {
    public float LocalRibbonDistance,LateralOffset,RotationOffset,Scale=1;
    public RotateRingsOriginalMaterialRef sprite;
}
[Serializable] public sealed class RotateRingsPullVisual {
    public Vector3[] pathPoints;
    public RotateRingsPullBuildSettings buildSettings;
    public Color vertexColor=Color.white;
    public RotateRingsPullDecoration[] decorations;
    public RotateRingsOriginalMaterialRef sourceMaterial;
}
[Serializable] public sealed class RotateRingsOriginalMovement {
    public int lockDirectionType;
    public Vector2 axisLocal;
    public bool lockLeft,lockRight,lockUp,lockDown;
}
[Serializable] public sealed class RotateRingsTemplateCatalog {
    public int schemaVersion; public RotateRingsOriginalTemplate[] templates;
}
[Serializable] public sealed class RotateRingsLink {
    public int source,target,relationKind,activityKind; public float bearing,pinWidth; public int attachmentId;
    public string sourceFeature,targetFeature; public int[] sourceShapeIndices,targetShapeIndices;
}
[Serializable] public sealed class RotateRingsGesture { public int part; public float angle; public float delta; public string kind; }

public static class RotateRingsRules {
    // JSON stores decimal degrees, while Unity consumes Float32. This tolerance covers only
    // binary representation error at an inclusive threshold (15.0 can decode as 14.999998).
    public const float GestureThresholdEpsilonDegrees = 0.0001f;
    public static float Delta(float a, float b) => Mathf.DeltaAngle(a, b);
    public static bool MeetsGestureThreshold(float delta, float threshold) =>
        Mathf.Abs(delta) + GestureThresholdEpsilonDegrees >= threshold;
    public static bool IsSlider(RotateRingsPart p) => p != null && string.Equals(p.kind, "slider", StringComparison.OrdinalIgnoreCase);
    public static bool IsMovable(RotateRingsPart p) => p != null && (p.kind=="ring"||IsSlider(p));
    public static Vector2 SlideAxis(RotateRingsPart p,RotateRingsOriginalTemplate template) {
        if(template.movement!=null){
            if(template.movement.axisLocal.sqrMagnitude<.99f)throw new InvalidOperationException("Unsupported custom movement must not be replaced by a default axis: "+p.templateId);
            return (Vector2)(Quaternion.Euler(0,0,p.angle)*(Vector3)template.movement.axisLocal.normalized);
        }
        return new Vector2(Mathf.Cos(p.axis*Mathf.Deg2Rad),Mathf.Sin(p.axis*Mathf.Deg2Rad));
    }

    public static bool IsLinkActive(RotateRingsLink link, RotateRingsPart source, RotateRingsPart target,
        float sourceAngle, float targetAngle, bool sourceRemoved, bool targetRemoved) {
        if (sourceRemoved || targetRemoved || source == null || target == null) return false;
        if (IsSlider(target)) return true;
        float currentBearing = link.bearing + Delta(source.angle, sourceAngle);
        return Mathf.Abs(Delta(targetAngle, currentBearing)) > Mathf.Max(0f, target.gap * .5f - 9f);
    }
}
