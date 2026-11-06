using RotateRings.Prop;

namespace RotateRings.GamePlay.Services.Prop;

public interface IRocketPropUseCallback : IRotateRingPropUseCallback
{
	void OnRocketShotHit(TrackPropUseReason reason, bool success);

	void OnFastFireSessionComplete();
}
