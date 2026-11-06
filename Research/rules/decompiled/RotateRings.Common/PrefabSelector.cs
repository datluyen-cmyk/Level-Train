using UnityEngine;

namespace RotateRings.Common;

public class PrefabSelector : MonoBehaviour
{
	public int ActiveIndex { get; private set; }

	public void SetActiveIndex(int index)
	{
		if (((Component)this).transform.childCount == 0)
		{
			ActiveIndex = 0;
			return;
		}
		index = Mathf.Clamp(index, 0, ((Component)this).transform.childCount - 1);
		ActiveIndex = index;
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			GameObject gameObject = ((Component)((Component)this).transform.GetChild(i)).gameObject;
			if ((Object)(object)gameObject != (Object)null)
			{
				gameObject.SetActive(i == index);
			}
		}
	}

	public T GetActiveComponent<T>() where T : Component
	{
		if (((Component)this).transform.childCount == 0)
		{
			return default(T);
		}
		int num = Mathf.Clamp(ActiveIndex, 0, ((Component)this).transform.childCount - 1);
		GameObject gameObject = ((Component)((Component)this).transform.GetChild(num)).gameObject;
		if ((Object)(object)gameObject == (Object)null || !gameObject.activeSelf)
		{
			return default(T);
		}
		return gameObject.GetComponent<T>();
	}
}
