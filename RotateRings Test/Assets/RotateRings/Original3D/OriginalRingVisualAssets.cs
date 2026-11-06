using UnityEngine;

/// <summary>
/// Geometry helpers ported from the recovered RingSegmentMeshBuilder and
/// RingSegmentController. Meshes are flat ribbons carrying the source UV/color
/// channels and the source cap quads/attachment meshes.
/// </summary>
public static class OriginalRingVisualAssets
{
    /// <summary>
    /// Builds one source ring piece and every serialized attachment in its original local pose.
    /// Attachments must be passed as a complete array from one source piece; this method does not
    /// infer mounts or dimensions from gameplay links.
    /// </summary>
    public static Mesh CreatePieceMesh(float visualRadius, float visualWidth, float visualGap,
        RotateRingsVisualAttachment[] attachments, Color color, float gapDirectionDeg = 0f,
        float capRatio = .5f, bool extraEnabled = true, float tailLength = -1f,
        float headLength = -1f, float bodyTileLength = 2f, int styleIndex = 0,
        int fullCircleDivisions = 0, float ringZ = 0f, float stemZ = 0f, float crossBarZ = -.2f)
    {
        MeshData data = new MeshData();
        // The native v2 template intentionally stores geometry, while the shared
        // material owns these values. At runtime the source controller overrides
        // tail/head to its build width; _BodyTileLength comes from the source material.
        if (tailLength <= 0f) tailLength = visualWidth;
        if (headLength <= 0f) headLength = visualWidth;
        if (bodyTileLength <= 0f) bodyTileLength = 2f;
        if (fullCircleDivisions <= 0)
        {
            float innerCore = visualRadius * 24f - visualWidth * 12f;
            fullCircleDivisions = Mathf.Clamp(Mathf.RoundToInt(64f * Mathf.Pow(Mathf.Max(1f, innerCore / 36f), .7f)), 64, 384);
        }
        // Native submission order matters at equal-Z pin/ring junctions with
        // the original transparent, depth-writing material: stems, ring, bars.
        if (attachments != null)
        {
            for (int i = 0; i < attachments.Length; i++)
            {
                RotateRingsVisualAttachment a = attachments[i];
                float angle = a.angleDeg * Mathf.Deg2Rad;
                Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                Vector3 extend = a.isInnerMount ? -radial : radial;
                float insert = Mathf.Max(0f, a.stemInsertOffset);
                float totalLength = Mathf.Max(0f, a.stemLength) + insert;
                Vector3 origin = radial * (visualRadius + (a.isInnerMount ? -visualWidth * .5f + insert : visualWidth * .5f - insert));
                bool headed = a.type == 0;
                data.AppendStraightRibbon(origin, extend, a.stemWidth, totalLength,
                    headed ? a.crossWidth : 0f, headed, a.styleIndex, visualWidth, color, stemZ);
            }
        }

        data.AppendSourceArc(visualRadius, visualWidth, visualGap, gapDirectionDeg, capRatio,
            tailLength, headLength, bodyTileLength, styleIndex, fullCircleDivisions, color, ringZ);

        if (attachments != null)
        {
            for (int i = 0; i < attachments.Length; i++)
            {
                RotateRingsVisualAttachment a = attachments[i];
                float angle = a.angleDeg * Mathf.Deg2Rad;
                Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                Vector3 extend = a.isInnerMount ? -radial : radial;
                float insert = Mathf.Max(0f, a.stemInsertOffset);
                float totalLength = Mathf.Max(0f, a.stemLength) + insert;
                Vector3 origin = radial * (visualRadius + (a.isInnerMount ? -visualWidth * .5f + insert : visualWidth * .5f - insert));
                bool headed = a.type == 0;

                if (!headed)
                {
                    Vector3 endpoint = origin + extend * totalLength;
                    Vector3 crossDirection = new Vector3(-radial.y, radial.x, 0f);
                    if (extraEnabled)
                    {
                        float span = Mathf.Max(0f, a.crossLength) * 2f;
                        data.AppendCrossBar(endpoint - crossDirection * (span * .5f), crossDirection,
                            radial, a.crossWidth, span, a.stemWidth, a.styleIndex, a.isInnerMount,
                            visualWidth, true, color, zOffset: crossBarZ);
                    }
                    else
                    {
                        float cap = a.crossWidth * Mathf.Clamp01(a.crossCapLength);
                        float bodyLength = Mathf.Max(0f, a.crossLength);
                        data.AppendCrossBar(endpoint - crossDirection * (bodyLength * .5f), crossDirection,
                            radial, a.crossWidth, bodyLength, a.stemWidth, a.styleIndex,
                            a.isInnerMount, visualWidth, false, color, cap, tailLength, headLength,
                            zOffset: crossBarZ);
                    }
                }
            }
        }
        return data.ToMesh("OriginalRingPiece");
    }

    public static Mesh CreateOriginalRingMesh(RotateRingsOriginalTemplate template, Color color)
    {
        if (template == null) throw new System.ArgumentNullException(nameof(template));
        return CreatePieceMesh(template.visualRadius, template.visualWidth, template.visualGap,
            template.attachments, color, template.gapDirectionDeg, template.capRatio,
            template.extraEnabled, template.tailLength, template.headLength,
            template.bodyTileLength, template.styleIndex, template.arcSubdivisions,
            template.ringZ, template.stemZ, template.crossBarZ);
    }

    public static void ApplyPieceMaterialSettings(Material material, RotateRingsOriginalTemplate template)
    {
        if (material == null || template == null) return;
        if (material.HasProperty("_ExtraEnable")) material.SetFloat("_ExtraEnable", template.extraEnabled ? 1f : 0f);
        if (material.HasProperty("_StyleIndexFromMesh")) material.SetFloat("_StyleIndexFromMesh", 1f);
        if (material.HasProperty("_StyleIndex")) material.SetFloat("_StyleIndex", template.styleIndex);
        if (material.HasProperty("_TailLength")) material.SetFloat("_TailLength", template.tailLength > 0f ? template.tailLength : template.visualWidth);
        if (material.HasProperty("_HeadLength")) material.SetFloat("_HeadLength", template.headLength > 0f ? template.headLength : template.visualWidth);
        if (material.HasProperty("_BodyTileLength")) material.SetFloat("_BodyTileLength", template.bodyTileLength > 0f ? template.bodyTileLength : 2f);
    }

    public static Mesh CreateRingMesh(float radius, float width, float gap, Color color)
    {
        return BuildArcMesh(radius, width, gap, 0f, color, 0, 0, true);
    }

    /// <param name="gapCenterDeg">Gap center angle. Zero points along +X.</param>
    public static Mesh BuildArcMesh(float radius, float width, float gap, float gapCenterDeg,
        Color color, int styleIndex = 0, int fullCircleDivisions = 0, bool useHeadTail = true)
    {
        var mesh = new Mesh { name = "OriginalRingSegment" };
        float gapDeg = Mathf.Clamp(gap, 0f, 360f);
        float visibleDeg = 360f - gapDeg;
        if (visibleDeg < .01f) return mesh;

        // Source controller starts at the clockwise edge of the opening and
        // proceeds positively around the rest of the ring.
        float start = (gapCenterDeg + gapDeg * .5f) * Mathf.Deg2Rad;
        float end = start + visibleDeg * Mathf.Deg2Rad;
        if (fullCircleDivisions <= 0)
        {
            // RingSegmentArcSubdivision.ComputeFullCircleArcSubdivisions and
            // ResolveArcDivisionsForVisibleArc from the recovered controller.
            float innerRadiusCore = radius * 24f - width * 12f;
            float scale = Mathf.Pow(Mathf.Max(1f, innerRadiusCore / 36f), .7f);
            fullCircleDivisions = Mathf.Clamp(Mathf.RoundToInt(64f * scale), 64, 384);
        }
        int divisions = Mathf.Max(1, Mathf.RoundToInt(fullCircleDivisions * visibleDeg / 360f));
        float arcLength = Mathf.PI * 2f * radius * visibleDeg / 360f;
        if (radius > .0001f) divisions = Mathf.Max(divisions, Mathf.CeilToInt(arcLength / .35f));
        divisions = Mathf.Clamp(divisions, 1, 96);
        int across = 2;
        var vertices = new Vector3[(divisions + 1) * across];
        var normals = new Vector3[vertices.Length];
        var tangents = new Vector4[vertices.Length];
        var colors = new Color[vertices.Length];
        var uv0 = new Vector2[vertices.Length];
        var uv1 = new Vector2[vertices.Length];
        var uv2 = new Vector2[vertices.Length];
        var triangles = new int[divisions * 6];
        float half = Mathf.Max(0f, width) * .5f;
        float length = visibleDeg * Mathf.Deg2Rad * radius;
        float headTailStart = useHeadTail ? 0f : 100f;
        float headTailEnd = useHeadTail ? length : 99999f;
        Color32 c = color;

        for (int i = 0; i <= divisions; i++)
        {
            float t = (float)i / divisions;
            float angle = Mathf.Lerp(start, end, t);
            Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 tangent = new Vector3(-radial.y, radial.x, 0f);
            int row = i * 2;
            vertices[row] = radial * (radius + half);
            vertices[row + 1] = radial * (radius - half);
            for (int side = 0; side < 2; side++)
            {
                int v = row + side;
                normals[v] = Vector3.forward;
                tangents[v] = new Vector4(tangent.x, tangent.y, tangent.z, -1f);
                colors[v] = c;
                uv0[v] = new Vector2(t * length + headTailStart, side == 0 ? 1f : 0f);
                uv1[v] = new Vector2(headTailEnd, styleIndex + 1);
                uv2[v] = new Vector2(1f, 0f);
            }
            if (i == divisions) continue;
            int tri = i * 6;
            triangles[tri] = row;
            triangles[tri + 1] = row + 1;
            triangles[tri + 2] = row + 2;
            triangles[tri + 3] = row + 1;
            triangles[tri + 4] = row + 3;
            triangles[tri + 5] = row + 2;
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.colors = colors;
        mesh.uv = uv0;
        mesh.uv2 = uv1;
        mesh.SetUVs(2, new System.Collections.Generic.List<Vector2>(uv2));
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh CreateSliderMesh(float length, float width, Color color)
    {
        var mesh = new Mesh { name = "OriginalSliderRibbon" };
        float half = Mathf.Max(0f, width) * .5f;
        var vertices = new[] { new Vector3(0, half), new Vector3(0, -half), new Vector3(length, half), new Vector3(length, -half) };
        mesh.vertices = vertices;
        mesh.normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
        mesh.tangents = new[] { new Vector4(1, 0, 0, -1), new Vector4(1, 0, 0, -1), new Vector4(1, 0, 0, -1), new Vector4(1, 0, 0, -1) };
        mesh.colors = new[] { color, color, color, color };
        mesh.uv = new[] { new Vector2(0, 1), new Vector2(0, 0), new Vector2(length, 1), new Vector2(length, 0) };
        mesh.uv2 = new[] { new Vector2(length, 1), new Vector2(length, 1), new Vector2(length, 1), new Vector2(length, 1) };
        mesh.SetUVs(2, new System.Collections.Generic.List<Vector2> { Vector2.right, Vector2.right, Vector2.right, Vector2.right });
        mesh.triangles = new[] { 0, 1, 2, 1, 3, 2 };
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Mesh CreatePinMesh(float length, float width, Color color)
    {
        // The common authored attachment is a TBar. Serialized examples use
        // crossLength == stemWidth; _extraEnabled doubles its visible span, and
        // crossWidth is about 1.6x stemWidth.
        return CreateTBarPinMesh(length, width, width * 2f, width * 1.6f, color);
    }

    public static Mesh CreateTBarPinMesh(float length, float stemWidth, float crossLength, float crossWidth, Color color)
    {
        length = Mathf.Max(0f, length);
        stemWidth = Mathf.Max(0f, stemWidth);
        crossLength = Mathf.Max(0f, crossLength);
        crossWidth = Mathf.Max(0f, crossWidth);
        Mesh mesh = CreatePinMesh(length, stemWidth, 0f, color);
        var vertices = new System.Collections.Generic.List<Vector3>(mesh.vertices);
        var normals = new System.Collections.Generic.List<Vector3>(mesh.normals);
        var tangents = new System.Collections.Generic.List<Vector4>(mesh.tangents);
        var colors = new System.Collections.Generic.List<Color>(mesh.colors);
        var uv0 = new System.Collections.Generic.List<Vector2>(mesh.uv);
        var uv1 = new System.Collections.Generic.List<Vector2>(mesh.uv2);
        var uv2 = new System.Collections.Generic.List<Vector2>();
        mesh.GetUVs(2, uv2);
        int first = vertices.Count;
        float halfLength = crossLength * .5f;
        float halfWidth = crossWidth * .5f;
        // Original crossbar is perpendicular to the stem and centered over its tip.
        vertices.Add(new Vector3(length-halfWidth, -halfLength, -.002f));
        vertices.Add(new Vector3(length+halfWidth, -halfLength, -.002f));
        vertices.Add(new Vector3(length-halfWidth, halfLength, -.002f));
        vertices.Add(new Vector3(length+halfWidth, halfLength, -.002f));
        for (int i = 0; i < 4; i++)
        {
            normals.Add(Vector3.forward);
            tangents.Add(new Vector4(0f, 1f, 0f, -1f));
            colors.Add(color);
            float along = i < 2 ? 0f : crossLength;
            float across = (i == 0 || i == 2) ? 1f : 0f;
            // Negative style marks the EXTRA atlas path; uv2.y=0 selects Extra A.
            uv0.Add(new Vector2(along, across));
            uv1.Add(new Vector2(crossLength, -1f));
            uv2.Add(new Vector2(1f, 0f));
        }
        var triangles = new System.Collections.Generic.List<int>(mesh.triangles);
        triangles.Add(first); triangles.Add(first+1); triangles.Add(first+2);
        triangles.Add(first+1); triangles.Add(first+3); triangles.Add(first+2);
        mesh.Clear();
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

    /// <summary>Ribbon pin with the source UV head/tail window; headLength is in path units.</summary>
    public static Mesh CreatePinMesh(float length, float width, float headLength, Color color)
    {
        var mesh = new Mesh { name = "OriginalPinRibbon" };
        length = Mathf.Max(0f, length);
        width = Mathf.Max(0f, width);
        headLength = Mathf.Clamp(headLength, 0f, length);
        Vector3 direction = Vector3.right;
        Vector3 side = Vector3.up * (width * .5f);
        Color32 c = color;
        // This matches AppendStraightRibbon: a three-row seam where the shader
        // switches from stem atlas body to pin head near the free tip.
        Vector3[] vertices = headLength > 0f && headLength < length
            ? new[] { side, -side, new Vector3(length - headLength, 0f) + side, new Vector3(length - headLength, 0f) - side, new Vector3(length, 0f) + side, new Vector3(length, 0f) - side }
            : new[] { side, -side, new Vector3(length, 0f) + side, new Vector3(length, 0f) - side };
        int rows = vertices.Length / 2;
        var normals = new Vector3[vertices.Length];
        var tangents = new Vector4[vertices.Length];
        var colors = new Color[vertices.Length];
        var uv0 = new Vector2[vertices.Length];
        var uv1 = new Vector2[vertices.Length];
        var uv2 = new Vector2[vertices.Length];
        float tiledLength = length;
        for (int row = 0; row < rows; row++)
        {
            float path = row == 0 ? 0f : row == rows - 1 ? tiledLength : Mathf.Max(0f, tiledLength - headLength);
            for (int sideIndex = 0; sideIndex < 2; sideIndex++)
            {
                int i = row * 2 + sideIndex;
                normals[i] = Vector3.forward;
                tangents[i] = new Vector4(direction.x, direction.y, direction.z, -1f);
                colors[i] = c;
                uv0[i] = new Vector2(100f + path, sideIndex == 0 ? 1f : 0f);
                uv1[i] = new Vector2(100f + length, 1f);
                uv2[i] = Vector2.right;
            }
        }
        var triangles = new int[(rows - 1) * 6];
        for (int row = 0; row < rows - 1; row++)
        {
            int v = row * 2, t = row * 6;
            triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
            triangles[t + 3] = v + 1; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.tangents = tangents;
        mesh.colors = colors;
        mesh.uv = uv0;
        mesh.uv2 = uv1;
        mesh.SetUVs(2, new System.Collections.Generic.List<Vector2>(uv2));
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    sealed class MeshData
    {
        readonly System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();
        readonly System.Collections.Generic.List<Vector3> normals = new System.Collections.Generic.List<Vector3>();
        readonly System.Collections.Generic.List<Vector4> tangents = new System.Collections.Generic.List<Vector4>();
        readonly System.Collections.Generic.List<Color> colors = new System.Collections.Generic.List<Color>();
        readonly System.Collections.Generic.List<Vector2> uv0 = new System.Collections.Generic.List<Vector2>();
        readonly System.Collections.Generic.List<Vector2> uv1 = new System.Collections.Generic.List<Vector2>();
        readonly System.Collections.Generic.List<Vector2> uv2 = new System.Collections.Generic.List<Vector2>();
        readonly System.Collections.Generic.List<int> triangles = new System.Collections.Generic.List<int>();

        public void Append(Mesh mesh)
        {
            if (mesh == null) return;
            int first = vertices.Count;
            vertices.AddRange(mesh.vertices);
            normals.AddRange(mesh.normals);
            tangents.AddRange(mesh.tangents);
            colors.AddRange(mesh.colors);
            uv0.AddRange(mesh.uv);
            uv1.AddRange(mesh.uv2);
            var channel2 = new System.Collections.Generic.List<Vector2>();
            mesh.GetUVs(2, channel2);
            uv2.AddRange(channel2);
            var tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i++) triangles.Add(first + tris[i]);
        }

        public void AppendSourceArc(float radius, float width, float gapDeg, float gapDirectionDeg,
            float capRatio, float tailLength, float headLength, float bodyTileLength,
            int styleIndex, int fullCircleDivisions, Color color, float zOffset)
        {
            float gap = Mathf.Clamp(gapDeg, 0f, 360f);
            float visibleDeg = 360f - gap;
            if (visibleDeg < .01f) return;
            bool hasGap = gap > .01f;
            bool closedLoop = !hasGap;
            float arcLength = visibleDeg * Mathf.Deg2Rad * radius;
            float totalUvLength = arcLength;
            if (hasGap)
            {
                float bodyUv = arcLength - tailLength - headLength;
                if (bodyUv > .001f && bodyTileLength > .001f)
                    totalUvLength = tailLength + Mathf.Max(1, Mathf.RoundToInt(bodyUv / bodyTileLength)) * bodyTileLength + headLength;
            }
            else if (arcLength > .001f && bodyTileLength > .001f)
                totalUvLength = Mathf.Max(1, Mathf.RoundToInt(arcLength / bodyTileLength)) * bodyTileLength;

            float start = (gapDirectionDeg + gap * .5f) * Mathf.Deg2Rad;
            float end = start + visibleDeg * Mathf.Deg2Rad;
            float tailAngle = hasGap && radius > .001f ? tailLength / radius * capRatio : 0f;
            float headAngle = hasGap && radius > .001f ? headLength / radius * capRatio : 0f;
            float arcStart = start + tailAngle;
            float arcEnd = end - headAngle;
            float visibleUvStart = hasGap ? tailLength * capRatio : 0f;
            float visibleUvEnd = totalUvLength - (hasGap ? headLength * capRatio : 0f);
            float widthHalf = width * .5f;
            if (arcEnd > arcStart + .0001f)
            {
                float sweep = Mathf.Abs(arcEnd - arcStart) * Mathf.Rad2Deg;
                float circumference = Mathf.PI * 2f * radius * sweep / 360f;
                int divisions = Mathf.Max(1, Mathf.RoundToInt(fullCircleDivisions * sweep / 360f));
                if (radius > .0001f) divisions = Mathf.Max(divisions, Mathf.CeilToInt(circumference / .35f));
                divisions = closedLoop ? Mathf.Clamp(divisions, 64, 384) : Mathf.Clamp(divisions, 1, 96);
                int first = vertices.Count;
                float ringDataLength = closedLoop ? 99999f : totalUvLength;
                for (int i = 0; i <= divisions; i++)
                {
                    float t = i / (float)divisions;
                    float angle = Mathf.Lerp(arcStart, arcEnd, t);
                    Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                    Vector3 tangent = new Vector3(-radial.y, radial.x, 0f);
                    float u = closedLoop ? 100f + t * totalUvLength : Mathf.Lerp(visibleUvStart, visibleUvEnd, t);
                    Vector3 outer = radial * (radius + widthHalf); outer.z += zOffset;
                    Vector3 inner = radial * (radius - widthHalf); inner.z += zOffset;
                    if (closedLoop && i == divisions)
                    {
                        outer = vertices[first];
                        inner = vertices[first + 1];
                    }
                    AddVertex(outer, tangent, color, new Vector2(u, 1f), new Vector2(ringDataLength, styleIndex + 1), 1f);
                    AddVertex(inner, tangent, color, new Vector2(u, 0f), new Vector2(ringDataLength, styleIndex + 1), 1f);
                }
                for (int i = 0; i < divisions; i++)
                {
                    int v = first + i * 2;
                    AddQuad(v, v + 1, v + 2, v + 3);
                }
            }
            if (hasGap && capRatio > .001f && tailLength > .001f)
            {
                float capAngle = arcStart;
                Vector3 radial = new Vector3(Mathf.Cos(capAngle), Mathf.Sin(capAngle), 0f);
                Vector3 anchor = radial * radius;
                Vector3 outward = new Vector3(Mathf.Sin(capAngle), -Mathf.Cos(capAngle), 0f);
                AppendCapQuad(anchor, outward, radial, width, tailLength * capRatio,
                    tailLength * capRatio, 0f, totalUvLength, styleIndex, color, zOffset);
            }
            if (hasGap && capRatio > .001f && headLength > .001f)
            {
                float capAngle = arcEnd;
                Vector3 radial = new Vector3(Mathf.Cos(capAngle), Mathf.Sin(capAngle), 0f);
                Vector3 anchor = radial * radius;
                Vector3 outward = new Vector3(-Mathf.Sin(capAngle), Mathf.Cos(capAngle), 0f);
                AppendCapQuad(anchor, outward, radial, width, headLength * capRatio,
                    totalUvLength - headLength * capRatio, totalUvLength, totalUvLength, styleIndex, color, zOffset);
            }
        }

        void AppendCapQuad(Vector3 anchor, Vector3 outward, Vector3 radial, float width, float length,
            float uvAnchor, float uvTip, float totalUv, int styleIndex, Color color, float zOffset)
        {
            Vector3 tangent = uvTip >= uvAnchor ? outward : -outward;
            Vector3 tip = anchor + outward * length;
            anchor.z += zOffset;
            tip.z += zOffset;
            float half = width * .5f;
            int first = vertices.Count;
            AddVertex(anchor + radial * half, tangent, color, new Vector2(uvAnchor, 1f), new Vector2(totalUv, styleIndex + 1), 1f);
            AddVertex(anchor - radial * half, tangent, color, new Vector2(uvAnchor, 0f), new Vector2(totalUv, styleIndex + 1), 1f);
            AddVertex(tip + radial * half, tangent, color, new Vector2(uvTip, 1f), new Vector2(totalUv, styleIndex + 1), 1f);
            AddVertex(tip - radial * half, tangent, color, new Vector2(uvTip, 0f), new Vector2(totalUv, styleIndex + 1), 1f);
            if (outward.x * radial.y - outward.y * radial.x < 0f)
            {
                triangles.Add(first); triangles.Add(first + 1); triangles.Add(first + 2);
                triangles.Add(first + 1); triangles.Add(first + 3); triangles.Add(first + 2);
            }
            else
            {
                triangles.Add(first); triangles.Add(first + 2); triangles.Add(first + 1);
                triangles.Add(first + 1); triangles.Add(first + 2); triangles.Add(first + 3);
            }
        }

        public void AppendStraightRibbon(Vector3 origin, Vector3 direction, float width, float length,
            float headLength, bool hasHead, int styleIndex, float referenceWidth, Color color, float zOffset)
        {
            float half = width * .5f;
            Vector3 up = -Vector3.forward;
            Vector3 side = Vector3.Cross(up, direction).normalized;
            float uvScale = referenceWidth > 0f && width > 0f ? referenceWidth / width : 1f;
            float[] positions;
            if (hasHead && headLength > 0f && headLength < length)
                positions = new[] { 0f, length - headLength, length };
            else
                positions = new[] { 0f, length };
            int first = vertices.Count;
            float totalUv = hasHead ? 100f + length * uvScale : 99999f;
            for (int row = 0; row < positions.Length; row++)
            {
                float path = positions[row];
                Vector3 point = origin + direction * path;
                point.z += zOffset;
                float tiledPath = path * uvScale;
                AddVertex(point + side * half, direction, color, new Vector2(100f + tiledPath, 1f),
                    new Vector2(totalUv, styleIndex + 1), 1f);
                AddVertex(point - side * half, direction, color, new Vector2(100f + tiledPath, 0f),
                    new Vector2(totalUv, styleIndex + 1), 1f);
            }
            for (int row = 0; row < positions.Length - 1; row++)
            {
                int v = first + row * 2;
                AddQuad(v, v + 1, v + 2, v + 3);
            }
        }

        public void AppendCrossBar(Vector3 bodyOrigin, Vector3 direction, Vector3 side, float width,
            float bodyLength, float stemWidth, int styleIndex, bool innerMount, float referenceWidth,
            bool isExtra, Color color, float capLength = 0f, float tailAtlasLength = 0f,
            float headAtlasLength = 0f, float zOffset = 0f)
        {
            if (width <= 0f || bodyLength <= 0f) return;
            float half = width * .5f;
            float halfStem = stemWidth * .25f;
            bool hasCaps = capLength > .0001f;
            float uvScale = referenceWidth > 0f && width > 0f ? referenceWidth / width : 1f;
            float uvTotal;
            float uvStart;
            float uvTail;
            float uvHead;
            float[] path;
            if (hasCaps)
            {
                float seamA = Mathf.Clamp(capLength + bodyLength * .5f - halfStem, capLength, capLength + bodyLength);
                float seamB = Mathf.Clamp(capLength + bodyLength * .5f + halfStem, capLength, capLength + bodyLength);
                path = new[] { 0f, capLength, seamA, seamB, capLength + bodyLength, bodyLength + capLength * 2f };
                uvTail = tailAtlasLength;
                uvHead = headAtlasLength;
                uvTotal = uvTail + bodyLength * uvScale + uvHead;
                uvStart = 0f;
            }
            else
            {
                float seamA = Mathf.Clamp(bodyLength * .5f - halfStem, 0f, bodyLength);
                float seamB = Mathf.Clamp(bodyLength * .5f + halfStem, 0f, bodyLength);
                path = new[] { 0f, seamA, seamB, bodyLength };
                uvTail = isExtra ? 0f : 100f;
                uvHead = 0f;
                uvTotal = isExtra ? bodyLength * uvScale : 99999f;
                uvStart = isExtra ? 0f : 100f;
            }
            Vector3 start = hasCaps ? bodyOrigin - direction * capLength : bodyOrigin;
            float[] acrossUv = { 1f, .5f, 0f };
            float[] acrossPos = { half, 0f, -half };
            int first = vertices.Count;
            int rowCount = path.Length;
            for (int i = 0; i < rowCount; i++)
            {
                float pathDistance = path[i];
                Vector3 center = start + direction * pathDistance;
                center.z += zOffset;
                float u;
                if (hasCaps)
                {
                    float bodyPath = Mathf.Clamp(pathDistance - capLength, 0f, bodyLength);
                    u = (pathDistance <= capLength) ? Mathf.Lerp(0f, uvTail, capLength <= 0f ? 0f : pathDistance / capLength)
                        : (uvTail + bodyPath * uvScale);
                    if (pathDistance > capLength + bodyLength) u = uvTail + bodyLength * uvScale + headAtlasLength * ((pathDistance - capLength - bodyLength) / Mathf.Max(capLength, .0001f));
                }
                else u = uvStart + pathDistance * uvScale;
                for (int across = 0; across < 3; across++)
                {
                    bool outlineMask = !hasCaps && i == 1 && across == (innerMount ? 0 : 1);
                    AddVertex(center + side * acrossPos[across], direction, color,
                        new Vector2(u, acrossUv[across]),
                        new Vector2(uvTotal, isExtra ? -(styleIndex + 1) : styleIndex + 1),
                        outlineMask ? 0f : 1f);
                }
            }
            for (int i = 0; i < rowCount - 1; i++)
            for (int across = 0; across < 2; across++)
            {
                int v = first + i * 3 + across;
                AddQuad(v, v + 1, v + 3, v + 4);
            }
        }

        void AddVertex(Vector3 position, Vector3 tangent, Color color, Vector2 uv, Vector2 ringData, float outline)
        {
            vertices.Add(position);
            normals.Add(Vector3.forward);
            tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, -1f));
            colors.Add(color);
            uv0.Add(uv);
            uv1.Add(ringData);
            uv2.Add(new Vector2(outline, 0f));
        }

        void AddQuad(int a, int b, int c, int d)
        {
            triangles.Add(a); triangles.Add(b); triangles.Add(c);
            triangles.Add(b); triangles.Add(d); triangles.Add(c);
        }

        public Mesh ToMesh(string name)
        {
            var mesh = new Mesh { name = name };
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
    }

    public static Material CreateMaterial(Color color, bool catchMaterial = false)
    {
        Shader shader = catchMaterial ? Shader.Find("Sprites/BrowCatchSprite") : Shader.Find("HsApp/Effect/XYStyleLine");
        if (shader == null)
            throw new System.InvalidOperationException(catchMaterial
                ? "The recovered BrowCatchSprite shader is not available in this project."
                : "The recovered XYStyleLine shader port was not imported.");
        var material = new Material(shader) { name = catchMaterial ? "OriginalCatchSpriteMaterial" : "OriginalXYStyleLineMaterial" };
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (catchMaterial)
        {
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", Resources.Load<Texture2D>("Original/Textures/catch"));
        }
        else
        {
            material.SetFloat("_UseVertexColor", 1f);
            material.SetFloat("_StyleRowCount", 1f);
            material.SetFloat("_AlphaClipEnable", 1f);
            material.SetFloat("_AlphaClip", .25f);
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_DstBlend", 10f);
            material.SetFloat("_Cull", 2f);
            material.SetFloat("_ZWrite", 1f);
            material.SetFloat("_ZTest", 4f);
            material.SetFloat("_BodyTileLength", 2f);
            material.SetFloat("_TailLength", .3f);
            material.SetFloat("_HeadLength", .3f);
            material.SetFloat("_ColorIntensity", 1f);
            material.SetFloat("_Blend", .5f);
            material.SetFloat("_Contrast", 1f);
            material.SetFloat("_Saturation", 1f);
            material.SetFloat("_NormalMapEnable", 1f);
            material.SetFloat("_NormalScale", 1.5f);
            material.SetFloat("_SimpleLightEnable", 1f);
            material.SetFloat("_LightIntensity", 1.4f);
            material.SetFloat("_LightStrength", 1f);
            material.SetFloat("_LightWrap", 1f);
            material.SetVector("_TailRect", new Vector4(0f, 0f, .25f, 1f));
            material.SetVector("_BodyRect", new Vector4(.25f, 0f, .5f, 1f));
            material.SetVector("_HeadRect", new Vector4(.75f, 0f, .25f, 1f));
            material.SetColor("_LightColor", new Color(.95f, .95f, .95f, 1f));
            material.SetColor("_ShadowColor", Color.black);
            material.SetTexture("_MainTex", Resources.Load<Texture2D>("Original/Textures/ringextra"));
            material.SetTexture("_NormalMap", Resources.Load<Texture2D>("Original/Textures/ringextra_n"));
            material.SetTexture("_SDFTex", Resources.Load<Texture2D>("Original/Textures/ringextra_SDF"));
        }
        return material;
    }

    /// <summary>Applies the recovered active-ring outline settings to a ring material instance.</summary>
    public static void ApplySelectionOutline(Material material, bool selected)
    {
        if (material == null) return;
        if (selected)
        {
            material.EnableKeyword("_OUTLINE_ON");
            if (material.HasProperty("_OutlineEnable")) material.SetFloat("_OutlineEnable", 1f);
            if (material.HasProperty("_OutlineColor")) material.SetColor("_OutlineColor", new Color(.5215686f, 1f, .9411765f, 1f));
            if (material.HasProperty("_OutlineWidth")) material.SetFloat("_OutlineWidth", .1f);
            if (material.HasProperty("_OutlineSoftness")) material.SetFloat("_OutlineSoftness", .01f);
            if (material.HasProperty("_OutlineMode")) material.SetFloat("_OutlineMode", 2f);
            if (material.HasProperty("_OutlineUseVertColor")) material.SetFloat("_OutlineUseVertColor", 0f);
            if (material.HasProperty("_ShadowTintColor")) material.SetColor("_ShadowTintColor", new Color(.3396226f, .1390865f, .00801f, 0f));
        }
        else
        {
            material.DisableKeyword("_OUTLINE_ON");
            if (material.HasProperty("_OutlineEnable")) material.SetFloat("_OutlineEnable", 0f);
            if (material.HasProperty("_ShadowTintColor")) material.SetColor("_ShadowTintColor", new Color(.3396226f, .1390865f, .00801f, .2745098f));
        }
    }
}
