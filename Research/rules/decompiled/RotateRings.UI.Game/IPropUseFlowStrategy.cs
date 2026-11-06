using RotateRings.Prop;

namespace RotateRings.UI.Game;

public interface IPropUseFlowStrategy
{
	EPropType PropType { get; }

	bool HasPendingFastFire { get; }

	bool TryHandlePropUse(PropUseEvent eventData);

	void RefreshUIState();

	void OnLevelChanged();

	void OnPropChanged(PropChangeEvent eventData);

	void OnRingSolved();

	void OnSessionEnd();

	void FireFromPanel(EPropType propType);

	void PrepareForSettlement();
}
