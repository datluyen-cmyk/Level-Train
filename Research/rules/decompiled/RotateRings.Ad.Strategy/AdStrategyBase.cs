using RotateRings.Model;

namespace RotateRings.Ad.Strategy;

public abstract class AdStrategyBase
{
	protected AdStrategyRuntimeProxy Proxy { get; private set; }

	protected PlayerModelProxy PlayerProxy { get; private set; }

	public void SetContext(AdStrategyRuntimeProxy proxy, PlayerModelProxy playerProxy)
	{
		Proxy = proxy;
		PlayerProxy = playerProxy;
	}
}
