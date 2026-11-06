using System;
using UnityEngine;

namespace Basis.Common.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public abstract class UIResourceLoaderBase : MonoBehaviour
{
	[Header("资源名/地址")]
	[SerializeField]
	[Tooltip("资源短名或地址，由子类的 ResolvePath 解析为完整资源路径")]
	protected string resourceName;

	[Header("是否对象池复用")]
	[SerializeField]
	[Tooltip("勾选：走对象池 Spawn/Recycle；不勾选：一次性 Instantiate/Destroy 即时释放。")]
	protected bool useObjectPool = true;

	[Header("是否自动隐藏")]
	[SerializeField]
	[Tooltip("勾选后，播放指定秒数自动隐藏并回收实例。")]
	protected bool autoHide;

	[Header("自动隐藏延迟(秒)")]
	[SerializeField]
	[Tooltip("autoHide 勾选时生效，延迟该秒数后隐藏（运行时下限 0.01 秒）。")]
	protected float hideDelay = 1f;

	[Header("加载时机")]
	[SerializeField]
	[Tooltip("OnEnable：启用时加载；Play：由代码调用 Play 时再加载。")]
	protected UIResourceLoadTiming playTiming;

	protected GameObject instance;

	private GameObject cachedInstance;

	private bool isLoading;

	private int loadGeneration;

	private GameObject deferredRecycleTarget;

	private bool pendingPlayRequest;

	private GameObject lastSpawnedInstance;

	private const string PoolRootName = "[Pool]";

	protected abstract string LogTag { get; }

	protected virtual bool LoadOnEnable => playTiming == UIResourceLoadTiming.OnEnable;

	protected bool ShouldPlayOnReady
	{
		get
		{
			if (playTiming != UIResourceLoadTiming.OnEnable)
			{
				return pendingPlayRequest;
			}
			return true;
		}
	}

	protected virtual bool HasDeferredLoad()
	{
		return pendingPlayRequest;
	}

	protected void SetPendingPlayRequest(bool value)
	{
		pendingPlayRequest = value;
	}

	public virtual void Play()
	{
		SetPendingPlayRequest(value: true);
		if ((Object)(object)instance != (Object)null)
		{
			if (autoHide)
			{
				GameApp.Timer.KillByOwner((Object)(object)((Component)this).transform);
				GameApp.Timer.Delay((Object)(object)((Component)this).transform, Mathf.Max(0.01f, hideDelay), (Action)OnAutoHide);
			}
			OnPlayWithInstance();
			SetPendingPlayRequest(value: false);
		}
		else if (((Behaviour)this).isActiveAndEnabled)
		{
			Load();
		}
	}

	protected virtual void OnPlayWithInstance()
	{
	}

	private void OnEnable()
	{
		if (!TryReuseDeferredInstance())
		{
			if ((Object)(object)instance != (Object)null && autoHide)
			{
				GameApp.Timer.KillByOwner((Object)(object)((Component)this).transform);
				GameApp.Timer.Delay((Object)(object)((Component)this).transform, Mathf.Max(0.01f, hideDelay), (Action)OnAutoHide);
			}
			if (LoadOnEnable || HasDeferredLoad())
			{
				Load();
			}
		}
	}

	private void OnDisable()
	{
		Unload(deferRecycle: true);
	}

	private void OnDestroy()
	{
		DestroyOwnedInstance();
	}

	public virtual void SetResource(string newResourceName)
	{
		if (resourceName == newResourceName && ((Object)(object)instance != (Object)null || (Object)(object)deferredRecycleTarget != (Object)null))
		{
			return;
		}
		resourceName = newResourceName;
		Unload(deferRecycle: false);
		if (((Behaviour)this).isActiveAndEnabled)
		{
			if (playTiming == UIResourceLoadTiming.Play)
			{
				SetPendingPlayRequest(value: true);
			}
			Load();
		}
	}

	protected void Load()
	{
		if ((Object)(object)instance != (Object)null || isLoading)
		{
			return;
		}
		if (string.IsNullOrEmpty(resourceName))
		{
			NotifyLoadFailed();
			return;
		}
		string text = ResolvePath(resourceName);
		if (string.IsNullOrEmpty(text))
		{
			NotifyLoadFailed();
			return;
		}
		int generation = loadGeneration;
		isLoading = true;
		GameApp.Res.LoadAssetAsync<GameObject>(text, (Action<GameObject>)delegate(GameObject prefab)
		{
			isLoading = false;
			if (generation == loadGeneration && !((Object)(object)instance != (Object)null))
			{
				if ((Object)(object)prefab == (Object)null)
				{
					NotifyLoadFailed();
				}
				else if (((Behaviour)this).isActiveAndEnabled)
				{
					SpawnInstance(prefab);
				}
			}
		}, (string)null);
	}

	private void SpawnInstance(GameObject prefab)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		instance = (useObjectPool ? GameApp.Pool.Spawn(prefab, ((Component)this).transform) : Object.Instantiate<GameObject>(prefab, ((Component)this).transform));
		Transform transform = instance.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = prefab.transform.localScale;
		if ((Object)(object)cachedInstance != (Object)(object)instance)
		{
			cachedInstance = instance;
			OnInstanceChanged(instance);
		}
		lastSpawnedInstance = instance;
		OnInstanceReady(instance);
		pendingPlayRequest = false;
		if (autoHide)
		{
			GameApp.Timer.Delay((Object)(object)((Component)this).transform, Mathf.Max(0.01f, hideDelay), (Action)OnAutoHide);
		}
	}

	private void DestroyOwnedInstance()
	{
		loadGeneration++;
		isLoading = false;
		GameApp.Timer.KillByOwner((Object)(object)((Component)this).transform);
		OnLoadCancelled(isDeferredUnload: false);
		GameObject val = instance ?? deferredRecycleTarget;
		instance = null;
		deferredRecycleTarget = null;
		if ((Object)(object)val != (Object)null)
		{
			if ((Object)(object)val == (Object)(object)lastSpawnedInstance)
			{
				lastSpawnedInstance = null;
			}
			OnInstanceUnloading(val);
			Object.Destroy((Object)(object)val);
		}
		DestroyIdlePooledSpawnedInstance();
	}

	private void DestroyIdlePooledSpawnedInstance()
	{
		if (useObjectPool && !((Object)(object)lastSpawnedInstance == (Object)null))
		{
			GameObject val = lastSpawnedInstance;
			lastSpawnedInstance = null;
			if (IsIdleInPool(val))
			{
				OnInstanceUnloading(val);
				Object.Destroy((Object)(object)val);
			}
		}
	}

	private static bool IsIdleInPool(GameObject go)
	{
		if ((Object)(object)go == (Object)null || go.activeInHierarchy)
		{
			return false;
		}
		Transform parent = go.transform.parent;
		if ((Object)(object)parent != (Object)null)
		{
			return ((Object)parent).name == "[Pool]";
		}
		return false;
	}

	protected virtual string ResolvePath(string name)
	{
		return name;
	}

	protected virtual void OnInstanceChanged(GameObject newInstance)
	{
	}

	protected virtual void OnInstanceReady(GameObject readyInstance)
	{
	}

	protected virtual void OnLoadFailed()
	{
	}

	protected virtual void OnLoadCancelled(bool isDeferredUnload)
	{
		if (!isDeferredUnload)
		{
			pendingPlayRequest = false;
		}
	}

	private void OnAutoHide()
	{
		if (((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private bool TryReuseDeferredInstance()
	{
		if ((Object)(object)deferredRecycleTarget == (Object)null)
		{
			return false;
		}
		instance = deferredRecycleTarget;
		deferredRecycleTarget = null;
		if ((Object)(object)cachedInstance != (Object)(object)instance)
		{
			cachedInstance = instance;
			OnInstanceChanged(instance);
		}
		OnInstanceReady(instance);
		pendingPlayRequest = false;
		if (autoHide)
		{
			GameApp.Timer.Delay((Object)(object)((Component)this).transform, Mathf.Max(0.01f, hideDelay), (Action)OnAutoHide);
		}
		return true;
	}

	protected void Unload(bool deferRecycle)
	{
		loadGeneration++;
		isLoading = false;
		GameApp.Timer.KillByOwner((Object)(object)((Component)this).transform);
		OnLoadCancelled(deferRecycle);
		if ((Object)(object)instance != (Object)null)
		{
			GameObject val = instance;
			instance = null;
			OnInstanceUnloading(val);
			ReleaseInstance(val, deferRecycle);
		}
		else if ((Object)(object)deferredRecycleTarget != (Object)null && !deferRecycle)
		{
			GameObject val2 = deferredRecycleTarget;
			deferredRecycleTarget = null;
			OnInstanceUnloading(val2);
			ReleaseInstance(val2, deferRecycle: false);
		}
	}

	private void ReleaseInstance(GameObject target, bool deferRecycle)
	{
		if (!useObjectPool)
		{
			if ((Object)(object)target == (Object)(object)lastSpawnedInstance)
			{
				lastSpawnedInstance = null;
			}
			Object.Destroy((Object)(object)target);
		}
		else if (deferRecycle)
		{
			deferredRecycleTarget = target;
			GameApp.Timer.NextFrame((Object)(object)((Component)this).transform, (Action)delegate
			{
				if (!((Object)(object)deferredRecycleTarget != (Object)(object)target))
				{
					deferredRecycleTarget = null;
					GameApp.Pool.Recycle(target);
				}
			});
		}
		else
		{
			GameApp.Pool.Recycle(target);
		}
	}

	protected virtual void OnInstanceUnloading(GameObject unloadingInstance)
	{
	}

	private void NotifyLoadFailed()
	{
		OnLoadFailed();
	}
}
