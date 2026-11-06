using UnlockRing.Core.Domain;
using UnlockRing.Core.Geometry;

public sealed class CachedRenderFeature
{
	public FeatureDefinition Feature;

	public IGeometryShape Shape;

	public CachedRenderFeature(FeatureDefinition f, IGeometryShape s)
	{
		Feature = f;
		Shape = s;
	}
}
