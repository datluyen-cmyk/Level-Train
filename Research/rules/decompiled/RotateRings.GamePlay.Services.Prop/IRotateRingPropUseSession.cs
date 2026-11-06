namespace RotateRings.GamePlay.Services.Prop;

public interface IRotateRingPropUseSession : IPropUseFlow
{
	bool IsOnSession { get; set; }

	void CompleteStep();
}
