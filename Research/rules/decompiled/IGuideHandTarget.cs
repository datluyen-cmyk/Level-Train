using DG.Tweening;

public interface IGuideHandTarget
{
	void SetupRotation();

	void SetupInstance();

	Tween CreateGestureTween();
}
