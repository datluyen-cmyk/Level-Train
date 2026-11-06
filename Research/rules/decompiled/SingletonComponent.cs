using UnityEngine;

public abstract class SingletonComponent<T> : MonoBehaviour where T : Component
{
	private const string TAG = "[RotateRing][SingletonComponent] ";

	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		if ((Object)(object)Instance != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Instance = ((Component)this).GetComponent<T>();
		}
	}
}
