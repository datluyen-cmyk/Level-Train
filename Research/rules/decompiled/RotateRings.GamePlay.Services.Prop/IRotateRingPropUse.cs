using RotateRings.Prop;

namespace RotateRings.GamePlay.Services.Prop;

public interface IRotateRingPropUse
{
	EPropType PropType { get; }

	bool CanStart(IRotateRingPropHost host);

	void Begin(IRotateRingPropHost host, IRotateRingPropUseCallback callback);

	void OnPropGuideOpened();

	void OnUseButtonClicked();

	void OnGuideReady();

	void OnGuideDismissed(bool cancelled);

	void End();
}
