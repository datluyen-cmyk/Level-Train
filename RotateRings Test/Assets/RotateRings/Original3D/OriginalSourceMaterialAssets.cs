using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
sealed class OriginalSourceMaterialCatalogData
{
    public int schemaVersion;
    public OriginalSourceMaterialRecord[] materials;
}

[Serializable]
sealed class OriginalSourceMaterialRecord
{
    public string name;
    public long pathId;
    public int[] sourceFileIds;
    public string sourceBundle;
    public string shaderName;
    public long shaderPathId;
    public OriginalSourceFloatProperty[] floats;
    public OriginalSourceColorProperty[] colors;
    public OriginalSourceIntProperty[] ints;
    public OriginalSourceTextureProperty[] textures;
}

[Serializable] sealed class OriginalSourceFloatProperty { public string name; public float value; }
[Serializable] sealed class OriginalSourceIntProperty { public string name; public int value; }
[Serializable] sealed class OriginalSourceColorProperty { public string name; public Color value; }
[Serializable] sealed class OriginalSourceTextureProperty
{
    public string name;
    public long texturePathId;
    public string resourcePath;
    public Vector2 scale;
    public Vector2 offset;
}

/// <summary>
/// Resolves the exact source material referenced by a serialized original component.
/// Material path IDs identify assets; Unity file IDs are external-reference table
/// indices local to each source scene, so they are deliberately not used as keys.
/// </summary>
public static class OriginalSourceMaterialAssets
{
    const string CatalogResourcePath = "Original/material_catalog";
    static OriginalSourceMaterialCatalogData catalog;

    public static Material Resolve(RotateRingsOriginalMaterialRef reference)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (reference.pathId == 0) throw new ArgumentException("Source material reference has no path ID.", nameof(reference));

        OriginalSourceMaterialRecord record = Find(reference.pathId);
        Shader shader = Shader.Find(record.shaderName);
        if (shader == null)
            throw new InvalidOperationException("Required source shader is unavailable: " + record.shaderName +
                " (material " + record.name + ", pathID " + record.pathId + ").");

        var material = new Material(shader) { name = record.name + " (Source " + record.pathId + ")" };
        try
        {
            var unsupported = new List<string>();
            ApplyFloats(material, record.floats, unsupported);
            ApplyColors(material, record.colors, unsupported);
            ApplyInts(material, record.ints, unsupported);
            ApplyTextures(material, record.textures, unsupported);
            if (unsupported.Count > 0)
                throw new InvalidOperationException("Source material " + record.name + " uses properties missing from " +
                    record.shaderName + ": " + string.Join(", ", unsupported.ToArray()));
            return material;
        }
        catch
        {
            UnityEngine.Object.Destroy(material);
            throw;
        }
    }

    public static string[] SourcePropertiesMissingFromShader(RotateRingsOriginalMaterialRef reference)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (reference.pathId == 0) throw new ArgumentException("Source material reference has no path ID.", nameof(reference));
        OriginalSourceMaterialRecord record = Find(reference.pathId);
        Shader shader = Shader.Find(record.shaderName);
        if (shader == null) return new[] { "shader:" + record.shaderName };
        var missing = new List<string>();
        if (record.floats != null) foreach (var p in record.floats) if (shader.FindPropertyIndex(p.name) < 0) missing.Add(p.name);
        if (record.colors != null) foreach (var p in record.colors) if (shader.FindPropertyIndex(p.name) < 0) missing.Add(p.name);
        if (record.ints != null) foreach (var p in record.ints) if (shader.FindPropertyIndex(p.name) < 0) missing.Add(p.name);
        if (record.textures != null) foreach (var p in record.textures) if (shader.FindPropertyIndex(p.name) < 0) missing.Add(p.name);
        missing.Sort(StringComparer.Ordinal);
        return missing.ToArray();
    }

    static OriginalSourceMaterialRecord Find(long pathId)
    {
        if (catalog == null)
        {
            TextAsset asset = Resources.Load<TextAsset>(CatalogResourcePath);
            if (asset == null) throw new InvalidOperationException("Original source material catalog is missing from Resources: " + CatalogResourcePath);
            catalog = JsonUtility.FromJson<OriginalSourceMaterialCatalogData>(asset.text);
            if (catalog == null || catalog.schemaVersion != 1 || catalog.materials == null)
                throw new InvalidOperationException("Original source material catalog is invalid.");
        }
        OriginalSourceMaterialRecord found = null;
        foreach (var record in catalog.materials)
        {
            if (record == null || record.pathId != pathId) continue;
            if (found != null) throw new InvalidOperationException("Duplicate source material path ID in catalog: " + pathId);
            found = record;
        }
        if (found == null) throw new KeyNotFoundException("Unknown original source material path ID: " + pathId);
        return found;
    }

    static void ApplyFloats(Material material, OriginalSourceFloatProperty[] properties, List<string> unsupported)
    {
        if (properties == null) return;
        foreach (var property in properties)
        {
            if (material.HasProperty(property.name)) material.SetFloat(property.name, property.value);
            else if (property.name != "_RenderQueue") unsupported.Add(property.name);
        }
    }

    static void ApplyColors(Material material, OriginalSourceColorProperty[] properties, List<string> unsupported)
    {
        if (properties == null) return;
        foreach (var property in properties)
        {
            if (material.HasProperty(property.name)) material.SetColor(property.name, property.value);
            else unsupported.Add(property.name);
        }
    }

    static void ApplyInts(Material material, OriginalSourceIntProperty[] properties, List<string> unsupported)
    {
        if (properties == null) return;
        foreach (var property in properties)
        {
            if (material.HasProperty(property.name)) material.SetInt(property.name, property.value);
            else unsupported.Add(property.name);
        }
    }

    static void ApplyTextures(Material material, OriginalSourceTextureProperty[] properties, List<string> unsupported)
    {
        if (properties == null) return;
        foreach (var property in properties)
        {
            if (!material.HasProperty(property.name))
            {
                unsupported.Add(property.name);
                continue;
            }
            Texture texture = null;
            if (property.texturePathId != 0)
            {
                if (string.IsNullOrEmpty(property.resourcePath))
                    throw new InvalidOperationException("Source texture path is missing for material property " + property.name + ".");
                texture = Resources.Load<Texture2D>(property.resourcePath);
                if (texture == null)
                    throw new InvalidOperationException("Source texture failed to load: " + property.resourcePath +
                        " (pathID " + property.texturePathId + ").");
            }
            material.SetTexture(property.name, texture);
            material.SetTextureScale(property.name, property.scale);
            material.SetTextureOffset(property.name, property.offset);
        }
    }
}
