using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Builds the static visual mesh for an original PullObject from its recovered
/// ArrowRibbonController path and build settings. The path is source data; this
/// class never substitutes collider bounds or a circle for missing render data.
/// </summary>
public static class OriginalPullVisualAssets
{
    const float EpsilonSqr = 1e-8f;

    /// <summary>
    /// Ports ArrowPathMeshBuilder's point cleanup, corner rounding, exit extension,
    /// width subdivision, tangent, vertex-color and UV generation for a static pull.
    /// </summary>
    public static Mesh CreatePullMesh(RotateRingsPullVisual visual, Color fallbackColor)
    {
        if (visual == null) throw new ArgumentNullException(nameof(visual));
        var settings = visual.buildSettings;
        if (settings == null)
            throw new ArgumentException("An original PullObject is missing its serialized ArrowRibbonBuildSettings.", nameof(visual));
        var input = visual.pathPoints;
        if (input == null || input.Length < 2)
            throw new ArgumentException("An original PullObject needs its serialized ArrowRibbonController path.", nameof(visual));

        var points = CleanPoints(input);
        var centerline = BuildRoundedCenterline(points, Mathf.Max(0f, settings.cornerRadius),
            Mathf.Max(1, settings.cornerSegments));
        if (centerline.Count < 2)
            throw new ArgumentException("The serialized PullObject path has no usable length.", nameof(visual));

        float visibleLength = PolylineLength(centerline);
        int visibleEndIndex = centerline.Count - 1;
        float extra = Mathf.Max(0f, settings.exitExtraLength);
        if (extra > .0001f)
        {
            Vector3 direction = centerline[centerline.Count - 1] - centerline[centerline.Count - 2];
            if (direction.sqrMagnitude > EpsilonSqr)
                centerline.Add(centerline[centerline.Count - 1] + direction.normalized * extra);
        }

        Color sourceColor = visual.vertexColor;
        if (sourceColor == default(Color)) sourceColor = fallbackColor;
        Color32 vertexColor = QualitySettings.activeColorSpace == ColorSpace.Linear
            ? (Color32)sourceColor.linear
            : (Color32)sourceColor;
        if (vertexColor.a == 0 && vertexColor.r == 0 && vertexColor.g == 0 && vertexColor.b == 0)
            vertexColor = new Color32(255, 255, 255, 255);

        int acrossCount = Mathf.Clamp(settings.widthSubdivisions, 0, 8) + 2;
        float halfWidth = Mathf.Max(0f, settings.width) * .5f;
        int count = centerline.Count;
        var vertices = new List<Vector3>(count * acrossCount);
        var normals = new List<Vector3>(count * acrossCount);
        var tangents = new List<Vector4>(count * acrossCount);
        var colors = new List<Color32>(count * acrossCount);
        var uv0 = new List<Vector2>(count * acrossCount);
        var uv1 = new List<Vector2>(count * acrossCount);
        var uv2 = new List<Vector2>(count * acrossCount);
        var triangles = new List<int>((count - 1) * (acrossCount - 1) * 6);

        var distances = new float[count];
        for (int i = 1; i < count; i++)
            distances[i] = distances[i - 1] + Vector3.Distance(centerline[i - 1], centerline[i]);
        float originalLength = distances[visibleEndIndex];
        float distanceScale = originalLength > .0001f ? visibleLength / originalLength : 1f;

        for (int i = 0; i < count; i++)
        {
            Vector3 tangent = ComputeTangent(centerline, i);
            Vector3 side = Vector3.Cross(Vector3.forward, tangent).normalized;
            for (int j = 0; j < acrossCount; j++)
            {
                float across = (float)j / (acrossCount - 1);
                float offset = halfWidth - across * 2f * halfWidth;
                vertices.Add(centerline[i] + side * offset);
                normals.Add(Vector3.forward);
                tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, -1f));
                colors.Add(vertexColor);
                uv0.Add(new Vector2(distances[i] * distanceScale, across));
                uv1.Add(new Vector2(visibleLength, settings.styleIndex + 1));
                uv2.Add(new Vector2(1f, 0f));
            }
        }

        for (int row = 0; row < count - 1; row++)
        {
            int first = row * acrossCount;
            int next = (row + 1) * acrossCount;
            for (int column = 0; column < acrossCount - 1; column++)
            {
                int a = first + column;
                int b = next + column;
                int c = first + column + 1;
                int d = next + column + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(c); triangles.Add(b); triangles.Add(d);
            }
        }

        var mesh = new Mesh { name = "OriginalPullArrowRibbon" };
        if (vertices.Count > 65535) mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetColors(colors);
        mesh.SetUVs(0, uv0);
        mesh.SetUVs(1, uv1);
        mesh.SetUVs(2, uv2);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    /// <summary>Clones a resolved source material before applying PullObject runtime values.</summary>
    public static Material CreatePullMaterial(Material sourceMaterial, RotateRingsPullVisual visual)
    {
        if (sourceMaterial == null) throw new ArgumentNullException(nameof(sourceMaterial));
        var material = new Material(sourceMaterial) { name = sourceMaterial.name + " (Original Pull)" };
        ApplyPullMaterialSettings(material, visual);
        return material;
    }

    /// <summary>
    /// Matches ArrowRibbonController.Build followed by ApplyCapLengthsFromBuildWidth.
    /// All other properties remain those of the exact resolved source material.
    /// </summary>
    public static void ApplyPullMaterialSettings(Material material, RotateRingsPullVisual visual)
    {
        if (material == null || visual == null) return;
        float width = Mathf.Max(0f, visual.buildSettings.width);
        if (material.HasProperty("_TailLength")) material.SetFloat("_TailLength", width);
        if (material.HasProperty("_HeadLength")) material.SetFloat("_HeadLength", width);
        if (material.HasProperty("_WindowStart")) material.SetFloat("_WindowStart", 0f);
    }

    static List<Vector3> CleanPoints(IList<Vector3> input)
    {
        var cleaned = new List<Vector3>(input.Count);
        for (int i = 0; i < input.Count; i++)
        {
            Vector3 point = input[i];
            if (cleaned.Count == 0 || (point - cleaned[cleaned.Count - 1]).sqrMagnitude > EpsilonSqr)
                cleaned.Add(point);
        }
        if (cleaned.Count < 3) return cleaned;

        int write = 1;
        for (int i = 1; i < cleaned.Count - 1; i++)
        {
            Vector3 incoming = cleaned[i] - cleaned[write - 1];
            Vector3 outgoing = cleaned[i + 1] - cleaned[i];
            if (incoming.sqrMagnitude <= EpsilonSqr || outgoing.sqrMagnitude <= EpsilonSqr) continue;
            incoming.Normalize();
            outgoing.Normalize();
            if (Vector3.Cross(incoming, outgoing).magnitude >= .001f || Vector3.Dot(incoming, outgoing) <= 0f)
                cleaned[write++] = cleaned[i];
        }
        cleaned[write++] = cleaned[cleaned.Count - 1];
        cleaned.RemoveRange(write, cleaned.Count - write);
        return cleaned;
    }

    static List<Vector3> BuildRoundedCenterline(IList<Vector3> points, float radius, int segments)
    {
        var result = new List<Vector3>(points.Count * (segments + 1));
        if (points.Count == 0) return result;
        result.Add(points[0]);
        if (points.Count < 3)
        {
            for (int i = 1; i < points.Count; i++) result.Add(points[i]);
            return result;
        }

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 previous = points[i - 1];
            Vector3 corner = points[i];
            Vector3 next = points[i + 1];
            Vector3 firstLeg = corner - previous;
            Vector3 secondLeg = next - corner;
            float firstLength = firstLeg.magnitude;
            float secondLength = secondLeg.magnitude;
            if (firstLength < .0001f || secondLength < .0001f)
            {
                AddDistinct(result, corner);
                continue;
            }

            Vector3 firstDirection = firstLeg / firstLength;
            Vector3 secondDirection = secondLeg / secondLength;
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(-firstDirection, secondDirection), -1f, 1f));
            if (angle < .02f || angle > 3.1215928f)
            {
                AddDistinct(result, corner);
                continue;
            }
            float halfAngle = angle * .5f;
            float tanHalf = Mathf.Tan(halfAngle);
            float sinHalf = Mathf.Sin(halfAngle);
            float tangentDistance = tanHalf > .0001f ? radius / tanHalf : 0f;
            float maxDistance = .5f * Mathf.Min(firstLength, secondLength);
            float distance = Mathf.Min(tangentDistance, maxDistance);
            float centerDistance = distance * tanHalf;
            if (distance < .0001f || centerDistance < .0001f || sinHalf < .0001f)
            {
                AddDistinct(result, corner);
                continue;
            }

            Vector3 entry = corner - firstDirection * distance;
            Vector3 exit = corner + secondDirection * distance;
            Vector3 bisector = -firstDirection + secondDirection;
            if (bisector.sqrMagnitude < EpsilonSqr)
            {
                AddDistinct(result, corner);
                continue;
            }
            bisector.Normalize();
            Vector3 center = corner + bisector * (centerDistance / sinHalf);
            Vector3 fromCenter = entry - center;
            Vector3 toCenter = exit - center;
            for (int step = 0; step <= segments; step++)
                AddDistinct(result, center + Vector3.Slerp(fromCenter, toCenter, (float)step / segments));
        }
        AddDistinct(result, points[points.Count - 1]);
        return result;
    }

    static void AddDistinct(List<Vector3> points, Vector3 point)
    {
        if (points.Count == 0 || (point - points[points.Count - 1]).sqrMagnitude > EpsilonSqr)
            points.Add(point);
    }

    static float PolylineLength(IList<Vector3> points)
    {
        float length = 0f;
        for (int i = 1; i < points.Count; i++) length += Vector3.Distance(points[i - 1], points[i]);
        return length;
    }

    static Vector3 ComputeTangent(IList<Vector3> points, int index)
    {
        Vector3 tangent = index == 0
            ? points[1] - points[0]
            : index == points.Count - 1
                ? points[index] - points[index - 1]
                : points[index + 1] - points[index - 1];
        return tangent.sqrMagnitude > EpsilonSqr ? tangent.normalized : Vector3.right;
    }
}
