using System;

public interface IInteractableObject
{
	void RegisterOnPointerDown(Action action);

	void RegisterOnPointerUp(Action action);
}
