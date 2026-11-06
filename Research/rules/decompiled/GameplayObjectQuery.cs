using UnityEngine;

public static class GameplayObjectQuery
{
	public static bool IsRingOrStick(IGameplayObject gameplayObject)
	{
		DraggableObjectWithLimitRange limitRangeObject;
		if (!TryGetDragRotateObject(gameplayObject, out var _))
		{
			return TryGetObjectWithLimitRange(gameplayObject, out limitRangeObject);
		}
		return true;
	}

	public static bool TryGetDragRotateObject(IGameplayObject gameplayObject, out DragRotateObject dragRotate)
	{
		dragRotate = null;
		MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
		if (val == null)
		{
			return false;
		}
		dragRotate = val as DragRotateObject;
		return (Object)(object)dragRotate != (Object)null;
	}

	public static bool TryGetObjectWithLimitRange(IGameplayObject gameplayObject, out DraggableObjectWithLimitRange limitRangeObject)
	{
		limitRangeObject = null;
		MonoBehaviour val = (MonoBehaviour)((gameplayObject is MonoBehaviour) ? gameplayObject : null);
		if (val == null)
		{
			return false;
		}
		limitRangeObject = val as DraggableObjectWithLimitRange;
		return (Object)(object)limitRangeObject != (Object)null;
	}
}
