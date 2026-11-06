using System.Collections.Generic;

namespace XY.Common.ParticleImage;

public class XYParticlePool
{
	private readonly List<XYParticle> _particles;

	private readonly XYParticleImage _source;

	private int _totalCreated;

	public XYParticlePool(int capacity, XYParticleImage source)
	{
		_particles = new List<XYParticle>(capacity);
		_source = source;
		EnsureCapacity(capacity);
	}

	public void EnsureCapacity(int minAvailable)
	{
		if (minAvailable > 0)
		{
			while (_particles.Count < minAvailable)
			{
				_particles.Add(new XYParticle(_source));
				_totalCreated++;
			}
		}
	}

	public XYParticle Get()
	{
		if (_particles.Count > 0)
		{
			XYParticle result = _particles[_particles.Count - 1];
			_particles.RemoveAt(_particles.Count - 1);
			return result;
		}
		_totalCreated++;
		return new XYParticle(_source);
	}

	public void Release(XYParticle particle)
	{
		_particles.Add(particle);
	}
}
