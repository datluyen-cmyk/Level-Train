using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rebuilds the source FixedObject quad and FixedButterflyMeshPart render forms.
/// Catch objects stay flat source quads with BrowCatchSprite; this helper never
/// substitutes a ring or inferred collider shape for their render geometry.
/// </summary>
public static class OriginalCatchVisualAssets
{
    public static void ApplyLocalPose(Transform target,RotateRingsCatchQuad quad)
    {
        target.localPosition=quad.localPosition;
        target.localRotation=Quaternion.Euler(quad.localEuler);
        target.localScale=quad.localScale;
    }
    /// <summary>
    /// Builds one serialized catch render quad. Transform pose is intentionally
    /// kept in the DTO for the caller to apply to the child GameObject; localCenter
    /// and zOffset are baked into FixedButterflyMeshPart vertices as in source.
    /// </summary>
    public static Mesh CreateCatchMesh(RotateRingsCatchQuad quad, Material resolvedMaterial = null)
    {
        if (quad == null) throw new ArgumentNullException(nameof(quad));
        if (!string.IsNullOrEmpty(quad.sourceClass) &&
            quad.sourceClass != "QuadPlaneController" && quad.sourceClass != "FixedButterflyMeshPart")
            throw new NotSupportedException("Unsupported source catch renderer: " + quad.sourceClass);
        if (quad.role < -1 || quad.role > 1)
            throw new NotSupportedException("Unsupported source catch quad role: " + quad.role);
        if (quad.sourceClass == "FixedButterflyMeshPart" && quad.role < 0)
            throw new ArgumentException("A butterfly mesh part must identify its bridge or slot role.", nameof(quad));
        if (quad.sourceClass == "QuadPlaneController" && (quad.orientation < 0 || quad.orientation > 1))
            throw new NotSupportedException("Unsupported QuadPlaneController orientation: " + quad.orientation);
        if (quad.width <= 0f || quad.height <= 0f)
            throw new ArgumentException("A source catch quad requires positive serialized width and height.", nameof(quad));

        Color32 color = ToColor32(quad.vertexColor);
        if (quad.role == 1)
            return CreateButterflySlotMesh(quad, color, resolvedMaterial);

        return CreatePlainQuad(quad.width, quad.height,
            quad.orientation, quad.localCenter, quad.zOffset, color,
            quad.role == 0 ? "OriginalButterflyBridge" : "OriginalFixedCatchQuad");
    }

    /// <summary>
    /// Creates the genuine BrowCatchSprite material through the existing source
    /// shader/texture port. Callers may instead resolve the serialized material
    /// reference from the source scene.
    /// </summary>
    public static Material CreateCatchMaterial(RotateRingsCatchQuad quad,
        Func<RotateRingsOriginalMaterialRef, Material> materialResolver = null)
    {
        if (quad == null) throw new ArgumentNullException(nameof(quad));
        Material source = materialResolver != null && quad.sourceMaterial != null
            ? materialResolver(quad.sourceMaterial)
            : null;
        if (source != null) return new Material(source) { name = "OriginalCatchSourceMaterial" };
        if (quad.role == 0)
            throw new InvalidOperationException("A butterfly bridge requires its serialized body material; no source material resolver returned it.");
        return OriginalRingVisualAssets.CreateMaterial(Color.white, true);
    }

    /// <summary>Creates each quad's material without combining separate source renderers.</summary>
    public static Material[] CreateCatchMaterials(RotateRingsCatchVisual visual,
        Func<RotateRingsOriginalMaterialRef, Material> materialResolver = null)
    {
        if (visual == null) throw new ArgumentNullException(nameof(visual));
        var quads = visual.quads ?? Array.Empty<RotateRingsCatchQuad>();
        var result = new Material[quads.Length];
        for (int i = 0; i < quads.Length; i++) result[i] = CreateCatchMaterial(quads[i], materialResolver);
        return result;
    }

    /// <summary>
    /// Ports QuadPlaneController.SetupWidthAndHeight semantics: dimensions are
    /// clamped to 0.001 and a fresh source-style quad is returned for runtime setup.
    /// </summary>
    public static Mesh SetupWidthAndHeight(float width, float height, int orientation = 0,
        Color? vertexColor = null)
    {
        return CreatePlainQuad(Mathf.Max(.001f, width), Mathf.Max(.001f, height),
            orientation, Vector3.zero, 0f, ToColor32(vertexColor ?? Color.white), "OriginalFixedCatchQuad");
    }

    static Mesh CreateButterflySlotMesh(RotateRingsCatchQuad quad, Color32 color, Material material)
    {
        // FixedButterflyMeshPart derives natural slot length from the catch sprite's aspect ratio.
        // A missing texture leaves the serialized rectangle intact rather than inventing dimensions.
        Texture tex = material != null ? material.mainTexture : null;
        float naturalLength = tex != null && tex.width > 0
            ? quad.height * tex.height / tex.width
            : 0f;
        if (naturalLength <= .0001f || quad.capStartLength + quad.capEndLength <= 0f)
            return CreatePlainQuad(quad.width, quad.height, 0, quad.localCenter,
                quad.zOffset, color, "OriginalButterflySlot");

        float capStart, capEnd;
        ResolveCapLengths(quad.width, naturalLength, quad.capStartLength, quad.capEndLength,
            out capStart, out capEnd);
        float halfHeight = quad.height * .5f;
        float left = quad.localCenter.x - quad.width * .5f;
        float[] xs = { left, left + capStart, left + quad.width - capEnd, left + quad.width };
        float[] u = { 0f, capStart / naturalLength, 1f - capEnd / naturalLength, 1f };
        var vertices = new Vector3[8];
        var uv = new Vector2[8];
        for (int row = 0; row < 2; row++)
        {
            // Source builds row 0 at the top and maps it to UV x = 0.
            float y = quad.localCenter.y + (row == 0 ? halfHeight : -halfHeight);
            for (int col = 0; col < 4; col++)
            {
                int i = row * 4 + col;
                vertices[i] = new Vector3(xs[col], y, quad.zOffset);
                uv[i] = new Vector2(row, u[col]);
            }
        }
        var triangles = new List<int>(18);
        for (int col = 0; col < 3; col++)
        {
            int a = col, b = col + 1, c = col + 4, d = col + 5;
            triangles.Add(a); triangles.Add(c); triangles.Add(b);
            triangles.Add(c); triangles.Add(d); triangles.Add(b);
        }
        var mesh = new Mesh { name = "OriginalButterflySlot" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();
        var colors = new Color32[vertices.Length];
        for (int i = 0; i < colors.Length; i++) colors[i] = color;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    static Mesh CreatePlainQuad(float width, float height, int orientation,
        Vector3 center, float zOffset, Color32 color, string meshName)
    {
        width = Mathf.Max(.001f, width);
        height = Mathf.Max(.001f, height);
        float hw = width * .5f, hh = height * .5f;
        Vector3[] vertices;
        if (orientation == 1)
        {
            // QuadPlaneController orientation 1 lies in XZ; source orientation 0 lies in XY.
            vertices = new[] {
                new Vector3(center.x-hw, center.y, center.z-hh+zOffset),
                new Vector3(center.x+hw, center.y, center.z-hh+zOffset),
                new Vector3(center.x-hw, center.y, center.z+hh+zOffset),
                new Vector3(center.x+hw, center.y, center.z+hh+zOffset)
            };
        }
        else
        {
            vertices = new[] {
                new Vector3(center.x-hw, center.y-hh, center.z+zOffset),
                new Vector3(center.x+hw, center.y-hh, center.z+zOffset),
                new Vector3(center.x-hw, center.y+hh, center.z+zOffset),
                new Vector3(center.x+hw, center.y+hh, center.z+zOffset)
            };
        }
        var mesh = new Mesh { name = meshName };
        mesh.vertices = vertices;
        // QuadPlaneController and FixedButterflyMeshPart source UVs put the
        // U change on the vertical edge; preserve that authored orientation.
        mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.right, Vector2.one };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.colors32 = new[] { color, color, color, color };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    static void ResolveCapLengths(float width, float naturalLength, float sourceStart, float sourceEnd,
        out float start, out float end)
    {
        float sum = sourceStart + sourceEnd;
        if (sum <= 0f) { start = end = 0f; return; }
        if (naturalLength >= sum) { start = sourceStart; end = sourceEnd; }
        else { start = naturalLength * sourceStart / sum; end = naturalLength - start; }
        float max = width * .999f;
        if (start + end > max)
        {
            float scale = max / (start + end);
            start *= scale;
            end *= scale;
        }
    }

    static Color32 ToColor32(Color color)
    {
        if (QualitySettings.activeColorSpace == ColorSpace.Linear) color = color.linear;
        return (Color32)color;
    }
}


